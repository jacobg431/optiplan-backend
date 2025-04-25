using Optiplan.WebApi.Repositories;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.DataTransferObjects;
using System.ComponentModel.DataAnnotations;

namespace Optiplan.WebApi.Services;

public class OptimizationService : IOptimizationService
{    
    public async Task<WorkOrder[]> OptimizeByPartsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await Task.Run(() => OptimizeByParts(dtoList));
    }
    public async Task<WorkOrder[]> OptimizeByCostsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await Task.Run(() => DateTimeRandomizer(dtoList)); // Placeholder for now
    }
    public async Task<WorkOrder[]> OptimizeBySafetyAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await Task.Run(() => DateTimeRandomizer(dtoList)); // Placeholder for now
    }

    private WorkOrder[] OptimizeByParts(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        // Extract work orders that have available parts
        IEnumerable<WorkOrder> partsAvailableWorkOrders = GetWorkOrdersByDependency(
            dtoList, 
            "Materials and Parts", 
            dto => dto.BooleanAttributeValue != 0);

        // Sort work orders by criticality in descending order
        IEnumerable<WorkOrder> workOrdersToOptimize = SortWorkOrdersByDependency(
            dtoList,
            "Criticality",
            dto => dto.IntegerAttributeValue,
            partsAvailableWorkOrders
        );

        //IEnumerable<WorkOrder> unresolvableWorkOrders = new List<WorkOrder>();

        IEnumerable<WorkOrder> workOrders = OptimizeWorkOrders(dtoList, workOrdersToOptimize);

        // Extract work orders that don't have available parts
        IEnumerable<WorkOrder> partsNotAvailableWorkOrders = GetWorkOrdersByDependency(
            dtoList, 
            "Materials and Parts", 
            dto => dto.BooleanAttributeValue == 0);

        // Make sure work order without available parts are sorted by criticality
        workOrdersToOptimize = SortWorkOrdersByDependency(
            dtoList,
            "Criticality",
            dto => dto.IntegerAttributeValue,
            partsNotAvailableWorkOrders
        );

        // Concatinate work orders
        workOrders = workOrders.Union(OptimizeWorkOrders(dtoList, workOrdersToOptimize));

        //return workOrders.Union(unresolvableWorkOrders).ToArray();
        return workOrders.ToArray();
    }

    private static IEnumerable<WorkOrder> OptimizeWorkOrders(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        IEnumerable<WorkOrder> workOrdersToOptimize
    )
    {
        Dictionary<int, DateTime?> earliestStarts = ToDateTimeDictionary(
            dtoList, 
            workOrdersToOptimize, 
            "Work Order Start (Earliest)"
        );

        Dictionary<int, DateTime?> latestStarts = ToDateTimeDictionary(
            dtoList, 
            workOrdersToOptimize, 
            "Work Order Start (Latest)"
        );

        Dictionary<int, DateTime?> deadlines = ToDateTimeDictionary(
            dtoList, 
            workOrdersToOptimize, 
            "Work Order Deadline",
            false
        );

        IEnumerable<WorkOrder> workOrders = SetStartStopDateTimes(
            workOrdersToOptimize, 
            earliestStarts, 
            latestStarts, 
            deadlines,
            dtoList
        );

        return workOrders;
    }

    private static IEnumerable<WorkOrder> GetWorkOrdersByDependency(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        string dependencyName,
        Func<CustomWorkOrderDependencyDto, bool> filter
    )
    {
        return dtoList
            .Where(dto => dto.DependencyName == dependencyName)
            .Where(filter)
            .DistinctBy(dto => dto.WorkOrderId)
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder);
    }

    private static IEnumerable<WorkOrder> SortWorkOrdersByDependency(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        string dependencyName,
        Func<CustomWorkOrderDependencyDto, int?> sortSelector,
        IEnumerable<WorkOrder> workOrdersToSort
    )
    {
        return dtoList
            .Where(dto => dto.DependencyName == dependencyName)
            .DistinctBy(dto => dto.WorkOrderId)
            .OrderByDescending(sortSelector)
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder)
            .IntersectBy(workOrdersToSort.Select(p => p.Id), c => c.Id);
    }

    private static Dictionary<int, DateTime?> ToDateTimeDictionary(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        IEnumerable<WorkOrder> workOrders,
        string dependencyName,
        bool start = true
    )
    {
        IEnumerable<CustomWorkOrderDependencyDto> filteredDtoList = dtoList
            .Where(dto => dto.DependencyName == dependencyName)
            .IntersectBy(workOrders.Select(w => w.Id), e => e.WorkOrderId)
            .DistinctBy(dto => dto.WorkOrderId);

            return filteredDtoList
                .ToDictionary(dto => dto.WorkOrderId, dto => start? dto.DependencyStart : dto.DependencyStop);
    }

    private static IEnumerable<WorkOrder> SetStartStopDateTimes(
        IEnumerable<WorkOrder> workOrdersToOptimize,
        Dictionary<int, DateTime?> earliestStartDateTimes,
        Dictionary<int, DateTime?> latestStartDateTimes,
        Dictionary<int, DateTime?> deadlineDateTimes,
        IEnumerable<CustomWorkOrderDependencyDto> dtoList
    )
    {
        int expectedCount = workOrdersToOptimize.Count();
        if (
            earliestStartDateTimes.Count != expectedCount ||
            latestStartDateTimes.Count != expectedCount ||
            deadlineDateTimes.Count != expectedCount
        )
        {
            throw new ValidationException("DateTime count did not match expected");
        }

        List<WorkOrder> workOrders = new List<WorkOrder>();
        
        IEnumerable<CustomWorkOrderDependencyDto> dependentDtoList = dtoList
            .Where(dto => dto.DependencyName == "Other Work Orders" && dto.IntegerAttributeValue > 0)
            .IntersectBy(workOrdersToOptimize.Select(w => w.Id), dto => dto.WorkOrderId);
        IEnumerable<WorkOrder> dependentWorkOrders = dependentDtoList
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder);
        IEnumerable<int> dependentIds = dependentDtoList.Select(dto => dto.WorkOrderId).Distinct();

        // 1) Get the set of target work-order IDs that others depend on:
        IEnumerable<int> targetIds = dtoList
            .Where(dto => dto.DependencyName == "Other Work Orders"
                    && dto.IntegerAttributeValue > 0
                    // optional: only consider dependencies coming from workOrdersToOptimize
                    && workOrdersToOptimize.Select(w => w.Id).Contains(dto.WorkOrderId))
            .Select(dto => dto.IntegerAttributeValue!.Value)
            .Distinct();

        // 2) Filter your scheduled set to only those targets:
        IEnumerable<WorkOrder> workOrdersOtherAreDependentOn = workOrdersToOptimize
            .Where(w => targetIds.Contains(w.Id));

        foreach(WorkOrder workOrder in workOrdersToOptimize)
        {
            string workOrderName = workOrder.Name ?? "";

            if (workOrder.Name is null || workOrder.StartDateTime is null || workOrder.StopDateTime is null)
            {
                workOrders.Add(new WorkOrder{
                    Id = workOrder.Id,
                    Name = workOrderName,
                    StartDateTime = null,
                    StopDateTime = null
                });
                continue;
            }

            TimeSpan diffDateTime = (TimeSpan)(workOrder.StopDateTime - workOrder.StartDateTime);
            DateTime? earliestStart = earliestStartDateTimes[workOrder.Id];
            DateTime? latestStart = latestStartDateTimes[workOrder.Id];
            DateTime? deadline = deadlineDateTimes[workOrder.Id];

            // If work order dependent on work order not yet optimized, place in list
            //if (targetIds.Contains())

            // If this work order has any other work order dependencies, get the start and stop times of these
            if (dependentIds.Contains(workOrder.Id))
            {
                DateTime? earliestPossibleStart = GetEarliestPossibleStartDate(workOrder.Id, dtoList);
                earliestStart = earliestPossibleStart > earliestStart ? earliestPossibleStart : earliestStart;
            }

            if (!IsScheduleFeasible(earliestStart, latestStart, deadline))
            {
                workOrders.Add(new WorkOrder{
                    Id = workOrder.Id,
                    Name = workOrderName,
                    StartDateTime = null,
                    StopDateTime = null
                });

                continue;
            }          

            workOrders.Add(new WorkOrder{
                Id = workOrder.Id,
                Name = workOrderName,
                StartDateTime = earliestStart,
                StopDateTime = earliestStart?.Add(diffDateTime)
            });
        }

        return workOrders.ToArray();
    }

    private static DateTime? GetEarliestPossibleStartDate(int workOrderId, IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        IEnumerable<CustomWorkOrderDependencyDto> targetDtoList = dtoList
            .Where(dto => dto.WorkOrderId == workOrderId);
        
        DateTime? earliest = null;

        foreach(CustomWorkOrderDependencyDto targetDto in targetDtoList)
        {
            DateTime? time = targetDto.BooleanAttributeValue == 0 ? targetDto.WorkOrderStart : targetDto.WorkOrderStop;
            earliest = time > earliest ? time : earliest;
        }

        return earliest;
    }

    private static bool IsScheduleFeasible(DateTime? start, DateTime? latest, DateTime? deadline)
    {
        return !(start > latest || start > deadline);
    }

    private WorkOrder[] DateTimeRandomizer(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {

        IEnumerable<CustomWorkOrderDependencyDto> distinctByWorkOrderList = dtoList.DistinctBy(dto => dto.WorkOrderId);
        List<WorkOrder> workOrders = new List<WorkOrder>();

        foreach(CustomWorkOrderDependencyDto dto in distinctByWorkOrderList)
        {
            DateTime? dateTimeStart = dto.WorkOrderStart;
            DateTime? dateTimeStop = dto.WorkOrderStop;

            Random rnd = new Random();
            int rndNum = rnd.Next(-48, 48);

            dateTimeStart = dateTimeStart?.AddHours(rndNum);
            dateTimeStop = dateTimeStop?.AddHours(rndNum);

            string WorkOrderName = dto.WorkOrderName ?? "" ;

            workOrders.Add(new WorkOrder{
                Id = dto.WorkOrderId,
                Name = WorkOrderName,
                StartDateTime = dateTimeStart,
                StopDateTime = dateTimeStop
            });

        }

        return workOrders.ToArray();
    }

}
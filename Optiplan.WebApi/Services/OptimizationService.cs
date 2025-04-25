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
        // Sort all work orders by criticality in descending order
        IEnumerable<CustomWorkOrderDependencyDto> criticalitySortedSubset = dtoList
            .Where(dto => dto.DependencyName == "Criticality")
            .OrderByDescending(dto => dto.IntegerAttributeValue);
        IEnumerable<WorkOrder> criticalitySortedWorkOrders = criticalitySortedSubset
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder);

        // Extract work orders that have available parts
        IEnumerable<CustomWorkOrderDependencyDto> partsAvailableSubset = dtoList
            .Where(dto => dto.DependencyName == "Materials and parts" && dto.BooleanAttributeValue != 0)
            .DistinctBy(dto => dto.WorkOrderId);
        IEnumerable<WorkOrder> partsAvailableWorkOrders = partsAvailableSubset
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder);

        // Make sure work order with available parts are sorted by criticality
        IEnumerable<WorkOrder> workOrdersToOptimize = criticalitySortedWorkOrders
            .IntersectBy(partsAvailableWorkOrders
            .Select(p => p.Id), c => c.Id);

        //IEnumerable<WorkOrder> unresolvableWorkOrders = new List<WorkOrder>();

        IEnumerable<WorkOrder> workOrders = OptimizeWorkOrders(dtoList, workOrdersToOptimize);

        // Extract work orders that don't have available parts
        IEnumerable<CustomWorkOrderDependencyDto> partsNotAvailableSubset = dtoList
            .Where(dto => dto.DependencyName == "Materials and parts" && dto.BooleanAttributeValue == 0)
            .DistinctBy(dto => dto.WorkOrderId);
        IEnumerable<WorkOrder> partsNotAvailableWorkOrders = partsAvailableSubset
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder);

        // Make sure work order without available parts are sorted by criticality
        workOrdersToOptimize = criticalitySortedWorkOrders
            .IntersectBy(partsNotAvailableWorkOrders
            .Select(p => p.Id), c => c.Id);

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

    private static Dictionary<int, DateTime?> ToDateTimeDictionary(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        IEnumerable<WorkOrder> workOrders,
        string dependencyName,
        bool start = true
    )
    {
        IEnumerable<CustomWorkOrderDependencyDto> tempDtoList = dtoList
            .Where(dto => dto.DependencyName == dependencyName)
            .IntersectBy(workOrders.Select(w => w.Id), e => e.WorkOrderId)
            .DistinctBy(dto => dto.WorkOrderId);

            return tempDtoList
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

        foreach(WorkOrder w in workOrdersToOptimize)
        {
            string dtoName = w.Name is null ? "" : w.Name;

            if (w.Name is null || w.StartDateTime is null || w.StopDateTime is null)
            {
                workOrders.Add(new WorkOrder{
                    Id = w.Id,
                    Name = dtoName,
                    StartDateTime = null,
                    StopDateTime = null
                });
                continue;
            }

            TimeSpan diffDateTime = (TimeSpan)(w.StopDateTime - w.StartDateTime);
            DateTime? earliestStart = earliestStartDateTimes[w.Id];
            DateTime? latestStart = latestStartDateTimes[w.Id];
            DateTime? deadline = deadlineDateTimes[w.Id];

            // If work order dependent on work order not yet optimized, place in list
            //if (targetIds.Contains())

            // If this work order has any other work order dependencies, get the start and stop times of these
            if (dependentIds.Contains(w.Id))
            {

                IEnumerable<CustomWorkOrderDependencyDto> targets = dependentDtoList
                    .Where(dto => dto.WorkOrderId == w.Id);

                DateTime? earliestPossibleStart = earliestStart;

                foreach(CustomWorkOrderDependencyDto target in targets)
                {
                    byte hasFinished = dependentDtoList
                        .Where(dto => dto.IntegerAttributeValue == target.WorkOrderId)
                        .Select(dto => dto.BooleanAttributeValue!.Value)
                        .FirstOrDefault();

                    // Work order is dependent on other work to have started
                    if (hasFinished == 0)
                    {
                        DateTime? targetStart = target.WorkOrderStart;
                        earliestPossibleStart = targetStart > earliestPossibleStart ? targetStart : earliestPossibleStart;
                        continue;
                    }

                    // Work order is dependent on other work order to have stopped
                    DateTime? targetStop = target.WorkOrderStop;
                    earliestPossibleStart = targetStop > earliestPossibleStart ? targetStop : earliestPossibleStart;
                }
                
                earliestStart = earliestPossibleStart > earliestStart ? earliestPossibleStart : earliestStart;
            }

            if (earliestStart > latestStart || earliestStart > deadline)
            {
                workOrders.Add(new WorkOrder{
                    Id = w.Id,
                    Name = dtoName,
                    StartDateTime = null,
                    StopDateTime = null
                });

                continue;
            }          

            workOrders.Add(new WorkOrder{
                Id = w.Id,
                Name = dtoName,
                StartDateTime = earliestStart,
                StopDateTime = earliestStart?.Add(diffDateTime)
            });
        }

        return workOrders.ToArray();
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

            string dtoName = dto.WorkOrderName is null ? "" : dto.WorkOrderName;

            workOrders.Add(new WorkOrder{
                Id = dto.WorkOrderId,
                Name = dtoName,
                StartDateTime = dateTimeStart,
                StopDateTime = dateTimeStop
            });

        }

        return workOrders.ToArray();

    }

}
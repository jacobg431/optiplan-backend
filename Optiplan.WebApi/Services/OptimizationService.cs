using Optiplan.WebApi.Utilities;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.DataTransferObjects;
using System.ComponentModel.DataAnnotations;

namespace Optiplan.WebApi.Services;

public class OptimizationService : IOptimizationService
{    
    public WorkOrder[] OptimizeByParts(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return OptimizeByPartsInner(dtoList);
    }
    public WorkOrder[] OptimizeByCosts(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return DateTimeRandomizer(dtoList); // Placeholder for now
    }
    public WorkOrder[] OptimizeBySafety(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return DateTimeRandomizer(dtoList); // Placeholder for now
    }

    private WorkOrder[] OptimizeByPartsInner(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        // Extract work orders that have available parts
        IEnumerable<WorkOrder> partsAvailableWorkOrders = GetWorkOrdersByDependency(
            dtoList, 
            6, //"Materials and Parts" 
            dto => dto.BooleanAttributeValue != 0
        );

        // Extract work orders that have no parts that need to be available)
        IEnumerable<WorkOrder> noPartsAvailableInfoWorkOrders = GetWorkOrdersWithoutDependency(
            dtoList,
            6 //"Materials and Parts"
        );

        // Merge work orders and remove any duplicates
        partsAvailableWorkOrders = RemoveDuplicateWorkOrdersAndMerge(partsAvailableWorkOrders, noPartsAvailableInfoWorkOrders);

        // Sort work orders by criticality in descending order
        partsAvailableWorkOrders = SortWorkOrdersByDependency(
            dtoList,
            9, //"Criticality"
            dto => dto.IntegerAttributeValue,
            partsAvailableWorkOrders
        );

        // Extract work orders that don't have available parts
        IEnumerable<WorkOrder> partsNotAvailableWorkOrders = GetWorkOrdersByDependency(
            dtoList, 
            6, //"Materials and Parts"
            dto => dto.BooleanAttributeValue == 0
        );

        // Make sure work order without available parts are sorted by criticality
        partsNotAvailableWorkOrders = SortWorkOrdersByDependency(
            dtoList,
            9, //"Criticality"
            dto => dto.IntegerAttributeValue,
            partsNotAvailableWorkOrders
        );

        // Merge work orders and remove any duplicates
        IEnumerable<WorkOrder> workOrdersToOptimize = RemoveDuplicateWorkOrdersAndMerge(partsAvailableWorkOrders, partsNotAvailableWorkOrders);
        IEnumerable<WorkOrder> workOrders = OptimizeWorkOrders(dtoList, workOrdersToOptimize);

        // Debug info
        IEnumerable<WorkOrder> allWorkOrders = dtoList
            .DistinctBy(dto => dto.WorkOrderId)
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder);
        
        var workOrderIdSet = allWorkOrders.Select(w => w.Id).ToHashSet();
        var missing = workOrderIdSet.Except(workOrders.Select(w => w.Id));
        Console.WriteLine($"Expected: {workOrderIdSet.Count}, Found: {workOrders.Count()}, Missing: {string.Join(", ", missing)}");

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
            2 //"Work Order Start (Earliest)"
        );

        Dictionary<int, DateTime?> latestStarts = ToDateTimeDictionary(
            dtoList, 
            workOrdersToOptimize, 
            3 //"Work Order Start (Latest)"
        );

        Dictionary<int, DateTime?> deadlines = ToDateTimeDictionary(
            dtoList, 
            workOrdersToOptimize, 
            4, //"Work Order Deadline",
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
        int dependencyId,
        Func<CustomWorkOrderDependencyDto, bool> filter
    )
    {
        return dtoList
            .Where(dto => dto.DependencyId == dependencyId)
            .Where(filter)
            .DistinctBy(dto => dto.WorkOrderId)
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder);
    }

    private static IEnumerable<WorkOrder> GetWorkOrdersWithoutDependency(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        int dependencyId
    )
    {
        // 1) Find all work-orders that have this dependency
        var referencedWorkOrderIds = dtoList
            .Where(dto => dto.DependencyId == dependencyId)
            .Select(dto => dto.WorkOrderId)
            .ToHashSet();

        // 2) From the full DTO list pick those whose WorkOrderId is NOT in referencedWorkOrderIds
        return dtoList
            .Where(dto => !referencedWorkOrderIds.Contains(dto.WorkOrderId))
            .DistinctBy(dto => dto.WorkOrderId)
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder);
    }

    private static IEnumerable<WorkOrder> SortWorkOrdersByDependency(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        int dependencyId,
        Func<CustomWorkOrderDependencyDto, int?> sortSelector,
        IEnumerable<WorkOrder> workOrdersToSort
    )
    {
        return dtoList
            .Where(dto => dto.DependencyId == dependencyId)
            .DistinctBy(dto => dto.WorkOrderId)
            .OrderByDescending(sortSelector)
            .Select(CustomWorkOrderDependencyMapper.ToWorkOrder)
            .IntersectBy(workOrdersToSort.Select(p => p.Id), c => c.Id);
    }

    private static IEnumerable<WorkOrder> RemoveDuplicateWorkOrdersAndMerge(
        IEnumerable<WorkOrder> workOrdersOld,
        IEnumerable<WorkOrder> workOrdersNew
    )
    {
        // Check for duplicates
        HashSet<int> duplicates = workOrdersOld
            .IntersectBy(workOrdersNew.Select(n => n.Id), p => p.Id)
            .Select(d => d.Id)
            .ToHashSet();
        
        if (!duplicates.Any()) {
            return workOrdersOld.Union(workOrdersNew);
        }

        // Remove duplicates from new list
        workOrdersNew = workOrdersNew
            .ExceptBy(workOrdersOld.Select(n => n.Id), p => p.Id);

        // Check for duplicates
        duplicates = workOrdersOld
            .IntersectBy(workOrdersNew.Select(n => n.Id), p => p.Id)
            .Select(d => d.Id)
            .ToHashSet();
        
        if (duplicates.Count > 0) {
            throw new ValidationException($"Could not remove duplicates");
        }

        return workOrdersOld.Union(workOrdersNew);
    }

    private static Dictionary<int, DateTime?> ToDateTimeDictionary(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        IEnumerable<WorkOrder> workOrders,
        int dependencyId,
        bool start = true
    )
    {
        IEnumerable<CustomWorkOrderDependencyDto> filteredDtoList = dtoList
            .Where(dto => dto.DependencyId == dependencyId)
            .Where(dto => workOrders.Any(w => w.Id == dto.WorkOrderId))
            .Where(dto => start ? dto.DependencyStart.HasValue : dto.DependencyStop.HasValue);
        
        Dictionary<int, DateTime?> filteredDictionary = filteredDtoList
            .GroupBy(dto => dto.WorkOrderId)
            .ToDictionary(
                g => g.Key,
                g => start 
                    ? g.First(dto => dto.DependencyStart.HasValue).DependencyStart
                    : g.First(dto => dto.DependencyStop.HasValue).DependencyStop
            );

        return filteredDictionary;
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
            throw new ValidationException($"DateTime count did not match expected. Expected {expectedCount}. Got {earliestStartDateTimes.Count}, {latestStartDateTimes.Count}, {deadlineDateTimes.Count}");
            //throw new ValidationException($"start {earliestStartDateTimes.ElementAt(13)}, start latest {latestStartDateTimes.ElementAt(12)}, deadline {deadlineDateTimes.ElementAt(12)}");
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
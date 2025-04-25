using Optiplan.WebApi.Repositories;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.DataTransferObjects;
using System.ComponentModel.DataAnnotations;

namespace Optiplan.WebApi.Services;

public class OptimizationService : IOptimizationService
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private static Queue<CustomWorkOrderDependencyDto> _dtoFifoQueue = new Queue<CustomWorkOrderDependencyDto>();

    public OptimizationService(IWorkOrderRepository workOrderRepository)
    {
        _workOrderRepository = workOrderRepository;
    }

    
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
        IEnumerable<CustomWorkOrderDependencyDto> criticalitySortedSubset = dtoList.Where(
            dto => dto.DependencyName == "Criticality"
        ).OrderByDescending(dto => dto.IntegerAttributeValue);
        IEnumerable<WorkOrder> criticalitySortedWorkOrders = criticalitySortedSubset.Select(CustomWorkOrderDependencyMapper.ToWorkOrder);

        IEnumerable<CustomWorkOrderDependencyDto> partsAvailableSubset = dtoList.Where(
            dto => dto.DependencyName == "Materials and parts" && 
            dto.BooleanAttributeValue != 0
        ).DistinctBy(dto => dto.WorkOrderId);
        IEnumerable<WorkOrder> partsAvailableWorkOrders = partsAvailableSubset.Select(CustomWorkOrderDependencyMapper.ToWorkOrder);

        // The following list should be sorted by criticality
        IEnumerable<WorkOrder> workOrdersToOptimize = criticalitySortedWorkOrders.IntersectBy(
            partsAvailableWorkOrders.Select(p => p.Id), 
            c => c.Id
        );

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

        IEnumerable<WorkOrder> workOrders = new List<WorkOrder>();
        workOrders = SetStartStopDateTimes(workOrdersToOptimize, earliestStarts, latestStarts, deadlines);

        return workOrders.ToArray();
    }

    private static Dictionary<int, DateTime?> ToDateTimeDictionary(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        IEnumerable<WorkOrder> workOrders,
        string dependencyName,
        bool start = true
    )
    {
        IEnumerable<CustomWorkOrderDependencyDto> tempDtoList = dtoList.Where(
            dto => dto.DependencyName == dependencyName
        ).IntersectBy(
            workOrders.Select(w => w.Id),
            e => e.WorkOrderId
        );
        
        if (start)
        {
            return tempDtoList.ToDictionary(dto => dto.WorkOrderId, dto => dto.DependencyStart);
        }
        return tempDtoList.ToDictionary(dto => dto.WorkOrderId, dto => dto.DependencyStop);
    }

    private static IEnumerable<WorkOrder> SetStartStopDateTimes(
        IEnumerable<WorkOrder> workOrdersToOptimize,
        Dictionary<int, DateTime?> earliestStartDateTimes,
        Dictionary<int, DateTime?> latestStartDateTimes,
        Dictionary<int, DateTime?> deadlineDateTimes,
        bool checkOtherWorkOrders = false
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
            
            if (checkOtherWorkOrders)
            {
                throw new NotImplementedException();
            }

            DateTime? latestStart = latestStartDateTimes[w.Id];
            DateTime? deadline = deadlineDateTimes[w.Id];
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
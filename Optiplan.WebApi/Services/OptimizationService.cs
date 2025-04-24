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
        return await Task.Run(() => OptimizeByParts(dtoList)); // Placeholder for now ...
    }
    public async Task<WorkOrder[]> OptimizeByCostsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await _workOrderRepository.RetrieveAllAsync();
    }
    public async Task<WorkOrder[]> OptimizeBySafetyAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await _workOrderRepository.RetrieveAllAsync();
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
        ).DistinctBy(dto => dto.DependencyId);
        IEnumerable<WorkOrder> partsAvailableWorkOrders = partsAvailableSubset.Select(CustomWorkOrderDependencyMapper.ToWorkOrder);

        // The following list should be sorted by criticality
        IEnumerable<WorkOrder> workOrdersToOptimize = criticalitySortedWorkOrders.IntersectBy(
            partsAvailableWorkOrders.Select(p => p.Id), 
            c => c.Id
        );

        IEnumerable<CustomWorkOrderDependencyDto> earliestStarts = IntersectDependencyByWorkOrder(
            dtoList, 
            workOrdersToOptimize, 
            "Work Order Start (Earliest)"
        );
        Queue<DateTime?> earliestStartDateTimes = GetDateTimes(earliestStarts);

        IEnumerable<CustomWorkOrderDependencyDto> latestStarts = IntersectDependencyByWorkOrder(
            dtoList, 
            workOrdersToOptimize, 
            "Work Order Start (Earliest)"
        );
        Queue<DateTime?> latestStartDateTimes = GetDateTimes(latestStarts);

        IEnumerable<CustomWorkOrderDependencyDto> deadlines = IntersectDependencyByWorkOrder(
            dtoList, 
            workOrdersToOptimize, 
            "Work Order Deadline"
        );
        Queue<DateTime?> deadlineDateTimes = GetDateTimes(deadlines, false);

        IEnumerable<WorkOrder> workOrders = new List<WorkOrder>();
        workOrders = SetStartStopDateTimes(workOrdersToOptimize, earliestStartDateTimes, latestStartDateTimes, deadlineDateTimes);

        return workOrders.ToArray();
    }

    private static IEnumerable<CustomWorkOrderDependencyDto> IntersectDependencyByWorkOrder(
        IEnumerable<CustomWorkOrderDependencyDto> dtoList,
        IEnumerable<WorkOrder> workOrders,
        string dependencyName
    )
    {
        return dtoList.Where(
            dto => dto.DependencyName == dependencyName
        ).IntersectBy(
            workOrders.Select(w => w.Id),
            e => e.WorkOrderId
        );
    }

    private static Queue<DateTime?> GetDateTimes(IEnumerable<CustomWorkOrderDependencyDto> dtoList, bool start = true)
    {
        Queue<DateTime?> dateTimes = new Queue<DateTime?>();
        foreach(CustomWorkOrderDependencyDto dto in dtoList)
        {
            if (start)
            {
                dateTimes.Append(dto.DependencyStart);
                continue;
            }
            dateTimes.Append(dto.DependencyStop);
        }

        return dateTimes;
    }

    private static IEnumerable<WorkOrder> SetStartStopDateTimes(
        IEnumerable<WorkOrder> workOrdersToOptimize,
        Queue<DateTime?> earliestStartDateTimes,
        Queue<DateTime?> latestStartDateTimes,
        Queue<DateTime?> deadlineDateTimes,
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
                workOrders.Append(new WorkOrder{
                    Id = w.Id,
                    Name = dtoName,
                    StartDateTime = null,
                    StopDateTime = null
                });

                earliestStartDateTimes.Dequeue();
                latestStartDateTimes.Dequeue();
                deadlineDateTimes.Dequeue();
                continue;
            }

            TimeSpan diffDateTime = (TimeSpan)(w.StopDateTime - w.StartDateTime);
            DateTime? earliestStart = earliestStartDateTimes.Dequeue();
            
            if (checkOtherWorkOrders)
            {
                throw new NotImplementedException();
            }

            DateTime? latestStart = latestStartDateTimes.Dequeue();
            DateTime? deadline = deadlineDateTimes.Dequeue();
            if (earliestStart > latestStart || earliestStart > deadline)
            {
                workOrders.Append(new WorkOrder{
                    Id = w.Id,
                    Name = dtoName,
                    StartDateTime = null,
                    StopDateTime = null
                });

                continue;
            }          

            workOrders.Append(new WorkOrder{
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
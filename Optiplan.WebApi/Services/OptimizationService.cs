using Optiplan.WebApi.Repositories;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.DataTransferObjects;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Optiplan.WebApi.Services;

public class OptimizationService : IOptimizationService
{
    private readonly IWorkOrderRepository _workOrderRepository;

    public OptimizationService(IWorkOrderRepository workOrderRepository)
    {
        _workOrderRepository = workOrderRepository;
    }

    
    public async Task<WorkOrder[]> OptimizeByPartsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await Task.Run(() => DateTimeRandomizer(dtoList)); // Placeholder for now ...
    }
    public async Task<WorkOrder[]> OptimizeByCostsAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await _workOrderRepository.RetrieveAllAsync();
    }
    public async Task<WorkOrder[]> OptimizeBySafetyAsync(IEnumerable<CustomWorkOrderDependencyDto> dtoList)
    {
        return await _workOrderRepository.RetrieveAllAsync();
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

            dateTimeStart = dateTimeStop?.AddHours(rndNum);
            dateTimeStop = dateTimeStop?.AddHours(rndNum);

            workOrders.Add(new WorkOrder{
                Id = dto.WorkOrderId,
                Name = dto.Name,
                StartDateTime = dateTimeStart,
                StopDateTime = dateTimeStop
            });

        }

        return workOrders.ToArray();

    }

}
using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.DataTransferObjects;

public class CustomWorkOrderDependencyMapper
{
    public static WorkOrder ToWorkOrder(CustomWorkOrderDependencyDto dtoList)
    {
        string? dtoName = dtoList.WorkOrderName is null ? "" : dtoList.WorkOrderName;
        return new WorkOrder
        {
            Id = dtoList.WorkOrderId,
            Name = dtoName,
            StartDateTime = dtoList.WorkOrderStart,
            StopDateTime = dtoList.WorkOrderStop
        };
    }
}
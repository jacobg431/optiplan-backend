using System.ComponentModel.DataAnnotations;
using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.DataTransferObjects;

public class CustomWorkOrderDependencyMapper
{
    public static IEnumerable<CustomWorkOrderDependencyDto> ToDtoList(
        IEnumerable<Dependency> dependencies,
        IEnumerable<WorkOrder> workOrders,
        IEnumerable<WorkOrderToDependency> workOrderToDependencies
    )
    {
        if (!dependencies.Any())
        {
            throw new ArgumentException("No dependencies defined");
        }
        if (!workOrders.Any())
        {
            throw new ArgumentException("No work orders defined");
        }
        if (!workOrderToDependencies.Any())
        {
            throw new ArgumentException("No work order to dependency information defined");
        }

        var resultFirstJoin = workOrderToDependencies.Join(workOrders, wotd => wotd.WorkOrderId, wo => wo.Id, (wotd, wo) => new {
            wotd.DependencyInstanceId,
            wotd.WorkOrderId,
            wotd.DependencyId,
            WorkOrderName = wo.Name,
            WorkOrderStart = wo.StartDateTime,
            WorkOrderStop = wo.StopDateTime,
            wotd.TextAttributeValue,
            wotd.IntegerAttributeValue,
            wotd.NumberAttributeValue,
            wotd.BooleanAttributeValue,
            DependencyStart = wotd.StartDateTime,
            DependencyStop = wotd.StopDateTime
        });

        int expectedNumberOfWorkOrders = resultFirstJoin.Select(r => r.WorkOrderId).Distinct().Count();
        if (expectedNumberOfWorkOrders == 0)
        {
            throw new ValidationException("No references to valid work orders");
        }

        var resultSecondJoin = resultFirstJoin.Join(dependencies, r => r.DependencyId, d => d.Id, (r, d) => new {
            r.DependencyInstanceId,
            r.WorkOrderId,
            r.DependencyId,
            r.WorkOrderName,
            r.WorkOrderStart,
            r.WorkOrderStop,
            r.TextAttributeValue,
            r.IntegerAttributeValue,
            r.NumberAttributeValue,
            r.BooleanAttributeValue,
            r.DependencyStart,
            r.DependencyStop,
            DependencyName = d.Name
        });

        return resultSecondJoin.Select(r => new CustomWorkOrderDependencyDto {
            DependencyInstanceId = r.DependencyInstanceId,
            WorkOrderId = r.WorkOrderId,
            DependencyId = r.DependencyId,
            WorkOrderName = r.WorkOrderName,
            WorkOrderStart = r.WorkOrderStart,
            WorkOrderStop = r.WorkOrderStop,
            TextAttributeValue = r.TextAttributeValue,
            IntegerAttributeValue = r.IntegerAttributeValue,
            NumberAttributeValue = r.NumberAttributeValue,
            BooleanAttributeValue = r.BooleanAttributeValue,
            DependencyStart = r.DependencyStart,
            DependencyStop = r.DependencyStop,
            DependencyName = r.DependencyName
        });
    }
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
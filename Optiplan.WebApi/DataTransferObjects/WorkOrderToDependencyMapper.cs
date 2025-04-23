using Optiplan.DatabaseResources;

namespace Optiplan.WebApi.DataTransferObjects;

public static class WorkOrderToDependencyMapper
{
    public static WorkOrderToDependency ToEntity(WorkOrderToDependencyDto dto)
    {
        return new WorkOrderToDependency
        {
            DependencyInstanceId = dto.DependencyInstanceId,
            DependencyId = dto.DependencyId,
            WorkOrderId = dto.WorkOrderId,
            TextAttributeValue = dto.TextAttributeValue,
            IntegerAttributeValue = dto.IntegerAttributeValue,
            NumberAttributeValue = dto.NumberAttributeValue,
            BooleanAttributeValue = dto.BooleanAttributeValue,
            StartDateTime = dto.StartDateTime,
            StopDateTime = dto.StopDateTime
        };
    }

    public static WorkOrderToDependencyDto ToDto(WorkOrderToDependency entity)
    {
        return new WorkOrderToDependencyDto
        {
            DependencyInstanceId = entity.DependencyInstanceId,
            DependencyId = entity.DependencyId,
            WorkOrderId = entity.WorkOrderId,
            TextAttributeValue = entity.TextAttributeValue,
            IntegerAttributeValue = entity.IntegerAttributeValue,
            NumberAttributeValue = entity.NumberAttributeValue,
            BooleanAttributeValue = entity.BooleanAttributeValue,
            StartDateTime = entity.StartDateTime,
            StopDateTime = entity.StopDateTime
        };
    }
}

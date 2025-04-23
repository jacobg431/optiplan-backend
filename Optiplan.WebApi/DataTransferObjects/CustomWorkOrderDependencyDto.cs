namespace Optiplan.WebApi.DataTransferObjects;

public class CustomWorkOrderDependencyDto
{
    public int DependencyInstanceId { get; init; }
    public int WorkOrderId { get; init; }
    public int DependencyId { get; init; }

    public DateTime? WorkOrderStart { get; init; }
    public DateTime? WorkOrderStop { get; init; }
    public string? TextAttributeValue { get; init; }
    public int? IntegerAttributeValue { get; init; }
    public double? NumberAttributeValue { get; init; }
    public byte? BooleanAttributeValue { get; init; }
    public DateTime? DependencyStart { get; init; }
    public DateTime? DependencyStop { get; init; }
    public string? Name { get; init; }

}
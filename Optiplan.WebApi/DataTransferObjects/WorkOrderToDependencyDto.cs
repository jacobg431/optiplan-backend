namespace Optiplan.WebApi.DataTransferObjects;

public class WorkOrderToDependencyDto
{
    public int DependencyInstanceId { get; init; }
    public int DependencyId { get; init; }
    public int WorkOrderId { get; init; }

    public string? TextAttributeValue { get; init; }
    public int? IntegerAttributeValue { get; init; }
    public double? NumberAttributeValue { get; init; }
    public byte? BooleanAttributeValue { get; init; }
    public DateTime? StartDateTime { get; init; }
    public DateTime? StopDateTime { get; init; }
}
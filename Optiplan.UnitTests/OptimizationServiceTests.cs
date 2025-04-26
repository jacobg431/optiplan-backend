using Moq;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;
using Optiplan.WebApi.Utilities;
using Optiplan.WebApi.DataTransferObjects;
using Xunit.Abstractions;

namespace Optiplan.UnitTests;

public class OptimizationServiceTests
{
    private readonly IOptimizationService _optimizationService;
    private readonly ITestOutputHelper _output;
    private static readonly string _generalSamplesDirectory = AppContext.BaseDirectory + "../../../Resources/GeneralSamples";
    private static readonly string _optimizationServiceSamplesDirectory = AppContext.BaseDirectory 
        + "../../../Resources/OptimizationServiceSamples";

    public OptimizationServiceTests(ITestOutputHelper output)
    {
        _optimizationService = new OptimizationService();
        _output = output;
    }

    [Fact]
    public async Task ReturnsEmptyArrayWhenNoWotd()
    {
        string workOrderSamplesPath = _generalSamplesDirectory + "/WorkOrderSamples";
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples";

        WorkOrder[]? workOrders = await FileUtilities.JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);
        
        Dependency[]? dependencies = await FileUtilities.JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        Assert.IsType<Dependency[]>(dependencies);

        WorkOrderToDependency[] workOrderToDependencies = new List<WorkOrderToDependency>().ToArray();
    }

    
    [Fact]
    public void ThrowsArgumentNullExceptionWhenNullArgument(){}

    [Fact]
    public void OptimizedWhenAllPartsAvailable(){}
    
    [Fact]
    public void OptimizedWhenAllNoPartsAvailable(){}
    
    [Fact]
    public void OptimizedWhenMixedPartsAvailable(){}
    
    [Fact]
    public void ThrowsValidationExceptionWhenMissingDateTimeAttribute(){}
    
    [Fact]
    public void UnScheduledWhenInfeasibleTimeWindow(){}
    
    [Fact]
    public void StartEqualsStopWhenZeroDurationWorkOrder(){}

    
}
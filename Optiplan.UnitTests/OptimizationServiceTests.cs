using Moq;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;
using Optiplan.WebApi.Utilities;
using Optiplan.WebApi.DataTransferObjects;
using Xunit.Abstractions;
using System.ComponentModel.DataAnnotations;

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
    public void ThrowsArgumentExceptionWhenEmptyDto()
    {
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples.json";
        string workOrderSamplesPath = _generalSamplesDirectory + "/WorkOrderSamples.json";

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = new List<CustomWorkOrderDependencyDto>();
        Assert.Throws<ArgumentException>(() => _optimizationService.OptimizeByParts(dtoList));
    }

    [Fact]
    public void ThrowsArgumentNullExceptionWhenNullArgument()
    {
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples.json";
        string workOrderSamplesPath = _generalSamplesDirectory + "/WorkOrderSamples.json";

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = new List<CustomWorkOrderDependencyDto>();
        Assert.Throws<ArgumentNullException>(() => _optimizationService.OptimizeByParts(null));
    }

    [Fact]
    public async Task OptimizedWhenAllPartsAvailable()
    {
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples.json";
        string workOrderSamplesPath = _generalSamplesDirectory + "/WorkOrderSamples.json";
        string workOrderToDependencySamplesPath = _optimizationServiceSamplesDirectory + "/WotdAllPartsAvailableSamples.json";
        
        Dependency[]? dependencies = await FileUtilities
            .JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        Assert.IsType<Dependency[]>(dependencies);

        WorkOrder[]? workOrders = await FileUtilities
            .JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities
            .JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        WorkOrder[] workOrdersReturned = _optimizationService.OptimizeByParts(dtoList);
        int[] expectedWorkOrderIds = [1, 5, 3, 2, 4];

        Assert.True(IsSortedByCriticality(dtoList, workOrdersReturned, expectedWorkOrderIds));
    }
    
    [Fact]
    public async Task OptimizedWhenAllNoPartsAvailable()
    {
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples.json";
        string workOrderSamplesPath = _generalSamplesDirectory + "/WorkOrderSamples.json";
        string workOrderToDependencySamplesPath = _optimizationServiceSamplesDirectory + "/WotdAllNoPartsAvailableSamples.json";
        
        Dependency[]? dependencies = await FileUtilities
            .JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        Assert.IsType<Dependency[]>(dependencies);

        WorkOrder[]? workOrders = await FileUtilities
            .JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities
            .JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        WorkOrder[] workOrdersReturned = _optimizationService.OptimizeByParts(dtoList);
        int[] expectedWorkOrderIds = [1, 5, 3, 2, 4];

        Assert.True(IsSortedByCriticality(dtoList, workOrdersReturned, expectedWorkOrderIds));
    }
    
    [Fact]
    public async Task OptimizedWhenMixedPartsAvailable()
    {
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples.json";
        string workOrderSamplesPath = _generalSamplesDirectory + "/WorkOrderSamples.json";
        string workOrderToDependencySamplesPath = _optimizationServiceSamplesDirectory + "/WotdMixedPartsAvailableSamples.json";
        
        Dependency[]? dependencies = await FileUtilities
            .JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        Assert.IsType<Dependency[]>(dependencies);

        WorkOrder[]? workOrders = await FileUtilities
            .JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities
            .JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        WorkOrder[] workOrdersReturned = _optimizationService.OptimizeByParts(dtoList);
        int[] expectedWorkOrderIds = [1, 2, 5, 3, 4];

        Assert.True(IsSortedByCriticality(dtoList, workOrdersReturned, expectedWorkOrderIds));
    }
    
    [Fact]
    public async Task ThrowsValidationExceptionWhenMissingDateTimeAttribute()
    {
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples.json";
        string workOrderSamplesPath = _generalSamplesDirectory + "/WorkOrderSamples.json";
        string workOrderToDependencySamplesPath = _optimizationServiceSamplesDirectory + "/WotdMissingOneDateTimeAttributeSamples.json";
        
        Dependency[]? dependencies = await FileUtilities
            .JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        Assert.IsType<Dependency[]>(dependencies);

        WorkOrder[]? workOrders = await FileUtilities
            .JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities
            .JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        Assert.Throws<ValidationException>(() => _optimizationService.OptimizeByParts(dtoList));
    }
    
    [Fact (Skip = "Reasons")]
    public void UnScheduledWhenInfeasibleTimeWindow(){}
    
    [Fact (Skip = "Reasons")]
    public void StartEqualsStopWhenZeroDurationWorkOrder(){}


    private bool IsSortedByCriticality(IEnumerable<CustomWorkOrderDependencyDto> dtoList, WorkOrder[] workOrders, int[] expectedWorkOrderIds)
    {        
        if (workOrders.Length != expectedWorkOrderIds.Length)
        {
            return false;
        }
        
        for(int i = 0; i < workOrders.Length; i++)
        {
            int workOrderId = workOrders[i].Id;
            if (workOrderId != expectedWorkOrderIds[i])
            {
                _output.WriteLine($"Expected: {expectedWorkOrderIds[i]}. Got: {workOrderId}");
                return false;
            }
        }

        return true;
    }

    
}
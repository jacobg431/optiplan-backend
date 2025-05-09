using Optiplan.DatabaseResources;
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
        Assert.Throws<ArgumentNullException>(() => _optimizationService.OptimizeByParts(default!));
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
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        WorkOrder[] workOrdersReturned = _optimizationService.OptimizeByParts(dtoList);
        int[] expectedWorkOrderIds = [1, 5, 3, 2, 4];

        Assert.True(IsSortedByCriticality(workOrdersReturned, expectedWorkOrderIds));
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
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        WorkOrder[] workOrdersReturned = _optimizationService.OptimizeByParts(dtoList);
        int[] expectedWorkOrderIds = [1, 5, 3, 2, 4];

        Assert.True(IsSortedByCriticality(workOrdersReturned, expectedWorkOrderIds));
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
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        WorkOrder[] workOrdersReturned = _optimizationService.OptimizeByParts(dtoList);
        int[] expectedWorkOrderIds = [1, 2, 5, 3, 4];

        Assert.True(IsSortedByCriticality(workOrdersReturned, expectedWorkOrderIds));
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
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        Assert.Throws<ValidationException>(() => _optimizationService.OptimizeByParts(dtoList));
    }
    
    [Fact]
    public async Task UnScheduledWhenInfeasibleTimeWindow()
    {
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples.json";
        string workOrderSamplesPath = _generalSamplesDirectory + "/WorkOrderSamples.json";
        string workOrderToDependencySamplesPath = _optimizationServiceSamplesDirectory + "/WotdInfeasibleTimeWindowsSamples.json";
        
        Dependency[]? dependencies = await FileUtilities
            .JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        Assert.IsType<Dependency[]>(dependencies);

        WorkOrder[]? workOrders = await FileUtilities
            .JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities
            .JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        WorkOrder[] workOrdersReturned = _optimizationService.OptimizeByParts(dtoList);
        Assert.NotEmpty(workOrdersReturned);

        int targetWorkOrderId = 1;

        DateTime? dateTime = workOrdersReturned
            .Where(w => w.Id.Equals(targetWorkOrderId))
            .Select(w => w.StartDateTime)
            .FirstOrDefault(DateTime.Parse("2000-01-01 00:00:00"));
        
        Assert.Null(dateTime);
    }
    
    [Fact]
    public async Task StartEqualsStopWhenZeroDurationWorkOrder()
    {
        string dependencySamplesPath = _generalSamplesDirectory + "/DependencySamples.json";
        string workOrderSamplesPath = _optimizationServiceSamplesDirectory + "/WoZeroDurationWorkOrderSamples.json";
        string workOrderToDependencySamplesPath = _generalSamplesDirectory + "/WorkOrderToDependencySamples.json";
        
        Dependency[]? dependencies = await FileUtilities
            .JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        Assert.IsType<Dependency[]>(dependencies);

        WorkOrder[]? workOrders = await FileUtilities
            .JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities
            .JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);

        IEnumerable<CustomWorkOrderDependencyDto> dtoList = CustomWorkOrderDependencyMapper
            .ToDtoList(dependencies, workOrders, workOrdersToDependencies
        );

        WorkOrder[] workOrdersReturned = _optimizationService.OptimizeByParts(dtoList);
        Assert.NotEmpty(workOrdersReturned);

        int targetWorkOrderId = 18;

        DateTime? startDateTime = workOrdersReturned
            .Where(w => w.Id.Equals(targetWorkOrderId))
            .Select(w => w.StartDateTime)
            .FirstOrDefault();
        Assert.NotNull(startDateTime);

        DateTime? stopDateTime = workOrdersReturned
            .Where(w => w.Id.Equals(targetWorkOrderId))
            .Select(w => w.StopDateTime)
            .FirstOrDefault();
        Assert.NotNull(stopDateTime);

        Assert.Equal(startDateTime, stopDateTime);
    }

    private bool IsSortedByCriticality(WorkOrder[] workOrders, int[] expectedWorkOrderIds)
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
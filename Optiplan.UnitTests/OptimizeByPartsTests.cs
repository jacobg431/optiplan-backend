using Microsoft.AspNetCore.Mvc;
using Moq;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Controllers;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;
using Optiplan.WebApi.Utilities;
using Optiplan.WebApi.DataTransferObjects;
using Xunit.Abstractions;

namespace Optiplan.UnitTests;

public class OptimizeByPartsTests
{
    private readonly Mock<IDependencyRepository> _dependencyRepositoryMock;
    private readonly Mock<IWorkOrderToDependencyRepository> _workOrderToDependencyRepositoryMock;
    private readonly Mock<IWorkOrderRepository> _workOrderRepositoryMock;
    private readonly IOptimizationService _optimizationService;
    private readonly OptimizationController _optimizationController;
    private readonly ITestOutputHelper _output;

    public OptimizeByPartsTests(ITestOutputHelper output)
    {
        _workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        _workOrderToDependencyRepositoryMock = new Mock<IWorkOrderToDependencyRepository>();
        _dependencyRepositoryMock = new Mock<IDependencyRepository>();
        _optimizationService = new OptimizationService();

        _optimizationController = new OptimizationController(
            _dependencyRepositoryMock.Object,
            _workOrderToDependencyRepositoryMock.Object,
            _workOrderRepositoryMock.Object,
            _optimizationService
        );

        _output = output;
    }

    [Fact]
    public async Task ReturnsCreatedResult()
    {
        // Defining file paths for sample data
        string baseDirectory = System.AppContext.BaseDirectory + "../../../Resources";
        string workOrderToDependencySamplesPath = baseDirectory + "/WorkOrderToDependencySamples.json";
        string workOrderSamplesPath = baseDirectory + "/WorkOrderSamples.json";
        string dependencySamplesPath = baseDirectory + "/DependencySamples.json";

        // Mocking WorkOrderRepository
        WorkOrder[]? workOrders = await FileUtilities.JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);
        _workOrderRepositoryMock.Setup(r => r.RetrieveAllAsync()).ReturnsAsync(workOrders);

        // Mocking WorkOrderToDependencyRepository
        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities
            .JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);
        _workOrderToDependencyRepositoryMock.Setup(r => r.RetrieveAllAsync()).ReturnsAsync(workOrdersToDependencies);

        // Mocking WorkOrderToDependencyRepository
        Dependency[]? dependencies = await FileUtilities
            .JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        Assert.IsType<Dependency[]>(dependencies);
        _dependencyRepositoryMock.Setup(r => r.RetrieveAllAsync()).ReturnsAsync(dependencies);

        // Filter out "Other Work Orders" dependencies with invalid references and Creating DTO list
        IEnumerable<int> existingWorkOrderIds = workOrders.Select(w => w.Id).Distinct();
        IEnumerable<WorkOrderToDependencyDto> dtoList = workOrdersToDependencies
            .Where(dep =>
                dep.DependencyId != 1 || (dep.IntegerAttributeValue.HasValue && existingWorkOrderIds.Contains(dep.IntegerAttributeValue.Value))
            )
            .Select(WorkOrderToDependencyMapper.ToDto);

        // Logging to console which instances are being skipped
        var skippedDeps = workOrdersToDependencies.Where(dep =>
            dep.DependencyId == 1 && (!dep.IntegerAttributeValue.HasValue || !existingWorkOrderIds.Contains(dep.IntegerAttributeValue.Value))
        );

        foreach (var dep in skippedDeps)
        {
            _output.WriteLine($"Skipping invalid dependencyInstanceId={dep.DependencyInstanceId} referencing missing WorkOrderId={dep.IntegerAttributeValue}");
        }

        Assert.NotEmpty(dtoList);

        // Action
        ActionResult result = await _optimizationController.OptimizeByParts(dtoList);

        // Assertions
        var statusResult = Assert.IsType<ObjectResult>(result);
        _output.WriteLine(statusResult.Value?.ToString());
        Assert.Equal(201, statusResult.StatusCode);
        var optimizedWorkOrders = Assert.IsAssignableFrom<IEnumerable<WorkOrder>>(statusResult.Value);
        Assert.Equal(workOrders.Count(), optimizedWorkOrders.Count());
    }

    [Fact]
    public async Task ReturnsBadRequestWhenNoWorkOrdersToDependencies()
    {
        WorkOrderToDependency[] workOrdersToDependencies = [];
        _workOrderToDependencyRepositoryMock.Setup(r => r.RetrieveAllAsync()).ReturnsAsync(workOrdersToDependencies);

        IEnumerable<WorkOrderToDependencyDto> dtoList = workOrdersToDependencies.Select(WorkOrderToDependencyMapper.ToDto);
        var result = await _optimizationController.OptimizeByParts(dtoList);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ReturnsInternalServerError()
    {
        string baseDirectory = System.AppContext.BaseDirectory + "../../../Resources";
        string workOrderToDependencySamplesPath = baseDirectory + "/WorkOrderToDependencySamples.json";
        string workOrderSamplesPath = baseDirectory + "/WorkOrderSamples.json";

        WorkOrder[]? workOrders = await FileUtilities.JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);
        _workOrderRepositoryMock.Setup(r => r.RetrieveAllAsync()).ReturnsAsync(workOrders);

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities
            .JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);
        _workOrderToDependencyRepositoryMock.Setup(r => r.RetrieveAllAsync()).ThrowsAsync(new System.Exception("Database error"));

        IEnumerable<WorkOrderToDependencyDto> dtoList = workOrdersToDependencies.Select(WorkOrderToDependencyMapper.ToDto);
        var result = await _optimizationController.OptimizeByParts(dtoList);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
using Microsoft.AspNetCore.Mvc;
using Moq;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Controllers;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;
using Optiplan.WebApi.Utilities;

namespace Optiplan.UnitTests;

public class OptimizeByPartsTests
{
    private readonly Mock<IDependencyRepository> _dependencyRepositoryMock;
    private readonly Mock<IWorkOrderToDependencyRepository> _workOrderToDependencyRepositoryMock;
    private readonly Mock<IWorkOrderRepository> _workOrderRepositoryMock;
    private readonly Mock<IOptimizationService> _optimizationServiceMock;
    private readonly OptimizationController _optimizationController;

    public OptimizeByPartsTests()
    {
        _workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        _workOrderToDependencyRepositoryMock = new Mock<IWorkOrderToDependencyRepository>();
        _dependencyRepositoryMock = new Mock<IDependencyRepository>();
        _optimizationServiceMock = new Mock<IOptimizationService>();

        _optimizationController = new OptimizationController(
            _dependencyRepositoryMock.Object,
            _workOrderToDependencyRepositoryMock.Object,
            _workOrderRepositoryMock.Object,
            _optimizationServiceMock.Object
        );
    }

    [Fact]
    public async Task ReturnsCreatedResult()
    {
        // Defining file paths for sample data
        string baseDirectory = System.AppContext.BaseDirectory + "../../../Resources";
        string workOrderToDependencySamplesPath = baseDirectory + "/WorkOrderToDependencySamples.json";
        string workOrderSamplesPath = baseDirectory + "/WorkOrderSamples.json";
        string dependencySamplesPath = baseDirectory + "/DependencySamples.json";

        // Mocking repositories and services
        WorkOrder[]? workOrders = await FileUtilities.JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        Assert.IsType<WorkOrder[]>(workOrders);
        _workOrderRepositoryMock.Setup(r => r.RetrieveAllAsync()).ReturnsAsync(workOrders);

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities.JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);
        _workOrderToDependencyRepositoryMock.Setup(r => r.RetrieveAllAsync()).ReturnsAsync(workOrdersToDependencies);

        _optimizationServiceMock.Setup(s => s.OptimizeByPartsAsync(It.IsAny<object>())).ReturnsAsync(workOrders);

        // Action
        ActionResult result = await _optimizationController.OptimizeByParts(workOrdersToDependencies);

        // Method invocation verification
        _optimizationServiceMock.Verify(s => s.OptimizeByPartsAsync(It.IsAny<object>()), Times.Once());

        // Assertions
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, statusResult.StatusCode);
        var optimizedWorkOrders = Assert.IsAssignableFrom<IEnumerable<WorkOrder>>(statusResult.Value);
        Assert.Equal(workOrders.Count(), optimizedWorkOrders.Count());
    }

    [Fact]
    public async Task ReturnsBadRequestWhenNoWorkOrdersToDependencies()
    {
        WorkOrderToDependency[] workOrdersToDependencies = [];
        _workOrderToDependencyRepositoryMock.Setup(r => r.RetrieveAllAsync()).ReturnsAsync(workOrdersToDependencies);

        var result = await _optimizationController.OptimizeByParts(workOrdersToDependencies);

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

        WorkOrderToDependency[]? workOrdersToDependencies = await FileUtilities.JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        Assert.IsType<WorkOrderToDependency[]>(workOrdersToDependencies);
        _workOrderToDependencyRepositoryMock.Setup(r => r.RetrieveAllAsync()).ThrowsAsync(new System.Exception("Database error"));

        var result = await _optimizationController.OptimizeByParts(workOrdersToDependencies);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
using Microsoft.AspNetCore.Mvc;
using Moq;
using Optiplan.DatabaseResources;
using Optiplan.WebApi.Controllers;
using Optiplan.WebApi.Repositories;
using Optiplan.WebApi.Services;
using Optiplan.WebApi.Utilities;
using Xunit.Abstractions;

namespace Optiplan.UnitTests;

public class OptimizeByPartsTests
{
    private readonly Mock<IDependencyRepository> _dependencyRepositoryMock;
    private readonly Mock<IWorkOrderToDependencyRepository> _workOrderToDependencyRepositoryMock;
    private readonly Mock<IWorkOrderRepository> _workOrderRepositoryMock;
    private readonly Mock<IOptimizationService> _optimizationServiceMock;
    private readonly OptimizationController _optimizationController;

    private readonly ITestOutputHelper _output; // TEMP DEBUG

    public OptimizeByPartsTests(ITestOutputHelper output)
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

        _output = output;

    }

    [Fact]
    public async Task ReturnsCreatedResult()
    {
 
        string baseDirectory = System.AppContext.BaseDirectory + "../../../Resources";
        string workOrderToDependencySamplesPath = baseDirectory + "/WorkOrderToDependencySamples.json";
        string workOrderSamplesPath = baseDirectory + "/WorkOrderSamples.json";
        string dependencySamplesPath = baseDirectory + "/DependencySamples.json";

        WorkOrder[] workOrders = await FileUtilities.JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        _workOrderRepositoryMock.SetupSequence(r => r.RetrieveAllAsync()).ReturnsAsync(workOrders);

        //_output.WriteLine(workOrders.Count().ToString());
        
        WorkOrderToDependency[] workOrdersToDependencies = await FileUtilities.JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        _workOrderToDependencyRepositoryMock.SetupSequence(r => r.RetrieveAllAsync()).ReturnsAsync(workOrdersToDependencies);
        
        Dependency[] dependencies = await FileUtilities.JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        _dependencyRepositoryMock.SetupSequence(r => r.RetrieveAllAsync()).ReturnsAsync(dependencies);

        _optimizationServiceMock.SetupSequence(s => s.OptimizeByPartsAsync(workOrdersToDependencies)).ReturnsAsync(workOrders);

        ActionResult result = await _optimizationController.OptimizeByParts(workOrdersToDependencies);
        //_output.WriteLine(((ObjectResult) result).StatusCode.ToString());
        //_output.WriteLine(((ObjectResult) result).Value?.ToString());

        var createdResult = Assert.IsType<CreatedResult>(result);
        var optimizedData = Assert.IsAssignableFrom<IEnumerable<WorkOrder>>(createdResult.Value);
        Assert.Equal(dependencies.Count(), optimizedData.Count());
    }

    [Fact]
    public async Task ReturnsBadRequestWhenNoWorkOrdersToDependencies()
    {
        WorkOrderToDependency[] workOrdersToDependencies = [];
        _workOrderToDependencyRepositoryMock.SetupSequence(r => r.RetrieveAllAsync()).ReturnsAsync(workOrdersToDependencies);

        var result = await _optimizationController.OptimizeByParts(workOrdersToDependencies);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ReturnsInternalServerError()
    {
        string baseDirectory = System.AppContext.BaseDirectory + "../../../Resources";
        string workOrderToDependencySamplesPath = baseDirectory + "/WorkOrderToDependencySamples.json";
        string workOrderSamplesPath = baseDirectory + "/WorkOrderSamples.json";

        WorkOrder[] workOrders = await FileUtilities.JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        _workOrderRepositoryMock.SetupSequence(r => r.RetrieveAllAsync()).ReturnsAsync(workOrders);

        WorkOrderToDependency[] workOrdersToDependencies = await FileUtilities.JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        _workOrderToDependencyRepositoryMock.Setup(r => r.RetrieveAllAsync()).ThrowsAsync(new System.Exception("Database error"));

        var result = await _optimizationController.OptimizeByParts(workOrdersToDependencies);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
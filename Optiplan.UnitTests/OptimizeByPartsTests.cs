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
        //var workOrders = new WorkOrder[]
        //{
        //    new WorkOrder { Id = 1 },
        //    new WorkOrder { Id = 2 },
        //    new WorkOrder { Id = 3 }
        //};

        string baseDirectory = System.AppContext.BaseDirectory + "/Optiplan.UnitTests/Resources";
        string workOrderToDependencySamplesPath = baseDirectory + "/WorkOrderToDependencySamples.json";
        string workOrderSamplesPath = baseDirectory + "/WorkOrderSamples.json";
        string dependencySamplesPath = baseDirectory + "/DependencySamples.json";

        WorkOrder[] workOrders = await FileUtilities.JsonFileReaderAsync<WorkOrder[]>(workOrderSamplesPath);
        _workOrderRepositoryMock.SetupSequence(r => r.RetrieveAllAsync()).ReturnsAsync(workOrders);
        
        WorkOrderToDependency[] workOrdersToDependencies = await FileUtilities.JsonFileReaderAsync<WorkOrderToDependency[]>(workOrderToDependencySamplesPath);
        _workOrderToDependencyRepositoryMock.SetupSequence(r => r.RetrieveAllAsync()).ReturnsAsync(workOrdersToDependencies);
        
        Dependency[] dependencies = await FileUtilities.JsonFileReaderAsync<Dependency[]>(dependencySamplesPath);
        _dependencyRepositoryMock.SetupSequence(r => r.RetrieveAllAsync()).ReturnsAsync(dependencies);

        //_optimizationServiceMock.SetupSequence(s => s.OptimizeByPartsAsync(
        //    It.IsAny<WorkOrderToDependency[]>())
        //).ReturnsAsync();

        var result = await _optimizationController.OptimizeByParts(workOrdersToDependencies);

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
}
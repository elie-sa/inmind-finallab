using System.Text.Json;
using LoggingMicroservice.Controllers;
using LoggingMicroservice.DbContext;
using LoggingMicroservice.Models;
using LoggingMicroservice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinalLabInmind.Tests.LoggingMicroserviceTests;

public class LoggingControllerTests
{
    private readonly Mock<IAppDbContext> _contextMock;
    private readonly LogQueue _logQueue;
    private readonly LoggingController _controller;

    public LoggingControllerTests()
    {
        _contextMock = new Mock<IAppDbContext>();
        _logQueue = new LogQueue();
        _controller = new LoggingController(_contextMock.Object, _logQueue);
    }

    [Fact]
    public async Task StoreLogs_ReturnsBadRequest_WhenNoLogsExist()
    {
        // Arrange (LogQueue is empty)

        // Act
        var result = await _controller.StoreLogs();

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No logs to store.", badRequest.Value);
    }

    [Fact]
    public async Task StoreLogs_ReturnsOk_WhenLogsExist()
    {
        // Arrange
        var log = new Log
        {
            RequestId = Guid.NewGuid(),
            RouteURL = "/test",
            Timestamp = DateTime.UtcNow,
            RequestObject = JsonDocument.Parse("{}")
        };
        _logQueue.Enqueue(log);

        var dbSetMock = new Mock<DbSet<Log>>();
        _contextMock.Setup(c => c.Logs).Returns(dbSetMock.Object);
        _contextMock.Setup(c => c.Logs.AddRangeAsync(It.IsAny<IEnumerable<Log>>(), default))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.StoreLogs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("logs stored successfully", okResult.Value.ToString());
    }
}
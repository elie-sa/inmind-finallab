using FinalLabInmind.Controllers;
using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
using FinalLabInmind.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinalLabInmind.Tests.Events;

public class TransactionEventsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly TransactionEventsController _controller;
    private readonly IAppDbContext _context;

    public TransactionEventsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TransactionEventsController(_mediatorMock.Object);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task PostEvent_ShouldPublishTransactionEvent()
    {
        var dto = new EventDto { EventType = "Refunded", Details = "Transaction rolled back", Timestamp = DateTime.UtcNow };

        var result = await _controller.PostEvent(dto, transactionId: 1);

        _mediatorMock.Verify(m => m.Publish(It.IsAny<TransactionEvent>(), default), Times.Once);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("dispatched", ok.Value?.ToString());
    }

    [Fact]
    public async Task GetEvents_ShouldReturnEvents_WhenTheyExist()
    {
        long transactionId = 22;

        var ctx = _context as AppDbContext;
        ctx.TransactionEvents.AddRange(
            new TransactionEvent { TransactionId = transactionId, EventType = "Rollback", Details = "Rollback triggered", Timestamp = DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        var controller = new TransactionEventsController(_mediatorMock.Object);
        var result = await controller.GetEvents(_context, transactionId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<List<TransactionEvent>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnNotFound_WhenNoEvents()
    {
        var result = await _controller.GetEvents(_context, 888);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No events found for this transaction.", notFound.Value);
    }
}
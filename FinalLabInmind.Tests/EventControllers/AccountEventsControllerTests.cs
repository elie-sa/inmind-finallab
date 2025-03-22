using FinalLabInmind.Controllers;
using FinalLabInmind.DbContext;
using FinalLabInmind.DTOs;
using FinalLabInmind.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinalLabInmind.Tests.EventControllers;

public class AccountEventsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AccountEventsController _controller;
    private readonly IAppDbContext _context;

    public AccountEventsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AccountEventsController(_mediatorMock.Object);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task PostEvent_ShouldPublishAccountEvent()
    {
        var dto = new EventDto { EventType = "Created", Details = "Account created", Timestamp = DateTime.UtcNow };

        var result = await _controller.PostEvent(dto, accountId: 1);

        _mediatorMock.Verify(m => m.Publish(It.IsAny<AccountEvent>(), default), Times.Once);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("dispatched", ok.Value?.ToString());
    }

    [Fact]
    public async Task GetEvents_ShouldReturnEvents_WhenTheyExist()
    {
        long accountId = 10;

        // Seed test events
        var ctx = _context as AppDbContext;
        ctx.AccountEvents.AddRange(
            new AccountEvent { AccountId = accountId, EventType = "Updated", Details = "Changed name", Timestamp = DateTime.UtcNow },
            new AccountEvent { AccountId = accountId, EventType = "Notified", Details = "User notified", Timestamp = DateTime.UtcNow.AddMinutes(-10) }
        );
        await ctx.SaveChangesAsync();

        var controller = new AccountEventsController(_mediatorMock.Object);
        var result = await controller.GetEvents(_context, accountId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<List<AccountEvent>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnNotFound_WhenNoEvents()
    {
        var result = await _controller.GetEvents(_context, 999);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No events found for this account.", notFound.Value);
    }
}
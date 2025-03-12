using System.Text;
using FinalLabInmind.Interfaces;
using Newtonsoft.Json;

namespace FinalLabInmind;

public class RequestLoggingMiddleware: IMiddleware
{
    private readonly IMessagePublisher _messagePublisher;

    public RequestLoggingMiddleware(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var request = context.Request;
        var requestTime = DateTime.UtcNow;

        string requestBody = string.Empty;
        if (request.ContentLength > 0 && request.Method != "GET")
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        var logData = new
        {
            request_id = Guid.NewGuid(),
            request_object = requestBody,
            route_url = context.Request.Path,
            timestamp = DateTime.UtcNow 
        };

        string jsonMessage = JsonConvert.SerializeObject(logData);

        await _messagePublisher.PublishLogAsync(jsonMessage);
        
        await next(context);
    }
}
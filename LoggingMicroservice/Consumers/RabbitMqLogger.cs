using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonElement = System.Text.Json.JsonElement;
using System.Text.Json;
using LoggingMicroservice.DbContext;
using LoggingMicroservice.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LoggingMicroservice.Consumers;

public class RabbitMqLogger : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;
        
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqLogger(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var rabbitMQConfig = configuration.GetSection("RabbitMQ");
            _factory = new ConnectionFactory
            {
                HostName = rabbitMQConfig["Host"],
                UserName = rabbitMQConfig["Username"],
                Password = rabbitMQConfig["Password"]
            };
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = await _factory.CreateConnectionAsync("LoggingListener");
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: "logging_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received: {message}");

                await ProcessMessageAsync(message);

            };

            await _channel.BasicConsumeAsync(
                queue: "logging_queue",
                autoAck: false, 
                consumer: consumer
            );

            await Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(string message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            
                var messageObject = JsonSerializer.Deserialize<JsonElement>(message);
                var id = messageObject.GetProperty("request_id").GetGuid();
                var requestObject = messageObject.GetProperty("request_object").GetRawText();
                var routeUrl = messageObject.GetProperty("route_url").GetString();
                var timeStamp = messageObject.GetProperty("timestamp").GetDateTime();
            
                Console.WriteLine($"request object: {requestObject}");
                var log = new Log
                {
                    RequestId = id,
                    RequestObject = JsonDocument.Parse(requestObject),
                    RouteURL = routeUrl,
                    Timestamp = timeStamp
                };

                context.Logs.Add(log);
                await context.SaveChangesAsync();
            }
        }
    }
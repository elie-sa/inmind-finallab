using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonElement = System.Text.Json.JsonElement;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LoggingMicroservice.Consumers;

public class RabbitMqListenerService : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;

        private readonly IServiceProvider _serviceProvider;

        public RabbitMqListenerService(IConfiguration configuration, IServiceProvider serviceProvider)
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
            _connection = await _factory.CreateConnectionAsync("BankingServiceListener");
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: "transaction_logs",
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
                queue: "transaction_logs",
                autoAck: true, 
                consumer: consumer
            );

            await Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(string message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var messageObject = JsonSerializer.Deserialize<JsonElement>(message);
                var transactionType = messageObject.GetProperty("TransactionType").GetString();
                var accountId = messageObject.GetProperty("AccountId").GetInt64();
                var amount = messageObject.GetProperty("Amount").GetDecimal();
                var status = messageObject.GetProperty("Status").GetString();
                var details = messageObject.GetProperty("Details").GetString();

                Console.WriteLine($"{transactionType} of amount: {amount} for account {accountId}");
                Console.WriteLine($"Status: {status}, Details: {details}");
            }
        }
    }
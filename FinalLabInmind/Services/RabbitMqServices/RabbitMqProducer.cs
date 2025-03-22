using System.Text;
using FinalLabInmind.DTOs;
using FinalLabInmind.Interfaces;
using LoggingMicroservice.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace FinalLabInmind.Services.RabbitMq;

public class RabbitMqProducer : IMessagePublisher
{
    private readonly string _host;
    private readonly string _transactionQueue;
    private readonly string _loggingQueue;
    private readonly string _username;
    private readonly string _password;

    public RabbitMqProducer(IConfiguration configuration)
    {
        _host = configuration["RabbitMQ:Host"];
        _transactionQueue = configuration["RabbitMQ:TransactionQueue"];
        _loggingQueue = configuration["RabbitMQ:LoggingQueue"];
        _username = configuration["RabbitMQ:Username"];
        _password = configuration["RabbitMQ:Password"];
    }

    public async Task PublishTransactionAsync(TransactionLog transaction)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _host,
            UserName = _username,
            Password = _password
        };

        using var connection = await factory.CreateConnectionAsync("BankingServiceListener");
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: _transactionQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var transactionDto = new TransactionLogDto(transaction);
        var message = JsonConvert.SerializeObject(transactionDto);
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(exchange: "", routingKey: _transactionQueue, body: body);
    }

    public async Task PublishLogAsync(string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _host,
            UserName = _username,
            Password = _password
        };

        using var connection = await factory.CreateConnectionAsync("LoggingListener");
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: _loggingQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(exchange: "", routingKey: _loggingQueue, body: body);
    }
}
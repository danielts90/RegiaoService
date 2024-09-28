using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RegiaoApi.Models;

public class Producer : IMessageProducer
{
    private readonly string _queueName;

    public Producer(string queueName)
    {
        _queueName = queueName;
    }

    public void SendMessageToQueue<T>(Message<T> bodyMessage) where T : class
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var messageJson = JsonSerializer.Serialize(bodyMessage);
            var body = Encoding.UTF8.GetBytes(messageJson);

            channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}


using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;

class Worker
{
    public static void Main()
    {
        //https://leonidius2010.wordpress.com/2017/09/21/reading-appsettings-in-net-core-2-console-application/
        IConfiguration config = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", true, true)
          .Build();
        string RabbitMQHost = config["RabbitMQHost"];
        string RabbitMQQueueName = config["RabbitMQQueueName"];

        //https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html
        var factory = new ConnectionFactory() { HostName = RabbitMQHost };
        using(var connection = factory.CreateConnection())
        using(var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: RabbitMQQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            Console.WriteLine(" [*] Waiting for messages..");
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
                //int dots = message.Split('.').Length - 1;
                //Thread.Sleep(dots * 1000);
                Console.WriteLine(" [x] Done {0}", message);
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            channel.BasicConsume(queue: RabbitMQQueueName, autoAck: false, consumer: consumer);
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}

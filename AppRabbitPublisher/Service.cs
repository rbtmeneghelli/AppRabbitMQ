using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppRabbitPublisher
{
    public class Service
    {
        public void ServiceSimple()
        {
            // Conexão do App com o RabbitMQ
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                // Declaração da fila e envio de mensagem
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.QueueDeclare(
                        queue: "MessageService",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    string message = "Tutorial RabbitMQ!";
                    var body = Encoding.UTF8.GetBytes(message);

                    // Publicação da mensagem
                    channel.BasicPublish(exchange: "",
                        routingKey: "MessageService",
                        basicProperties: null,
                        body: body);

                    Console.WriteLine("Mensagem Enviada!");
                }
                Console.ReadLine();
            }
        }

        public void ServiceMultiple()
        {
            // Conexão do App com o RabbitMQ
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                // Declaração da fila e envio de mensagem
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.QueueDeclare(
                        queue: "MessageService",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    for (int i = 0; i < 10; i++)
                    {
                        var user = new User
                        {
                            Id = i
                        };

                        string steam = JsonConvert.SerializeObject(user);

                        channel.BasicPublish("", "MessageService", properties, Encoding.UTF8.GetBytes(steam));
                        Console.WriteLine("Mensagem Enviada!");
                    }
                }
                Console.ReadLine();
            }
        }

        public void ServiceThird()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                var message = GetMessage(new string[] { "Texto para RABBITMQ" });
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "logs",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        public void ServiceFour()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");

                var severity = "error";
                var message = "ABCDE";
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "direct_logs",
                                     routingKey: severity,
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0)
                   ? string.Join(" ", args)
                   : "info: Hello World!");
        }
    }
}

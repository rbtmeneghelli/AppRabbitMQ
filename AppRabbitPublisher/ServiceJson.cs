using AppRabbit.Business;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppRabbitPublisher
{
    public class ServiceJson
    {
        public static ConnectionFactory GetConnectionFactory()
        {
            return new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
        }

        public static IConnection CreateConnection(ConnectionFactory connectionFactory)
        {
            return connectionFactory.CreateConnection();
        }

        private static void SaveLog(Exception ex)
        {
            Console.WriteLine(ex.ToString());
            File.AppendAllText("log.txt", $"{DateTime.Now}\r\n\r\n{ex.ToString()}\r\n\r\n");
        }

        private static void Sleep(int timeOut)
        {
            Thread.Sleep(TimeSpan.FromSeconds(timeOut));
        }

        public static byte[] ObjectToByteArray(ValidacaoRecord model)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, model);
                return memoryStream.ToArray();
            }
        }

        private static async Task<List<ValidacaoRecord>> BuildProcessQueue()
        {
            Console.WriteLine("Iniciando montagem de fila de processos para validação, por favor aguarde...");
            // Chamar metodo que vai montar a fila de processos...
            return await Task.FromResult(new List<ValidacaoRecord>());
        }

        private static async Task CreateProcessInQueue(IModel channel, List<ValidacaoRecord> filaProcesso)
        {
            if (filaProcesso.Count > 0)
            {
                Console.WriteLine($"Enviando o(s) {filaProcesso.Count} processos da fila, por favor aguarde...");
                foreach (ValidacaoRecord itemFilaProcesso in filaProcesso)
                {
                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    channel.BasicPublish(exchange: String.Empty, routingKey: "Fila de arquivos", basicProperties: properties, body: ObjectToByteArray(itemFilaProcesso));
                }
                Console.WriteLine("Processos Enviados com sucesso");
            }
            else
            {
                Console.WriteLine("Aguardando novos processos para envio montagem de fila...");
            }
        }

        private static async Task Process()
        {
            // Na main colocar Process().Wait() para dar start!
            bool temErro = false;
            IConnection connection = CreateConnection(GetConnectionFactory());
            IModel channel = connection.CreateModel();
            channel.QueueDeclare(queue: "Fila de arquivos", durable: true, exclusive: false, autoDelete: false, arguments: null);
            while (true)
            {
                try
                {
                    Program program = new Program();
                    List<ValidacaoRecord> lista = await BuildProcessQueue();
                    await CreateProcessInQueue(channel, lista);
                    Sleep(60);
                }
                catch (Exception ex)
                {
                    temErro = true;
                    SaveLog(ex);
                }
                if (temErro)
                {
                    temErro = false;
                    continue;
                }
            }
        }
    }
}


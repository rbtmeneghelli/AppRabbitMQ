using AppRabbit.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppRabbitConsumer
{
    public class ReceiveJson
    {
        private static string urlApi = "URL DA API A SER CHAMADA...";
        private static int NUMBER_OF_WORKROLES = 3;

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

        private static ValidacaoRecord ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            memoryStream.Write(arrBytes, 0, arrBytes.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            ValidacaoRecord model = (ValidacaoRecord)binaryFormatter.Deserialize(memoryStream);
            return model;
        }

        private static async Task SendRequestToGetFile(ValidacaoRecord model)
        {
            using (var client = new HttpClient())
            {
                string Json = JsonConvert.SerializeObject(model, Formatting.Indented);
                var dados = new StringContent(Json, Encoding.UTF8, "application/json");
                var result = await client.PostAsync($"{urlApi}", dados);
                var contents = await result.Content.ReadAsStringAsync();
                var json = JObject.Parse(contents);
                var success = bool.Parse(json.GetValue("sucesso").ToString());
                if (success)
                {
                    var validacaoRecord = JsonConvert.DeserializeObject<ValidacaoRecord>(json.GetValue("dados").ToString());
                    await CreateFileOnProcessFolder(validacaoRecord);
                }
            }
        }

        private static async Task CreateFileOnProcessFolder(ValidacaoRecord validacaoRecord)
        {
            if (!string.IsNullOrWhiteSpace(validacaoRecord.NomeArquivo))
            {
                string diretorio = Path.Combine(@"d:\\Pasta_Roberto", validacaoRecord.NomeArquivo.Replace("/", ""));
                if (File.Exists(diretorio))
                    File.Delete(diretorio);
                File.WriteAllBytes(diretorio, validacaoRecord.ArquivoByte);
            }
        }

        private static async Task Process()
        {
            bool temErro = false;
            IConnection connection = CreateConnection(GetConnectionFactory());
            IModel channel = connection.CreateModel();
            for (int i = 0; i < NUMBER_OF_WORKROLES; i++)
            {
                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        lock (channel)
                        {
                            var consumer = new EventingBasicConsumer(channel);
                            consumer.ConsumerTag = Guid.NewGuid().ToString();
                            consumer.Received += (sender, ea) =>
                            {
                                ValidacaoRecord body = ByteArrayToObject(ea.Body);
                                SendRequestToGetFile(body).Wait();
                                Sleep(20);
                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: true);
                            };
                            //Registra os consumidor no RabbitMQ
                            channel.BasicConsume("Fila de arquivos", autoAck: false, consumer: consumer);
                        }
                    });
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

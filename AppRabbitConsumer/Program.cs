using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace AppRabbitConsumer
{
    class Program
    {
        private static Receive receive;

        static void Main(string[] args)
        {
            receive = new Receive();
            // receive.ReceiveFirstAndSecond();
            // receive.ReceiveThird();
            receive.ReceiveFour();
        }
    }
}
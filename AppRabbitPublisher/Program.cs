using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;

namespace AppRabbitPublisher
{
    class Program
    {
        private static Service service;

        static void Main(string[] args)
        {
            service = new Service();
            // service.ServiceSimple();
            // service.ServiceMultiple();
            // service.ServiceThird();
            service.ServiceFour();
        }
    }
}

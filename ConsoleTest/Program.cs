using Fleck;
using Fleck.Extensions;
using Fleck.Extensions.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var configuration = configBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            FleckLog.Level = Fleck.LogLevel.Error;

            var build = new WebSocketServerBuilder(services);

            var redis = configuration["Redis"];
            var address = configuration["WebSocketListenAddres"];


            build.AddStackExchangeRedis(redis)
                .UseHandlerMessage<GroupHandler>("opt.group")
                .UseHandlerMessage<GroupSendHandler>("opt.sendgroup")
                .UseHandlerMessage<TestHandler>("opt.test")
                .Build((option) =>
                {
                    option.Location = address;
                }).Start();

            var input = Console.ReadLine();
            while (input != "exit")
            {
                input = Console.ReadLine();
            }
        }
    }
}

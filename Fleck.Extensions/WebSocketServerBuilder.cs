using Fleck.Extensions.Core;
using Fleck.Extensions.Core.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public class WebSocketServerBuilder : IDisposable
    {      
        private ServiceConfigurationOption serviceConfigurationOption;

        public IServiceCollection Services { get; }
        public IMessageTypeMapper MessageTypeMapper { get; }
        public IInvokeHandlerManager InvokeHandlerManager { get; }
        public IServiceProvider ServiceProvider { get; private set; }

        public WebSocketServerBuilder(IServiceCollection services)
        {
            this.Services = services;
            this.MessageTypeMapper = new MessageTypeMapper();
            this.InvokeHandlerManager = new InvokeHandlerManager();
            this.RegisterDefaultService();
        }


        public HaWebSocketServer Build(Action<ServiceConfigurationOption> optionsAction)
        {
            if (optionsAction==null)
            {
                throw new Exception("option Action is Null");
            }

            this.serviceConfigurationOption = new ServiceConfigurationOption();

            optionsAction(this.serviceConfigurationOption);

            this.Services.AddSingleton<ServiceConfigurationOption>(serviceConfigurationOption);

            this.Services.AddTransient<IServiceProvider>(service => this.ServiceProvider);

            this.ServiceProvider = this.Services.BuildServiceProvider();

            return this.ServiceProvider.GetRequiredService<HaWebSocketServer>();
        }

        private void RegisterDefaultService()
        {
            this.Services.AddTransient<IMessageTypeMapper>(service => this.MessageTypeMapper);
            this.Services.AddTransient<IInvokeHandlerManager>(service => this.InvokeHandlerManager);

            this.Services.AddSingleton<IConnectionLifetimeManager, ConnectionLifetimeManager>();
            this.Services.AddSingleton<ISimpleProtocol, NewtonsoftJsonSimpleProtocol>();
            this.Services.AddSingleton<IUserIdProvider, DefaultUserIdProvider>();
            this.Services.AddSingleton<HaWebSocketServer, HaWebSocketServer>();
        }

        public void Dispose()
        {

        }
    }
}

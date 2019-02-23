using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions
{
    public class HaWebSocketServer 
    {
        private readonly ServiceConfigurationOption serviceConfigurationOption;
        private readonly WebSocketServer innerWebSocketServer;
        private readonly IConnectionLifetimeManager connectionLifetimeManager;
        private readonly ILoggerFactory loggerFactory;
        private readonly ISimpleProtocol simpleProtocol;
        private readonly IInvokeHandlerManager invokeHandlerManager;
        private readonly ILogger logger;

        private bool isStarted;

        public HaWebSocketServer(ServiceConfigurationOption option,
            IConnectionLifetimeManager connectionLifetimeManager,
            IServiceProvider serviceProvider,
            IInvokeHandlerManager invokeHandlerManager,
            ILoggerFactory loggerFactory)
        {
            this.serviceConfigurationOption = option;
            this.connectionLifetimeManager = connectionLifetimeManager;
            this.innerWebSocketServer = new WebSocketServer(serviceConfigurationOption.Location);
            this.loggerFactory = loggerFactory;
            this.simpleProtocol = serviceProvider.GetRequiredService<ISimpleProtocol>();
            this.invokeHandlerManager = invokeHandlerManager;
            this.logger = loggerFactory.CreateLogger<HaWebSocketServer>();
        }

        public void Start()
        {
            this.logger.LogInformation("服务开始启动......");

            if (isStarted)
                return;
            isStarted = true;

            innerWebSocketServer.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    this.logger.LogInformation("开启链接.....");
                    this.OnOpen(socket).Wait();
                };

                socket.OnClose = () =>
                {
                    this.logger.LogInformation("关闭链接.....");
                    this.OnClose(socket).Wait();
                };

                socket.OnMessage = message =>
                {
                    this.logger.LogInformation("接收消息.....");
                    this.OnMessage(message, socket).Wait();
                };
            });
        }

        private async Task OnOpen(IWebSocketConnection socket)
        {
            var connectionId = socket.ConnectionInfo.Id.ToString();

            var context = new ConnectionContext(connectionId, socket);
            context.Protocol = simpleProtocol;

            await this.connectionLifetimeManager.OnConnectedAsync(context);
        }

        private async Task OnClose(IWebSocketConnection socket)
        {
            var connectionId = socket.ConnectionInfo.Id.ToString();
            var context = await this.connectionLifetimeManager.GetConnectionContext(connectionId);

            await this.connectionLifetimeManager.OnDisconnectedAsync(context);
        }

        private async Task OnMessage(string message, IWebSocketConnection socket)
        {
            var logger = this.loggerFactory.CreateLogger("Socket Received");
            try
            {
                var connectionId = socket.ConnectionInfo.Id.ToString();
                var context = new ConnectionContext(connectionId, socket);
                context.Protocol = simpleProtocol;

                if (this.simpleProtocol.TryParseMessage(message, out Message messageData))
                {
                    await Invoke(messageData, message, context);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        private async Task Invoke(Message message, string messageContent, IConnectionContext context)
        {
            ReturnMessage returnMessage = null;
            try
            {
                var response = await this.InvokeHandler(message, messageContent, context);
                if (response != null && response != Response.VoidResponse)
                {
                    returnMessage = new DataReturnMessage()
                    {
                        op = "op.return",
                        id = message.id,
                        targetOp = message.op,
                        data = response.GetData()
                    };
                }
                else if (response == Response.VoidResponse)
                {
                    returnMessage = new VoidReturnMesage()
                    {
                        op = "op.return",
                        targetOp = message.op,
                        id = message.id
                    };
                }
            }
            catch (ReturnException ex)
            {
                returnMessage = new ErrorReturnMessage()
                {
                    op = "op.return",
                    id = message.id,
                    targetOp = message.op,
                    error = ex.ErrorCode,
                    message = ex.Message
                };
            }
            catch (Exception ex)
            {
                returnMessage = new ErrorReturnMessage()
                {
                    op = "op.return",
                    id = message.id,
                    targetOp = message.op,
                    error = 500,
                    message = "inner system error"
                };
            }

            await context.WriteAsync(CreateSerializedIMessage(returnMessage), CancellationToken.None);
        }

        private SerializedSimpleMessage CreateSerializedIMessage(Message message)
        {
            return new SerializedSimpleMessage(message);
        }

        private async Task<Response> InvokeHandler(Message message, string messageContent, IConnectionContext context)
        {
            IMessageHandler handler;
            if ((handler = invokeHandlerManager.GetMessageHandler(message.op)) != null)
            {
                return await handler.HandleMessage(message, messageContent, context);
            }
            return Response.VoidResponse;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions.Redis
{
    /// <summary>
    /// Extension methods for configuring Redis-based scale-out for a SignalR Server in an <see cref="ISignalRServerBuilder" />.
    /// </summary>
    public static class StackExchangeRedisDependencyInjectionExtensions
    {
        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Redis server.
        /// </summary>
        /// <param name="servicebuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static WebSocketServerBuilder AddStackExchangeRedis(this WebSocketServerBuilder servicebuilder)
        {
            return AddStackExchangeRedis(servicebuilder, o => { });
        }

        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Redis server.
        /// </summary>
        /// <param name="servicebuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <param name="redisConnectionString">The connection string used to connect to the Redis server.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static WebSocketServerBuilder AddStackExchangeRedis(this WebSocketServerBuilder servicebuilder, string redisConnectionString)
        {
            return AddStackExchangeRedis(servicebuilder, o =>
            {
                o.Configuration = ConfigurationOptions.Parse(redisConnectionString);
            });
        }

        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Redis server.
        /// </summary>
        /// <param name="servicebuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <param name="configure">A callback to configure the Redis options.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static WebSocketServerBuilder AddStackExchangeRedis(this WebSocketServerBuilder servicebuilder, Action<RedisOptions> configure)
        {
            servicebuilder.Services.Configure(configure);
            servicebuilder.Services.AddSingleton(typeof(IConnectionLifetimeManager), typeof(RedisConnectionLifetimeManager));
            return servicebuilder;
        }

        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Redis server.
        /// </summary>
        /// <param name="servicebuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <param name="redisConnectionString">The connection string used to connect to the Redis server.</param>
        /// <param name="configure">A callback to configure the Redis options.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static WebSocketServerBuilder AddStackExchangeRedis(this WebSocketServerBuilder servicebuilder, string redisConnectionString, Action<RedisOptions> configure)
        {
            return AddStackExchangeRedis(servicebuilder, o =>
            {
                o.Configuration = ConfigurationOptions.Parse(redisConnectionString);
                configure(o);
            });
        }
    }
}

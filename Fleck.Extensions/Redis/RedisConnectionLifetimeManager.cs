using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions.Redis
{
    public class RedisConnectionLifetimeManager : IConnectionLifetimeManager
    {
        private readonly ConnectionStore _connections = new ConnectionStore();
        private readonly RedisSubscriptionManager _groups = new RedisSubscriptionManager();
        private readonly RedisSubscriptionManager _users = new RedisSubscriptionManager();
        private IConnectionMultiplexer _redisServerConnection;
        private ISubscriber _bus;
        private readonly ILogger _logger;
        private readonly RedisOptions _options;
        private readonly RedisChannels _channels;
        private readonly string _serverName = GenerateServerName();
        private readonly RedisProtocol _protocol;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1);

        private readonly AckHandler _ackHandler;
        private int _internalId;

        public RedisConnectionLifetimeManager(ILogger<RedisConnectionLifetimeManager> logger,
                                       IOptions<RedisOptions> options,
                                       IHubProtocolResolver hubProtocolResolver)
        {
            _logger = logger;
            _options = options.Value;
            _ackHandler = new AckHandler();
            _channels = new RedisChannels("RedisConnectionLifetimeManager");
            _protocol = new RedisProtocol(hubProtocolResolver.AllProtocols);

            RedisLog.ConnectingToEndpoints(_logger, options.Value.Configuration.EndPoints, _serverName);
            _ = EnsureRedisServerConnection();
        }

        public Task<IConnectionContext> GetConnectionContext(string connectionId)
        {
            var connection = _connections[connectionId];
            return Task.FromResult(connection);
        }

        public async Task OnConnectedAsync(IConnectionContext connection)
        {
            await EnsureRedisServerConnection();
            var feature = new RedisFeature();
            connection.Features.Set<IRedisFeature>(feature);

            var connectionTask = Task.CompletedTask;
            var userTask = Task.CompletedTask;

            _connections.Add(connection);

            connectionTask = SubscribeToConnection(connection);

            if (!string.IsNullOrEmpty(connection.UserIdentifier))
            {
                userTask = SubscribeToUser(connection);
            }

            await Task.WhenAll(connectionTask, userTask);
        }

        public Task OnDisconnectedAsync(IConnectionContext connection)
        {
            _connections.Remove(connection);

            var tasks = new List<Task>();

            var connectionChannel = _channels.Connection(connection.ConnectionId);
            RedisLog.Unsubscribe(_logger, connectionChannel);
            tasks.Add(_bus.UnsubscribeAsync(connectionChannel));

            var feature = connection.Features.Get<IRedisFeature>();
            var groupNames = feature.Groups;

            if (groupNames != null)
            {
                // Copy the groups to an array here because they get removed from this collection
                // in RemoveFromGroupAsync
                foreach (var group in groupNames.ToArray())
                {
                    // Use RemoveGroupAsyncCore because the connection is local and we don't want to
                    // accidentally go to other servers with our remove request.
                    tasks.Add(RemoveGroupAsyncCore(connection, group));
                }
            }

            if (!string.IsNullOrEmpty(connection.UserIdentifier))
            {
                tasks.Add(RemoveUserAsync(connection));
            }

            return Task.WhenAll(tasks);
        }

        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException(nameof(connectionId));
            }

            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var connection = _connections[connectionId];
            if (connection != null)
            {
                // short circuit if connection is on this server
                return AddGroupAsyncCore(connection, groupName);
            }

            return SendGroupActionAndWaitForAck(connectionId, groupName, GroupAction.Add);
        }

        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException(nameof(connectionId));
            }

            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var connection = _connections[connectionId];
            if (connection != null)
            {
                // short circuit if connection is on this server
                return RemoveGroupAsyncCore(connection, groupName);
            }

            return SendGroupActionAndWaitForAck(connectionId, groupName, GroupAction.Remove);
        }

        public Task SendGroupAsync(string groupName, Message message, CancellationToken cancellationToken = default)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var payload = _protocol.WriteInvocation(message);
            return PublishAsync(_channels.Group(groupName), payload);
        }

        public Task SendGroupsAsync(IReadOnlyList<string> groupNames, Message message, CancellationToken cancellationToken = default)
        {
            if (groupNames == null)
            {
                throw new ArgumentNullException(nameof(groupNames));
            }
            var publishTasks = new List<Task>(groupNames.Count);
            var payload = _protocol.WriteInvocation(message);

            foreach (var groupName in groupNames)
            {
                if (!string.IsNullOrEmpty(groupName))
                {
                    publishTasks.Add(PublishAsync(_channels.Group(groupName), payload));
                }
            }

            return Task.WhenAll(publishTasks);
        }

        public Task SendGroupExceptAsync(string groupName, Message message, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var payload = _protocol.WriteInvocation(message, excludedConnectionIds);
            return PublishAsync(_channels.Group(groupName), payload);
        }

        public Task SendAllAsync(Message message, CancellationToken cancellationToken = default)
        {
            var payload = _protocol.WriteInvocation(message);
            return PublishAsync(_channels.All, payload);
        }

        public Task SendAllExceptAsync(Message message, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            var payload = _protocol.WriteInvocation(message, excludedConnectionIds);
            return PublishAsync(_channels.All, payload);
        }

        public Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, Message message, CancellationToken cancellationToken = default)
        {
            if (connectionIds == null)
            {
                throw new ArgumentNullException(nameof(connectionIds));
            }

            var publishTasks = new List<Task>(connectionIds.Count);
            var payload = _protocol.WriteInvocation(message);

            foreach (var connectionId in connectionIds)
            {
                publishTasks.Add(PublishAsync(_channels.Connection(connectionId), payload));
            }

            return Task.WhenAll(publishTasks);
        }

        public Task SendUsersAsync(IReadOnlyList<string> userIds, Message message, CancellationToken cancellationToken = default)
        {
            if (userIds.Count > 0)
            {
                var payload = _protocol.WriteInvocation(message);
                var publishTasks = new List<Task>(userIds.Count);
                foreach (var userId in userIds)
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        publishTasks.Add(PublishAsync(_channels.User(userId), payload));
                    }
                }

                return Task.WhenAll(publishTasks);
            }

            return Task.CompletedTask;
        }

        private async Task PublishAsync(string channel, byte[] payload)
        {
            await EnsureRedisServerConnection();
            RedisLog.PublishToChannel(_logger, channel);
            await _bus.PublishAsync(channel, payload);
        }

        private Task AddGroupAsyncCore(IConnectionContext connection, string groupName)
        {
            var feature = connection.Features.Get<IRedisFeature>();
            var groupNames = feature.Groups;

            lock (groupNames)
            {
                // Connection already in group
                if (!groupNames.Add(groupName))
                {
                    return Task.CompletedTask;
                }
            }

            var groupChannel = _channels.Group(groupName);
            return _groups.AddSubscriptionAsync(groupChannel, connection, SubscribeToGroupAsync);
        }

        /// <summary>
        /// This takes <see cref="HubConnectionContext"/> because we want to remove the connection from the
        /// _connections list in OnDisconnectedAsync and still be able to remove groups with this method.
        /// </summary>
        private async Task RemoveGroupAsyncCore(IConnectionContext connection, string groupName)
        {
            var groupChannel = _channels.Group(groupName);

            await _groups.RemoveSubscriptionAsync(groupChannel, connection, channelName =>
            {
                RedisLog.Unsubscribe(_logger, channelName);
                return _bus.UnsubscribeAsync(channelName);
            });

            var feature = connection.Features.Get<IRedisFeature>();
            var groupNames = feature.Groups;
            if (groupNames != null)
            {
                lock (groupNames)
                {
                    groupNames.Remove(groupName);
                }
            }
        }

        private async Task SendGroupActionAndWaitForAck(string connectionId, string groupName, GroupAction action)
        {
            var id = Interlocked.Increment(ref _internalId);
            var ack = _ackHandler.CreateAck(id);
            // Send Add/Remove Group to other servers and wait for an ack or timeout
            var message = _protocol.WriteGroupCommand(new RedisGroupCommand(id, _serverName, action, groupName, connectionId));
            await PublishAsync(_channels.GroupManagement, message);

            await ack;
        }

        private Task RemoveUserAsync(IConnectionContext connection)
        {
            var userChannel = _channels.User(connection.UserIdentifier);

            return _users.RemoveSubscriptionAsync(userChannel, connection, channelName =>
            {
                RedisLog.Unsubscribe(_logger, channelName);
                return _bus.UnsubscribeAsync(channelName);
            });
        }

        public void Dispose()
        {
            _bus?.UnsubscribeAll();
            _redisServerConnection?.Dispose();
            _ackHandler.Dispose();
        }

        private async Task SubscribeToAll()
        {
            RedisLog.Subscribing(_logger, _channels.All);
            var channel = await _bus.SubscribeAsync(_channels.All);
            channel.OnMessage(async channelMessage =>
            {
                try
                {
                    RedisLog.ReceivedFromChannel(_logger, _channels.All);

                    var invocation = _protocol.ReadInvocation((byte[])channelMessage.Message);

                    var tasks = new List<Task>(_connections.Count);

                    foreach (var connection in _connections)
                    {
                        if (invocation.ExcludedConnectionIds == null || !invocation.ExcludedConnectionIds.Contains(connection.ConnectionId))
                        {
                            tasks.Add(connection.WriteAsync(invocation.Message, CancellationToken.None));
                        }
                    }

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    RedisLog.FailedWritingMessage(_logger, ex);
                }
            });
        }

        private async Task SubscribeToGroupManagementChannel()
        {
            var channel = await _bus.SubscribeAsync(_channels.GroupManagement);
            channel.OnMessage(async channelMessage =>
            {
                try
                {
                    var groupMessage = _protocol.ReadGroupCommand((byte[])channelMessage.Message);

                    var connection = _connections[groupMessage.ConnectionId];
                    if (connection == null)
                    {
                        // user not on this server
                        return;
                    }

                    if (groupMessage.Action == GroupAction.Remove)
                    {
                        await RemoveGroupAsyncCore(connection, groupMessage.GroupName);
                    }

                    if (groupMessage.Action == GroupAction.Add)
                    {
                        await AddGroupAsyncCore(connection, groupMessage.GroupName);
                    }

                    // Send an ack to the server that sent the original command.
                    await PublishAsync(_channels.Ack(groupMessage.ServerName), _protocol.WriteAck(groupMessage.Id));
                }
                catch (Exception ex)
                {
                    RedisLog.InternalMessageFailed(_logger, ex);
                }
            });
        }

        private async Task SubscribeToAckChannel()
        {
            // Create server specific channel in order to send an ack to a single server
            var channel = await _bus.SubscribeAsync(_channels.Ack(_serverName));
            channel.OnMessage(channelMessage =>
            {
                var ackId = _protocol.ReadAck((byte[])channelMessage.Message);

                _ackHandler.TriggerAck(ackId);
            });
        }

        private async Task SubscribeToConnection(IConnectionContext connection)
        {
            var connectionChannel = _channels.Connection(connection.ConnectionId);

            RedisLog.Subscribing(_logger, connectionChannel);
            var channel = await _bus.SubscribeAsync(connectionChannel);
            channel.OnMessage(channelMessage =>
            {
                var invocation = _protocol.ReadInvocation((byte[])channelMessage.Message);
                return connection.WriteAsync(invocation.Message,CancellationToken.None);
            });
        }

        private Task SubscribeToUser(IConnectionContext connection)
        {
            var userChannel = _channels.User(connection.UserIdentifier);

            return _users.AddSubscriptionAsync(userChannel, connection, async (channelName, subscriptions) =>
            {
                RedisLog.Subscribing(_logger, channelName);
                var channel = await _bus.SubscribeAsync(channelName);
                channel.OnMessage(async channelMessage =>
                {
                    try
                    {
                        var invocation = _protocol.ReadInvocation((byte[])channelMessage.Message);

                        var tasks = new List<Task>();
                        foreach (var userConnection in subscriptions)
                        {
                            tasks.Add(userConnection.WriteAsync(invocation.Message, CancellationToken.None));
                        }

                        await Task.WhenAll(tasks);
                    }
                    catch (Exception ex)
                    {
                        RedisLog.FailedWritingMessage(_logger, ex);
                    }
                });
            });
        }

        private async Task SubscribeToGroupAsync(string groupChannel, ConnectionStore groupConnections)
        {
            RedisLog.Subscribing(_logger, groupChannel);
            var channel = await _bus.SubscribeAsync(groupChannel);
            channel.OnMessage(async (channelMessage) =>
            {
                try
                {
                    var invocation = _protocol.ReadInvocation((byte[])channelMessage.Message);

                    var tasks = new List<Task>();
                    foreach (var groupConnection in groupConnections)
                    {
                        if (invocation.ExcludedConnectionIds?.Contains(groupConnection.ConnectionId) == true)
                        {
                            continue;
                        }

                        tasks.Add(groupConnection.WriteAsync(invocation.Message, CancellationToken.None));
                    }

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    RedisLog.FailedWritingMessage(_logger, ex);
                }
            });
        }

        private async Task EnsureRedisServerConnection()
        {
            if (_redisServerConnection == null)
            {
                await _connectionLock.WaitAsync();
                try
                {
                    if (_redisServerConnection == null)
                    {
                        var writer = new LoggerTextWriter(_logger);
                        _redisServerConnection = await _options.ConnectAsync(writer);
                        _bus = _redisServerConnection.GetSubscriber();

                        _redisServerConnection.ConnectionRestored += (_, e) =>
                        {
                            // We use the subscription connection type
                            // Ignore messages from the interactive connection (avoids duplicates)
                            if (e.ConnectionType == ConnectionType.Interactive)
                            {
                                return;
                            }

                            RedisLog.ConnectionRestored(_logger);
                        };

                        _redisServerConnection.ConnectionFailed += (_, e) =>
                        {
                            // We use the subscription connection type
                            // Ignore messages from the interactive connection (avoids duplicates)
                            if (e.ConnectionType == ConnectionType.Interactive)
                            {
                                return;
                            }

                            RedisLog.ConnectionFailed(_logger, e.Exception);
                        };

                        if (_redisServerConnection.IsConnected)
                        {
                            RedisLog.Connected(_logger);
                        }
                        else
                        {
                            RedisLog.NotConnected(_logger);
                        }

                        await SubscribeToAll();
                        await SubscribeToGroupManagementChannel();
                        await SubscribeToAckChannel();
                    }
                }
                finally
                {
                    _connectionLock.Release();
                }
            }
        }

        private static string GenerateServerName()
        {
            // Use the machine name for convenient diagnostics, but add a guid to make it unique.
            // Example: MyServerName_02db60e5fab243b890a847fa5c4dcb29
            return $"{Environment.MachineName}_{Guid.NewGuid():N}";
        }

        private class LoggerTextWriter : TextWriter
        {
            private readonly ILogger _logger;

            public LoggerTextWriter(ILogger logger)
            {
                _logger = logger;
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {

            }

            public override void WriteLine(string value)
            {
                RedisLog.ConnectionMultiplexerMessage(_logger, value);
            }
        }

        private interface IRedisFeature
        {
            HashSet<string> Groups { get; }
        }

        private class RedisFeature : IRedisFeature
        {
            public HashSet<string> Groups { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

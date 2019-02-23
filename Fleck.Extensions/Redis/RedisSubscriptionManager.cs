using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fleck.Extensions.Redis
{
    public class RedisSubscriptionManager
    {
        private readonly ConcurrentDictionary<string, ConnectionStore> _subscriptions = new ConcurrentDictionary<string, ConnectionStore>(StringComparer.Ordinal);
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public async Task AddSubscriptionAsync(string id, IConnectionContext connection, Func<string, ConnectionStore, Task> subscribeMethod)
        {
            await _lock.WaitAsync();

            try
            {
                var subscription = _subscriptions.GetOrAdd(id, _ => new ConnectionStore());

                subscription.Add(connection);

                // Subscribe once
                if (subscription.Count == 1)
                {
                    await subscribeMethod(id, subscription);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveSubscriptionAsync(string id, IConnectionContext connection, Func<string, Task> unsubscribeMethod)
        {
            await _lock.WaitAsync();

            try
            {
                if (!_subscriptions.TryGetValue(id, out var subscription))
                {
                    return;
                }

                subscription.Remove(connection);

                if (subscription.Count == 0)
                {
                    _subscriptions.TryRemove(id, out _);
                    await unsubscribeMethod(id);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}

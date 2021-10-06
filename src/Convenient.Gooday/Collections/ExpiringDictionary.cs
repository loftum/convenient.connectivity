using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Convenient.Gooday.Collections
{
    internal class ExpiringDictionary<TKey, TValue>
    {
        public event EventHandler<CacheItemEventArgs<TValue>> ItemAdded;
        public event EventHandler<CacheItemEventArgs<TValue>> ItemRemoved;
        public event EventHandler<CacheItemEventArgs<TValue>> ItemUpdated;
        
        private readonly ConcurrentDictionary<TKey, DateTimeOffset> _expiry = new ConcurrentDictionary<TKey, DateTimeOffset>();
        private readonly ConcurrentDictionary<TKey, TValue> _values = new ConcurrentDictionary<TKey, TValue>();

        private bool _alive = true;
        private Task _updateTask;

        public ExpiringDictionary()
        {
            _updateTask = ExpireKeysAsync();
        }

        ~ExpiringDictionary()
        {
            _alive = false;
        }

        private async Task ExpireKeysAsync()
        {
            while (_alive)
            {
                try
                {
                    var keys = _expiry.ToList();
                    foreach (var key in keys)
                    {
                        if (key.Value > DateTimeOffset.UtcNow)
                        {
                            continue;
                        }
                        
                        if (_values.TryRemove(key.Key, out var value) && _expiry.TryRemove(key.Key, out _))
                        {
                            ItemRemoved?.Invoke(this, new CacheItemEventArgs<TValue>(value));
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ExpiringDictionary: Exception: {e}");
                }
            }
        }
        
        public void SetTtl(TKey key, uint ttl)
        {
            var expiry = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(ttl);
            _expiry.AddOrUpdate(key, expiry, (k, d) => expiry);
        }
        
        public void AddOrUpdate(TKey key, TValue value, uint ttl)
        {
            var added = false;
            _values.AddOrUpdate(key, k =>
                {
                    added = true;
                    return value;
                },
                (k, v) =>
                {
                    added = false;
                    return value;
                }
            );
            var expiry = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(ttl);
            _expiry.AddOrUpdate(key, expiry, (k, d) => expiry);
            
            if (added)
            {
                ItemAdded?.Invoke(this, new CacheItemEventArgs<TValue>(value));
            }
            else // updated
            {
                ItemUpdated?.Invoke(this, new CacheItemEventArgs<TValue>(value));
            }
        }
    }
}
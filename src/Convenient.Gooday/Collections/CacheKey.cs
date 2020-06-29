using System;
using System.Collections.Generic;

namespace Convenient.Gooday.Collections
{
    internal class CacheKey<TKey>
    {
        public TKey Key { get; }
        public DateTimeOffset Expiry { get; private set;  }

        public CacheKey(TKey key, int ttl)
        {
            Key = key;
            SetTtl(ttl);
        }

        public bool IsExpired() => Expiry < DateTimeOffset.UtcNow;

        public void SetTtl(int ttl)
        {
            Expiry = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(ttl);
        }

        public override string ToString()
        {
            return $"{Key} Expiry: {Expiry}, Expired: {IsExpired()}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CacheKey<TKey>);
        }
        
        protected bool Equals(CacheKey<TKey> other)
        {
            return EqualityComparer<TKey>.Default.Equals(Key, other.Key);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TKey>.Default.GetHashCode(Key);
        }
    }
}
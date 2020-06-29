using System;

namespace Convenient.Gooday.Collections
{
    internal class CacheItemEventArgs<TValue> : EventArgs
    {
        public TValue Value { get; }
        
        public CacheItemEventArgs(TValue value)
        {
            Value = value;
        }
    }
}
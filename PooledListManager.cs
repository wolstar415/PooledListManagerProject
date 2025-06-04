using System;
using System.Collections.Generic;

public sealed class PooledListManager<T>
{
    private readonly Stack<PooledList> _pool = new();
    private readonly int _defaultCapacity;
    private readonly int _maxAllowedCapacity;

    public PooledListManager(int defaultCapacity = 32, int maxAllowedCapacity = 512)
    {
        _defaultCapacity = defaultCapacity;
        _maxAllowedCapacity = Math.Max(defaultCapacity, maxAllowedCapacity);
    }

    public PooledList Rent(int minCapacity = -1)
    {
        var pooled = _pool.Count > 0 ? _pool.Pop() : new PooledList(this, _defaultCapacity);
        pooled.Reset(minCapacity);
        return pooled;
    }

    public PooledList Rent(out List<T> list, int minCapacity = -1)
    {
        var pooled = Rent(minCapacity);
        list = pooled.List;
        return pooled;
    }

    private void Return(PooledList pooled)
    {
        if (pooled.List.Capacity > _maxAllowedCapacity)
            pooled.ResetCapacity(_defaultCapacity);

        _pool.Push(pooled);
    }

    public sealed class PooledList : IDisposable
    {
        public List<T> List { get; private set; }

        private bool _disposed;
        private readonly PooledListManager<T> _owner;

        internal PooledList(PooledListManager<T> owner, int capacity)
        {
            _owner = owner;
            List = new List<T>(capacity);
        }

        internal void Reset(int minCapacity)
        {
            _disposed = false;
            List.Clear();
            if (minCapacity > 0 && List.Capacity < minCapacity)
                List.Capacity = minCapacity;
        }

        internal void ResetCapacity(int capacity)
        {
            List = new List<T>(capacity);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _owner.Return(this);
            GC.SuppressFinalize(this);
        }

        ~PooledList()
        {
#if UNITY_ENGINE
            UnityEngine.Debug.LogWarning($"PooledList<{typeof(T).Name}> was not disposed properly.");
#endif
        }

        public override string ToString() => $"PooledList<{typeof(T).Name}>[{List.Count}]";
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}

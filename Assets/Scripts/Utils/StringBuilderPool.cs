using System;
using System.Text;
using UnityEngine.Pool;

/// <summary>
/// Rents reusable StringBuilder instances from a DI-owned object pool.
/// </summary>
public sealed class StringBuilderPool
{
    private const int InitialPoolCapacity = 8;
    private const int MaxPoolSize = 8;
    private const int MaxRetainedCapacity = 512 * 1024;

    private readonly ObjectPool<StringBuilder> _pool;

    public StringBuilderPool()
    {
        _pool = new ObjectPool<StringBuilder>(
            createFunc: () => new StringBuilder(),
            actionOnGet: builder => builder.Clear(),
            actionOnRelease: ReleaseBuilder,
            actionOnDestroy: null,
            collectionCheck: true,
            defaultCapacity: InitialPoolCapacity,
            maxSize: MaxPoolSize);
    }

    /// <summary>
    /// Rents a builder. Dispose the lease when finished; do not keep the builder afterwards.
    /// </summary>
    public StringBuilderLease Rent(int initialCapacity = 0)
    {
        var builder = _pool.Get();
        if (initialCapacity > builder.Capacity)
        {
            builder.EnsureCapacity(initialCapacity);
        }

        return new StringBuilderLease(this, builder);
    }

    internal void Return(StringBuilder builder)
    {
        _pool.Release(builder);
    }

    private static void ReleaseBuilder(StringBuilder builder)
    {
        builder.Clear();
        if (builder.Capacity > MaxRetainedCapacity)
        {
            builder.Capacity = MaxRetainedCapacity;
        }
    }
}

/// <summary>
/// Idempotent disposable lease for a StringBuilder borrowed from StringBuilderPool.
/// </summary>
public sealed class StringBuilderLease : IDisposable
{
    private StringBuilderPool _pool;
    private readonly StringBuilder _builder;

    internal StringBuilderLease(StringBuilderPool pool, StringBuilder builder)
    {
        _pool = pool;
        _builder = builder;
    }

    public StringBuilder Builder => _builder;

    public void Dispose()
    {
        var pool = _pool;
        if (pool == null)
        {
            return;
        }

        _pool = null;
        pool.Return(_builder);
    }
}

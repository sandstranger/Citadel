using System;
using System.Text;
using UnityEngine.Pool;

/// <summary>
/// Rents reusable StringBuilder instances from one process-wide pool.
/// </summary>
public static class StringBuilderPool
{
    private const int InitialPoolCapacity = 8;
    private const int MaxPoolSize = 8;
    private const int MaxRetainedCapacity = 512 * 1024;

    private static readonly ObjectPool<StringBuilder> Pool = new ObjectPool<StringBuilder>(
        createFunc: () => new StringBuilder(),
        actionOnGet: builder => builder.Clear(),
        actionOnRelease: ReleaseBuilder,
        actionOnDestroy: null,
        collectionCheck: true,
        defaultCapacity: InitialPoolCapacity,
        maxSize: MaxPoolSize);

    /// <summary>
    /// Rents a builder. Dispose the lease when finished; do not keep the builder afterwards.
    /// </summary>
    public static StringBuilderLease Rent(int initialCapacity = 0)
    {
        var builder = Pool.Get();
        if (initialCapacity > builder.Capacity)
        {
            builder.EnsureCapacity(initialCapacity);
        }

        return new StringBuilderLease(builder);
    }

    internal static void Return(StringBuilder builder)
    {
        Pool.Release(builder);
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
public readonly struct StringBuilderLease : IDisposable
{
    private readonly StringBuilder _builder;

    internal StringBuilderLease(StringBuilder builder) => _builder = builder;

    public StringBuilder Builder => _builder;

    public void Dispose()
    {
        if (_builder is not null)
        {
            StringBuilderPool.Return(_builder);
        }
    }
}

using System.Buffers;
using System.Runtime.CompilerServices;

namespace Cocoar.Json.Mutable;
internal sealed class PooledBufferWriter : IBufferWriter<byte>, IDisposable
{
    private byte[] _buffer;
    private int _position;
    private const int DefaultInitialSize = 4096;

    public PooledBufferWriter(int initialCapacity = DefaultInitialSize)
    {
        if (initialCapacity <= 0) initialCapacity = DefaultInitialSize;
        _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        _position = 0;
    }

    public int WrittenCount => _position;
    public ReadOnlySpan<byte> WrittenSpan => new ReadOnlySpan<byte>(_buffer, 0, _position);

    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        int newPos = _position + count;
        if ((uint)newPos > (uint)_buffer.Length)
            throw new InvalidOperationException("Advance beyond buffer size");
        _position = newPos;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(_position);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(_position);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int sizeHint)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);
        int required = _position + (sizeHint == 0 ? 1 : sizeHint);
        if (required <= _buffer.Length) return;

        int newSize = _buffer.Length * 2;
        if (newSize < required)
            newSize = required;

        var newBuf = ArrayPool<byte>.Shared.Rent(newSize);
        Buffer.BlockCopy(_buffer, 0, newBuf, 0, _position);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuf;
    }

    public void Dispose()
    {
        if (_buffer is not null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null!;
            _position = 0;
        }
    }
}

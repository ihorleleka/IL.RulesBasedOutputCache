using IL.RulesBasedOutputCache.Helpers;

namespace IL.RulesBasedOutputCache.StreamExt;

internal sealed class SegmentWriteStream : Stream
{
    private readonly List<byte[]> _segments = new();
    private readonly int _segmentSize;
    private byte[] _currentBuffer;
    private int _currentBufferIndex;
    private long _length;
    private bool _closed;
    private bool _disposed;

    internal SegmentWriteStream(int segmentSize)
    {
        if (segmentSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentSize), segmentSize, $"{nameof(segmentSize)} must be greater than 0.");
        }

        _segmentSize = segmentSize;
        _currentBuffer = new byte[_segmentSize];
    }

    // Extracting the buffered segments closes the stream for writing
    internal List<byte[]> GetSegments()
    {
        if (!_closed)
        {
            _closed = true;
            FinalizeSegments();
        }
        return _segments;
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => !_closed;

    public override long Length => _length;

    public override long Position
    {
        get
        {
            return _length;
        }
        set
        {
            throw new NotSupportedException("The stream does not support seeking.");
        }
    }

    private void FinalizeSegments()
    {
        // Append any remaining data in the current buffer
        if (_currentBufferIndex > 0)
        {
            // We need to resize the last buffer to the actual usage if it's not full
            // But usually for segments we want them full except the last one.
            // However, the original implementation did: _bufferStream.ToArray() which returns exact size.
            // So we should return exact size array.
            
            var lastSegment = new byte[_currentBufferIndex];
            Array.Copy(_currentBuffer, lastSegment, _currentBufferIndex);
            _segments.Add(lastSegment);
        }
        
        // Help GC
        _currentBuffer = [];
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _segments.Clear();
                _currentBuffer = [];
            }

            _disposed = true;
            _closed = true;
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    public override void Flush()
    {
        if (!CanWrite)
        {
            throw new ObjectDisposedException("The stream has been closed for writing.");
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("The stream does not support reading.");
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException("The stream does not support seeking.");
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("The stream does not support seeking.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Non-negative number required.");
        }
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Non-negative number required.");
        }
        if (count > buffer.Length - offset)
        {
            throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
        }
        if (!CanWrite)
        {
            throw new ObjectDisposedException("The stream has been closed for writing.");
        }

        Write(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            if (_currentBufferIndex == _segmentSize)
            {
                _segments.Add(_currentBuffer);
                _currentBuffer = new byte[_segmentSize];
                _currentBufferIndex = 0;
            }

            var bytesToWrite = Math.Min(buffer.Length, _segmentSize - _currentBufferIndex);
            buffer.Slice(0, bytesToWrite).CopyTo(_currentBuffer.AsSpan(_currentBufferIndex));
            
            _currentBufferIndex += bytesToWrite;
            buffer = buffer.Slice(bytesToWrite);
            _length += bytesToWrite;
        }
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        Write(buffer, offset, count);
        return Task.CompletedTask;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        Write(buffer.Span);
        return default;
    }

    public override void WriteByte(byte value)
    {
        if (!CanWrite)
        {
            throw new ObjectDisposedException("The stream has been closed for writing.");
        }

        if (_currentBufferIndex == _segmentSize)
        {
            _segments.Add(_currentBuffer);
            _currentBuffer = new byte[_segmentSize];
            _currentBufferIndex = 0;
        }

        _currentBuffer[_currentBufferIndex++] = value;
        _length++;
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => TaskToApm.Begin(WriteAsync(buffer, offset, count, CancellationToken.None), callback, state);

    public override void EndWrite(IAsyncResult asyncResult)
        => TaskToApm.End(asyncResult);
}
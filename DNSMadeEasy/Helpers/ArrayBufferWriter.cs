using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;

namespace DNSMadeEasy
{
	internal sealed class ArrayBufferWriter : IBufferWriter<byte>, IDisposable
	{
		private byte[] _buffer;
		private int _index;

		/// <summary>
		/// Creates an instance of an <see cref="ArrayBufferWriter{T}"/>, in which data can be written to,
		/// with an initial capacity specified.
		/// </summary>
		/// <param name="initialCapacity">The minimum capacity with which to initialize the underlying buffer.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="initialCapacity"/> is not positive (i.e. less than or equal to 0).
		/// </exception>
		public ArrayBufferWriter(int capacity)
		{
			if (capacity <= 0)
				throw new ArgumentException(null, nameof(capacity));

			_buffer = ArrayPool<byte>.Shared.Rent(capacity);
			_index  = 0;
		}

		/// <summary>
		/// Returns the data written to the underlying buffer so far, as a <see cref="ReadOnlyMemory{T}"/>.
		/// </summary>
		public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, _index);

		/// <summary>
		/// Returns the data written to the underlying buffer so far, as a <see cref="ReadOnlySpan{T}"/>.
		/// </summary>
		public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _index);

		/// <summary>
		/// Returns the amount of data written to the underlying buffer so far.
		/// </summary>
		public int WrittenCount => _index;

		/// <summary>
		/// Returns the total amount of space within the underlying buffer.
		/// </summary>
		public int Capacity => _buffer.Length;

		/// <summary>
		/// Returns the amount of space available that can still be written into without forcing the underlying buffer to grow.
		/// </summary>
		public int FreeCapacity => _buffer.Length - _index;

		/// <summary>
		/// Notifies <see cref="IBufferWriter{T}"/> that <paramref name="count"/> amount of data was written to the output <see cref="Span{T}"/>/<see cref="Memory{T}"/>
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="count"/> is negative.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown when attempting to advance past the end of the underlying buffer.
		/// </exception>
		/// <remarks>
		/// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
		/// </remarks>
		public void Advance(int count)
		{
			if (count < 0)
				throw new ArgumentException(null, nameof(count));

			if (_index > _buffer.Length - count)
				throw new InvalidOperationException("Buffer writer advanced too far");

			_index += count;
		}

		/// <summary>
		/// Returns a <see cref="Memory{T}"/> to write to that is at least the requested length (specified by <paramref name="sizeHint"/>).
		/// If no <paramref name="sizeHint"/> is provided (or it's equal to <code>0</code>), some non-empty buffer is returned.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="sizeHint"/> is negative.
		/// </exception>
		/// <remarks>
		/// This will never return an empty <see cref="Memory{T}"/>.
		/// </remarks>
		/// <remarks>
		/// There is no guarantee that successive calls will return the same buffer or the same-sized buffer.
		/// </remarks>
		/// <remarks>
		/// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
		/// </remarks>
		public Memory<byte> GetMemory(int sizeHint = 0)
		{
			CheckBuffer(sizeHint);
			Debug.Assert(_buffer.Length > _index);
			return _buffer.AsMemory(_index);
		}

		/// <summary>
		/// Returns a <see cref="Span{T}"/> to write to that is at least the requested length (specified by <paramref name="sizeHint"/>).
		/// If no <paramref name="sizeHint"/> is provided (or it's equal to <code>0</code>), some non-empty buffer is returned.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="sizeHint"/> is negative.
		/// </exception>
		/// <remarks>
		/// This will never return an empty <see cref="Span{T}"/>.
		/// </remarks>
		/// <remarks>
		/// There is no guarantee that successive calls will return the same buffer or the same-sized buffer.
		/// </remarks>
		/// <remarks>
		/// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
		/// </remarks>
		public Span<byte> GetSpan(int sizeHint = 0)
		{
			CheckBuffer(sizeHint);
			Debug.Assert(_buffer.Length > _index);
			return _buffer.AsSpan(_index);
		}

		private void CheckBuffer(int sizeHint)
		{
			if (sizeHint < 0)
				throw new ArgumentException("sizeHint must be positive", nameof(sizeHint));

			if (sizeHint == 0)
			{
				sizeHint = 1;
			}

			if (sizeHint > FreeCapacity)
				throw new ArgumentOutOfRangeException(nameof(sizeHint) , "The rented array has been completely consumed");
		}

		public void Dispose()
		{
			var buffer = Interlocked.Exchange(ref _buffer, null!);
			if (buffer == null)
				return;

			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public class SendBufferHelper
	{
		// 쓰레드끼리의 경합을 없애기 위해 ThreadLocal로 구현
		public static ThreadLocal<SendBuffer> CurrentBuffer = 
			new ThreadLocal<SendBuffer>(() => { return null; });

		public static int ChunkSize { get; set; } = 4096;

		public static ArraySegment<byte> Open(int _reserveSize)
		{
			if (CurrentBuffer.Value == null)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			if (CurrentBuffer.Value.FreeSize < _reserveSize)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			return CurrentBuffer.Value.Open(_reserveSize);
		}

		public static ArraySegment<byte> Close(int _usedSize)
		{
			return CurrentBuffer.Value.Close(_usedSize);
		}
	}

	public class SendBuffer
	{
		// [][][][][][][][][u][]
		byte[] m_buffer;
		int m_usedSize = 0;

		public int FreeSize { get { return m_buffer.Length - m_usedSize; } }

		public SendBuffer(int _chunkSize)
		{
			m_buffer = new byte[_chunkSize];
		}

		public ArraySegment<byte> Open(int _reserveSize)
		{
			if (_reserveSize > FreeSize)
				return null;

			return new ArraySegment<byte>(m_buffer, m_usedSize, _reserveSize);
		}

		public ArraySegment<byte> Close(int _usedSize)
		{
			ArraySegment<byte> segment = new ArraySegment<byte>(m_buffer, m_usedSize, _usedSize);
			m_usedSize += _usedSize;
			return segment;
		}
	}
}

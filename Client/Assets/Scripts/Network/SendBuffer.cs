using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public class SendBufferHelper
	{
		// 쓰레드끼리의 경합을 없애기 위해 ThreadLocal로 구현
		public static ThreadLocal<SendBuffer> s_currentBuffer =
			new ThreadLocal<SendBuffer>(() => { return null; });

		public static int s_chunkSize { get; set; } = 65535;

		public static ArraySegment<byte> Open(int _reserveSize)
		{
			if (s_currentBuffer.Value == null)
				s_currentBuffer.Value = new SendBuffer(s_chunkSize);

			// C#이므로 그냥 새로 다시 할당
			if (s_currentBuffer.Value.FreeSize < _reserveSize)
				s_currentBuffer.Value = new SendBuffer(s_chunkSize);

			return s_currentBuffer.Value.Open(_reserveSize);
		}

		public static ArraySegment<byte> Close(int _usedSize)
		{
			return s_currentBuffer.Value.Close(_usedSize);
		}
	}

	public class SendBuffer
	{
		// 여러 쓰레드에서 참조할 순 있으나 읽기만 하므로 큰 문제는 없다.
		byte[] m_buffer;
		int m_usedSize = 0;

		public int FreeSize { get { return m_buffer.Length - m_usedSize; } }

		public SendBuffer(int _chunkSize)
		{
			m_buffer = new byte[_chunkSize];
		}

		public ArraySegment<byte> Open(int _reserveSize)
		{
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

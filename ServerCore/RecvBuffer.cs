using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
	public class RecvBuffer
	{
		// [r][][w][][][][][][][]
		ArraySegment<byte> m_buffer;
		int m_readPos;
		int m_writePos;

		public RecvBuffer(int _bufferSize)
		{
			m_buffer = new ArraySegment<byte>(new byte[_bufferSize], 0, _bufferSize);
		}

		public int DataSize { get { return m_writePos - m_readPos; } }
		public int FreeSize { get { return m_buffer.Count - m_writePos; } }

		public ArraySegment<byte> ReadSegment
		{
			get { return new ArraySegment<byte>(m_buffer.Array, 
				m_buffer.Offset + m_readPos, DataSize); }
		}

		public ArraySegment<byte> WriteSegment
		{
			get { return new ArraySegment<byte>(m_buffer.Array, 
				m_buffer.Offset + m_writePos, FreeSize); }
		}

		public void Clean()
		{
			int dataSize = DataSize;
			if (dataSize == 0)
			{
				// 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
				m_readPos = m_writePos = 0;
			}
			else
			{
				// 남은 데이터가 있으면 시작 위치로 복사
				Array.Copy(m_buffer.Array, m_buffer.Offset + m_readPos, 
					m_buffer.Array, m_buffer.Offset, dataSize);
				m_readPos = 0;
				m_writePos = dataSize;
			}
		}

		public bool OnRead(int _numOfBytes)
		{
			if (_numOfBytes > DataSize)
				return false;

			m_readPos += _numOfBytes;
			return true;
		}

		public bool OnWrite(int _numOfBytes)
		{
			if (_numOfBytes > FreeSize)
				return false;

			m_writePos += _numOfBytes;
			return true;
		}
	}
}

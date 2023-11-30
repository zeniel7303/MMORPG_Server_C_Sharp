using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public abstract class PacketSession : Session
	{
		public static readonly int s_headerSize = 2;

		// [size(2)][id(2)][ ... ][size(2)][id(2)][ ... ]
		public sealed override int OnRecv(ArraySegment<byte> _buffer)
		{
			int processLen = 0;
			int packetCount = 0;

			while (true)
			{
				// 최소한 헤더를 파싱할 수 있는지 확인
				if (_buffer.Count < s_headerSize)
					break;

				// 패킷이 완전체로 도착했는지 확인
				ushort dataSize = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
				if (_buffer.Count < dataSize)
					break;

				// 여기까지 왔으면 패킷 처리 가능
				// ArraySegment는 class가 아니라 struct이므로 스택 영역에 복사되므로 부담없이 해도된다.
				OnRecvPacket(new ArraySegment<byte>(_buffer.Array, _buffer.Offset, dataSize));
				packetCount++;

				processLen += dataSize;
				_buffer = new ArraySegment<byte>(_buffer.Array, _buffer.Offset + dataSize, _buffer.Count - dataSize);
			}

			if (packetCount > 1)
				Console.WriteLine($"패킷 모아보내기 : {packetCount}");

			return processLen;
		}

		public abstract void OnRecvPacket(ArraySegment<byte> _buffer);
	}

	public abstract class Session
	{
		Socket m_socket;
		int m_disconnected = 0;

		// 64kb
		RecvBuffer m_recvBuffer = new RecvBuffer(65535);

		object m_lock = new object();
		Queue<ArraySegment<byte>> m_sendQueue = new Queue<ArraySegment<byte>>();
		List<ArraySegment<byte>> m_pendingList = new List<ArraySegment<byte>>();
		SocketAsyncEventArgs m_sendArgs = new SocketAsyncEventArgs();
		SocketAsyncEventArgs m_recvArgs = new SocketAsyncEventArgs();

		public abstract void OnConnected(EndPoint _endPoint);
		public abstract int OnRecv(ArraySegment<byte> _buffer);
		public abstract void OnSend(int _numOfBytes);
		public abstract void OnDisconnected(EndPoint _endPoint);

		void Clear()
		{
			lock (m_lock)
			{
				m_sendQueue.Clear();
				m_pendingList.Clear();
			}
		}

		public void Start(Socket _socket)
		{
			m_socket = _socket;

			m_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
			m_sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

			RegisterRecv();
		}

		public void Disconnect()
		{
			if (Interlocked.Exchange(ref m_disconnected, 1) == 1)
				return;

			OnDisconnected(m_socket.RemoteEndPoint);
			m_socket.Shutdown(SocketShutdown.Both);
			m_socket.Close();
			Clear();
		}

		public void Send(ArraySegment<byte> _sendBuffer)
        {
			lock (m_lock)
            {
				m_sendQueue.Enqueue(_sendBuffer);
				if (m_pendingList.Count == 0)       //if (m_pending == false)
					RegisterSend();
			}
		}

		public void Send(List<ArraySegment<byte>> _sendBufferList)
		{
			if (_sendBufferList.Count == 0)
				return;

			lock (m_lock)
			{
				foreach(ArraySegment<byte> sendBuffer in _sendBufferList)
					m_sendQueue.Enqueue(sendBuffer);

				if (m_pendingList.Count == 0)       //if (m_pending == false)
					RegisterSend();
			}
		}

		#region 네트워크 통신

		void RegisterSend()
		{
			if (m_disconnected == 1)
				return;

			while (m_sendQueue.Count > 0)
            {
				ArraySegment<byte> buff = m_sendQueue.Dequeue();
				m_pendingList.Add(buff);
			}
			m_sendArgs.BufferList = m_pendingList;

            try
            {
                bool pending = m_socket.SendAsync(m_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, m_sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }

		void OnSendCompleted(object _sender, SocketAsyncEventArgs _args)
		{
			// 이 곳의 호출 시점을 알 수도 없고(Send 및 EventHandler부분)
			// 멀티쓰레드 환경도 생각해야하므로 Lock 걸어야함
			lock (m_lock)
            {
                if (_args.BytesTransferred > 0 && _args.SocketError == SocketError.Success)
                {
                    try
                    {
						// 전부 성공적으로 보냈으니 초기화
						m_sendArgs.BufferList = null;	// 사실 크게 할 필요는 없다.
						m_pendingList.Clear();

						OnSend(m_sendArgs.BytesTransferred);

						if (m_sendQueue.Count > 0)
							RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

		void RegisterRecv()
		{
			if (m_disconnected == 1)
				return;

			m_recvBuffer.Clean();
			// 다음으로 버퍼를 받을 공간 할당
			ArraySegment<byte> segment = m_recvBuffer.WriteSegment;
			m_recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

			try
			{
				bool pending = m_socket.ReceiveAsync(m_recvArgs);
				if (pending == false)
					OnRecvCompleted(null, m_recvArgs);
			}
			catch (Exception e)
			{
				Console.WriteLine($"RegisterRecv Failed {e}");
			}
		}

		void OnRecvCompleted(object sender, SocketAsyncEventArgs _args)
		{
			if (_args.BytesTransferred > 0 && _args.SocketError == SocketError.Success)
			{
				try
				{
					// Write 커서 이동
					if (m_recvBuffer.OnWrite(_args.BytesTransferred) == false)
					{
						Disconnect();
						return;
					}

					// 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
					int processLen = OnRecv(m_recvBuffer.ReadSegment);
					// 처리한 길이가 0보다 작거나 recvBuffer의 데이터사이즈보다 많다면 문제가 있는 상황
					if (processLen < 0 || m_recvBuffer.DataSize < processLen)
					{
						Disconnect();
						return;
					}

					// Read 커서 이동
					if (m_recvBuffer.OnRead(processLen) == false)
					{
						Disconnect();
						return;
					}

					RegisterRecv();
				}
				catch (Exception e)
				{
					Console.WriteLine($"OnRecvCompleted Failed {e}");
				}
			}
			else
			{
				Disconnect();
			}
		}

		#endregion
	}
}

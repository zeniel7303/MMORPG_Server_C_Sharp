using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
	/*
	public abstract class PacketSession : Session
	{
		public static readonly int HeaderSize = 2;

		// [size(2)][packetId(2)][ ... ][size(2)][packetId(2)][ ... ]
		public sealed override int OnRecv(ArraySegment<byte> _buffer)
		{
			int processLen = 0;
			int packetCount = 0;

			while (true)
			{
				// 최소한 헤더는 파싱할 수 있는지 확인
				if (_buffer.Count < HeaderSize)
					break;

				// 패킷이 완전체로 도착했는지 확인
				ushort dataSize = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
				if (_buffer.Count < dataSize)
					break;

				// 여기까지 왔으면 패킷 조립 가능
				OnRecvPacket(new ArraySegment<byte>(_buffer.Array, _buffer.Offset, dataSize));
				packetCount++;

				processLen += dataSize;
				_buffer = new ArraySegment<byte>(_buffer.Array, _buffer.Offset + dataSize, _buffer.Count - dataSize);
			}

			if (packetCount > 1)
				Console.WriteLine($"패킷 모아보내기 : {packetCount}");

			return processLen;
		}

		public abstract void OnRecvPacket(ArraySegment<byte> buffer);
	}
	*/

	public class Session
	//public abstract class Session
	{
		Socket m_socket;
		int m_disconnected = 0;

		//RecvBuffer m_recvBuffer = new RecvBuffer(65535);

		object m_lock = new object();
        bool m_pending = false;
		Queue<byte[]> m_sendQueue = new Queue<byte[]>();
		//Queue<ArraySegment<byte>> m_sendQueue = new Queue<ArraySegment<byte>>();
		//List<ArraySegment<byte>> m_pendingList = new List<ArraySegment<byte>>();
		SocketAsyncEventArgs m_sendArgs = new SocketAsyncEventArgs();
		SocketAsyncEventArgs m_recvArgs = new SocketAsyncEventArgs();

		//public abstract void OnConnected(EndPoint _endPoint);
		//public abstract int OnRecv(ArraySegment<byte> _buffer);
		//public abstract void OnSend(int _numOfBytes);
		//public abstract void OnDisconnected(EndPoint _endPoint);

		void Clear()
		{
			lock (m_lock)
			{
				//m_sendQueue.Clear();
				//m_pendingList.Clear();
			}
		}

		public void Start(Socket _socket)
		{
			m_socket = _socket;

			m_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
			m_sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

			m_recvArgs.SetBuffer(new byte[1024], 0, 1024);
			RegisterRecv();
		}

		public void Disconnect()
		{
			if (Interlocked.Exchange(ref m_disconnected, 1) == 1)
				return;

			//OnDisconnected(m_socket.RemoteEndPoint);
			m_socket.Shutdown(SocketShutdown.Both);
			m_socket.Close();
			Clear();
		}

		public void Send(byte[] _sendBuffer)
        {
			lock (m_lock)
            {
				m_sendQueue.Enqueue(_sendBuffer);
				if (m_pending == false)
					RegisterSend();
			}
		}

		public void Send(List<ArraySegment<byte>> _sendBuffList)
		{
			//if (_sendBuffList.Count == 0)
			//	return;

			//lock (m_lock)
			//{
			//	foreach (ArraySegment<byte> sendBuff in _sendBuffList)
			//		m_sendQueue.Enqueue(sendBuff);

			//	if (m_pendingList.Count == 0)
			//		RegisterSend();
			//}
		}

		#region 네트워크 통신

		void RegisterSend()
		{
			if (m_disconnected == 1)
				return;

			m_pending = true;
			byte[] buff = m_sendQueue.Dequeue();
			m_sendArgs.SetBuffer(buff, 0, buff.Length);

			bool pending = m_socket.ReceiveAsync(m_sendArgs);
			if (pending == false)
				OnSendCompleted(null, m_sendArgs);

			//while (m_sendQueue.Count > 0)
			//{
			//	ArraySegment<byte> buff = m_sendQueue.Dequeue();
			//	m_pendingList.Add(buff);
			//}
			//m_sendArgs.BufferList = m_pendingList;

			//try
			//{
			//	bool pending = m_socket.SendAsync(m_sendArgs);
			//	if (pending == false)
			//		OnSendCompleted(null, m_sendArgs);
			//}
			//catch (Exception e)
			//{
			//	Console.WriteLine($"RegisterSend Failed {e}");
			//}
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
						//m_sendArgs.BufferList = null;
						//m_pendingList.Clear();

						//OnSend(m_sendArgs.BytesTransferred);

						if (m_sendQueue.Count > 0)
							RegisterSend();
						else
							m_pending = false;
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

			bool pending = m_socket.ReceiveAsync(m_recvArgs);
			if (pending == false)
				OnRecvCompleted(null, m_recvArgs);

			/*
			m_recvBuffer.Clean();
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
			*/
		}

		void OnRecvCompleted(object sender, SocketAsyncEventArgs _args)
		{
			if (_args.BytesTransferred > 0 && _args.SocketError == SocketError.Success)
			{
				try
				{
					/*
					// Write 커서 이동
					if (m_recvBuffer.OnWrite(_args.BytesTransferred) == false)
					{
						Disconnect();
						return;
					}

					// 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
					int processLen = OnRecv(m_recvBuffer.ReadSegment);
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
					*/
					string recvData = Encoding.UTF8.GetString(
						_args.Buffer, _args.Offset, _args.BytesTransferred);
					Console.WriteLine($"[From Client] {recvData}");

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

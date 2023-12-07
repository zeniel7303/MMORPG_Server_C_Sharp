using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

namespace DummyClient
{
	class ServerSession : PacketSession
	{
		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");			
		}

		public override void OnDisconnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnDisconnected : {_endPoint}");
		}

		public override void OnRecvPacket(ArraySegment<byte> _buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, _buffer, (s, p) => PacketQueue.Instance.Push(p));
		}

		public override void OnSend(int _numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {_numOfBytes}");
		}
	}
}

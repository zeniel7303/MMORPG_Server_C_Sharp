using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using ServerCore;

namespace DummyClient
{
	class ServerSession : PacketSession
	{
		/*static unsafe void ToBytes(byte[] _array, int _offset, ulong _value)
		{
			fixed (byte* ptr = &_array[_offset])
				*(ulong*)ptr = _value;
		}*/

		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");
		}
		public override void OnRecvPacket(ArraySegment<byte> _buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, _buffer);
		}

		public override void OnSend(int _numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes : {_numOfBytes}");
		}

		public override void OnDisconnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnDisconnected : {_endPoint}");
		}
	}
}

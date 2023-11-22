using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
	class ClientSession : PacketSession
	{
		public int SessionId { get; set; }
		public GameRoom Room { get; set; }

		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");

			try
			{
				//Packet packet = new Packet() { size = 4, id = 10 };

				//ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
				//byte[] buffer1 = BitConverter.GetBytes(packet.size);
				//byte[] buffer2 = BitConverter.GetBytes(packet.id);
				//Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
				//Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);
				//ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

				//Send(sendBuff);
				Thread.Sleep(5000);
				Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public override void OnRecvPacket(ArraySegment<byte> _buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, _buffer);
		}

		public override void OnSend(int _numOfBytes)
		{
			Console.WriteLine($"Transferred bytes : {_numOfBytes}");
		}

		public override void OnDisconnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnDisconnected : {_endPoint}");
		}
	}
}

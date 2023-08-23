using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Packet
    {
		public ushort size;
		public ushort id;
	}

	class GameSession : PacketSession
	{
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
			ushort size = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
			ushort id = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset + 2);
			Console.WriteLine($"RecvPacket Id : {id}, Size : {size}");
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

	class Program
	{
		static Listener m_listener = new Listener();

		static void Main(string[] args)
		{
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			m_listener.Init(endPoint, () => { return new GameSession(); });
			Console.WriteLine("Listening...");

			while (true)
			{

			}
		}
	}
}

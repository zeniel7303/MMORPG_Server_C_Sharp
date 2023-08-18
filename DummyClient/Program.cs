using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
	class GameSession : Session
	{
		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");

			try
			{
				for (int i = 0; i < 5; i++)
				{
					byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World! {i} ");
					Send(sendBuff);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
		public override void OnRecv(ArraySegment<byte> _buffer)
		{
			string recvData = Encoding.UTF8.GetString(
						_buffer.Array, _buffer.Offset, _buffer.Count);
			Console.WriteLine($"[From Server] {recvData}");
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
		static void Main(string[] args)
		{
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			Connector connector = new Connector();

			connector.Connect(endPoint, () => { return new GameSession(); });

			while (true)
            {
				try
				{
					
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}

				Thread.Sleep(100);
			}
		}
	}
}

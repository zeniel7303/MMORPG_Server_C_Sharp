using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
	class GameSession : Session
	{
		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");

			try
			{
				byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome");
				Send(sendBuff);

				Thread.Sleep(1000);

				Disconnect();
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
			Console.WriteLine($"[From Client] {recvData}");
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

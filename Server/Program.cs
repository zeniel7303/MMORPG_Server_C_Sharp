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
    class Player
    {
		public int hp;
		public int attack;
		public string name;
		public List<int> skillList = new List<int>();
	}

	class GameSession : Session
	{
		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");

			Player player = new Player() { hp = 100, attack = 10 };

			try
			{
				ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
				byte[] buffer1 = BitConverter.GetBytes(player.hp);
				byte[] buffer2 = BitConverter.GetBytes(player.attack);
				Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
				Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);
				ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer1.Length + buffer2.Length);

				Send(sendBuff);
				Thread.Sleep(1000);
				Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
		public override int OnRecv(ArraySegment<byte> _buffer)
		{
			string recvData = Encoding.UTF8.GetString(
						_buffer.Array, _buffer.Offset, _buffer.Count);
			Console.WriteLine($"[From Client] {recvData}");

			return _buffer.Count;
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

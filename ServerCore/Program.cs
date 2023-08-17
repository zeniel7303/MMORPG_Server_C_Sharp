using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
	class Program
	{
		static Listener m_listener = new Listener();
		static void OnAcceptHandler(Socket _clientSocket)
        {
			try
			{
				Session session = new Session();
				session.Start(_clientSocket);

				byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome");
				session.Send(sendBuff);

				Thread.Sleep(1000);

				session.Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		static void Main(string[] args)
		{
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			m_listener.Init(endPoint, OnAcceptHandler);
			Console.WriteLine("Listening...");

			while (true)
			{

			}
		}
	}
}

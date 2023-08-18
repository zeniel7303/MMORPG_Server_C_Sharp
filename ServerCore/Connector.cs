using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Connector
	{
		Func<Session> m_sessionFactory;

		public void Connect(IPEndPoint _endPoint, Func<Session> _sessionFactory, int _count = 1)
		{
			for (int i = 0; i < _count; i++)
			{
				// 휴대폰 설정
				Socket socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				m_sessionFactory = _sessionFactory;

				SocketAsyncEventArgs args = new SocketAsyncEventArgs();
				args.Completed += OnConnectCompleted;
				args.RemoteEndPoint = _endPoint;
				args.UserToken = socket;

				RegisterConnect(args);
			}
		}

		void RegisterConnect(SocketAsyncEventArgs _args)
		{
			Socket socket = _args.UserToken as Socket;
			if (socket == null)
				return;

			bool pending = socket.ConnectAsync(_args);
			if (pending == false)
				OnConnectCompleted(null, _args);
		}

		void OnConnectCompleted(object _sender, SocketAsyncEventArgs _args)
		{
			if (_args.SocketError == SocketError.Success)
			{
				Session session = m_sessionFactory.Invoke();
				session.Start(_args.ConnectSocket);
				session.OnConnected(_args.RemoteEndPoint);
			}
			else
			{
				Console.WriteLine($"OnConnectCompleted Fail: {_args.SocketError}");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Listener
	{
		Socket m_listenSocket;
		Func<Session> m_sessionFactory;

		public void Init(IPEndPoint _endPoint, Func<Session> _sessionFactory,
			int _register = 10, int _backLog = 100)
		{
			m_listenSocket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			m_sessionFactory += _sessionFactory;

			m_listenSocket.Bind(_endPoint);

			// backlog : 최대 대기수
			m_listenSocket.Listen(_backLog);

            for (int i = 0; i < _register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }
        }

		// 비동기
		void RegisterAccept(SocketAsyncEventArgs _args)
		{
			_args.AcceptSocket = null;

			bool pending = m_listenSocket.AcceptAsync(_args);
			if (pending == false)
				OnAcceptCompleted(null, _args);
		}

		// 멀티쓰레드로 실행될 수 있는 부분
		void OnAcceptCompleted(object sender, SocketAsyncEventArgs _args)
		{
			if (_args.SocketError == SocketError.Success)
			{
				Session session = m_sessionFactory.Invoke();
				session.Start(_args.AcceptSocket);
				session.OnConnected(_args.AcceptSocket.RemoteEndPoint);
			}
			else
				Console.WriteLine(_args.SocketError.ToString());

			RegisterAccept(_args);
		}
	}
}
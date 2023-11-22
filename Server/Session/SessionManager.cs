using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class SessionManager
    {
        static SessionManager m_sessionManager = new SessionManager();
        public static SessionManager Instance { get { return m_sessionManager; } }

		int m_sessionId = 0;
		Dictionary<int, ClientSession> m_sessions = new Dictionary<int, ClientSession>();
		object m_lock = new object();

		public ClientSession Generate()
		{
			lock (m_lock)
			{
				int sessionId = ++m_sessionId;

				ClientSession session = new ClientSession();
				session.SessionId = sessionId;
				m_sessions.Add(sessionId, session);

				Console.WriteLine($"Connected : {sessionId}");

				return session;
			}
		}

		public ClientSession Find(int id)
		{
			lock (m_lock)
			{
				ClientSession session = null;
				m_sessions.TryGetValue(id, out session);
				return session;
			}
		}

		public void Remove(ClientSession session)
		{
			lock (m_lock)
			{
				m_sessions.Remove(session.SessionId);
			}
		}
	}
}

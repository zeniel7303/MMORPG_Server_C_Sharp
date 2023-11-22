using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> m_sessions = new List<ClientSession>();
		object m_lock = new object();

		public void Enter(ClientSession session)
		{
			lock(m_lock)
            {
				m_sessions.Add(session);
				session.Room = this;
			}
		}

		public void Leave(ClientSession session)
		{
			lock (m_lock)
			{
				m_sessions.Remove(session);
			}
		}
	}
}
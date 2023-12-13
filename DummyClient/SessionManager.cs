using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    class SessionManager
    {
        static SessionManager m_sessionManager = new SessionManager();
        public static SessionManager Instance { get { return m_sessionManager; } }

        List<ServerSession> m_sessions = new List<ServerSession>();
        object m_lock = new object();
        Random m_random = new Random();

        public ServerSession Generate()
        {
            lock(m_lock)
            {
                ServerSession session = new ServerSession();
                m_sessions.Add(session);
                return session;
            }
        }

        public void SendForEach()
        {
            lock(m_lock)
            {
                foreach (ServerSession session in m_sessions)
                {
                    C_Move movePacket = new C_Move();
                    movePacket.posX = m_random.Next(-50, 50);
                    movePacket.posY = 0;
                    movePacket.posZ = m_random.Next(-50, 50);
                    session.Send(movePacket.Write());
                }
            }
        }
    }
}

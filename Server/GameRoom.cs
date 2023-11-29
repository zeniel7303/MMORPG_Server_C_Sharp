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

		public void Broadcast(ClientSession _clientSession, string _chat)
        {
			S_Chat packet = new S_Chat();
			packet.playerId = _clientSession.SessionId;
			packet.chat = $"{_chat} I am {packet.playerId} \n";
			ArraySegment<byte> segment = packet.Write();

			lock(m_lock)
            {
				foreach (ClientSession session in m_sessions)
					session.Send(segment);
            }
		}
    }
}
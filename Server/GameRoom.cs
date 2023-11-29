using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> m_sessions = new List<ClientSession>();
		JobQueue m_jobQueue = new JobQueue();

		public void Push(Action _job)
        {
			m_jobQueue.Push(_job);
        }

		public void Enter(ClientSession session)
		{
			m_sessions.Add(session);
			session.Room = this;
		}

		public void Leave(ClientSession session)
		{
			m_sessions.Remove(session);
		}

		public void Broadcast(ClientSession _clientSession, string _chat)
        {
			S_Chat packet = new S_Chat();
			packet.playerId = _clientSession.SessionId;
			packet.chat = $"{_chat} I am {packet.playerId} \n";
			ArraySegment<byte> segment = packet.Write();

			foreach (ClientSession session in m_sessions)
				session.Send(segment);
		}
    }
}
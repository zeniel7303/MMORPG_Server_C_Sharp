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
		List<ArraySegment<byte>> m_pendingList = new List<ArraySegment<byte>>();

		public void Push(Action _job)
        {
			m_jobQueue.Push(_job);
        }

		public void Flush()
        {
			foreach (ClientSession session in m_sessions)
				session.Send(m_pendingList);

			Console.WriteLine($"Flushed {m_pendingList.Count} items");
			m_pendingList.Clear();
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

			m_pendingList.Add(segment);

			// n ^ 2
			// -> 모아뒀다가 보내는 방식으로 구현을 바꾸면 n으로 바꿀 수 있을 것이다.
			//foreach (ClientSession session in m_sessions)
			//	session.Send(segment);
		}
    }
}
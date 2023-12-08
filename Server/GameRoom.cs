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

			//Console.WriteLine($"Flushed {m_pendingList.Count} items");
			m_pendingList.Clear();
		}

		public void Broadcast(ArraySegment<byte> _segment)
		{
			m_pendingList.Add(_segment);
		}

		public void Enter(ClientSession _session)
		{
			m_sessions.Add(_session);
			_session.Room = this;

			// 신규 유저한테 모든 플레이어 목록 전송
			S_PlayerList players = new S_PlayerList();
			foreach (ClientSession s in m_sessions)
			{
				players.players.Add(new S_PlayerList.Player()
				{
					isSelf = (s == _session),
					playerId = s.SessionId,
					posX = s.PosX,
					posY = s.PosY,
					posZ = s.PosZ,
				});
			}
			_session.Send(players.Write());

			// 신규 유저 입장을 모두에게 알린다
			S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
			enter.playerId = _session.SessionId;
			enter.posX = 0;
			enter.posY = 0;
			enter.posZ = 0;
			Broadcast(enter.Write());
		}

		public void Leave(ClientSession _session)
		{
			m_sessions.Remove(_session);

			// 모두에게 전송
			S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
			leave.playerId = _session.SessionId;
			Broadcast(leave.Write());
		}

		public void Move(ClientSession _session, C_Move _packet)
		{
			// 좌표 변경
			_session.PosX = _packet.posX;
			_session.PosY = _packet.posY;
			_session.PosZ = _packet.posZ;

			// 모두에게 알린다
			S_BroadcastMove move = new S_BroadcastMove();
			move.playerId = _session.SessionId;
			move.posX = _session.PosX;
			move.posY = _session.PosY;
			move.posZ = _session.PosZ;
			Broadcast(move.Write());
		}
	}
}
using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class PacketHandler
{
	public static void S_BroadcastEnterGameHandler(PacketSession _session, IPacket _packet)
	{
		S_BroadcastEnterGame pkt = _packet as S_BroadcastEnterGame;
		ServerSession serverSession = _session as ServerSession;

		PlayerManager.Instance.EnterGame(pkt);
	}

	public static void S_BroadcastLeaveGameHandler(PacketSession _session, IPacket _packet)
	{
		S_BroadcastLeaveGame pkt = _packet as S_BroadcastLeaveGame;
		ServerSession serverSession = _session as ServerSession;

		PlayerManager.Instance.LeaveGame(pkt);
	}

	public static void S_PlayerListHandler(PacketSession _session, IPacket _packet)
	{
		S_PlayerList pkt = _packet as S_PlayerList;
		ServerSession serverSession = _session as ServerSession;

		PlayerManager.Instance.Add(pkt);
	}

	public static void S_BroadcastMoveHandler(PacketSession _session, IPacket _packet)
	{
		S_BroadcastMove pkt = _packet as S_BroadcastMove;
		ServerSession serverSession = _session as ServerSession;

		PlayerManager.Instance.Move(pkt);
	}
}

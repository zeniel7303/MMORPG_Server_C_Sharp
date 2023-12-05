using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class PacketHandler
{
	public static void S_ChatHandler(PacketSession _session, IPacket _packet)
	{
		S_Chat chatPacket = _packet as S_Chat;
		ServerSession serverSession = _session as ServerSession;

		if (chatPacket.playerId == 1)
        {
			Debug.Log(chatPacket.chat);
		}
	}
}

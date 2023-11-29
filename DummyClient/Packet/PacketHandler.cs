using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void S_ChatHandler(PacketSession _session, IPacket _packet)
	{
		S_Chat chatPacket = _packet as S_Chat;
		ServerSession serverSession = _session as ServerSession;

		Console.Write(chatPacket.chat);
	}
}

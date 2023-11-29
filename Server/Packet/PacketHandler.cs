using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_ChatHandler(PacketSession _session, IPacket _packet)
	{
		C_Chat chatPacket = _packet as C_Chat;
		ClientSession clientSession = _session as ClientSession;

		if (clientSession.Room == null)
			return;

		GameRoom room = clientSession.Room;
		room.Broadcast(clientSession, chatPacket.chat);
	}
}

using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_LeaveGameHandler(PacketSession _session, IPacket _packet)
    {
		ClientSession clientSession = _session as ClientSession;

		if (clientSession.Room == null)
			return;

		GameRoom room = clientSession.Room;
		room.Push(() => room.Leave(clientSession));
	}

	public static void C_MoveHandler(PacketSession _session, IPacket _packet)
    {
		C_Move movePacket = _packet as C_Move;
		ClientSession clientSession = _session as ClientSession;

		if (clientSession.Room == null)
			return;

		Console.WriteLine($"{movePacket.posX}, {movePacket.posY}, {movePacket.posZ}");

		GameRoom room = clientSession.Room;
		room.Push(() => room.Move(clientSession, movePacket));
	}
}

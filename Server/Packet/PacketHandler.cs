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

		// clientSession의 Room이 NULL로 바뀌면 나중에 잡큐에서 꺼내쓸 때 문제가 생김.
		// -> 임시 객체를 만들어 문제 해결
		GameRoom tempRoom = clientSession.Room;
		tempRoom.Push(
			() => tempRoom.Broadcast(clientSession, chatPacket.chat));
	}
}

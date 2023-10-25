using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void PlayerInfoReqHandler(PacketSession _session, IPacket _packet)
	{
		PlayerInfoReq packet = _packet as PlayerInfoReq;

		Console.WriteLine($"PlayerInfoReq: {packet.playerId} {packet.name}");

		foreach (PlayerInfoReq.Skill skill in packet.skills)
		{
			Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
		}
	}

	public static void TestHandler(PacketSession _session, IPacket _packet)
	{

	}
}

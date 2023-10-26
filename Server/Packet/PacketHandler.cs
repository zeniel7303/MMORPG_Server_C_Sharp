using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_PlayerInfoReqHandler(PacketSession _session, IPacket _packet)
	{
		C_PlayerInfoReq packet = _packet as C_PlayerInfoReq;

		Console.WriteLine($"PlayerInfoReq: {packet.playerId} {packet.name}");

		foreach (C_PlayerInfoReq.Skill skill in packet.skills)
		{
			Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
		}
	}
}

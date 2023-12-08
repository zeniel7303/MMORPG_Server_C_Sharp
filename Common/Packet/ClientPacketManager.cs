using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{
	#region Singleton
	static PacketManager m_instance = new PacketManager();
	public static PacketManager Instance { get { return m_instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> m_makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
	Dictionary<ushort, Action<PacketSession, IPacket>> m_handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
		
	public void Register()
	{
		m_makeFunc.Add((ushort)PacketID.S_BroadcastEnterGame, MakePacket<S_BroadcastEnterGame>);
		m_handler.Add((ushort)PacketID.S_BroadcastEnterGame, PacketHandler.S_BroadcastEnterGameHandler);
		m_makeFunc.Add((ushort)PacketID.S_BroadcastLeaveGame, MakePacket<S_BroadcastLeaveGame>);
		m_handler.Add((ushort)PacketID.S_BroadcastLeaveGame, PacketHandler.S_BroadcastLeaveGameHandler);
		m_makeFunc.Add((ushort)PacketID.S_PlayerList, MakePacket<S_PlayerList>);
		m_handler.Add((ushort)PacketID.S_PlayerList, PacketHandler.S_PlayerListHandler);
		m_makeFunc.Add((ushort)PacketID.S_BroadcastMove, MakePacket<S_BroadcastMove>);
		m_handler.Add((ushort)PacketID.S_BroadcastMove, PacketHandler.S_BroadcastMoveHandler);

	}

	public void OnRecvPacket(PacketSession _session, ArraySegment<byte> _buffer, Action<PacketSession, IPacket> _onRecvCallback = null)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset + count);
		count += 2;

		Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
		if (m_makeFunc.TryGetValue(id, out func))
        {
			IPacket packet = func.Invoke(_session, _buffer);

			if (_onRecvCallback != null)
				_onRecvCallback.Invoke(_session, packet);
			else
				HandlePacket(_session, packet);
		}
	}

	T MakePacket<T>(PacketSession _session, ArraySegment<byte> _buffer) where T : IPacket, new()
	{
		T pkt = new T();
		pkt.Read(_buffer);
		return pkt;
	}

	public void HandlePacket(PacketSession _session, IPacket _packet)
    {
		Action<PacketSession, IPacket> action = null;
		if (m_handler.TryGetValue(_packet.Protocol, out action))
			action.Invoke(_session, _packet);
	}
}
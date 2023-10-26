using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance;
	public static PacketManager Instance
	{
		get
		{
			if (_instance == null)
				_instance = new PacketManager();
			return _instance;
		}
	}
	#endregion

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> m_onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
	Dictionary<ushort, Action<PacketSession, IPacket>> m_handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();
		
	public void Register()
	{
		m_onRecv.Add((ushort)PacketID.S_Test, MakePacket<S_Test>);
		m_handler.Add((ushort)PacketID.S_Test, PacketHandler.S_TestHandler);

	}

	public void OnRecvPacket(PacketSession _session, ArraySegment<byte> _buffer)
	{
		ushort count = 0;

		// 사이즈와 id 읽기
		ushort size = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>> action = null;
		if (m_onRecv.TryGetValue(id, out action))
			action.Invoke(_session, _buffer);
	}

	void MakePacket<T>(PacketSession _session, ArraySegment<byte> _buffer) where T : IPacket, new()
	{
		T pkt = new T();
		pkt.Read(_buffer);
		Action<PacketSession, IPacket> action = null;
		if (m_handler.TryGetValue(pkt.Protocol, out action))
			action.Invoke(_session, pkt);
	}
}
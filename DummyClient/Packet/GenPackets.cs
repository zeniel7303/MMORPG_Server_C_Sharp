using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
	C_Chat = 1,
	S_Chat = 2,
	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> _segment);
	ArraySegment<byte> Write();
}


class C_Chat : IPacket
{
	public string chat;

	public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }

	public void Read(ArraySegment<byte> _segment)
    {
		ushort count = 0;

		ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(_segment.Array, _segment.Offset, _segment.Count);
		//ushort size = BitConverter.ToUInt16(_segment.Array, _segment.Offset);
		count += sizeof(ushort);
		//ushort id = BitConverter.ToUInt16(_segment.Array, _segment.Offset + count);
		count += sizeof(ushort);
		
		ushort chatLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(span.Slice(count, chatLen));
		count += chatLen;
	}

    public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.C_Chat);
		count += sizeof(ushort);
		
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;

		success &= BitConverter.TryWriteBytes(span, count);

		if (!success) return null;

		return SendBufferHelper.Close(count);
	}
}

class S_Chat : IPacket
{
	public int playerId;
	public string chat;

	public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }

	public void Read(ArraySegment<byte> _segment)
    {
		ushort count = 0;

		ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(_segment.Array, _segment.Offset, _segment.Count);
		//ushort size = BitConverter.ToUInt16(_segment.Array, _segment.Offset);
		count += sizeof(ushort);
		//ushort id = BitConverter.ToUInt16(_segment.Array, _segment.Offset + count);
		count += sizeof(ushort);
		
		this.playerId = BitConverter.ToInt32(span.Slice(count, span.Length - count));
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(span.Slice(count, chatLen));
		count += chatLen;
	}

    public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;

		Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.S_Chat);
		count += sizeof(ushort);
		
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;

		success &= BitConverter.TryWriteBytes(span, count);

		if (!success) return null;

		return SendBufferHelper.Close(count);
	}
}


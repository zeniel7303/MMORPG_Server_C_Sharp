using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
	public abstract class Packet
	{
		public ushort size;
		public ushort id;

		public abstract ArraySegment<byte> Write();
		public abstract void Read(ArraySegment<byte> _seg);
	}

	class PlayerInfoReq : Packet
	{
		public long playerId;
		public string name;

		public struct SkillInfo
		{
			public int id;
			public short level;
			public float duration;

			public bool Write(Span<byte> _span, ref ushort _count)
			{
				bool success = true;

				success &= BitConverter.TryWriteBytes(_span.Slice(_count, _span.Length - _count), id);
				_count += sizeof(int);
				success &= BitConverter.TryWriteBytes(_span.Slice(_count, _span.Length - _count), level);
				_count += sizeof(short);
				success &= BitConverter.TryWriteBytes(_span.Slice(_count, _span.Length - _count), duration);
				_count += sizeof(float);

				return success;
			}

			public void Read(ReadOnlySpan<byte> _span, ref ushort _count)
			{
				id = BitConverter.ToInt32(_span.Slice(_count, _span.Length - _count));
				_count += sizeof(int);
				level = BitConverter.ToInt16(_span.Slice(_count, _span.Length - _count));
				_count += sizeof(short);
				duration = BitConverter.ToSingle(_span.Slice(_count, _span.Length - _count));
				_count += sizeof(float);
			}
		}
		public List<SkillInfo> skills = new List<SkillInfo>();

		public PlayerInfoReq()
		{
			this.id = (ushort)PacketID.PlayerInfoReq;
		}

		public override void Read(ArraySegment<byte> _segment)
		{
			ushort count = 0;

			ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(_segment.Array, _segment.Offset, _segment.Count);

			//ushort size = BitConverter.ToUInt16(_seg.Array, _seg.Offset);
			count += sizeof(ushort);
			//ushort id = BitConverter.ToUInt16(_seg.Array, _seg.Offset + count);
			count += sizeof(ushort);
			this.playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
			count += sizeof(long);

			// string
			ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
			count += sizeof(ushort);
			this.name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
			count += nameLen;

			// skill list
			skills.Clear();
			ushort skillLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
			count += sizeof(ushort);
			for (int i = 0; i < skillLen; i++)
			{
				SkillInfo skill = new SkillInfo();
				skill.Read(span, ref count);
				skills.Add(skill);
			}
		}

		public override ArraySegment<byte> Write()
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);

			ushort count = 0;
			bool success = true;

			Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

			count += sizeof(ushort);
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
			count += sizeof(ushort);
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
			count += sizeof(long);

			// string
			/*ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
			count += sizeof(ushort);
			Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
			count += nameLen;*/
			ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
			count += sizeof(ushort);
			count += nameLen;

			// skill list
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
			count += sizeof(ushort);
			foreach (SkillInfo skill in skills)
				success &= skill.Write(span, ref count);

			success &= BitConverter.TryWriteBytes(span, count);

			if (!success) return null;

			return SendBufferHelper.Close(count);
		}
	}

	public enum PacketID
	{
		PlayerInfoReq = 1,
		PlayerInfoRes = 2,
	}

	class ClientSession : PacketSession
	{
		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");

			try
			{
				//Packet packet = new Packet() { size = 4, id = 10 };

				//ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
				//byte[] buffer1 = BitConverter.GetBytes(packet.size);
				//byte[] buffer2 = BitConverter.GetBytes(packet.id);
				//Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
				//Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);
				//ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

				//Send(sendBuff);
				Thread.Sleep(5000);
				Disconnect();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public override void OnRecvPacket(ArraySegment<byte> _buffer)
		{
			ushort count = 0;
			ushort size = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset);
			count += 2;
			ushort id = BitConverter.ToUInt16(_buffer.Array, _buffer.Offset + count);
			count += 2;

            switch ((PacketID)id)
            {
			case PacketID.PlayerInfoReq:
					{
						PlayerInfoReq packet = new PlayerInfoReq();
						packet.Read(_buffer);
						Console.WriteLine($"PlayerInfoReq : {packet.playerId} {packet.name}");

						foreach(PlayerInfoReq.SkillInfo skill in packet.skills)
                        {
							Console.WriteLine($"Skill({skill.id}, {skill.level}, {skill.duration})");
						}
					}
				break;
            }

			Console.WriteLine($"RecvPacket Id : {id}, Size : {size}");
		}

		public override void OnSend(int _numOfBytes)
		{
			Console.WriteLine($"Transferred bytes : {_numOfBytes}");
		}

		public override void OnDisconnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnDisconnected : {_endPoint}");
		}
	}
}

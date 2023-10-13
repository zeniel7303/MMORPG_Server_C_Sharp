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
	public enum PacketID
	{
		PlayerInfoReq = 1,
		Test = 2,

	}

	class PlayerInfoReq
	{
		public byte testByte;
		public long playerId;
		public string name;
		public class Skill
		{
			public int id;
			public short level;
			public float duration;
			public class Attribute
			{
				public int att;

				public void Read(ReadOnlySpan<byte> span, ref ushort count)
				{
					this.att = BitConverter.ToInt32(span.Slice(count, span.Length - count));
					count += sizeof(int);
				}

				public bool Write(Span<byte> span, ref ushort count)
				{
					bool success = true;

					success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.att);
					count += sizeof(int);

					return success;
				}
			}
			public List<Attribute> attributes = new List<Attribute>();

			public void Read(ReadOnlySpan<byte> span, ref ushort count)
			{
				this.id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
				count += sizeof(int);
				this.level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
				count += sizeof(short);
				this.duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
				count += sizeof(float);
				this.attributes.Clear();
				ushort attributeLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
				count += sizeof(ushort);
				for (int i = 0; i < attributeLen; i++)
				{
					Attribute attribute = new Attribute();
					attribute.Read(span, ref count);
					attributes.Add(attribute);
				}
			}

			public bool Write(Span<byte> span, ref ushort count)
			{
				bool success = true;

				success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
				count += sizeof(int);
				success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.level);
				count += sizeof(short);
				success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.duration);
				count += sizeof(float);
				success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.attributes.Count);
				count += sizeof(ushort);
				foreach (Attribute attribute in this.attributes)
					success &= attribute.Write(span, ref count);

				return success;
			}
		}
		public List<Skill> skills = new List<Skill>();

		public void Read(ArraySegment<byte> _segment)
		{
			ushort count = 0;

			ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(_segment.Array, _segment.Offset, _segment.Count);
			//ushort size = BitConverter.ToUInt16(_segment.Array, _segment.Offset);
			count += sizeof(ushort);
			//ushort id = BitConverter.ToUInt16(_segment.Array, _segment.Offset + count);
			count += sizeof(ushort);

			this.testByte = (byte)_segment.Array[_segment.Offset + count];
			count += sizeof(byte);
			this.playerId = BitConverter.ToInt64(span.Slice(count, span.Length - count));
			count += sizeof(long);
			ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
			count += sizeof(ushort);
			this.name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
			count += nameLen;
			this.skills.Clear();
			ushort skillLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
			count += sizeof(ushort);
			for (int i = 0; i < skillLen; i++)
			{
				Skill skill = new Skill();
				skill.Read(span, ref count);
				skills.Add(skill);
			}
		}

		public ArraySegment<byte> Write()
		{
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);
			ushort count = 0;
			bool success = true;

			Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

			count += sizeof(ushort);
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.PlayerInfoReq);
			count += sizeof(ushort);

			segment.Array[segment.Offset + count] = (byte)this.testByte;
			count += sizeof(byte);
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
			count += sizeof(long);
			ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
			count += sizeof(ushort);
			count += nameLen;
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.skills.Count);
			count += sizeof(ushort);
			foreach (Skill skill in this.skills)
				success &= skill.Write(span, ref count);

			success &= BitConverter.TryWriteBytes(span, count);

			if (!success) return null;

			return SendBufferHelper.Close(count);
		}
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

						foreach(PlayerInfoReq.Skill skill in packet.skills)
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

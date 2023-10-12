using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using ServerCore;

namespace DummyClient
{
	class PlayerInfoReq
	{
		public long playerId;
		public string name;
		public class Skill
		{
			public int id;
			public short level;
			public float duration;

			public void Read(ReadOnlySpan<byte> span, ref ushort count)
			{
				this.id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
				count += sizeof(int);
				this.level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
				count += sizeof(short);
				this.duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
				count += sizeof(float);
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


	public enum PacketID
    {
		PlayerInfoReq = 1,
		PlayerInfoRes = 2,
	}

	class ServerSession : Session
	{
		/*static unsafe void ToBytes(byte[] _array, int _offset, ulong _value)
		{
			fixed (byte* ptr = &_array[_offset])
				*(ulong*)ptr = _value;
		}*/

		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");

			PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "ABCD" };
			packet.skills.Add(new PlayerInfoReq.Skill() { id = 101, level = 1, duration = 3.0f });
			packet.skills.Add(new PlayerInfoReq.Skill() { id = 201, level = 2, duration = 4.0f });
			packet.skills.Add(new PlayerInfoReq.Skill() { id = 301, level = 3, duration = 5.0f });
			packet.skills.Add(new PlayerInfoReq.Skill() { id = 401, level = 4, duration = 6.0f });

			try
			{
				//for (int i = 0; i < 5; i++)
				{
					ArraySegment<byte> s = packet.Write();
					if(s != null)
						Send(s);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
		public override int OnRecv(ArraySegment<byte> _buffer)
		{
			string recvData = Encoding.UTF8.GetString(
						_buffer.Array, _buffer.Offset, _buffer.Count);
			Console.WriteLine($"[From Server] {recvData}");

			return _buffer.Count;
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

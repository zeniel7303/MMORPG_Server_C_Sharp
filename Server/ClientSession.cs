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

		public PlayerInfoReq()
		{
			this.playerId = (ushort)PacketID.PlayerInfoReq;
		}

		public override void Read(ArraySegment<byte> _seg)
		{
			ushort count = 0;
			//ushort size = BitConverter.ToUInt16(_seg.Array, _seg.Offset);
			count += 2;
			//ushort id = BitConverter.ToUInt16(_seg.Array, _seg.Offset + count);
			count += 2;

			this.playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(_seg.Array, _seg.Offset + count, _seg.Count - count));
			count += 8;
		}

		public override ArraySegment<byte> Write()
		{
			ArraySegment<byte> s = SendBufferHelper.Open(4096);

			ushort count = 0;
			bool success = true;

			count += 2;
			success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.id);
			count += 2;
			success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.playerId);
			count += 8;
			success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);

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
						Console.WriteLine($"PlayerInfoReq : {packet.playerId}");
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

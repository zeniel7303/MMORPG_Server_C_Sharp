using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using ServerCore;

namespace DummyClient
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
			this.id = (ushort)PacketID.PlayerInfoReq;
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

			PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001 };

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

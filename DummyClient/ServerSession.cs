using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using ServerCore;

namespace DummyClient
{
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

			C_PlayerInfoReq packet = new C_PlayerInfoReq() { playerId = 1001, name = "ABCD" };

			var skill = new C_PlayerInfoReq.Skill() { id = 101, level = 1, duration = 3.0f };
			skill.attributes.Add(new C_PlayerInfoReq.Skill.Attribute() { att = 77 });
			packet.skills.Add(skill);

			packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 201, level = 2, duration = 4.0f });
			packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 301, level = 3, duration = 5.0f });
			packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 401, level = 4, duration = 6.0f });

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

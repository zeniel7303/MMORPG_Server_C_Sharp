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
	class ClientSession : PacketSession
	{
		public int SessionId { get; set; }
		public GameRoom Room { get; set; }
		public float PosX { get; set; }
		public float PosY { get; set; }
		public float PosZ { get; set; }

		public override void OnConnected(EndPoint _endPoint)
		{
			Console.WriteLine($"OnConnected : {_endPoint}");

			try
			{
				Program.Room.Push(() => Program.Room.Enter(this));
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public override void OnRecvPacket(ArraySegment<byte> _buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, _buffer);
		}

		public override void OnSend(int _numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes : {_numOfBytes}");
		}

		public override void OnDisconnected(EndPoint _endPoint)
		{
			SessionManager.Instance.Remove(this);
			if(Room != null)
            {
				GameRoom tempRoom = Room;
				tempRoom.Push(() => tempRoom.Leave(this));
				Room = null;
            }

			Console.WriteLine($"OnDisconnected : {_endPoint}");
		}
	}
}

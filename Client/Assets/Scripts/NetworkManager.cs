using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
	ServerSession m_session = new ServerSession();

    void Start()
    {
		// DNS (Domain Name System)
		string host = Dns.GetHostName();
		IPHostEntry ipHost = Dns.GetHostEntry(host);
		IPAddress ipAddr = ipHost.AddressList[0];
		IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

		Connector connector = new Connector();

		connector.Connect(endPoint,
			() => { return m_session; },
			1);

		StartCoroutine("CoSendPacket");
	}

    void Update()
    {
		IPacket packet = PacketQueue.Instance.Pop();
		if (packet != null)
		{
			PacketManager.Instance.HandlePacket(m_session, packet);
		}
	}

	IEnumerator CoSendPacket()
    {
		while(true)
        {
			yield return new WaitForSeconds(3.0f);

			C_Chat chatPacket = new C_Chat();
			chatPacket.chat = "Hello Unity !";
			ArraySegment<byte> segment = chatPacket.Write();

			m_session.Send(segment);
        }
    }
}

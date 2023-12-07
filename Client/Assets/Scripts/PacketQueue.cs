using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> m_packetQueue = new Queue<IPacket>();
    object m_lock = new object();

    public void Push(IPacket _packet)
    {
        lock(m_lock)
        {
            m_packetQueue.Enqueue(_packet);
        }
    }

    public IPacket Pop()
    {
        lock(m_lock)
        {
            if (m_packetQueue.Count == 0)
                return null;

            return m_packetQueue.Dequeue();
        }
    }
}

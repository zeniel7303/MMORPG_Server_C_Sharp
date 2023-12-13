using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer m_myPlayer;
    Dictionary<int, Player> m_players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(S_PlayerList _packet)
    {
        Object obj = Resources.Load("Player");

        foreach (S_PlayerList.Player p in _packet.players)
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            if (p.isSelf)
            {
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                m_myPlayer = myPlayer;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                m_players.Add(p.playerId, player);
            }
        }
    }

    public void Move(S_BroadcastMove _packet)
    {
        if (m_myPlayer.PlayerId == _packet.playerId)
        {
            m_myPlayer.transform.position = new Vector3(_packet.posX, _packet.posY, _packet.posZ);
        }
        else
        {
            Player player = null;
            if (m_players.TryGetValue(_packet.playerId, out player))
            {
                player.transform.position = new Vector3(_packet.posX, _packet.posY, _packet.posZ);
            }
        }
    }

    public void EnterGame(S_BroadcastEnterGame _packet)
    {
        if (_packet.playerId == m_myPlayer.PlayerId)
            return;

        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(_packet.posX, _packet.posY, _packet.posZ);
        m_players.Add(_packet.playerId, player);
    }

    public void LeaveGame(S_BroadcastLeaveGame _packet)
    {
        if (m_myPlayer.PlayerId == _packet.playerId)
        {
            GameObject.Destroy(m_myPlayer.gameObject);
            m_myPlayer = null;
        }
        else
        {
            Player player = null;
            if (m_players.TryGetValue(_packet.playerId, out player))
            {
                GameObject.Destroy(player.gameObject);
                m_players.Remove(_packet.playerId);
            }
        }
    }
}

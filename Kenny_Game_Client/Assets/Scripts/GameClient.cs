using UnityEngine;
using System.Collections.Generic;

using System.Net;//pour Com R-UDP
using System.Net.Sockets;//pour Com R-UDP

using LiteNetLib;
using LiteNetLib.Utils;

using Assets.Scripts;

#region Ex
#endregion Ex

public enum NetworkTags
{
    No_Player_Online,
    On_Player_Disconected,
    Player_Position,
    N_Players_Moved_Array
}

class Globals
{
    // public const string IP = "YourWebDomainName.com";// On Real Web Serveur Test OK
    public const string IP = "127.0.0.1";//Local Test 
    public static int Serveur_Port = 15000;
    public static string Serv_Key = "Server_app_key";
}

public class GameClient : MonoBehaviour, INetEventListener {

    private NetManager _netClient;
    public NetPeer _netPeer;
   // int player_Serveur_netPeer_Id;
    bool OnPeerConnected_ok = false;
    bool player_spown_decalage = false;

    private NetDataWriter netDataWriter;
    private Dictionary<int, NetPlayer> Dictionary_NetPlayer;
  
    public Player player;
    public GameObject Net_Player_GameObject;

    const float MIN_DISTANCE_TO_SEND_POSITION = 0.01f;
    private float last_Distance = 0.0f;
    private Vector3 last_Networked_Position = Vector3.zero;

    
    #region EventListener

    public void OnPeerConnected(NetPeer peer)
    {
        _netPeer = peer;
        Debug.Log($" ---------------------> OnPeerConnected Address = {peer.EndPoint.Address} Port = {peer.EndPoint.Port}" +
            $" NetPeer.ConnectId = {peer.Id}");
    }
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log($" ---------------------> OnPeerDisconnected {peer.EndPoint.Address} : {peer.EndPoint.Port} Reason: {disconnectInfo.Reason.ToString()}");
    }
    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Debug.LogError($" ---------------------> OnNetworkError {socketError}");
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        if (reader.IsNull)
            return;

        // Debug.Log($" <--------------------- OnNetworkReceive: {reader.AvailableBytes}");
        if (reader.AvailableBytes >= 4)
        {
            NetworkTags networkTag = (NetworkTags)reader.GetInt();
            if (networkTag == NetworkTags.N_Players_Moved_Array)
            {
                 int Nb_de_player = reader.GetInt();
                // int Nb_de_player_O = (reader.AvailableBytes - 4) / (sizeof(long) + sizeof(float) * 3);//pour info 
                Debug.Log($" <--------------------- OnNetworkReceive - FOUND     NetworkTags.{networkTag}    Total player MOVED = { Nb_de_player} ");// ORIGINAL CODE = { Nb_de_player_O}");

                if (!player_spown_decalage && (0 < Nb_de_player))
                {
                    Vector3 new_Local_player_pose = new Vector3(player.transform.position.x + (1.5f * Nb_de_player), player.transform.position.y, player.transform.position.z);
                    player.transform.position = new_Local_player_pose;
                    Debug.Log($" <--------------------- OnNetworkReceive - ---------------> Decalage Player POSE     {player.transform.position.x + (2f * Nb_de_player)} | {player.transform.position.y} | { player.transform.position.z}");
                    player_spown_decalage = true;
                    OnPeerConnected_ok = true;
                }

                for (int i = 0; i < Nb_de_player; i++)
                {
                    int playerid = reader.GetInt();

                    if (!Dictionary_NetPlayer.ContainsKey(playerid))
                    {
                        Dictionary_NetPlayer.Add(playerid, new NetPlayer());
                        Debug.Log($" <--------------------- OnNetworkReceive   CREAT NEW  Dictionary_NetPlayer.Add  --->  SERVER netpeer.id = " + playerid);
                    }

                    //Get online player MOVED inputs
                    //
                    //
                    //
                    //
                    //
                    //
                    Dictionary_NetPlayer[playerid].X = reader.GetFloat();
                    Dictionary_NetPlayer[playerid].Y = reader.GetFloat();
                    Dictionary_NetPlayer[playerid].Z = reader.GetFloat();
                    Debug.Log($" <--------------------- OnNetworkReceive    UPDATE   Dictionary_NetPlayer    SERVER Playerid = SERVER netpeer.id = {playerid} POSE = " +
                        $"  X = { Dictionary_NetPlayer[playerid].X }  Y = { Dictionary_NetPlayer[playerid].Y}  Z = { Dictionary_NetPlayer[playerid].Z}");
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                }
            }
            else if (networkTag == NetworkTags.No_Player_Online)
            {
                Debug.Log($" <--------------------- OnNetworkReceive: {NetworkTags.No_Player_Online}  player_spown_decalage  ANULLER OK     AvailableBytes = {reader.AvailableBytes}");
                player_spown_decalage = true;
                OnPeerConnected_ok = true;
            }
            else if (networkTag == NetworkTags.On_Player_Disconected)
            {
                int Disconnected_netPeer_Id = reader.GetInt();
                Debug.Log($" <--------------------- OnNetworkReceive: {NetworkTags.On_Player_Disconected}  ID = {Disconnected_netPeer_Id} ");
            
                if (Dictionary_NetPlayer.ContainsKey(Disconnected_netPeer_Id))
                {
                    Destroy(Dictionary_NetPlayer[Disconnected_netPeer_Id].GameObject);
                    Dictionary_NetPlayer.Remove(Disconnected_netPeer_Id);                   
                    Debug.Log($" <--------------------- OnNetworkReceive: {NetworkTags.On_Player_Disconected}  SUPRESION Disconected Player  ID  =  {Disconnected_netPeer_Id}");
                }
            }
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        Debug.Log($"OnNetworkReceiveUnconnected: {reader.ToString()}");
    }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        //Debug.Log("OnNetworkLatencyUpdate");
    }
    public void OnConnectionRequest(ConnectionRequest request)
    {
        Debug.Log("OnConnectionRequest");
    }

    #endregion EventListener

    void Start()
    {
        _netClient = new NetManager(this);
        _netClient.Start();
        _netClient.UpdateTime = 15;

        Dictionary_NetPlayer = new Dictionary<int, NetPlayer>();
        netDataWriter = new NetDataWriter();
 
        if (_netClient.IsRunning)
        {
            _netClient.Connect(/*"127.0.0.1"*/Globals.IP, /*15000*/Globals.Serveur_Port, /*"Server_app_key"*/Globals.Serv_Key);

            _netClient.SendDiscoveryRequest(new byte[] { 1 }, /*15000*/Globals.Serveur_Port);

            Debug.Log(" ---------------------> netClient.IsRunning OK !");
        }
        else
            Debug.LogError("Could not start net manager!");
    }

    // private void Update()
    private void FixedUpdate()
    {
        _netClient.PollEvents();

        if (OnPeerConnected_ok)
        {
            // var peer = _netClient.FirstPeer/*    .GetFirstPeer()*/;
            // if (peer != null && peer.ConnectionState == ConnectionState.Connected)
            // {
            last_Distance = Vector3.Distance(last_Networked_Position, player.transform.position);
                if (last_Distance >= MIN_DISTANCE_TO_SEND_POSITION)
                {
                    netDataWriter.Reset();


                    Debug.Log($" ---------------------> FixedUpdate ----->    SEND    Tags.{NetworkTags.Player_Position}   " +
                        $" Position  X ={ player.transform.position.x}  Y ={ player.transform.position.y}  Z = { player.transform.position.z}");
                    netDataWriter.Put((int)NetworkTags.Player_Position);
                    //Send Local player inputs Online
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    netDataWriter.Put(player.transform.position.x);
                    netDataWriter.Put(player.transform.position.y);
                    netDataWriter.Put(player.transform.position.z);

                    //netDataWriter.Put(player.transform.position.x);
                    //netDataWriter.Put(player.transform.position.y);
                    //netDataWriter.Put(player.transform.position.z);
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //
                    //

                    _netPeer.Send(netDataWriter, DeliveryMethod.Sequenced);

                    last_Networked_Position = player.transform.position;
                }
            // }
            //  else
            // {
            // //Debug.Log(" ERROR FixedUpdate peer = null = SendDiscoveryRequest");
            //  _netClient.SendDiscoveryRequest(new byte[] { 1 }, Globals.Serveur_Port);
            //}

            foreach (var netPlayer in Dictionary_NetPlayer)
            {
                // if (Dictionary_NetPlayer.ContainsKey(netPlayer.Key))
                // {
                if (!netPlayer.Value.GameObjectAdded)
                {
                    Debug.Log($" ---------------------> FixedUpdate ----->    INSTANCIATE new NetPlayerPrefab Id = {netPlayer.Key}   " +
                        $"netPlayer.Value.Position  X ={ netPlayer.Value.X}  Y ={ netPlayer.Value.Y}  Z = { netPlayer.Value.Z}");
                    netPlayer.Value.GameObjectAdded = true;

                    netPlayer.Value.GameObject = Instantiate(Net_Player_GameObject, netPlayer.Value.Position, Quaternion.identity);
                }
                else
                {
                    //Debug.Log($" ---------------------> FixedUpdate -----> POSE UPDATE Id = {netPlayer.Key} " +
                    //    $"netPlayer.Value.Position X={netPlayer.Value.X} Y={netPlayer.Value.Y} Z= {netPlayer.Value.Z}");
                    netPlayer.Value.GameObject.transform.position = netPlayer.Value.Position;
                }
                // } 
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (_netClient != null)
            if (_netClient.IsRunning)
                _netClient.Stop();
    }
}

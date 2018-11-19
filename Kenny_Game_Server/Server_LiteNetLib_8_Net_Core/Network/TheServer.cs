using System;
//using System.Linq;
//using System.Threading;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

using LiteNetLib;
using LiteNetLib.Utils;

public enum NetworkTags
{
    No_Player_Online,
    On_Player_Disconected,
    Player_Position,
    N_Players_Moved_Array
}

namespace Server_LiteNetLib_8_Net_Core
{
    public class TheServer : INetEventListener
    {
        public static NetManager _netManager = new NetManager(Globals._Server);

        private Dictionary<int, Server_Player> _dictionary_Server_Player;

        NetDataWriter _netDataWriter;

        //public static NetPacketProcessor _Processor = new NetPacketProcessor();
        //public static Dictionary<long, NetPeer> _ClientList = new Dictionary<long, NetPeer>();     
        //public void OnReceiveNotification(Notification packet, NetPeer peer)
        //{
        //    Console.WriteLine(packet.Message);
        //}

        public void InitializePackets()
        {
            //_Processor.SubscribeReusable<Notification, NetPeer>(OnReceiveNotification);
            _dictionary_Server_Player = new Dictionary<int, Server_Player>();
            _netDataWriter = new NetDataWriter();
        }

        #region EventListener 

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey(Config.Serv_Key);
            Console.WriteLine($"OnConnectionRequest <== {request.RemoteEndPoint} <====---Serv_Key---> netClient.IsRunning OK !");
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            try
            {
                Console.WriteLine($"OnNetworkError: {socketError}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnNetworkError Error: {ex.Message}");
            }
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnNetworkLatencyUpdate Error: {ex.Message}");
            }
        }

        void INetEventListener.OnNetworkReceive(NetPeer _netPeer, NetPacketReader _netPacketReader, DeliveryMethod deliveryMethod)
        {
            //_Processor.ReadAllPackets(_netPacketReader, _netPeer);
            try
            {
                if (_netPacketReader.IsNull)
                {
                    Console.WriteLine($"OnNetworkReceive <==== _netPacketReader.== null== null== null== null:");
                    return;
                }

                NetworkTags networkTag = (NetworkTags)_netPacketReader.GetInt();
                if (networkTag == NetworkTags.Player_Position)
                {
                    float x = _netPacketReader.GetFloat();
                    float y = _netPacketReader.GetFloat();
                    float z = _netPacketReader.GetFloat();

                    Console.WriteLine($"OnNetworkReceive   <== UPDATE ID  {_netPeer.Id}  Tags.{networkTag}:      {x} | {y} | {z}");

                    _dictionary_Server_Player[_netPeer.Id].X = x;
                    _dictionary_Server_Player[_netPeer.Id].Y = y;
                    _dictionary_Server_Player[_netPeer.Id].Z = z;

                    _dictionary_Server_Player[_netPeer.Id].Moved = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnNetworkReceive Error: {ex.Message}");
            }
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint _iPEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            try
            {
                Console.WriteLine($"OnNetworkReceiveUnconnected");

                if (messageType == UnconnectedMessageType.DiscoveryRequest)
                {
                    _netManager.SendDiscoveryResponse(new byte[] { 1 }, _iPEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnNetworkReceiveUnconnected Error: {ex.Message}");
            }
        }

        void INetEventListener.OnPeerConnected(NetPeer _netPeer)
        {
            try
            {
                Console.WriteLine($"On_Peer_Connected <==== {_netPeer.EndPoint} == NEW Client connected netPeer.ConnectId = {_netPeer.Id} ================"); /*netPeer.EndPoint.Port = {_netPeer.EndPoint.Port}");*/

                if (_dictionary_Server_Player.Count > 0)
                {
                    _netDataWriter.Reset();

                    //NetworkTags Envoyer la Pose des players au nouveaux client
                    Console.WriteLine($"On_Peer_Connected =====> Put ---------> Tags.{NetworkTags.N_Players_Moved_Array} ");
                    _netDataWriter.Put((int)NetworkTags.N_Players_Moved_Array);

                    //Envoyer Nb de player Actuel
                    Console.WriteLine($"On_Peer_Connected ====> Put ---------> _dictionary_Server_Player.Count =   {_dictionary_Server_Player.Count } = Nb de player Actuel ");
                    _netDataWriter.Put(_dictionary_Server_Player.Count);

                    //Envoyer All Players Pos moins pos du NEW Client
                    foreach (var p in _dictionary_Server_Player)
                    {
                        _netDataWriter.Put(p.Key);
                        _netDataWriter.Put(p.Value.X);
                        _netDataWriter.Put(p.Value.Y);
                        _netDataWriter.Put(p.Value.Z);
                        Console.WriteLine($"On_Peer_Connected Tags.{NetworkTags.N_Players_Moved_Array} Id = {p.Key}  Pos = {p.Value.X} | {p.Value.Y} | {p.Value.Z} ");
                    }

                    Console.WriteLine($"On_Peer_Connected ====> Send ---------> ReliableOrdered  All Players Pose to New Client  Tags.{NetworkTags.N_Players_Moved_Array} ");
                    _netPeer.Send(_netDataWriter, DeliveryMethod.ReliableOrdered);
                    // ReliableOrdered = 3, Fiable et commandé.Tous les paquets seront envoyés et reçus dans l'ordre

                    //Moved = true;
                    // _dictionary_Server_Player[_netPeer.ConnectId].Moved = true;
                }
                else
                {
                    //NetDataWriter netDataWriter = new NetDataWriter();
                    _netDataWriter.Reset();

                    //NetworkTags Envoyer la Pose des players au nouveaux client
                    Console.WriteLine($"On_Peer_Connected =====> Put ---------> Tags.{NetworkTags.No_Player_Online} ");
                    _netDataWriter.Put((int)NetworkTags.No_Player_Online);

                    ////Envoyer Nb de player Actuel moins 1 
                    //Console.WriteLine($"On_Peer_Connected ====> Put ---------> _dictionary_Server_Player.Count =   {_dictionary_Server_Player.Count /*- 1*/ } = Nb de player Actuel ");
                    //_netDataWriter.Put(_dictionary_Server_Player.Count);

                    // Console.WriteLine($"On_Peer_Connected ====> Send ---------> ReliableOrdered  All Players Pose to New Client  Tags.{NetworkTags.N_Players_Moved_Array} ");
                    _netPeer.Send(_netDataWriter, DeliveryMethod.ReliableOrdered);
                    // ReliableOrdered = 3, Fiable et commandé.Tous les paquets seront envoyés et reçus dans l'ordre
                }

                //Cree un nouveaux player dans la liste == client 
                if (!_dictionary_Server_Player.ContainsKey(_netPeer.Id))
                {
                    _dictionary_Server_Player.Add(_netPeer.Id, new Server_Player(_netPeer));
                    Console.WriteLine($"On_Peer_Connected <==== NEW Client Added in dictionary_Server_Player_Pose Peer.Id = {_netPeer.Id} ================");

                    //_dictionary_Server_Player[_netPeer.ConnectId].X = x;
                    //_dictionary_Server_Player[_netPeer.ConnectId].Y = y;
                    //_dictionary_Server_Player[_netPeer.ConnectId].Z = z;
                }
                else
                {
                    _dictionary_Server_Player[_netPeer.Id].Moved = true;
                }
                //Moved = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPeerConnected Error: {ex.Message}");
            }
        }

        void INetEventListener.OnPeerDisconnected(NetPeer _netPeer, DisconnectInfo disconnectInfo)
        {
            try
            {
                Console.WriteLine($"OnPeerDisconnected <==== [Server] ID disconnected: {_netPeer.Id}  =  {_netPeer.EndPoint.Port} -- Reason -- {disconnectInfo.Reason}");

                if (_dictionary_Server_Player.ContainsKey(_netPeer.Id))
                {
                    Console.WriteLine($"OnPeerDisconnected <==== dictionary_Server_Player.Remove(_netPeer.Id)  =  {_netPeer.Id} ");

                    int Disconnected_netPeer_Id = _netPeer.Id;

                    _dictionary_Server_Player.Remove(_netPeer.Id);

                    if (_dictionary_Server_Player.Count > 0)
                    {
                        //NetDataWriter netDataWriter = new NetDataWriter();
                        _netDataWriter.Reset();

                        //NetworkTags Envoyer la Pose des players au nouveaux client
                        _netDataWriter.Put((int)NetworkTags.On_Player_Disconected);

                        //Envoyer Id Du player Disconnected 
                        _netDataWriter.Put(Disconnected_netPeer_Id);
                        foreach (var p in _dictionary_Server_Player)
                        {
                            Console.WriteLine($"OnPeerDisconnected =====> SEND To {p.Key}  ---------> Tags.{NetworkTags.On_Player_Disconected}   ID = {Disconnected_netPeer_Id} ");
                            p.Value.NetPeer.Send(_netDataWriter, DeliveryMethod.ReliableOrdered);
                            // ReliableOrdered = 3, Fiable et commandé.Tous les paquets seront envoyés et reçus dans l'ordre
                        }
                        //Moved = true;
                        // _dictionary_Server_Player[_netPeer.ConnectId].Moved = true;
                    }
                }                   
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPeerDisconnected Error: {ex.Message}");
            }
        }

        #endregion EventListener

        public void SendPlayerPositions()
        {
            try
            {
                Dictionary<int, Server_Player> _dictionary_send_to_Players =
                    new Dictionary<int, Server_Player>(_dictionary_Server_Player);

                foreach (var Player_dest in _dictionary_send_to_Players)
                {
                    if (Player_dest.Value == null)
                        continue;

                    int amountPlayersMoved = 0;
                    foreach (var posPlayers in _dictionary_send_to_Players)
                    {
                        if (Player_dest.Key == posPlayers.Key)
                            continue;

                        if (!posPlayers.Value.Moved)
                            continue;

                        amountPlayersMoved++;
                    }

                    _netDataWriter.Reset();

                    _netDataWriter.Put((int)NetworkTags.N_Players_Moved_Array);

                    _netDataWriter.Put(amountPlayersMoved);

                    amountPlayersMoved = 0;

                    foreach (var Ps in _dictionary_send_to_Players)
                    {
                        if (Player_dest.Key == Ps.Key)
                            continue;

                        if (!Ps.Value.Moved)
                            continue;

                        _netDataWriter.Put(Ps.Key);

                        _netDataWriter.Put(Ps.Value.X);
                        _netDataWriter.Put(Ps.Value.Y);
                        _netDataWriter.Put(Ps.Value.Z);

                        amountPlayersMoved++;
                        Console.WriteLine($"Send List {amountPlayersMoved} > PLAYER MOVED ID = {Ps.Key}  POS = {Ps.Value.X} | {Ps.Value.Y} | {Ps.Value.Z}");
                    }

                    if (amountPlayersMoved > 0)
                    {
                        Player_dest.Value.NetPeer.Send(_netDataWriter, DeliveryMethod.Sequenced);

                        Console.WriteLine($"Send To ====> ID {Player_dest.Key}  DeliveryMethod.Sequenced  TOTAL = {amountPlayersMoved}  PLAYER MOVED");
                    }
                    //Player_dest.Value.NetPeer.Send(_netDataWriter, DeliveryMethod.Sequenced);
                    //// Unreliable = 0,Les paquets peuvent être déposés, dupliqués ou arriver sans commande
                    //// ReliableUnordered = 1,Tous les paquets seront envoyés et reçus, mais sans commande
                    //// Sequenced = 2,Les paquets peuvent être déposés, mais jamais dupliqués et arrivent dans l'ordre
                    //// ReliableOrdered = 3, Fiable et commandé.Tous les paquets seront envoyés et reçus dans l'ordre
                    //// ReliableSequenced = 4 Fiable que dernier paquet

                }

                //reinit Moved = false;
                foreach (var player in _dictionary_Server_Player)
                    player.Value.Moved = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendPlayerPositions Error: {ex.Message}");
            }
        }
    }
}

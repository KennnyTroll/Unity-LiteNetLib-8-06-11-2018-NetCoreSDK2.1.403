using LiteNetLib;//pour Com R-UDP LiteNetLib 8

namespace Server_LiteNetLib_8_Net_Core
{
    public class Server_Player
    {
        public NetPeer NetPeer { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public bool Moved { get; set; }


        public Server_Player(NetPeer peer_Id)
        {
            NetPeer = peer_Id;

            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;

            Moved = false;
        }
    }
}

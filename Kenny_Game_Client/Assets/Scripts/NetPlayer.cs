using UnityEngine;

namespace Assets.Scripts
{
    public class NetPlayer
    {
        public GameObject GameObject { get; set; }
        public bool GameObjectAdded { get; set; }
        public NetPlayer()
        {
            GameObjectAdded = false;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Vector3 Position => new Vector3(X, Y, Z);


        //public float RX { get; set; }
        //public float RY { get; set; }
        //public float RZ { get; set; }
        //public Vector3 Rotation => new Vector3(RX, RY, RZ);
    }
}

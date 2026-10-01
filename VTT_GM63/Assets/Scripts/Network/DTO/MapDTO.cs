using System;

namespace Network.DTO
{
    [Serializable]
    public class MapDTO
    {
        public int Width;
        public int Height;

        public int[] Terrain;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapRoom
    {
        public int StartX { get; init; }

        public int StartY { get; init; }

        public int Width { get; init; }

        public int Height { get; init; }

        public MapDoor[] Doors { get; private set; } = Array.Empty<MapDoor>();

        internal void SetDoors(MapDoor[] doors)
        {
            Doors = doors;
        }
    }
}

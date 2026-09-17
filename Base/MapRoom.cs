using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapRoom
    {
        public int StartX { get; internal set; }

        public int StartY { get; internal set; }

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public MapDoor[] Doors { get; private set; } = Array.Empty<MapDoor>();

        internal void SetDoors(MapDoor[] doors)
        {
            Doors = doors;
        }

        public bool AreEqual(MapRoom anotherRoom)
        {
            if (anotherRoom is null)
            {
                return false;
            }
            else
            {
                return StartX == anotherRoom.StartX && StartY == anotherRoom.StartY;
            }
        }
    }
}

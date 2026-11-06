using MapGenerationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapRoom : ILocatiable
    {
        public MapPoint StartLocation { get; } = new MapPoint();

        public int StartX { get => StartLocation.X; internal set => StartLocation.SetX(value); }

        public int StartY { get => StartLocation.Y; internal set => StartLocation.SetY(value); }

        public MapPoint Size { get; } = new MapPoint();

        public int Width { get => Size.X; internal set => Size.SetX(value); }

        public int Height { get => Size.Y; internal set => Size.SetY(value); }

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

        public override string ToString() => $"<{StartLocation}, [{StartLocation.X + Size.X - 1}, {StartLocation.Y + Size.Y - 1}]>";
    }
}

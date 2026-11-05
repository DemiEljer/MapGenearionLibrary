using MapGenearionLibrary.Base;
using MapGenerationLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Navigation
{
    public class MapNavigationHandler
    {
        public Map Map { get; }

        public MapDoor[] Doors { get; private set; } = Array.Empty<MapDoor>();

        public MapLocationTree<MapRoom> Rooms { get; private set; } = null;

        public MapNavigationGraph NavigationGraph { get; }

        public MapObstaclesHandler StaticObstacles { get; }

        public MapDynamicObstaclesHandler DynamicObstacles { get; }

        internal MapSeperationAreaIterator? AreasIterator { get; } = null;

        public MapNavigationHandler(Map map)
        {
            Map = map;
            if (Map.MapConstruction != null)
            {
                AreasIterator = new(Map.MapConstruction);
            }

            _GetAllDoors();
            _AssosiateRoomsWithDoors();

            NavigationGraph = new(this);
            StaticObstacles = new(Map);
            DynamicObstacles = new(Map);
        }

        private void _GetAllDoors()
        {
            var foundDoors = new LinkedList<MapDoor>();

            if (AreasIterator is not null)
            {
                foreach (var area in AreasIterator)
                {
                    foreach (var door in area.Doors)
                    {
                        foundDoors.AddLast(door);
                    }
                }
            }

            Doors = foundDoors.ToArray();
        }

        public void _AssosiateRoomsWithDoors()
        {
            Rooms = MapLocationTree<MapRoom>.CreateTree(AreasIterator, (area, index) => area.GetRoom());

            foreach (var room in Rooms)
            {
                room.SetDoors(Doors.Where(door => room.DoesRoomContainsDoor(door)).ToArray());
            }
        }

        public MapRoom? GetRoom(MapPoint point) => Rooms.GetValue(point);
    }
}

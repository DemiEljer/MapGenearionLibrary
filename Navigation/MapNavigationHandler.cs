using MapGenearionLibrary.Base;
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

        public MapRoom[] Rooms { get; private set; } = Array.Empty<MapSeperationArea>();

        public MapNavigationGraph NavigationGraph { get; }

        public MapObstaclesHandler Obstacles { get; }

        private MapSeperationAreaIterator? _AreasIterator { get; } = null;

        public MapNavigationHandler(Map map)
        {
            Map = map;
            if (Map.MapConstruction != null)
            {
                _AreasIterator = new(Map.MapConstruction);
            }

            _GetAllDoors();
            _AssosiateRoomsWithDoors();

            NavigationGraph = new(this);
            Obstacles = new(Map);
        }

        private void _GetAllDoors()
        {
            var foundDoors = new List<MapDoor>();

            if (_AreasIterator is not null)
            {
                foreach (var area in _AreasIterator)
                {
                    foundDoors.AddRange(area.Doors);
                }
            }

            Doors = foundDoors.ToArray();
        }

        public void _AssosiateRoomsWithDoors()
        {
            if (_AreasIterator is not null)
            {
                Rooms = _AreasIterator.Where(a => a.ChildAreas.Length == 0).Select(area => area.GetRoom()).ToArray();
            }

            foreach (var room in Rooms)
            {
                room.SetDoors(Doors.Where(door => MapPointOperations.DoesRoomContainsDoor(room, door)).ToArray());
            }
        }

        public MapRoom? GetRoom(MapPoint point) => Rooms.FirstOrDefault(room => MapPointOperations.DoesRoomContainsPoint(room, point));
    }
}

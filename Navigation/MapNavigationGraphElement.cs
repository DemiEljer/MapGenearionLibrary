using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Navigation
{
    public class MapNavigationGraphElement
    {
        public MapRoom Room { get; }

        public (MapNavigationGraphElement Element, MapDoor Door)[] RoomsTo { get; internal set; } = Array.Empty<(MapNavigationGraphElement Element, MapDoor Door)>();

        public MapNavigationGraphElement(MapRoom room)
        {
            Room = room;
        }
    }
}

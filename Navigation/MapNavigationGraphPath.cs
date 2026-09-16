using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Navigation
{
    public class MapNavigationGraphPath
    {
        public MapRoom RoomFrom { get; internal set; }

        public MapRoom RoomTo { get; internal set; }

        public MapPoint PointFrom { get; internal set; }

        public MapPoint PointTo { get; internal set; }

        public (MapPoint point, MapRoom room)[] PointsSequence { get; internal set; }

        public double Distance { get; set; }

        internal MapNavigationGraphPath() { }
    }
}

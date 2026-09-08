using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapDoor
    {
        public MapPoint Area1 { get; }

        public MapPoint Area2 { get; }

        public MapDoor(MapPoint area1, MapPoint area2)
        {
            Area1 = area1;
            Area2 = area2;
        }
    }
}

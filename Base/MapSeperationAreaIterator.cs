using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Base
{
    public class MapSeperationAreaIterator : IEnumerable<MapSeperationArea>
    {
        public MapSeperationArea MainArea { get; }

        public MapSeperationArea[] AllAreas { get; private set; } = Array.Empty<MapSeperationArea>();

        public int Count => AllAreas.Length;

        public MapSeperationAreaIterator(MapSeperationArea area)
        {
            MainArea = area;

            _InitAreasCollection();
        }

        private void _InitAreasCollection()
        {
            List<MapSeperationArea> _areas = new();

            void _GetSubAreas(MapSeperationArea area)
            {
                _areas.Add(area);

                if (area.ChildAreas.Length > 0)
                {
                    _GetSubAreas(area.ChildAreas[0]);
                    _GetSubAreas(area.ChildAreas[1]);
                }
            }

            _GetSubAreas(MainArea);

            AllAreas = _areas.ToArray();
        }

        public IEnumerator<MapSeperationArea> GetEnumerator()
        {
            foreach (var area in AllAreas)
            {
                yield return area;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

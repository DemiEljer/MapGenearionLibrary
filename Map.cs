using MapGenearionLibrary.Base;
using System;
using System.Linq;


namespace MapGenearionLibrary
{
    public class Map
    {
        private MapCell[] _Cells { get; } = Array.Empty<MapCell>();

        public MapSeperationArea? MapConstruction { get; internal set; } = null;

        public int Width { get; } = 0;

        public int Height { get; } = 0;

        public MapCell this[int x, int y]
        {
            get => GetCell(x, y);
        }

        public MapCell this[MapPoint point]
        {
            get => GetCell(point);
        }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;

            _Cells = Enumerable.Range(0, Width * Height).Select(i => new MapCell(i % Width, i / Width)).ToArray();
        }

        public MapCell? GetCell(MapPoint point) => GetCell(point.X, point.Y);

        public MapCell? GetCell(int x, int y)
        {
            if (_Cells.Length == 0)
            {
                return null;
            }
            else
            {
                return _Cells[this.GetMapCellLocationIndex(x, y)];
            }
        }

        public void Foreach(Action<MapPoint, MapCell>? handler)
        {
            if (handler is not null)
            {
                foreach (var cell in _Cells)
                {
                    handler.Invoke(cell.Point, cell);
                }
            }
        }
    }
}

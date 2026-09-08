using MapGenearionLibrary.Base;

namespace MapGenearionLibrary
{
    public class Map
    {
        private MapCell[] _Cells { get; } = Array.Empty<MapCell>();

        public MapSeperationArea? MapConstruction { get; internal set; } = null;

        public int Width { get; } = 0;

        public int Height { get; } = 0;

        public Map(int width, int height)
        {
            Width = width;
            Height = height;

            _Cells = Enumerable.Range(0, Width * Height).Select(i => new MapCell(i % Width, i / Width)).ToArray();
        }

        public MapCell? GetCell(int x, int y)
        {
            if (_Cells.Length == 0)
            {
                return null;
            }
            else
            {
                //
                x = x % Width;
                y = y % Height;
                // 
                x = x < 0 ? x + Width : x;
                y = y < 0 ? y + Height : y;

                return _Cells[y * Width + x];
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

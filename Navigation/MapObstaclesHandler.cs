using MapGenearionLibrary;
using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Navigation
{
    public class MapObstaclesHandler
    {
        public Map Map { get; }

        private int[] _Obstacles { get; }

        public bool this[MapPoint point]
        {
            get => GetCellObstacle(point);
            set => SetCellObstacle(point, value);
        }

        public bool this[int x, int y]
        {
            get => GetCellObstacle(x, y);
            set => SetCellObstacle(x, y, value);
        }

        public event Action<int, int, bool> ObstacleHasBeenChanged;

        public MapObstaclesHandler(Map map)
        {
            Map = map;

            _Obstacles = new int[Map.Width * Map.Height];
        }

        public bool GetCellObstacle(MapPoint point) => GetCellObstacle(point.X, point.Y);

        public bool GetCellObstacle(int x, int y)
        {
            if (_Obstacles.Length > 0)
            {
                return _Obstacles[Map.GetMapCellLocationIndex(x, y)] > 0;
            }
            else
            {
                return false;
            }
        }

        public void SetCellObstacle(MapPoint point, bool obstacleExistance) => SetCellObstacle(point.X, point.Y, obstacleExistance);

        public void SetCellObstacle(int x, int y, bool obstacleExistance)
        {
            if (_Obstacles.Length > 0)
            {
                int cellIndex = Map.GetMapCellLocationIndex(x, y);

                _Obstacles[cellIndex] += obstacleExistance ? 1 : -1;
                _Obstacles[cellIndex] = Math.Max(0, _Obstacles[cellIndex]);

                ObstacleHasBeenChanged?.Invoke(x, y, obstacleExistance);
            }
        }
    }
}

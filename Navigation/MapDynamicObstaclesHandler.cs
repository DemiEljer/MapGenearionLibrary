using MapGenearionLibrary;
using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary.Navigation
{
    public class MapDynamicObstaclesHandler
    {
        public Map Map { get; }

        private Object[] _Obstacles { get; }

        public bool this[MapPoint point, Object owner]
        {
            get => GetCellObstacle(point, owner);
        }

        public bool this[int x, int y, Object owner]
        {
            get => GetCellObstacle(x, y, owner);
        }

        public event Action<int, int, Object> ObstacleHasBeenChanged;

        public MapDynamicObstaclesHandler(Map map)
        {
            Map = map;

            _Obstacles = new Object[Map.Width * Map.Height];
        }

        public bool GetCellObstacle(MapPoint point, Object owner) => GetCellObstacle(point.X, point.Y, owner);

        public bool GetCellObstacle(int x, int y, Object owner)
        {
            if (_Obstacles.Length > 0)
            {
                var obstacleOwner = _Obstacles[Map.GetMapCellLocationIndex(x, y)];

                return obstacleOwner is null || obstacleOwner == owner;
            }
            else
            {
                return false;
            }
        }

        public void SetCellObstacle(MapPoint point, bool obstacleExistance, Object owner) => SetCellObstacle(point.X, point.Y, obstacleExistance, owner);

        public void SetCellObstacle(int x, int y, bool obstacleExistance, Object owner)
        {
            if (_Obstacles.Length > 0)
            {
                int cellIndex = Map.GetMapCellLocationIndex(x, y);

                if (owner is null)
                {
                    _Obstacles[cellIndex] = null;

                    ObstacleHasBeenChanged?.Invoke(x, y, owner);
                }
                else if (_Obstacles[cellIndex] is null)
                {
                    _Obstacles[cellIndex] = owner;

                    ObstacleHasBeenChanged?.Invoke(x, y, owner);
                }
            }
        }
    }
}

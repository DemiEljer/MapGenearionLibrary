using MapGenearionLibrary;
using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.UI.GridLayoutGroup;

namespace MapGenearionLibrary.Navigation
{
    public class MapDynamicObstaclesHandler
    {
        public Map Map { get; }

        private object[] _Obstacles { get; }

        public bool this[MapPoint point, object owner]
        {
            get => GetCellObstacle(point, owner);
        }

        public bool this[int x, int y, object owner]
        {
            get => GetCellObstacle(x, y, owner);
        }

        public event Action<int, int, object> ObstacleHasBeenChanged;

        public MapDynamicObstaclesHandler(Map map)
        {
            Map = map;

            _Obstacles = new object[Map.Width * Map.Height];
        }

        public bool GetCellObstacle(MapPoint point, object owner) => GetCellObstacle(point.X, point.Y, owner);

        public bool GetCellObstacle(int x, int y, object owner)
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

        public void SetCellObstacle(MapPoint point, object owner) => SetCellObstacle(point.X, point.Y, owner);

        public void SetCellObstacle(int x, int y, object owner)
        {
            if (_Obstacles.Length > 0)
            {
                int cellIndex = Map.GetMapCellLocationIndex(x, y);

                if (owner is null 
                    || _Obstacles[cellIndex] is null)
                {
                    _Obstacles[cellIndex] = owner;

                    ObstacleHasBeenChanged?.Invoke(x, y, owner);
                }
            }
        }

        public object GetCellObstacleOwner(MapPoint point) => GetCellObstacleOwner(point.X, point.Y);

        public object GetCellObstacleOwner(int x, int y)
        {
            if (_Obstacles.Length > 0)
            {
                return _Obstacles[Map.GetMapCellLocationIndex(x, y)];
            }
            else
            {
                return null;
            }
        }

        public void ForceTake(MapPoint point, object owner) => ForceTake(point.X, point.Y, owner);

        public void ForceTake(int x, int y, object owner)
        {
            if (_Obstacles.Length > 0)
            {
                _Obstacles[Map.GetMapCellLocationIndex(x, y)] = owner;
            }
        }

        public void Release(MapPoint point, object owner) => Release(point.X, point.Y, owner);

        public void Release(int x, int y, object owner)
        {
            if (_Obstacles.Length > 0)
            {
                int cellIndex = Map.GetMapCellLocationIndex(x, y);

                if (_Obstacles[cellIndex] == owner)
                {
                    _Obstacles[cellIndex] = null;

                    ObstacleHasBeenChanged?.Invoke(x, y, owner);
                }
            }
        }

        public void HandleSingle(Action<MapDynamicObstaclesHandler> handler)
        {
            if (handler is not null)
            {
                lock (this)
                {
                    handler?.Invoke(this);
                }
            }
        }
    }
}

using MapGenearionLibrary.Base;
using MapGenearionLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary
{
    public class MapCell
    {
        private bool[] _Walls { get; } = new bool[4];

        public bool TopWall { get => GetWall(CellWallOrientationEnum.Top); set => SetWall(CellWallOrientationEnum.Top, value); }

        public bool BottomWall { get => GetWall(CellWallOrientationEnum.Bottom); set => SetWall(CellWallOrientationEnum.Bottom, value); }

        public bool LeftWall { get => GetWall(CellWallOrientationEnum.Left); set => SetWall(CellWallOrientationEnum.Left, value); }

        public bool RightWall { get => GetWall(CellWallOrientationEnum.Right); set => SetWall(CellWallOrientationEnum.Right, value); }

        public MapPoint Point { get; } = new();

        public MapCell(int x, int y)
        {
            ResetWalls();

            Point.Set(x, y);
        }

        public bool GetWall(CellWallOrientationEnum wall) => _Walls[(int)wall];

        public void SetWall(CellWallOrientationEnum wall, bool existance) => _Walls[(int)wall] = existance;

        public void ResetWalls()
        {
            for (int i = 0; i < _Walls.Length; i++)
            {
                _Walls[i] = false;
            }
        }
    }
}

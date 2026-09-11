using MapGenearionLibrary.Base;
using MapGenearionLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary
{
    public class MapFabric
    {
        public Random Rnd { get; }

        public MapFabric()
        {
            Rnd = new();
        }

        public MapFabric(int randomSeed)
        {
            Rnd = new(randomSeed);
        }

        public Map GenerateMap(MapGenerationConfig mapConfig)
        {
            Map map = new(Math.Max(0, mapConfig.Width), Math.Max(0, mapConfig.Height));

            void WallSeperation(MapSeperationArea area, int layerIndex)
            {
                var newAreas = area.Seperate((x1, y1, x2, y2, orientation) =>
                {
                    if (mapConfig.MaxDoorsCount <= 0)
                    {
                        return MapBuilder.SetLongWallWithDoors(map, Rnd, 1, new MapPoint(x1, y1), new MapPoint(x2, y2), orientation);
                    }
                    else
                    {
                        if (mapConfig.IsRandomDoorsCount)
                        {
                            return MapBuilder.SetLongWallWithDoors(map, Rnd, Rnd.Next(mapConfig.MaxDoorsCount), new MapPoint(x1, y1), new MapPoint(x2, y2), orientation);
                        }
                        else
                        {
                            return MapBuilder.SetLongWallWithDoors(map, Rnd, mapConfig.MaxDoorsCount, new MapPoint(x1, y1), new MapPoint(x2, y2), orientation);
                        } 
                    }
                }
                , 
                mapConfig.MinRoomWidth > 0 ? mapConfig.MinRoomWidth : 1
                ,
                mapConfig.MinRoomHeight > 0 ? mapConfig.MinRoomHeight : 1);

                if (mapConfig.MaxLayerCount <= 0 || layerIndex < mapConfig.MaxLayerCount)
                {
                    foreach (var newArea in newAreas)
                    {
                        if (mapConfig.MinRoomWidth <= 0
                            || mapConfig.MinRoomHeight <= 0
                            || (newArea.Width >= mapConfig.MinRoomWidth && newArea.Height >= mapConfig.MinRoomHeight))
                        {
                            WallSeperation(newArea, layerIndex + 1);
                        }
                    }
                }
            }

            var mainArea = new MapSeperationArea(Rnd)
            {
                StartX = 0
                    ,
                StartY = 0
                    ,
                Width = map.Width
                    ,
                Height = map.Height
            };

            if (map.Width > 0
                && map.Height > 0)
            {
                WallSeperation(mainArea, 0);
            }

            map.MapConstruction = mainArea;

            return map;
        }

        public void SetMapBorderWalls(Map map)
        {
            if (map.Height > 0
                && map.Width > 0)
            {
                MapBuilder.SetLongWall(map, new MapPoint(0, 0), new MapPoint(map.Width, 0), CellWallOrientationEnum.Top);
                MapBuilder.SetLongWall(map, new MapPoint(0, map.Height - 1), new MapPoint(map.Width, map.Height - 1) , CellWallOrientationEnum.Bottom);
                MapBuilder.SetLongWall(map, new MapPoint(0, 0), new MapPoint(0, map.Height), CellWallOrientationEnum.Left);
                MapBuilder.SetLongWall(map, new MapPoint(map.Width - 1, 0), new MapPoint(map.Width - 1, map.Height), CellWallOrientationEnum.Right);
            }
        }
    }
}

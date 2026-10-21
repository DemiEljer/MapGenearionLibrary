using MapGenearionLibrary.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MapGenearionLibrary.Navigation
{
    public class MapNavigationGraph
    {
        public MapNavigationHandler NavigationHandler { get; }

        public MapNavigationGraphElement[] Nodes { get; }

        public MapNavigationGraph(MapNavigationHandler navigationHandler)
        {
            NavigationHandler = navigationHandler;

            Nodes = navigationHandler.Rooms.Select(room => new MapNavigationGraphElement(room)).ToArray();

            IEnumerable<(MapNavigationGraphElement to, MapDoor[] doors)> GetAllRoomDoorsPairs(MapNavigationGraphElement nodeFrom)
            {
                foreach (var node in Nodes)
                {
                    var doorsToRoom = node.Room.Doors.Where(door => node != nodeFrom && nodeFrom.Room.DoesRoomContainsDoor(door)).ToArray();

                    if (doorsToRoom.Length > 0)
                    {
                        yield return (node, doorsToRoom);
                    }
                }
            }

            foreach (var node in Nodes)
            {
                node.RoomsTo = GetAllRoomDoorsPairs(node).ToArray();
            }
        }

        public MapNavigationGraphElement? GetElement(MapPoint point) => Nodes.FirstOrDefault(node => MapPointOperations.DoesRoomContainsPoint(node.Room, point));

        public MapNavigationGraphElement? GetElement(MapRoom room) => Nodes.FirstOrDefault(node => node.Room == room);

        public MapRoom[] GetNeighbourRooms(MapPoint point)
        {
            var element = GetElement(point);

            if (element is not null)
            {
                return element.RoomsTo.Select(element => element.Element.Room).ToArray();
            }
            else
            {
                return Array.Empty<MapRoom>();
            }
        }

        public MapNavigationGraphPath[] GetPathes(MapPoint pointFrom, MapPoint pointTo, int layersCount = -1)
        {
            MapNavigationGraphElement? roomFrom = GetElement(pointFrom);
            MapNavigationGraphElement? roomTo = GetElement(pointTo);

            if (roomFrom is null
                || roomTo is null
                || NavigationHandler.Obstacles[pointTo])
            {
                return Array.Empty<MapNavigationGraphPath>();
            }
            else
            {
                List<MapNavigationGraphPath> resultPathes = new();
                List<MapNavigationGraphElement> currentElementPath = new();
                List<(MapPoint point, MapRoom room)> currentPointPath = new();
                double minDistanationDistance = double.MaxValue;
                double currentDistance = 0;
                int currentLayerIndex = 0;

                void _PathFinder(MapNavigationGraphElement currentRoom, MapPoint currentPosition)
                {
                    if (currentDistance > minDistanationDistance
                        || currentElementPath.FirstOrDefault(element => element == currentRoom) != null)
                    {
                        return;
                    }

                    currentElementPath.Add(currentRoom);
                    currentPointPath.Add((currentPosition, currentRoom.Room));
                    currentLayerIndex++;

                    if (currentRoom == roomTo)
                    {
                        if (currentDistance < minDistanationDistance)
                        {
                            currentPointPath.Add((pointTo, currentRoom.Room));

                            resultPathes.Add(new MapNavigationGraphPath()
                            {
                                RoomFrom = roomFrom.Room
                                ,
                                RoomTo = roomTo.Room
                                ,
                                PointFrom = pointFrom
                                ,
                                PointTo = pointTo
                                ,
                                PointsSequence = currentPointPath.ToArray()
                                ,
                                Distance = currentDistance
                            });

                            currentPointPath.RemoveAt(currentPointPath.Count - 1);

                            minDistanationDistance = currentDistance;
                            // Удаление более долгих путей
                            for (int i = 0; i < resultPathes.Count;)
                            {
                                if (resultPathes[i].Distance > currentDistance + 0.5)
                                {
                                    resultPathes.RemoveAt(i);
                                }
                                else
                                {
                                    i++;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (currentLayerIndex > layersCount && layersCount >= 0)
                        {
                            return;
                        }

                        IEnumerable<(MapNavigationGraphElement Element, MapDoor Door)> _GetNextRooms()
                        {
                            var nextTargetRoom = currentRoom.RoomsTo.FirstOrDefault(room => room.Element.Room.AreEqual(roomTo.Room));

                            if (nextTargetRoom.Element is not null)
                            {
                                foreach (var door in nextTargetRoom.Doors)
                                {
                                    yield return (nextTargetRoom.Element, door);
                                }
                            }
                            else
                            {
                                foreach (var nextRoom in currentRoom.RoomsTo)
                                {
                                    foreach (var door in nextRoom.Doors)
                                    {
                                        yield return (nextRoom.Element, door);
                                    }
                                }
                            }
                        }

                        var nextRooms = _GetNextRooms()
                        .Where(room =>
                            (!NavigationHandler.Obstacles[room.Door.Area1]
                            && !NavigationHandler.Obstacles[room.Door.Area2])
                            || room.Door.Area1.AreEqual(pointFrom)
                            || room.Door.Area2.AreEqual(pointFrom))
                        .Select(room =>
                        {
                            if (MapPointOperations.DoesRoomContainsPoint(currentRoom.Room, room.Door.Area1))
                            {
                                return (room.Door.Area1, room.Door.Area2, room, MapPointOperations.GetDistance(currentPosition, room.Door.Area2));
                            }
                            else
                            {
                                return (room.Door.Area2, room.Door.Area1, room, MapPointOperations.GetDistance(currentPosition, room.Door.Area2));
                            }
                        })
                        .GroupBy(tuple => tuple.Item3.Element)
                        .Select(group =>
                        {
                            var minDistance = group.Min(value => MapPointOperations.GetDistance(value.Item2, currentPosition) + MapPointOperations.GetDistance(value.Item2, pointTo));

                            return group.Where(value => MapPointOperations.GetDistance(value.Item2, currentPosition) + MapPointOperations.GetDistance(value.Item2, pointTo) <= minDistance).First();
                        }).OrderBy(tuple => tuple.Item4);

                        foreach (var nextRoom in nextRooms)
                        {
                            currentDistance += nextRoom.Item4;

                            if (currentPointPath.Last().point.Compare(nextRoom.Item1))
                            {
                                _PathFinder(nextRoom.Item3.Element, nextRoom.Item2);
                            }
                            else
                            {
                                currentPointPath.Add((nextRoom.Item1, currentRoom.Room));

                                _PathFinder(nextRoom.Item3.Element, nextRoom.Item2);

                                currentPointPath.RemoveAt(currentPointPath.Count - 1);
                            }

                            currentDistance -= nextRoom.Item4;
                        }
                    }

                    currentElementPath.RemoveAt(currentElementPath.Count - 1);
                    currentPointPath.RemoveAt(currentPointPath.Count - 1);
                    currentLayerIndex--;
                }

                _PathFinder(roomFrom, pointFrom);

                return resultPathes.ToArray();
            }
        }

        class RoomPathelement
        {
            public RoomPathelement PreviousElement { get; } = null;

            public MapNavigationGraphElement CurrentElement { get; } = null;

            public MapDoor[] Doors { get; } = Array.Empty<MapDoor>();

            public double Distance { get; } = 0;

            public RoomPathelement(RoomPathelement prev, MapNavigationGraphElement current, MapDoor[] doors = null)
            {
                PreviousElement = prev;
                CurrentElement = current;

                if (PreviousElement is not null)
                {
                    Distance = PreviousElement.Distance + PreviousElement.CurrentElement.Room.GetRoomsDistance(CurrentElement.Room);
                }

                if (doors is not null)
                {
                    Doors = doors;
                }
            }
        }

        public MapNavigationGraphPath GetPathesWave(MapPoint pointFrom, MapPoint pointTo, int layersCount = -1)
        {
            MapNavigationGraphElement roomFrom = GetElement(pointFrom);
            MapNavigationGraphElement roomTo = GetElement(pointTo);

            var roomsSequence = _FindRoomsSqeuence(roomFrom, roomTo, layersCount);

            if (roomsSequence.Length > 0)
            {
                LinkedList<(MapPoint Point, MapRoom Room)> currentPointPath = new();
                double currentDistance = 0;

                currentPointPath.AddLast((pointFrom, roomFrom.Room));

                foreach (var nextRoom in roomsSequence.TakeLast(roomsSequence.Length - 1))
                {
                    var doorsTo = nextRoom.Doors
                        .Select(door =>
                        {
                            if (currentPointPath.Last.Value.Room.DoesRoomContainsPoint(door.Area1))
                            {
                                return (door.Area1, door.Area2, currentPointPath.Last.Value.Point.GetDistance(door.Area1) + 1);
                            }
                            else
                            {
                                return (door.Area2, door.Area1, currentPointPath.Last.Value.Point.GetDistance(door.Area2) + 1);
                            }
                        })
                        .OrderBy(tuple => tuple.Item3);

                    var nearestDoor = doorsTo.First();

                    currentPointPath.AddLast((nearestDoor.Item1, currentPointPath.Last.Value.Room));
                    currentPointPath.AddLast((nearestDoor.Item2, nextRoom.CurrentElement.Room));
                    currentDistance += nearestDoor.Item3;
                }

                currentDistance += currentPointPath.Last.Value.Point.GetDistance(pointTo);
                currentPointPath.AddLast((pointTo, roomTo.Room));

                return new MapNavigationGraphPath()
                {
                    RoomFrom = roomFrom.Room
                    ,
                    RoomTo = roomTo.Room
                    ,
                    PointFrom = pointFrom
                    ,
                    PointTo = pointTo
                    ,
                    PointsSequence = currentPointPath.ToArray()
                    ,
                    Distance = currentDistance
                };
            }
            else
            {
                return null;
            }
        }

        private RoomPathelement[] _FindRoomsSqeuence(MapNavigationGraphElement roomFrom, MapNavigationGraphElement roomTo, int layersCount = -1)
        {
            if (roomFrom is null
                || roomTo is null)
            {
                return Array.Empty<RoomPathelement>();
            }
            else
            {
                LinkedList<RoomPathelement> pathesWave = new();
                Dictionary<MapNavigationGraphElement, RoomPathelement> passedElements = new();
                
                {   
                    double minDistance = double.MaxValue;
                    RoomPathelement targetSequenceElement = null;

                    IEnumerable<RoomPathelement> _GetPathFromSequence()
                    {
                        while (targetSequenceElement is not null)
                        {
                            yield return targetSequenceElement;

                            targetSequenceElement = targetSequenceElement.PreviousElement;
                        }
                    }

                    bool _AddWaveElement(RoomPathelement prev, MapNavigationGraphElement room)
                    {
                        if (passedElements.ContainsKey(room))
                        {
                            return false;
                        }
                        else
                        {
                            pathesWave.AddLast(new RoomPathelement(prev, room));
                            passedElements.Add(room, pathesWave.Last.Value);

                            return true;
                        }
                    }

                    bool _AddWaveElementWithDoors(RoomPathelement prev, (MapNavigationGraphElement Element, MapDoor[] Doors) room)
                    {
                        if (passedElements.ContainsKey(room.Element))
                        {
                            return false;
                        }
                        else
                        {
                            pathesWave.AddLast(new RoomPathelement(prev, room.Element, room.Doors));
                            passedElements.Add(room.Element, pathesWave.Last.Value);

                            return true;
                        }
                    }

                    void _HandleWaveElement(RoomPathelement element)
                    {
                        if (element.CurrentElement == roomTo)
                        {
                            if (minDistance > element.Distance)
                            {
                                targetSequenceElement = element;
                                minDistance = element.Distance;
                            }
                        }
                        else
                        {
                            foreach (var nextRoom in element.CurrentElement.RoomsTo)
                            {
                                if (element.Distance < minDistance)
                                {
                                    _AddWaveElementWithDoors(element, nextRoom);
                                }
                            }
                        }
                    }

                    _AddWaveElement(null, roomFrom);
                    int iterationsCount = 0;

                    while (pathesWave.Count > 0)
                    {
                        int currentWaveSise = pathesWave.Count;

                        for (int i = 0; i < currentWaveSise; i++)
                        {
                            _HandleWaveElement(pathesWave.First.Value);

                            pathesWave.RemoveFirst();
                        }

                        iterationsCount++;
                        if (layersCount > 0
                            && iterationsCount > layersCount)
                        {
                            break;
                        }
                    }

                    return _GetPathFromSequence().Reverse().ToArray();
                }
            }
        }

        public MapPoint[] FindPathInRoom(MapPoint pointFrom, MapPoint pointTo) => FindPathInRoom(
            NavigationHandler.GetRoom(pointFrom)
            ,
            pointFrom
            ,
            pointTo
            );

        public MapPoint[] FindPathInRoom(MapRoom room, MapPoint pointFrom, MapPoint pointTo)
        {
            if (room is null
                || !(room.DoesRoomContainsPoint(pointFrom) && room.DoesRoomContainsPoint(pointTo))
                || pointFrom.AreEqual(pointTo))
            {
                return Array.Empty<MapPoint>();
            }

            List<MapPoint> resultPoints = new List<MapPoint>();

            bool _FindPathInRoom(MapPoint currentPoint)
            {
                if (!room.DoesRoomContainsPoint(currentPoint)
                    || NavigationHandler.Obstacles[currentPoint]
                    || resultPoints.FirstOrDefault(point => point.AreEqual(currentPoint)) is not null)
                {
                    return false;
                }

                resultPoints.Add(currentPoint);

                if (currentPoint.AreEqual(pointTo))
                {
                    return true;
                }

                int deltaX = Math.Clamp(pointTo.X - currentPoint.X, -1, 1);
                int deltaY = Math.Clamp(pointTo.Y - currentPoint.Y, -1, 1);

                if (deltaX != 0 && deltaY != 0)
                {
                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y + deltaY] && !NavigationHandler.Obstacles[currentPoint.X + deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y + deltaY)))
                        {
                            return true;
                        }
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X, currentPoint.Y + deltaY)))
                    {
                        return true;
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y)))
                    {
                        return true;
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y + deltaY] && !NavigationHandler.Obstacles[currentPoint.X - deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y + deltaY)))
                        {
                            return true;
                        }
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y - deltaY] && !NavigationHandler.Obstacles[currentPoint.X + deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y - deltaY)))
                        {
                            return true;
                        }
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X, currentPoint.Y - deltaY)))
                    {
                        return true;
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y)))
                    {
                        return true;
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y - deltaY] && !NavigationHandler.Obstacles[currentPoint.X - deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y - deltaY)))
                        {
                            return true;
                        }
                    }
                }
                else if (deltaX != 0)
                {
                    deltaY = deltaX;

                    if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y)))
                    {
                        return true;
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y + deltaY] && !NavigationHandler.Obstacles[currentPoint.X + deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y + deltaY)))
                        {
                            return true;
                        }
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y - deltaY] && !NavigationHandler.Obstacles[currentPoint.X + deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y - deltaY)))
                        {
                            return true;
                        }
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X, currentPoint.Y + deltaY)))
                    {
                        return true;
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X, currentPoint.Y - deltaY)))
                    {
                        return true;
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y + deltaY] && !NavigationHandler.Obstacles[currentPoint.X - deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y + deltaY)))
                        {
                            return true;
                        }
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y - deltaY] && !NavigationHandler.Obstacles[currentPoint.X - deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y - deltaY)))
                        {
                            return true;
                        }
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y)))
                    {
                        return true;
                    }
                }
                else if (deltaY != 0)
                {
                    deltaX = deltaY;

                    if (_FindPathInRoom(new MapPoint(currentPoint.X, currentPoint.Y + deltaY)))
                    {
                        return true;
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y + deltaY] && !NavigationHandler.Obstacles[currentPoint.X + deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y + deltaY)))
                        {
                            return true;
                        }
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y + deltaY] && !NavigationHandler.Obstacles[currentPoint.X - deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y + deltaY)))
                        {
                            return true;
                        }
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y)))
                    {
                        return true;
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y)))
                    {
                        return true;
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y - deltaY] && !NavigationHandler.Obstacles[currentPoint.X + deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X + deltaX, currentPoint.Y - deltaY)))
                        {
                            return true;
                        }
                    }

                    if (!NavigationHandler.Obstacles[currentPoint.X, currentPoint.Y - deltaY] && !NavigationHandler.Obstacles[currentPoint.X - deltaX, currentPoint.Y])
                    {
                        if (_FindPathInRoom(new MapPoint(currentPoint.X - deltaX, currentPoint.Y - deltaY)))
                        {
                            return true;
                        }
                    }

                    if (_FindPathInRoom(new MapPoint(currentPoint.X, currentPoint.Y - deltaY)))
                    {
                        return true;
                    }
                }

                resultPoints.RemoveAt(resultPoints.Count - 1);

                return false;
            }

            _FindPathInRoom(pointFrom);

            return resultPoints.ToArray();
        }

        public MapPoint[] FindDirectionInRoom(MapPoint pointFrom, MapPoint pointTo) => FindDirectionInRoom(
            NavigationHandler.GetRoom(pointFrom)
            ,
            pointFrom
            ,
            pointTo
            );

        public MapPoint[] FindDirectionInRoom(MapRoom room, MapPoint pointFrom, MapPoint pointTo)
        {
            if (room is null || !room.DoesRoomContainsPoint(pointFrom))
            {
                return Array.Empty<MapPoint>();
            }

            MapPoint _CheckPoint(MapPoint point)
            {
                if (room.DoesRoomContainsPoint(point)
                    && !NavigationHandler.Obstacles[point])
                {
                    return point;
                }
                else
                {
                    return null;
                }
            }

            int deltaX = Math.Clamp(pointTo.X - pointFrom.X, -1, 1);
            int deltaY = Math.Clamp(pointTo.Y - pointFrom.Y, -1, 1);

            if (deltaX == 0 && deltaY == 0)
            {
                deltaX = 1;
                deltaY = 1;
            }
            else if (deltaX == 0 && deltaY != 0)
            {
                deltaX = deltaY;
            }
            else if (deltaX != 0 && deltaY == 0)
            {
                deltaY = deltaX;
            }

            IEnumerable<MapPoint> _GetNearbyPoints()
            {
                MapPoint point = _CheckPoint(new MapPoint(pointFrom.X - deltaX, pointFrom.Y - deltaY));
                if (point is not null)
                {
                    yield return point;
                }
                point = _CheckPoint(new MapPoint(pointFrom.X - deltaX, pointFrom.Y));
                if (point is not null)
                {
                    yield return point;
                }
                point = _CheckPoint(new MapPoint(pointFrom.X, pointFrom.Y - deltaY));
                if (point is not null)
                {
                    yield return point;
                }
                point = _CheckPoint(new MapPoint(pointFrom.X - deltaX, pointFrom.Y + deltaY));
                if (point is not null)
                {
                    yield return point;
                }
                point = _CheckPoint(new MapPoint(pointFrom.X + deltaX, pointFrom.Y - deltaY));
                if (point is not null)
                {
                    yield return point;
                }
                point = _CheckPoint(new MapPoint(pointFrom.X + deltaX, pointFrom.Y));
                if (point is not null)
                {
                    yield return point;
                }
                point = _CheckPoint(new MapPoint(pointFrom.X, pointFrom.Y + deltaY));
                if (point is not null)
                {
                    yield return point;
                }
                point = _CheckPoint(new MapPoint(pointFrom.X + deltaX, pointFrom.Y + deltaY));
                if (point is not null)
                {
                    yield return point;
                }
                point = _CheckPoint(new MapPoint(pointFrom.X, pointFrom.Y));
                if (point is not null)
                {
                    yield return point;
                }
            }

            return _GetNearbyPoints().ToArray();
        }

        public MapRoom[] GetFathestRecursive()
        {
            (MapNavigationGraphElement[] path, double distance) resultPath = (Array.Empty<MapNavigationGraphElement>(), 0);

            bool _DistanceCounter(MapNavigationGraphElement currentElement, List<MapNavigationGraphElement> path, double accumulatedDistance)
            {
                if (path.Contains(currentElement))
                {
                    return false;
                }
                else
                {
                    path.Add(currentElement);
                    var currentRoomCenter = MapPointOperations.GetRoomCenter(currentElement.Room);

                    bool theNextRoomExists = false;

                    foreach (var nextElement in currentElement.RoomsTo.GroupBy(element => element.Element).Select(pair => pair.Key))
                    {
                        var distance = MapPointOperations.GetDistance(currentRoomCenter, MapPointOperations.GetRoomCenter(nextElement.Room));
                        accumulatedDistance += distance;

                        if (_DistanceCounter(nextElement, path, accumulatedDistance))
                        {
                            theNextRoomExists = true;
                        }

                        accumulatedDistance -= distance;
                    }

                    if (!theNextRoomExists)
                    {
                        if (accumulatedDistance > resultPath.distance)
                        {
                            resultPath = (path.ToArray(), accumulatedDistance);
                        }
                    }

                    path.RemoveAt(path.Count - 1);

                    return theNextRoomExists;
                }
            }

            foreach (var room in Nodes)
            {
                _DistanceCounter(room, new(), 0.0);
            }

            if (resultPath.distance > 0)
            {
                return new MapRoom[]
                {
                    resultPath.path.First().Room
                    ,
                    resultPath.path.Last().Room
                };
            }
            else
            {
                return Array.Empty<MapRoom>();
            }
        }

        public MapNavigationGraphPath? GetFathestRoomsByPathes()
        {
            MapNavigationGraphPath? resultPath = null;

            foreach (var nodeFrom in Nodes)
            {
                var pointFrom = MapPointOperations.GetRoomCenter(nodeFrom.Room);

                foreach (var nodeTo in Nodes)
                {
                    var pointTo = MapPointOperations.GetRoomCenter(nodeTo.Room);

                    var pathes = GetPathes(pointFrom, pointTo);

                    if (pathes.Length > 0)
                    {
                        var foundPath = pathes.First();

                        if (resultPath is null
                            || resultPath.Distance < foundPath.Distance)
                        {
                            resultPath = foundPath;
                        }
                    }
                }
            }

            return resultPath;
        }
    }
}

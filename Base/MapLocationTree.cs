using MapGenearionLibrary.Base;
using MapGenerationLibrary.Enums;
using MapGenerationLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerationLibrary.Base
{
    public class MapLocationTree<TValue> : IEnumerable<TValue>
        where TValue : class
    {
        public class LocationTreeElement : ILocatiable
        {
            internal LocationTreeElement[] Leaves { get; } = new LocationTreeElement[2];

            public LocationTreeElement Parent { get; internal set; } = null;

            public LocationTreeElement LeaveSmaller { get => Leaves[0]; internal set => Leaves[0] = value; }

            public LocationTreeElement LeaveBigger { get => Leaves[1]; internal set => Leaves[1] = value; }

            public MapPoint StartLocation { get; } = new();

            public MapPoint Size { get; } = new();

            public MapPoint MinLocation { get; } = new(int.MaxValue, int.MaxValue);

            public MapPoint MaxLocation { get; } = new(int.MinValue, int.MinValue);

            public TValue Value { get; internal set; } = default(TValue);

            public SeperationParameterEnum Parameter { get; }

            public LocationTreeElement(MapPoint point, MapPoint size, SeperationParameterEnum parameter, TValue value)
            {
                point.CopyTo(StartLocation);
                size.CopyTo(Size);
                Parameter = parameter;
                Value = value;
            }

            public bool DoesContainsPoint(MapPoint point)
            {
                return point.X >= MinLocation.X && point.X <= MaxLocation.X
                       && point.Y >= MinLocation.Y && point.Y < MaxLocation.Y;
            }
        }

        public LocationTreeElement Head { get; private set; } = null;

        public TValue[] Values { get; private set; } = Array.Empty<TValue>();

        public TValue this[int index]
        {
            get => Values[index];
        }

        public int Count => Values.Length;

        internal MapLocationTree() { }

        public TValue GetValue(MapPoint point)
        {
            var currentElement = Head;

            while (currentElement is not null)
            {
                if (currentElement.DoesLocatibleContainsPoint(point))
                {
                    return currentElement.Value;
                }
                else if (currentElement.LeaveBigger is not null
                         && currentElement.IsLocatibleBigger(point, currentElement.Parameter)
                         && currentElement.LeaveBigger.DoesContainsPoint(point))
                {
                    currentElement = currentElement.LeaveBigger;
                }
                else if (currentElement.LeaveSmaller is not null
                         && currentElement.LeaveSmaller.DoesContainsPoint(point))
                {
                    currentElement = currentElement.LeaveSmaller;
                }
                else
                {
                    break;
                }
            }

            return null;
        }

        public static MapLocationTree<TValue> CreateTree(MapSeperationAreaIterator iterator, Func<MapSeperationArea, int, TValue> valueCreatingDelegate)
        {
            if (iterator is not null)
            {
                MapLocationTree<TValue> newTree = new();
                LinkedList<TValue> createdObjects = new();

                IEnumerable<MapSeperationArea> _GetRoomsFromIterator(MapSeperationAreaIterator iterator)
                {
                    var middleIndex = iterator.RoomsAreas.Length / 2;

                    yield return iterator.RoomsAreas[middleIndex];

                    for (int i = 1; i < middleIndex; i++)
                    {
                        yield return iterator.RoomsAreas[middleIndex - i];
                        yield return iterator.RoomsAreas[middleIndex + i];
                    }

                    yield return iterator.RoomsAreas[0];

                    if ((iterator.RoomsAreas.Length % 2) == 1)
                    {
                        yield return iterator.RoomsAreas[iterator.RoomsAreas.Length - 1];
                    }
                }

                (LocationTreeElement Parent, int LeaveIndex, SeperationParameterEnum Parameter) _FindElementLocation(MapSeperationArea area)
                {
                    if (newTree.Head is null)
                    {
                        return (null, 0, SeperationParameterEnum.None);
                    }
                    else
                    {
                        var currentElement = newTree.Head;
                        var parameter = newTree.Head.Parameter;
                        var leaveIndex = 0;

                        while (currentElement is not null)
                        {
                            if (currentElement.IsLocatibleBigger(area, currentElement.Parameter))
                            {
                                leaveIndex = 1;
                            }
                            else
                            {
                                leaveIndex = 0;
                            }

                            if (currentElement.Leaves[leaveIndex] is not null)
                            {
                                currentElement = currentElement.Leaves[leaveIndex];
                            }
                            else
                            {
                                break;
                            }
                        }

                        return (currentElement, leaveIndex, (currentElement.Parameter == SeperationParameterEnum.X ? SeperationParameterEnum.Y : SeperationParameterEnum.X));
                    }
                }

                void _ApdateLeavesSize(LocationTreeElement element)
                {
                    var currentElement = element;

                    int minLocationX = element.StartLocation.X;
                    int minLocationY = element.StartLocation.Y;
                    int maxLocationX = element.StartLocation.X + element.Size.X - 1;
                    int maxLocationY = element.StartLocation.Y + element.Size.Y - 1;

                    while (element is not null)
                    {
                        if (element.MinLocation.X > minLocationX)
                        {
                            element.MinLocation.SetX(minLocationX);
                        }

                        if (element.MaxLocation.X < maxLocationX)
                        {
                            element.MaxLocation.SetX(maxLocationX);
                        }

                        if (element.MinLocation.Y > minLocationY)
                        {
                            element.MinLocation.SetY(minLocationY);
                        }

                        if (element.MaxLocation.Y < maxLocationY)
                        {
                            element.MaxLocation.SetY(maxLocationY);
                        }

                        element = element.Parent;
                    }
                }

                void _AppendArea(MapSeperationArea area)
                {
                    var areaParent = _FindElementLocation(area);
                    TValue valueObject = default(TValue);

                    if (area.SeperationParameter == SeperationParameterEnum.None)
                    {
                        valueObject = valueCreatingDelegate?.Invoke(area, createdObjects.Count);
                        createdObjects.AddLast(valueObject);
                    }

                    if (areaParent.Parent is null)
                    {
                        newTree.Head = new LocationTreeElement(area.StartLocation, area.Size, area.Parent.SeperationParameter, valueObject);

                        _ApdateLeavesSize(newTree.Head);
                    }
                    else
                    {

                        areaParent.Parent.Leaves[areaParent.LeaveIndex] = new LocationTreeElement(area.StartLocation, area.Size, areaParent.Parameter, valueObject);
                        areaParent.Parent.Leaves[areaParent.LeaveIndex].Parent = areaParent.Parent;

                        _ApdateLeavesSize(areaParent.Parent.Leaves[areaParent.LeaveIndex]);
                    }
                }

                foreach (var roomArea in _GetRoomsFromIterator(iterator))
                {
                    _AppendArea(roomArea);
                }

                newTree.Values = createdObjects.ToArray();

                return newTree;
            }
            else
            {
                return null;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var value in Values)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            List<StringBuilder> lines = new();

            void _ValueAdder(int lineIndex, int parentIndex, LocationTreeElement element, bool isBigger)
            {
                var value = $"[{lineIndex},{parentIndex}, {(isBigger ? "B" : "S")}, {element.Parameter}]<{element.StartLocation},{element.Size}>";

                if (lineIndex == lines.Count)
                {
                    lines.Add(new());
                }

                lines[lineIndex].Append(value);

                if (element.LeaveBigger is not null)
                {
                    _ValueAdder(lineIndex + 1, parentIndex * 2, element.LeaveBigger, true);
                }

                if (element.LeaveSmaller is not null)
                {
                    _ValueAdder(lineIndex + 1, parentIndex * 2 + 1, element.LeaveSmaller, false);
                }
            }

            if (Head is not null)
            {
                _ValueAdder(0, 0, Head, false);
            }

            foreach (var line in lines)
            {
                sb.AppendLine(line.ToString());
            }

            return sb.ToString();
        }
    }
}

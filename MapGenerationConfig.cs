using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenearionLibrary
{
    public class MapGenerationConfig
    {
        public int Width { get; set; } = 0;

        public int Height { get; set; } = 0;

        public int MaxLayerCount { get; set; } = -1;

        public int MinRoomWidth { get; set; } = -1;

        public int MinRoomHeight { get; set; } = -1;

        public int MaxDoorsCount { get; set; } = -1;

        public bool IsRandomDoorsCount { get; set; } = false;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapMaker;
using System.IO;

namespace Maker
{
    class Program
    {
        static int MapSize = 20;
        static void Main(string[] args)
        {
            var mapMaker = new MapMakerObj(MapSize, MapSize);
            var map = mapMaker.GetMapSpeciallyForNastya();
            File.WriteAllLines("../Server/input.txt", map.Map);
            File.WriteAllLines("../Server/config.xml", map.PlayersXml);
        }
    }
}

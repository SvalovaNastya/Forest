using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;

namespace MapMaker
{
	[TestFixture]
	class MapMakerShould
	{
		[Test]
		public void ReturnMap()
		{
			const int height = 20;
			const int width = 20;
			var rand = new Random();//1545806551
			var seed = rand.Next();
			var mapMaker = new MapMakerObj(height, width, seed) { Debug = true };
			var map = mapMaker.GetMap();
			for (var y = 0; y < height + 3; y++)
			{
				Console.WriteLine(map[y]);
			}
			Console.WriteLine();
			Console.WriteLine("Seed: " + seed);
		}

		[Test]
		public void ReturnMapForNastya()
		{
			const int height = 20;
			const int width = 20;
			var rand = new Random();
			var seed = rand.Next();
			var mapMaker = new MapMakerObj(height, width, 1545806551) { Debug = false };
			var map = mapMaker.GetMapSpeciallyForNastya();
			foreach (var s in map.Map)
			{
				Console.WriteLine(s);
			}
			Console.WriteLine();
			foreach (var s in map.PlayersXml)
			{
				Console.WriteLine(s);
			}
			Console.WriteLine();
			Console.WriteLine("Seed: " + seed);
		}
	}
}

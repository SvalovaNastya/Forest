using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MapMaker
{
	public enum Cell
	{
		Wall = 0,
		Road,
		Trap,
		Health,
		Hero,
		Finish
	}

	public class MapMakerAnswer
	{
		public string[] Map { get; private set; }
		public string[] PlayersXml { get; private set; }

		public MapMakerAnswer(string[] map, string[] playersXml)
		{
			Map = map;
			PlayersXml = playersXml;
		}
	}


	[XmlRoot(Namespace = "http://localhost", IsNullable = false)]
	public class Config
	{
		public string Filename;
		[XmlArrayAttribute("players")]
		public ConfigPoints[] Points;

		public int FogOfWar;
	}

	public class ConfigPoints
	{
		public int StartPointX;
		public int StartPointY;
		public int TargetX;
		public int TargetY;
		public int Hp;
	}

	public class MapMakerObj
	{
		private readonly int height;
		private readonly int width;
		private readonly Random random;

		private const int ComponentWeight = 50;
		private const int TrapCoefficient = 10;
		private const char DebugWall = '█';
		private const char DebugRoad = ' ';
		private const char DebugTrap = '#';
		private const char DebugHealth = '♥';

		private const char ReleaseWall = '1';
		private const char ReleaseRoad = '0';
		private const char ReleaseTrap = 'K';
		private const char ReleaseHealth = 'L';

		private char wall = '1';
		private char road = '0';
		private char trap = 'K';
		private char health = 'L';

		private readonly Player player1 = new Player();
		private readonly Player player2 = new Player();

		private bool debug;

		public bool Debug
		{
			get { return debug; }
			set { ApplyDebug(value); }
		}

		private void ApplyDebug(bool value)
		{
			if (value)
			{
				wall = DebugWall;
				road = DebugRoad;
				trap = DebugTrap;
				health = DebugHealth;
			}
			else
			{
				wall = ReleaseWall;
				road = ReleaseRoad;
				trap = ReleaseTrap;
				health = ReleaseHealth;
			}
			debug = value;
		}

		public MapMakerObj(int height, int width)
		{
			debug = false;
			random = new Random();
			this.width = width;
			this.height = height;
		}

		public MapMakerObj(int height, int width, int seed)
			: this(height, width)
		{
			random = new Random(seed);
		}

		public MapMakerAnswer GetMapSpeciallyForNastya()
		{
			var mapDescription = GetMap();
			var map = mapDescription.Take(mapDescription.Length - 3).ToArray();
			var players = mapDescription
				.Skip(map.Length)
				.Take(2)
				.Select(x => x.Split(' ')
					.Select(int.Parse)
					.ToArray())
				.ToArray();
			var fogOfWar = int.Parse(mapDescription.Last());
			var config = GetConfig(fogOfWar, players);
			var serializer = new XmlSerializer(typeof (Config));
			var stringWritter = new StringWriter();
			serializer.Serialize(stringWritter, config);

			return new MapMakerAnswer(map, stringWritter.GetStringBuilder().ToString().Split('\n'));
		}

		private static Config GetConfig(int fogOfWar, IReadOnlyList<int[]> players)
		{
			var config = new Config
			{
				FogOfWar = fogOfWar,
				Points = new[]
				{
					new ConfigPoints
					{
						Hp = players[0][4],
						StartPointX = players[0][0],
						StartPointY = players[0][1],
						TargetX = players[0][2],
						TargetY = players[0][3]
					},
					new ConfigPoints
					{
						Hp = players[1][4],
						StartPointX = players[1][0],
						StartPointY = players[1][1],
						TargetX = players[1][2],
						TargetY = players[1][3]
					}
				}
			};
			return config;
		}

		public string[] GetMap()
		{
			while (true)
			{
				try
				{
					var localHeight = height - 2;
					var localWidth = width - 2;
					var map = GetDefaultMap(localHeight, localWidth);
					map[new Point(random.Next(localHeight), random.Next(localWidth))] = Cell.Road;
					map = CreateWorldLikeGod(map);
					CreatePlayers(map);
					map = CreateTraps(map);
					map = PutHealth(map);
					var charMap = GetCharMap(localHeight, localWidth, map);
					charMap = ToPrettyChar(charMap);
					return ToStringMap(charMap);
				}
				catch (Exception)
				{
					// ignored
				}
			}
		}

		private string[] ToStringMap(char[,] charMap)
		{
			var answer = new string[height + 3];
			for (var i = 0; i < height; i++)
			{
				var builder = new StringBuilder();
				for (int j = 0; j < width; j++)
					builder.Append(charMap[i, j]);
				answer[i] = builder.ToString();
			}
			answer[height] = string.Format("{0} {1} {2} {3} {4}", player1.Start.X + 1, player1.Start.Y + 1, player1.Destination.X + 1,
				player1.Destination.Y + 1, player1.Hp);
			answer[height + 1] = string.Format("{0} {1} {2} {3} {4}", player2.Start.X + 1, player2.Start.Y + 1, player2.Destination.X + 1,
				player2.Destination.Y + 1, player2.Hp);
			answer[height + 2] = Math.Min(Math.Max((width + height) / 20, 1) + 1, 4).ToString();
			return answer;
		}

		private Dictionary<Point, Cell> PutHealth(Dictionary<Point, Cell> map)
		{
			var gameMod = random.Next(2);
			if (gameMod == 0)
				return SurvivalMoge(map);
			if (gameMod == 1)
				return SurvivalMoge(map);//TODO: do this tank mod, lazy idiot!
			return map;
		}

		private Dictionary<Point, Cell> TankMode(Dictionary<Point, Cell> map)
		{
			return map;
		}

		private Dictionary<Point, Cell> SurvivalMoge(Dictionary<Point, Cell> map)
		{
			player1.Hp = Math.Max(2, (width + height) / 20);
			player2.Hp = Math.Max(2, (width + height) / 20);
			PutSurvivalHealth(map);
			return map;
		}

		private void PutSurvivalHealth(Dictionary<Point, Cell> map)
		{
			var allEmptyes = new HashSet<Point>(map
				.Where(x => x.Value == Cell.Road)
				.Select(x => x.Key));
			var components = new List<HashSet<Point>>();
			while (allEmptyes.Count > 0)
			{
				var component = GetConnectedComponent(map, allEmptyes.First(), new HashSet<Cell> { Cell.Road, Cell.Health });
				foreach (var point in component)
					allEmptyes.Remove(point);
				components.Add(component);
			}

			foreach (var component in components)
			{
				var points = component.ToList();
				var healthPoint = points[random.Next(points.Count)];
				map[healthPoint] = Cell.Health;
				points.Remove(healthPoint);
				if (points.Count == 0)
					continue;

				healthPoint = points[random.Next(points.Count)];
				map[healthPoint] = Cell.Health;
				points.Remove(healthPoint);
			}

			//foreach (var point in components
			//	.Select(component => component.ToList())
			//	.Select(cells => cells[random.Next(cells.Count)]))
			//{
			//	map[point] = Cell.Health;
			//}
			//foreach (var point in components
			//	.Select(component => component.ToList())
			//	.Select(cells => cells[random.Next(cells.Count)]))
			//{
			//	map[point] = Cell.Health;
			//}
			return;
			//var curHp = player.Hp;
			//var cellForMove = new HashSet<Cell>
			//{
			//	Cell.Finish,
			//	Cell.Hero,
			//	Cell.Road
			//};
			//
			//var queue = new Queue<Point>();
			//var visited = new HashSet<Point> { player.Start };
			//var visitedTrap = new HashSet<Point>();
			//var healths = new HashSet<Point>();
			//queue.Enqueue(player.Start);
			//while (true)
			//{
			//	bool isFinish = false;
			//	while (queue.Count > 0)
			//	{
			//		var current = queue.Dequeue();
			//		var neighbours = GetNeighbours(current).ToList();
			//		foreach (var cell in neighbours.Where(x => cellForMove.Contains(map[x]) && !visited.Contains(x)))
			//		{
			//			if (map[cell] == Cell.Finish)
			//			{
			//				isFinish = true;
			//				break;
			//			}
			//			queue.Enqueue(cell);
			//			visited.Add(cell);
			//		}
			//		foreach (var cell in neighbours.Where(x => map[x] == Cell.Trap && !visitedTrap.Contains(x)))
			//		{
			//			visitedTrap.Add(cell);
			//		}
			//	}
			//	if (isFinish)
			//		break;
			//	if (curHp > 1)
			//	{
			//		var num = random.Next();
			//		if (num%2 == 0)
			//		{
			//			if (!PutOneHealth(visited, healths, map))
			//				curHp++;
			//		}
			//		else
			//			curHp--;
			//	}
			//	else
			//	{
			//		if (!PutOneHealth(visited, healths, map))
			//			curHp++;
			//	}
			//	foreach (var trap in visitedTrap)
			//	{
			//		queue.Enqueue(trap);
			//		visited.Add(trap);
			//	}
			//}
			//foreach (var health in healths)
			//	map[health] = Cell.Health;
		}

		private bool PutOneHealth(HashSet<Point> visited, HashSet<Point> healths, Dictionary<Point, Cell> map)
		{
			var emptyes = visited.Where(x => !healths.Contains(x) && map[x] != Cell.Hero).ToList();
			if (emptyes.Count == 0)
				return false;
			var index = random.Next(emptyes.Count);
			healths.Add(emptyes[index]);
			return true;
		}

		private Dictionary<Point, Cell> CreateTraps(Dictionary<Point, Cell> map)
		{
			var emptyCells = map.Where(x => x.Value == Cell.Road).ToList();

			var trapCount = emptyCells.Count / TrapCoefficient;

			for (int i = 0; i < trapCount; i++)
			{
				var point = emptyCells[random.Next(emptyCells.Count)].Key;
				while (GetNeighbours(point).Any(x => map[x] == Cell.Trap))
					point = emptyCells[random.Next(emptyCells.Count)].Key;
				map[point] = Cell.Trap;
			}

			return map;
		}

		private void CreatePlayers(Dictionary<Point, Cell> map)
		{
			for (var i = 0; i < 50; i++)
			{
				try
				{
					SetPlayers(map);
					break;
				}
				catch
				{
				}
			}
		}

		private void SetPlayers(Dictionary<Point, Cell> map)
		{
			var emptyes = map.Keys.ToList();
			var finishPlace = SelectFinishPlace(emptyes);
			var pathes = GetPathMap(map, finishPlace, Cell.Road);
			var maximumLength = pathes.Keys.Max();
			var minimumPossibleLength = Math.Max(1, (maximumLength * 1) / 2);
			var length1 = -1;
			var length2 = -1;
			int max = 0;
			int index = -1;
			for (var i = maximumLength; i >= minimumPossibleLength; i--)
			{
				if (pathes[i].Count >= 3)
				{
					max = pathes[i].Count;
					index = i;
					break;
				}
				if (pathes[i].Count > max)
				{
					max = pathes[i].Count;
					index = i;
				}
			}
			//for (var i = minimumPossibleLength; i <= maximumLength; i++)
			//	if (pathes[i].Count >= 2)
			//	{
			//		length1 = i;
			//		length2 = i;
			//		break;
			//	}
			//if (length2 <= 0)
			//	throw new Exception("Cant put players");
			var point1 = pathes[index][random.Next(pathes[index].Count)];
			var point2 = pathes[index][random.Next(pathes[index].Count)];
			//var point1 = pathes[length1][random.Next(pathes[length1].Count)];
			//var point2 = pathes[length2][random.Next(pathes[length2].Count)];

			if (max == 1)
			{
				point1 = pathes[maximumLength][0];
				point2 = pathes[maximumLength - 1][0];
			}
			while (point1.Equals(point2))
				point2 = pathes[index][random.Next(pathes[index].Count)];
			player1.Start = point1;
			player2.Start = point2;
			player1.Destination = finishPlace;
			player2.Destination = finishPlace;
			map[player1.Start] = Cell.Hero;
			map[player2.Start] = Cell.Hero;
			map[player1.Destination] = Cell.Finish;
		}

		private Point SelectFinishPlace(List<Point> emptyes)
		{
			var cells = new List<Point>();
			for (int i = 0; i < (width + height) / 2; i++)
				cells.Add(emptyes[random.Next(emptyes.Count)]);
			int minLength = 2 * (width + height);
			int index = -1;
			for (int i = 0; i < cells.Count; i++)
			{
				var length = Math.Min(cells[i].X, width - 2 - cells[i].X) + Math.Min(cells[i].Y, height - 2 - cells[i].Y);
				if (length < minLength)
				{
					minLength = length;
					index = i;
				}
			}
			return cells[index];
		}

		private Dictionary<Point, Cell> CreateWorldLikeGod(Dictionary<Point, Cell> map)
		{
			var mapMode = random.Next();
			if (mapMode % 3 > 0)
			{
				map = Kraskal(map);
				if (mapMode % 3 == 1)
				{
					map = SplitOnComponents(map);
					map = FillComponents(map);
					map = UnionComponents(map);
					map = ReplaceBigWallMassive(map);
				}
				else
				{
					map = ReplaceRandomWall(map);
				}
			}
			else
			{
				map = RectangleRooms(map);
				map = FillComponents(map);
				map = UnionComponents(map);
			}
			return map;
		}

		private Dictionary<Point, Cell> RectangleRooms(Dictionary<Point, Cell> map)
		{
			foreach (var cell in map.Keys.ToList())
			{
				map[cell] = Cell.Road;
			}
			var emptyes = new HashSet<Point>(map.Where(x => x.Value == Cell.Road).Select(x => x.Key));
			var centerCount = ((height - 2) * (width - 2) / ComponentWeight) / 2;
			for (int i = 0; i < centerCount; i++)
			{
				var emptyesList = emptyes.ToList();
				var emptyPlace = emptyesList[random.Next(emptyesList.Count)];
				//while (GetNeighbours(emptyPlace.Key).Any(x => map[x] != Cell.Road))
				//	emptyPlace = emptyes[random.Next(emptyes.Count)];
				foreach (var wall in GrowRoom(emptyPlace, map))
					emptyes.Remove(wall);
			}
			return map;
		}

		private HashSet<Point> GrowRoom(Point emptyPlace, Dictionary<Point, Cell> map)
		{
			var walls = new HashSet<Point>();

			for (int i = emptyPlace.X - 1; i >= 0; i--)
			{
				var point = new Point(emptyPlace.Y, i);
				if (map[point] == Cell.Wall)
					break;
				map[point] = Cell.Wall;
				foreach (var neighbour in GetNeighbours(point))
					walls.Add(neighbour);
			}
			for (int i = emptyPlace.X + 1; i < width - 2; i++)
			{
				var point = new Point(emptyPlace.Y, i);
				if (map[point] == Cell.Wall)
					break;
				map[point] = Cell.Wall;
				foreach (var neighbour in GetNeighbours(point))
					walls.Add(neighbour);
			}

			for (int i = emptyPlace.Y - 1; i >= 0; i--)
			{
				var point = new Point(i, emptyPlace.X);
				if (map[point] == Cell.Wall)
					break;
				map[point] = Cell.Wall;
				foreach (var neighbour in GetNeighbours(point))
					walls.Add(neighbour);
			}
			for (int i = emptyPlace.Y + 1; i < height - 2; i++)
			{
				var point = new Point(i, emptyPlace.X);
				if (map[point] == Cell.Wall)
					break;
				map[point] = Cell.Wall;
				foreach (var neighbour in GetNeighbours(point))
					walls.Add(neighbour);
			}

			map[emptyPlace] = Cell.Wall;
			return walls;
		}

		private Dictionary<Point, Cell> ReplaceRandomWall(Dictionary<Point, Cell> map)
		{
			var walls = map.Where(x => x.Value == Cell.Wall).ToList();
			var replacedCount = 2 * ((height - 2) * (width - 2) / ComponentWeight);
			for (var i = 0; i < replacedCount; i++)
			{
				map[walls[random.Next(walls.Count)].Key] = Cell.Road;
			}
			return map;
		}

		private Dictionary<Point, Cell> ReplaceBigWallMassive(Dictionary<Point, Cell> map)
		{
			var allWalls = new HashSet<Point>(map.Where(x => x.Value == Cell.Wall).Select(x => x.Key));
			var components = new List<HashSet<Point>>();
			while (allWalls.Count > 0)
			{
				var component = GetConnectedComponent(map, allWalls.First(), new HashSet<Cell> { Cell.Wall });
				foreach (var point in component)
					allWalls.Remove(point);
				components.Add(component);
			}
			foreach (var component in components)
			{
				var border = new List<Point>();
				foreach (var point in component.ToList()
					.Where(point => GetNeighbours(point).Any(x => map[x] == Cell.Road)))
				{
					component.Remove(point);
					border.Add(point);
				}
				if (component.Count <= 4)
					continue;
				var mapPiece = new Dictionary<Point, Cell>();
				foreach (var point in component)
					mapPiece[point] = Cell.Wall;
				mapPiece[component.First()] = Cell.Road;
				mapPiece = Kraskal(mapPiece);
				foreach (var point in mapPiece)
					map[point.Key] = point.Value;
				map[border[random.Next(border.Count)]] = Cell.Road;
			}
			return map;
		}

		private Dictionary<Point, Cell> FillComponents(Dictionary<Point, Cell> map)
		{
			var allEmptyes = new HashSet<Point>(map.Where(x => x.Value == Cell.Road).Select(x => x.Key));
			var components = new List<HashSet<Point>>();
			while (allEmptyes.Count > 0)
			{
				var component = GetConnectedComponent(map, allEmptyes.First(), new HashSet<Cell> { Cell.Road });
				foreach (var point in component)
					allEmptyes.Remove(point);
				components.Add(component);
			}
			foreach (var component in components)
			{
				var mapPiese = new Dictionary<Point, Cell>();
				foreach (var point in component)
					mapPiese[point] = Cell.Wall;
				mapPiese[component.First()] = Cell.Road;
				mapPiese = Kraskal(mapPiese);
				foreach (var pair in mapPiese)
					map[pair.Key] = pair.Value;
			}
			return map;
		}

		private Dictionary<Point, Cell> UnionComponents(Dictionary<Point, Cell> map)
		{
			var allEmptyes = new HashSet<Point>(map.Where(x => x.Value == Cell.Road).Select(x => x.Key));
			var components = new List<HashSet<Point>>();
			while (allEmptyes.Count > 0)
			{
				var component = GetConnectedComponent(map, allEmptyes.First(), new HashSet<Cell> { Cell.Road });
				foreach (var point in component)
					allEmptyes.Remove(point);
				components.Add(component);
			}
			var cellsOwner = new Dictionary<Point, int>();
			for (var i = 0; i < components.Count; i++)
				foreach (var point in components[i])
					cellsOwner[point] = i;
			var wallForRemove = new List<Point>();
			foreach (var component in components)
			{
				var owner = cellsOwner[component.First()];
				var border = component
					.SelectMany(x => GetNeighbours(x)
						.Where(y => map[y] == Cell.Wall)
						.Where(y => GetNeighbours(y)
							.Where(z => map[z] == Cell.Road)
							.Any(z => cellsOwner[z] != owner)))
					.GroupBy(x => GetNeighbours(x)
						.Where(y => map[y] == Cell.Road)
						.Select(y => cellsOwner[y])
						.First(y => y != owner))
					.Select(x => new KeyValuePair<int, List<Point>>(x.Key, x.ToList()));
				wallForRemove.AddRange(border.Select(walls => walls.Value[random.Next(walls.Value.Count)]));
			}
			var componentsPair = new HashSet<Tuple<int, int>>();
			foreach (var wall in wallForRemove)
			{
				var owners = GetNeighbours(wall)
					.Where(y => map[y] == Cell.Road)
					.Where(cellsOwner.ContainsKey)
					.Select(y => cellsOwner[y])
					.ToList();
				var counter = 0;
				for (int i = 0; i < owners.Count - 1; i++)
				{
					if (componentsPair.Contains(new Tuple<int, int>(owners[i], owners[i + 1])))
						continue;
					counter++;
					componentsPair.Add(new Tuple<int, int>(owners[i], owners[i + 1]));
					componentsPair.Add(new Tuple<int, int>(owners[i + 1], owners[i]));
				}
				if (counter > 0)
					map[wall] = Cell.Road;
			}
			return map;
		}

		private Dictionary<int, List<Point>> GetPathMap(Dictionary<Point, Cell> map, Point startPoint, Cell cellType)
		{
			var queue = new Queue<Tuple<Point, int>>();
			var visited = new HashSet<Point> { startPoint };
			queue.Enqueue(new Tuple<Point, int>(startPoint, 0));
			var answer = new Dictionary<int, List<Point>>();
			answer[0] = new List<Point> { startPoint };
			while (queue.Count > 0)
			{
				var current = queue.Dequeue();
				foreach (var cell in GetNeighbours(current.Item1).Where(x => map[x] == cellType && !visited.Contains(x)))
				{
					queue.Enqueue(new Tuple<Point, int>(cell, current.Item2 + 1));
					if (!answer.ContainsKey(current.Item2 + 1))
						answer[current.Item2 + 1] = new List<Point>();
					answer[current.Item2 + 1].Add(cell);
					visited.Add(cell);
				}
			}
			return answer;
		}

		private HashSet<Point> GetConnectedComponent(Dictionary<Point, Cell> map, Point startPoint, HashSet<Cell> cellTypes)
		{
			var queue = new Queue<Point>();
			var visited = new HashSet<Point> { startPoint };
			queue.Enqueue(startPoint);
			while (queue.Count > 0)
			{
				var current = queue.Dequeue();
				foreach (var cell in GetNeighbours(current).Where(x => cellTypes.Contains(map[x]) && !visited.Contains(x)))
				{
					queue.Enqueue(cell);
					visited.Add(cell);
				}
			}
			return visited;
		}

		private Dictionary<Point, Cell> SplitOnComponents(Dictionary<Point, Cell> map)
		{
			var allEmptyes = GetConnectedComponent(map, map.First(x => x.Value == Cell.Road).Key, new HashSet<Cell> { Cell.Road });
			var forSplit = allEmptyes.ToList();
			var componentCount = (height - 2) * (width - 2) / ComponentWeight;
			for (var i = 0; i < componentCount; i++)
			{
				var splitter = forSplit[random.Next(forSplit.Count)];
				map[splitter] = Cell.Wall;
				allEmptyes.Remove(splitter);
			}
			var components = new List<HashSet<Point>>();
			while (allEmptyes.Count > 0)
			{
				var component = GetConnectedComponent(map, allEmptyes.First(), new HashSet<Cell> { Cell.Road });
				foreach (var point in component)
					allEmptyes.Remove(point);
				components.Add(component);
			}
			foreach (var component in components)
				foreach (var neighbour in component.ToList()
					.SelectMany(point => GetDiagonalNeighbours(point).Concat(GetNeighbours(point))
						.Where(x => map[x] == Cell.Wall)
						.Where(x => GetNeighbours(x)
							.Where(y => map[y] == Cell.Road)
							.All(y => component.Contains(y)))))
				{
					map[neighbour] = Cell.Road;
					component.Add(neighbour);
				}
			return map;
		}


		private static Dictionary<Point, Cell> GetDefaultMap(int localHeight, int localWidth)
		{
			var map = new Dictionary<Point, Cell>();
			for (var y = 0; y < localHeight; y++)
				for (var x = 0; x < localWidth; x++)
					map[new Point(y, x)] = Cell.Wall;
			return map;
		}

		private char[,] GetCharMap(int localHeight, int localWidth, IReadOnlyDictionary<Point, Cell> map)
		{
			var charMap = new Char[height, width];
			for (var y = 0; y < localHeight; y++)
				for (var x = 0; x < localWidth; x++)
					charMap[y + 1, x + 1] = map[new Point(y, x)] == Cell.Wall
						? wall
						: map[new Point(y, x)] == Cell.Road
							? road
							: map[new Point(y, x)] == Cell.Trap
								? trap
								: health;
			if (debug)
			{
				charMap[player1.Destination.Y + 1, player1.Destination.X + 1] = '*';
				charMap[player1.Start.Y + 1, player1.Start.X + 1] = '☺';
				charMap[player2.Start.Y + 1, player2.Start.X + 1] = '☺';
			}
			return charMap;
		}

		private char[,] ToPrettyChar(char[,] charMap)
		{
			for (var y = 0; y < height; y++)
			{
				charMap[y, 0] = wall;
				charMap[y, width - 1] = wall;
			}
			for (var x = 0; x < width; x++)
			{
				charMap[0, x] = wall;
				charMap[height - 1, x] = wall;
			}
			return charMap;
		}

		private Dictionary<Point, Cell> Kraskal(Dictionary<Point, Cell> map)
		{
			var generatorCells = new HashSet<Point>(map.Where(x => x.Value == Cell.Road).Select(x => x.Key));
			while (generatorCells.Count() != 0)
			{
				var randomIndex = random.Next(generatorCells.Count());
				var cell = generatorCells.Where((x, i) => i == randomIndex).First();
				var neighbours = GetNeighbours(cell).Where(map.ContainsKey)
					.Where(x => map[x] == Cell.Wall)
					.Where(x => GetNeighbours(x).Where(map.ContainsKey).Count(y => map[y] == Cell.Road) == 1)
					.ToList();
				if (neighbours.Count == 0)
				{
					generatorCells.Remove(cell);
					continue;
				}
				randomIndex = random.Next(neighbours.Count);
				var nextCell = neighbours[randomIndex];
				map[nextCell] = Cell.Road;
				generatorCells.Add(nextCell);
			}
			return map;
		}

		private static bool CheckBound(Point point, int localWidth, int localHeight)
		{
			return point.X >= 0 && point.Y >= 0 && point.X < localWidth && point.Y < localHeight;
		}

		private readonly int[] dx = { 0, 1, 0, -1 };
		private readonly int[] dy = { 1, 0, -1, 0 };
		private IEnumerable<Point> GetNeighbours(Point point)
		{
			return dx.Select((x, i) => point.Add(new Point(dy[i], x))).Where(x => CheckBound(x, width - 2, height - 2));
		}

		private readonly int[] diagonalDx = { 1, 1, -1, -1 };
		private readonly int[] diagonalDy = { 1, -1, -1, 1 };
		private IEnumerable<Point> GetDiagonalNeighbours(Point point)
		{
			return diagonalDx.Select((x, i) => point.Add(new Point(diagonalDy[i], x))).Where(x => CheckBound(x, width - 2, height - 2));
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForestSolver
{
    static public class FileReader
    {

        static public ICell[,] GetField(string source)
        {
            var gameObjectsDictionary = new Dictionary<char, Func<ICell>>
            {
                { '0', () => new Path() }, 
                { '1', () => new Wall() }, 
                { 'K', () => new Trap() }, 
                { 'L', () => new Life() }
            };
            var map =
                ReadFromFile(source)
                    .Select(line => line.ToCharArray()
                        .Select(symbol => gameObjectsDictionary[symbol]).ToArray())
                    .ToArray();
            var answer = new ICell[map.Length, map[0].Length];
            for (int i = 0; i < map.Length; i++)
                for (int j = 0; j < map[0].Length; j++)
                    answer[i, j] = map[i][j]();
            return answer;
        }

        private static string[] ReadFromFile(string fileName)
        {
            return System.IO.File.ReadAllLines(fileName);
        }

        public static List<Tuple<Point, Point>> GetAllPaticipants(string sourse)
        {
            var paticipantsLines = ReadFromFile(sourse);
            var paticipants = new List<Tuple<Point, Point>>();
            foreach (var paticipant in paticipantsLines)
            {
                var t = paticipant.Split().Select(int.Parse).ToArray();
                paticipants.Add(Tuple.Create(new Point(t[0], t[1]), new Point(t[2], t[3])));
            }
            return paticipants;
        }
    }
}

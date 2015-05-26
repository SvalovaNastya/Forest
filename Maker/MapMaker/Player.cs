using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
	public class Player
	{
		public Player()
		{
		}

		public Point Destination { get; set; }
		public Point Start { get; set; }
		public int Hp { get; set; }
	}
}

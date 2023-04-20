namespace test
{
	internal class MapBuilder
	{
		struct Room
		{
			public int x;
			public int y;
			public int width;
			public int height;
			public bool isinroom(int x, int y)
			{
				return (y >= this.y && y < this.y + this.height && x >= this.x && x < this.x + this.height);
			}
		}
		public int startX = -1;
		public int startY = -1;
		public bool[][] Map = new bool[10][];
		public MapBuilder(int size, int rooms, int minwidthroom, int maxwidthroom, int minheightroom, int maxheightroom, bool logging)
		{
			if (logging) Console.WriteLine("Making map");
			//------------------------------------------------------------------------------------------------------create map
			Map = new bool[size][];
			for (int i = 0; i < size; i++)
			{
				Map[i] = new bool[size];
				for (int j = 0; j < Map[i].Length; j++)
				{
					Map[i][j] = true;
				}
			}
			if (logging) Console.WriteLine("Map made");
			//------------------------------------------------------------------------------------------------------create rooms
			Random random = new Random();
			Room[] Rooms = new Room[rooms];
			for (int i = 0; i < rooms; i++)
			{
				int width = random.Next(minwidthroom, maxwidthroom);
				int height = random.Next(minheightroom, maxheightroom);
				int rx = random.Next(1, Map[0].Length - width);
				int ry = random.Next(1, Map.Length - height);
				Rooms[i] = new Room();
				Rooms[i].x = rx;
				Rooms[i].y = ry;
				Rooms[i].width = width;
				Rooms[i].height = height;
				//Console.WriteLine($"x:{rx} y:{ry} w:{width} h:{height}");
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						Map[y + ry][x + rx] = false;
					}
				}

			}
			if (logging) Console.WriteLine("Rooms created");
			//------------------------------------------------------------------------------------------------------make pathes
			foreach (Room room in Rooms)
			{
				int radius = 1;
				bool found = false;
				int centerX = room.x + room.width / 2;
				int centerY = room.y + room.height / 2;
				int toy = 0;
				int tox = 0;
				while (radius < 50)
				{


					//top
					for (int x = centerX - radius; x < centerX + radius; x++)
					{
						if (x >= 0 && x < Map[0].Length && centerY + radius >= 0 && centerY + radius < Map.Length && !room.isinroom(x, centerY + radius) && Map[centerY + radius][x] == false)
						{
							found = true;
							toy = centerY + radius;
							tox = x;
							break;
						}
					}
					if (!found)
					{
						//bottom
						for (int x = centerX - radius; x < centerX + radius; x++)
						{
							if (x >= 0 && x < Map[0].Length && centerY - radius >= 0 && centerY - radius < Map.Length && !room.isinroom(x, centerY - radius) && Map[centerY - radius][x] == false)
							{
								found = true;
								toy = centerY - radius;
								tox = x;
								break;
							}
						}
					}
					if (!found)
					{
						//left
						for (int y = centerY - radius; y < centerY + radius; y++)
						{
							if (y >= 0 && y < Map.Length && centerX - radius >= 0 && centerX - radius < Map[0].Length && !room.isinroom(centerX - radius, y) && Map[y][centerX - radius] == false)
							{
								found = true;
								toy = y;
								tox = centerX - radius;
								break;
							}
						}
					}
					if (!found)
					{
						//right
						for (int y = centerY - radius; y < centerY + radius; y++)
						{
							if (y >= 0 && y < Map.Length && centerX + radius >= 0 && centerX + radius < Map[0].Length && !room.isinroom(centerX + radius, y) && Map[y][centerX + radius] == false)
							{
								found = true;
								toy = y;
								tox = centerX + radius;
								break;
							}
						}
					}
					if (found)
					{
						break;
					}
					else
					{
						radius++;
					}
				}
				if (found)
				{
					double tX = room.x;
					double tY = room.y;
					double step = 0.05;
					double sx = (tox - room.x) * step;
					double sy = (toy - room.y) * step;

					//Console.WriteLine("Found line| Start: {0} {1}| End: {2} {3}| step: {4} {5}", room.x, room.y, tox, toy, sx, sy);
					while (Math.Abs(tox - tX) > 1 && Math.Abs(toy - tY) > 1 && (tX > 0 && tY > 0 && tX < Map[0].Length && tY < Map.Length))
					{
						int rx = (int)(tX);
						int ry = (int)(tY);
						//Console.WriteLine("ch:{0} {1}", rx, ry);
						for (int y = ry - 1; y < ry + 1; y++)
						{
							for (int x = rx - 1; x < rx + 1; x++)
							{
								if (x >= 0 && x < Map[0].Length && y >= 0 && y < Map.Length) Map[y][x] = false;

							}
						}
						tX += sx;
						tY += sy;

					}
					for (int y = toy - 1; y < toy + 1; y++)
					{
						for (int x = tox - 1; x < tox + 1; x++)
						{
							if (x >= 0 && x < Map[0].Length && y >= 0 && y < Map.Length) Map[y][x] = false;

						}
					}
					tX += sx;
					tY += sy;
				}
				else
				{
					//Console.WriteLine("Cant find path");
				}
			}
			if (logging) Console.WriteLine("Made paths");
			//------------------------------------------------------------------------------------------------------walls
			{
				for (int x = 0; x < Map[0].Length; x++)
				{
					Map[0][x] = true;
				}
				for (int x = 0; x < Map[0].Length; x++)
				{
					Map[Map.Length - 1][x] = true;
				}
				for (int y = 0; y < Map.Length; y++)
				{
					Map[y][0] = true;
				}
				for (int y = 0; y < Map.Length; y++)
				{
					Map[y][Map[0].Length - 1] = true;
				}
			}
			if (logging) Console.WriteLine("Made walls");
			//------------------------------------------------------------------------------------------------------Find starting point
			for (int y = 1; y < size - 1; y++)
			{
				for (int x = 1; x < size - 1; x++)
				{
					if (!Map[y][x]) { startX = x; startY = y; break; }
				}
				if (startX != -1 && startY != -1) break;
			}
		}
		public string[] getmap(int _x, int _y, int range, int degree)
		{
			char playerch = '@';

			/*if (degree >= 23 && degree < 68) playerch = '↘';
			else if (degree >= 68 && degree < 113) playerch = '↓';
			else if (degree >= 113 && degree < 158) playerch = '↙';
			else if (degree >= 158 && degree < 203) playerch = '←';
			else if (degree >= 203 && degree < 248) playerch = '↖';
			else if (degree >= 248 && degree < 293) playerch = '↑';
			else if (degree >= 293 && degree < 337) playerch = '↗';
			else if ((degree >= 337 && degree < 360)|| (degree >= 0 && degree < 23)) playerch = '→';*/

			if (degree >= 45 && degree < 135) playerch = '←';
			else if (degree >= 135 && degree < 225) playerch = '↑';
			else if (degree >= 225 && degree < 315) playerch = '→';
			else if ((degree >= 315 && degree <= 360) || (degree >= 0 && degree < 45)) playerch = '↓';
			List<string> map = new List<string>();
			map.Add('┌' + new string('─', (int)range * 2) + '┐');
			map.Add('│' + new string(' ', (int)((range * 2) - 3) / 2) + "MAP " + new string(' ', (int)((range * 2) - 3) / 2) + '│');
			map.Add('├' + new string('─', (int)range * 2) + '┤');
			//┤ ├
			for (double y = _y - range; y < _y + range; y++)
			{
				string line = "│";
				for (double x = _x - range; x < _x + range; x++)
				{

					if (x >= 0 && y >= 0 && x < Map[0].Length && y < Map.Length)
					{//░▒█▓
						line += (y == _y && x == _x) ? playerch : (Map[(int)y][(int)x]) ? '▒' : ' ';
					}
					else
					{
						line += ' ';
					}
				}
				line += '│';
				map.Add(line);
			}//└──┘
			map.Add('└' + new string('─', (int)range * 2) + '┘');
			return map.ToArray();
		}

	}
}

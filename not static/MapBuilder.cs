using System.Drawing;
using System.Drawing.Imaging;

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
	public int[][] Map = new int[10][];
	public MapBuilder(int size, int rooms, int minwidthroom, int maxwidthroom, int minheightroom, int maxheightroom, bool logging, int seed,bool saveimage)
	{
		if (logging) Console.WriteLine("Making map");
		//------------------------------------------------------------------------------------------------------create map
		Map = new int[size][];
		for (int i = 0; i < size; i++)
		{
			Map[i] = new int[size];
			for (int j = 0; j < Map[i].Length; j++)
			{
				Map[i][j] = 1;
			}
		}
		if (logging) Console.WriteLine("Map made");
		//------------------------------------------------------------------------------------------------------create rooms
		Random random = new Random(seed);
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
					Map[y + ry][x + rx] = 0;
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
					if (x >= 0 && x < Map[0].Length && centerY + radius >= 0 && centerY + radius < Map.Length && !room.isinroom(x, centerY + radius) && Map[centerY + radius][x] == 0)
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
						if (x >= 0 && x < Map[0].Length && centerY - radius >= 0 && centerY - radius < Map.Length && !room.isinroom(x, centerY - radius) && Map[centerY - radius][x] == 0)
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
						if (y >= 0 && y < Map.Length && centerX - radius >= 0 && centerX - radius < Map[0].Length && !room.isinroom(centerX - radius, y) && Map[y][centerX - radius] == 0)
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
						if (y >= 0 && y < Map.Length && centerX + radius >= 0 && centerX + radius < Map[0].Length && !room.isinroom(centerX + radius, y) && Map[y][centerX + radius] == 0)
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
							if (x >= 0 && x < Map[0].Length && y >= 0 && y < Map.Length) Map[y][x] = 0;

						}
					}
					tX += sx;
					tY += sy;

				}
				for (int y = toy - 1; y < toy + 1; y++)
				{
					for (int x = tox - 1; x < tox + 1; x++)
					{
						if (x >= 0 && x < Map[0].Length && y >= 0 && y < Map.Length) Map[y][x] = 0;

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
				Map[0][x] = 2;
			}
			for (int x = 0; x < Map[0].Length; x++)
			{
				Map[Map.Length - 1][x] = 2;
			}
			for (int y = 0; y < Map.Length; y++)
			{
				Map[y][0] = 2;
			}
			for (int y = 0; y < Map.Length; y++)
			{
				Map[y][Map[0].Length - 1] = 2;
			}
		}
		if (logging) Console.WriteLine("Made walls");
		//------------------------------------------------------------------------------------------------------Find starting point
		for (int y = 1; y < size - 1; y++)
		{
			for (int x = 1; x < size - 1; x++)
			{
				if (Map[y][x]==0) { startX = x; startY = y; break; }
			}
			if (startX != -1 && startY != -1) break;
		}
		//save image
		if (saveimage)
		{
			Bitmap mapbmp = new Bitmap(size, size);
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					if (Map[y][x] == 0) mapbmp.SetPixel(x, y, Color.White);
					else if (Map[y][x] == 1) mapbmp.SetPixel(x, y, Color.Black);
					else if (Map[y][x] == 2) mapbmp.SetPixel(x, y, Color.Blue);
				}
			}
			mapbmp.SetPixel(startX, startY, Color.Red);
			mapbmp.Save($"maps\\Map[{seed}].bmp", ImageFormat.Bmp); mapbmp.Dispose();
		}
	}
	public string[] getmap(int _x, int _y, int range, int degree, Point[] seenwalls)
	{
		char playerch = '@';


		if (degree >= 45 && degree < 135) playerch = '˃';
		else if (degree >= 135 && degree < 225) playerch = '˄';
		else if (degree >= 225 && degree < 315) playerch = '˂';
		else if ((degree >= 315 && degree <= 360) || (degree >= 0 && degree < 45)) playerch = '˅';
		List<string> map = new List<string>();
		map.Add('┌' + new string('─', (int)range * 2) + '┐');
		map.Add('│' + new string(' ', (int)((range * 2) - 3) / 2) + "MAP " + new string(' ', (int)((range * 2) - 3) / 2) + '│');
		map.Add('├' + new string('─', (int)range * 2) + '┤');
		//┤ ├
		for (int y = _y - range; y < _y + range; y++)
		{
			string line = "│";
			for (int x = _x - range; x < _x + range; x++)
			{

				if (x >= 0 && y >= 0 && x < Map[0].Length && y < Map.Length)
				{//░▒█▓
					if (y == _y && x == _x) line += playerch;
					else if (Map[y][x]!=0)
					{
						if (seenwalls.Contains(new Point(x, y))) line += '▓';
						else line += '▒';
					}
					else
					{
						line += ' ';
					}
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
	public bool CheckCollision(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
	{
		// calculate the right, bottom, and left edges of each rectangle
		int r1 = x1 + w1;
		int b1 = y1 + h1;
		int r2 = x2 + w2;
		int b2 = y2 + h2;

		// check for intersection along both axes
		if (x1 < r2 && r1 > x2 && y1 < b2 && b1 > y2)
		{
			// rectangles overlap, handle collision here
			return true;
		}

		// no intersection found
		return false;
	}

}

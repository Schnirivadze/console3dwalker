using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using test;

internal class Program
{
	#region Imports
	[DllImport("user32.dll")]
	static extern bool GetCursorPos(out POINT lpPoint);
	[StructLayout(LayoutKind.Sequential)]
	struct POINT
	{
		public int X;
		public int Y;
	}
	#endregion
	#region Static variables
	static int fps = 30;
	static int seed = 0;
	static MapBuilder mb = new(50, 50, 7, 10, 7, 10, false ,14);
	static StringBuilder OutputBuilder = new();
	static double pX = mb.startX;
	static double pY = mb.startY;
	static int pD = 0;
	static int pov = 60;
	static double steppov = 0.5;
	static double speed = 0.1;
	static int viewlength = 15;
	static int ScreenHeight = 30;
	static bool showmap = false;
	#endregion
	static void Main(string[] args)
	{
		#region XmlSettings
		XmlDocument xmlDoc = new XmlDocument();

		// Load the XML file from the specified path
		xmlDoc.Load("settings.xml");

		// Get the root node of the document
		XmlNode rootNode = xmlDoc.DocumentElement;

		// Traverse the child nodes of the root node
		foreach (XmlNode node in rootNode.ChildNodes)
		{
			int minroomsize = -1, maxroomsize = -1, mapsize = -1, rooms = -1;

			//Console.WriteLine(node.InnerText);

			switch (node.Name)
			{
				case "fps":
					fps = Convert.ToInt32(node.InnerText);
					break;
				case "startviewdegree":
					pD = Convert.ToInt32(node.InnerText);
					break;
				case "pov":
					pov = Convert.ToInt32(node.InnerText);
					break;
				case "steppov":
					steppov = XmlConvert.ToDouble(node.InnerText);
					break;
				case "speed":
					speed = XmlConvert.ToDouble(node.InnerText);
					break;
				case "rangeofview":
					viewlength = Convert.ToInt32(node.InnerText);
					break;
				case "screenheight":
					ScreenHeight = Convert.ToInt32(node.InnerText);
					break;

				case "mapinfo":
					foreach (XmlNode mapinfo in node.ChildNodes)
					{
						switch (mapinfo.Name)
						{
							case "seed":
								if(mapinfo.InnerText.ToLower() == "random")
								{
									Random r = new();
									seed = r.Next();
								}
								else
								{
									seed = Convert.ToInt32(mapinfo.InnerText);
								}
								break;
							case "size":
								mapsize = Convert.ToInt32(mapinfo.InnerText);
								break;
							case "minroomsize":
								minroomsize = Convert.ToInt32(mapinfo.InnerText);
								break;
							case "maxroomsize":
								maxroomsize = Convert.ToInt32(mapinfo.InnerText);
								break;
							case "rooms":
								rooms = Convert.ToInt32(mapinfo.InnerText);
								break;
						}
					}
					break;

			}
			if (minroomsize != (-1) && maxroomsize != (-1) && mapsize != (-1) && rooms != (-1))
			{
				mb = new(50, 50, 7, 10, 7, 10, false, seed);
				//mb = new MapBuilder(mapsize, 20, minroomsize, maxroomsize, minroomsize, maxroomsize, false);
				pX = mb.startX;
				pY = mb.startY;
			}
			//Console.WriteLine(node.Name + ": " + node.InnerText);
		}
		#endregion
		#region Set up
		Console.SetWindowSize((int)(pov / steppov) + 1, ScreenHeight + 1);
		Console.SetBufferSize((int)(pov / steppov) + 1, ScreenHeight + 1);
		Console.CursorVisible = false;
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		Thread curs = new Thread(new ThreadStart(CursWhatch));
		Thread movement = new Thread(new ThreadStart(movementWhatch));
		movement.Start();
		curs.Start();
		List<int> CeilingList = new List<int>();
		List<int> FloorList = new List<int>();
		List<double> DistanceList = new List<double>();
		int sf = 0;
		string[] minimap = new string[1];
		Stopwatch stpw = new();
		#endregion
		while (true)
		{
			stpw.Restart();
			sf++;
			OutputBuilder.Clear();
			DistanceList.Clear();
			CeilingList.Clear();
			FloorList.Clear();
			//Console.Title = $"D:{pD}";
			// For each column, calculate the projected ray angle into world space
			for (double d = 0 - pov / 2; d < pov / 2; d += steppov)
			{
				double RayX = Math.Sin(((d + pD) / 180) * Math.PI);
				double RayY = Math.Cos(((d + pD) / 180) * Math.PI);

				double distanceToWall = 0;
				while (distanceToWall < viewlength)
				{
					distanceToWall += 0.1;
					int testX = (int)(pX + (RayX * distanceToWall));
					int testY = (int)(pY + (RayY * distanceToWall));
					//if out of bounds
					if (testX < 0 || testY < 0 || testX > mb.Map.Length || testY > mb.Map.Length)
					{
						distanceToWall = viewlength; break;
					}
					else if (mb.Map[testY][testX])
					{
						distanceToWall *= Math.Cos(Math.Abs(d) / 180 * Math.PI);
						break;
					}

				}

				// Calculate distance to ceiling and floor
				DistanceList.Add(distanceToWall);
				int Ceiling = (int)((double)(ScreenHeight / 2.0) - ScreenHeight / ((double)distanceToWall));
				CeilingList.Add(Ceiling);
				int Floor = ScreenHeight - Ceiling;
				FloorList.Add(Floor);
			}
			//Fill Outputbuilder
			int startx = 0;
			if (showmap) { minimap = mb.getmap((int)pX, (int)pY, 10, pD); }
			for (int y = 0; y < ScreenHeight; y++)
			{
				if (showmap && y < minimap.Length) { OutputBuilder.Append(minimap[y]); startx = minimap[y].Length; }
				else { startx = 0; }

				for (int x = startx; x < (int)(pov / steppov); x++)
				{
					char wallshade = ' ';
					if (DistanceList[x] <= viewlength / 4) wallshade = '█';
					else if (DistanceList[x] < viewlength / 3) wallshade = '▓';
					else if (DistanceList[x] < viewlength / 2) wallshade = '▒';
					else if (DistanceList[x] < viewlength) wallshade = '░';
					else wallshade = ' ';


					if (y < CeilingList[x]) OutputBuilder.Append(' ');
					else if (y >= CeilingList[x] && y < FloorList[x]) OutputBuilder.Append(wallshade);
					else
					{
						double f = 1.0 - ((double)y - ScreenHeight / 2) / ((double)ScreenHeight / 2);
						if (f < 0.25) wallshade = '#';
						else if (f < 0.5) wallshade = 'x';
						else if (f < 0.75) wallshade = '.';
						else if (f < 0.9) wallshade = '-';
						else wallshade = ' ';
						OutputBuilder.Append(wallshade);
					}
				}
				OutputBuilder.Append('\n');
			}
			//Output
			Console.SetCursorPosition(0, 0);
			Console.Write(OutputBuilder.ToString());
			stpw.Stop();
			if (1000 / fps > stpw.Elapsed.Milliseconds) Thread.Sleep(1000 / fps - stpw.Elapsed.Milliseconds);


		}
	}

	private static void movementWhatch()
	{
		double step = speed;
		while (true)
		{
			ConsoleKeyInfo key = Console.ReadKey();
			// Convert degrees to radians
			double angle = pD * Math.PI / 180.0;

			if (key.Key == ConsoleKey.W && !mb.Map[(int)(pY + step * Math.Cos(angle))][(int)(pX + step * Math.Sin(angle))])
			{
				pX += step * Math.Sin(angle);
				pY += step * Math.Cos(angle);
			}
			else if (key.Key == ConsoleKey.S && !mb.Map[(int)(pY - step * Math.Cos(angle))][(int)(pX - step * Math.Sin(angle))])
			{
				pX -= step * Math.Sin(angle);
				pY -= step * Math.Cos(angle);
			}
			else if (key.Key == ConsoleKey.M)
			{
				showmap = !showmap;
			}

		}

	}

	private static void CursWhatch()
	{
		POINT mousePos;
		while (true)
		{
			GetCursorPos(out mousePos);
			pD = (720 * mousePos.X / 1919 > 360) ? (720 * mousePos.X / 1919) - 360 : 720 * mousePos.X / 1919;
			Thread.Sleep(50); // Add a 50-millisecond delay

		}
	}


}

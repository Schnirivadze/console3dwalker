using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

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
	static int fpslimit = 10;
	static int seed = 0;
	static MapBuilder mb = new(50, 50, 7, 10, 7, 10, false, 14,false);
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
	static bool showstats = false;
	#endregion
	static void Main(string[] args)
	{
		#region Set up
		//Console--------------------------------------------------------------------------
		Console.SetWindowSize((int)(pov / steppov) + 1, ScreenHeight + 1);
		Console.SetBufferSize((int)(pov / steppov) + 1, ScreenHeight + 1);
		Console.CursorVisible = false;
		Console.OutputEncoding = Encoding.UTF8;
		//threads--------------------------------------------------------------------------
		new Thread(new ThreadStart(CursWhatch)).Start();
		new Thread(new ThreadStart(movementWhatch)).Start();
		//lists----------------------------------------------------------------------------
		List<int> CeilingList = new();
		List<int> FloorList = new();
		List<double> DistanceList = new();
		List<Point> Seenwalls = new();
		List<double> WallPartList = new();
		List<Shader> ShaderList = new();
		List<int> WallTypeList = new();
		//2d arrays------------------------------------------------------------------------
		string[] minimap = new string[1];
		string[] stats = new string[1];
		//other----------------------------------------------------------------------------
		Stopwatch stpw = new();
		//byte[][] currentshader.texture = Functions.Gettexture("image.jpg");
		#endregion
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
				case "fpslimit":
					if (node.InnerText.ToLower() == "none") fpslimit = 1000000;
					else fpslimit = Convert.ToInt32(node.InnerText);
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
								if (mapinfo.InnerText.ToLower() == "random")
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
				case "shaders":
					foreach (XmlNode shadernode in node.ChildNodes)
					{
						switch (shadernode.Name)
						{
							case "shaderpath":
								ShaderList.Add(new(Functions.Gettexture(shadernode.InnerText)));
								break;
							case "shaderstr":
								ShaderList.Add(new(shadernode.InnerText));
								break;
						}
					}
					break;
			}
			if (minroomsize != (-1) && maxroomsize != (-1) && mapsize != (-1) && rooms != (-1))
			{
				mb = new(50, 50, 7, 10, 7, 10, false, seed,true);
				//mb = new MapBuilder(mapsize, 20, minroomsize, maxroomsize, minroomsize, maxroomsize, false);
				pX = mb.startX;
				pY = mb.startY;
			}
			//Console.WriteLine(node.Name + ": " + node.InnerText);
		}
		#endregion
		while (true)
		{
			
			stpw.Restart();
			#region Clean up
			OutputBuilder.Clear();
			DistanceList.Clear();
			CeilingList.Clear();
			FloorList.Clear();
			Seenwalls.Clear();
			WallPartList.Clear();
			WallTypeList.Clear();
			#endregion
			#region Distance measure
			// For each column, calculate the projected ray angle into world space
			double RayX, RayY, distanceToWall, interX, interY,raystep=0.1;
			bool addedwal;
			int testX, testY;
			int lookingat=-1;
			for (double d = 0 - pov / 2; d < pov / 2; d += steppov)
			{
				RayX = Math.Sin(((d + pD) / 180) * Math.PI);
				RayY = Math.Cos(((d + pD) / 180) * Math.PI);
				addedwal = false;
				distanceToWall = 0;
				while (distanceToWall < viewlength)
				{
					distanceToWall += raystep;
					testX = (int)(pX + (RayX * distanceToWall));
					testY = (int)(pY + (RayY * distanceToWall));
					//if out of bounds
					if (testX < 0 || testY < 0 || testX > mb.Map.Length || testY > mb.Map.Length)
					{
						distanceToWall = viewlength;
						break;
					}
					else if (mb.Map[testY][testX]!=0)
					{
						if (d == 0) { lookingat = mb.Map[testY][testX]; }
						WallTypeList.Add(mb.Map[testY][testX]);
						interX = (pX + (RayX * distanceToWall)) - testX;
						interY = (pY + (RayY * distanceToWall)) - testY;
						//if corner
						 if ((interX < raystep && interY < raystep) || (interX > 1- raystep && interY > 1- raystep) ||(interX < raystep && interY > 1 - raystep) || (interX > 1 - raystep && interY < raystep)) WallPartList.Add(-2.0);
						//if not corner
						else if(interY < raystep) WallPartList.Add(1.0 - interX);//t
						else if (interY > 1 - raystep) WallPartList.Add(interX);//b
						else if (interX < raystep) WallPartList.Add(interY);//l
						else if (interX > 1 - raystep) WallPartList.Add(1.0 - interY);//r
						//else
						else WallPartList.Add(-1.0);
						distanceToWall *= Math.Cos(Math.Abs(d) / 180 * Math.PI);
						Seenwalls.Add(new(testX, testY));
						addedwal = true;
						break;
					}
				}
				if (!addedwal)
				{
					Seenwalls.Add(new()); WallPartList.Add(-1.0);
					WallTypeList.Add(0);
				}
				// Calculate distance to ceiling and floor
				DistanceList.Add(distanceToWall);
				int Ceiling = (int)((double)(ScreenHeight / 2.0) - ScreenHeight / ((double)distanceToWall));
				if (Ceiling < 0) Ceiling = 0;
				CeilingList.Add(Ceiling);
				int Floor = ScreenHeight - Ceiling;
				FloorList.Add(Floor);
			}
			#endregion
			#region Output
			//Fill Outputbuilder
			int startx = 0;
			string shader = "░▒▓█";
			int endx = (int)(pov / steppov);
			Shader currentshader;
			if (showmap) { minimap = mb.getmap((int)pX, (int)pY, 10, pD, Seenwalls.ToArray()); }
			if (showstats) { stats = Functions.getStats(pX, pY, pD, seed, pov, speed, steppov, viewlength, fps, fpslimit,lookingat); }

			for (int y = 0; y < ScreenHeight; y++)
			{
				if (showmap && y < minimap.Length) { OutputBuilder.Append(minimap[y]); startx = minimap[y].Length; }
				else { startx = 0; }
				if (showstats && y < stats.Length) { endx = (int)(pov / steppov) - stats[y].Length; }
				else { endx = (int)(pov / steppov); }

				for (int x = startx; x < endx; x++)
				{
					char wallshade = ' ';
					if (y >= CeilingList[x] && y < FloorList[x])
					{
						if (WallPartList[x] >0)
						{

							currentshader = ShaderList[WallTypeList[x]];
							int x_ = (int)(WallPartList[x] * currentshader.texture[0].Length);
							int y_ = ((y - CeilingList[x]) * currentshader.texture.Length) / (FloorList[x] - CeilingList[x]);
							if (y_ > currentshader.texture.Length - 1) y_ = currentshader.texture.Length - 1;
							double gray = currentshader.texture[y_][x_] * (1.0 - (DistanceList[x] / (double)(viewlength * 2)));
							wallshade = shader[(int)(gray * shader.Length) / 255];
						}
						else if (WallPartList[x] == -2.0)
						{
							wallshade = '|';
						}
						else
						{

							if (DistanceList[x] <= viewlength / 4) wallshade = '█';
							else if (DistanceList[x] < viewlength / 3) wallshade = '▓';
							else if (DistanceList[x] < viewlength / 2) wallshade = '▒';
							else if (DistanceList[x] < viewlength) wallshade = '░';
						}
					}
					//wallshade = dirr[x];



					if (y < CeilingList[x]) OutputBuilder.Append(' ');
					else if (y >= CeilingList[x] && y < FloorList[x])
						OutputBuilder.Append(wallshade);
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

				if (showstats && y < stats.Length) OutputBuilder.Append(stats[y]);
				OutputBuilder.Append('\n');
			}
			//Output
			Console.SetCursorPosition(0, 0);
			Console.Write(OutputBuilder.ToString());
			#endregion
			#region Fps handling
			// Calculate remaining time until next frame
			double remainingSeconds = 1.0 / fpslimit - stpw.Elapsed.TotalSeconds;

			// Wait for remaining time using a thread sleep
			if (remainingSeconds > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(remainingSeconds));
			}
			fps = (int)(1000 / stpw.Elapsed.TotalMilliseconds);
			#endregion

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

			if (key.Key == ConsoleKey.W && mb.Map[(int)(pY + step * Math.Cos(angle))][(int)(pX + step * Math.Sin(angle))]==0)
			{
				pX += step * Math.Sin(angle);
				pY += step * Math.Cos(angle);
			}
			else if (key.Key == ConsoleKey.S && mb.Map[(int)(pY - step * Math.Cos(angle))][(int)(pX - step * Math.Sin(angle))]==0)
			{
				pX -= step * Math.Sin(angle);
				pY -= step * Math.Cos(angle);
			}
			else if (key.Key == ConsoleKey.M)
			{
				showmap = !showmap;
			}
			else if (key.Key == ConsoleKey.I)
			{
				showstats = !showstats;
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

using System.Drawing;

    public class Functions
    {
        public static byte[][] Gettexture(string path)
        {
            Bitmap textureimg = new Bitmap(path);
            byte[][] texture = new byte[textureimg.Height][];
            for (int y = 0; y < textureimg.Height; y++)
            {
                texture[y] = new byte[textureimg.Width];
                for (int x = 0; x < textureimg.Width; x++)
                {
                    texture[y][x] = textureimg.GetPixel(x, y).R;//(byte)(textureimg.GetPixel(x, y).R + textureimg.GetPixel(x, y).G+ textureimg.GetPixel(x, y).B/3);
                }
            }
            textureimg.Dispose();
            return texture;
        }
        public static string[] getStats(double x, double y, int degree, int seed, int pov, double speed, double steppov, int viewlength, int fps, int fpslimit,int lookingat)
        {
            int biggestlength = 0;
            List<string> stats = new List<string>
            {
                "│ X: " + x.ToString("0.00") + " ",
                "│ Y: " + y.ToString("0.00") + " ",
                "│ D: " + degree + " ",
                "│ Seed: " + seed + " ",
                "│ FOV: " + pov + "/" + steppov + " ",
                "│ Speed: " + speed.ToString("0.00") + " ",
                "│ Range: " + viewlength + " ",
                "│ FPS: " + fps + "/"+(fpslimit==1000000? '∞': fpslimit.ToString())+' ',
				"│ Looking at: " + lookingat + " ",

			};
            foreach (string line in stats)
            {
                if (line.Length > biggestlength) biggestlength = line.Length;
            }
            for (int i = 0; i < stats.Count; i++)
            {
                if (stats[i].Length < biggestlength) stats[i] += new string(' ', biggestlength - stats[i].Length) + '│';
                else stats[i] += '│';
            }
            stats.Add('└' + new string('─', stats[0].Length - 2) + '┘');
            stats.Insert(0, '├' + new string('-', stats[0].Length - 2) + '┤');
            stats.Insert(0, '│' + new string(' ', (stats[0].Length - 8) / 2) + "STATS " + new string(' ', (stats[0].Length - 8) / 2) + '│');
            stats.Insert(0, '┌' + new string('─', stats[0].Length - 2) + '┐');

            return stats.ToArray();
        }

    }


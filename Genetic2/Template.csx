#r "C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Drawing.dll"
using System;
using System.Drawing;

const string targetFile = @"[TargetFilePath]";
const string wannabeFileName = @"[wannabeFileName]";
Bitmap wannabe = null;
Graphics g;

void getDimensions(out int width, out int height)
{
    var b = new Bitmap(targetFile);
    width = b.Width;
    height = b.Height;
    b.Dispose();
}

int score()
{
    int _score = 0;
    var target = new Bitmap(targetFile);
    Color original = Color.Red;
    Color variant = Color.Red;

    for (int x = 0; x < target.Width; x++)
    {
        for (int y = 0; y < target.Height; y++)
        {
            original = target.GetPixel(x, y);
            variant = wannabe.GetPixel(x, y);
            _score += Math.Abs(original.R - variant.R) + Math.Abs(original.G - variant.G) + Math.Abs(original.B - variant.B);
        }
    }
    wannabe.Save(wannabeFileName);
    wannabe.Dispose();
    target.Dispose();
    return _score;
}
void cyclePixels(int startX, int startY, int endX, int endY)
{
    var r = new Random();
    for (int x = startX; x < endX; x++)
    {
        for(int y = startY; y< endY; y++)
        {
            var i = r.Next(0, 3);
            switch (i)
            {
                case 0:
                    wannabe.SetPixel(x, y, Color.Red);
                    break;
                case 1:
                    wannabe.SetPixel(x, y, Color.White);
                    break;
                case 2:
                    wannabe.SetPixel(x, y, Color.Black);
                    break;

            }
        }
    }
}

int Width;
int Height;
getDimensions(out Width, out Height);
wannabe = new Bitmap(Width, Height);
g = Graphics.FromImage(wannabe);
var red = new SolidBrush(Color.Red);
var white = new SolidBrush(Color.White);
var black = new SolidBrush(Color.Black);
g.FillRectangle(white, 0, 0, Width, Height);
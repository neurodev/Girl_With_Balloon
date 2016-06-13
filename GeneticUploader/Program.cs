using System;
using System.Linq;
using System.Drawing;
using System.Data;
using Genetic2.Data;
using System.Text;
using System.IO;

namespace GeneticUploader
{
    public class Program
    {
        public static void DrawText(Bitmap bmp, long generation)
        {
            var brush = new SolidBrush(Color.Black);
            var font = new Font("Arial", 24);
            var graphics = Graphics.FromImage(bmp);
            graphics.DrawString(string.Format("Generation: {0}", generation.ToString("D7")), font, brush, 10, 10);
        }

        static string SuccessIntervalGraph(long mostRecentGeneration, string saveDirectory, string imageDirectory, EvolutionEntities DBContext)
        {
            var filename = saveDirectory + "\\success_intervals-" + mostRecentGeneration.ToString() + ".png";
            var points = (from g in DBContext.GenerationsBeforeSuccess() select g.GenerationsBeforeSuccess.Value).ToArray();

            var bmp = Grapher.Graph.LineGraph(512, 1024, points, "Number of generations", "Failed generations before success", Color.Blue);
            bmp.Save(filename);

            return filename;
        }

        static string ScoreGraph(long mostRecentGeneration, string saveDirectory, string imageDirectory, EvolutionEntities DBContext)
        {
            var filename = saveDirectory + "\\scores-" + mostRecentGeneration.ToString() + ".png";
            var points = (from g in DBContext.GraphData().Where(x=>x.WasSuccessful.Value) select g.Score.Value).ToArray();

            var bmp = Grapher.Graph.LineGraph(512, 1024, points, "Number of generations", "Score", Color.Blue);
            bmp.Save(filename);

            return filename;
        }

        static void CalculatePixels()
        {
            var red = 0;
            var black = 0;
            var white = 0;
            var bmp = new Bitmap(@"c:\temp\DNA\GWB.bmp");
            for (var x = 0; x< bmp.Width; x++)
            {
                for (var y = 0; y<bmp.Height; y++)
                {
                    var color = bmp.GetPixel(x, y);
                    if (color.ToArgb() == Color.Red.ToArgb())
                    {
                        red++;
                        
                    }else if (color.ToArgb() ==Color.Black.ToArgb())
                    {
                        black++;
                    }else if(color.ToArgb() == Color.White.ToArgb())
                    {
                        white++;
                    }
                    else
                    {
                        Console.WriteLine("Well shit");
                    }
                }
                
            }
            Console.Out.WriteLine("Red:" + red);
            Console.WriteLine("Black:" + black);
            Console.WriteLine("White:" + white);
            Console.ReadLine();

        }

        static string GenerateGif(long mostRecentGeneration, string saveDirectory, string imageDirectory, EvolutionEntities DBContext)
        {
            
            var e = new Gif.Components.AnimatedGifEncoder();

            String outputFilePath = saveDirectory + "\\progress-" + mostRecentGeneration.ToString() + ".gif";
            e.Start(outputFilePath);
            e.SetDelay(100);
            //-1:no repeat,0:always repeat
            e.SetRepeat(-1);



            var filenametemplate = imageDirectory + @"\{0}-{1}.jpg";
            foreach (var generation in DBContext.FamilyTrees.Where(x => x.WasSuccessful).OrderBy(x => x.Generation))
            {
                
                Bitmap bmp = new Bitmap(string.Format(filenametemplate, generation.Generation, generation.Child));
                DrawText(bmp, generation.Generation);
                e.AddFrame(bmp);

            }
            
            e.Finish();
            return outputFilePath;

        }
        static string GenerateMostRecent(long mostRecentGeneration, int childNumber, string saveDirectory, string imageDirectory)
        {
            var filenametemplate = imageDirectory + @"\{0}-{1}.jpg";
            var filename = saveDirectory + "\\most_recent-" + mostRecentGeneration.ToString() + ".png";
            Bitmap bmp = new Bitmap(string.Format(filenametemplate, mostRecentGeneration, childNumber));
            DrawText(bmp, mostRecentGeneration);
            bmp.Save(filename);
            return filename;
        }

        public static void Main(string[] args)
        {
            string saveDirectory = @"c:\temp\DNA\upload";
            string imageDirectory = @"c:\temp\DNA\evolution";
            var DBContext = new EvolutionEntities();
            var mostRecentSuccess = DBContext.FamilyTrees.Where(x => x.WasSuccessful).OrderByDescending(x => x.Generation).First();

            var gifFilePath = GenerateGif(mostRecentSuccess.Generation, saveDirectory,imageDirectory,DBContext);
            var successIntervalsFilePath = SuccessIntervalGraph(mostRecentSuccess.Generation, saveDirectory, imageDirectory, DBContext);
            var scoresFilePath=ScoreGraph(mostRecentSuccess.Generation, saveDirectory, imageDirectory, DBContext);
            var mostRecentFilePath = GenerateMostRecent(mostRecentSuccess.Generation, mostRecentSuccess.Child, saveDirectory, imageDirectory);
            var indexFile = GenerateHTML(successIntervalsFilePath,scoresFilePath,gifFilePath,mostRecentFilePath,saveDirectory);
            //CalculatePixels();
            DBContext.Dispose();
            
            Console.WriteLine("done");
            //Console.ReadLine();
        }

        private static string GenerateHTML(string successfulIntervalsFileName, string scoreFileName, string gifFileName, string mostRecentFileName,string saveDirectory)
        {
            var sb = new StringBuilder(File.ReadAllText(saveDirectory + @"\template.html"));
            //[current_date]
            var current_date = DateTime.UtcNow.Day.ToString() + ((DateTime.UtcNow.Day % 10 == 1 && DateTime.UtcNow.Day != 11) ? "st"
                                     : (DateTime.UtcNow.Day % 10 == 2 && DateTime.UtcNow.Day != 12) ? "nd"
                                     : (DateTime.UtcNow.Day % 10 == 3 && DateTime.UtcNow.Day != 13) ? "rd"
                                     : "th") + " " + DateTime.UtcNow.ToString("MMMM") + " " 
                                     + DateTime.UtcNow.Year.ToString() + " " 
                                     + DateTime.UtcNow.Hour.ToString("00") + ":" + DateTime.UtcNow.Minute.ToString("00") + " GMT";
            sb.Replace("[current_date]", current_date);
            sb.Replace("[successful_intervals.png]", successfulIntervalsFileName.Substring(successfulIntervalsFileName.LastIndexOf("\\")+1));
            sb.Replace("[scores.png]", scoreFileName.Substring(scoreFileName.LastIndexOf("\\")+1));
            sb.Replace("[progress.gif]", gifFileName.Substring(gifFileName.LastIndexOf("\\")+1));
            sb.Replace("[most_recent.jpg]", mostRecentFileName.Substring(scoreFileName.LastIndexOf("\\") + 1));

            var DB = new EvolutionEntities();
            var numberOfGenerations = DB.FamilyTrees.Max(x => x.Generation);
            var numberOfSuccessfulGenerations = DB.FamilyTrees.Where(x => x.WasSuccessful).Count();
            sb.Replace("[current_generation]", numberOfGenerations.ToString());
            sb.Replace("[successful_generations]", numberOfSuccessfulGenerations.ToString());

            var fileName = saveDirectory + "\\index.html";
            File.WriteAllText(fileName, sb.ToString());

            return fileName;
        }
    }
}

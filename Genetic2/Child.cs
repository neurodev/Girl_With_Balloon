using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Threading.Tasks;
using Genetic2.Data;
using System.IO;
using System.Drawing;

namespace Genetic2
{
    public class Child
    {
        public int Index { get; set; }
        private List<string> _programBody;
        private string _targetImage;
        private long _generation;
        private Int64 _localscore = 0;
        private string _wannabefilename;
        private string _parentScript;
        private static string[] colours = { "red", "white", "black" };
        private long runTime;
        public Int64 Score
        {
            get
            {
                return _localscore;
            }
        }

        public Child(long generation, int index, string target)
        {
            Index = index;
            _generation = generation;
            _targetImage = target;
            _wannabefilename = Path.Combine(Properties.Settings.Default.ImageDirectory, _generation.ToString() + "-" + index.ToString() + ".jpg");

        }

        public void MutateAndRun(string parentScript)
        {
            var mainScript = new List<string>(File.ReadAllLines(Properties.Settings.Default.TemplateFilePath));
            mainScript[4] = mainScript[4].Replace("[TargetFilePath]", _targetImage);
            mainScript[5] = mainScript[5].Replace("[wannabeFileName]", _wannabefilename);
            _parentScript = parentScript;
            _programBody = new List<string>(File.ReadAllLines(parentScript));
            MutateScript();

            mainScript.AddRange(_programBody);

            mainScript.Add("g.Dispose();");
            mainScript.Add("score()");
            var r = new Random();
            var combinedScript = String.Join(Environment.NewLine, mainScript);
            mainScript.Clear();
            var preRunTimeStamp = DateTime.UtcNow.Ticks;
            this._localscore = CSharpScript.EvaluateAsync<long>(combinedScript).Result;
            runTime = DateTime.UtcNow.Ticks - preRunTimeStamp;
            Console.WriteLine(_generation.ToString() + "-" + Index.ToString());
        }

        private void MutateScript()
        {
            var targetBitMap = new Bitmap(_targetImage);
            var r = new Random();
            var mutation = r.Next(0, 4);
            switch (mutation)
            {
                case 0:
                    _programBody.Insert(r.Next(0, _programBody.Count), getRect(targetBitMap.Width, targetBitMap.Height));
                    break;
                case 1:
                    _programBody.Insert(r.Next(0, _programBody.Count), getCircle(targetBitMap.Width, targetBitMap.Height));
                    break;
                case 2:
                    _programBody.Insert(r.Next(0, _programBody.Count), getCycle(targetBitMap.Width, targetBitMap.Height));
                    break;

                default:
                    if (_programBody.Count > 0) _programBody.RemoveAt(r.Next(0, _programBody.Count));
                    break;
            }
            targetBitMap.Dispose();

        }
        private void Log(bool successfulGeneration)
        {
            var DBContext = new EvolutionEntities();
            DBContext.FamilyTrees.Add(new FamilyTree()
            {
                Child = Index,
                Generation = _generation,
                LinesOfCode = _programBody.Count,
                Score = Score,
                WasSuccessful = successfulGeneration,
                RunTime = runTime
            });
            try
            {
                DBContext.SaveChanges();
            }
            catch
            {
            }
            DBContext.Dispose();
        }

        private static string getRect(int maxWidth, int maxHeight)
        {
            var r = new Random();
            var startX = r.Next(0, maxWidth);
            var width = r.Next(0, maxWidth - startX);
            var startY = r.Next(0, maxHeight);
            var height = r.Next(0, maxHeight - startY);
            var pen = colours[r.Next(0, 3)];

            return String.Format("g.FillRectangle({0},{1},{2},{3},{4});", pen, startX, startY, width, height);
        }

        private static string getCycle(int maxWidth, int maxHeight)
        {
            var r = new Random();
            var startX = r.Next(0, maxWidth);
            var width = r.Next(0, maxWidth - startX);
            var startY = r.Next(0, maxHeight);
            var height = r.Next(0, maxHeight - startY);
            var pen = colours[r.Next(0, 3)];

            return String.Format("cyclePixels({0},{1},{2},{3});", startX, startY, width, height);
        }

        private static string getCircle(int maxWidth, int maxHeight)
        {
            var r = new Random();
            var startX = r.Next(0, maxWidth);
            var width = r.Next(0, maxWidth - startX);
            var startY = r.Next(0, maxHeight);
            var height = r.Next(0, maxHeight - startY);
            var pen = colours[r.Next(0, 3)];

            return String.Format("g.FillEllipse({0},{1},{2},{3},{4});", pen, startX, startY, width, height);
        }

        public void Survive()
        {
            File.Delete(_parentScript);
            File.WriteAllLines(_parentScript, _programBody);
            var t = Task.Run(() => GeneticUploader.Program.Main(new string[] { }));
            Log(true);
            _programBody.Clear();
            Task.WaitAll(t);
        }

        public void DarwinAward()
        {
            File.Delete(_wannabefilename);
            Log(false);
            _programBody.Clear();
        }


    }
}

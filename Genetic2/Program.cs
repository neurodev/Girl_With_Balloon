using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetic2.Data;

namespace Genetic2
{
    class Program
    {
        static void FightToTheDeath(List<Child> children, ref long daddysScore)
        {
            var winner = children.OrderBy(x => x.Score).First();
            foreach (var child in children.Where(x => x.Index != winner.Index))
            {
                child.DarwinAward();
            }
            if (daddysScore > winner.Score || daddysScore == -1)
            {
                daddysScore = winner.Score;
                winner.Survive();
            }
            else
            {
                winner.DarwinAward();
            }
        }

        static void Main(string[] args)
        {
            //CSharpScript causes memory leaks. 
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/be68af47-b420-4440-9075-484505948a5f/memory-leak-issue-with-roslyn-compilation?forum=roslyn
            
            int iterationsBeforeTerminate = 100;
            var DBContext = new EvolutionEntities();
            var generation = DBContext.FamilyTrees.Max(x => (long?)x.Generation) ?? 0;
            long daddysScore = DBContext.FamilyTrees.Min(x => (long?)x.Score) ?? -1;

            var tasks = new List<Task>();
            var children = new List<Child>();


            for (int counter = 0; counter < iterationsBeforeTerminate; counter++)
            {
                generation++;

                for (int i = 0; i < 5; i++)
                {
                    Child c = new Child(generation, i, Properties.Settings.Default.TargetPicture);// @"c:\temp\DNA\GWB.bmp");
                    children.Add(c);
                    tasks.Add(Task.Factory.StartNew(() => c.MutateAndRun(Properties.Settings.Default.CurrentGenerationPath)));// @"c:\temp\DNA\currentGeneration.csx")));
                }


                Task.WaitAll(tasks.ToArray());

                FightToTheDeath(children, ref daddysScore);

                children.Clear();
                tasks.Clear();
            }
            Console.WriteLine("All done");
        }
    }
}

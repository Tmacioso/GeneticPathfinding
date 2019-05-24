using SFML.Graphics;
using SFML.Window;
using System;
using SFML.System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GeneticSFML
{
    public class Generation
    {
        public Generation(int populationCount, int bestCount, int dnaSize, Vector2f startPosition, float mutationChance, RectangleShape target, RenderWindow renderWindow)
        {
            PopulationCount = populationCount;
            BestCount = bestCount;
            DnaSize = dnaSize;
            StartPosition = startPosition;
            MutationChance = mutationChance;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Window = renderWindow ?? throw new ArgumentNullException(nameof(renderWindow));

            Population = new Rocket[PopulationCount];
            for (int i = 0; i < PopulationCount; i++)
            {
                Population[i] = new Rocket(startPosition, dnaSize, mutationChance);
                Population[i].Mutate();
            }
        }

        public int PopulationCount { get; set; }
        public int BestCount { get; set; }
        public int DnaSize { get; set; }
        public Vector2f StartPosition { get; set; }
        public float MutationChance { get; set; }
        public RectangleShape Target { get; set; }
        public Rocket[] Population { get; set; }
        public RenderWindow Window { get; set; }

        public void Score()
        {
            var scored = new KeyValuePair<Rocket, double>[PopulationCount];

            for (int i = 0; i < PopulationCount; i++)
            {
                var score = Math.Sqrt(Math.Pow(Target.Position.X - Population[i].Position.X, 2) + Math.Pow(Target.Position.Y - (Window.Size.Y - Population[i].Position.Y), 2));
                scored[i] = new KeyValuePair<Rocket, double>(Population[i], score);
            }
            var best = scored
                .OrderBy(x => x.Value)
                .Take(BestCount)
                .Select(x =>x.Key)
                .ToArray();
            Cross(best);
        }
        public void Cross(Rocket[] best)
        {
            if (best.Length != BestCount) throw new Exception("Best count fucked");

        }
        public void LazyDraw()
        {
            for (int j = 0; j < DnaSize; j++)
            {
                for (int i = 0; i < PopulationCount; i++)
                {
                    if (Population[i].Position.Y >= Window.Size.Y) continue;//If on top, skip

                    Population[i].NextStep();
                    var point = new RectangleShape(new Vector2f(1, 1));
                    point.Position = new Vector2f(Population[i].Position.X, Window.Size.Y - Population[i].Position.Y);
                    point.OutlineThickness = 0;
                    point.FillColor = Color.White;
                    Window.Draw(point);
                    Window.Display();
                }
            }
        }
        public void FastDraw()
        {
            for (int j = 0; j < DnaSize; j++)
            {
                for (int i = 0; i < PopulationCount; i++)
                {
                    if (Population[i].Position.Y >= Window.Size.Y) continue;//If on top, skip

                    Population[i].NextStep();
                    var point = new RectangleShape(new Vector2f(1, 1));
                    point.Position = new Vector2f(Population[i].Position.X, Window.Size.Y - Population[i].Position.Y);
                    point.OutlineThickness = 0;
                    point.FillColor = Color.White;
                    Window.Draw(point);
                }
            }
            Window.Display();
        }
    }
    public class Rocket
    {
        public enum Move
        {
            Up,
            Left,
            Right
        }
        public Vector2f Position { get; set; }
        public Move[] DNA { get; set; }
        public int Step { get; set; } = 0;
        public int DNASize { get; set; }
        public float MutationChance { get; set; }



        //public Rocket(int genomeSize)
        //{
        //    this.DNASize = genomeSize;
        //    DNA = new Move[genomeSize];
        //}

        //public Rocket()
        //{
        //}

        public Rocket(Vector2f position, int dNASize, float mutationChance)
        {
            Position = position;
            DNASize = dNASize;
            MutationChance = mutationChance;
            DNA = new Move[DNASize];
        }

        public void Mutate()
        {
            for (int i = 0; i < DNASize; i++)
            {
                var r = new Random().NextDouble() * 100;
                if (100 - MutationChance < r)
                {
                    DNA[i] = (Move)new Random().Next(3);
                }
            }
        }
        public void NextStep()
        {
            if (Step >= DNASize) return;
            switch (DNA[Step])
            {
                case Move.Up:
                    Position = new Vector2f(Position.X, Position.Y + 1);
                    break;
                case Move.Left:
                    Position = new Vector2f(Position.X + 1, Position.Y);
                    break;
                case Move.Right:
                    Position = new Vector2f(Position.X - 1, Position.Y);
                    break;
                default:
                    break;
            }
            //Position = new Vector2f(Position.X, Position.Y + 1);

            Step++;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //int bestCount = 5;
            //int specCount = 70;
            //int DnaCount = 1500;
            //float mutationRate = 0.15f;

            RenderWindow window = new RenderWindow(new VideoMode(500, 500), "Windows");

            //var centerCollider = new RectangleShape(new Vector2f(150, 150));
            //centerCollider.Position = new Vector2f(window.Size.X / 2 - centerCollider.Size.X / 2, window.Size.Y / 2 - centerCollider.Size.Y / 2);
            //centerCollider.FillColor = new Color(50, 50, 50);

            var target = new RectangleShape(new Vector2f(50, 20));
            target.Position = new Vector2f(window.Size.X / 2 - target.Size.X / 2, 0);
            target.FillColor = new Color(120, 120, 120);
            window.SetActive();

            var pop = new Generation(100, 5, 1000, new Vector2f(50, 0), 0.15f, target, window);

            while(window.IsOpen)
            {
                pop.LazyDraw();

                throw new Exception();
                //Console.ReadLine();
            }


            //List<Rocket> rockets = new List<Rocket>();
            //for (int i = 0; i < specCount; i++)
            //{
            //    rockets.Add(new Rocket(DnaCount));
            //}
            //foreach (var r in rockets)
            //{
            //    r.MutationChance = mutationRate;
            //    r.Mutate();
            //}
            //window.SetActive();
            //while (window.IsOpen)
            //{
            //    window.Clear();
            //    window.DispatchEvents();
            //    //window.Draw(centerCollider);
            //    window.Draw(target);
            //    window.Display();

            //    while (true)
            //    {
            //        window.Clear();
            //        window.Draw(target);
            //        foreach (var rocket in rockets)
            //        {
            //            for (int i = 0; i < rocket.DNASize; i++)
            //            {
            //                rocket.NextStep();
            //                var r = new RectangleShape(new Vector2f(1, 1));
            //                r.Position = new Vector2f(rocket.Position.X, 500 - rocket.Position.Y);
            //                window.Draw(r);
            //                if (rocket.Position.Y >= 500) break;
            //            }
            //        }
            //        window.Display();


            //        var scoredList = new List<KeyValuePair<Rocket, double>>();
            //        foreach (var rocket in rockets)
            //        {
            //            var d = Math.Sqrt(Math.Pow(target.Position.X - rocket.Position.X, 2) + Math.Pow(target.Position.Y - (500 - rocket.Position.Y), 2));
            //            scoredList.Add(new KeyValuePair<Rocket, double>(rocket, d));
            //        }
            //        scoredList = scoredList.OrderBy(x => x.Value).ToList();

            //        var bestList = new List<Rocket>();
            //        for (int i = 0; i < bestCount; i++)
            //        {
            //            bestList.Add(scoredList[i].Key);
            //        }


            //        rockets = new List<Rocket>();
            //        for (int i = 0; i < specCount; i++)
            //        {
            //            rockets.Add(new Rocket(DnaCount));
            //        }
            //        foreach (var r in rockets)
            //        {
            //            r.DNA = bestList[new Random().Next(bestCount)].DNA;
            //            r.MutationChance = mutationRate;
            //            r.Mutate();
            //        }
            //        for (int i = 0; i < specCount; i++)
            //        {
            //            var r = new System.Random().Next(DnaCount);
            //            var dna1 = rockets[i].DNA.Take(r);
            //            var dna2 = rockets[new Random().Next(specCount)].DNA.Skip(r);
            //            var dna = new Rocket.Move[DnaCount];
            //            dna = dna1.Concat(dna2).ToArray();
            //            rockets[i].DNA = dna;
            //        }

            //        //window.Display();
            //        //Thread.Sleep(1000);
            //    }
            //}



        }
    }
}

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
                Population[i] = new Rocket(StartPosition, DnaSize, MutationChance);
            }
            Mutate();
        }
        int gen = 0;
        public int PopulationCount { get; set; }
        public int BestCount { get; set; }
        public int DnaSize { get; set; }
        public Vector2f StartPosition { get; set; }
        public float MutationChance { get; set; }
        public RectangleShape Target { get; set; }
        public Rocket[] Population { get; set; }
        public RenderWindow Window { get; set; }
        public List<RectangleShape> Colliders { get; set; } = new List<RectangleShape>();

        Random random = new Random();

        public void Score()
        {
            gen++;
            var scored = new KeyValuePair<Rocket, double>[PopulationCount];

            for (int i = 0; i < PopulationCount; i++)
            {
                //var score = Math.Sqrt(Math.Pow(Target.Position.X - Population[i].Position.X, 2) + Math.Pow(Target.Position.Y - (Window.Size.Y - Population[i].Position.Y), 2));

                var score = Math.Sqrt(
                    Math.Pow((Target.Position.X) - Population[i].Position.X, 2) + 
                    Math.Pow((Target.Position.Y) - (Window.Size.Y - Population[i].Position.Y), 2));

                scored[i] = new KeyValuePair<Rocket, double>(Population[i], score);
            }

            var bestScored = scored
                .OrderBy(x => x.Value)
                .Take(BestCount);

            Console.WriteLine($"Generation {gen} - best fitness: {bestScored.First().Value}");
            if(bestScored.ToArray()[0].Value <= 10)
            {
                PrintSolution(bestScored.ToArray()[0].Key);
            }

            var best = bestScored
                .Select(x => x.Key)
                .ToArray();

            Cross(best);
            Mutate();
        }
        public void Cross(Rocket[] best)
        {
            if (best.Length != BestCount) throw new Exception("Best count fucked");

            var newPopulation = new Rocket[PopulationCount];
            for (int i = 0; i < PopulationCount; i++)
            {
                var dnaSplitPlace = random.Next(DnaSize);
                var dna1 = best[random.Next(BestCount)].DNA.Take(dnaSplitPlace);
                var dna2 = best[random.Next(BestCount)].DNA.Skip(dnaSplitPlace);
                var crossedDna = dna1.Concat(dna2).ToArray();
                newPopulation[i] = new Rocket(StartPosition, DnaSize, MutationChance);
                newPopulation[i].DNA = crossedDna;
            }

            Population = newPopulation;
        }
        public void Mutate()
        {
            for (int i = 0; i < PopulationCount; i++)
            {
                Population[i].Mutate();
            }
        }
        public void LazyDraw()
        {
            Window.Clear();
            Window.Draw(Target);
            Window.Display();

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
            Window.Clear();
            Window.Draw(Target);
            Colliders.ForEach(x => Window.Draw(x));
            Window.Display();


            for (int i = 0; i < PopulationCount; i++)
            {
                for (int j = 0; j < DnaSize; j++)
                {
                    if (Population[i].Position.Y >= Window.Size.Y || CheckCollisions(Population[i])) continue;//If on top, skip

                    Population[i].NextStep();
                    var point = new RectangleShape(new Vector2f(1, 1));
                    point.Position = new Vector2f(Population[i].Position.X, Window.Size.Y - Population[i].Position.Y);
                    point.OutlineThickness = 0;
                    point.FillColor = Color.White;
                    Window.Draw(point);
                }
                Window.Display();
            }
        }
        public bool CheckCollisions(Rocket rocket)
        {
            foreach (var item in Colliders)
            {
                if (rocket.Position.X > item.Position.X &&
                    rocket.Position.X < item.Position.X + item.Size.X &&
                    Window.Size.Y - rocket.Position.Y > item.Position.Y &&
                    Window.Size.Y - rocket.Position.Y < item.Position.Y + item.Size.Y)
                    return true;
            }
            return false;
        }
        public void PrintSolution(Rocket rocket)
        {
            Window.Clear();
            Window.Draw(Target);
            Colliders.ForEach(x => Window.Draw(x));

            var bestRocket = new Rocket(StartPosition, DnaSize, MutationChance);
            bestRocket.DNA = rocket.DNA;

            for (int j = 0; j < DnaSize; j++)
            {
                if (bestRocket.Position.Y >= Window.Size.Y || CheckCollisions(bestRocket)) continue;//If on top, skip

                bestRocket.NextStep();
                var point = new RectangleShape(new Vector2f(3, 3));
                point.Position = new Vector2f(bestRocket.Position.X, Window.Size.Y - bestRocket.Position.Y);
                point.OutlineThickness = 0;
                point.FillColor = Color.Green;
                Window.Draw(point);
            }
            Window.Display();


            Console.WriteLine("Solution: ");
            rocket.DNA.ToList().ForEach(x => Console.Write(x + " "));
            Console.WriteLine();
            Console.ReadLine();
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
            RenderWindow window = new RenderWindow(new VideoMode(500, 500), "Windows");

            var target = new RectangleShape(new Vector2f(30, 30));
            target.Position = new Vector2f(window.Size.X / 2 - target.Size.X / 2, 0);
            target.FillColor = Color.Red;

            var centerCollider = new RectangleShape(new Vector2f(50, 200));
            centerCollider.Position = new Vector2f(window.Size.X / 2 - centerCollider.Size.X / 2, window.Size.Y / 2 - centerCollider.Size.Y / 2);
            centerCollider.FillColor = Color.Yellow;
            //window.Draw(centerCollider);
            //window.Display();

            window.SetActive();
            var pop = new Generation(100, 5, 1000, new Vector2f(window.Size.X / 2, 0), 5f, target, window);
            pop.Colliders.Add(centerCollider);

            while (window.IsOpen)
            {
                pop.FastDraw();
                pop.Score();
            }
        }
    }
}

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
        public Generation(int populationCount, int bestCount, int dnaSize, Vector2f startPosition, float mutationChance, RectangleShape target, RenderWindow renderWindow, bool randomPath = false)
        {
            PopulationCount = populationCount;
            BestCount = bestCount;
            DnaSize = dnaSize;
            StartPosition = startPosition;
            BaseMutationChance = mutationChance;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Window = renderWindow ?? throw new ArgumentNullException(nameof(renderWindow));

            Population = new Rocket[PopulationCount];
            for (int i = 0; i < PopulationCount; i++)
            {
                Population[i] = new Rocket(StartPosition, DnaSize, BaseMutationChance);
            }
            if (randomPath)
            {
                for (int i = 0; i < PopulationCount; i++)
                {
                    for (int j = 0; j < DnaSize; j++)
                    {
                        Population[i].DNA[j] = (Rocket.Move)random.Next(4);//3 -> without DOWN
                    }
                }
            }
            else
                Mutate();

            //Window.Clear();
            //Window.Draw(Target);
            //Colliders.ForEach(x => Window.Draw(x));
            //Window.Display();
        }
        double lastFitness = double.MaxValue;
        int gen = 0;

        public int PopulationCount { get; set; }
        public int BestCount { get; set; }
        public int DnaSize { get; set; }
        public Vector2f StartPosition { get; set; }
        public float BaseMutationChance { get; set; }
        public float CurrentMutationChance { get; set; }
        public float MutationChanceIncrese { get; set; } = 0.1f;
        public float MaxMutatonChance { get; set; } = 100f;
        public float LastFitnessOffset { get; set; } = 10f;
        public RectangleShape Target { get; set; }
        public Rocket[] Population { get; set; }
        public RenderWindow Window { get; set; }
        public List<RectangleShape> Colliders { get; set; } = new List<RectangleShape>();
        public bool DeadlyCollisions { get; set; } = false;

        Random random = new Random();

        public void Score()
        {
            gen++;
            var scored = new KeyValuePair<Rocket, double>[PopulationCount];

            for (int i = 0; i < PopulationCount; i++)
            {
                //var score = Math.Sqrt(Math.Pow(Target.Position.X - Population[i].Position.X, 2) + Math.Pow(Target.Position.Y - (Window.Size.Y - Population[i].Position.Y), 2));

                if (DeadlyCollisions && Population[i].Fitness == -1)
                {
                    scored[i] = new KeyValuePair<Rocket, double>(Population[i], float.MaxValue);
                    continue;
                }

                var score = Math.Sqrt(
                    Math.Pow((Target.Position.X) - Population[i].Position.X, 2) +
                    Math.Pow((Target.Position.Y) - (Window.Size.Y - Population[i].Position.Y), 2));

                scored[i] = new KeyValuePair<Rocket, double>(Population[i], score);
            }

            var bestScored = scored
                .OrderBy(x => x.Value)
                .Take(BestCount);

            if (lastFitness <= bestScored.First().Value + LastFitnessOffset)
            {
                CurrentMutationChance += MutationChanceIncrese;
                if (CurrentMutationChance > MaxMutatonChance) CurrentMutationChance = MaxMutatonChance;

                //Doesnt work lol
                //bestScored = scored
                //    .OrderByDescending(x => x.Value)
                //    .Take(BestCount);//VER EXPERIMENTAL

                //for (int i = 0; i < BestCount; i++)
                //{
                //    bestScored.ToArray()[i] = scored.ToArray()[random.Next(PopulationCount)];
                //}

            }
            else
            {
                CurrentMutationChance = BaseMutationChance;
            }

            for (int i = 0; i < PopulationCount; i++)
            {
                Population[i].MutationChance = CurrentMutationChance;
            }
            Console.WriteLine($"Generation {gen} - best fitness: {bestScored.First().Value} Mutation chance: {CurrentMutationChance}%");


            if (bestScored.ToArray()[0].Value <= 10)
            {
                PrintSolution(bestScored.ToArray()[0].Key);
            }

            var best = bestScored
                .Select(x => x.Key)
                .ToArray();



            Cross(best);
            Mutate();

            lastFitness = bestScored.First().Value;
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
                newPopulation[i] = new Rocket(StartPosition, DnaSize, BaseMutationChance);
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
        public void FastDraw()
        {
            Window.Clear();

            for (int i = 0; i < PopulationCount; i++)
            {
                VertexArray line = new VertexArray(PrimitiveType.LineStrip);
                for (uint j = 0; j < DnaSize; j++)
                {
                    if (Population[i].Position.Y >= Window.Size.Y) break;
                    if (CheckCollisions(Population[i]))
                    {
                        Population[i].Fitness = -1;
                        break;//If on top, skip
                    }

                    Population[i].NextStep();
                    line.Append(new Vertex(new Vector2f(Population[i].Position.X, Window.Size.Y - Population[i].Position.Y), new Color(255, 255, 255, 35)));
                }
                Window.Draw(line);
            //Window.Display();
            }

            Window.Draw(Target);
            Colliders.ForEach(x => Window.Draw(x));
            Window.Display();
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

            DrawRocket(rocket, Color.Green);

            Window.Display();

            Console.WriteLine("Solution: ");
            rocket.DNA.ToList().ForEach(x => Console.Write(x + " "));
            Console.WriteLine();
            Console.ReadLine();
        }
        public void DrawRocket(Rocket rocket, Color color)
        {
            VertexArray line = new VertexArray(PrimitiveType.LineStrip);
            for (int i = 0; i < DnaSize; i++)
            {
                var r = new Rocket(StartPosition, DnaSize, BaseMutationChance);
                r.DNA = rocket.DNA;

                for (int j = 0; j < DnaSize; j++)
                {
                    if (r.Position.Y >= Window.Size.Y || CheckCollisions(r)) break;//If on top, skip

                    r.NextStep();
                    line.Append(new Vertex(new Vector2f(r.Position.X, Window.Size.Y - r.Position.Y), color));

                    Window.Draw(line);
                }
            }
        }
    }
    public class Rocket
    {
        public enum Move
        {
            Up,
            Left,
            Right,
            Down
        }
        public Vector2f Position { get; set; }
        public Move[] DNA { get; set; }
        public int Step { get; set; } = 0;
        public int DNASize { get; set; }
        public float MutationChance { get; set; }
        public float Fitness { get; set; } = 0;

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
                    DNA[i] = (Move)new Random().Next(4);
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
                case Move.Down:
                    Position = new Vector2f(Position.X, Position.Y - 1);
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
            RenderWindow window = new RenderWindow(new VideoMode(500, 500), "Generic SFML");

            var target = new RectangleShape(new Vector2f(30, 30));
            target.Position = new Vector2f(0, 0);
            target.FillColor = Color.Red;

            window.SetActive();
            var pop = new Generation(100, 3, 8000, new Vector2f(window.Size.X-1, 0), 5f, target, window, true);
            pop.MutationChanceIncrese = 1f;
            pop.DeadlyCollisions = true;

            var rnd = new Random();
            for (int i = 0; i < 8; i++)
            {
                var a = rnd.Next(30, 100);
                var col = new RectangleShape(new Vector2f(a, a));
                col.FillColor = new Color(100, 100, 100);
                col.Position = new Vector2f(rnd.Next(0, 500), rnd.Next(0, 500));
                pop.Colliders.Add(col);
            }

            while (window.IsOpen)
            {
                pop.FastDraw();
                pop.Score();
            }
        }
    }
}

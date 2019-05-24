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
                        Population[i].DNA[j] = (Rocket.Move)random.Next(3);//3 -> without DOWN
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

        Random random = new Random();

        public void Score()
        {
            gen++;
            var scored = new KeyValuePair<Rocket, double>[PopulationCount];

            for (int i = 0; i < PopulationCount; i++)
            {
                //var score = Math.Sqrt(Math.Pow(Target.Position.X - Population[i].Position.X, 2) + Math.Pow(Target.Position.Y - (Window.Size.Y - Population[i].Position.Y), 2));

                //if(Population[i].Fitness == -1 )
                //{
                //    scored[i] = new KeyValuePair<Rocket, double>(Population[i], float.MaxValue);
                //    continue;
                //}

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
            Window.Draw(Target);
            Colliders.ForEach(x => Window.Draw(x));
            Window.Display();


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
            //var ss = Window.Capture();
            //ss.SaveToFile($"[{DateTime.Now}] - Normal");

            Window.Clear();
            Window.Draw(Target);
            Colliders.ForEach(x => Window.Draw(x));

            var bestRocket = new Rocket(StartPosition, DnaSize, BaseMutationChance);
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
            //ss = Window.Capture();
            //ss.SaveToFile($"[{DateTime.Now}] - Highlighted");

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
            Right,
            Down
        }
        public Vector2f Position { get; set; }
        public Move[] DNA { get; set; }
        public int Step { get; set; } = 0;
        public int DNASize { get; set; }
        public float MutationChance { get; set; }

        public float Fitness { get; set; } = 0;

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
            RenderWindow window = new RenderWindow(new VideoMode(500, 500), "Windows");

            var target = new RectangleShape(new Vector2f(30, 30));
            target.Position = new Vector2f(window.Size.X / 2 - target.Size.X / 2, 0);
            target.FillColor = Color.Red;


            //var left1 = new RectangleShape(new Vector2f(100, 20));
            //left1.Position = new Vector2f(0, 120);
            //left1.FillColor = Color.Yellow;

            //var right1 = new RectangleShape(new Vector2f(350, 20));
            //right1.Position = new Vector2f(150, 120);
            //right1.FillColor = Color.Yellow;


            //var left2 = new RectangleShape(new Vector2f(125, 20));
            //left2.Position = new Vector2f(0, 140);
            //left2.FillColor = Color.Yellow;

            //var right2 = new RectangleShape(new Vector2f(500 - 125 - 50, 20));
            //right2.Position = new Vector2f(125 + 50, 140);
            //right2.FillColor = Color.Yellow;


            var left3 = new RectangleShape(new Vector2f(150, 20));
            left3.Position = new Vector2f(0, 160);
            left3.FillColor = Color.Yellow;

            var right3 = new RectangleShape(new Vector2f(500 - 150 - 50, 20));
            right3.Position = new Vector2f(150 + 50, 160);
            right3.FillColor = Color.Yellow;


            var left4 = new RectangleShape(new Vector2f(175, 20));
            left4.Position = new Vector2f(0, 180);
            left4.FillColor = Color.Yellow;

            var right4 = new RectangleShape(new Vector2f(500 - 175 - 50, 20));
            right4.Position = new Vector2f(175 + 50, 180);
            right4.FillColor = Color.Yellow;


            var centerCollider = new RectangleShape(new Vector2f(50, 200));
            centerCollider.Position = new Vector2f(window.Size.X / 2 - centerCollider.Size.X / 2, window.Size.Y / 2 - centerCollider.Size.Y / 2);
            centerCollider.FillColor = Color.Yellow;


            window.SetActive();
            var pop = new Generation(70, 5, 30000, new Vector2f(window.Size.X / 2, 0), 5f, target, window, true);
            //pop.Colliders.Add(left1);
            //pop.Colliders.Add(right1);
            //pop.Colliders.Add(left2);
            //pop.Colliders.Add(right2);
            pop.Colliders.Add(left3);
            pop.Colliders.Add(right3);
            pop.Colliders.Add(left4);
            pop.Colliders.Add(right4);
            pop.MutationChanceIncrese = 2.5f;
            //pop.Colliders.Add(centerCollider);

            while (window.IsOpen)
            {
                pop.FastDraw();
                pop.Score();
            }
        }
    }
}

using SFML.Graphics;
using SFML.Window;
using System;
using SFML.System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GeneticSFML
{
    public class Rocket
    {
        public enum Move
        {
            Up,
            Left,
            Right
        }
        public Vector2f Position { get; set; } = new Vector2f(450, 0);
        public Move[] DNA { get; set; }
        public int step = 0, DNASize;
        public float mutationRate = 5;

        public Rocket(int genomeSize)
        {
            this.DNASize = genomeSize;
            DNA = new Move[genomeSize];
        }
        public void Mutate()
        {
            for (int i = 0; i < DNASize; i++)
            {
                var r = new Random().NextDouble() * 100;
                if (100 - mutationRate < r)
                {
                    DNA[i] = (Move)new Random().Next(3);
                }
            }
        }
        public void NextStep()
        {
            if (step >= DNASize) return;
            switch (DNA[step])
            {
                //case Move.Up:
                //    Position = new Vector2f(Position.X, Position.Y + 1);
                //    break;
                case Move.Left:
                    Position = new Vector2f(Position.X + 1, Position.Y);
                    break;
                case Move.Right:
                    Position = new Vector2f(Position.X - 1, Position.Y);
                    break;
                default:
                    break;
            }
            Position = new Vector2f(Position.X, Position.Y + 1);

            step++;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            int bestCount = 5;
            int specCount = 70;
            int DnaCount = 1500;
            float mutationRate = 0.15f;

            RenderWindow window = new RenderWindow(new VideoMode(500, 500), "Windows");

            //var centerCollider = new RectangleShape(new Vector2f(150, 150));
            //centerCollider.Position = new Vector2f(window.Size.X / 2 - centerCollider.Size.X / 2, window.Size.Y / 2 - centerCollider.Size.Y / 2);
            //centerCollider.FillColor = new Color(50, 50, 50);

            var target = new RectangleShape(new Vector2f(50, 20));
            target.Position = new Vector2f(window.Size.X / 2 - target.Size.X / 2, 0);
            target.FillColor = new Color(120, 120, 120);


            List<Rocket> rockets = new List<Rocket>();
            for (int i = 0; i < specCount; i++)
            {
                rockets.Add(new Rocket(DnaCount));
            }
            foreach (var r in rockets)
            {
                r.mutationRate = mutationRate;
                r.Mutate();
            }
            window.SetActive();
            while (window.IsOpen)
            {
                window.Clear();
                window.DispatchEvents();
                //window.Draw(centerCollider);
                window.Draw(target);
                window.Display();

                while (true)
                {
                    window.Clear();
                    window.Draw(target);
                    foreach (var rocket in rockets)
                    {
                        for (int i = 0; i < rocket.DNASize; i++)
                        {
                            rocket.NextStep();
                            var r = new RectangleShape(new Vector2f(1, 1));
                            r.Position = new Vector2f(rocket.Position.X, 500 - rocket.Position.Y);
                            window.Draw(r);
                            if (rocket.Position.Y >= 500) break;
                        }
                    }
                    window.Display();


                    var scoredList = new List<KeyValuePair<Rocket, double>>();
                    foreach (var rocket in rockets)
                    {
                        var d = Math.Sqrt(Math.Pow(target.Position.X - rocket.Position.X, 2) + Math.Pow(target.Position.Y - (500 - rocket.Position.Y), 2));
                        scoredList.Add(new KeyValuePair<Rocket, double>(rocket, d));
                    }
                    scoredList = scoredList.OrderBy(x => x.Value).ToList();

                    var bestList = new List<Rocket>();
                    for (int i = 0; i < bestCount; i++)
                    {
                        bestList.Add(scoredList[i].Key);
                    }


                    rockets = new List<Rocket>();
                    for (int i = 0; i < specCount; i++)
                    {
                        rockets.Add(new Rocket(DnaCount));
                    }
                    foreach (var r in rockets)
                    {
                        r.DNA = bestList[new Random().Next(bestCount)].DNA;
                        r.mutationRate = mutationRate;
                        r.Mutate();
                    }
                    for (int i = 0; i < specCount; i++)
                    {
                        var r = new System.Random().Next(DnaCount);
                        var dna1 = rockets[i].DNA.Take(r);
                        var dna2 = rockets[new Random().Next(specCount)].DNA.Skip(r);
                        var dna = new Rocket.Move[DnaCount];
                        dna = dna1.Concat(dna2).ToArray();
                        rockets[i].DNA = dna;
                    }

                    //window.Display();
                    //Thread.Sleep(1000);
                }
            }
        }
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Match3Game.Core
{
    // Uçan Füzeler (Madde 19, 20, 21)
    public class Destroyer
    {
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // 250ms Gecikmeli Bomba (Madde 27)
    public class ActiveBomb
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Timer { get; set; } = 0.250f; // 250 milisaniye
    }

    public class Board
    {
        public const int Width = 8;
        public const int Height = 8;

        public Gem[,] Grid { get; private set; }
        private Random _random;

        // Efekt Listeleri
        public List<Destroyer> Destroyers { get; private set; }
        public List<ActiveBomb> ActiveBombs { get; private set; }
        public int PendingScore { get; set; } = 0; // Efektlerden gelen skoru ekrana taşımak için

        public Board()
        {
            Grid = new Gem[Width, Height];
            _random = new Random();
            Destroyers = new List<Destroyer>();
            ActiveBombs = new List<ActiveBomb>();
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GemType newType;
                    do { newType = GetRandomGemType(); } while (CreatesMatch(x, y, newType));
                    Grid[x, y] = new Gem(newType);
                }
            }
        }

        private bool CreatesMatch(int x, int y, GemType type)
        {
            if (x >= 2 && Grid[x - 1, y] != null && Grid[x - 1, y].Type == type && Grid[x - 2, y] != null && Grid[x - 2, y].Type == type) return true;
            if (y >= 2 && Grid[x, y - 1] != null && Grid[x, y - 1].Type == type && Grid[x, y - 2] != null && Grid[x, y - 2].Type == type) return true;
            return false;
        }

        private GemType GetRandomGemType() => (GemType)_random.Next(1, 6);

        private List<List<Point>> GetHorizontalMatches()
        {
            var matches = new List<List<Point>>();
            for (int y = 0; y < Height; y++)
            {
                int matchLength = 1;
                for (int x = 0; x < Width; x++)
                {
                    if (x < Width - 1 && Grid[x, y].Type != GemType.Empty && Grid[x, y].Type == Grid[x + 1, y].Type) matchLength++;
                    else
                    {
                        if (matchLength >= 3)
                        {
                            var match = new List<Point>();
                            for (int i = 0; i < matchLength; i++) match.Add(new Point(x - i, y));
                            matches.Add(match);
                        }
                        matchLength = 1;
                    }
                }
            }
            return matches;
        }

        private List<List<Point>> GetVerticalMatches()
        {
            var matches = new List<List<Point>>();
            for (int x = 0; x < Width; x++)
            {
                int matchLength = 1;
                for (int y = 0; y < Height; y++)
                {
                    if (y < Height - 1 && Grid[x, y].Type != GemType.Empty && Grid[x, y].Type == Grid[x, y + 1].Type) matchLength++;
                    else
                    {
                        if (matchLength >= 3)
                        {
                            var match = new List<Point>();
                            for (int i = 0; i < matchLength; i++) match.Add(new Point(x, y - i));
                            matches.Add(match);
                        }
                        matchLength = 1;
                    }
                }
            }
            return matches;
        }

        public bool HasAnyMatch() => GetHorizontalMatches().Count > 0 || GetVerticalMatches().Count > 0;

        private Point GetBonusTarget(List<Point> match, Point? p1, Point? p2)
        {
            if (p1.HasValue && match.Contains(p1.Value)) return p1.Value;
            if (p2.HasValue && match.Contains(p2.Value)) return p2.Value;
            return match[match.Count / 2];
        }

        // Taşı yok eder ve eğer içindeyse BONUSU TETİKLER! (Zincirleme reaksiyonun sırrı)
        public int DestroyGemAndTrigger(int x, int y)
        {
            if (Grid[x, y] == null || Grid[x, y].Type == GemType.Empty) return 0;

            BonusType bonus = Grid[x, y].Bonus;
            Grid[x, y].Type = GemType.Empty;
            Grid[x, y].Bonus = BonusType.None;

            Vector2 visualPos = new Vector2(x * 60 + 160 + 2, y * 60 + 60 + 2);

            if (bonus == BonusType.LineHorizontal)
            {
                Destroyers.Add(new Destroyer { Position = visualPos, Direction = new Vector2(-1, 0) });
                Destroyers.Add(new Destroyer { Position = visualPos, Direction = new Vector2(1, 0) });
            }
            else if (bonus == BonusType.LineVertical)
            {
                Destroyers.Add(new Destroyer { Position = visualPos, Direction = new Vector2(0, -1) });
                Destroyers.Add(new Destroyer { Position = visualPos, Direction = new Vector2(0, 1) });
            }
            else if (bonus == BonusType.Bomb)
            {
                ActiveBombs.Add(new ActiveBomb { X = x, Y = y });
            }

            return 10; // Her patlayan taş 10 puan
        }

        // EFEKT GÜNCELLEME DÖNGÜSÜ (Füzeleri uçur, Bombaları patlat)
        public void UpdateEffects(float dt)
        {
            bool boardChanged = false;

            // 1. Bombaları Say (250ms)
            for (int i = ActiveBombs.Count - 1; i >= 0; i--)
            {
                ActiveBombs[i].Timer -= dt;
                if (ActiveBombs[i].Timer <= 0)
                {
                    int cx = ActiveBombs[i].X;
                    int cy = ActiveBombs[i].Y;
                    ActiveBombs.RemoveAt(i);

                    // 3x3 Alanı Patlat (Madde 27)
                    for (int x = cx - 1; x <= cx + 1; x++)
                    {
                        for (int y = cy - 1; y <= cy + 1; y++)
                        {
                            if (x >= 0 && x < Width && y >= 0 && y < Height)
                            {
                                PendingScore += DestroyGemAndTrigger(x, y);
                                boardChanged = true;
                            }
                        }
                    }
                }
            }

            // 2. Füzeleri Uçur (Saniyede 400 piksel hızla uçarlar)
            foreach (var d in Destroyers)
            {
                d.Position += d.Direction * 400f * dt;

                int gridX = (int)((d.Position.X - 160) / 60);
                int gridY = (int)((d.Position.Y - 60) / 60);

                if (gridX >= 0 && gridX < Width && gridY >= 0 && gridY < Height)
                {
                    if (Grid[gridX, gridY].Type != GemType.Empty)
                    {
                        PendingScore += DestroyGemAndTrigger(gridX, gridY); // Uçarken altındakini patlat!
                        boardChanged = true;
                    }
                }
                else
                {
                    d.IsActive = false; // Ekraddan çıktı
                }
            }
            Destroyers.RemoveAll(d => !d.IsActive);

            if (boardChanged) ApplyGravityAndRefill();
        }

        public void ApplyGravityAndRefill()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = Height - 1; y >= 0; y--)
                {
                    if (Grid[x, y].Type == GemType.Empty)
                    {
                        for (int k = y - 1; k >= 0; k--)
                        {
                            if (Grid[x, k].Type != GemType.Empty)
                            {
                                Grid[x, y].Type = Grid[x, k].Type;
                                Grid[x, y].Bonus = Grid[x, k].Bonus;
                                Grid[x, y].Position = Grid[x, k].Position;
                                Grid[x, y].TargetPosition = new Vector2(x * 60 + 160 + 2, y * 60 + 60 + 2);

                                Grid[x, k].Type = GemType.Empty;
                                Grid[x, k].Bonus = BonusType.None;
                                break;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Grid[x, y].Type == GemType.Empty)
                    {
                        Grid[x, y].Type = GetRandomGemType();
                        Grid[x, y].Bonus = BonusType.None;
                        Grid[x, y].Position = new Vector2(x * 60 + 160 + 2, -100);
                        Grid[x, y].TargetPosition = new Vector2(x * 60 + 160 + 2, y * 60 + 60 + 2);
                    }
                }
            }
        }

        public int ProcessBoardAndGetScore(Point? p1 = null, Point? p2 = null)
        {
            var horizMatches = GetHorizontalMatches();
            var vertMatches = GetVerticalMatches();

            if (horizMatches.Count == 0 && vertMatches.Count == 0) return 0;

            HashSet<Point> toDestroy = new HashSet<Point>();
            Dictionary<Point, BonusType> newBonuses = new Dictionary<Point, BonusType>();
            Dictionary<Point, GemType> bonusColors = new Dictionary<Point, GemType>();

            foreach (var m in horizMatches) foreach (var p in m) toDestroy.Add(p);
            foreach (var m in vertMatches) foreach (var p in m) toDestroy.Add(p);

            var intersections = horizMatches.SelectMany(hm => vertMatches.Where(vm => hm.Intersect(vm).Any()).SelectMany(vm => hm.Intersect(vm))).Distinct().ToList();

            foreach (var p in intersections)
            {
                newBonuses[p] = BonusType.Bomb;
                bonusColors[p] = Grid[p.X, p.Y].Type;
            }

            var allMatches = Enumerable.Concat(horizMatches, vertMatches).ToList();
            foreach (var match in allMatches)
            {
                if (match.Count >= 5)
                {
                    Point target = GetBonusTarget(match, p1, p2);
                    if (!newBonuses.ContainsKey(target))
                    {
                        newBonuses[target] = BonusType.Bomb;
                        bonusColors[target] = Grid[target.X, target.Y].Type;
                    }
                }
                else if (match.Count == 4)
                {
                    Point target = GetBonusTarget(match, p1, p2);
                    if (!newBonuses.ContainsKey(target))
                    {
                        bool isHorizontal = (match[0].Y == match[1].Y);
                        newBonuses[target] = isHorizontal ? BonusType.LineVertical : BonusType.LineHorizontal;
                        bonusColors[target] = Grid[target.X, target.Y].Type;
                    }
                }
            }

            int scoreEarned = 0;
            foreach (Point p in toDestroy)
            {
                scoreEarned += DestroyGemAndTrigger(p.X, p.Y);
            }

            foreach (var kvp in newBonuses)
            {
                Point p = kvp.Key;
                Grid[p.X, p.Y].Type = bonusColors[p];
                Grid[p.X, p.Y].Bonus = kvp.Value;
            }

            ApplyGravityAndRefill();

            return scoreEarned;
        }
    }
}
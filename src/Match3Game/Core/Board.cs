namespace Match3Game.Core;

public class Board
{
    public const int Width = 8;
    public const int Height = 8;

    public Gem[,] Grid { get; private set; }
    private Random _random;


    public List<Destroyer> Destroyers { get; private set; }
    public List<ActiveBomb> ActiveBombs { get; private set; }
    public int PendingScore { get; set; } = 0; // the score that will be added after all effects are processed

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

    /// <summary>
    /// CreatesMatch checks if placing a gem of the given type at (x, y) would create a match of 3 or more in a row.
    /// </summary>
    private bool CreatesMatch(int x, int y, GemType type)
    {
        if (x >= 2 && Grid[x - 1, y] != null && Grid[x - 1, y].Type == type && Grid[x - 2, y] != null && Grid[x - 2, y].Type == type) return true;
        if (y >= 2 && Grid[x, y - 1] != null && Grid[x, y - 1].Type == type && Grid[x, y - 2] != null && Grid[x, y - 2].Type == type) return true;
        return false;
    }

    /// <summary>
    ///  GetRandomGemType returns a random GemType (excluding Empty) to fill the board. 
    ///  It uses the Random instance to generate a number between 1 and 5, which corresponds to the valid gem types.
    /// </summary>
    private GemType GetRandomGemType() => (GemType)_random.Next(1, 6);

    /// <summary>
    /// GetHorizontalMatches scans the board for horizontal matches of 3 or more gems of the same type. 
    /// It returns a list of matches, where each match is a list of Points representing the positions
    /// of the matched gems. The function iterates through each row and counts consecutive gems of 
    /// the same type, adding matches to the result list when a sequence of 3 or more is found.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// GetVerticalMatches scans the board for vertical matches of 3 or more gems of the same type.
    /// </summary>
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

    /// <summary>
    /// HassAnyMatch is a helper function that checks if there are any matches 
    /// on the board by calling both GetHorizontalMatches and GetVerticalMatches.
    /// </summary>
    /// <returns></returns>
    public bool HasAnyMatch() => GetHorizontalMatches().Count > 0 || GetVerticalMatches().Count > 0;

    /// <summary>
    /// GetBonusTarget determines the position where a bonus gem should be 
    /// created for a given match.
    /// </summary>
    private Point GetBonusTarget(List<Point> match, Point? p1, Point? p2)
    {
        if (p1.HasValue && match.Contains(p1.Value)) return p1.Value;
        if (p2.HasValue && match.Contains(p2.Value)) return p2.Value;
        return match[match.Count / 2];
    }

    /// <summary>
    /// DestroyGemAndTrigger is the function that destroys a gem at the specified (x, y) 
    /// position and triggers any effects associated with it.
    /// </summary>
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

        return 10; 
    }

    /// <summary>
    /// UpdateEffects is the function that updates the state of 
    /// active effects on the board, such as bombs and destroyers.
    public void UpdateEffects(float dt)
    {
        bool boardChanged = false;

        // Update bombs and trigger their explosions if their timers have run out
        for (int i = ActiveBombs.Count - 1; i >= 0; i--)
        {
            ActiveBombs[i].Timer -= dt;
            if (ActiveBombs[i].Timer <= 0)
            {
                int cx = ActiveBombs[i].X;
                int cy = ActiveBombs[i].Y;
                ActiveBombs.RemoveAt(i);

                // Destroy the 3x3 area around the bomb
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

        // Update destroyers: move them and destroy any gems they pass through
        foreach (var d in Destroyers.ToList())
        {
            d.Position += d.Direction * 400f * dt;

            int gridX = (int)((d.Position.X - 160) / 60);
            int gridY = (int)((d.Position.Y - 60) / 60);

            // Check if the destroyer is still within the board bounds and destroy any gem it passes through
            if (gridX >= 0 && gridX < Width && gridY >= 0 && gridY < Height)
            {
                if (Grid[gridX, gridY].Type != GemType.Empty)
                {
                    PendingScore += DestroyGemAndTrigger(gridX, gridY);
                    boardChanged = true;
                }
            }
            else
            {
                d.IsActive = false; 
            }
        }
        Destroyers.RemoveAll(d => !d.IsActive);

        if (boardChanged) ApplyGravityAndRefill();
    }


    /// <summary>
    /// The function that applies gravity to the board and refills empty spaces with new gems. It first makes existing gems
    /// fall down to fill empty spaces, and then generates new gems at the top for any remaining empty spaces.
    /// This function is called after matches are destroyed and also after effects like bombs and destroyers have been processed.
    /// </summary>
    public void ApplyGravityAndRefill()
    {
        // First, make existing gems fall down to fill empty spaces
        for (int x = 0; x < Width; x++)
        {
            // Start from the bottom of the column and move upwards
            for (int y = Height - 1; y >= 0; y--)
            {
                // If we find an empty space, look upwards for the nearest non-empty gem to fall down
                if (Grid[x, y].Type == GemType.Empty)
                {
                    // Look upwards for the nearest non-empty gem
                    for (int k = y - 1; k >= 0; k--)
                    {
                        // If we find a non-empty gem, move it down to the current empty space
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

        // Then, generate new gems at the top for any remaining empty spaces
        for (int x = 0; x < Width; x++)
        {
            // Start from the top of the column and move downwards
            for (int y = 0; y < Height; y++)
            {
                // If we find an empty space, generate a new gem there
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

    /// <summary>
    /// The main function that processes the board after a move: finds matches, destroys gems,
    /// creates bonuses, applies gravity, and returns the score earned from this cascade. 
    /// It also takes into account the positions of the swapped gems to prioritize bonus creation.
    /// </summary>
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

        // Find intersections of horizontal and vertical matches to create bombs
        var intersections = horizMatches.SelectMany(hm => vertMatches.Where(vm => hm.Intersect(vm).Any()).SelectMany(vm => hm.Intersect(vm))).Distinct().ToList();

        // For each intersection point, create a bomb bonus and store its color for later assignment
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
        // Destroy all matched gems and trigger their effects, accumulating score
        foreach (Point p in toDestroy)
        {
            scoreEarned += DestroyGemAndTrigger(p.X, p.Y);
        }

        // Assign bonuses to the appropriate gems after destruction, ensuring they are created in the correct positions
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
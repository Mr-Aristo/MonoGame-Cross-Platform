namespace Match3Game.Screens;

/// <summary>
/// Class GameplayScreen is responsible for managing the main gameplay of the match-3 game.
/// It handles the game board, user input for swapping gems, scoring, and the game timer.
/// It also manages the visual representation of the game state, including drawing the gems,
/// bonuses, and any active effects like bombs or destroyers. The class interacts with the
/// Board class to process game logic and updates the score based on player actions and 
/// matches made on the board.
/// </summary>
public class GameplayScreen : BaseScreen
{
    private Board _board;
    private Texture2D _pixelTexture;
    private Point? _selectedCell = null; 
    private bool _isSwapping = false; 
    private int _score = 0;
    private SpriteFont _font;
    private ContentManager _content;
    private const int CellSize = 60;
    private Vector2 _boardOffset = new Vector2(160, 60);
    private float _timeLeft = 60f;

    public GameplayScreen(GraphicsDevice graphicsDevice, ContentManager content)
    {
        _board = new Board();
        _content = content;
        
        //creating a 1x1 white texture to use for drawing rectangles (gems, bonuses, etc.)
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        _font = content.Load<SpriteFont>("GameFont");

        for (int x = 0; x < Board.Width; x++)
        {
            for (int y = 0; y < Board.Height; y++)
            {
                if (_board.Grid[x, y] != null)
                {
                    _board.Grid[x, y].Position = GetVisualPosition(x, y);
                    _board.Grid[x, y].TargetPosition = GetVisualPosition(x, y);
                }
            }
        }

    }

    /// <summary>
    ///  the visual position of a cell in the grid based on its x and y coordinates.
    /// </summary>
    private Vector2 GetVisualPosition(int x, int y)
    {
        return new Vector2(_boardOffset.X + (x * CellSize) + 2, _boardOffset.Y + (y * CellSize) + 2);
    }

    /// <summary>
    /// Update method is responsible for handling the game logic that needs to be processed every frame.
    /// </summary>
    public override void Update(GameTime gameTime)
    {
        for (int x = 0; x < Board.Width; x++)
        {
            for (int y = 0; y < Board.Height; y++)
            {
                if (_board.Grid[x, y] != null)
                    _board.Grid[x, y].Update(gameTime);
            }
        }

        
        _timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        // finish game if time is up
        if (_timeLeft <= 0)
        {
            ScreenManager.ChangeScreen(new GameOverScreen(_pixelTexture.GraphicsDevice, _content, _score));
            return;
        }


        // rockets and bombs update
        _board.UpdateEffects((float)gameTime.ElapsedGameTime.TotalSeconds);

        // add pending score to total score and reset pending score
        _score += _board.PendingScore;
        _board.PendingScore = 0;

        if (_board.HasAnyMatch() && _board.ActiveBombs.Count == 0 && _board.Destroyers.Count == 0 && !_isSwapping)
        {
            _score += _board.ProcessBoardAndGetScore();
        }

        // if there are active bombs or destroyers, we don't allow player input to swap gems
        if (_isSwapping) return;

        // check for mouse click
        if (InputManager.IsLeftMouseClicked())
        {
            int mouseX = InputManager.MouseRectangle.X;
            int mouseY = InputManager.MouseRectangle.Y;

            // convert mouse coordinates to grid coordinates
            int gridX = (mouseX - (int)_boardOffset.X) / CellSize;
            int gridY = (mouseY - (int)_boardOffset.Y) / CellSize;

            //check if the click is within the board boundaries
            if (gridX >= 0 && gridX < Board.Width && gridY >= 0 && gridY < Board.Height)
            {
                HandleCellClick(gridX, gridY);
            }
            else
            {
                // clicked outside the board, deselect any selected cell
                _selectedCell = null;
            }
        }
    }

    /// <summary>
    ///  the HandleCellClick method is responsible for handling the logic when a player clicks on a cell in the game board.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void HandleCellClick(int x, int y)
    {
        // if the clicked cell is empty, ignore the click
        if (_board.Grid[x, y] == null || _board.Grid[x, y].Type == GemType.Empty) return;

        if (_selectedCell == null)
        {
            // first click on a cell, select it
            _selectedCell = new Point(x, y);
        }
        else
        {
            // second click on a cell, check if it's adjacent to the first selected cell
            int dx = System.Math.Abs(_selectedCell.Value.X - x);
            int dy = System.Math.Abs(_selectedCell.Value.Y - y);

            // if the second clicked cell is adjacent to the first one, swap them
            if ((dx == 1 && dy == 0) || (dx == 0 && dy == 1))
            {
                // start swapping and set the flag to prevent other input until swap is processed
                SwapGems(_selectedCell.Value, new Point(x, y));
            }
            // if the second clicked cell is not adjacent, just select the new cell

            _selectedCell = null;
        }
    }

    /// <summary>
    /// The SwapGems method is responsible for swapping two gems on the board based on their grid coordinates.
    /// </summary>
    private void SwapGems(Point p1, Point p2)
    {
        _board.Grid[p1.X, p1.Y].TargetPosition = GetVisualPosition(p2.X, p2.Y);
        _board.Grid[p2.X, p2.Y].TargetPosition = GetVisualPosition(p1.X, p1.Y);

        Gem temp = _board.Grid[p1.X, p1.Y];
        _board.Grid[p1.X, p1.Y] = _board.Grid[p2.X, p2.Y];
        _board.Grid[p2.X, p2.Y] = temp;

        if (!_board.HasAnyMatch())
        {
            // if there is no match after the swap, swap back immediately (this will be animated in the future)
            temp = _board.Grid[p1.X, p1.Y];
            _board.Grid[p1.X, p1.Y] = _board.Grid[p2.X, p2.Y];
            _board.Grid[p2.X, p2.Y] = temp;
            _board.Grid[p1.X, p1.Y].TargetPosition = GetVisualPosition(p1.X, p1.Y);
            _board.Grid[p2.X, p2.Y].TargetPosition = GetVisualPosition(p2.X, p2.Y);
        }
        else
        {

            int gainedScore;
            bool isFirstMatch = true;
            do
            {
                if (isFirstMatch)
                {
                    // the first match after the swap, we pass the swapped positions to calculate any potential bonuses correctly
                    gainedScore = _board.ProcessBoardAndGetScore(p1, p2);
                    isFirstMatch = false;
                }
                else
                {
                    // if there are subsequent matches from falling gems, we don't need to pass specific positions
                    gainedScore = _board.ProcessBoardAndGetScore(null, null);
                }

                _score += gainedScore;
            }
            while (gainedScore > 0);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
 
        spriteBatch.GraphicsDevice.Clear(Color.Black);
        spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(20, 20), Color.White);
        spriteBatch.DrawString(_font, $"Time: {Math.Ceiling(_timeLeft)}", new Vector2(20, 60), Color.Yellow);

        
        for (int x = 0; x < Board.Width; x++)
        {
            for (int y = 0; y < Board.Height; y++)
            {
                Gem currentGem = _board.Grid[x, y];

                if (currentGem != null && currentGem.Type != GemType.Empty)
                {
                    // set stone color based on its type
                    Color gemColor = GetColorForGemType(currentGem.Type);

                    // stone rectangle for drawing
                    Rectangle rect = new Rectangle((int)currentGem.Position.X, (int)currentGem.Position.Y, CellSize - 4, CellSize - 4);

                    // draw stone
                    spriteBatch.Draw(_pixelTexture, rect, gemColor);
                    if (currentGem.Bonus != BonusType.None)
                    {
                        Rectangle bonusRect = new Rectangle();

                        if (currentGem.Bonus == BonusType.LineHorizontal)
                        {
                            // horizontal line bonus: a horizontal white line across the middle of the gem
                            bonusRect = new Rectangle(rect.X + 5, rect.Y + (rect.Height / 2) - 3, rect.Width - 10, 6);
                            spriteBatch.Draw(_pixelTexture, bonusRect, Color.White);
                        }
                        else if (currentGem.Bonus == BonusType.LineVertical)
                        {
                            // vertical line bonus: a vertical white line across the middle of the gem
                            bonusRect = new Rectangle(rect.X + (rect.Width / 2) - 3, rect.Y + 5, 6, rect.Height - 10);
                            spriteBatch.Draw(_pixelTexture, bonusRect, Color.White);
                        }
                        else if (currentGem.Bonus == BonusType.Bomb)
                        {
                            // Bomb bonus: a white square in the center of the gem
                            bonusRect = new Rectangle(rect.X + (rect.Width / 2) - 8, rect.Y + (rect.Height / 2) - 8, 16, 16);
                            spriteBatch.Draw(_pixelTexture, bonusRect, Color.White);
                        }
                    }
                    // if this cell is currently selected, we can draw a highlight around it to indicate selection
                    if (_selectedCell.HasValue && _selectedCell.Value.X == x && _selectedCell.Value.Y == y)
                    {
                        // draw a semi-transparent white rectangle over the gem to indicate it's selected
                        spriteBatch.Draw(_pixelTexture, rect, Color.White * 0.5f);
                    }
                }
            }
        }
        foreach (var destroyer in _board.Destroyers)
        {
            Rectangle dRect = new Rectangle((int)destroyer.Position.X, (int)destroyer.Position.Y, CellSize - 4, CellSize - 4);
            spriteBatch.Draw(_pixelTexture, dRect, Color.Cyan);
        }
    }

    /// <summary>
    /// this method returns a specific color for each type of gem, 
    /// which is used when drawing the gems on the screen.
    /// </summary>
    private Color GetColorForGemType(GemType type)
    {
        switch (type)
        {
            case GemType.RedSquare: return Color.Red;
            case GemType.BlueCircle: return Color.CornflowerBlue;
            case GemType.GreenTriangle: return Color.Green;
            case GemType.YellowStar: return Color.Yellow;
            case GemType.PurpleHexagon: return Color.Purple;
            default: return Color.Transparent;
        }
    }
}
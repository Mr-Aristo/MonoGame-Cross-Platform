namespace Match3Game.Screens;

/// <summary>
/// This class represents the Game Over screen that is displayed when the player loses the game.
/// </summary>
public class GameOverScreen : BaseScreen
{
    private Rectangle _okButtonRect;
    private Texture2D _pixelTexture;
    private SpriteFont _font;
    private ContentManager _content;
    private int _finalScore;

 
    public GameOverScreen(GraphicsDevice graphicsDevice, ContentManager content, int finalScore)
    {
        _content = content;
        _finalScore = finalScore;

        // Define the rectangle for the "OK" button
        _okButtonRect = new Rectangle(350, 300, 100, 50);

        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Load the font for displaying text
        _font = _content.Load<SpriteFont>("GameFont");
    }

    public override void Update(GameTime gameTime)
    {
        // Check if the mouse is over the "OK" button and if it is clicked
        if (_okButtonRect.Intersects(InputManager.MouseRectangle))
        {
            // If the left mouse button is clicked, change to the main menu screen
            if (InputManager.IsLeftMouseClicked())
            {
                // Change to the main menu screen
                ScreenManager.ChangeScreen(new MainMenuScreen(_pixelTexture.GraphicsDevice, _content));
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Clear the screen with a dark red color to indicate game over
        spriteBatch.GraphicsDevice.Clear(Color.DarkRed);
      
        spriteBatch.DrawString(_font, "GAME OVER", new Vector2(320, 150), Color.White);
        spriteBatch.DrawString(_font, $"Final Score: {_finalScore}", new Vector2(330, 200), Color.Yellow);

     
        Color buttonColor = _okButtonRect.Intersects(InputManager.MouseRectangle) ? Color.LightGray : Color.White;
        spriteBatch.Draw(_pixelTexture, _okButtonRect, buttonColor);

        
        spriteBatch.DrawString(_font, "OK", new Vector2(380, 315), Color.Black);
    }
}
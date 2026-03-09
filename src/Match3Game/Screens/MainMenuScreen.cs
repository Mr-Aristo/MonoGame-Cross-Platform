namespace Match3Game.Screens;

/// <summary>
/// This class represents the Main Menu screen of the game.
/// It displays a "Play" button that the player can click to start the game.
/// </summary>
public class MainMenuScreen : BaseScreen
{
    private Rectangle _playButtonRect;
    private Texture2D _pixelTexture;
    private ContentManager _content;
    private SpriteFont _font;
    public MainMenuScreen(GraphicsDevice graphicsDevice, ContentManager content)
    {
        
        _playButtonRect = new Rectangle(300, 200, 200, 80);
       
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        _content = content;
        _font = _content.Load<SpriteFont>("GameFont");
    }
    public override void Update(GameTime gameTime)
    {
        // Check if the mouse is over the play button and if it's clicked
        if (_playButtonRect.Intersects(InputManager.MouseRectangle))
        {
            // Change the button color on hover (handled in Draw) and check for click
            if (InputManager.IsLeftMouseClicked())
            {
                // Transition to the GameplayScreen when the play button is clicked
                ScreenManager.ChangeScreen(new GameplayScreen(_pixelTexture.GraphicsDevice, _content));
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(Color.DarkBlue);

        Color buttonColor = _playButtonRect.Intersects(InputManager.MouseRectangle) ? Color.LightGray : Color.Red;
        
        spriteBatch.Draw(_pixelTexture, _playButtonRect, buttonColor);
        spriteBatch.DrawString(_font, "PLAY", new Vector2(360, 225), Color.White);
    }
}

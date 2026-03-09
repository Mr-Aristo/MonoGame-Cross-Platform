namespace Match3Game.Screens;

/// <summary>
/// This is the base class for all screens in the game. 
/// It defines the basic structure and methods that all screens must implement, such as Update and Draw.
/// </summary>
public abstract class BaseScreen
{
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);

}

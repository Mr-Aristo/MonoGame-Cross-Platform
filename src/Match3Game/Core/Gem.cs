namespace Match3Game.Core;

public class Gem
{
    public GemType Type { get; set; }
    public BonusType Bonus { get; set; }

    
    public Vector2 Position { get; set; }       // screen coordinates of the gem (for animation purposes)
    public Vector2 TargetPosition { get; set; } // to which the gem should move (based on its grid position)

    public Gem(GemType type)
    {
        Type = type;
        Bonus = BonusType.None;
    }

    // Lerp method to smoothly move the gem towards its target position
    public void Update(GameTime gameTime)
    {
        // If the gem is not close enough to its target position, move it closer
        if (Vector2.Distance(Position, TargetPosition) > 1f)
        {
            // Move the gem towards the target position using linear interpolation (lerp)
            Position = Vector2.Lerp(Position, TargetPosition, 0.2f);
        }
        else
        {
            // If the gem is close enough to the target position, snap it to the target position
            // If the gem is close enough to the target position, snap it to the target position
            Position = TargetPosition;
        }
    }
}
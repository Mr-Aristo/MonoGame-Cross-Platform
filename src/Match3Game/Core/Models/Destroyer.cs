using Microsoft.Xna.Framework;

namespace Match3Game.Core.Models;

/// <summary>
/// This class represents a destroyer in the game, 
/// which is responsible for destroying tiles in a specific direction.
/// </summary>
public class Destroyer
{
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }
    public bool IsActive { get; set; } = true;
}

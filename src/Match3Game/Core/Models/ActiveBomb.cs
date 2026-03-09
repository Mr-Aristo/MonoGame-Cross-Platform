namespace Match3Game.Core.Models;

/// <summary>
/// This class represents an active bomb on the game board. 
/// It contains the position of the bomb and a timer that counts 
/// down until the bomb explodes. When the timer reaches zero, the 
/// bomb will detonate, affecting the surrounding tiles.
/// </summary>
public class ActiveBomb
{
    public int X { get; set; }
    public int Y { get; set; }
    public float Timer { get; set; } = 0.250f; // 250 milisecond delay
}

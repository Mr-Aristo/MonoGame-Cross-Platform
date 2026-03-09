using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3Game.Core;

public enum GemType
{
    Empty, 
    RedSquare,
    BlueCircle,
    GreenTriangle,
    YellowStar,
    PurpleHexagon
}

public enum BonusType
{
    None,
    LineHorizontal,
    LineVertical,
    Bomb
}
public class Gem
{
    public GemType Type { get; set; }
    public BonusType Bonus { get; set; }

    // Animasyon için görsel pozisyonlar
    public Vector2 Position { get; set; }       // Ekrandaki ŞU ANKİ yeri
    public Vector2 TargetPosition { get; set; } // Ekranda GİTMESİ GEREKEN yer

    public Gem(GemType type)
    {
        Type = type;
        Bonus = BonusType.None;
    }

    // Taşı hedefine doğru yumuşakça kaydıran Lerp animasyonu!
    public void Update(GameTime gameTime)
    {
        // Eğer taş hedefine henüz ulaşmadıysa (aralarındaki mesafe 1 pikselden fazlaysa)
        if (Vector2.Distance(Position, TargetPosition) > 1f)
        {
            // Her frame'de aradaki mesafenin %20'si kadar hedefe yaklaş (Yumuşak duruş efekti - Ease Out)
            Position = Vector2.Lerp(Position, TargetPosition, 0.2f);
        }
        else
        {
            // Çok yaklaştıysa tam üstüne oturt (titremeyi önlemek için)
            Position = TargetPosition;
        }
    }
}
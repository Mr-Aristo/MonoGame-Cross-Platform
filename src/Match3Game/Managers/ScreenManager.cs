using Match3Game.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3Game.Managers;

/// <summary>
/// the ScreenManager class is responsible for managing the current screen of the game.
/// It allows changing screens, updating the current screen, and drawing the current screen.
/// This is a simple implementation and can be expanded to include features like screen transitions,
/// a stack of screens for navigation, etc.
/// </summary>
public static class ScreenManager
{
    private static BaseScreen _currentScreen;

    public static void ChangeScreen(BaseScreen newScreen)
    {
        _currentScreen = newScreen;
    }

    public static void Update(GameTime gameTime)
    {
        _currentScreen?.Update(gameTime);
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        _currentScreen?.Draw(spriteBatch);
    }
}
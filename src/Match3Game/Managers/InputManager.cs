using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Match3Game.Managers;

public static class InputManager
{
    private static MouseState _currentMouseState;
    private static MouseState _previousMouseState;

    // Her frame'de (saniyede 60 kez) çağrılacak
    public static void Update()
    {
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
    }

    // Sadece farenin sol tuşuna *ilk* basıldığı anı yakalar
    public static bool IsLeftMouseClicked()
    {
        return _currentMouseState.LeftButton == ButtonState.Pressed &&
               _previousMouseState.LeftButton == ButtonState.Released;
    }

    // Farenin ekrandaki koordinatlarını 1x1 piksellik bir dikdörtgen olarak verir. 
    // Bu, butonların (dikdörtgenlerin) üstüne gelip gelmediğimizi anlamak (Kesişim/Intersect) için çok işimize yarayacak!
    public static Rectangle MouseRectangle => new Rectangle(_currentMouseState.X, _currentMouseState.Y, 1, 1);
}

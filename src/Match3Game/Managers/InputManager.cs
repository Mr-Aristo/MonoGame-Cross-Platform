namespace Match3Game.Managers;

/// <summary>
/// This class is responsible for managing user input, specifically mouse input in this case.
/// </summary>
public static class InputManager
{
    private static MouseState _currentMouseState;
    private static MouseState _previousMouseState;

    // 60 times will be called in a second, so we can check for mouse clicks in real-time
    public static void Update()
    {
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
    }

    /// <summary>
    ///  This method checks if the left mouse button was just clicked in the current frame.
    /// </summary>
    public static bool IsLeftMouseClicked()
    {
        return _currentMouseState.LeftButton == ButtonState.Pressed &&
               _previousMouseState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// This method checks if the right mouse button was just clicked in the current frame.
    /// </summary>
    public static Rectangle MouseRectangle => new Rectangle(_currentMouseState.X, _currentMouseState.Y, 1, 1);
}

using Match3Game.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace Match3Game.Screens;

public class MainMenuScreen : BaseScreen
{
    private Rectangle _playButtonRect;
    private Texture2D _pixelTexture;
    private ContentManager _content;
    private SpriteFont _font;
    public MainMenuScreen(GraphicsDevice graphicsDevice, ContentManager content)
    {
        // Ekranın ortasına denk gelecek 200x80 piksellik bir buton alanı tanımlıyoruz
        _playButtonRect = new Rectangle(300, 200, 200, 80);

        // 1x1 piksellik beyaz bir resim (doku) üretiyoruz
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        _content = content;
        _font = _content.Load<SpriteFont>("GameFont");
    }
    public override void Update(GameTime gameTime)
    {
        // Eğer fare (MouseRectangle), Play butonunun (PlayButtonRect) üzerindeyse VE sol tıklandıysa:
        if (_playButtonRect.Intersects(InputManager.MouseRectangle))
        {
            if (InputManager.IsLeftMouseClicked())
            {
                // Mülakat Görevi Madde 2: Play'e basılınca Oyun Ekranı açılır!
                ScreenManager.ChangeScreen(new GameplayScreen(_pixelTexture.GraphicsDevice, _content));
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(Color.DarkBlue);

        // Fare butonun üzerindeyse rengi Gri olsun (Hover efekti), değilse Kırmızı olsun
        Color buttonColor = _playButtonRect.Intersects(InputManager.MouseRectangle) ? Color.LightGray : Color.Red;

        // Beyaz pikselimizi, _playButtonRect boyutlarına esneterek ve seçtiğimiz renge boyayarak çiziyoruz
        spriteBatch.Draw(_pixelTexture, _playButtonRect, buttonColor);
        spriteBatch.DrawString(_font, "PLAY", new Vector2(360, 225), Color.White);
    }
}

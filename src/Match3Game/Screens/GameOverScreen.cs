using Match3Game.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Match3Game.Screens
{
    public class GameOverScreen : BaseScreen
    {
        private Rectangle _okButtonRect;
        private Texture2D _pixelTexture;
        private SpriteFont _font;
        private ContentManager _content;
        private int _finalScore;

        // Ekrana geçerken GraphicsDevice ve Content dışında, oyuncunun Skorunu da alıyoruz!
        public GameOverScreen(GraphicsDevice graphicsDevice, ContentManager content, int finalScore)
        {
            _content = content;
            _finalScore = finalScore;

            // "Ok" butonu için 100x50 piksellik bir alan (Ekranın ortasına yakın)
            _okButtonRect = new Rectangle(350, 300, 100, 50);

            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            // Yazıları yazabilmek için Gameplay'de kullandığımız fontu burada da yüklüyoruz
            _font = _content.Load<SpriteFont>("GameFont");
        }

        public override void Update(GameTime gameTime)
        {
            // Fare "Ok" butonunun üzerindeyse ve tıklandıysa:
            if (_okButtonRect.Intersects(InputManager.MouseRectangle))
            {
                if (InputManager.IsLeftMouseClicked())
                {
                    // Madde 14: Ana Menüye geri dön!
                    ScreenManager.ChangeScreen(new MainMenuScreen(_pixelTexture.GraphicsDevice, _content));
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Oyun bittiği için arka planı dramatik bir Koyu Kırmızı yapalım
            spriteBatch.GraphicsDevice.Clear(Color.DarkRed);

            // "GAME OVER" ve "Final Score" yazıları
            spriteBatch.DrawString(_font, "GAME OVER", new Vector2(320, 150), Color.White);
            spriteBatch.DrawString(_font, $"Final Score: {_finalScore}", new Vector2(330, 200), Color.Yellow);

            // "Ok" butonunun çizimi (Fare üzerindeyse Gri, değilse Beyaz olsun)
            Color buttonColor = _okButtonRect.Intersects(InputManager.MouseRectangle) ? Color.LightGray : Color.White;
            spriteBatch.Draw(_pixelTexture, _okButtonRect, buttonColor);

            // Butonun tam ortasına Siyah renkle "OK" yazalım
            spriteBatch.DrawString(_font, "OK", new Vector2(380, 315), Color.Black);
        }
    }
}
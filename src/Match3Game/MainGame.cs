using Match3Game.Managers;
using Match3Game.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Match3Game
{
    public class MainGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            ScreenManager.ChangeScreen(new MainMenuScreen(GraphicsDevice, Content));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            InputManager.Update();
            ScreenManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {     // TODO: Add your drawing code here

            _spriteBatch.Begin();

            // Hangi ekrandaysak (şu an MainMenuScreen), ona "Al bu spriteBatch'i ve kendi içeriğini çiz" diyoruz.
            ScreenManager.Draw(_spriteBatch);

            // Çizim işlemini bitiriyoruz (Bu çok önemlidir, End demezsek ekranda hiçbir şey görünmez)
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

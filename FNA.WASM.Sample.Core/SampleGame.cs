using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FNA.WASM.Sample.Core;

public class SampleGame : Game
{
    private KeyboardState _keyboardPrev = new KeyboardState();
    private MouseState _mousePrev = new MouseState();
    private GamePadState _gamepadPrev = new GamePadState();

    private SpriteBatch _spriteBatch;
    private Texture2D _texture;
    
    public SampleGame()
    {
        var gdm = new GraphicsDeviceManager(this);

        gdm.PreferredBackBufferWidth = 600;
        gdm.PreferredBackBufferHeight = 400;
        gdm.IsFullScreen = false;
        gdm.SynchronizeWithVerticalRetrace = true; //TODO: does this do anything on WebGL?

        Content.RootDirectory = "/assets";
    }
    
    protected override void Initialize()
    {
        /* This is a nice place to start up the engine, after
         * loading configuration stuff in the constructor
         */
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _texture = Content.Load<Texture2D>("img/test1.png");
    }

    protected override void UnloadContent()
    {
        _spriteBatch.Dispose();
        _texture.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();
        var gamepad = GamePad.GetState(PlayerIndex.One);

        if (keyboard.IsKeyUp(Keys.Space) && _keyboardPrev.IsKeyDown(Keys.Space))
        {
            Console.WriteLine("Space bar pressed!");
        }

        if (mouse.LeftButton == ButtonState.Released && _mousePrev.LeftButton == ButtonState.Pressed)
        {
            Console.WriteLine("Mouse clicked!");
        }

        if (gamepad.Buttons.A == ButtonState.Released && _gamepadPrev.Buttons.A == ButtonState.Pressed)
        {
            Console.WriteLine("Gamepad A pressed!");
        }

        _keyboardPrev = keyboard;
        _mousePrev = mouse;
        _gamepadPrev = gamepad;
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render stuff in here. Do NOT run game logic in here!
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture, Vector2.Zero, Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}
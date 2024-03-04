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
    private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    private class Entity
    {
        private const float LinearDrag = 0.01f;
        private const float AngularDrag = 0.01f;
        
        public string Sprite { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 DeltaPosition { get; set; }
        public float Rotation { get; set; }
        public float DeltaRotation { get; set; }
        public float Scale { get; set; }

        public Vector2? CachedAnchor { get; set; } = null;

        public void Update(float deltaTime)
        {
            Position += DeltaPosition * deltaTime;
            DeltaPosition *= 1.0f - (LinearDrag * deltaTime);

            Rotation += DeltaRotation * deltaTime;
            while (Rotation < 0.0f)
            {
                Rotation += 360.0f;
            }
            while (Rotation > 360.0f)
            {
                Rotation -= 360.0f;
            }

            DeltaRotation *= 1.0f - (AngularDrag * deltaTime);
        }
    }
    private Entity? _ship;
    private List<Entity> _meteors = new List<Entity>();

    private Vector2 _viewportOffset = Vector2.Zero;
    
    public SampleGame()
    {
        var gdm = new GraphicsDeviceManager(this);

        gdm.PreferredBackBufferWidth = 600;
        gdm.PreferredBackBufferHeight = 400;
        gdm.IsFullScreen = false;
        gdm.SynchronizeWithVerticalRetrace = true; //TODO: does this do anything on WebGL?
        _viewportOffset = new Vector2(-gdm.PreferredBackBufferWidth / 2.0f, -gdm.PreferredBackBufferHeight / 2.0f);

        Content.RootDirectory = "/assets";
    }
    
    protected override void Initialize()
    {
        var rnd = new Random();
        _ship = new Entity()
        {
            Sprite = "ship1",
            Position = Vector2.Zero,
            DeltaPosition = Vector2.Zero,
            Rotation = 0.0f,
            DeltaRotation = 0.0f,
            Scale = 0.5f
        };
        var wVar = 300;
        var hVar = 200;
        for (var i = 0; i < 10; i++)
        {
            var sprite = "meteor" + rnd.Next(1, 5);
            var x = rnd.Next(-wVar, wVar);
            var y = rnd.Next(-hVar, hVar);
            var dx = rnd.Next(-1000, 1000) / 50.0f;
            var dy = rnd.Next(-1000, 1000) / 50.0f;
            var r = rnd.Next(0, 3600)/100.0f;
            var dr = rnd.Next(-100, 100) / 10.0f;
            _meteors.Add(new Entity()
            {
                Sprite = sprite,
                Position = new Vector2(x, y),
                DeltaPosition = new Vector2(dx, dy),
                Rotation = r,
                DeltaRotation = dr,
                Scale = 0.2f
            });
        }
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _textures["ship1"] = Content.Load<Texture2D>("img/ship1.png");
        _textures["meteor1"] = Content.Load<Texture2D>("img/meteor1.png");
        _textures["meteor2"] = Content.Load<Texture2D>("img/meteor2.png");
        _textures["meteor3"] = Content.Load<Texture2D>("img/meteor3.png");
        _textures["meteor4"] = Content.Load<Texture2D>("img/meteor4.png");
    }

    protected override void UnloadContent()
    {
        try
        {
            _spriteBatch.Dispose();
        }
        catch (Exception)
        {
            //no issue
        }
        foreach (var pair in _textures)
        {
            try
            {
                pair.Value.Dispose();
            }
            catch (Exception)
            {
                //no issue
            }
        }
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

        if (_ship != null)
        {
            _ship.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
        foreach (var entity in _meteors)
        {
            entity.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render stuff in here. Do NOT run game logic in here!
        GraphicsDevice.Clear(Color.DarkBlue);

        var cameraPos = _ship?.Position ?? Vector2.Zero;
        cameraPos += _viewportOffset;

        _spriteBatch.Begin();
        if (_ship != null)
        {
            Draw(cameraPos, _ship);
        }

        foreach (var entity in _meteors)
        {
            Draw(cameraPos, entity);
        }
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    private void Draw(Vector2 cameraPos, Entity entity)
    {
        //TODO: camera rotation
        if (entity.CachedAnchor == null)
        {
            var t = _textures[entity.Sprite];
            entity.CachedAnchor = new Vector2(t.Width/2.0f, t.Height/2.0f);
        }
        var oPos = entity.Position - cameraPos;
        _spriteBatch.Draw(_textures[entity.Sprite], oPos, null, Color.White, entity.Rotation, entity.CachedAnchor.Value,
            Vector2.One * entity.Scale, SpriteEffects.None, 0.0f);
    }
}
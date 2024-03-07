using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FNA.WASM.Sample.Core;

public class SampleGame : Game
{
    private KeyboardState _keyboardPrev = new KeyboardState();
    private MouseState _mousePrev = new MouseState();
    private GamePadState _gamepadPrev = new GamePadState();

    private SpriteBatch _spriteBatch;
    private FontSystem _fontSystem;
    private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
    
    private readonly Dictionary<string, SoundEffect> _sounds = new Dictionary<string, SoundEffect>();
    private readonly Dictionary<string, Song> _songs = new Dictionary<string, Song>();

    private bool _audioInit = false;

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
    private bool _shouldPlaySound = false;
    private bool _shouldBePlayingMusic = false;

    private Vector2 _viewportOffset = Vector2.Zero;
    private float _rollingRenderFps = 30.0f;
    private float _rollingUpdateFps = 30.0f;
    private const float RollingHistory = 30.0f;
    
    public SampleGame()
    {
        var gdm = new GraphicsDeviceManager(this);

        gdm.PreferredBackBufferWidth = 600;
        gdm.PreferredBackBufferHeight = 400;
        gdm.IsFullScreen = false;
        gdm.SynchronizeWithVerticalRetrace = true; //TODO: does this do anything on WebGL?
        _viewportOffset = new Vector2(-gdm.PreferredBackBufferWidth / 2.0f, -gdm.PreferredBackBufferHeight / 2.0f);

        Content.RootDirectory = "assets";
    }

    public void OnAudioAllowedToInit()
    {
        //we have to wait for a user to interact with the app before we're allowed to use the audio system
        if (!_audioInit)
        {
            Console.WriteLine("Initialising audio");
            _audioInit = true;
            _songs["music"] = Content.Load<Song>("audio/music.ogg");
            _sounds["click"] = Content.Load<SoundEffect>("audio/click.wav");
        }
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
        
        _fontSystem = new FontSystem();
        _fontSystem.AddFont(File.ReadAllBytes(Content.RootDirectory + "/fonts/Roboto-Regular.ttf"));

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
        //calculate update FPS
        var lastFramerate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
        _rollingUpdateFps = (_rollingUpdateFps * (RollingHistory - 1) + lastFramerate) / RollingHistory;

        if (_audioInit)
        {
            if (_shouldPlaySound)
            {
                _sounds["click"].Play();
                _shouldPlaySound = false;
            }

            if (_shouldBePlayingMusic &&
                (MediaPlayer.State == MediaState.Stopped || MediaPlayer.State == MediaState.Paused))
            {
                Console.WriteLine("Starting to play music");
                MediaPlayer.Play(_songs["music"]);
            }
            else if (!_shouldBePlayingMusic && MediaPlayer.State == MediaState.Playing)
            {
                Console.WriteLine("Stopping music");
                MediaPlayer.Stop();
            }
        }

        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();
        var gamepad = GamePad.GetState(PlayerIndex.One);

        if (keyboard.IsKeyUp(Keys.Space) && _keyboardPrev.IsKeyDown(Keys.Space))
        {
            Console.WriteLine("Space bar pressed!");
            _shouldBePlayingMusic = !_shouldBePlayingMusic;
        }

        if (mouse.LeftButton == ButtonState.Released && _mousePrev.LeftButton == ButtonState.Pressed)
        {
            Console.WriteLine("Mouse clicked!");
            _shouldPlaySound = true;
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
        //calculate render FPS
        var lastFramerate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
        _rollingRenderFps = (_rollingRenderFps * (RollingHistory - 1) + lastFramerate) / RollingHistory;
        
        GraphicsDevice.Clear(Color.DarkBlue);

        var cameraPos = _ship?.Position ?? Vector2.Zero;
        cameraPos += _viewportOffset;

        _spriteBatch.Begin();

        var font = _fontSystem.GetFont(18.0f);
        _spriteBatch.DrawString(font, $"Render FPS: {_rollingRenderFps:F0} ({lastFramerate:F0})", Vector2.Zero, Color.White);
        _spriteBatch.DrawString(font, $"Update FPS: {_rollingUpdateFps:F0}", new Vector2(0.0f, 20.0f), Color.White);
        
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
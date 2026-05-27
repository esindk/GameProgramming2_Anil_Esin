using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace esin;

public enum GameState
{
    Menu,
    Settings,
    Playing
}

public class Button
{
    public Rectangle Bounds { get; set; }
    public string Text { get; set; }
    public bool IsHovered { get; set; }

    public Button(Rectangle bounds, string text)
    {
        Bounds = bounds;
        Text = text;
        IsHovered = false;
    }

    public bool IsClicked(MouseState mouse)
    {
        return Bounds.Contains(mouse.Position) && mouse.LeftButton == ButtonState.Pressed;
    }
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private SpriteFont _gameFont;
    private TileMap _map;
    private Texture2D _tileset;
    private Camera2D _camera;

    // Game State
    private GameState _gameState = GameState.Menu;

    // Menu UI
    private Button _playButton = null!;
    private Button _settingsButton = null!;
    private Button _musicButton = null!;
    private Button _backButton = null!;
    private Button _exitButton = null!;
    private bool _isMusicEnabled = true;
    private MouseState _previousMouseState;

    // Player/Game
    private Vector2 _playerPosition;
    private Vector2 _playerVelocity;
    private const int PlayerWidth = 40;
    private const int PlayerHeight = 60;
    private float _gravityDirection = 1f;
    private const float GravityStrength = 1000f;
    private const float PlayerSpeed = 220f;
    private const float JumpStrength = 420f;
    private bool _isGrounded;
    private KeyboardState _previousKeyboard;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "esin and anil oyun";
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
    }

    protected override void Initialize()
    {
        _playerPosition = new Vector2(100, 400);
        _playerVelocity = Vector2.Zero;

        int buttonWidth = 200;
        int buttonHeight = 50;
        int centerX = (_graphics.PreferredBackBufferWidth - buttonWidth) / 2;
        _camera = new Camera2D(GraphicsDevice.Viewport);

        _playButton = new Button(new Rectangle(centerX, 150, buttonWidth, buttonHeight), "PLAY");
        _settingsButton = new Button(new Rectangle(centerX, 220, buttonWidth, buttonHeight), "SETTINGS");
        _exitButton = new Button(new Rectangle(centerX, 290, buttonWidth, buttonHeight), "EXIT");
        _backButton = new Button(new Rectangle(centerX, 350, buttonWidth, buttonHeight), "BACK");
        _musicButton = new Button(new Rectangle(centerX, 220, buttonWidth, buttonHeight), $"MUSIC: {(_isMusicEnabled ? "ON" : "OFF")}");

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _tileset = Content.Load<Texture2D>("tilesetbaba");

        _map = new TileMap(_tileset, mapW: 90, mapH: 30, tileW: 16, tileH: 16, tileScale: 2);

        _map.Load(
            "Content/Maps/TileMap_Background.csv",
            "Content/Maps/TileMap_Foreground.csv",
            "Content/Maps/TileMap_Spikes.csv",
            "Content/Maps/TileMap_Red_Blocks.csv",
            "Content/Maps/TileMap_Blue_Blocks.csv"
        );
        _map.Position = new Vector2(-1200, 0);

        try
        {
            _gameFont = Content.Load<SpriteFont>("Fontx");
        }
        catch
        {
            _gameFont = null;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();

        if (_gameState == GameState.Menu)
        {
            UpdateMenu(mouse);
        }
        else if (_gameState == GameState.Settings)
        {
            UpdateSettings(mouse);
        }
        else if (_gameState == GameState.Playing)
        {
            UpdateGame(gameTime, keyboard);
        }

        Console.WriteLine("Is player grounded: " + _isGrounded);

        _previousMouseState = mouse;
        base.Update(gameTime);
    }

    private void UpdateMenu(MouseState mouse)
    {
        _playButton.IsHovered = _playButton.Bounds.Contains(mouse.Position);
        _settingsButton.IsHovered = _settingsButton.Bounds.Contains(mouse.Position);
        _exitButton.IsHovered = _exitButton.Bounds.Contains(mouse.Position);

        if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            if (_playButton.Bounds.Contains(mouse.Position))
            {
                _gameState = GameState.Playing;
                ResetGame();
            }
            else if (_settingsButton.Bounds.Contains(mouse.Position))
            {
                _gameState = GameState.Settings;
            }
            else if (_exitButton.Bounds.Contains(mouse.Position))
            {
                Exit();
            }
        }
    }

    private void UpdateSettings(MouseState mouse)
    {
        _musicButton.IsHovered = _musicButton.Bounds.Contains(mouse.Position);
        _backButton.IsHovered = _backButton.Bounds.Contains(mouse.Position);

        if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            if (_musicButton.Bounds.Contains(mouse.Position))
            {
                _isMusicEnabled = !_isMusicEnabled;
                _musicButton.Text = $"MUSIC: {(_isMusicEnabled ? "ON" : "OFF")}";
            }
            else if (_backButton.Bounds.Contains(mouse.Position))
            {
                _gameState = GameState.Menu;
            }
        }
    }

    private void Die()
    {
        _playerPosition = new Vector2(100, 400);
        _playerVelocity = Vector2.Zero;
        _gravityDirection = 1f;
        _isGrounded = false;
    }

    private void ResetGame()
    {
        Die();
        _map.ToggleState = true;
        _previousKeyboard = Keyboard.GetState();
    }

    private void UpdateGame(GameTime gameTime, KeyboardState keyboard)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            _gameState = GameState.Menu;
            _previousKeyboard = keyboard;
            return;
        }

        float move = 0f;

        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
            move = -PlayerSpeed;

        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
            move = PlayerSpeed;

        _playerVelocity.X = move;

        if ((keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) && _isGrounded)
        {
            _playerVelocity.Y = -JumpStrength * _gravityDirection;
            _isGrounded = false;
        }

        // toggle blocks
        if (keyboard.IsKeyDown(Keys.E) && !_previousKeyboard.IsKeyDown(Keys.E))
        {
            _map.ToggleState = !_map.ToggleState;
            var pr = new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, PlayerWidth, PlayerHeight);
            if (IsTouchingSolid(pr))
                Die();
        }

        // gravity groundcheck
        if (keyboard.IsKeyDown(Keys.Space) && !_previousKeyboard.IsKeyDown(Keys.Space))
        {
            if (_isGrounded)
            {
                _gravityDirection *= -1f;
                _isGrounded = false;
            }
        }

        _playerVelocity.Y += GravityStrength * _gravityDirection * dt;

        // X movement and collision
        _playerPosition.X += _playerVelocity.X * dt;

        Rectangle playerRect = new Rectangle(
            (int)_playerPosition.X,
            (int)_playerPosition.Y,
            PlayerWidth,
            PlayerHeight
        );

        if (IsTouchingSolid(playerRect))
        {
            _playerPosition.X -= _playerVelocity.X * dt;   // X'i geri al
            _playerVelocity.X = 0f;
            // _isGrounded'a DOKUNMA — yatay çarpışma grounded yapmaz
        }

        // Y movement and collision
        _playerPosition.Y += _playerVelocity.Y * dt;

        playerRect = new Rectangle(
            (int)_playerPosition.X,
            (int)_playerPosition.Y,
            PlayerWidth,
            PlayerHeight
        );

        _isGrounded = false;

        if (IsTouchingSolid(playerRect))
        {
            _playerPosition.Y -= _playerVelocity.Y * dt;

            // sadece gravity yönünde düşerken çarptıysak grounded olalım
            if (_gravityDirection > 0f && _playerVelocity.Y > 0f)
                _isGrounded = true;
            if (_gravityDirection < 0f && _playerVelocity.Y < 0f)
                _isGrounded = true;

            _playerVelocity.Y = 0f;
        }
        _camera.Follow(_playerPosition);

        if (_playerPosition.Y > 2000 || _playerPosition.Y < -1000)
        {
            ResetGame();
        }

        _previousKeyboard = keyboard;
    }

    private bool IsTouchingSolid(Rectangle rect)
    {
        int tileSize = _map.TileW * _map.TileScale;

        int leftTile = (int)Math.Floor((rect.Left - _map.Position.X) / tileSize);
        int rightTile = (int)Math.Floor((rect.Right - 1 - _map.Position.X) / tileSize);

        int topTile = (int)Math.Floor((rect.Top - _map.Position.Y) / tileSize);
        int bottomTile = (int)Math.Floor((rect.Bottom - 1 - _map.Position.Y) / tileSize);

        for (int y = topTile; y <= bottomTile; y++)
        {
            for (int x = leftTile; x <= rightTile; x++)
            {
                if (_map.IsSolidTile(x, y))
                    return true;
            }
        }

        return false;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(25, 25, 40));

        _spriteBatch.Begin();

        if (_gameState == GameState.Menu)
        {
            DrawMenu();
        }
        else if (_gameState == GameState.Settings)
        {
            DrawSettings();
        }
        else if (_gameState == GameState.Playing)
        {
            DrawGame();
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawMenu()
    {
        DrawRectangle(new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), new Color(18, 18, 30));

        DrawSimpleText("ESIN GAME", _graphics.PreferredBackBufferWidth / 2, 50, Color.Cyan);

        DrawButton(_playButton, Color.LimeGreen);
        DrawButton(_settingsButton, Color.Yellow);
        DrawButton(_exitButton, Color.Red);
    }

    private void DrawSettings()
    {
        DrawRectangle(new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), new Color(18, 18, 30));

        DrawSimpleText("SETTINGS", _graphics.PreferredBackBufferWidth / 2, 50, Color.Yellow);

        DrawButton(_musicButton, _isMusicEnabled ? Color.LimeGreen : Color.Red);
        DrawButton(_backButton, Color.White);
    }

    private void DrawGame()
    {
        // Önce normal batch'i kapat
        _spriteBatch.End();

        // Kamera ile yeni batch başlat
        _spriteBatch.Begin(
        SpriteSortMode.Deferred,
        BlendState.AlphaBlend,
        SamplerState.PointClamp,
        null,
        null,
        null,
        _camera.GetViewMatrix()
      );

        // MAP
        _map.DrawBackground(_spriteBatch);
        if (_map.ToggleState) _map.DrawRed(_spriteBatch);
        else                  _map.DrawBlue(_spriteBatch);
        _map.DrawSpikes(_spriteBatch);
        _map.DrawForeground(_spriteBatch);

        // PLAYER
        DrawRectangle(
        new Rectangle(
          (int)_playerPosition.X,
          (int)_playerPosition.Y,
          PlayerWidth,
          PlayerHeight
        ),
        new Color(235, 130, 70)
      );

        // Kamera batch'ini kapat
        _spriteBatch.End();

        // UI için normal batch
        _spriteBatch.Begin();

        DrawSimpleText(
          "A/D: Move | W: Jump | Space: Flip | E: Toggle | ESC: Menu",
          10,
          10,
          Color.White
        );
    }
    private void DrawButton(Button button, Color color)
    {
        Color bgColor = button.IsHovered ? new Color(color.R / 2, color.G / 2, color.B / 2) : color;
        DrawRectangle(button.Bounds, bgColor);
        DrawRectangleBorder(button.Bounds, Color.White, 2);

        if (_gameFont != null)
        {
            Vector2 textSize = _gameFont.MeasureString(button.Text);
            Vector2 textPosition = new Vector2(
              button.Bounds.Center.X - textSize.X / 2,
              button.Bounds.Center.Y - textSize.Y / 2
            );

            _spriteBatch.DrawString(_gameFont, button.Text, textPosition, Color.Black);
        }
        else
        {
            DrawSimpleTextFallback(button.Text, button.Bounds.Center.X, button.Bounds.Center.Y);
        }
    }

    private void DrawSimpleText(string text, int x, int y, Color color)
    {
        if (_gameFont != null)
        {
            _spriteBatch.DrawString(_gameFont, text, new Vector2(x, y), color);
        }
        else
        {
            DrawSimpleTextFallback(text, x, y);
        }
    }

    private void DrawSimpleTextFallback(string text, int x, int y)
    {
        int boxWidth = text.Length * 8;
        int boxHeight = 14;
        DrawRectangle(new Rectangle(x - boxWidth / 2, y - boxHeight / 2, boxWidth, boxHeight), Color.White);
    }

    private void DrawRectangle(Rectangle rect, Color color)
    {
        _spriteBatch.Draw(_pixel, rect, color);
    }

    private void DrawRectangleBorder(Rectangle rect, Color color, int thickness)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    }
}

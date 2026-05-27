using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

// I added this class to handle tilemap loading from CSV files, drawing layers (background, foreground, spikes),
// and checking for solid tiles for collision detection with the player.
//only foreground layer is solid for collisions.
//I had a lot of trouble implementing this feature but with the help of ai and some tutorials from codewithsphre I managed to do it.
// the map was broken I tried for 3 hours then I realised that I forgot to set firstGid to 1 it was 0 by default and that caused the issue.

namespace esin;



public sealed class TileMap  // TileMap class to handle tilemap loading and rendering
{
    public int FirstGid { get; set; } = 1;   
    public int Spacing { get; set; } = 0;   
     public int Margin  { get; set; } = 0;    

    public int TileW { get; }
    public int TileH { get; }
    public int TileScale { get; }
    public int Width { get; }
    public int Height { get; }
    public Vector2 Position = Vector2.Zero;

    private readonly Texture2D _tileset; // Tileset texture First I put TileMap instead of tileset here and it caused an error took me a while to figure it out.

    private int[,] _bg = null!;
    private int[,] _fg = null!;

    private int[,] _sp   = null!;
    private int[,] _red  = null!;
    private int[,] _blue = null!;

    public bool ToggleState { get; set; } = true;

    public TileMap(Texture2D tileset, int mapW, int mapH, int tileW, int tileH, int tileScale = 1) // Constructor to initialize the tilemap
    {
        _tileset = tileset ?? throw new ArgumentNullException(nameof(tileset));
        Width = mapW;
        Height = mapH;
        TileW = tileW;
        TileH = tileH;
        TileScale = Math.Max(5, tileScale);
    }

    public void Load(string backgroundCsvPath, string foregroundCsvPath,
                     string spikesCsvPath = "", string redCsvPath = "", string blueCsvPath = "")
    {
        _bg   = LoadCsv(backgroundCsvPath, Width, Height);
        _fg   = LoadCsv(foregroundCsvPath, Width, Height);
        _sp   = spikesCsvPath != "" ? LoadCsv(spikesCsvPath, Width, Height) : new int[Height, Width];
        _red  = redCsvPath    != "" ? LoadCsv(redCsvPath,    Width, Height) : new int[Height, Width];
        _blue = blueCsvPath   != "" ? LoadCsv(blueCsvPath,   Width, Height) : new int[Height, Width];
    }
    public void DrawSpikes(SpriteBatch sb)     => DrawLayer(sb, _sp);
    public void DrawBackground(SpriteBatch sb) => DrawLayer(sb, _bg);
    public void DrawForeground(SpriteBatch sb) => DrawLayer(sb, _fg);
    public void DrawRed(SpriteBatch sb)        => DrawLayer(sb, _red);
    public void DrawBlue(SpriteBatch sb)       => DrawLayer(sb, _blue);

    
    public bool IsSolidTile(int tx, int ty) // Checks if the tile at (tx, ty) is solid (for collision)
    {
        if (tx < 0 || ty < 0 || tx >= Width || ty >= Height) return true;
        if (_fg[ty, tx] > 0) return true;
        if (ToggleState  && _red [ty, tx] > 0) return true;
        if (!ToggleState && _blue[ty, tx] > 0) return true;
        return false;
    }

    public Rectangle GetTileBounds(int tx, int ty)
    {
        int sw = TileW * TileScale;
        int sh = TileH * TileScale;
        return new Rectangle(
    (int)Position.X + tx * sw,
    (int)Position.Y + ty * sh,
    sw,
    sh
);
    }

    private void DrawLayer(SpriteBatch sb, int[,] grid)
    {
        int columns = _tileset.Width / TileW; 

        int sw = TileW * TileScale;
        int sh = TileH * TileScale;

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            int gid = grid[y, x];
            if (gid <= 0) continue;

            
            int index = gid;

            int sx = (index % columns) * TileW;
            int sy = (index / columns) * TileH;

            var src = new Rectangle(sx, sy, TileW, TileH);
           var dst = new Rectangle(
            (int)Position.X + x * sw,
            (int)Position.Y + y * sh,
            sw,
            sh
            );
            sb.Draw(_tileset, dst, src, Color.White);
        }
    }

    private static int[,] LoadCsv(string relativePath, int width, int height) // Loads a CSV file and returns a 2D array of tile GIDs
    {
        string baseDir = AppContext.BaseDirectory;
        string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        string fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);

if (!File.Exists(fullPath))
{
    fullPath = Path.Combine(AppContext.BaseDirectory, "Content", relativePath);
}

if (!File.Exists(fullPath))
{
    fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
}

if (!File.Exists(fullPath))
{
    fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Content", relativePath);
}

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                "CSV not found.\n" +
                $"BaseDir: {baseDir}\n" +
                $"Tried:   {fullPath}\n" +
                "Beklenen: bin/.../Content/Maps/TileMap_Background.csv"
            );
        }

       
        string[] lines = File.ReadAllLines(fullPath); // Read all lines from the CSV file and parse them into a 2D array

        int[,] grid = new int[height, width];

        if (lines.Length < height)
            throw new Exception($"CSV row mismatch. Expected {height} rows, got {lines.Length}");

        for (int y = 0; y < height; y++)
        {
            string line = lines[y].Trim();
            if (string.IsNullOrEmpty(line))
                throw new Exception($"CSV empty line at row {y}");

            string[] parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < width)
                throw new Exception($"CSV col mismatch at row {y}. Expected {width} cols, got {parts.Length}");

            for (int x = 0; x < width; x++)
                grid[y, x] = int.Parse(parts[x]);
        }

        return grid;
    }
}

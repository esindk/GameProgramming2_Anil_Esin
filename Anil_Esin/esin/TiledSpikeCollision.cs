using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
// I added this class to load spike collision rectangles from Tiled JSON export
// I used ai and watched some tutorials to implement this feature.
// so basically this script reads the Tiled JSON file, finds the specified object layer,
// and extracts rectangle objects to use for spike collision detection in the game.
public static class TiledSpikeCollision
{
    public static List<Rectangle> LoadSpikeRects(string jsonPath, string layerName = "spikecollision", int worldScale = 1)
    {
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException("Tiled JSON yok: " + jsonPath);

        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var root = doc.RootElement;

        if (!root.TryGetProperty("layers", out var layers))
            throw new Exception("JSON içinde 'layers' yok.");

        foreach (var layer in layers.EnumerateArray())
        {
            string type = layer.GetProperty("type").GetString() ?? "";
            if (type != "objectgroup") continue;

            string name = layer.GetProperty("name").GetString() ?? "";
            if (!string.Equals(name, layerName, StringComparison.OrdinalIgnoreCase))
                continue;

            var rects = new List<Rectangle>();

            if (!layer.TryGetProperty("objects", out var objects))
                return rects;

            foreach (var obj in objects.EnumerateArray())
            {
                float x = obj.GetProperty("x").GetSingle();
                float y = obj.GetProperty("y").GetSingle();
                float w = obj.GetProperty("width").GetSingle();
                float h = obj.GetProperty("height").GetSingle();

                // Scale to world units 
                int rx = (int)MathF.Round(x * worldScale);
                int ry = (int)MathF.Round(y * worldScale);
                int rw = (int)MathF.Round(w * worldScale);
                int rh = (int)MathF.Round(h * worldScale);

                rects.Add(new Rectangle(rx, ry, rw, rh));
            }

            return rects; 
        }

        
        return new List<Rectangle>();
    }
}

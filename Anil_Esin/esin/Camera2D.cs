using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
// I added this class to implement a 2D camera that follows the player smoothly,
// clamps to world bounds, and provides a view matrix for rendering.

namespace esin
{
    public class Camera2D
    {
        public Vector2 Position;
        public float Zoom = 1f;

        private Viewport _viewport;
        public Rectangle WorldBounds; 

        public Camera2D(Viewport viewport)
        {
            _viewport = viewport;
        }

                public void Follow(Vector2 target, float smooth = 0.12f)
        {
            // Calculate desired position to center the target
            Vector2 desired = target - new Vector2(_viewport.Width * 0.5f, _viewport.Height * 0.5f);

            
            Position = Vector2.Lerp(Position, desired, smooth); // used lerp for smooth followinng

            
            Position.X = MathF.Round(Position.X); // Pixel-perfect rounding
            Position.Y = MathF.Round(Position.Y); 
        }

        public void ClampToWorld() // Clamps the camera position to the defined world bounds
        {
            
            Position.X = MathHelper.Clamp(Position.X, WorldBounds.Left, WorldBounds.Right - _viewport.Width);
            Position.Y = MathHelper.Clamp(Position.Y, WorldBounds.Top, WorldBounds.Bottom - _viewport.Height);

            Position.X = MathF.Round(Position.X);
            Position.Y = MathF.Round(Position.Y);
        }
        // matrix is used to transform the world coordinates to camera view
        public Matrix GetViewMatrix() // Returns the view matrix for rendering
        {
            
            float x = MathF.Round(Position.X);
            float y = MathF.Round(Position.Y);

            return Matrix.CreateTranslation(-x, -y, 0f);
        }
    }
}

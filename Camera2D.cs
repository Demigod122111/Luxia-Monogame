using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Luxia;

public class Camera2D
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public float Zoom { get; set; } = 1f;

    private readonly Viewport _viewport;
    public Viewport Viewport => _viewport;

    public Camera2D(Viewport viewport)
    {
        _viewport = viewport;
    }

    public Matrix GetViewMatrix()
    {
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom, Zoom, 1f) *
            Matrix.CreateTranslation(new Vector3(_viewport.Width * 0.5f, _viewport.Height * 0.5f, 0f));
    }

    // Convert screen space → world space
    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, Matrix.Invert(GetViewMatrix()));
    }

    // Convert world space → screen space
    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Vector2.Transform(worldPos, GetViewMatrix());
    }

    // Optional: Clamp camera so it doesn’t show outside the world
    public void ClampToWorld(Rectangle worldBounds)
    {
        var cameraWorldMin = ScreenToWorld(Vector2.Zero);
        var cameraWorldMax = ScreenToWorld(new Vector2(_viewport.Width, _viewport.Height));
        var cameraSize = cameraWorldMax - cameraWorldMin;

        Position = new Vector2(
            MathHelper.Clamp(Position.X, worldBounds.Left + cameraSize.X / 2, worldBounds.Right - cameraSize.X / 2),
            MathHelper.Clamp(Position.Y, worldBounds.Top + cameraSize.Y / 2, worldBounds.Bottom - cameraSize.Y / 2)
        );
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Luxia.UI;

public class Label : UIElement
{
    public string Text { get; set; } = string.Empty;
    public SpriteFont Font { get; set; }
    public Color Color { get; set; } = Color.White;
    public Vector2 Origin { get; set; } = Vector2.Zero; // allows centering or alignment
    public float Scale { get; set; } = 1f;

    public Label()
    {
        AcceptEvents = false;
    }
    public Label(SpriteFont font, string text = "")
    {
        AcceptEvents = false;
        Font = font;
        Text = text;
    }

    public override void Render(Camera2D camera)
    {
        if (Font == null || string.IsNullOrEmpty(Text))
            return;

        Application.SpriteBatch.DrawString(Font, Text, Position, Color, 0f, Origin, Scale, SpriteEffects.None, 0f);
        Size = Font.MeasureString(Text) * Scale;
    }

    public Vector2 Measure()
    {
        if (Font == null || string.IsNullOrEmpty(Text))
            return Vector2.Zero;

        return Font.MeasureString(Text) * Scale;
    }
}

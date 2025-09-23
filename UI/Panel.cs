using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Luxia.UI;

public class Panel : UIElement
{
    public Color BackgroundColor = Color.Gray * 0.5f;
    public Texture2D? Texture;
    public List<UIElement> Children { get; } = new();

    public override void Update(Camera2D camera)
    {
        base.Update(camera);
        foreach (var child in Children)
            child.Update(camera);
    }

    public void AddChild(UIElement child)
    {
        Children.Add(child);
    }

    public void RemoveChild(UIElement child)
    {
        Children.Remove(child);
    }

    public override void Render(Camera2D camera)
    {
        if (!IsVisible) return;

        Application.SpriteBatch.Draw(Texture ?? UIManager.WhiteTexture, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), BackgroundColor);

        foreach (var child in Children)
            child.Render(camera);
    }
}

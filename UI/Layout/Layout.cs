using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Luxia.UI.Layout;

public abstract class Layout : UIElement
{
    public Vector2 Padding = Vector2.Zero;
    public Vector2 Spacing = Vector2.Zero;

    public override void Update(Camera2D camera)
    {
        AcceptEvents = false;
        base.Update(camera);
        LayoutChildren();
        foreach (var child in Children)
        {
            child.UIManager = UIManager;
            child.Update(camera);
        }
    }

    public override void Render(Camera2D camera)
    {
        if (!IsVisible) return;

        LayoutChildren();
        foreach (var child in Children)
        {
            child.UIManager = UIManager;
            child.Render(camera);
        }

        Size = Vector2.Zero;
    }

    protected abstract void LayoutChildren();
}

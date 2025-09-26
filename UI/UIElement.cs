using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Luxia.UI;

public abstract class UIElement
{
    internal UIManager? UIManager
    {
        get 
        {
            if (uIManager == null && AutoFallbackUIManager)
            {
                var parent = Parent;
                if (parent != null)
                    uIManager = parent.UIManager;
                uIManager ??= Application.ActiveScene?.UIManager;
            }
            return uIManager;
        }

        set => uIManager = value;
    }

    private UIManager? uIManager;
    public bool AutoFallbackUIManager { get; set; } = true;

    public UIElement Parent { get; private set; }
    public List<UIElement> Children { get; } = new();

    public Vector2 Position
    {
        get
        {
            return (Parent?.Position ?? Vector2.Zero) + localPosition;
        }

        set
        {
            localPosition = value - (Parent?.Position ?? Vector2.Zero);
        }
    }
    public Vector2 LocalPosition { get => localPosition; set => localPosition = value; }
    Vector2 localPosition = Vector2.Zero;

    public Vector2 Size = new(100, 100);

    public Rectangle BoundsRect => new Rectangle(
        (int)Position.X, (int)Position.Y,
        (int)Size.X, (int)Size.Y
    );

    public Rectangle EffectiveClipRect =>
        ClipRect == Rectangle.Empty ? BoundsRect : ClipRect;


    public Rectangle ClipRect = Rectangle.Empty;
    /// <summary>
    /// For mouse detection only
    /// </summary>
    public bool IsWorldUI = false;
    public bool IsVisible => (Parent?.IsVisible ?? true) && IsVisibleSelf;
    public bool IsVisibleSelf = true;
    public bool IsEnabled = true;
    public bool AcceptEvents = true;

    /// <summary>
    /// Higher layers render on top of lower ones.
    /// </summary>
    public int RenderLayer { get; set; } = 0;

    /// <summary>
    /// Order within the same layer. Higher comes last.
    /// </summary>
    public int RenderOrder { get; set; } = 0;

    public virtual void Update(Camera2D camera) { }
    public abstract void Render(Camera2D camera);

    // Helper for mouse over detection in virtual space
    public virtual bool ContainsPoint(Point point)
    {
        if (!Input.IsMouseWithinFrame)
            return false;

        return EffectiveClipRect.Contains(point);
    }

    public bool IsTopMostAt(Point point, bool eventOnly = false)
    {
        if (UIManager == null || !IsVisible)
            return false;

        if (!ContainsPoint(point) || (eventOnly && !AcceptEvents))
            return false;

        var top = UIManager.GetTopMostAt(point, eventOnly);
        return top == this;
    }

    /*
    public bool IsTopMostAt(Point point, bool eventOnly = false)
    {
        if (UIManager == null || !IsVisible)
            return false;

        if (!ContainsPoint(point) || (eventOnly && !AcceptEvents))
            return false;

        // Walk through all elements managed by this UIManager
        var elements = UIManager.Elements;
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            var e = elements[i];
            if (!e.IsVisible)
                continue;

            if (e.ContainsPoint(point) && (!eventOnly || e.AcceptEvents))
            {
                // First element from the top that contains point
                return e == this;
            }
        }

        return false;
    }
    */

    public bool IsParentOf(UIElement element) => element.IsChildOf(this);
    public bool IsChildOf(UIElement element)
    {
        var parent = Parent;

        while (parent?.Parent != null)
        {
            if (parent.Parent == element)
                return true;
            parent = parent.Parent;
        }

        return parent == element;
    }

    public bool EventPoint(Point point) => ContainsPoint(point) && IsTopMostAt(point, true);
    public bool EventPassthroughPoint(Point point) => ContainsPoint(point) && (IsTopMostAt(point, false) || (UIManager?.GetTopMostAt(point, false)?.IsChildOf(this) ?? true));

    public void AddChild(UIElement child)
    {
        if (child.Parent != null && child.Parent != this)
            child.Parent.RemoveChild(child);

        child.UIManager = UIManager;

        child.Parent = this;

        if (!Children.Contains(child))
            Children.Add(child);
    }

    public void RemoveChild(UIElement child)
    {
        if (Children.Contains(child))
        {
            Children.Remove(child);
            child.UIManager = null;
        }

        if (child.Parent == this)
            child.Parent = null;
    }

    public void ClearChildren()
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            RemoveChild(Children[i]);
        }
    }

    public List<UIElement> GetDescendants()
    {
        var elements = new List<UIElement>();

        for (int i = 0; i < Children.Count; i++)
        {
            elements.Add(Children[i]);

            if (Children[i].Children.Count > 0)
                elements.AddRange(Children[i].GetDescendants());
        }

        return elements;
    }

    public static void ExpandBoundsRecursive(UIElement element,
                                   ref float minX, ref float minY,
                                   ref float maxX, ref float maxY)
    {
        var globalPos = element.Position;
        var elemMin = globalPos;
        var elemMax = globalPos + element.Size;

        if (elemMin.X < minX) minX = elemMin.X;
        if (elemMin.Y < minY) minY = elemMin.Y;
        if (elemMax.X > maxX) maxX = elemMax.X;
        if (elemMax.Y > maxY) maxY = elemMax.Y;

        foreach (var child in element.Children)
            ExpandBoundsRecursive(child, ref minX, ref minY, ref maxX, ref maxY);
    }
}

using Luxia.UI.Layout;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luxia.UI;

public class ScrollView : UIElement
{
    public Color BackgroundColor = Color.Gray * 0.2f;
    public Texture2D? Texture;
    public bool FitContent = true;
    public float ScrollSpeed = 1.5f;

    public bool HasVerticalScroll { get; set; } = true;
    public bool HasHorizontalScroll { get; set; } = false;

    public Vector2 ContentSize
    {
        get
        {
            if (!FitContent)
                return contentSize;

            if (Children.Count == 0)
                return Size; // no children, default to own size

            // Bounding box covering all descendants
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var child in Children)
            {
                ExpandBoundsRecursive(child, ref minX, ref minY, ref maxX, ref maxY);
            }

            return new Vector2(maxX - minX, maxY - minY);
        }

        set => contentSize = value;
    } // size of scrollable content
    private Vector2 contentSize;
    public Vector2 ScrollOffset
    {
        get => scrollOffset;
        set
        {
            if (value != scrollOffset)
                OnScrollChanged?.Invoke(value);

            scrollOffset = value;
        }
    }
    Vector2 scrollOffset; // current offset
    private Vector2 velocity;    // momentum for elastic feel

    private bool isDragging = false;
    private Point lastMouse;

    // Elastic scroll settings
    private const float ElasticStrength = 0.25f;
    private const float Damping = 0.9f;

    public Action<Vector2> OnScrollChanged;

    public void TransferScrollFrom(ScrollView scrollView)
    {
        if (scrollView == null)
            return;

        this.ScrollOffset = scrollView.ScrollOffset;
        this.velocity = scrollView.velocity;
        this.lastMouse = scrollView.lastMouse;
        this.isDragging = scrollView.isDragging;
    }

    public ScrollView()
    {
        Size = new(200, 300);
        ContentSize = new(200, 600);
        AcceptEvents = false;
    }

    Queue<Tuple<UIElement, bool, Rectangle>> savedVisible = new();

    private void ApplyClipping(UIElement element, Rectangle viewRect)
    {
        savedVisible.Clear();
        // Global position of this element
        var globalPos = element.Position;

        // If element has children, recurse (don’t clip container, clip leaves)
        if (element.Children.Count > 0)
        {
            foreach (var child in element.Children)
                ApplyClipping(child, viewRect);
        }
        else
        {
            // Compute child's global bounds
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            ExpandBoundsRecursive(element, ref minX, ref minY, ref maxX, ref maxY);
            var elemRect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
            // Leaf element -> clip
            savedVisible.Enqueue(new(element, element.IsVisibleSelf, element.ClipRect));
            element.IsVisibleSelf = viewRect.Intersects(elemRect);
            element.ClipRect = Rectangle.Intersect(viewRect, element.BoundsRect);
        }
    }

    private void UnapplyClipping()
    {
        while (savedVisible.Count > 0)
        {
            var item = savedVisible.Dequeue();
            item.Item1.IsVisibleSelf = item.Item2;
            item.Item1.ClipRect = item.Item3;
        }
    }

    public override void Update(Camera2D camera)
    {
        if (!IsEnabled || !IsVisible) return;

        var trueMouse = Input.MousePosition;
        Point mouse = IsWorldUI ? camera.ScreenToWorld(new(trueMouse.X, trueMouse.Y)).ToPoint() : trueMouse;

        // Mouse wheel scroll
        if (EventPassthroughPoint(mouse))
        {
            var wheel = Input.MouseWheelDelta;
            if (wheel.Y != 0 && HasVerticalScroll)
                velocity.Y -= wheel.Y * 0.25f;

            if (wheel.X != 0 && HasHorizontalScroll)
                velocity.X += wheel.X * 0.25f;

            // Drag scroll
            if (!isDragging && Input.IsMousePressed(MouseButton.Left))
            {
                isDragging = true;
                lastMouse = mouse;
            }
        }

        if (isDragging && Input.IsMousePressed(MouseButton.Left))
        {
            int dx = mouse.X - lastMouse.X;
            int dy = mouse.Y - lastMouse.Y;
            if (HasVerticalScroll)
            {
                ScrollOffset = new(ScrollOffset.X, ScrollOffset.Y - dy);
                velocity.Y = dy; // momentum
            }
            if (HasHorizontalScroll)
            {
                ScrollOffset = new(ScrollOffset.X - dx, ScrollOffset.Y);
                velocity.X = dx; // momentum
            }
            lastMouse = mouse;
        }
        if (Input.IsMouseReleased(MouseButton.Left))
        {
            isDragging = false;
        }

        // Apply velocity (momentum)
        ScrollOffset += velocity * Time.DeltaTime * ScrollSpeed;
        velocity *= Damping;

        // Elastic bounce if overscrolled
        float maxScrollY = Math.Max(0, ContentSize.Y - Size.Y);
        if (ScrollOffset.Y < 0)
        {
            ScrollOffset = new(ScrollOffset.X, ScrollOffset.Y + (-ScrollOffset.Y) * ElasticStrength);
            velocity.Y = 0;
        }
        else if (ScrollOffset.Y > maxScrollY)
        {
            float overshoot = ScrollOffset.Y - maxScrollY;
            ScrollOffset = new(ScrollOffset.X, ScrollOffset.Y - overshoot * ElasticStrength);
            velocity.Y = 0;
        }

        float maxScrollX = Math.Max(0, ContentSize.X - Size.X);
        if (ScrollOffset.X < 0)
        {
            ScrollOffset = new(ScrollOffset.X + (-ScrollOffset.X) * ElasticStrength, ScrollOffset.Y);
            velocity.X = 0;
        }
        else if (ScrollOffset.X > maxScrollX)
        {
            float overshoot = ScrollOffset.X - maxScrollX;
            ScrollOffset = new(ScrollOffset.X - overshoot * ElasticStrength, ScrollOffset.Y);
            velocity.X = 0;
        }

        if (!HasHorizontalScroll)
        {
            ScrollOffset = new(0, ScrollOffset.Y);
            velocity = new(0, velocity.Y);
        }
        if (!HasVerticalScroll)
        {
            ScrollOffset = new(ScrollOffset.X, 0);
            velocity = new(velocity.X, 0);
        }

        var viewRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        // Update children
        foreach (var child in Children)
        {
            var savedPos = child.Position;

            // Shift position just like Render does
            child.Position = savedPos - new Vector2(ScrollOffset.X, ScrollOffset.Y);
            child.UIManager = UIManager;
            ApplyClipping(child, viewRect);
            child.Update(camera);
            UnapplyClipping();
            child.Position = savedPos; // restore

        }
    }

    public override void Render(Camera2D camera)
    {
        if (!IsVisible) return;

        var rect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        Application.SpriteBatch.Draw(Texture ?? UIManager.WhiteTexture, rect, BackgroundColor);

        var oldRect = Application.GraphicsDevice.ScissorRectangle;

        Application.SpriteBatch.PushBegin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            null, new RasterizerState() { ScissorTestEnable = true });
        Application.GraphicsDevice.ScissorRectangle = rect;
        var viewRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

        // Render children with offset
        foreach (var child in Children)
        {
            var savedPos = child.Position;
            child.Position = savedPos - new Vector2(ScrollOffset.X, ScrollOffset.Y);
            child.UIManager = UIManager;
            ApplyClipping(child, viewRect);
            child.Render(camera);
            UnapplyClipping();
            child.Position = savedPos; // restore
        }
        Application.SpriteBatch.PopBegin();
        Application.GraphicsDevice.ScissorRectangle = oldRect;
    }
}

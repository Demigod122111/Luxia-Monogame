using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Luxia.UI;

public class UIManager
{
    public static Texture2D WhiteTexture { get; set; }

    private readonly List<UIElement> elements = new();
    internal IReadOnlyList<UIElement> RootElements
    {
        get
        {
            // Use a HashSet to keep uniqueness
            var set = new HashSet<UIElement>(elements);

            foreach (var e in immediateElements)
                set.Add(e);

            return [..set];
        }
    }

    internal IReadOnlyList<UIElement> Elements
    {
        get
        {
            // Use a HashSet to keep uniqueness
            var set = new HashSet<UIElement>();

            void add(UIElement e)
            {
                set.Add(e);

                var des = e.GetDescendants();

                foreach (var e2 in des)
                {
                    set.Add(e2);
                }
            }

            foreach (var e in elements)
                add(e);

            foreach (var e in immediateElements)
                add(e);

            return [.. set];
        }
    }

    private readonly List<UIElement> immediateElements = new();
    private readonly Queue<UIElement> immediateElementsUpdate = new();
    private readonly Queue<UIElement> immediateElementsRender = new();

    static Camera2D defaultCamera
    {
        get
        {
            return _defaultCamera ??= new(Application.GraphicsDevice.Viewport);
        }
    }
    static Camera2D _defaultCamera;

    /// <summary>
    /// Creates Immediate UI Element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public void FrameElement<T>(Action<T> setup) where T: UIElement, new()
    {
        T element = new();

        setup?.Invoke(element);
        element.UIManager = this;

        immediateElementsUpdate.Enqueue(element);
        immediateElementsRender.Enqueue(element);
        immediateElements.Add(element);
    }

    public void AddElement(UIElement element)
    {
        element.UIManager = this;
        elements.Add(element);
    }

    public void RemoveElement(UIElement element)
    {
        elements.Remove(element);
        element.UIManager = null;
    }

    public void Update(Camera2D? camera)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].UIManager = this;
            elements[i].Update(camera ?? defaultCamera);
        }

        while (immediateElementsUpdate.Count > 0)
        {
            var element = immediateElementsUpdate.Dequeue();
            element.UIManager = this;
            element.Update(camera ?? defaultCamera);
        }

        immediateElements.RemoveAll(x => !immediateElementsRender.Contains(x) && !immediateElementsUpdate.Contains(x));
    }

    public void Render(Camera2D? camera)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].Render(camera ?? defaultCamera);
        }

        int requeueCount = 0;
        while (immediateElementsRender.Count - requeueCount > 0)
        {
            var item = immediateElementsRender.Dequeue();
            if (immediateElementsUpdate.Contains(item))
            {
                immediateElementsRender.Enqueue(item);
                requeueCount++;
            }
            else item.Render(camera ?? defaultCamera);
        }
    }

    // Bring element to front
    public void BringToFront(UIElement element)
    {
        if (elements.Remove(element))
            elements.Add(element);
    }

    // Send element to back
    public void SendToBack(UIElement element)
    {
        if (elements.Remove(element))
            elements.Insert(0, element);
    }

    public UIElement? GetTopMostAt(Point point, bool eventOnly = false)
    {
        // Walk through all elements managed by this UIManager
        var elements = Elements;
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            var e = elements[i];
            if (!e.IsVisible)
                continue;

            if (e.ContainsPoint(point) && (!eventOnly || e.AcceptEvents))
            {
                // First element from the top that contains point
                return e;
            }
        }

        return null;
    }
    public void Clear() => elements.Clear();
}

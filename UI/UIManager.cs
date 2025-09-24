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

            foreach (var e in immediateElementsRendered)
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

            foreach (var e in RootElements)
                add(e);

            return [.. set];
        }
    }

    private IEnumerable<UIElement> OrderElementsForRender(IEnumerable<UIElement> elements)
    {
        var elementsL = elements.ToList();
        return elementsL
            .Where(e => e.IsVisible)
            .OrderBy(e => e.RenderLayer)
            .ThenBy(e => e.RenderOrder)
            .ThenBy(e => elementsL.IndexOf(e));
    }

    private readonly Queue<UIElement> immediateElementsToRender = new();
    private readonly List<UIElement> immediateElementsRendered = new();
    private readonly Queue<AdditionalRender> nextFrameAdditionalRender = new();

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

        immediateElementsToRender.Enqueue(element);
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

    public void SendAdditionalRender(AdditionalRender additionalRender) => nextFrameAdditionalRender.Enqueue(additionalRender);

    public void Update(Camera2D? camera)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].UIManager = this;
            elements[i].Update(camera ?? defaultCamera);
        }
    }

    public void Render(Camera2D? camera, List<AdditionalRender> additionalRenders=null)
    {
        additionalRenders ??= new();

        var cam = camera ?? defaultCamera;
        var elements = new HashSet<dynamic>(this.elements);

        var immediateElements = new List<UIElement>();

        foreach (var item in additionalRenders)
        {
            elements.Add(item);
        }

        while (nextFrameAdditionalRender.Count > 0)
        {
            elements.Add(nextFrameAdditionalRender.Dequeue());
        }

        while (immediateElementsToRender.Count > 0)
        {
            var item = immediateElementsToRender.Dequeue();
            elements.Add(item);
            immediateElements.Add(item);
        }

        immediateElementsRendered.Clear();
        immediateElementsRendered.AddRange(immediateElements);

        var ordered = elements
            .Where(e => e is UIElement ue ? ue.IsVisible : true)
            .Select((e, idx) => new { obj = e, idx })
            .OrderBy(x =>
            {
                if (x.obj is UIElement ue)
                    return (ue.RenderLayer, ue.RenderOrder, x.idx);
                else if (x.obj is AdditionalRender ar)
                    return (ar.RenderLayer, ar.RenderOrder, x.idx);
                else
                    return (0, 0, x.idx);
            })
            .Select(x => x.obj);

        // Finally render in that order (lower first, higher later)
        foreach (var obj in ordered)
        {
            if (obj is UIElement uiE)
            {
                if (immediateElements.Contains(uiE))
                    uiE.Update(cam);
                uiE.Render(cam);
            }
            else if (obj is AdditionalRender ar)
            {
                ar.Render?.Invoke(cam);
            }
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
        // Use the same ordering as Render(), but reversed (topmost → bottom)
        var ordered = OrderElementsForRender(RootElements).Reverse();

        var closed = new HashSet<UIElement>();

        UIElement? Search(UIElement root)
        {
            // Search children first, starting from the topmost
            foreach (var child in OrderElementsForRender(root.Children).Reverse())
            {
                var hit = Search(child);
                if (hit != null)
                    return hit;
            }

            if (root.ContainsPoint(point))
            {
                // If no child matches, this root is the hit
                return root;
            }

            closed.Add(root);

            return null;
        }

        foreach (var e in ordered)
        {
            var res = Search(e);
            if (res != null)
                return res;
        }

        return null;
    }


    public void Clear() => elements.Clear();
}

public class AdditionalRender
{
    /// <summary>
    /// Higher layers render on top of lower ones.
    /// </summary>
    public int RenderLayer { get; set; } = 0;

    /// <summary>
    /// Order within the same layer. Higher comes last.
    /// </summary>
    public int RenderOrder { get; set; } = 0;

    /// <summary>
    /// Action that contains render draw calls
    /// </summary>
    public Action<Camera2D> Render { get; set; }
}

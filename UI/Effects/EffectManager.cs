using System;
using System.Collections.Generic;

namespace Luxia.UI.Effects;

/// <summary>
/// Manages all active UI effects globally. 
/// Handles updating, adding, and removing effects.
/// </summary>
public class EffectManager
{
    private readonly List<Effect> _effects = new();

    /// <summary>
    /// Adds a new effect of type <typeparamref name="T"/> to the manager.
    /// </summary>
    /// <typeparam name="T">The effect type to add.</typeparam>
    /// <param name="setup">An optional action to configure the effect before it starts.</param>
    /// <returns>The created effect instance.</returns>
    public T Add<T>(Action<T>? setup = null) where T : Effect, new()
    {
        var effect = new T();
        setup?.Invoke(effect);
        _effects.Add(effect);
        return effect;
    }

    /// <summary>
    /// Updates all active effects.
    /// Automatically removes effects that are finished.
    /// </summary>
    internal void Update()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var effect = _effects[i];
            effect.Update();

            if (!effect.IsActive)
            {
                _effects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Applies all active effects.
    /// </summary>
    internal void Apply()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var effect = _effects[i];
            effect.InternalApply();
        }
    }

    /// <summary>
    /// Removes all active effects.
    /// </summary>
    public void Clear() => _effects.Clear();

    /// <summary>
    /// Gets the number of currently active effects.
    /// </summary>
    public int Count => _effects.Count;
}

using Microsoft.Xna.Framework;

namespace Luxia.UI.Effects;

/// <summary>
/// Represents a time-based UI effect that can be updated and applied
/// to animate visual elements (e.g., scaling, fading, movement).
/// </summary>
public abstract class Effect
{
    /// <summary>
    /// Gets a value indicating whether the effect is currently active.
    /// </summary>
    public bool IsActive { get; protected set; } = true;

    /// <summary>
    /// Gets or sets the total duration of the effect, in seconds.
    /// A value of <c>null</c> means the effect runs indefinitely.
    /// </summary>
    public float? Duration { get; protected set; }

    /// <summary>
    /// Gets the elapsed time since the effect started, in seconds.
    /// </summary>
    public float ElapsedTime { get; protected set; }

    /// <summary>
    /// Updates the effect’s internal state over time.
    /// </summary>
    public void Update()
    {
        if (!IsActive)
            return;

        ElapsedTime += Time.DeltaTime;

        if (Duration.HasValue && ElapsedTime >= Duration.Value)
        {
            ElapsedTime = Duration.Value;
            IsActive = false;
            OnFinished();
        }
    }

    /// <summary>
    /// Applies the effect logic (must be implemented by subclasses).
    /// </summary>
    protected abstract void Apply();

    internal void InternalApply() => Apply();

    /// <summary>
    /// Called once when the effect finishes naturally (optional override).
    /// </summary>
    protected virtual void OnFinished() { }

    /// <summary>
    /// Resets the effect to its initial state.
    /// </summary>
    public virtual void Reset()
    {
        ElapsedTime = 0f;
        IsActive = true;
    }
}

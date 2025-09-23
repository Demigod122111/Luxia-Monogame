using System;


namespace Luxia.Mathematics;

/// <summary>
/// Collection of common easing functions for smooth interpolation and animations.
/// All functions take a normalized time <c>t</c> in the range 0..1.
/// </summary>
internal static class Easings
{
    #region Linear
    public static float Linear(float t) => t;
    #endregion

    #region Quadratic
    public static float QuadIn(float t) => t * t;
    public static float QuadOut(float t) => t * (2f - t);
    public static float QuadInOut(float t) => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    #endregion

    #region Cubic
    public static float CubicIn(float t) => t * t * t;
    public static float CubicOut(float t) { t -= 1f; return t * t * t + 1f; }
    public static float CubicInOut(float t) => t < 0.5f ? 4f * t * t * t : (t - 1f) * 2f * (t - 1f) * (t - 1f) + 1f;
    #endregion

    #region Quartic
    public static float QuartIn(float t) => t * t * t * t;
    public static float QuartOut(float t) { t -= 1f; return 1f - t * t * t * t; }
    public static float QuartInOut(float t) => t < 0.5f ? 8f * t * t * t * t : 1f - 8f * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f);
    #endregion

    #region Quintic
    public static float QuintIn(float t) => t * t * t * t * t;
    public static float QuintOut(float t) { t -= 1f; return t * t * t * t * t + 1f; }
    public static float QuintInOut(float t) => t < 0.5f ? 16f * t * t * t * t * t : 1f + 16f * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f);
    #endregion

    #region Sine
    public static float SineIn(float t) => 1f - (float)Math.Cos((t * Math.PI) / 2f);
    public static float SineOut(float t) => (float)Math.Sin((t * Math.PI) / 2f);
    public static float SineInOut(float t) => -0.5f * ((float)Math.Cos(Math.PI * t) - 1f);
    #endregion

    #region Exponential
    public static float ExpoIn(float t) => t == 0f ? 0f : (float)Math.Pow(2f, 10f * (t - 1f));
    public static float ExpoOut(float t) => t == 1f ? 1f : 1f - (float)Math.Pow(2f, -10f * t);
    public static float ExpoInOut(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        if (t < 0.5f) return (float)Math.Pow(2f, 20f * t - 10f) / 2f;
        return (2f - (float)Math.Pow(2f, -20f * t + 10f)) / 2f;
    }
    #endregion

    #region Circular
    public static float CircIn(float t) => 1f - (float)Math.Sqrt(1f - t * t);
    public static float CircOut(float t) { t -= 1f; return (float)Math.Sqrt(1f - t * t); }
    public static float CircInOut(float t)
    {
        t *= 2f;
        if (t < 1f) return -0.5f * ((float)Math.Sqrt(1f - t * t) - 1f);
        t -= 2f;
        return 0.5f * ((float)Math.Sqrt(1f - t * t) + 1f);
    }
    #endregion

    #region Back
    public static float BackIn(float t)
    {
        const float s = 1.70158f;
        return t * t * ((s + 1f) * t - s);
    }
    public static float BackOut(float t)
    {
        const float s = 1.70158f;
        t -= 1f;
        return t * t * ((s + 1f) * t + s) + 1f;
    }
    public static float BackInOut(float t)
    {
        const float s = 1.70158f * 1.525f;
        t *= 2f;
        if (t < 1f) return 0.5f * (t * t * ((s + 1f) * t - s));
        t -= 2f;
        return 0.5f * (t * t * ((s + 1f) * t + s) + 2f);
    }
    #endregion

    #region Elastic
    public static float ElasticIn(float t)
    {
        if (t == 0f || t == 1f) return t;
        return -(float)Math.Pow(2f, 10f * (t - 1f)) * (float)Math.Sin((t - 1.1f) * 5f * Math.PI);
    }
    public static float ElasticOut(float t)
    {
        if (t == 0f || t == 1f) return t;
        return (float)Math.Pow(2f, -10f * t) * (float)Math.Sin((t - 0.1f) * 5f * Math.PI) + 1f;
    }
    public static float ElasticInOut(float t)
    {
        if (t == 0f || t == 1f) return t;
        t *= 2f;
        if (t < 1f) return -0.5f * (float)Math.Pow(2f, 10f * (t - 1f)) * (float)Math.Sin((t - 1.1f) * 5f * Math.PI);
        t -= 1f;
        return 0.5f * (float)Math.Pow(2f, -10f * t) * (float)Math.Sin((t - 0.1f) * 5f * Math.PI) + 1f;
    }
    #endregion

    #region Bounce
    public static float BounceOut(float t)
    {
        if (t < 1f / 2.75f) return 7.5625f * t * t;
        if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }
    public static float BounceIn(float t) => 1f - BounceOut(1f - t);
    public static float BounceInOut(float t) => t < 0.5f ? BounceIn(t * 2f) * 0.5f : BounceOut(t * 2f - 1f) * 0.5f + 0.5f;
    #endregion
}

using Luxia.Mathematics;
using Microsoft.Xna.Framework;
using System;

namespace Luxia.UI.Effects
{
    /// <summary>
    /// A pop-in scale effect that animates a UI element from a start scale,
    /// overshoots to a mid scale, and then settles at the final scale.
    /// </summary>
    public class PopInEffect : Effect
    {
        private Vector2 startScale;
        private Vector2 midScale;
        private Vector2 endScale;

        /// <summary>
        /// Gets the current scale value. Use this to apply to UI elements.
        /// </summary>
        public Vector2 CurrentScale { get; private set; }

        /// <summary>
        /// Creates a new PopInEffect.
        /// </summary>
        /// <param name="start">Initial scale at the beginning of the effect.</param>
        /// <param name="mid">Overshoot scale (usually larger than final).</param>
        /// <param name="end">Final scale at the end of the effect.</param>
        /// <param name="duration">Total duration of the effect in seconds.</param>
        public void Setup(Vector2 start, Vector2 mid, Vector2 end, float duration)
        {
            this.startScale = start;
            this.midScale = mid;
            this.endScale = end;
            this.Duration = duration;

            CurrentScale = start;
            Reset();
        }

        protected override void Apply()
        {
            if (!Duration.HasValue || Duration.Value <= 0f)
            {
                CurrentScale = endScale;
                return;
            }

            float t = MathHelper.Clamp(ElapsedTime / Duration.Value, 0f, 1f);

            // Split the animation: first half start->mid, second half mid->end
            if (t < 0.5f)
            {
                float nt = t * 2f; // normalize 0..1
                CurrentScale = Vector2.Lerp(startScale, midScale, Easings.Linear(nt));
            }
            else
            {
                float nt = (t - 0.5f) * 2f;
                CurrentScale = Vector2.Lerp(midScale, endScale, Easings.ElasticOut(nt));
            }
        }
    }
}

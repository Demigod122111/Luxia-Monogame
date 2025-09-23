using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Luxia
{
    public struct SpriteBatchState
    {
        public SpriteSortMode SortMode;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public DepthStencilState DepthStencilState;
        public RasterizerState RasterizerState;
        public Effect Effect;
        public Matrix? TransformMatrix;
    }

    public static class SpriteBatchStack
    {
        // Each SpriteBatch gets its own stack
        private static readonly ConditionalWeakTable<SpriteBatch, Stack<SpriteBatchState>> stacks
            = new();

        private static Stack<SpriteBatchState> GetStack(SpriteBatch sb)
        {
            return stacks.GetOrCreateValue(sb);
        }

        public static void PushBegin(this SpriteBatch sb,
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blend = null,
            SamplerState sampler = null,
            DepthStencilState depthStencil = null,
            RasterizerState rasterizer = null,
            Effect effect = null,
            Matrix? transform = null)
        {
            var stack = GetStack(sb);

            if (stack.Count > 0)
                sb.End(); // close previous state before starting new

            var state = new SpriteBatchState
            {
                SortMode = sortMode,
                BlendState = blend ?? BlendState.AlphaBlend,
                SamplerState = sampler ?? SamplerState.PointClamp,
                DepthStencilState = depthStencil,
                RasterizerState = rasterizer,
                Effect = effect,
                TransformMatrix = transform
            };

            stack.Push(state);
            sb.Begin(state.SortMode, state.BlendState, state.SamplerState,
                     state.DepthStencilState, state.RasterizerState, state.Effect, state.TransformMatrix);
        }

        public static void PopBegin(this SpriteBatch sb)
        {
            var stack = GetStack(sb);

            if (stack.Count == 0)
                throw new InvalidOperationException("SpriteBatch state stack underflow");

            stack.Pop();
            sb.End();

            if (stack.Count > 0)
            {
                var prev = stack.Peek();
                sb.Begin(prev.SortMode, prev.BlendState, prev.SamplerState,
                         prev.DepthStencilState, prev.RasterizerState, prev.Effect, prev.TransformMatrix);
            }
        }
    }
}

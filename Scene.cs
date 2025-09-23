using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Luxia;

public abstract class Scene
{
    public readonly UI.Effects.EffectManager EffectManager = new();
    public readonly UI.UIManager UIManager = new();
    public Camera2D? Camera2D { get; set; } = null;

    public virtual void Reset()
    {
        UIManager?.Clear();
    }
    public virtual void Load() { }
    public virtual void Unload() { }
    public virtual void Update() { }
    public virtual void Render() { }
    /// <summary>
    /// Called before UI Manager gets rendered
    /// </summary>
    public virtual void UIRender() { }
    /// <summary>
    /// Called after UI Manager gets rendered
    /// </summary>
    public virtual void RenderUI() { }

    protected void SetupCamera2D() => Camera2D = new(Application.GraphicsDevice.Viewport);

    public void BeginDrawing()
    {
        Application.BeginDrawing(
            SortMode,
            BlendState,
            SamplerState,
            DepthStencilState,
            RasterizerState,
            Effect,
            TransformMatrix
        );
    }

    public bool AutoDraw { get; set; } = true;
    public SpriteSortMode SortMode = SpriteSortMode.Deferred;
    public BlendState BlendState = null;
    public SamplerState SamplerState = SamplerState.PointClamp;
    public DepthStencilState DepthStencilState = null;
    public RasterizerState RasterizerState = null;
    public Effect Effect = null;
    public Matrix? TransformMatrix => Camera2D?.GetViewMatrix() ?? transformMatrix;
    Matrix? transformMatrix = null;
}

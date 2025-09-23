using System;
using System.Collections.Generic;
using Luxia.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Luxia;

public abstract class Application : Game
{
    private static Application s_instance;

    /// <summary>
    /// Gets a reference to the Application instance.
    /// </summary>
    public static Application Instance => s_instance;

    /// <summary>
    /// Gets the graphics device manager to control the presentation of graphics.
    /// </summary>
    public static GraphicsDeviceManager Graphics { get; private set; }

    /// <summary>
    /// Gets the graphics device used to create graphical resources and perform primitive rendering.
    /// </summary>
    public static new GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Gets the sprite batch used for all 2D rendering.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    /// Gets the content manager used to load global assets.
    /// </summary>
    public static new ContentManager Content { get; private set; }


    private static List<Scene> Scenes { get; set; }
    public static Scene? ActiveScene => ActiveSceneIndex >= 0 && ActiveSceneIndex < Scenes.Count ? Scenes[ActiveSceneIndex] : null;
    private static int ActiveSceneIndex { get; set; }
    private int virtualWidth;
    private int virtualHeight;

    private static RenderTarget2D renderTarget;

    /// <summary>
    /// Gets the width, in pixels, of the virtual rendering surface.
    /// </summary>
    private static int VirtualWidth => renderTarget.Width;

    /// <summary>
    /// Gets the height, in pixels, of the virtual rendering surface.
    /// </summary>
    private static int VirtualHeight => renderTarget.Height;

    /// <summary>
    /// Gets the width and height, in pixels, of the virtual rendering surface.
    /// </summary>
    public static Point VirtualSize => new(VirtualWidth, VirtualHeight);

    /// <summary>
    /// Gets the client bounds (destination rectangle) where the virtual render target 
    /// is drawn, with aspect ratio preserved.
    /// </summary>
    public static Rectangle ClientBounds
    {
        get
        {
            int backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            float targetAspect = (float)renderTarget.Width / renderTarget.Height;
            float windowAspect = (float)backBufferWidth / backBufferHeight;

            int drawWidth, drawHeight;
            if (windowAspect > targetAspect)
            {
                // Window is wider -> pillarbox
                drawHeight = backBufferHeight;
                drawWidth = (int)(drawHeight * targetAspect);
            }
            else
            {
                // Window is taller -> letterbox
                drawWidth = backBufferWidth;
                drawHeight = (int)(drawWidth / targetAspect);
            }

            return new Rectangle(
                (backBufferWidth - drawWidth) / 2,
                (backBufferHeight - drawHeight) / 2,
                drawWidth,
                drawHeight
            );
        }
    }

    


    /// <summary>
    /// Creates a new Application instance.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    /// <param name="virtualWidth">The width of the drawing surface.</param>
    /// <param name="virtualHeight">The height of the drawing surface.</param>
    public Application(string title, int width, int height, bool allowResize, bool fullScreen, int? virtualWidth=null, int? virtualHeight=null)
    {
        // Ensure that multiple cores are not created.
        if (s_instance != null)
        {
            throw new InvalidOperationException($"Only a single Application instance can be created");
        }

        // Store reference to engine for global member access.
        s_instance = this;

        // Create a new graphics device manager.
        Graphics = new GraphicsDeviceManager(this);

        // Set the graphics defaults.
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;

        // Apply the graphic presentation changes.
        Graphics.ApplyChanges();

        // Set the window title.
        Window.Title = title;

        // Set the window resize flag.
        Window.AllowUserResizing = allowResize;

        // Set the core's content manager to a reference of the base Game's
        // content manager.
        Content = base.Content;

        // Set the root directory for content.
        Content.RootDirectory = "Content";

        // Mouse is visible by default.
        IsMouseVisible = true;

        // Initalize the scene list.
        Scenes = new();
        ActiveSceneIndex = -1;

        this.virtualWidth = virtualWidth ?? width;
        this.virtualHeight = virtualHeight ?? height;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Set the core's graphics device to a reference of the base Game's
        // graphics device.
        GraphicsDevice = base.GraphicsDevice;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // Initialize the render texture.
        renderTarget = new RenderTarget2D(
            GraphicsDevice,
            virtualWidth,
            virtualHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );

        UIManager.WhiteTexture = new(Application.GraphicsDevice, 1, 1);
        UIManager.WhiteTexture.SetData(new[] { Color.White });

        SetupScenes();
    }

    protected override void Update(GameTime gameTime)
    {
        Time.LastUpdate = gameTime;

        Time.Update();
        Input.Update();

        ActiveScene?.Update();
        ActiveScene?.EffectManager.Update();
        ActiveScene?.UIManager.Update(ActiveScene.Camera2D);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Time.LastRender = gameTime;

        if (ActiveScene == null)
        {
            ClearBackground(Color.Black with
            {
                B = (byte)((Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 0.5 + 0.5) * 10)
            });
        }
        else
        {
            GraphicsDevice.SetRenderTarget(renderTarget);

            Span<bool> autoDraw = stackalloc bool[1] { ActiveScene.AutoDraw };
            if (autoDraw[0])
                ActiveScene.BeginDrawing();

            ActiveScene.Render();

            ActiveScene.EffectManager.Apply();

            if (autoDraw[0])
                EndDrawing();

            BeginDrawing(samplerState: SamplerState.PointClamp);
            ActiveScene.UIRender();
            ActiveScene.UIManager.Render(ActiveScene.Camera2D);
            ActiveScene.RenderUI();
            EndDrawing();

            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin(samplerState: SamplerState.LinearClamp);
            SpriteBatch.Draw(renderTarget, ClientBounds, Color.White);
            SpriteBatch.End();
        }

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();

        Content.Unload();
    }

    public static T Load<T>(string assetName) where T : class
    {
        return Content.Load<T>(assetName);
    }

    public static void BeginDrawing(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null) => SpriteBatch.PushBegin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
    public static void EndDrawing() => SpriteBatch.PopBegin();
    public static void ClearBackground(Color color) => GraphicsDevice.Clear(color);

    public abstract void SetupScenes();

    public static int AddScene<T>(bool makeActive = false) where T : Scene, new()
    {
        T scene = new T();
        Scenes.Add(scene);
        if (makeActive) LoadScene(Scenes.Count - 1);
        return Scenes.Count - 1;
    }

    public static void LoadScene(int index)
    {
        if (index < 0 || index >= Scenes.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Scene index is out of range.");

        ActiveScene?.Unload();
        ActiveSceneIndex = index;
        ActiveScene?.Reset();
        ActiveScene?.Load();
    }
}
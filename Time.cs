using Microsoft.Xna.Framework;

namespace Luxia;

public static class Time
{
    public static GameTime LastUpdate { get; internal set; }
    public static GameTime LastRender { get; internal set; }
    public static float DeltaTime => (float)LastUpdate.ElapsedGameTime.TotalSeconds;
    public static float RenderDeltaTime => (float)LastRender.ElapsedGameTime.TotalSeconds;
    public static float TotalTime => (float)LastUpdate.TotalGameTime.TotalSeconds;
    public static float TimeScale { get; set; } = 1f;
    public static float ScaledDeltaTime => DeltaTime * TimeScale;
    public static bool IsPaused => TimeScale == 0f;

    public static int FPS => currentFps ?? (int)(1f / DeltaTime);

    private static float fpsTimer = 0f;
    private static int fpsFrames = 0;
    private static int? currentFps = null;

    internal static void Update()
    {
        fpsTimer += DeltaTime;
        fpsFrames++;

        if (fpsTimer >= 1f) // every second
        {
            currentFps = fpsFrames;
            fpsFrames = 0;
            fpsTimer = 0f;
        }
    }
}

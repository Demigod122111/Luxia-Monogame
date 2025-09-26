using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Luxia.UI;

public class Button : UIElement
{
    public string Text = "";
    public Vector2 TextOffset = Vector2.Zero;
    public SpriteFont Font;
    public Texture2D? Texture;
    public Color TextColor = Color.White;
    public Color NormalColor = Color.White;
    public Color HoverColor = Color.LightGray;
    public Color PressedColor = Color.Gray;
    public float TextScale = 1f;
    public bool TextMatchBackground = false;

    public Action OnClick;
    public Action OnHoverStart;
    public Action OnHoverStay;
    public Action OnHoverEnd;

    public bool IsHovered { get; private set; } = false;
    private bool wasHovered = false;

    private bool isPressed = false;       // visual pressed state
    private bool pressStartedHere = false; // ensures click belongs to this button

    public Button()
    {
        Size = new(120, 30);
    }

    public override void Update(Camera2D camera)
    {
        var trueMouse = Input.MousePosition;
        Point mouse = IsWorldUI
            ? camera.ScreenToWorld(new(trueMouse.X, trueMouse.Y)).ToPoint()
            : trueMouse;

        bool hoverNow = EventPoint(mouse);

        if (!IsEnabled || !IsVisible)
        {
            IsHovered = false;
            wasHovered = false;
            pressStartedHere = false;
            return;
        }

        // Hover events
        if (hoverNow && !wasHovered)
            OnHoverStart?.Invoke();
        if (hoverNow)
            OnHoverStay?.Invoke();
        if (!hoverNow && wasHovered)
            OnHoverEnd?.Invoke();

        IsHovered = hoverNow;
        wasHovered = hoverNow;

        // --- Mouse press handling ---
        if (Input.GetMouseDown(MouseButton.Left))
        {
            if (IsHovered)
            {
                pressStartedHere = true;
                isPressed = true;
            }
        }
        else if (Input.IsMousePressed(MouseButton.Left)) // held
        {
            isPressed = pressStartedHere && IsHovered;
        }
        else if (Input.GetMouseUp(MouseButton.Left))
        {
            if (pressStartedHere && IsHovered)
                OnClick?.Invoke();

            pressStartedHere = false;
            isPressed = false;
        }
        else
        {
            isPressed = false;
        }
    }

    public override void Render(Camera2D camera)
    {
        if (!IsVisible) return;

        Color drawColor = NormalColor;
        if (isPressed) drawColor = PressedColor;
        else if (IsHovered) drawColor = HoverColor;

        if (!IsEnabled)
            drawColor = NormalColor;

        // Draw background
        Application.SpriteBatch.Draw(
            Texture ?? UIManager.WhiteTexture,
            new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y),
            drawColor
        );

        // Draw text centered
        if (Font != null && !string.IsNullOrEmpty(Text))
        {
            Vector2 textSize = Font.MeasureString(Text) * TextScale;
            Vector2 textPos = Position + TextOffset + (Size - textSize) / 2;
            Application.SpriteBatch.DrawString(
                Font,
                Text,
                textPos,
                TextMatchBackground ? drawColor : TextColor,
                0f,
                Vector2.Zero,
                TextScale,
                SpriteEffects.None,
                0f
            );
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TextCopy;

namespace Luxia.UI;

public class InputField : UIElement
{
    public string Text = "";
    public SpriteFont Font;
    public Texture2D? Texture;
    public Color TextColor = Color.Black;
    public Color BackgroundColor = Color.White;
    public Color SelectionColor = Color.CornflowerBlue;

    private bool isFocused = false;
    private int caretIndex = 0;
    private double caretTimer = 0;
    private bool caretVisible = true;

    private int scrollOffset = 0;

    // Selection
    private int selectionStart = -1;
    private int selectionEnd = -1;
    private bool isSelecting = false;
    private Point lastMousePosition;

    // Key repeat configuration
    private const double KeyRepeatDelay = 0.4; // seconds before repeat starts
    private const double KeyRepeatRate = 0.05; // seconds per repeat
    private KeyCode? repeatingKey = null;
    private double repeatingTimer = 0;

    public InputField()
    {
        Size = new(120, 30);
    }
    public override void Update(Camera2D camera)
    {
        base.Update(camera);
        if (!IsEnabled || !IsVisible) return;

        var trueMouse = Input.MousePosition;
        Point mouse = IsWorldUI ? camera.ScreenToWorld(new(trueMouse.X, trueMouse.Y)).ToPoint() : trueMouse;

        // Focus & click handling
        if (Input.GetMouseClicked(MouseButton.Left))
        {
            if (EventPoint(mouse))
            {
                isFocused = true;
                int clickIndex = GetCaretIndexFromMouse(mouse.X);

                if (Input.Shift)
                {
                    // Shift + click: extend selection
                    if (selectionStart == -1)
                        selectionStart = caretIndex; // anchor
                    caretIndex = clickIndex;
                    selectionEnd = caretIndex;
                    isSelecting = true;
                }
                else
                {
                    // Normal click: move caret, clear selection
                    caretIndex = clickIndex;
                    selectionStart = -1;
                    selectionEnd = -1;
                    isSelecting = true; // start potential drag
                }
            }
            else
            {
                isFocused = false;
                isSelecting = false;
                selectionStart = -1;
                selectionEnd = -1;
            }
        }

        // Mouse drag selection
        if (isSelecting && Input.IsMousePressed(MouseButton.Left))
        {
            int dragIndex = GetCaretIndexFromMouse(mouse.X);
            selectionEnd = dragIndex;
            caretIndex = dragIndex;
        }
        else if (!Input.IsMousePressed(MouseButton.Left))
        {
            isSelecting = false; // finished drag
        }

        if (isFocused)
        {
            // Shortcuts
            if (Input.Control)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    selectionStart = 0;
                    selectionEnd = Text.Length;
                    caretIndex = Text.Length;
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    if (HasSelection())
                        ClipboardService.SetText(GetSelectedText());
                    else
                        ClipboardService.SetText("");
                }
                else if (Input.GetKeyDown(KeyCode.V))
                {
                    string paste = ClipboardService.GetText() ?? "";
                    DeleteSelection();
                    Text = Text.Insert(caretIndex, paste);
                    caretIndex += paste.Length;
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    if (HasSelection())
                        ClipboardService.SetText(GetSelectedText());
                    else
                        ClipboardService.SetText("");

                    DeleteSelection();
                }
            }


            // Text input with Handling key repeat
            var keysDown = Input.GetKeysDown(); // keys pressed this frame
            if (keysDown.Length > 0)
            {
                repeatingKey = keysDown[0];
                repeatingTimer = 0;
            }
            else if (repeatingKey.HasValue && Input.GetKey(repeatingKey.Value))
            {
                repeatingTimer += Time.DeltaTime;
                if (repeatingTimer >= KeyRepeatDelay)
                {
                    repeatingTimer -= KeyRepeatRate;
                    keysDown = new KeyCode[] { repeatingKey.Value }; // simulate repeated key press
                }
                else keysDown = Array.Empty<KeyCode>();
            }
            else repeatingKey = null;

            void HandleKeyPress(KeyCode key)
            {
                // Arrow keys and Home/End
                if (key == KeyCode.Left)
                {
                    if (Input.Shift)
                    {
                        if (selectionStart == -1) selectionStart = caretIndex;
                        caretIndex = Math.Max(0, caretIndex - 1);
                        selectionEnd = caretIndex;
                    }
                    else
                    {
                        caretIndex = Math.Max(0, caretIndex - 1);
                        selectionStart = selectionEnd = caretIndex;
                    }
                    return;
                }
                if (key == KeyCode.Right)
                {
                    if (Input.Shift)
                    {
                        if (selectionStart == -1) selectionStart = caretIndex;
                        caretIndex = Math.Min(Text.Length, caretIndex + 1);
                        selectionEnd = caretIndex;
                    }
                    else
                    {
                        caretIndex = Math.Min(Text.Length, caretIndex + 1);
                        selectionStart = selectionEnd = caretIndex;
                    }
                    return;
                }

                if (key == KeyCode.Home)
                {
                    caretIndex = 0;
                    if (Input.Shift) selectionEnd = caretIndex;
                    else selectionStart = selectionEnd = caretIndex;
                    return;
                }

                if (key == KeyCode.End)
                {
                    caretIndex = Text.Length;
                    if (Input.Shift) selectionEnd = caretIndex;
                    else selectionStart = selectionEnd = caretIndex;
                    return;
                }

                // Backspace/Delete
                if (key == KeyCode.Backspace)
                {
                    if (HasSelection())
                        DeleteSelection();
                    else if (caretIndex > 0)
                    {
                        Text = Text.Remove(caretIndex - 1, 1);
                        caretIndex--;
                    }
                    selectionStart = selectionEnd = caretIndex;
                    return;
                }
                if (key == KeyCode.Delete)
                {
                    if (HasSelection())
                        DeleteSelection();
                    else if (caretIndex < Text.Length)
                        Text = Text.Remove(caretIndex, 1);
                    selectionStart = selectionEnd = caretIndex;
                    return;
                }

                if (Input.Control && (key == KeyCode.A || key == KeyCode.C || key == KeyCode.V || key == KeyCode.X)) return;

                char c = KeyToChar(key);
                if (c != '\0')
                {
                    DeleteSelection();
                    Text = Text.Insert(caretIndex, c.ToString());
                    caretIndex++;
                    selectionStart = selectionEnd = caretIndex;
                }
            }

            // Process keysDown array normally
            foreach (var key in keysDown)
            {
                HandleKeyPress(key);
            }
        }

        // Caret blink
        caretTimer += Time.DeltaTime;
        if (caretTimer >= 0.5f)
        {
            caretTimer = 0;
            caretVisible = !caretVisible;
        }

        // Scroll
        if (Font != null)
        {
            float caretX = Font.MeasureString(Text[..caretIndex]).X;
            if (caretX - scrollOffset > Size.X - 10) scrollOffset = (int)(caretX - Size.X + 10);
            if (caretX - scrollOffset < 0) scrollOffset = (int)caretX;
        }

        lastMousePosition = mouse;
    }

    public override void Render(Camera2D camera)
    {
        if (!IsVisible) return;

        var rect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        Application.SpriteBatch.Draw(Texture ?? UIManager.WhiteTexture, rect, BackgroundColor);

        if (Font != null)
        {
            var textPos = Position + new Vector2(5, 5);

            var oldRect = Application.GraphicsDevice.ScissorRectangle;

            Application.SpriteBatch.PushBegin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                null, new RasterizerState() { ScissorTestEnable = true });
            Application.GraphicsDevice.ScissorRectangle = rect;

            // Selection highlight
            if (HasSelection())
            {
                int start = Math.Min(selectionStart, selectionEnd);
                int end = Math.Max(selectionStart, selectionEnd);
                float selX = Font.MeasureString(Text[..start]).X;
                float selWidth = Font.MeasureString(Text[start..end]).X;
                Application.SpriteBatch.Draw(Texture ?? UIManager.WhiteTexture,
                    new Rectangle((int)(textPos.X + selX - scrollOffset), (int)textPos.Y, (int)selWidth, (int)Font.LineSpacing),
                    SelectionColor);
            }

            // Draw text
            Application.SpriteBatch.DrawString(Font, Text, textPos - new Vector2(scrollOffset, 0), TextColor);

            // Caret
            if (isFocused && caretVisible)
            {
                string beforeCaret = Text[..caretIndex];
                float caretX = Font.MeasureString(beforeCaret).X;
                var caretPos = new Vector2(textPos.X + caretX - scrollOffset, textPos.Y);
                Application.SpriteBatch.Draw(Texture ?? UIManager.WhiteTexture,
                    new Rectangle((int)caretPos.X, (int)caretPos.Y, 1, (int)Font.LineSpacing), TextColor);
            }

            Application.SpriteBatch.PopBegin();
            Application.GraphicsDevice.ScissorRectangle = oldRect;
        }
    }

    private bool HasSelection() => selectionStart != selectionEnd && selectionStart >= 0 && selectionEnd >= 0;
    private string GetSelectedText()
    {
        if (!HasSelection()) return "";
        int start = Math.Min(selectionStart, selectionEnd);
        int end = Math.Max(selectionStart, selectionEnd);
        return Text[start..end];
    }
    private void DeleteSelection()
    {
        if (!HasSelection()) return;
        int start = Math.Min(selectionStart, selectionEnd);
        int end = Math.Max(selectionStart, selectionEnd);
        Text = Text.Remove(start, end - start);
        caretIndex = start;
        selectionStart = selectionEnd = caretIndex;
    }

    private int GetCaretIndexFromMouse(int mouseX)
    {
        if (Font == null) return 0;
        float x = mouseX - Position.X + scrollOffset - 5;
        int index = 0;
        for (int i = 0; i <= Text.Length; i++)
        {
            float width = Font.MeasureString(Text[..i]).X;
            if (width > x)
            {
                index = i - 1;
                break;
            }
            index = i;
        }
        return Math.Clamp(index, 0, Text.Length);
    }

    private char KeyToChar(KeyCode key)
    {
        bool shift = Input.Shift;

        if (key >= KeyCode.A && key <= KeyCode.Z)
            return (char)((shift ? 'A' : 'a') + (key - KeyCode.A));

        if (key >= KeyCode.D0 && key <= KeyCode.D9)
        {
            if (shift)
            {
                return key switch
                {
                    KeyCode.D1 => '!',
                    KeyCode.D2 => '@',
                    KeyCode.D3 => '#',
                    KeyCode.D4 => '$',
                    KeyCode.D5 => '%',
                    KeyCode.D6 => '^',
                    KeyCode.D7 => '&',
                    KeyCode.D8 => '*',
                    KeyCode.D9 => '(',
                    KeyCode.D0 => ')',
                    _ => (char)('0' + (key - KeyCode.D0))
                };
            }
            else return (char)('0' + (key - KeyCode.D0));
        }

        if (key == KeyCode.Space) return ' ';
        if (key == KeyCode.OemComma) return shift ? '<' : ',';
        if (key == KeyCode.OemPeriod) return shift ? '>' : '.';
        if (key == KeyCode.OemMinus) return shift ? '_' : '-';
        if (key == KeyCode.OemPlus) return shift ? '+' : '=';
        if (key == KeyCode.OemSemicolon) return shift ? ':' : ';';
        if (key == KeyCode.OemQuotes) return shift ? '"' : '\'';
        if (key == KeyCode.OemPipe) return shift ? '|' : '\\';
        if (key == KeyCode.OemOpenBrackets) return shift ? '{' : '[';
        if (key == KeyCode.OemCloseBrackets) return shift ? '}' : ']';
        if (key == KeyCode.OemQuestion) return shift ? '?' : '/';
        if (key == KeyCode.OemTilde) return shift ? '~' : '`';

        return '\0';
    }
}

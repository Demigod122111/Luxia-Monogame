using Microsoft.Xna.Framework;

namespace Luxia.UI.Layout;

public enum GridOverflowAxis
{
    Row,   // fill across columns first, overflow downward (default)
    Column // fill down rows first, overflow rightward
}

public class GridLayout : Layout
{
    /// <summary> Number of columns. </summary>
    public int Columns { get; set; } = 2;

    /// <summary> Number of rows. </summary>
    public int Rows { get; set; } = 0; // 0 = auto based on children

    /// <summary> Fixed size of each cell. If (0,0), auto-calculate. </summary>
    public Vector2 CellSize { get; set; } = Vector2.Zero;

    /// <summary> How children overflow when count exceeds capacity. </summary>
    public GridOverflowAxis OverflowAxis { get; set; } = GridOverflowAxis.Row;

    protected override void LayoutChildren()
    {
        if (Children.Count == 0) return;

        int cols = Columns <= 0 ? 1 : Columns;
        var invCols = 1f / (float)cols;
        int rows = Rows > 0 ? Rows : (int)System.Math.Ceiling(Children.Count * invCols);
        var invRows = 1f / (float)rows;

        float cellWidth, cellHeight;

        if (CellSize == Vector2.Zero) // auto sizing
        {
            cellWidth = (Size.X - Padding.X * 2 - Spacing.X * (cols - 1)) * invCols;
            cellHeight = (Size.Y - Padding.Y * 2 - Spacing.Y * (rows - 1)) * invRows;
        }
        else
        {
            cellWidth = CellSize.X;
            cellHeight = CellSize.Y;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            int row, col;

            if (OverflowAxis == GridOverflowAxis.Row)
            {
                row = (int)(i * invCols);
                col = i % cols;
            }
            else // overflow by column
            {
                col = (int)(i * invRows);
                row = i % rows;
            }

            var child = Children[i];
            child.Size = new(cellWidth, cellHeight);
            child.LocalPosition = new(
                Padding.X + col * (cellWidth + Spacing.X),
                Padding.Y + row * (cellHeight + Spacing.Y)
            );
        }
    }
}


namespace Luxia.UI.Layout;

public class GridLayout : Layout
{
    public int Columns = 2;

    protected override void LayoutChildren()
    {
        if (Columns <= 0) Columns = 1;

        float cellWidth = (Size.X - Padding.X * 2 - Spacing * (Columns - 1)) / Columns;
        float cellHeight = (Size.Y - Padding.Y * 2) / ((Children.Count + Columns - 1) / Columns);

        for (int i = 0; i < Children.Count; i++)
        {
            int row = i / Columns;
            int col = i % Columns;

            var child = Children[i];
            child.Size = new(cellWidth, cellHeight);
            child.LocalPosition = new(Padding.X + col * (cellWidth + Spacing),
                                         Padding.Y + row * (cellHeight + Spacing));
        }
    }
}

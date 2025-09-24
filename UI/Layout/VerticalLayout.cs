
namespace Luxia.UI.Layout;

public class VerticalLayout : Layout
{
    protected override void LayoutChildren()
    {
        float currentY = Padding.Y;

        foreach (var child in Children)
        {
            child.LocalPosition = new(Padding.X, currentY);
            currentY += child.Size.Y + Spacing.Y;
        }
    }
}

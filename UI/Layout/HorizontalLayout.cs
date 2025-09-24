
namespace Luxia.UI.Layout;

public class HorizontalLayout : Layout
{
    protected override void LayoutChildren()
    {
        float currentX = Padding.X;

        foreach (var child in Children)
        {
            child.LocalPosition = new(currentX, Padding.Y);
            currentX += child.Size.X + Spacing.X;
        }
    }
}

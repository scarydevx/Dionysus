namespace Dionysus.App.Renders;

public class ContextMenuStripRender : ToolStripProfessionalRenderer
{
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using (SolidBrush _brush = new SolidBrush(ColorTranslator.FromHtml("#191919"))) 
        {
            e.Graphics.FillRectangle(_brush, e.AffectedBounds);
        }
    }
    
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        Rectangle _rectanglerect = new Rectangle(Point.Empty, e.Item.Size);
        Color _fillColor = e.Item.Selected ? ColorTranslator.FromHtml("#555") : ColorTranslator.FromHtml("#191919");
        using (SolidBrush brush = new SolidBrush(_fillColor))
        {
            e.Graphics.FillRectangle(brush, _rectanglerect);
        }
    }
    
    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = Color.White;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        
    }
}
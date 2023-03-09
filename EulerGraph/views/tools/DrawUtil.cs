using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EulerGraph.tools;

public static class DrawUtil
{
    public static (Ellipse,List<TextBlock>) DrawNode(Point pos, SolidColorBrush color, ref Canvas canvas, int radius = 9, int index = -1)
    {
        var node = new Ellipse();
        node.Fill = color;
        node.Margin = new Thickness(pos.X - radius, pos.Y - radius, 0, 0);
        node.Width = radius * 2;
        node.Height = node.Width;
        canvas.Children.Add(node);

        var tb = DrawText($"{index}", new Point(pos.X - radius / 2, pos.Y - radius + 2), Brushes.White, ref canvas);

        Point coords = new(pos.X / 10, pos.Y / 10);
        var coord = DrawText($"({coords.X.ToString("F0")}|{coords.Y.ToString("F0")})",
            new Point(pos.X - radius / 2, pos.Y - 3 * radius),
            Brushes.Black,
            ref canvas);
        tb.IsHitTestVisible = false;
        // add mouse over and mouse out events!
        return (node,new (){tb,coord});
    }

    public static Line DrawLine(Point from, Point to, ref Canvas canvas)
    {
        return DrawLine(from, to, Brushes.Black, ref canvas);
    }

    public static Line DrawLine(Point from, Point to, SolidColorBrush color, ref Canvas canvas)
    {
        var line = new Line();
        line.X1 = from.X;
        line.Y1 = from.Y;
        line.X2 = to.X;
        line.Y2 = to.Y;
        line.Stroke = color;
        line.StrokeThickness = 1;

        canvas.Children.Add(line);
        return line;
    }

    public static (Line line, TextBlock lbl) DrawWeightedLine(Point from, Point to, SolidColorBrush color, int weight,
        ref Canvas canvas)
    {
        var line = DrawLine(from, to, color, ref canvas);
        var lbl = DrawText($"{weight}", new Point((to.X + from.X) / 2, (to.Y + from.Y) / 2), color, ref canvas);
        return (line, lbl);
    }

    public static TextBlock DrawText(string text, Point pos, SolidColorBrush color, ref Canvas canvas,
        int fontSize = 10)
    {
        var lbl = new TextBlock();
        lbl.Text = $"{text}";
        lbl.Foreground = color;
        lbl.FontSize = fontSize;
        lbl.Margin = new Thickness(pos.X, pos.Y, 0, 0);
        canvas.Children.Add(lbl);
        return lbl;
    }
}
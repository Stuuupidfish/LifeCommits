using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LifeCommits.Models;
using Grid = LifeCommits.Models.Grid;

namespace LifeCommits.Views
{
    public class GridRenderer : Control
    {
        //give the renderer a property specifically for the Grid it needs to draw
        public static readonly StyledProperty<Grid> GridToDrawProperty =
            AvaloniaProperty.Register<GridRenderer, Grid>(nameof(GridToDraw));

        public Grid GridToDraw
        {
            get => GetValue(GridToDrawProperty);
            set => SetValue(GridToDrawProperty, value);
        }

        static GridRenderer()
        {
            //whenever Grid changes, tell control to redraw
            GridToDrawProperty.Changed.AddClassHandler<GridRenderer>((x, e) => x.InvalidateVisual());
        }

        private readonly List<int>[] colors = new List<int>[7];

        public GridRenderer()
        {
            //ROY G BIV
            colors[0] = new List<int> { 0xFFBAB8, 0xFF8E8A, 0xFF352E, 0xA30500 };
            colors[1] = new List<int> { 0xFFE4B8, 0xFFC05C, 0xFF9D00, 0xD18100 };
            colors[2] = new List<int> { 0xFFF7B8, 0xFFF18A, 0xFFE72E, 0xD1B900 };
            colors[3] = new List<int> { 0xD6F5C2, 0x9AE667, 0x6DDB24, 0x468C17 };
            colors[4] = new List<int> { 0xC2E2F5, 0x9AD0EF, 0x4BABE2, 0x17618C };
            colors[5] = new List<int> { 0xE0C2F5, 0xB167E6, 0x8F24DB, 0x5B178C };
            colors[6] = new List<int> { 0xF5C2F5, 0xE667E6, 0xDB24DB, 0x8C178C };
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // If we don't have a Grid to draw yet, nothing to draw
            if (GridToDraw == null) return;

            double size = 12;    // Size of the square
            double spacing = 3;  // Padding between squares

            // 7 rows (days), 53 columns (weeks)
            for (int c = 0; c < 53; c++)
            {
                for (int r = 0; r < 7; r++)
                {
                    double xOffset = c * (size + spacing);
                    double yOffset = r * (size + spacing);

                    var rect = new Rect(xOffset, yOffset, size, size);
                    var square = GridToDraw.Squares[r, c];

                    IBrush color;

                    if (square == null)
                    {
                        color = Brushes.Transparent;
                    }
                    else if (square.Commits == null || square.Commits == 0)
                    {
                        color = new SolidColorBrush(Color.Parse("#ebedf0"));
                    }
                    else if (square.Commits < 3)
                    {
                        color = new SolidColorBrush(Color.Parse("#9be9a8"));
                    }
                    else
                    {
                        color = new SolidColorBrush(Color.Parse("#216e39"));
                    }

                    context.DrawRectangle(color, null, rect);
                }
            }
        }
    }
}

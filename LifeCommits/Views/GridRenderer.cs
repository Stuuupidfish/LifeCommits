using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
            get { return (Grid)GetValue(GridToDrawProperty); }
            set { SetValue(GridToDrawProperty, value); }
        }

        static GridRenderer()
        {
            //whenever Grid changes, tell control to redraw
            GridToDrawProperty.Changed.AddClassHandler<GridRenderer>(OnGridToDrawChanged);
        }

        private static void OnGridToDrawChanged(GridRenderer sender, AvaloniaPropertyChangedEventArgs e)
        {
            sender.InvalidateVisual();
        }

        private readonly List<int>[] colors = new List<int>[7];

        // hover state
        private int hoveredRow = -1;
        private int hoveredCol = -1;
        private bool isHovering = false;
        private Point lastPointerPos = new Point();

        // square drawing metrics exposed so pointer logic and Render use same values
        private double squareSize = 12.0;    // Size of the square
        private double spacing = 3.0;  // Padding between squares

        // (hover state and drawing metrics declared above)

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

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            Point p = e.GetPosition(this);
            lastPointerPos = p;

            int col = (int)(p.X / (squareSize + spacing));
            int row = (int)(p.Y / (squareSize + spacing));

            if (GridToDraw == null)
                return;

            if (col >= 0 && col < 53 && row >= 0 && row < 7)
            {
                double xOffset = col * (squareSize + spacing);
                double yOffset = row * (squareSize + spacing);
                if (p.X >= xOffset && p.X <= xOffset + squareSize && p.Y >= yOffset && p.Y <= yOffset + squareSize)
                {
                    hoveredCol = col;
                    hoveredRow = row;
                    isHovering = true;

                    Square square = GridToDraw.Squares[row][col];
                    if (square != null && square.Date != null)
                    {
                        DateOnly date = square.Date.Value;
                        int commits = square.Commits ?? 0;
                        string tip = DateOnlyToString(date) + ": " + commits.ToString() + (commits == 1 ? " commit" : " commits");
                        ToolTip.SetTip(this, tip);
                    }
                    else
                    {
                        ToolTip.SetTip(this, null);
                    }

                    InvalidateVisual();
                    return;
                }
            }

            // if here, not over square
            if (isHovering)
            {
                isHovering = false;
                hoveredCol = -1;
                hoveredRow = -1;
                ToolTip.SetTip(this, null);
                InvalidateVisual();
            }
        }

        // Note: we do not override OnPointerLeave because Control does not provide that override
        // Hover state is cleared when pointer moves outside squares in OnPointerMoved.

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (GridToDraw == null) return;

            // Determine color palette index
            int paletteIdx = GridToDraw.ColorKey;
            if (paletteIdx < 0 || paletteIdx >= colors.Length)
                paletteIdx = 3; // fallback to green
            List<int> palette = colors[paletteIdx];

            for (int c = 0; c < 53; c++)
            {
                for (int r = 0; r < 7; r++)
                {
                    double xOffset = c * (squareSize + spacing);
                    double yOffset = r * (squareSize + spacing);

                    Rect rect = new Rect(xOffset, yOffset, squareSize, squareSize);
                    Square square = GridToDraw.Squares[r][c];

                    IBrush color;

                    if (square == null)
                    {
                        color = Brushes.Transparent;
                    }
                    else if (square.Commits == null || square.Commits == 0)
                    {
                        color = new SolidColorBrush(Color.Parse("#ebedf0"));
                    }
                    else
                    {
                        // Use palette: 1 commit = lightest, 2 = next, 3+ = darkest
                        int idx = 0;
                        if (square.Commits >= 3) idx = 3;
                        else if (square.Commits == 2) idx = 2;
                        else if (square.Commits == 1) idx = 1;
                        int rgb = palette[Math.Min(idx, palette.Count - 1)];
                        color = new SolidColorBrush(Color.FromArgb(0xFF,
                            (byte)((rgb >> 16) & 0xFF),
                            (byte)((rgb >> 8) & 0xFF),
                            (byte)(rgb & 0xFF)));
                    }

                    context.DrawRectangle(color, null, rect);
                }
            }

            // draw hover border
            if (isHovering && hoveredRow >= 0 && hoveredCol >= 0)
            {
                double xOffset = hoveredCol * (squareSize + spacing);
                double yOffset = hoveredRow * (squareSize + spacing);
                Rect rect = new Rect(xOffset - 1, yOffset - 1, squareSize + 2, squareSize + 2);
                Pen pen = new Pen(Brushes.White, 2);
                context.DrawRectangle(null, pen, rect);
            }
        }

        // small helper to format a DateOnly consistently
        private string DateOnlyToString(DateOnly d)
        {
            return d.Year.ToString("D4") + "-" + d.Month.ToString("D2") + "-" + d.Day.ToString("D2");
        }

        // messages shown after clicking a square
        // Event to notify parent when a square is clicked (messages, position and optional date key)
        public class SquareClickedEventArgs : EventArgs
        {
            public IReadOnlyList<string> Messages { get; }
            public Point Position { get; }
            // A simple string key representing the date (e.g. "YYYY-MM-DD") for toggling
            public string? DateKey { get; }

            public SquareClickedEventArgs(IReadOnlyList<string> messages, Point position, string? dateKey = null)
            {
                Messages = messages;
                Position = position;
                DateKey = dateKey;
            }
        }

        public event EventHandler<SquareClickedEventArgs> SquareClicked;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Point p = e.GetPosition(this);

            int col = (int)(p.X / (squareSize + spacing));
            int row = (int)(p.Y / (squareSize + spacing));

            if (GridToDraw == null)
            {
                return;
            }

            if (col >= 0 && col < 53 && row >= 0 && row < 7)
            {
                double xOffset = col * (squareSize + spacing);
                double yOffset = row * (squareSize + spacing);
                if (p.X >= xOffset && p.X <= xOffset + squareSize && p.Y >= yOffset && p.Y <= yOffset + squareSize)
                {
                    Square square = GridToDraw.Squares[row][col];
                    if (square != null && square.CommitMessageList != null && square.CommitMessageList.Count > 0)
                    {
                        IReadOnlyList<string> msgs = square.CommitMessageList.AsReadOnly();
                        // include a simple date key so the host can toggle the popup for the same day
                        string? dateKey = null;
                        if (square.Date != null)
                        {
                            DateOnly d = square.Date.Value;
                            dateKey = d.Year.ToString("D4") + "-" + d.Month.ToString("D2") + "-" + d.Day.ToString("D2");
                        }
                        SquareClickedEventArgs args = new SquareClickedEventArgs(msgs, p, dateKey);
                        SquareClicked?.Invoke(this, args);
                        return;
                    }
                }
            }

            // clicked outside a square or no messages: notify host to hide any panel
            // clicked outside a square or no messages: notify host to hide any panel
            SquareClickedEventArgs hideArgs = new SquareClickedEventArgs(new List<string>().AsReadOnly(), p, null);
            SquareClicked?.Invoke(this, hideArgs);
        }
    }
}

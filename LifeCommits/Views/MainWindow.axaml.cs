using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Media;
using LifeCommits.ViewModels;
using LifeCommits.Models; //YEAH I KNOW THIS BREAKS MVVM STRUCTURE BUT I CANT TAKE IT ANYMORE
using System;
using System.Linq;

namespace LifeCommits.Views
{
    public partial class MainWindow : Window
    {
        // remember the last date key for the currently shown messages popup (for toggle)
        private string? lastMessagesDateKey = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            // subscribe to square click events from the renderer to show commit notes
            GridRenderer gridRenderer = this.FindControl<GridRenderer>("GridRenderer");
            if (gridRenderer != null)
            {
                gridRenderer.SquareClicked += GridRenderer_SquareClicked;
            }
            // ensure window is placed at bottom of z-order when opened (Windows only)
            this.Opened += MainWindow_Opened;
        }

        private void GridRenderer_SquareClicked(object? sender, GridRenderer.SquareClickedEventArgs e)
        {
            Border popup = this.FindControl<Border>("MessagesPopup");
            StackPanel panel = this.FindControl<StackPanel>("MessagesPanel");
            if (popup == null || panel == null)
            {
                return;
            }

            // clear existing
            panel.Children.Clear();

            // if clicked outside or no messages, hide popup and clear last key
            if (e.Messages == null || e.Messages.Count == 0)
            {
                popup.IsVisible = false;
                lastMessagesDateKey = null;
                return;
            }

            // if popup already visible and the same date was clicked, toggle it closed
            if (popup.IsVisible && e.DateKey != null && lastMessagesDateKey != null && e.DateKey == lastMessagesDateKey)
            {
                popup.IsVisible = false;
                lastMessagesDateKey = null;
                return;
            }

            foreach (string msg in e.Messages)
            {
                TextBlock tb = new TextBlock();
                tb.Text = "• " + msg;
                tb.Foreground = Brushes.White;
                tb.FontFamily = new FontFamily("Monospace");
                tb.Margin = new Thickness(0, 0, 0, 4);
                panel.Children.Add(tb);
            }

            // determine mouse position relative to the window (translate from renderer coords)
            Point winPoint = new Point(e.Position.X, e.Position.Y);
            if (sender is Control srcControl)
            {
                // translate the click point into the Canvas coordinate space (the Window.Content is the Canvas)
                Canvas? canvas = this.Content as Canvas;
                Point? translated;
                if (canvas != null)
                {
                    translated = srcControl.TranslatePoint(e.Position, canvas);
                }
                else
                {
                    translated = srcControl.TranslatePoint(e.Position, this);
                }

                if (translated.HasValue)
                {
                    winPoint = translated.Value;
                }
            }

            // measure popup content so sizing (auto) is taken into account
            // Measure the inner panel first to get reliable content size
            double availableContentWidth = double.PositiveInfinity;
            if (!double.IsInfinity(popup.MaxWidth) && popup.MaxWidth > 0)
            {
                availableContentWidth = Math.Max(0.0, popup.MaxWidth - (popup.Padding.Left + popup.Padding.Right));
            }
            // ask the panel to measure itself with the available width and max height
            panel.Measure(new Size(availableContentWidth, popup.MaxHeight));
            double contentWidth = panel.DesiredSize.Width;
            double contentHeight = panel.DesiredSize.Height;

            double popupWidth = contentWidth + (popup.Padding.Left + popup.Padding.Right);
            double popupHeight = contentHeight + (popup.Padding.Top + popup.Padding.Bottom);

            // enforce min/max constraints defined on the popup
            if (popupWidth < popup.MinWidth)
            {
                popupWidth = popup.MinWidth;
            }
            if (!double.IsInfinity(popup.MaxWidth) && popupWidth > popup.MaxWidth)
            {
                popupWidth = popup.MaxWidth;
            }
            if (!double.IsInfinity(popup.MaxHeight) && popupHeight > popup.MaxHeight)
            {
                popupHeight = popup.MaxHeight;
            }

            // position so the bottom-left corner of the popup is at the click point
            double left = winPoint.X;
            double top = winPoint.Y - popupHeight;

            // clamp horizontally within window
            double maxX = this.Bounds.Width - popupWidth - 4.0;
            if (left > maxX)
            {
                left = Math.Max(0.0, maxX);
            }
            if (left < 0.0)
            {
                left = 0.0;
            }

            // if popup would go above the window, place it below the click point instead
            if (top < 0.0)
            {
                top = winPoint.Y + 4.0;
            }

            // ensure popup doesn't run off the bottom
            double maxY = this.Bounds.Height - popupHeight - 4.0;
            if (top > maxY)
            {
                top = Math.Max(0.0, maxY);
            }

            Canvas.SetLeft(popup, left);
            Canvas.SetTop(popup, top);
            // bring the popup to front by reordering it as the last child of the Canvas
            if (popup.Parent is Canvas parentCanvas)
            {
                parentCanvas.Children.Remove(popup);
                parentCanvas.Children.Add(popup);
            }
            popup.IsVisible = true;

            // remember which date's messages are currently shown so clicking it again will close
            lastMessagesDateKey = e.DateKey;
        }

        private void GoalPrev_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectPreviousGoal();
            }
        }

        private void GoalNext_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectNextGoal();
            }
        }

        private void MainWindow_Opened(object? sender, EventArgs e)
        {
            TrySetWindowToBottom();
        }

        private void ShowNewGoalPanel_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                // toggle visibility so repeated clicks show/hide the panel
                viewModel.IsNewGoalPanelVisible = !viewModel.IsNewGoalPanelVisible;
            }
        }

        private void ShowContributePanel_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                // toggle visibility so repeated clicks show/hide the panel
                viewModel.IsContributePanelVisible = !viewModel.IsContributePanelVisible;
            }
        }

        private void YearPrev_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                int[] years = viewModel.Years.ToArray();
                if (years == null || years.Length == 0)
                {
                    return;
                }

                int best = int.MinValue;
                bool found = false;
                for (int i = 0; i < years.Length; i++)
                {
                    int y = years[i];
                    if (y < viewModel.SelectedYear)
                    {
                        if (!found || y > best)
                        {
                            best = y;
                            found = true;
                        }
                    }
                }

                if (found)
                {
                    viewModel.SelectedYear = best;
                }
            }
        }

        private void YearNext_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                int[] years = viewModel.Years.ToArray();
                if (years == null || years.Length == 0)
                {
                    return;
                }

                int best = int.MaxValue;
                bool found = false;
                for (int i = 0; i < years.Length; i++)
                {
                    int y = years[i];
                    if (y > viewModel.SelectedYear)
                    {
                        if (!found || y < best)
                        {
                            best = y;
                            found = true;
                        }
                    }
                }

                if (found)
                {
                    viewModel.SelectedYear = best;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            // only start dragging when the actual Border/window received the pointer (not a child control)
            if (e.Source == sender)
            {
                BeginMoveDrag(e);
            }
        }
        


        private int selectedColorIndex = 3; // default to green
        private void ColorBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // Tag set in XAML as "0".."6"
                if (btn.Tag is string s && int.TryParse(s, out int idx))
                {
                    selectedColorIndex = idx;
                }
                else if (btn.Tag is int i)
                {
                    selectedColorIndex = i;
                }

                // Visual feedback: highlight selected button border and clear others
                for (int i = 0; i <= 6; i++)
                {
                    string name = $"ColorBtn{i}";
                    Button b = this.FindControl<Button>(name);
                    if (b == null) continue;

                    if (i == selectedColorIndex)
                    {
                        b.BorderBrush = Brushes.White;
                        b.BorderThickness = new Thickness(2);
                        b.Opacity = 1.0;
                    }
                    else
                    {
                        b.BorderBrush = Brushes.Transparent;
                        b.BorderThickness = new Thickness(0);
                        b.Opacity = 0.8;
                    }
                }
            }
        }

        private void ConfirmNewContribution_Click(object sender, RoutedEventArgs e)
        {
            TextBox tb = this.FindControl<TextBox>("ContributeTextBox");
            string notes = tb?.Text?.Trim();

            if (DataContext is MainWindowViewModel viewModel)
            {
                //no notes contributions should be ok-- notes should be optional
                viewModel.MakeNewContribution(notes);
                viewModel.IsContributePanelVisible = false;
            }

            //force grid redraw
            Control gridRenderer = this.FindControl<Control>("GridRenderer");
            gridRenderer?.InvalidateVisual();

            if (tb != null)
            {
                tb.Text = string.Empty;
            }
        }
        private void ConfirmNewGoal_Click(object? sender, RoutedEventArgs e)
        {
            TextBox tb = this.FindControl<TextBox>("NewGoalTextBox");
            string name = tb?.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MakeNewGoal(name, selectedColorIndex);
                viewModel.IsNewGoalPanelVisible = false;
            }

            if (tb != null)
            {
                tb.Text = string.Empty;
            }
        }

        private void DeleteGoal_Click(object? sender, RoutedEventArgs e)
        {
            Goal? target = null;

            // Common patterns: the Button's DataContext can be the Goal, or a Tag/CommandParameter
            if (sender is Button btn)
            {
                if (btn.DataContext is Goal g)
                {
                    target = g;
                }
            }
            if (DataContext is MainWindowViewModel viewModel)
            {
                // if the button wasn't bound to a Goal, use the currently selected goal from VM
                if (target == null)
                {
                    target = viewModel.SelectedGoal;
                }

                if (target == null)
                {
                    return;
                }

                viewModel.DeleteGoal(target);
            }
        }

        // put main window at bottom (Windows only)
        private void TrySetWindowToBottom()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                return;
            }

            IWindowImpl impl = this.PlatformImpl;
            IPlatformHandle platformHandle = impl?.TryGetFeature(typeof(IPlatformHandle)) as IPlatformHandle;
            IntPtr handle = platformHandle?.Handle ?? IntPtr.Zero;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    }
}
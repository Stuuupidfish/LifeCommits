using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Media;
using LifeCommits.ViewModels;
using System;

namespace LifeCommits.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
            // ensure window is placed at bottom of z-order when opened (Windows only)
            this.Opened += MainWindow_Opened;
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

        private void YearPrev_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectedYear = viewModel.SelectedYear - 1;
            }
        }

        private void YearNext_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectedYear = viewModel.SelectedYear + 1;
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
                if (btn.Tag is string s && int.TryParse(s, out var idx))
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
                    var name = $"ColorBtn{i}";
                    var b = this.FindControl<Button>(name);
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

        private void NewGoalButton_Click(object sender, RoutedEventArgs e)
        {
            //GoalPopup.IsOpen = true;
        }
        private void ConfirmNewGoal_Click(object? sender, RoutedEventArgs e)
        {
            var tb = this.FindControl<TextBox>("NewGoalTextBox");
            var name = tb?.Text?.Trim();
            if (string.IsNullOrEmpty(name))
                return;

            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MakeNewGoal(name, selectedColorIndex);
                viewModel.IsNewGoalPanelVisible = false;
            }

            if (tb != null)
                tb.Text = string.Empty;
        }

        // put main window at bottom (Windows only)
        private void TrySetWindowToBottom()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return;

            var impl = this.PlatformImpl;
            var platformHandle = impl?.TryGetFeature(typeof(IPlatformHandle)) as IPlatformHandle;
            var handle = platformHandle?.Handle ?? IntPtr.Zero;
            if (handle == IntPtr.Zero) return;

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
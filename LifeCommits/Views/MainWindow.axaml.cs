using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using LifeCommits.ViewModels;
using System;

namespace LifeCommits.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            //i dont think this is right....
            //FIX LATER
            Hide();
        }
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }
        


        private int selectedColorIndex = 3; // default to green
        private void ColorBtn_Click(object? sender, RoutedEventArgs e)
        {
            
        }

        private void NewGoalButton_Click(object sender, RoutedEventArgs e)
        {
            //GoalPopup.IsOpen = true;
        }
        private void ConfirmNewGoal_Click(object? sender, RoutedEventArgs e)
        {
            
        }
    }
}
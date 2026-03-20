using System;
using System.Collections.Generic;
using Avalonia.Threading;
using LifeCommits.Models;

namespace LifeCommits.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        //central "state" of the window
        //instantiates Manager once when the app starts
        public Manager AppManager { get; }

        private string currentTitle = "Overview";
        public string CurrentTitle
        {
            get => currentTitle;
            set
            {
                if (currentTitle != value)
                {
                    currentTitle = value;
                    //notify View that the title changed so it redraws text
                    OnPropertyChanged(nameof(CurrentTitle));
                }
            }
        }
        
        private DateOnly currentDate;
        private DispatcherTimer dayCheckerTimer;
        public MainWindowViewModel()
        {
            AppManager = new Manager();
            currentDate = DateOnly.FromDateTime(DateTime.Now);

            //set up a timer that checks the time in the bkg
            dayCheckerTimer = new DispatcherTimer();
            dayCheckerTimer.Interval = TimeSpan.FromMinutes(1); //wake up & check every minute
            dayCheckerTimer.Tick += CheckIfNewDay;
            dayCheckerTimer.Start();
        }

        private void CheckIfNewDay(object? sender, EventArgs e)
        {
            var actualToday = DateOnly.FromDateTime(DateTime.Now);
            
            //did midnight pass?
            if (actualToday > currentDate)
            {
                if (actualToday.Year > currentDate.Year)
                {
                    //new year
                    AppManager.ResetOverviewGridForYear(actualToday.Year);
                }

                currentDate = actualToday;
                
                //update overview grid's today square
                AppManager.OverviewGrid.InitializeTodaysSquare(currentDate);
                //update all goal's today square
                foreach (var goal in AppManager.Goals)
                {
                    var thisYearGrid = goal.GetYearGrid(currentDate.Year.ToString());
                    thisYearGrid.InitializeTodaysSquare(currentDate);
                }
        
                OnPropertyChanged(nameof(AppManager));
            }
        }

        private string[] names = { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet" };
        public void MakeNewGoal(string goalName, int colorInd) // You can add a color parameter here later!
        {
            string color = names[colorInd];
            var newGoal = new Goal(goalName, color);
            AppManager.Goals.Add(newGoal);
        }
    }
}

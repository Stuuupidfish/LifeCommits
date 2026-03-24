using Avalonia.Threading;
using LifeCommits.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LifeCommits.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        //central "state" of the window
        //instantiates Manager once when the app starts
        public Manager AppManager { get; }


        private Grid selectedGrid;
        public Grid SelectedGrid
        {
            get
            {
                return selectedGrid;
            }
            private set
            {
                if (selectedGrid != value)
                {
                    selectedGrid = value;
                    OnPropertyChanged(nameof(SelectedGrid));
                }
            }
        }
        public IReadOnlyList<int> Years
        {
            get
            {
                if (AppManager.Goals.Count > 0)
                {
                    int current = DateTime.Now.Year;
                    return new List<int> { current - 2, current - 1, current }; //first 3 years-- CHANGE LATER
                }
                else
                {
                    int now = DateTime.Now.Year;
                    return new List<int> { now };
                }
            }
        }
        private int selectedYear = DateTime.Now.Year;
        public int SelectedYear
        {
            get { return selectedYear; }
            set
            {
                if (selectedYear != value)
                {
                    selectedYear = value;
                    OnPropertyChanged(nameof(SelectedYear));
                    UpdateSelectedGridForYear(selectedYear);
                }
            }
        }

        #region current title
        private string currentTitle = "Overview";
        public string CurrentTitle
        {
            get { return currentTitle; }
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
        #endregion

        #region goal panel
        private bool isNewGoalPanelVisible = false;
        public bool IsNewGoalPanelVisible
        {
            get { return isNewGoalPanelVisible; }
            set
            {
                isNewGoalPanelVisible = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region time
        private DateOnly currentDate;
        private DispatcherTimer dayCheckerTimer;
        #endregion

        public MainWindowViewModel()
        {
            AppManager = new Manager();
            // default selected grid is the overview for current year
            SelectedGrid = AppManager.OverviewGrid;
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
                // overview grid reference may have changed; notify view to redraw
                OnPropertyChanged(nameof(SelectedGrid));
            }
        }

        private string[] names = { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet" };
        public void MakeNewGoal(string goalName, int colorInd) // You can add a color parameter here later!
        {
            string color = names[colorInd];
            var newGoal = new Goal(goalName, color);
            AppManager.Goals.Add(newGoal);
        }

        private void UpdateSelectedGridForYear(int year)
        {
            if (AppManager.Goals.Count == 0)
            {
                SelectedGrid = null;
                return;
            }

            // Example: pick the first goal for now
            var goal = AppManager.Goals[0];

            // Goal exposes GetYearGrid(string)
            SelectedGrid = goal.GetYearGrid(year.ToString());
        }
    }
}

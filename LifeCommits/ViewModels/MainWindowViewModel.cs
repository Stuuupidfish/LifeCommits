using Avalonia.Threading;
using LifeCommits.Models;
using LifeCommits.Services;
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
        public Manager AppManager { get; private set; }


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

        // index of the currently selected goal in AppManager.Goals
        // -1 indicates the overview is selected
        private int currentGoalIndex = -1;

        // currently selected Goal (null when overview is selected)
        public Goal? SelectedGoal
        {
            get
            {
                if (currentGoalIndex == -1)
                {
                    return null;
                }

                if (currentGoalIndex < 0 || currentGoalIndex >= AppManager.Goals.Count)
                {
                    return null;
                }

                return AppManager.Goals[currentGoalIndex];
            }
        }

        public void SelectGoalByIndex(int index)
        {
            if (index < -1)
            {
                return;
            }

            if (index >= AppManager.Goals.Count)
            {
                return;
            }

            currentGoalIndex = index;

            // notify that selected goal reference changed
            OnPropertyChanged(nameof(SelectedGoal));

            // Update contribute button visibility: only visible if not on overview
            IsContributeButtonVisible = (currentGoalIndex != -1);

            if (currentGoalIndex == -1)
            {
                CurrentTitle = "Overview";
                SelectedGrid = AppManager.OverviewGrid;
                return;
            }

            Goal goal = AppManager.Goals[currentGoalIndex];
            CurrentTitle = goal.Name;
            SelectedGrid = goal.GetYearGrid(SelectedYear.ToString());
        }

        public void SelectNextGoal()
        {
            if (AppManager.Goals.Count == 0)
            {
                return;
            }

            if (currentGoalIndex == -1)
            {
                // move from overview to first goal
                SelectGoalByIndex(0);
                return;
            }

            if (currentGoalIndex < AppManager.Goals.Count - 1)
            {
                SelectGoalByIndex(currentGoalIndex + 1);
                return;
            }

            // Update contribute button visibility in case nothing changed
            IsContributeButtonVisible = (currentGoalIndex != -1);
        }

        public void SelectPreviousGoal()
        {
            if (AppManager.Goals.Count == 0)
            {
                return;
            }

            if (currentGoalIndex == -1)
            {
                // already at overview
                IsContributeButtonVisible = false;
                return;
            }

            if (currentGoalIndex > 0)
            {
                SelectGoalByIndex(currentGoalIndex - 1);
                return;
            }

            if (currentGoalIndex == 0)
            {
                // move back to overview
                SelectGoalByIndex(-1);
                return;
            }

            // Update contribute button visibility in case nothing changed
            IsContributeButtonVisible = (currentGoalIndex != -1);
        }
        public IReadOnlyList<int> Years
        {
            get
            {
                if (AppManager.Goals.Count > 0)
                {
                    HashSet<int> yearsSet = new HashSet<int>();
                    for (int i = 0; i < AppManager.Goals.Count; i++)
                    {
                        Goal g = AppManager.Goals[i];
                        IReadOnlyList<string> yl = g.YearsList;
                        for (int j = 0; j < yl.Count; j++)
                        {
                            string ys = yl[j];
                            int yi;
                            if (int.TryParse(ys, out yi))
                            {
                                yearsSet.Add(yi);
                            }
                        }
                    }

                    if (yearsSet.Count == 0)
                    {
                        return new List<int> { DateTime.Now.Year };
                    }

                    List<int> list = new List<int>(yearsSet);
                    list.Sort();
                    return list;
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

        private bool isContributeButtonVisible = false;
        public bool IsContributeButtonVisible
        {
            get { return isContributeButtonVisible; }
            set
            {                
                isContributeButtonVisible = value;
                OnPropertyChanged();
            }
        }

        private bool isContributePanelVisible = false;
        public bool IsContributePanelVisible
        {
            get { return isContributePanelVisible; }
            set
            {
                isContributePanelVisible = value;
                OnPropertyChanged();
            }
        }

        #region time
        private DateOnly currentDate;
        private DispatcherTimer dayCheckerTimer;
        #endregion

        public MainWindowViewModel()
        {
            // try to load saved state; fall back to a new Manager
            Manager? loaded = PersistenceService.LoadManager();
            if (loaded != null)
            {
                AppManager = loaded;
            }
            else
            {
                AppManager = new Manager();
            }
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
            DateOnly actualToday = DateOnly.FromDateTime(DateTime.Now);
            
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
                foreach (Goal goal in AppManager.Goals)
                {
                    Grid thisYearGrid = goal.GetYearGrid(currentDate.Year.ToString());
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
            // Ask the manager to create and register the new goal. Manager will
            // insert it as the primary goal so the UI shows it by default.
            AppManager.AddGoal(goalName, color);

            // The manager added the new goal at the end of the list — select it.
            SelectGoalByIndex(AppManager.Goals.Count - 1);

            // Ensure selected year is current and notify that Years may have changed.
            SelectedYear = DateTime.Now.Year;
            OnPropertyChanged(nameof(Years));
            // persist change
            PersistenceService.SaveManager(AppManager);
        }
        public void DeleteGoal(Goal target)
        {
            if (target == null)
            {
                return;
            }

            AppManager.DeleteGoal(target);

            SelectGoalByIndex(-1); //go back to overview after deletion

            OnPropertyChanged(nameof(Years));
            // persist change
            PersistenceService.SaveManager(AppManager);
        }

        public void MakeNewContribution(string contributionNotes)
        {
            // Only allow contribution when a goal (not overview) is selected.
            if (currentGoalIndex < 0 || currentGoalIndex >= AppManager.Goals.Count)
            {
                return;
            }

            Goal goal = AppManager.Goals[currentGoalIndex];
            goal.Contribute(contributionNotes);

            // Update overview grid for today so the overview reflects contributions across goals.
            AppManager.OverviewGrid.UpdateToday(DateOnly.FromDateTime(DateTime.Now));

            // Refresh the selected grid (either the goal's grid or the overview)
            if (currentGoalIndex == -1)
            {
                SelectedGrid = AppManager.OverviewGrid;
            }
            else
            {
                SelectedGrid = goal.GetYearGrid(SelectedYear.ToString());
            }

            // Notify UI that manager/overview may have changed
            OnPropertyChanged(nameof(AppManager));
            OnPropertyChanged(nameof(Years));
            // persist change
            PersistenceService.SaveManager(AppManager);
        }

        private void UpdateSelectedGridForYear(int year)
        {
            // If no goals exist, show overview
            if (AppManager.Goals.Count == 0)
            {
                SelectedGrid = AppManager.OverviewGrid;
                return;
            }

            // If overview is selected, update overview grid for year
            if (currentGoalIndex == -1)
            {
                AppManager.ResetOverviewGridForYear(year);
                SelectedGrid = AppManager.OverviewGrid;
                return;
            }

            // Otherwise show the selected goal's grid for the year
            Goal goal = AppManager.Goals[currentGoalIndex];
            SelectedGrid = goal.GetYearGrid(year.ToString());
        }
    }
}

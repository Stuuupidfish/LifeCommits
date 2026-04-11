using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace LifeCommits.Models
{
    public class Goal
    {
        //goals hold grids. one grid per year.
        //grids are 7 rows (sun-sat) and 53 columns (weeks in a year-- rounded accounting for leap year)
        private String color;
        public String Color
        {
            get { return color; }
            set { color = value; }
        }
        private String name;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        private int commits;
        public int Commits
        {
            get { return commits; }
            set { commits = value; }
        }
        private List<Grid> years;
        private List<String> yearsList; //years as strings for UI purposes
        private int currentYearInd; //index of the current year in the years list
        // expose for serialization
        public List<Grid> Years
        {
            get { return years; }
            set { years = value ?? new List<Grid>(); }
        }

        public List<string> YearsList
        {
            get { return yearsList; }
            set { yearsList = value ?? new List<string>(); }
        }

        public int CurrentYearIndex
        {
            get { return currentYearInd; }
            set { currentYearInd = value; }
        }
        private int maxStreak;
        private int currentStreak;
        private int colorIndex = 3; // default to Green
        // parameterless ctor needed for deserialization
        public Goal()
        {
            // initialize defaults in case deserializer sets only some members
            maxStreak = 0;
            currentStreak = 0;
            commits = 0;
            currentYearInd = 0;
            years = new List<Grid>();
            yearsList = new List<string>();
        }

        public Goal(String name, String color)
        {
            this.name = name;
            this.color = color;

            maxStreak = 0;
            currentStreak = 0;
            commits = 0;
            currentYearInd = 0;
            years = new List<Grid>();
            yearsList = new List<String>();

            yearsList.Add(DateTime.Now.Year.ToString());
            int idx = ColorNameToIndex(color);
            if (idx < 0)
            {
                idx = 3;
            }
            colorIndex = idx;
            Grid grid = new Grid();
            grid.ColorKey = colorIndex;
            years.Add(grid);
        }

        private int ColorNameToIndex(string color)
        {
            //maps color names to palette index (MainWindowViewModel)
            switch (color)
            {
                case "Red": return 0;
                case "Orange": return 1;
                case "Yellow": return 2;
                case "Green": return 3;
                case "Blue": return 4;
                case "Indigo": return 5;
                case "Violet": return 6;
                default: return -1;
            }
        }
        public void Contribute(String desc)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Grid thisYear = years[currentYearInd];
            thisYear.AddCommit(desc, today);
            commits++;
        }

        public Grid GetYearGrid(String year)
        {
            int yearInd = yearsList.IndexOf(year);
            return years[yearInd];
        }

        //call this at the start of year
        public void AddYear()
        {
            int newYear = DateTime.Now.Year;
            yearsList.Add(newYear.ToString());
            Grid g = new Grid();
            g.ColorKey = colorIndex;
            years.Add(g);
            currentYearInd++;
        }

        //call this at 11:59pm every day
        public bool CommittedToday()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Grid thisYear = years[currentYearInd];
            if (thisYear.TodaysSquare(today).Commits > 0)
            {
                currentStreak++;
                if (currentStreak > maxStreak)
                {
                    maxStreak = currentStreak;
                }
                return true;
            }
            else
            {
                currentStreak = 0;
                return false;
            }
        }
    }
}

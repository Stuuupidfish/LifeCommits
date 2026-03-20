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
        }
        private String name;
        public String Name 
        { 
            get { return name; } 
        }
        private int commits;
        public int Commits 
        { 
            get { return commits; } 
        }
        private List<Grid> years;
        private List<String> yearsList; //years as strings for UI purposes
        private int currentYearInd; //index of the current year in the years list
        private int maxStreak;
        private int currentStreak;
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
            years.Add(new Grid());
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
            years.Add(new Grid());
            currentYearInd++;
        }

        public void SwitchYearView(int dir)
        {

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

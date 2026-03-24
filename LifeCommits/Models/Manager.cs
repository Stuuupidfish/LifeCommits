using System;
using System.Collections.Generic;
using System.Text;

namespace LifeCommits.Models
{
    public sealed class Manager
    {
        private List<Goal> goals;
        public List<Goal> Goals 
        { 
            get { return goals; } 
        }
        private OverviewGrid overviewGrid;
        public OverviewGrid OverviewGrid 
        { 
            get { return overviewGrid; } 
        }
        
        public Manager()
        {
            goals = new List<Goal>();
            overviewGrid = new OverviewGrid(goals, DateTime.Now.Year);
        }

        public void AddGoal(String name, String color)
        {
            Goal newGoal = new Goal(name, color);
            goals.Add(newGoal);
        }
        public void DeleteGoal(Goal goal)
        {
            goals.Remove(goal);
        }

        public void ResetOverviewGridForYear(int year)
        {
            overviewGrid = new OverviewGrid(goals, year);
        }

    }
}
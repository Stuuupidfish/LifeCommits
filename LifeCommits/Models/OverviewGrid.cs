using System.Collections.Generic;
using System;

namespace LifeCommits.Models
{
    public sealed class OverviewGrid : Grid
    {
        private List<Goal> goals;

        //now accepts a year so overview can be built for a specific year
        public OverviewGrid(List<Goal> goals, int year) : base()
        {
            this.goals = goals;
            foreach (Goal goal in goals)
            {
                Grid thisYear = goal.GetYearGrid(year.ToString());
                for (int r = 0; r < 7; r++)
                {
                    for (int c = 0; c < 53; c++)
                    {
                        Square square = thisYear.Squares[r, c];
                        if (square != null)
                        {
                            squares[r, c].Commits += square.Commits;
                        }
                    }
                }
            }
        }

        //call after every commit to any goal
        public void UpdateToday(DateOnly today)
        {
            int tempCommits = 0;
            Square overviewToday = TodaysSquare(today);
            if (overviewToday == null)
            {
                InitializeTodaysSquare(today);
                overviewToday = TodaysSquare(today);
            }

            foreach (Goal goal in goals)
            {
                Grid thisYear = goal.GetYearGrid(today.Year.ToString());
                Square square = thisYear.TodaysSquare(today);
                if (square != null)
                {
                    tempCommits += (int) square.Commits;
                }
            }
            overviewToday.Commits = tempCommits;
        }
    }
}
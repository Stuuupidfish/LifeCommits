using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace LifeCommits
{
    internal class Goal
    {
        //goals hold grids. one grid per year.
        //grids are 7 rows (sun-sat) and 53 columns (weeks in a year-- rounded accounting for leap year)
        private int commits;
        public void addCommit(String desc)
        {
            DateOnly date = DateOnly.FromDateTime(DateTime.Now);

        }
    }
}

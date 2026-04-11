using System;
using System.Collections.Generic;
using System.Text;

namespace LifeCommits.Models
{
    public class Square
    {
        public DateOnly? Date { get; set; }
        public List<String> CommitMessageList { get; set; }
        public int? Commits { get; set; }

        public Square(DateOnly? date, int? commits)
        {
            Date = date;
            Commits = commits;
            if (date == null || commits == null)
            {
                CommitMessageList = null;
            }
            else
            {
                CommitMessageList = new List<String>();
            }
        }
    }

}

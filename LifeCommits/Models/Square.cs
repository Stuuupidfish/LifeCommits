using System;
using System.Collections.Generic;
using System.Text;

namespace LifeCommits.Models
{
    public class Square
    {
        private DateOnly? date;
        public DateOnly? Date
        {
            get { return date; }
            set { date = value; }
        }
        private List<String> commitMessageList;
        public List<String> CommitMessageList
        {
            get { return commitMessageList; }
            set { commitMessageList = value; }
        }
        private int? commits;
        public int? Commits
        {
            get { return commits; }
            set { commits = value; }
        }
        public Square(DateOnly? date, int? commits)
        {
            this.date = date;
            this.commits = commits;
            if (date == null || commits == null)
            {
                commitMessageList = null;
            }
            else
            {
                commitMessageList = new List<String>();
            }
        }
    }

}

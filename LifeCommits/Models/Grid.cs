using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LifeCommits.Models
{
    public class Grid
    {
        protected Square[][] squares;
        public Square[][] Squares
        {
            get { return squares; }
            set { squares = value; }
        }
        // ColorKey is an int index for color palette (0-6), default -1 for fallback
        private int colorKey = -1;
        public int ColorKey
        {
            get { return colorKey; }
            set { colorKey = value; }
        }
        private DateOnly jan1;
        private int dayOne;
        public DateOnly Jan1
        {
            get { return jan1; }
            set { jan1 = value; }
        }
        public int DayOne
        {
            get { return dayOne; }
            set { dayOne = value; }
        }
        public Grid()
        {
            // initialize jagged array (7 rows, 53 columns)
            squares = new Square[7][];
            for (int r = 0; r < 7; r++)
            {
                squares[r] = new Square[53];
            }

            int daysPassed = DateTime.Now.DayOfYear;

            //find the day of the week of jan 1st of the year, and fill in the grid starting from there.
            jan1 = new DateOnly(DateTime.Now.Year, 1, 1);
            dayOne = (int)jan1.DayOfWeek;

            int daysFilled = 0;
            //fill in first col starting from day 1
            for (int r = dayOne; r < 7; r++)
            {
                if (daysPassed == daysFilled)
                {
                    return;
                }
                squares[r][0] = new Square(jan1.AddDays(daysFilled), 0);
                daysFilled++;
            }

            //fill in the rest of the grid from col 2 until today
            for (int c = 1; c < 53; c++)
            {
                for (int r = 0; r < 7; r++)
                {
                    if (daysPassed == daysFilled)
                    {
                        return;
                    }
                    squares[r][c] = new Square(jan1.AddDays(daysFilled), 0);
                    daysFilled++;
                }
            }
        }
        public void AddCommit(String desc, DateOnly today)
        {
            int dayOfWeek = (int)today.DayOfWeek;
            int weekOfYear = (today.DayOfYear - 1 + dayOne) / 7;

            Square square = squares[dayOfWeek][weekOfYear];

            //this shouldnt happen as initialize todyas square should always be called before this, but just in case
            if (square == null)
            {
                InitializeTodaysSquare(today);
                square = squares[dayOfWeek][weekOfYear];
            }

            if (square.CommitMessageList == null)
            {
                square.CommitMessageList = new List<string>();
            }
            square.CommitMessageList.Add(desc);
            square.Commits++;
        }
        
        //call at 12am
        public void InitializeTodaysSquare(DateOnly today)
        {
            int dayOfWeek = (int)today.DayOfWeek;
            int weekOfYear = (today.DayOfYear - 1 + dayOne) / 7;
            squares[dayOfWeek][weekOfYear] = new Square(today, 0);
        }

        public Square TodaysSquare(DateOnly today)
        {
            int dayOfWeek = (int)today.DayOfWeek;
            int weekOfYear = (today.DayOfYear - 1 + dayOne) / 7;
            return squares[dayOfWeek][weekOfYear];
        }
    }
}

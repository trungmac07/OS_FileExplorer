using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    internal class DateTime1
    {
        public int Day { get; }
        public int Month { get; }
        public int Year { get; }
        public int Hour { get; }
        public int Minute { get; }
        public int Second { get; }

        public DateTime1(int day, int month, int year, int hour, int minute, int second)
        {
            Day = day;
            Month = month;
            Year = year;
            Hour = hour;
            Minute = minute;
            Second = second;
        }

        public DateTime1(long microsecond)
        {
            int[] dayInMonths = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            long value = microsecond / 1000000; //to second
            Second = (int)value % 60;
            value /= 60; // to minute;
            Minute = (int)value % 60;
            value /= 60; // to hour
            Hour = (int)value % 24;
            value /= 24; // to day

            


        }

        public static bool checkLeapYear(int year)
        {
            if ((year % 4 == 0 & year % 100 != 0) || (year % 400 == 0))
                return true;
            return false;
        }

    }
}

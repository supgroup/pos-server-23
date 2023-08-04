using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS_Server.Classes
{
    public class Calculate
    {
        public decimal percentValue(decimal? value, decimal? percent)
        {
            if (value == null)
            {
                value = 0;
            }
            if (percent == null)
            {
                percent = 0;
            }
            decimal? perval=  (value * percent / 100);
            return (decimal) perval;
        }
        public DateTime? changeDateformat(DateTime? date, string format)
        {//@"d/M/yyyy"
            string sdate = "";
            if (date != null)
            {
                DateTime ts = DateTime.Parse(date.ToString());
                // @"hh\:mm\:ss"
                sdate = ts.ToString(format);
            }

            return DateTime.Parse(sdate);
        }

       public int getdays(DateTime date)
        {
            int year;
            int month;
            int days;

            year = date.Year;
            month = date.Month;

            days = getdays(year, month);



          //  int days = DateTime.DaysInMonth(year, month);

            return days;
        }
       public int getdays(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);

            return days;
        }
        public decimal totalWithPercent(decimal? value, decimal? percent)
        {
            decimal? per = percentValue( value,   percent);
            decimal? total = value + per;
            return (decimal)total;
        }
        public static string DateTodbString(DateTime? date)
        {
            string sdate = "";
            if (date != null)
            {

                //"yyyy'-'MM'-'dd'T'HH':'mm':'ss"
                sdate = date.Value.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");


            }

            return sdate;
        }
        public bool IsDateTime(string text)
        {
            DateTime dateTime;
            bool isDateTime = false;

            // Check for empty string.
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            isDateTime = DateTime.TryParse(text, out dateTime);

            return isDateTime;
        }

        public  DateTime StartOfWeek( DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public DateTime StartOfMonth (DateTime dt)
        {
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            return startDate;
        }
        public DateTime EndOfMonth (DateTime dt)
        {
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return endDate;
        }
        public DateTime StartOfYear(int year)
        {
            return new DateTime(year, 1, 1);
        }
        public DateTime EndOfYear(int year)
        {
            return new DateTime(year, 12, 31);
        }
    }
}
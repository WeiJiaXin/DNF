using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using UnityEngine;

namespace Foundation
{
    public class TimeUtil
    {
        public static long CalculateDays(DateTime end, DateTime start)
        {
            if (start.Year == end.Year)
            {
                return end.DayOfYear - start.DayOfYear;
            }

            var middle = new DateTime(end.Year, start.Month, start.Day);
            var timeSpan = middle - start;
            var diff = (end.DayOfYear - middle.DayOfYear) + timeSpan.TotalDays;
            return (long) diff;
        }

        public static int GetCurrentUnixTimeStamp()
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime() -
                          new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return (int) ts.TotalSeconds;
        }

        public static double GetCurrentUnixTimeStampMil()
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime() -
                          new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return ts.TotalMilliseconds;
        }

        public static string GetCurrentUnixTimeStampStr()
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime() -
                          new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime UnixTimeStampToDateTimeUTC(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }


        public static int ConverToUnixTimestamp(DateTime date)
        {
            TimeSpan ts = date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (int) ts.TotalSeconds;
        }

        public static int GetCurrentWeekOfYear()
        {
            return GetCurrentWeekOfYear(GetCurrentUnixTimeStamp());
        }

        public static int GetCurrentWeekOfYear(long time)
        {
            // Gets the Calendar instance associated with a CultureInfo.
            CultureInfo myCI = new CultureInfo("en-US");
            Calendar myCal = myCI.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;

            DateTime dtDateTime = UnixTimeStampToDateTime(time);

            return myCal.GetWeekOfYear(dtDateTime, myCWR, myFirstDOW);
        }

        public static bool IsSameWeek(double unixTimeStamp1, double unixTimeStamp2)
        {
            DateTime dateTime1 = UnixTimeStampToDateTimeUTC(unixTimeStamp1);
            DateTime dateTime2 = UnixTimeStampToDateTimeUTC(unixTimeStamp2);

            dateTime1 = dateTime1.AddSeconds(-8 * 60 * 60);
            dateTime2 = dateTime2.AddSeconds(-8 * 60 * 60);

            var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            var d1 = dateTime1.Date.AddDays(-1 * (int) cal.GetDayOfWeek(dateTime1));
            var d2 = dateTime2.Date.AddDays(-1 * (int) cal.GetDayOfWeek(dateTime2));

            return d1 == d2;
        }

        public static bool IsSameDay(double unixTimeStamp1, double unixTimeStamp2)
        {
            DateTime dateTime1 = UnixTimeStampToDateTime(unixTimeStamp1);
            DateTime dateTime2 = UnixTimeStampToDateTime(unixTimeStamp2);
            return dateTime1.Year == dateTime2.Year &&
                   dateTime1.DayOfYear == dateTime2.DayOfYear;
        }

        public static bool IsLater(double unixTimeStamp1, double unixTimeStamp2)
        {
            DateTime dateTime1 = UnixTimeStampToDateTime(unixTimeStamp1);
            DateTime dateTime2 = UnixTimeStampToDateTime(unixTimeStamp2);
            return (dateTime1.Year > dateTime2.Year) ||
                   (dateTime1.Year == dateTime2.Year && dateTime1.DayOfYear > dateTime2.DayOfYear);
        }

        public static string TimeFormatInMinSec(int timeLeft)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60F);
            int seconds = Mathf.FloorToInt(timeLeft - minutes * 60);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public static string TimeFormatInHrsMinSec(int seconds)
        {
            int hrs = seconds / 60 / 60;
            int min = seconds / 60 % 60;
            int sec = seconds % 60;
            return string.Format("{0:00}:{1:00}:{2:00}", hrs, min, sec);
        }

        public static string TimeFormatInHrsMin(int seconds)
        {
            int hrs = seconds / 60 / 60;
            int min = seconds / 60 % 60;
            return string.Format("{0:00}:{1:00}", hrs, min);
        }

        public static int SecondsToTomorrow()
        {
            var tomorrow = DateTime.Today + new TimeSpan(1, 0, 0, 0);
            var timeSpan = tomorrow - DateTime.Now;
            return (int) timeSpan.TotalSeconds;
        }
    }
}
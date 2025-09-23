using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Tools;

public static class CalendarLogic
{
    //todo: populate holidays
    static readonly HashSet<DateTime> Holidays = [];
    public static int WeekOfYear(DateTime time)
    {
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    public static DateTime GetFirstMondayOfWeek(int year, int weekNumber)
    {
        DateTime jan1 = new(year, 1, 1);
        DayOfWeek dayOfWeekJan1 = jan1.DayOfWeek;
        int daysOffset = (int)DayOfWeek.Monday - (int)dayOfWeekJan1;
        if (daysOffset > 0)
        {
            daysOffset -= 7;
        }
        DateTime firstMondayOfFirstWeek = jan1.AddDays(daysOffset);
        DateTime result = firstMondayOfFirstWeek.AddDays((weekNumber - 1) * 7);

        return result;
    }

    public static int GetWorkDays(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End date must be after start date");

        int workDays = 0;
        DateTime current = start;

        while (current <= end)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday &&
                current.DayOfWeek != DayOfWeek.Sunday &&
                !Holidays.Contains(current.Date))
            {
                workDays++;
            }
            current = current.AddDays(1);
        }

        return workDays;
    }

    public static DateTime AddWorkDays(DateTime startDate, int workDays)
    {

        if (workDays < 0)
            throw new ArgumentException("Work days cannot be negative");

        DateTime current = startDate;
        int addedDays = 0;

        while (addedDays < workDays)
        {
            current = current.AddDays(1);
            if (current.DayOfWeek != DayOfWeek.Saturday &&
                current.DayOfWeek != DayOfWeek.Sunday &&
                !Holidays.Contains(current.Date))
            {
                addedDays++;
            }
        }

        return current;
    }
}

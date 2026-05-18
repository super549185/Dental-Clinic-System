using System;
using Dental_Clinic_System.Models;

namespace Dental_Clinic_System.Data
{
    public static class StaffHelper
    {
        public static string GetDynamicStatus(StaffItem staff)
        {
            // If no schedule is set, default to Off Duty
            if (string.IsNullOrWhiteSpace(staff.Schedule)) return "Off Duty";

            try
            {
                // Split schedule: "Mon-Fri 9:00 AM - 5:00 PM" -> ["Mon-Fri", "9:00", "AM", "-", "5:00", "PM"]
                string[] parts = staff.Schedule.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 6) return "Off Duty";

                string daysRange = parts[0]; // "Mon-Fri"
                string startTime = parts[1] + " " + parts[2]; // "9:00 AM"
                string endTime = parts[4] + " " + parts[5]; // "5:00 PM"

                // 1. Check if today is a working day (e.g., Mon-Fri)
                if (!IsWorkingDay(daysRange, DateTime.Now.DayOfWeek)) return "Off Duty";

                // 2. Parse the times
                if (!DateTime.TryParse(startTime, out DateTime start) || !DateTime.TryParse(endTime, out DateTime end))
                    return "Off Duty";

                // 3. Check if current time is between start and end time
                TimeSpan now = DateTime.Now.TimeOfDay;
                if (now >= start.TimeOfDay && now <= end.TimeOfDay)
                {
                    return "On Duty";
                }
                else
                {
                    return "Off Duty";
                }
            }
            catch
            {
                return "Off Duty";
            }
        }

        private static bool IsWorkingDay(string daysRange, DayOfWeek today)
        {
            string[] days = daysRange.Split('-');
            if (days.Length != 2) return false;

            DayOfWeek startDay = ParseDay(days[0]);
            DayOfWeek endDay = ParseDay(days[1]);

            // Handle normal ranges (Mon-Fri) and wrap-around ranges (Sat-Mon)
            if (startDay <= endDay)
                return today >= startDay && today <= endDay;
            else
                return today >= startDay || today <= endDay;
        }

        private static DayOfWeek ParseDay(string day)
        {
            return day switch
            {
                "Mon" => DayOfWeek.Monday,
                "Tue" => DayOfWeek.Tuesday,
                "Wed" => DayOfWeek.Wednesday,
                "Thu" => DayOfWeek.Thursday,
                "Fri" => DayOfWeek.Friday,
                "Sat" => DayOfWeek.Saturday,
                "Sun" => DayOfWeek.Sunday,
                _ => DayOfWeek.Monday
            };
        }
    }
}
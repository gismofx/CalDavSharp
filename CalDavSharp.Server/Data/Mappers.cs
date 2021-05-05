using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using CalDavSharp.Server.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace CalDavSharp.Server.Data
{
    public static class Mappers
    {
        public static Event ToCalDavEvent(this Ical.Net.CalendarComponents.CalendarEvent icalEvent, string eventID, string calendarID, string rawICS)
        {
            return new Event()
            {
                CalendarId = calendarID,
                Duration = int.Parse(icalEvent.Duration.TotalMinutes.ToString()),
                StartDateUtc = icalEvent.DtStart.AsUtc,
                EndDateUtc = icalEvent.DtEnd.AsUtc,
                IsAllDay = icalEvent.IsAllDay,
                EventId = eventID,
                IsRecurring = icalEvent.RecurrenceRules.Count == 0 ? false : true,
                Title = icalEvent.Summary,
                LastModifiedUtc = icalEvent.LastModified.AsUtc,
                RecurrenceRule = "",
                ICS = rawICS
            };
        }
    }
}

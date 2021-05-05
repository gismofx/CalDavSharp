using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;


namespace CalDavSharp.Server.Models
{
    [Table("Event")]
    public class Event
    {
        [ExplicitKey]
        public string EventId { get; set; }
        public string CalendarId { get; set; }
        public string Title { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public DateTime LastModifiedUtc { get; set; }
        public bool IsAllDay { get; set; }
        public int Duration { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrenceRule { get; set; }
        public string ObjectType { get; set; }
        public string ETag { get; set; }
        public string ICS { get; set; }
    }
}

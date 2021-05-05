using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;

namespace CalDavSharp.Server.Models
{
    [Table("Calendar")]
    public class Calendar
    {
        [ExplicitKey]
        public string CalendarId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CalendarName { get; set; }
        public string Description { get; set; }

        public string cTag { get; set; }

        [Write(false)]
        public virtual ICollection<Event> Events { get; set; }
        /*
        public virtual ICollection<ToDo> ToDos { get; set; }
        public virtual ICollection<TimeZone> TimeZones { get; set; }
        public virtual ICollection<JournalEntry> JournalEntries { get; set; }
        public virtual ICollection<FreeBusy> FreeBusy { get; set; }
        */
    }
}

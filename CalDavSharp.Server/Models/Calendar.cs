using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;

namespace CalDavSharp.Server.Models
{
    public class Calendar
    {
        [ExplicitKey]
        public string Id { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperRepository;
using CalDavSharp.Server.Models;
using Dapper;
using System.Diagnostics;

namespace CalDavSharp.Server.Data
{
    public class CalendarRepository
    {

        private IRepository<Calendar> _CalendarRepository;
        private IRepository<Event> _EventRepository;
        public CalendarRepository(IRepository<Calendar> calendarRepo, IRepository<Event> eventRepo)
        {
            _CalendarRepository = calendarRepo;
            _EventRepository = eventRepo;
        }

        public async Task<Calendar> GetCalendarByUserandNameAsync(string user, string calendarName)
        {
            using (var conn = _CalendarRepository.DbConnection)
            {
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("UserName", user.ToUpper());
                dynamicParams.Add("Name", calendarName.ToUpper());
                return await conn.QueryFirstOrDefaultAsync<Calendar>("SELECT * FROM Calendar WHERE UserName LIKE @UserName AND CalendarName LIKE @Name",dynamicParams);
            }
            
        }

        public async Task<string> GetCalendarIdByUserandNameAsync(string user, string calendarName)
        {
            using (var conn = _CalendarRepository.DbConnection)
            {
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("UserName", user.ToUpper());
                dynamicParams.Add("Name", calendarName.ToUpper());
                return await conn.QueryFirstOrDefaultAsync<string>("SELECT CalendarId FROM Calendar WHERE UserName LIKE @UserName AND CalendarName LIKE @Name", dynamicParams);
            }

        }

        public async Task<IEnumerable<Event>> GetObjects(Calendar calendarRoot)
        {
            using (var conn = _CalendarRepository.DbConnection)
            {
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("Id", calendarRoot.CalendarId);
                return await conn.QueryAsync<Event>("SELECT * FROM Event WHERE CalendarId=@Id", dynamicParams);
            }
        }

        public async Task InsertEvent(Event eventToInsert)
        {
            //add etag logic?           
            await _EventRepository.InsertAsync(eventToInsert);
        }

        public async Task UpdateEvent(Event eventToUpdate)
        {
            //add etag logic?           
            await _EventRepository.UpdateAsync(eventToUpdate);
        }

        public async Task DeleteCalendarObject(string objectID)
        {
            using (var conn = _EventRepository.DbConnection)
            {
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("ObjectID", objectID);
                await conn.ExecuteAsync("DELETE FROM Event WHERE EventId=@ObjectId",dynamicParams);
            }
        }

        public async Task ImportIcalDotNetEvents(Calendar intoCalendar, Ical.Net.Calendar calendarToImport)
        {
            var s = new Ical.Net.Serialization.ComponentSerializer();
            using (var conn = _EventRepository.DbConnection)
            {
                foreach (var c in calendarToImport.Events)
                {
                    if (c.RecurrenceRules.Count > 0)
                    {
                        Debug.WriteLine("stop!");
                    }
                    var ev = new Event()
                    {
                        CalendarId = intoCalendar.CalendarId,
                        EventId = c.Uid,
                        IsAllDay = c.IsAllDay,
                        Duration = 0,//@ToDo: Implement or fix this
                        IsRecurring = c.RecurrenceRules.Count > 0 ? true : false,
                        RecurrenceRule = c.RecurrenceRules.Count > 0 ? s.SerializeToString(c.RecurrenceRules.First()) : null,
                        StartDateUtc = c.DtStart.AsUtc,
                        EndDateUtc = c.DtEnd.AsUtc,
                        ICS = s.SerializeToString(c),
                        Title = c.Summary,
                        ObjectType = c.Name
                    };
                    await _EventRepository.InsertAsync(ev,conn);
                }
            }
        }

        public async Task<Event> GetObjectByUID(string calendarID, string eventID)
        {
            using (var conn = _EventRepository.DbConnection)
            {
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("calendarId", calendarID,System.Data.DbType.String);
                dynamicParams.Add("eventId", eventID, System.Data.DbType.String);
                return await conn.QueryFirstOrDefaultAsync<Event>(@"SELECT * FROM Event WHERE CalendarId=@calendarId AND EventId=@eventId",dynamicParams);
            }
        }

        public async Task<string> UpdateCalendarCTag(string calendarId)
        {
            var hash = DateTime.UtcNow.GetHashCode().ToString();
            using (var conn = _CalendarRepository.DbConnection)
            {
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("ctag", hash);
                await conn.ExecuteAsync("Update Calendar Set cTag = @ctag", dynamicParams);
            }
            return hash;
        }

        public async Task<IQueryable<Event>> GetObjectsByFilter(string calendarId, dynamic filter = null)
        {
            throw new NotImplementedException();
            return null;
        }

    }
}

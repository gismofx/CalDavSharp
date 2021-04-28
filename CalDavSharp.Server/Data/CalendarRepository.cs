using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperRepository;
using CalDavSharp.Server.Models;
using Dapper;

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
                dynamicParams.Add("Owner", user.ToLower());
                dynamicParams.Add("Name", calendarName.ToLower());
                return await conn.QueryFirstOrDefaultAsync<Calendar>("SELECT * FROM Calendar WHERE LOWER(Owner) = @Owner AND LOWER(Name)=@Name",dynamicParams);
            }
            
        }

        public async Task<IEnumerable<Event>> GetObjects(Calendar calendarRoot)
        {
            return null;
            }

    }
}

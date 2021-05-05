using System;
using DapperRepository;
using CalDavSharp.Server.Models;
using CalDavSharp.Server.Data;
using Ical.Net;
using System.Data.SQLite;
using System.Threading.Tasks;
//using System.Data.SQLite;


namespace CalDavSharp.Testing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            //Path to ICS file
            string pathToICS = "";
            var mycalendar = Ical.Net.Calendar.Load(System.IO.File.OpenText(pathToICS));
            Ical.Net.Serialization.CalendarSerializer cc = new Ical.Net.Serialization.CalendarSerializer();
            var xx = cc.SerializeToString(mycalendar);

            var c = new CalDavSharp.Server.Models.Calendar()
            {
                CalendarId = "1",
                Description = "My Test Calendar",
                CalendarName = "Test",
                UserName = "me",
                UserId = "me@me.com"
            };

            var connectionString = "Data Source={AppDir}\\CalDavServer.sqlite;Version=3;";
            connectionString = connectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory);
            //change path
            connectionString = @"Data Source=...\repos\CalDavSharp\CalDavSharp.Server\CalDavServer.sqlite;Version=3";
            var conn = new SQLiteConnection(connectionString);

            var repo1 = new Repository<CalDavSharp.Server.Models.Calendar>(conn);
            var repo2 = new Repository<Event>(new SQLiteConnection(connectionString));


            var calrepo = new CalendarRepository(repo1, repo2);

            //await repo1.InsertAsync(c);

            await calrepo.ImportIcalDotNetEvents(c, mycalendar);
            

        }
    }
}

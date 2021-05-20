using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CalDavSharp.Server.Services;
using DapperRepository;
using System.Data.SQLite;
using CalDavSharp.Server.Models;
using CalDavSharp.Server.Data;
using LiteDB;
using System.Xml.Linq;
using System.Diagnostics;
using DapperIdentity.Services;

namespace CalDavSharp.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(options=>
            { options.MaxValidationDepth = 9999; }).AddXmlSerializerFormatters();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CalDavServer", Version = "v1" });
            });

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            connectionString = connectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory);
            

            services.AddDbConnectionInstantiatorForRepositories<SQLiteConnection>(connectionString);
            services.AddTransientRepository<Event>()
                .AddTransientRepository<Calendar>();

            services.AddSingleton<CalendarRepository>();
            services.AddTransient<CalDavManager>();

            services.AddBasicAuthController();

            services.AddLogging(loggingBuilder => {
                loggingBuilder.AddFile("app.log", append: true);
            });

            /* Try LiteDB as alternative to sqlite.. sticking with sqlite for now
            var mapper = BsonMapper.Global;
            mapper.Entity<Ical.Net.Calendar>()
                .Id(x => x.ProductId);

            mapper.Entity<Ical.Net.CalendarComponents.CalendarEvent>()
                .Ignore(x => x.Parent);
                
            
            using (var ldb = new LiteDatabase(@"PathTo\CalDavSharp.Server\MyData.db"))
            {
                var col = ldb.GetCollection<Ical.Net.CalendarComponents.CalendarEvent>("events");
                col.InsertBulk(mycalendar.);

            }
            */

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CalDavServer v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRequestResponseLogging();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            
            /*
            //for debugging
            app.Use(async (context, next) =>
            {
                //get path
                var path = context.Request.Path.ToString();
                Debug.WriteLine("*****intercepted path*****");
                Debug.WriteLine(path);
                Debug.WriteLine("**************************");
                //when path == what you want,do something

                if (path == "xxxx")
                {
                   
                }
                await next();
            });
            */
            

        }
    }
}

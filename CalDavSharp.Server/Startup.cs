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

            services.AddControllers().AddXmlSerializerFormatters();
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
            services.AddSingleton<CalDavParser>();
            services.AddTransient<CalDavManager>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CalDavServer_Play v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Info;
using Steeltoe.Management.Endpoint.Loggers;
using Steeltoe.Management.Endpoint.Trace;

namespace PalTracker
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
            services.AddMvc();

            services.AddSingleton(sp => new WelcomeMessage(
                Configuration.GetValue<string>("WELCOME_MESSAGE", "WELCOME_MESSAGE not configured.")
            ));

            services.AddSingleton(sp => new CloudFoundryInfo(
                Configuration.GetValue<string>("PORT"),
                Configuration.GetValue<string>("MEMORY_LIMIT"),
                Configuration.GetValue<string>("CF_INSTANCE_INDEX"),
                Configuration.GetValue<string>("CF_INSTANCE_ADDR")
            ));

            services.AddScoped<ITimeEntryRepository, MySqlTimeEntryRepository>();
            services.AddDbContext<TimeEntryContext>(options => options.UseMySql(Configuration));

            services.AddCloudFoundryActuators(Configuration);

            services.AddSingleton<IOperationCounter<TimeEntry>, OperationCounter<TimeEntry>>();
            services.AddSingleton<IHealthContributor, TimeEntryHealthContributor>();
            services.AddSingleton<IInfoContributor, TimeEntryInfoContributor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            if (Configuration.GetValue("DISABLE_AUTH", false))
            {
                // There is no easy way to turn off
                // OAuth based security so for the sake
                // of the assignment submission just
                // work around it.
                // Feature request:
                // https://github.com/SteeltoeOSS/Management/issues/6
                app.UseCloudFoundryActuator();
                app.UseInfoActuator();
                app.UseHealthActuator();
                app.UseLoggersActuator();
                app.UseTraceActuator();
            }
            else
            {
                // Add management endpoints into pipeline
                // and integrate with Apps Manager.
                // See: https://steeltoe.io/docs/steeltoe-management/#1-2-9-cloud-foundry
                app.UseCloudFoundryActuators();
            }
        }
    }
}

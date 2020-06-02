using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnoCore.Json;
using EnoCore.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GamemasterChecker
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
            services.AddSingleton(new GamemasterDatabase());
            services.AddLogging(loggingBuilder =>
            {
                if (Environment.GetEnvironmentVariable("USE_ELK") != null)
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddProvider(new EnoLogMessageConsoleLoggerProvider("GamemasterChecker", CancellationToken.None));
                }
            });
            services.AddHttpClient("default")
                .ConfigureHttpMessageHandlerBuilder(builder =>
                {
                    if (builder.PrimaryHandler is HttpClientHandler handler)
                    {
                        handler.UseCookies = false;
                    }
                });
            services.AddControllers(options => { options.InputFormatters.Insert(0, new RawJsonBodyInputFormatter()); })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.Converters.Add(new CheckerResultMessageJsonConverter());
                });
            services.AddSingleton(typeof(IChecker), typeof(GamemasterChecker));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

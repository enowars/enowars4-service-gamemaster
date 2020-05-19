using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Gamemaster.Database;
using Gamemaster.Hubs;
using Microsoft.AspNetCore.Rewrite;

namespace Gamemaster
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            services.AddSignalR();
            services.AddControllers();
            services.AddDbContextPool<GamemasterDbContext>(options => options.UseSqlite(GamemasterDbContextFactory.CONNECTION_STRING));
            services.AddScoped<IPnPAppDb, PnPAppDb>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IPnPAppDb db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseDefaultFiles();
            
            var rewrite = new RewriteOptions()
            .AddRewrite(@"^[\w\/]*$", "/index.html", true);
            app.UseRewriter(rewrite);
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<SessionHub>("/hubs/Session");
                endpoints.MapControllers();
            });
            db.Migrate();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IdentityCore
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var connectionString = @"Server=(localdb)\mssqllocaldb;database=IdentityCoreDB.IdentityUser;trusted_connection=True";
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<IdentityDbContext>(opt => opt.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(migrationAssembly)));

            // Aqui está uma implementação padrão do Identity, sem contemplar o conceito de Roles 
            //services.AddIdentityCore<IdentityUser>(options => { });

            services.AddIdentity<IdentityUser, IdentityRole>(options => { })
                .AddEntityFrameworkStores<IdentityDbContext>();

            // Aqui há a necessidade de implementar todos os métodos em CustomIdentityUserStore,
            // além de ter a necessidade de implementar um ContextDb específico para a entitdade IdentityUser
            // services.AddScoped<IUserStore<IdentityUser>, CustomIdentityUserStore>();

            services.AddScoped<IUserStore<IdentityUser>, UserOnlyStore<IdentityUser, IdentityDbContext>>();

            // Ao substituir AddIdentityCore por AddIdentity, este último já implementa uma
            // autenticação padrão com cookies. Então não há necessidade de acrescentar uma manualmente.
            //services.AddAuthentication("cookies")
            //.AddCookie("cookies", options => options.LoginPath = "/Home/Login");

            // Por outro lado, ao utilizar AddIdentity padrão, ele nos dá acesso ao contexto
            // de configuração de cookie, o qual alterar as funcionlidades
            services.ConfigureApplicationCookie(options => options.LoginPath = "/Home/Login");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

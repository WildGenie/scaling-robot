﻿using Librarian.Application;
using Librarian.Data;
using Librarian.Data.Contexts;
using Librarian.Identity;
using Librarian.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using VueCliMiddleware;

namespace Librarian.WebApi
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
            /*
             * Web API'nin çalışma zamanının ihtiyaç duyacağı Application,Data(Entity Framework context'ini alacak) ve Shared servislerini 
             * aşağıdaki metotlar yardımıyla ekliyoruz. Hatta JWT bazlı yetkilendirme ile kullanıcı doğrulama için Identity...
             * 
             * İlgili servisleri burada da açık bir şekilde ekleyebilirdik ancak yapmadık. 
             * Bu sayede o kütüphanelerin servislerinin DI koleksiyonuna eklenme işini buradan soyutlamış olduk.
             * Orada servislerde bir değişiklik olursa buraya gelip bir şeyler yapmamıza gerek kalmayacak.
             * 
             */
            services.AddApplication(Configuration);
            services.AddData(Configuration);
            services.AddAppIdentity(Configuration);
            services.AddShared(Configuration);

            // SPA için eklendi
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "../librarian-app/dist";
            });

            //services.AddDbContext<LibrarianDbContext>(options => options.UseSqlite("Data Source=LibrarianDatabase.sqlite3"));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization schema",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new List<string>()
                    }
                });
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Librarian.WebApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Librarian.WebApi v1"));
            }

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<JwtHandler>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "../librarian-app";

                if (env.IsDevelopment())
                {
                    spa.UseVueCli(npmScript: "serve"); // npm run server komutunu tetikler
                }
            });
        }
    }
}

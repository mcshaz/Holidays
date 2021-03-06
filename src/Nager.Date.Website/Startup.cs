using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Nager.Date.Website.Context;
using Nager.Date.Website.Context.Entities;
using Nager.Date.Website.Contract;
using Nager.Date.Website.Middleware;
using Nager.Date.Website.Services;
using System;
using System.IO;
using System.Reflection;

namespace Nager.Date.Website
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
            #region MappingConfig

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            #endregion

            #region Membership
            services.AddDbContext<UserContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<RegisteredConsumer>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 5;
                options.Password.RequiredLength = 8;
            })
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<UserContext>();

            services.Configure<AuthMessageSenderOptions>(Configuration);
            services.AddTransient<IEmailSender, EmailSender>();
            #endregion

            #region Session
            // this is used to allow the MVC page to consume the API
            // whereas cross site requests to the API must register for an api_key
            var allowApiAccessOptions = Configuration.GetSection("AllowApiAccessOptions");
            services.Configure<AllowApiAccessOptions>(allowApiAccessOptions);
            // slight hack here:
            services.AddAllowApiAccess(allowApiAccessOptions.Get<AllowApiAccessOptions>().APIKeyBypassSeconds);

            #endregion

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });
            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = System.IO.Compression.CompressionLevel.Fastest; });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                options.JsonSerializerOptions.Converters.Add(new PublicHolidayTypeConverter());
            });

            services.AddControllersWithViews();

            services.AddCors(o => o.AddPolicy("ApiPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .WithMethods("GET")
                       .AllowAnyHeader();
            }));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Title = "Nager.Date API",
                    Description = "Nager.Date is open source software and is completely free for commercial use. If you would like to support the project you can award a GitHub star ??? or even better <a href='https://github.com/sponsors/nager'>actively support us</a>",
                    Contact = new OpenApiContact
                    {
                        Name = "Nager.Date on GitHub",
                        Url = new Uri("https://github.com/nager/Nager.Date/issues")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://github.com/nager/Nager.Date/blob/master/LICENSE.md")
                    },
                    Version = "v1.0"
                });

                c.TagActionsBy(api => new[] { api.GroupName });
                c.DocInclusionPredicate((name, api) => true);
                c.AddSecurityDefinition("query_api_key", new OpenApiSecurityScheme {
                    Type = SecuritySchemeType.ApiKey,
                    Name = "api_key",
                    In = ParameterLocation.Query
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "query_api_key"
                            },
                            Name = "query_api_key",
                            In = ParameterLocation.Query,
                        },
                        Array.Empty<string>()
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Create good names with NSwag
                c.CustomOperationIds(description =>
                {
                    var actionDescriptor = description.ActionDescriptor as ControllerActionDescriptor;
                    return $"{actionDescriptor.ControllerName}{actionDescriptor.ActionName}";
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Initialize Mapster
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetEntryAssembly());

            var enableCors = Configuration.GetValue<bool>("EnableCors");
            var enableIpRateLimiting = Configuration.GetValue<bool>("EnableIpRateLimiting");
            var enableSwaggerMode = Configuration.GetValue<bool>("EnableSwaggerMode");

            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Nager.Date API V1");

                if (enableSwaggerMode)
                {
                    c.RoutePrefix = string.Empty;
                }
            });

            if (!enableSwaggerMode)
            {
                app.UseHttpsRedirection();
            }

            app.UseResponseCompression();

            if (!enableSwaggerMode)
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        const int CacheDays = 365;
                        const int DurationInSeconds = 60 * 60 * 24 * CacheDays;
                        ctx.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={DurationInSeconds}";
                        ctx.Context.Response.Headers[HeaderNames.Expires] = new[] { DateTime.UtcNow.AddDays(CacheDays).ToString("R") }; // Format RFC1123
                    }
                });
            }

            app.UseRouting();

            // please note https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-5.0
            // The CORS middleware must be configured to execute between the calls to UseRouting and UseEndpoints.
            // If adding UseAuthorization and/or UseResponseCaching, calls must also be placed after useCors.

            if (enableCors)
            {
                app.UseCors("ApiPolicy");
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAllowApiAccess();

            if (enableSwaggerMode)
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
            else
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                    // below required for default membership pages
                    endpoints.MapRazorPages();
                });
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UW.Data;
using UW.Controllers.JsonRpc2;
using UW.Shared;
using UW.Shared.Services;

using Microsoft.AspNetCore.Builder;
using System.IO;
using System.IO.Compression;

namespace UW
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<Ntfy>();
            services.AddSingleton<Persistence>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new JwtValidator());
            });

            services.AddJsonRpc()
                .WithOptions(config =>
                {
                    config.ShowServerExceptions = true;
                    config.BatchRequestLimit = null;
                });

            services.AddMvc().AddControllersAsServices();
            services.AddTransient<RpcNotification>();

            services.AddLogging(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("EdjCase", LogLevel.Warning)
                    .AddConsole();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                new Playground();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                // app.UseHttpsRedirection();
            }

            // app.UseMvc();

            app.UseAuthentication();

            app.Map("/api", rpcApp =>
            {
                rpcApp.Use(LogRequest);
                rpcApp.UseManualJsonRpc(builder =>
                {
                    builder.RegisterController<RpcAuth>("auth");
                    builder.RegisterController<RpcContacts>("contacts");
                    builder.RegisterController<RpcProfile>("profile");
                    // builder.RegisterController<RpcPlatform>("platform");
                    builder.RegisterController<RpcNotification>("notification");
                    // builder.RegisterController<RpcTrading>("trading");
                    // builder.RegisterController<RpcExCurrency>("excurrency");
                    // builder.RegisterController<RpcTest>("test");
                });
            });
        }
        public async Task LogRequest(HttpContext context, Func<Task> next)
        {
            ILogger<Startup> logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
            using (MemoryStream newRequestStream = new MemoryStream())
            {
                // logger.LogInformation(context.Request.Headers.ToArray().ToJson());

                Stream requestStream = context.Request.Body;
                context.Request.Body.CopyTo(newRequestStream);
                newRequestStream.Seek(0, SeekOrigin.Begin);
                string requestBody = new StreamReader(newRequestStream).ReadToEnd();

                newRequestStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = newRequestStream;

                string responseBody;
                using (MemoryStream newBodyStream = new MemoryStream())
                {
                    Stream bodyStream = context.Response.Body;
                    context.Response.Body = newBodyStream;

                    await next();

                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    if (context.Response.Headers["Content-Encoding"].Contains("gzip"))
                    {
                        using (var cs = new GZipStream(newBodyStream, CompressionMode.Decompress, true))
                        {
                            responseBody = new StreamReader(cs).ReadToEnd();
                        }
                    }
                    else
                    {
                        responseBody = new StreamReader(newBodyStream).ReadToEnd();
                        logger.LogInformation(context.Response.Headers["Content-Encoding"]);
                    }

                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    newBodyStream.CopyTo(bodyStream);
                    context.Response.Body = bodyStream;
                }

                var log = new
                {
                    IP = context.Connection.RemoteIpAddress.ToString(),
                    JWT = context.Request.Headers?["Authorization"],
                    UserId = context.User.FindFirst(c => c.Type == D.CLAIM.USERID)?.Value,
                    RequestBody = requestBody?.ToObject(),
                    ResponseBody = responseBody?.ToObject()
                };
                logger.LogInformation("\n" + log.ToJson());
            }
        }
    }
}

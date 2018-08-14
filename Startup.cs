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
using UW.JsonRpc;
using UW.JWT;
using UW.Services;

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
            services.AddSingleton<Notifications>();
            services.AddSingleton<Persistence>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));

            var jwtSettings = new JwtSettings();
            Configuration.Bind("JwtSettings", jwtSettings);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                // {
                //     ValidIssuer = jwtSettings.Issuer,
                //     ValidAudience = jwtSettings.Audience,
                //     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                //     ValidateIssuer = true,
                //     ValidateAudience = true,
                //     ValidateIssuerSigningKey = true,
                //     RequireExpirationTime = false,
                // };
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new ApiTokenValidator());
                // options.Events = new JwtBearerEvents(){
                //     OnMessageReceived = context => {
                //         var token = context.Request.Headers["myToken"];
                //         context.Token = token.FirstOrDefault();
                //         return Task.CompletedTask;
                //     }
                // };
            });

            services.AddJsonRpc(config =>
            {
                config.ShowServerExceptions = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // app.UseMvc();

            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.Map("/api", rpcApp =>
            {
                rpcApp
                .UseManualJsonRpc(builder =>
                {
                    builder.RegisterController<RpcMath>("math");
                    builder.RegisterController<RpcAuth>("auth");
                    builder.RegisterController<RpcNotification>("notification");
                });
            });
        }
    }
}

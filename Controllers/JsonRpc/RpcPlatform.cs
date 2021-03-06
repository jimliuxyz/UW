﻿using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Router.Abstractions;
using EdjCase.JsonRpc.Router;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System;
using UW.Data;
using UW.Core.Persis.Collections;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using UW.Core;

namespace UW.Controllers.JsonRpc
{
    public class RpcPlatform : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Persistence db;
        private IHostingEnvironment env;
        public RpcPlatform(IHttpContextAccessor accessor, Persistence db, IHostingEnvironment env)
        {
            this.accessor = accessor;
            this.db = db;
            this.env = env;
        }

        /// <summary>
        /// 取得平台資訊
        /// todo : 僅用於測試,待移除或權限控管
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult info()
        {
            User user = null;
            try
            {
                var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;
                user = db.getUserByUserId(userId);
            }
            catch (System.Exception) { }

            return this.Ok(new
            {
                token_owner = user,
                csharp_env = new
                {
                    EnvironmentName = env.EnvironmentName,
                    ContentRootPath = env.ContentRootPath,
                    WebRootPath = env.WebRootPath,
                },
                shell_env = new
                {
                    REGION_NAME = Environment.GetEnvironmentVariable("REGION_NAME"),
                }
            });
        }

    }
}


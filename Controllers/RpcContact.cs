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
using UW.Services;
using UW.Data;
using UW.Models.Collections;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;

namespace UW.JsonRpc
{
    [Authorize]
    public class RpcContact : RpcController
    {
        private IHttpContextAccessor accessor;
        private Notifications notifications;
        private Persistence db;
        public RpcContact(IHttpContextAccessor accessor, Notifications notifications, Persistence db)
        {
            this.accessor = accessor;
            this.notifications = notifications;
            this.db = db;
        }

        public IRpcMethodResult getAllUsers()
        {
            return Ok(db.getUsers());
        }

    }
}

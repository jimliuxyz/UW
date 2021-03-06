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
using UW.Controllers;
using UW.Core;
using UW.Core.Services;

namespace UW.Controllers.JsonRpc
{
    [Authorize]
    public class RpcProfile : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Ntfy notifications;
        private Persistence db;
        public RpcProfile(IHttpContextAccessor accessor, Ntfy notifications, Persistence db)
        {
            this.accessor = accessor;
            this.notifications = notifications;
            this.db = db;
        }

        /// <summary>
        /// 取得自身用戶資料
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult getProfile()
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == "userid").Value;

            try
            {
                var user = db.getUserByUserId(userId);

                return Ok(new
                {
                    userId = user.userId,
                    name = user.name,
                    phoneno = user.phoneno,
                    avatar = user.avatar,
                    currencies = user.currencies
                });
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                return ERROR_ACT_FAILED;
            }
        }

        /// <summary>
        /// 取得其他用戶資料
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IRpcMethodResult getUsersProfile(string[] userIds)
        {
            var users = db.getUserByUserId(userIds);

            //重新map 排除隱私內容
            var users_ = users.Select(user => new
            {
                userId = user.userId,
                name = user.name,
                phoneno = user.phoneno,
                avatar = user.avatar
            });
            return Ok(new
            {
                list = users_
            });
        }

        /// <summary>
        /// 更新用戶資料
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult updateProfile(string[] keys, string[] values)
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            var user = db.getUserByUserId(userId);

            try
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    if (key == D.DOC_USER_NAME)
                        user.name = values[i];
                }
                db.upsertUser(user);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                return ERROR_ACT_FAILED;
            }

            return Ok();
        }

        public IRpcMethodResult updateCurrencySetting(List<CurrencySettings> list)
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            var user = db.getUserByUserId(userId);

            try
            {
                user.currencies.AddRange(list);
                user.currencies = user.currencies.Where((x, i) => user.currencies.FindLastIndex(z => z.name == x.name) == i).ToList();

                db.upsertUser(user);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                return ERROR_ACT_FAILED;
            }

            return Ok();
        }
    }
}


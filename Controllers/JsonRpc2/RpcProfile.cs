using Microsoft.AspNetCore.Authorization;
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
using UW.Shared.Persis.Collections;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;
using UW.Controllers;
using UW.Shared;
using UW.Shared.Services;
using UW.Shared.Persis.Helper;
using UW.Shared.Misc;
using Microsoft.Extensions.Logging;

namespace UW.Controllers.JsonRpc2
{
    [Authorize]
    public class RpcProfile : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Ntfy notifications;

        private UserHelper userHelper = new UserHelper();
        private Pkuid uid;

        public RpcProfile(ILogger<RpcProfile> logger, IHttpContextAccessor accessor, Ntfy notifications)
        {
            this.accessor = accessor;
            this.notifications = notifications;

            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID)?.Value;

            uid = UserHelper.IdGen.Parse(userId);
        }

        /// <summary>
        /// 取得自身用戶資料
        /// </summary>
        /// <returns></returns>
        public async Task<IRpcMethodResult> getProfile()
        {
            try
            {
                var user = await userHelper.GetById(uid);

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
        public async Task<IRpcMethodResult> getUsersProfile(string[] userIds)
        {
            var users = await userHelper.GetByIds(userIds);

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
        public async Task<IRpcMethodResult> updateProfile(string[] keys, string[] values)
        {
            try
            {
                var user = await userHelper.GetById(uid);

                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    if (key == D.DOC_USER_NAME)
                        user.name = values[i];
                }
                await userHelper.Update(user);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());
                return ERROR_ACT_FAILED;
            }

            return Ok();
        }

        public async Task<IRpcMethodResult> updateCurrencySetting(List<CurrencySettings> list)
        {
            var user = await userHelper.GetById(uid);

            try
            {
                //todo : remove unavailable currency to avoid attacks
                user.currencies.AddRange(list);
                user.currencies = user.currencies.Where((x, i) => user.currencies.FindLastIndex(z => z.name == x.name) == i).ToList();

                await userHelper.Update(user);
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


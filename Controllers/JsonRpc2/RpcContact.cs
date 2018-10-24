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
using UW.Core.Persis.Collections;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using UW.Core;
using UW.Core.Services;
using UW.Core.Persis.Helper;
using UW.Core.Misc;
using Microsoft.Azure.Documents;
using System.Net;

namespace UW.Controllers.JsonRpc2
{
    [Authorize]
    public class RpcContacts : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Ntfy notifications;

        private UserHelper userHelper = new UserHelper();
        private ContactsHelper contactsHelper = new ContactsHelper();
        private Pkuid uid;
        public RpcContacts(IHttpContextAccessor accessor, Ntfy notifications)
        {
            this.accessor = accessor;
            this.notifications = notifications;

            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID)?.Value;

            uid = UserHelper.IdGen.Parse(userId);
        }

        /// <summary>
        /// 取得所有使用者
        /// todo:此api僅供測試,待移除
        /// </summary>
        /// <returns></returns>
        public async Task<IRpcMethodResult> getAllUsers()
        {
            try
            {
                var resList = (await userHelper.GetFirst100()).Select(user =>
                    new
                    {
                        userId = user.userId,
                        name = user.name,
                        avatar = user.avatar
                    }
                );
                return Ok(new
                {
                    list = resList
                });
            }
            catch (DocumentClientException e)
            {
                Console.WriteLine(e.Message);
                return ERROR_ACT_FAILED;
            }
        }

        /// <summary>
        /// 取得連絡人
        /// </summary>
        /// <param name="friends"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> getContacts()
        {
            try
            {
                var contacts = await contactsHelper.Get(uid);
                return Ok(new
                {
                    list = contacts.friends
                });
            }
            catch (System.Exception)
            {
                return ERROR_ACT_FAILED;
            }
        }

        /// <summary>
        /// 新增好友
        /// todo : 上限500人?
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> addFriends(List<string> list)
        {
            try
            {
                await contactsHelper.Add(uid, list);
                return Ok();
            }
            catch (System.Exception)
            {
                return ERROR_ACT_FAILED;
            }
        }

        /// <summary>
        /// 刪除好友
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> delFriends(List<string> list)
        {
            try
            {
                await contactsHelper.Del(uid, list);
                return Ok();
            }
            catch (System.Exception)
            {
                return ERROR_ACT_FAILED;
            }
        }

        public async Task<IRpcMethodResult> setFriendFavourite(string userId, bool favourite)
        {
            return ERROR_ACT_FAILED;
        }

        /// <summary>
        /// 以電話號碼取得使用者列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<IRpcMethodResult> findUsersByPhone(List<string> list)
        {
            var resList = new List<dynamic>();

            try
            {
                resList = (await userHelper.GetByPhonenos(list.ToArray())).Select(u => new
                {
                    userId = u.userId,
                    name = u.name,
                    avatar = u.avatar
                }).ToList<dynamic>();
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                }
                else
                    return ERROR_ACT_FAILED;
            }

            return Ok(new
            {
                list = resList
            });
        }


    }
}


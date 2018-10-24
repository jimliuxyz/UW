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

namespace UW.Controllers.JsonRpc
{
    [Authorize]
    public class RpcContacts : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Ntfy notifications;
        private Persistence db;
        public RpcContacts(IHttpContextAccessor accessor, Ntfy notifications, Persistence db)
        {
            this.accessor = accessor;
            this.notifications = notifications;
            this.db = db;
        }

        /// <summary>
        /// 取得所有使用者
        /// todo:此api僅供測試,待移除
        /// </summary>
        /// <returns></returns>
        public IRpcMethodResult getAllUsers()
        {
            return Ok(new
            {
                list = db.getUsers().Select(user =>
                {
                    return new
                    {
                        userId = user.userId,
                        name = user.name,
                        avatar = user.avatar
                    };
                })
            });
        }

        /// <summary>
        /// 取得連絡人
        /// </summary>
        /// <param name="friends"></param>
        /// <returns></returns>
        public IRpcMethodResult getContacts()
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            var contacts = db.getContact(userId);

            return Ok(new
            {
                list = contacts.friends
            });
        }

        /// <summary>
        /// 新增好友
        /// todo : 上限500人?
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IRpcMethodResult addFriends(List<string> list)
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            db.addFriends(userId, list);
            return Ok();
        }

        /// <summary>
        /// 刪除好友
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IRpcMethodResult delFriends(List<string> list)
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == D.CLAIM.USERID).Value;

            db.delFriends(userId, list);
            return Ok();
        }

        /// <summary>
        /// 以電話號碼取得使用者列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IRpcMethodResult findUsersByPhone(List<string> list)
        {
            return Ok(new
            {
                list = db.findUsersByPhone(list).Select(user =>
                {
                    return new
                    {
                        userId = user.userId,
                        name = user.name,
                        avatar = user.avatar
                    };
                })
            });
        }


    }
}


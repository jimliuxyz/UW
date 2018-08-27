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
using UW.Services;
using UW.Data;
using UW.Models.Collections;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace UW.Controllers.JsonRpc
{
    [Authorize]
    public class RpcContacts : RpcBaseController
    {
        private IHttpContextAccessor accessor;
        private Notifications notifications;
        private Persistence db;
        public RpcContacts(IHttpContextAccessor accessor, Notifications notifications, Persistence db)
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
            return Ok(db.getUsers());
        }

        /// <summary>
        /// 取得連絡人
        /// </summary>
        /// <param name="friends"></param>
        /// <returns></returns>
        public IRpcMethodResult getContacts()
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == KEYSTR.CLAIM_USERID).Value;

            var contacts = db.getContact(userId);

            return Ok(new
            {
                contacts = contacts.friends,
                recent = new List<Friend>()
            });
        }

        /// <summary>
        /// 新增(或更新)好友
        /// todo : 上限500人?
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IRpcMethodResult addFriends(List<Friend> list)
        {
            var userId = this.accessor.HttpContext.User.FindFirst(c => c.Type == KEYSTR.CLAIM_USERID).Value;

            db.addFriends(userId, list);
            return Ok(true);
        }

        /// <summary>
        /// 刪除好友
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public IRpcMethodResult delFriends(List<Friend> list)
        {
            return Ok(db.getUsers());
        }
    }
}


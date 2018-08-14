using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Router.Abstractions;
using EdjCase.JsonRpc.Router;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Identity;
using UW.Services;

namespace UW.JsonRpc
{
    [Authorize]
    public class RpcMath : RpcController
    {
        private IHttpContextAccessor _accessor;
        private Notifications _notifications;
        public RpcMath(IHttpContextAccessor accessor, Notifications notifications){
            _accessor = accessor;
            _notifications = notifications;
        }
        public IRpcMethodResult MethodResult(int a, int b)
        {
            // return this.Ok("Test");
            return this.Ok(new
            {
                ans = a+b,
                phoneno = _accessor.HttpContext.User.Identity.Name
            });
        }

        public int Add(int a, int b)
        {
            return a + b;
        }

        public string broadcastMessage(string message){
            _notifications.broadcastMessage(message);
            return message;
        }
        public string sendMessage(string message, string to){
            _notifications.sendMessage(message, to);
            return message;
        }

        public async Task<int> AddAsync(int a, int b)
        {
            return await Task.Run(() => a + b);
        }

        public int AddArray(int[] a)
        {
            return a[0] + a[1];
        }

        public int AddList(List<int> a)
        {
            return a[0] + a[1];
        }

    }
}

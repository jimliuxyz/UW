using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Router.Abstractions;
using EdjCase.JsonRpc.Router;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Identity;

namespace UW.JsonRpc
{
    [Authorize]
    public class RpcMath : RpcController
    {
        private IHttpContextAccessor _accessor;
        public RpcMath(IHttpContextAccessor accessor){
            _accessor = accessor;
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

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
using Microsoft.AspNetCore.Hosting;
using UW.Core.MQueue;
using System.Threading;
using UW.Core.Misc;
using UW.Core.MQueue.Handlers;
using UW.Core;
using UW.Core.MQueue.MQException;
using UW.Core.Persis;
using UW.Core.Persis.Helper;

namespace UW.Controllers.JsonRpc
{
    public class RpcTest : RpcBaseController
    {
        public RpcTest()
        {
        }

        public async Task<IRpcMethodResult> testQueue()
        {
            try
            {
                var res = await MQTesting1.SendAndWaitReply(60000);
                return this.Ok(res);
            }
            catch (MQReplyTimeoutException)
            {
                return Bad(RPCERR.ACTION_TIMEOUT);
            }
        }

        public async Task<IRpcMethodResult> test()
        {
            try
            {
                var helper = new UserHelper();
                var guid = helper.GenUid();

                var phoneno = "XXX" + F.Random(100000000, 999999999);

                var res = await MQUserCreate.CreateUser(guid.ToString(), phoneno);

                // await Task.Delay(1000);
                res = await MQUserCreate.CreateUser(guid.ToString(), phoneno);
                return this.Ok(res);
            }
            catch (MQReplyTimeoutException)
            {
                return Bad(RPCERR.ACTION_TIMEOUT);
            }
        }
    }
}


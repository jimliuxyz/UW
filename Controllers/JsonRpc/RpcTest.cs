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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using UW.Shared.MQueue;
using System.Threading;
using UW.Shared.Misc;
using UW.Shared.MQueue.Handlers;
using UW.Shared;
using UW.Shared.MQueue.MQException;
using UW.Shared.Persis;

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
                // await MQTesting1.Send();
                // return this.Ok(true);
                var res = await MQTesting1.SendAndWaitReply(3000);
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
                var guid = await helper.GenUid();

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


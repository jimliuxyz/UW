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

namespace UW.Controllers.JsonRpc
{

    public class RpcTest : RpcBaseController
    {
        public RpcTest()
        {
        }

        public async Task<IRpcMethodResult> test()
        {
            var res = await MQTesting1.SendAndWaitReply();
            // await MQCreateUser.SendWithReply("12345");

            // var waiter = MQReplyCenter.NewWaiter("12345");

            // var res = await waiter.wait();

            return this.Ok(res);
            // return this.Ok(true);
        }

    }
}


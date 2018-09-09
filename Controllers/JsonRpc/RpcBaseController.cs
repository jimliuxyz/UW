using EdjCase.JsonRpc.Router;
using EdjCase.JsonRpc.Router.Defaults;

namespace UW.Controllers
{
    public abstract class RpcBaseController : RpcController
    {
        public static readonly RpcMethodErrorResult ERROR_ACT_FAILED
     = genError(RPCERR.ACTION_FAILED);

        public static RpcMethodErrorResult Bad(RPCERR err){
            return new RpcMethodErrorResult(err.code, err.msg);
        }

        private static RpcMethodErrorResult genError(RPCERR err)
        {
            return new RpcMethodErrorResult(err.code, err.msg);
        }
    }
}

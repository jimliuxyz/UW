using EdjCase.JsonRpc.Router;
using EdjCase.JsonRpc.Router.Defaults;

namespace UW.Controllers
{
    public abstract class RpcBaseController : RpcController
    {
        public static readonly int AUTHENTICATION_FAILED = -1001;
        public static readonly int ACTION_FAILED = -1002;

        public static readonly RpcMethodErrorResult ERROR_ACT_FAILED
     = new RpcMethodErrorResult(ACTION_FAILED, "Action failed");

    }
}

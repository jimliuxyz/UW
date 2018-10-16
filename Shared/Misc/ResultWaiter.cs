using System;
using System.Threading;
using System.Threading.Tasks;

namespace UW.Shared.Misc
{
    // public class ResultWaiter
    // {
    //     private Object _result = null;
    //     private AutoResetEvent _event = new AutoResetEvent(false);
    //     public async Task<Object> wait()
    //     {
    //         await Task.Run(() =>
    //         {
    //             _event.WaitOne();
    //         });
    //         return _result;
    //     }

    //     public void wakeup(Object result = null)
    //     {
    //         _result = result;
    //         _event.Set();
    //     }
    // }


    public class ResultWaiter
    {
        private Object _result = null;
        private static ManualResetEvent _event = new ManualResetEvent(false);

        private bool awake = false;
        public async Task<Object> wait()
        {
            await Task.Run(async () =>
            {
                while (!awake)
                {
                    await Task.Delay(10);   //async polling is more efficient than thread locker
                    // _event.WaitOne();    //too many waiter makes thread leak
                    // _event.Reset();
                }
            });
            return _result;
        }
        public void wakeup(Object result = null)
        {
            awake = true;
            _result = result;
            // _event.Set();
        }
    }
}
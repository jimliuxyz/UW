using System;
using System.Threading;
using System.Threading.Tasks;

namespace UW.Shared.Misc
{
    public class ResultWaiter
    {
        private Object _result = null;
        private AutoResetEvent _event = new AutoResetEvent(false);
        public async Task<Object> wait()
        {
            await Task.Run(() =>
            {
                _event.WaitOne();
            });
            return _result;
        }

        public void wakeup(Object result = null)
        {
            _result = result;
            _event.Set();
        }
    }
}
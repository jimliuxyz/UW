using System.Threading.Tasks;

namespace UW.Shared.MQueue.Interface
{
    public abstract class IReceiver
    {
        abstract public Task StartPeek();
        abstract public Task StartReceive();
    }
}
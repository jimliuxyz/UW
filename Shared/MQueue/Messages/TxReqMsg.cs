namespace UW.Shared.MQueue.Messages
{
    public class TxReqMsg
    {
        public string uid {get; set;}
        public int type {get; set;}
        public decimal amount {get; set;}
        
    }
}
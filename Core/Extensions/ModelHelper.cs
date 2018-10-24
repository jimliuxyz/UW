using UW.Core.Persis.Collections;

namespace UW.Core
{
    /// <summary>
    /// ModelHelper
    /// </summary>
    public static class ModelHelper
    {
        /// <summary>
        /// 將Collection Model轉型為api回傳的結果
        /// </summary>
        public static dynamic ToApiResult(this TxReceipt receipt)
        {
            return new
            {
                id = receipt.receiptId,
                datetime = receipt.datetime,
                currency = receipt.currency,
                message = receipt.message,
                statusCode = receipt.statusCode,
                statusMsg = receipt.statusMsg,
                txType = receipt.txType,
                txParams = receipt.txParams,
                txResult = receipt.txResult
            };
        }

        public static TxReceipt Derivative(this TxReceipt receipt, string currency, string ownerId, TxActResult txResult)
        {
            var rec = receipt.DeepClone();
            rec.receiptId = F.NewGuid();
            rec.isParent = false;
            rec.parentId = receipt.receiptId;

            rec.currency = currency;
            rec.ownerId = ownerId;
            rec.txResult = txResult;
            return rec;
        }
    }
}
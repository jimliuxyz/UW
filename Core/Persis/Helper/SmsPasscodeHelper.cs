using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using UW.Core.Misc;
using UW.Core.Persis.Collections;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using Microsoft.Azure.Documents;

namespace UW.Core.Persis.Helper
{
    //todo : store procedure
    public class SmsPasscodeHelper : PersisHelper
    {
        private readonly DocumentClient client;
        static SmsPasscodeHelper()
        {
        }

        public SmsPasscodeHelper()
        {
            client = GetClient();
        }

        // public async Task<User> Create(Pkuid guid, string phoneno, string name = "", string avatar = "")
        // {
        // }

        private string PhoneToPK(string phoneno)
        {
            //todo: mod(phoneno, 0, 1000)
            return phoneno;
        }

        public async Task<dynamic> IsSmsPasscodeMatched(string phoneno, string passcode)
        {
            var passed = false;
            var error = RPCERR.PASSCODE_EXPIRED;

            var phonenoHash = phoneno.ToHash(R.SALT_SMS);
            var passcodeHash = (phoneno + passcode).ToHash(R.SALT_SMS);
            var pk = PhoneToPK(phoneno);

            var feedOpt = new FeedOptions
            {
                PartitionKey = new PartitionKey(pk),
            };
            var reqOpt = new RequestOptions
            {
                PartitionKey = new PartitionKey(pk),
            };

            // 取得SmsPasscode
            var result = await
            client.CreateDocumentQuery<SmsPasscode>(SmsPasscode._URI_COL, feedOpt)
                .Where(p => p.phoneno == phonenoHash)
                .AsDocumentQuery()
                .ToListAsync();

            if (result.Count() > 0)
            {
                var sms = result.ToList().First();

                //是否在有效期間內
                if (sms.verifyAvailTime.CompareTo(DateTime.UtcNow) > 0)
                {
                    //是否嘗試3次內
                    if (sms.verifyCount < 3)
                    {
                        //是否正確
                        if (passcodeHash == sms.passcode)
                        {
                            passed = true;

                            //刪除sms
                            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(R.DB_NAME, SmsPasscode._COLLECTION_NAME, sms.id), reqOpt);
                        }
                        else
                        {
                            error = RPCERR.PASSCODE_MISMATCH;

                            //更新sms
                            sms.verifyCount++;
                            await client.UpsertDocumentAsync(SmsPasscode._URI_COL, sms, reqOpt);
                        }
                    }
                    else
                        error = RPCERR.PASSCODE_VERIFY_EXCEEDED;
                }
                else
                    error = RPCERR.PASSCODE_EXPIRED;
            }

            return new
            {
                passed,
                error
            };
        }
    }
}
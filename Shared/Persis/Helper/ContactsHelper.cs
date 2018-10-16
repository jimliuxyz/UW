using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using UW.Shared.Misc;
using UW.Shared.Persis.Collections;
using User = UW.Shared.Persis.Collections.User;

/**
    todo : 用stored procedure取代頻繁讀取的操作
    todo : 丟msg queue做反正規 (1:50000000時反正規代價太大)
 */
namespace UW.Shared.Persis.Helper
{
    public class ContactsHelper : BaseHelper
    {
        private readonly DocumentClient client;

        static ContactsHelper()
        {
        }

        public ContactsHelper()
        {
            client = GetClient();
        }

        public async Task<Contacts> Create(Pkuid uid)
        {
            var data = new Contacts()
            {
                userId = uid.Guid,
                pk = uid.PK,
                vol = 0,
            };

            var res = await client.CreateDocumentAsync(Contacts._URI_COL, data, new RequestOptions
            {
                PartitionKey = new PartitionKey(uid.PK)
            });

            data.id = ((Document)res).Id;
            return data;
        }

        public async Task Update(Contacts data)
        {
            var res = await client.ReplaceDocumentAsync(GetDocumentUri(data.id), data);
            // return res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Created;
        }

        private Uri GetDocumentUri(string docId)
        {
            return UriFactory.CreateDocumentUri(Contacts._DB_NAME, Contacts._COLLECTION_NAME, docId);
        }

        public async Task<Contacts> Get(Pkuid uid)    //Get or Create
        {
            var result = await client.CreateDocumentQuery<Contacts>(Contacts._URI_COL,
                new FeedOptions { PartitionKey = new PartitionKey(uid.PK) })
                .Where(d => d.userId == uid.Guid)
                .AsDocumentQuery()
                .ExecuteNextAsync<Contacts>();

            if (result.Count == 0)
                return await Create(uid);

            return result.FirstOrDefault();
        }

        public async Task Add(Pkuid uid, List<string> list)
        {
            var userHelper = new UserHelper();

            var users = await userHelper.GetByIds(list.ToArray());

            var friends = users.Select(u =>
                new Friend
                {
                    userId = u.userId,
                    name = u.name,
                    avatar = u.avatar
                }
            ).ToList();

            await Add(uid, friends);
        }
        public async Task Add(Pkuid uid, List<Friend> friends)
        {
            var contact = await Get(uid);

            // 去重
            contact.friends.AddRange(friends);
            contact.friends = contact.friends.Where((x, i) => contact.friends.FindLastIndex(z => z.userId == x.userId) == i).ToList();

            var res = await client.UpsertDocumentAsync(Contacts._URI_COL, contact, new RequestOptions { PartitionKey = new PartitionKey(uid.PK) });
        }

        public async Task Del(Pkuid uid, List<string> rmlist)
        {
            var contact = await Get(uid);

            // 移除
            contact.friends = contact.friends.Where((x, i) => rmlist.FindIndex(z => z == x.userId) < 0).ToList();

            var res = await client.UpsertDocumentAsync(Contacts._URI_COL, contact, new RequestOptions { PartitionKey = new PartitionKey(uid.PK) });
        }

    }
}
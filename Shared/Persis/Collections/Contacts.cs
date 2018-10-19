using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace UW.Shared.Persis.Collections
{
    public partial class Contacts
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string pk { get; set; }
        public string userId { get; set; }  //unique key
        public int vol { get; set; }  //unique key

        public List<string> friendOf { get; set; } = new List<string>();  //怎麼限制數量?
        public List<Friend> friends { get; set; } = new List<Friend>();
        public List<Friend> recents { get; set; } = new List<Friend>();
    }

    public class Friend
    {
        public string userId { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public bool favourite { get; set; }
    }


    public partial class Contacts
    {
        public static readonly string _COLLECTION_NAME = "Contacts";
        public static readonly string _PK = "/pk";


        public static readonly string _DB_NAME = R.DB_NAME;
        public static readonly Uri _URI_DB = UriFactory.CreateDatabaseUri(_DB_NAME);
        public static readonly Uri _URI_COL = UriFactory.CreateDocumentCollectionUri(_DB_NAME, _COLLECTION_NAME);

        public static readonly DocumentCollection _SPEC = new DocumentCollection
        {
            Id = _COLLECTION_NAME,
            IndexingPolicy = new IndexingPolicy(
                new RangeIndex(DataType.String) { Precision = -1 }
            )
            {
                IndexingMode = IndexingMode.Consistent,
                IncludedPaths = new Collection<IncludedPath>{
                    new IncludedPath {
                        Path = "/pk/*",
                        Indexes = new Collection<Index>()
                        {
                            new RangeIndex(DataType.Number) { Precision = -1 }
                        }
                    },
                    new IncludedPath {
                        Path = "/userId/*",
                        Indexes = new Collection<Index>()
                        {
                            new HashIndex(DataType.String) { Precision = -1 }
                        }
                    },
                },
                ExcludedPaths = new Collection<ExcludedPath>{
                    new ExcludedPath {
                        Path = "/"
                    }
                }
            },
            UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys = new Collection<UniqueKey>
                {
                    new UniqueKey { Paths = new Collection<string> { "/userId/vol" }},
                }
            },
            PartitionKey = new PartitionKeyDefinition
            {
                Paths = new Collection<string> { _PK }
            }
        };
    }
}


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace UW.Shared.Persis.Collections
{
    public partial class SysConfig
    {
        public string id { get; set; } = "SysConfig";
        public string pk { get; set; } = "SysConfig";
        public bool disableTx { get; set; } = false;
        public List<CurrencyConfig> currencies { get; set; } = DefCurrencyConfig;
    }

    public partial class ServConfig
    {
        public string id { get; set; }
        public string pk { get; set; } = "ServConfig";
        public bool disable { get; set; } = false;

        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? TimeToLive { get; set; } = 3 * 60;
    }

    public class CurrencyConfig
    {
        public string name { get; set; }
        public string icon { get; set; } = "";
    }

    public partial class SysConfig
    {
        static List<CurrencyConfig> DefCurrencyConfig = new List<CurrencyConfig>
        {
            new CurrencyConfig{
                name = D.CNY,
                icon = ""
            },
            new CurrencyConfig{
                name = D.USD,
                icon = ""
            },
            new CurrencyConfig{
                name = D.BTC,
                icon = ""
            },
            new CurrencyConfig{
                name = D.ETH,
                icon = ""
            }
        };
    }

    public partial class SysConfig
    {
        public static readonly string _COLLECTION_NAME = "SysConfig";
        public static readonly string _PK = "/pk";


        public static readonly string _DB_NAME = R.DB_NAME;
        public static readonly Uri _URI_DB = UriFactory.CreateDatabaseUri(_DB_NAME);
        public static readonly Uri _URI_COL = UriFactory.CreateDocumentCollectionUri(_DB_NAME, _COLLECTION_NAME);

        public static readonly DocumentCollection _SPEC = new DocumentCollection
        {
            Id = _COLLECTION_NAME,
            DefaultTimeToLive = -1,
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
                }
            },
            PartitionKey = new PartitionKeyDefinition
            {
                Paths = new Collection<string> { _PK }
            }
        };
    }

}
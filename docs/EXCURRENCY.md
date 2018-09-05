
https://uwbackend-dev.azurewebsites.net/api/excurrency

# 取得匯率

```js
//送
{
    "jsonrpc": "2.0",
    "method": "getExRate",
    "params": {},
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "USDCNY-sell": 6.86415,
        "USDCNY-buy": 6.79585,
        "BTCUSD-sell": 7031.81415,
        "BTCUSD-buy": 6961.84585,
        "ETHUSD-sell": 282.93765,
        "ETHUSD-buy": 280.12235
    },
    "error": null
}
```

# 預估換匯金額

```js
//送 (100人民幣兌換為美金)
{
    "jsonrpc": "2.0",
    "method": "estimateExFrom",
    "params": {
    	"fromCurrency": "CNY",
    	"toCurrency": "USD",
    	"fromAmount": "100"
    },
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "amount": 14.568446202370286197125645564
    },
    "error": null
}
```

> 手機端可嘗試自行換算 降低server負載

# 執行換匯

```js
//送
{
    "jsonrpc": "2.0",
    "method": "doExFrom",
    "params": {
    	"fromCurrency": "CNY",
    	"toCurrency": "USD",
    	"fromAmount": "100",
    	"message": "..."
    },
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "receiptId": "Lc7fnO73TkyrsAogiMEf8A=="
    },
    "error": null
}
```

叫用後立即回傳receiptId, 之後再透過notification方式傳送交易結果

```js
//以apple為例 用以下格式通知
{
    "aps": {
        "alert": "message"
    },
    "custom": {
        "type": "TX_RECEIPT",
        "payload": {
            "receiptId": "....",
            "action": "exchange",
            "status": 0,   //0 means done, -1 means failed, other means processing
            "message": "...",
            "fromCurrency": "cny",
            "fromAmount": "100",
            "toCurrency": "..."
        }
    }
}
```

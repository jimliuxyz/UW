
http://uwbackend-asia.azurewebsites.net/api/trading

# 取得餘額

```js
//送
{
    "jsonrpc": "2.0",
    "method": "getBalances",
    "params": {},
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": [
        {
            "name": "cny",
            "balance": "1000"
        },
        {
            "name": "usd",
            "balance": "1000"
        },
        {
            "name": "bitcoin",
            "balance": "1000"
        },
        {
            "name": "ether",
            "balance": "1000"
        }
    ],
    "error": null
}
```

# 付款/轉帳

```js
//送
{
    "jsonrpc": "2.0",
    "method": "transfer",
    "params": {
        "currency": "cny",
        "amount": "1000",
        "toUserId": "bae84936-bbbe-46ca-bf8c-9127f3239fa2"
    }
    "error": null
    "id": 99,
}
//收
{
    "jsonrpc": "2.0",
    "result": {
        "receiptId": "...."
    },
    "error": null,
    "id": 99,
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
            "action": "transfer",
            "status": 0,   //0 means done, -1 means failed, other means processing
            "currency": "cny",
            "amount": "100",
            "fromUserId": "...",
            "fromUserName": "jim",
            "toUserId": "...",
            "toUserName": "alan"
        }
    }
}
```

# 存錢 (未實作)

```js
//送
{
    "jsonrpc": "2.0",
    "method": "deposit",
    "params": {
        "currency": "cny",
        "amount": "1000"
    }
    "error": null
    "id": 99,
}
//收
{
    "jsonrpc": "2.0",
    "result": true,
    "error": null,
    "id": 99,
}
```

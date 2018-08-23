
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

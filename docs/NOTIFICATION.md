
https://uwbackend-dev.azurewebsites.net/api/notification

# 註冊(或變更)PNS(Push Notification Service) token

```js
//送
{
    "jsonrpc": "2.0",
    "method": "regPnsToken",
    "params": {
        "pns": "gcm",
        "pnsToken": " fOrBAl3Wj30:APA91bEWBIZa4ICzJCzusa3-udLTBML0G8U5oQmq_OcCF__ZnVpCzw_-UcFfidTBbhAN8CdsXhSO-mJk7zv4v8KKRxyM6UAt8KSbD3C7AMP-iIsuvgBbc8bXMellSzBtI-p4kZXWTXFvitCJ9UavwpNWMwD1hZ1MWA"
    },
    "id": 99
}

//收
{
    "jsonrpc": "2.0",
    "result": null,
    "error": null
    "id": 99,
}
```

# 發送訊息(個別)(測試用)

```js
//送
{
    "jsonrpc": "2.0",
    "method": "sendMessage",
    "params": {
        "userId": "bae84936-bbbe-46ca-bf8c-9127f3239fa2",
        "message": "hello"
    },
    "id": 99
}
//收
{
    "jsonrpc": "2.0",
    "result": null,
    "error": null
    "id": 99,
}
```

### 發送訊息(全體)(測試用)

```js
//送
{
    "jsonrpc": "2.0",
    "method": "broadcast",
    "params": {
        "message": "hello"
    },
    "id": 99
}
//收
{
    "jsonrpc": "2.0",
    "result": null,
    "error": null
    "id": 99,
}
```

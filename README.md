```sh
dotnet watch run
dotnet publish
```

# API

流程說明
- 登入系統取得token 之後連線必須將該token加入http的header
- 註冊裝置的pns token 以便之後發送通知
- 取得使用者列表(包含userId)
- 用userId發送訊息

### 登入系統
http://uwbackend-asia.azurewebsites.net/api/auth

```js
//送
{
    "jsonrpc": "2.0",
    "method": "login",
    "params": {
        "phoneno": "1234567890",
        "passcode": "3333",
    },
    "id": 99
}

//收
{
    "jsonrpc": "2.0",
    "result": {
        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9tb2JpbGVwaG9uZSI6IjEyMzQ1Njc4OTAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zaWQiOiIxMjM0NTY3ODkwIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IjEyMzQ1Njc4OTAiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJ1c2VyIiwidXNlcmlkIjoiYmFlODQ5MzYtYmJiZS00NmNhLWJmOGMtOTEyN2YzMjM5ZmEyIiwiZGV2aWNldHlwZSI6ImFuZHJvaWQiLCJkZXZpY2V0b2tlbiI6ImRldmljZXRva2VuIiwiaXNzIjoieGlud2FuZyIsImF1ZCI6InV3YWxsZXQifQ.jK0rC3-ffVCGe0R94n54G-9bVd1dIEAKxd2j9iYsvYE"
    },
    "error": null
    "id": 99,
}
```

- Auth改為auth
- passcode暫以3333替代

### 註冊PNS(Push Notification Service) token
http://uwbackend-asia.azurewebsites.net/api/notification

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
    "result": true,
    "error": null
    "id": 99,
}
```

### 取得user列表 (尚未實作)
http://uwbackend-asia.azurewebsites.net/api/contact

```js
//送
{
    "jsonrpc": "2.0",
    "method": "getAllUsers",
    "params": null,
    "id": 99
}
//收
{
    "jsonrpc": "2.0",
    "result": [
        {
            "userId": "bae84936-bbbe-46ca-bf8c-9127f3239fa2",
            "name":"jim",
            "phoneno":"1234567890"
        },
        {
            "userId": "bae84936-bbbe-46ca-bf8c-9127f3239fa2",
            "name":"alan",
            "phoneno":"1234567890"
        }
    ],
    "error": null
    "id": 99,
}
```

### 發送訊息(個別) (尚未實作)
http://uwbackend-asia.azurewebsites.net/api/notification

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
    "result": true,
    "error": null
    "id": 99,
}
```

### 發送訊息(全體) (尚未實作)
http://uwbackend-asia.azurewebsites.net/api/notification

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
    "result": true,
    "error": null
    "id": 99,
}
```

http://uwbackend-asia.azurewebsites.net/api/auth


# 登入系統

登入後請將token置於往後連接的http header中

```js
//送
{
    "jsonrpc": "2.0",
    "method": "login",
    "params": {
        "phoneno": "1234567890",
        "passcode": "8888",
        "pns": "apns",
        "pnsToken": "f607a1efa8ec3beb994d810a4b93623b81a257332aff8a9709990ba1611478c1"
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

重複登入時會發出以下通知到前裝置 目的在通知用戶將被登出
```js

//以apple為例 用以下格式通知
{
    "aps": {
        "alert": "someone logged into your account\nyou've got to logout!"
    },
    "custom": {
        "type": "LOGOUT",
        "payload": null
    }
}
```

# 檢查token(JWT)是否仍有效

照理說 應該在每個api呼叫時 後端自行檢查token是否有效
但我考慮到 每次檢查都需查詢資料庫 會造成一些延遲與開銷
故先嘗試以app啟動時主動查詢token是否有效

這個api現在有兩個目的 一個就是排除重複登入 另一個就是當前端或後端有較大異動時可以透過這個方式讓user重新註冊
```js
//送
{
    "jsonrpc": "2.0",
    "method": "isTokenAvailable",
    "params": {},
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": true,
    "error": null
}
```


http://uwbackend-asia.azurewebsites.net/api/auth


### 登入系統

登入後請將token置於往後連接的http header中

```js
//送
{
    "jsonrpc": "2.0",
    "method": "login",
    "params": {
        "phoneno": "1234567890",
        "passcode": "8888",
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

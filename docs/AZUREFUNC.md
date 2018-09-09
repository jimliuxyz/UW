
# 發送簡訊驗證碼

每一小時可發3次簡訊 每一個passcode有效期限10分鐘 可以驗證3次

https://uwfuncapp-dev.azurewebsites.net/api/reqSmsVerify

```js
//送
{
    "jsonrpc": "2.0",
    "method": "reqSmsVerify",
    "params": {
        "phoneno": "test-jim-123"
    },
    "id": 99
}
//收
{
    "jsonrpc": "2.0",
    "id": 99,
    "result": {
        "resendCount": 1
    },
    "error": null
}
//收(超過發送次數)
{
    "jsonrpc": "2.0",
    "id": 99,
    "result": null,
    "error": {
        "code": -1001,
        "message": "SMS resend limit Exceeded"
    }
}
```

> TODO : 後端會依時間單位內發送次數 控制回傳值

# 更改頭像

https://uwfuncapp-dev.azurewebsites.net/api/uploadAvatar

- http header必須帶JWT token
- form-data方式夾帶一圖檔

```js
//收
{
  "jsonrpc": "2.0",
  "id": 0,
  "result": {
    "url": "https://uwdefstorage.blob.core.windows.net/avatar/200/4kswGUTIi0Cvg7z-ZWpD9A==.jpg"
  },
  "error": null
}
```



# 發送簡訊驗證碼

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
  "result": null,
  "error": null
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


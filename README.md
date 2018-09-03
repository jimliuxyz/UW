# 更新內容(180903)

> 為了區分`release`與`develop`環境 我們將有至少有兩組api網域 例如:
 api service:
  http://uwbackend-rel.azurewebsites.net/api/
  http://uwbackend-dev.azurewebsites.net/api/
 function app:
  https://uwfuncapp-rel.azurewebsites.net/api/
  https://uwfuncapp-dev.azurewebsites.net/api/
 前端也必須有機制切換`release`與`develop`環境 (例如用程式變數或編譯參數?)

- cny/usd/bitcoin/ether字串更改為`CNY/USD/BTC/ETH`

- Function app的api送收也改為jsonRpc樣式 [參考](./docs/FUNCAPP.md)

- JsonRPC中的result將僅回傳null/物件/空物件 `不會直接回傳數字/字串/布林` 主要是方便用dictionary解析 與格式上的一至性
```js
//原本
{
    "jsonrpc": "2.0",
    "result": true,
    "error": null,
    "id": 99,
}
//更改為
{
    "jsonrpc": "2.0",
    "result": null,
    "error": null,
    "id": 99,
}
//這些狀況出現在...
//PROFILE : updateProfile
//CONTACTS : addFriends/delFriends
//NOTIFICATION : regPnsToken/sendMessage/broadcast
//TRADING : deposit
```


# Funciton App
將棄用

#### 發送簡訊驗證碼
https://uwfuncapp.azurewebsites.net/api/reqSmsVerify?phoneno=1234567890
- 原本回的status改為statusCode 正確時回傳200 錯誤時則依照http的規範

#### 更改頭像
https://uwfuncapp.azurewebsites.net/api/uploadAvatar
- http header必須帶token
- post動作夾帶一個圖檔
- 回傳
```js
{
    "statusCode": 200,
    "result": "https://uwdefstorage.blob.core.windows.net/avatar/200/{RANDOM_ID}.jpg"
}
```

# API Server (JsonRPC over https)

[FUNC APP - 授權](./docs/FUNCAPP.md)

[AUTH - 授權](./docs/AUTH.md)

[PROFILE - 個資](./docs/PROFILE.md)

[CONTACTS - 聯絡人](./docs/CONTACTS.md)

[NOTIFICATION - 通知](./docs/NOTIFICATION.md)

[TRADING - 交易](./docs/TRADING.md)

[EXRATE - 匯率](./docs/EXRATE.md)


# Notification通知格式

```js
//以apple為例
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
custom中的type若為null時 表示為`一般訊息`通知
#### type
- LOGOUT : 通知使用者將被登出 (token失效或其他因素))
- TX_RECEIPT : 交易收據 (交易雙方都會通知)


# 歷史更新



### 更新內容(180831)

- 新增 CONTACTS - findUsersByPhone 以電話號碼搜尋聯絡人

### 更新內容(180830)

- 更新 AUTH - login
```
  輸入參數新增pns與pnsToken 用以讓後端識別與前次裝置是否為不同裝置 以便發送登出通知
  (之後pnsToken有更動仍然要透過regPnsToken更新)
```

### 更新內容(180829)

- 更新 AUTH - login
```
  出現重複login時 會發'通知'到前裝置
```
- 新增 AUTH - isTokenAvailable
```
  用來檢查token是否有效
  每次app啟動時檢查一次
```
> 細節請再查閱api

### 更新內容(180828)

- 新增 付款api

### 更新內容(180827-2)
- 頭像url檔名改為隨機字串
- 修正 頭像url未正確讀出

### 更新內容(180827)

- 新增 頭像api
- 發送簡訊api返回的status改為statusCode, 正確時回傳200, 錯誤時則依照http的規範回傳status code (因為azure function的http trigger原本就依照這個規範回傳)

### 更新內容(180822)

- /api/profile 取得個人資料 與 貨幣設定
- /api/contact 取得聯絡人列表
- /api/trading 取得貨幣餘額
- http將被關閉 請改用https
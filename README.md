
# 更新內容(180911)

- 更正 Profile - getProfile/getUsersProfile 其中回傳的id欄位名改為userId

# 函數編程(functional programming)
在集合資料(Collection)轉換上可以善用`函數式編程(functional programming)`

關鍵字map/reduce/filter/sort

[swift參考](https://medium.com/@mikru168/swift3-%E9%AB%98%E9%9A%8E%E5%87%BD%E6%95%B8-higher-order-function-a97cf4577a11)

[kotlin參考](https://hk.saowen.com/a/e37e7e51f8f92225bdff9dbcb47ddfc9937a7af2bf3952385ff637ee34a6ee56)

# 建議安裝postman
可下載import [json檔](./docs/UWallet.postman_collection.json)做測試 (自行更改為遠端url)

出現ssl問題可到setting將`SSL cerificate verification`關閉

# 更新內容(180909)
- JsonRpc的params與result內都一律使用`物件{}`
- JsonRpc中物件中單一陣列盡量以`list`命名

- 修改 Contacts - getAllUser與findUsersByphone 回傳`id更改為userId`
- 修改 Contacts - addFriends 改傳入`字串陣列`
- 修改 Contacts - getContacts 回傳的contacts變數改為`list` (移除recent)

- 新增 Profile - updateCurrencySetting 更新貨幣設定

- 修改 Trading - getBalance 回傳改用map型別
```js
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "CNY": 1000,
        "USD": 1000,
        "BTC": 1000,
        "ETH": 1000
    },
    "error": null
}
```
- 新增 Trading - getReceipts 取得交易紀錄
- 修改 Trading 透過`通知`傳達的內容 改為與getReceipts相同
- 新增 AzureFunc - reqSmsVerify 正確時回傳發送次數 超過發送次數也將回傳錯誤
- 新增 Auth - login 錯誤時將回傳錯誤
- 新增 ErrorCode 定義相關於簡訊驗證的錯誤
- 萬用passcode改為88888888(手機端無法輸入)

# 更新內容(180905)

- 修改 Trading - transfer 參數加上message 供轉帳交易時紀錄訊息
- 新增 ExCurrency 處理匯率交易 [參考](./docs/EXCURRENCY.md)
- 新增[錯誤碼定義](./docs/DEFINE.md)
- 當任何api回傳以下內容 請將user登出 重新導到登入頁
```json
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": null,
    "error": {
        "code": -32600,
        "message": "Unauthorized",
        "data": null
    }
}
```

# API Service (JsonRPC over https)

[Azure Func](./docs/AZUREFUNC.md)

[AUTH - 授權](./docs/AUTH.md)

[PROFILE - 個資(設定))](./docs/PROFILE.md)

[CONTACTS - 聯絡人](./docs/CONTACTS.md)

[NOTIFICATION - 通知](./docs/NOTIFICATION.md)

[TRADING - 交易](./docs/TRADING.md)

[EXCURRENCY - 匯率交易](./docs/EXCURRENCY.md)

[各式**碼**定義](./docs/DEFINE.md)

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



### 更新內容(180903)

```
 為了區分`release`與`develop`環境 我們將至少有兩組api網域 例如:
 api service:
  https://uwbackend-rel.azurewebsites.net/api/
  https://uwbackend-dev.azurewebsites.net/api/
 function app:
  https://uwfuncapp-rel.azurewebsites.net/api/
  https://uwfuncapp-dev.azurewebsites.net/api/
 前端也必須有機制切換`release`與`develop`環境 (例如用程式變數或編譯參數?)
```
- cny/usd/bitcoin/ether字串更改為`CNY/USD/BTC/ETH`

- Azure Function的api送收也改為jsonRpc樣式 [參考](./docs/AZUREFUNC.md)

- JsonRPC中的result將僅回傳null/物件/空物件 `不會再直接回傳數字/字串/布林` 主要是方便用dictionary/map解析 與格式上的一至性
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
//AUTH : isTokenAvailable
//PROFILE : updateProfile
//CONTACTS : addFriends/delFriends
//NOTIFICATION : regPnsToken/sendMessage/broadcast
//TRADING : deposit
```


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
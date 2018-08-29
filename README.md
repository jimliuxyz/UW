# 更新內容(180829)

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

# 更新內容(180828)

- 新增 付款api

# Funciton App

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

# API Server (JsonRPC over http/https)

[AUTH - 授權](./docs/AUTH.md)

[PROFILE - 個資](./docs/PROFILE.md)

[CONTACTS - 聯絡人](./docs/CONTACTS.md)

[NOTIFICATION - 通知](./docs/NOTIFICATION.md)

[TRADING - 交易](./docs/TRADING.md)

[EXRATE - 匯率](./docs/EXRATE.md)

# 歷史更新


# 更新內容(180827-2)
- 頭像url檔名改為隨機字串
- 修正 頭像url未正確讀出

# 更新內容(180827)

- 新增 頭像api
- 發送簡訊api返回的status改為statusCode, 正確時回傳200, 錯誤時則依照http的規範回傳status code (因為azure function的http trigger原本就依照這個規範回傳)

### 更新內容(180822)

- /api/profile 取得個人資料 與 貨幣設定
- /api/contact 取得聯絡人列表
- /api/trading 取得貨幣餘額
- http將被關閉 請改用https
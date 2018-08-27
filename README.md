
# 更新內容(180827)

- 新增 頭像api
- 發送簡訊api返回的status改為statusCode, 正確時回傳200, 錯誤時則依照http的規範回傳status code (因為azure function的http trigger原本就依照這個規範回傳)

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
    "result": "https://uwdefstorage.blob.core.windows.net/avatar/200/{USER_ID}.jpg"
}
```
- 圖片名稱暫時固定用{USER_ID}.jpg 之後會改為亂數字串

# API Server (JsonRPC over http/https)

[AUTH - 授權](./docs/AUTH.md)

[PROFILE - 個資](./docs/PROFILE.md)

[CONTACT - 聯絡人](./docs/CONTACTS.md)

[NOTIFICATION - 通知](./docs/NOTIFICATION.md)

[TRADING - 交易](./docs/TRADING.md)

[EXRATE - 匯率](./docs/EXRATE.md)

# 歷史更新
### 更新內容(180822)

- /api/profile 取得個人資料 與 貨幣設定
- /api/contact 取得聯絡人列表
- /api/trading 取得貨幣餘額
- http將被關閉 請改用https
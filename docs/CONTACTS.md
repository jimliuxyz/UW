
https://uwbackend-dev.azurewebsites.net/api/contacts


# 取得聯絡人名單

> 聯絡人沒以map方式回傳而用list是因為資訊到手機端時仍須排序 以list較為方便

```js
//送
{
    "jsonrpc": "2.0",
    "method": "getContacts",
    "params": {},
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "list": [
            {
                "userId": "tempid-886986123456",
                "name": "buzz",
                "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
            },
            {
                "userId": "tempid-886986123457",
                "name": "jessie",
                "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-jessie.png"
            }
        ]
    },
    "error": null
}
```

# 新增聯絡人

```js
//送
{
    "jsonrpc": "2.0",
    "method": "addFriends",
    "params": {
    	"list": ["c6e3cd69-9755-4506-8c3a-4c64b1ca1ebf"]
    },
    "id": 99
}
//收
{
    "jsonrpc": "2.0",
    "result": true,
    "error": null,
    "id": 99,
}
```

# 刪除聯絡人

```js
//送
{
    "jsonrpc": "2.0",
    "method": "delFriends",
    "params": {
        "list": ["c6e3cd69-9755-4506-8c3a-4c64b1ca1ebf", "c6e3cd69-9755-4506-8c3a-4c64b1ca1ebf"]
    }
    "error": null
    "id": 99,
}
//收
{
    "jsonrpc": "2.0",
    "result": true,
    "error": null,
    "id": 99,
}
```

# 以電話號碼搜尋使用者

```js
//送
{
    "jsonrpc": "2.0",
    "method": "findUsersByPhone",
    "params": {
    	"list": ["886986123456"]
    },
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "list": [
            {
                "userId": "tempid-886986123456",
                "name": "buzz",
                "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
            }
        ]
    },
    "error": null
}
```

# 取得所有user列表 (測試用途)

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
    "result": {
        "list": [
            {
                "userId": "tempid-886986123456",
                "name": "buzz",
                "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
            },
            {
                "userId": "tempid-886986123457",
                "name": "jessie",
                "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-jessie.png"
            }
        ]
    },
    "error": null
    "id": 99,
}
```

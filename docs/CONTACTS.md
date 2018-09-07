
https://uwbackend-dev.azurewebsites.net/api/contacts


# 取得聯絡人名單

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
        "tempid-886986123456": {
            "name": "buzz",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
        },
        "tempid-886986123457": {
            "name": "jessie",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-jessie.png"
        }
    },
    "error": null
}
```

# 新增(或更新)聯絡人

```js
//送
{
    "jsonrpc": "2.0",
    "method": "addFriends",
    "params": {
    	"list": [
    		{
                "userId": "mock-id-3",
                "name": "buzz",
                "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
            }
         ]
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
        "ids": ["c6e3cd69-9755-4506-8c3a-4c64b1ca1ebf", "c6e3cd69-9755-4506-8c3a-4c64b1ca1ebf"]
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
    	"list": ["886919123456"]
    },
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "tempid-886986123456": {
            "name": "buzz",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
        },
        "tempid-886986123457": {
            "name": "jessie",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-jessie.png"
        }
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
        "tempid-886978123123": {
            "name": "alan",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-woody.png"
        },
        "tempid-886986123456": {
            "name": "buzz",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
        },
        "tempid-886986123457": {
            "name": "jessie",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-jessie.png"
        },
        "tempid-test-jim-123": {
            "name": "test-jim-123",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-woody.png"
        }
    },
    "error": null
    "id": 99,
}
```

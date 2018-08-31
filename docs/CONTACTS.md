
http://uwbackend-asia.azurewebsites.net/api/contacts


# 取得聯絡人名單

- 主要用於list 往後若需個別用戶的詳細內容將透過profile取回

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
        "contacts": [
            {
                "userId": "mock-id-1",
                "name": "buzz",
                "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
            },
            {
                "userId": "mock-id-2",
                "name": "jessie",
                "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-jessie.png"
            }
        ],
        "recent": []
    },
    "error": null
}
```

# 新增(或更新)聯絡人

- 暫時只有新增

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

# 刪除聯絡人 (未實作)

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
    "result": [
        {
            "id": "bae84936-bbbe-46ca-bf8c-9127f3239fa2",
            "name": "Nate",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-woody.png"
        }
    ],
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
    "result": [
        {
            "id": "bae84936-bbbe-46ca-bf8c-9127f3239fa2",
            "name":"jim",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
        },
        {
            "id": "bae84936-bbbe-46ca-bf8c-9127f3239fa2",
            "name":"alan",
            "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-buzz.png"
        }
    ],
    "error": null
    "id": 99,
}
```

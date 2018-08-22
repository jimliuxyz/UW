
http://uwbackend-asia.azurewebsites.net/api/profile

# 取得個人資料 (連帶個人設定)

```js
//送
{
    "jsonrpc": "2.0",
    "method": "getProfile",
    "params": {},
    "id": 99
}
//收
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "id": "040564a6-f5dd-4ba1-8d03-b428e4126b81",
        "name": "test123",
        "phoneno": "test123",
        "avatar": "https://ionicframework.com/dist/preview-app/www/assets/img/avatar-ts-woody.png",
        "currencies": [
            {
                "name": "cny",
                "order": 0,
                "isDefault": true,
                "isVisible": false
            },
            {
                "name": "usd",
                "order": 1,
                "isDefault": false,
                "isVisible": false
            },
            {
                "name": "bitcoin",
                "order": 2,
                "isDefault": false,
                "isVisible": false
            },
            {
                "name": "ether",
                "order": 3,
                "isDefault": false,
                "isVisible": false
            }
        ]
    },
    "error": null
}
```

# 更新個人資料

現在只有name

```js
//送
{
    "jsonrpc": "2.0",
    "method": "updateProfile",
    "params": {
    	"keys": ["name"],
    	"values": ["Jim"]
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

# 取得用戶資料

如果有取得用戶`細部`資料的需求 將以此api取得, 若從contacts足以取得足夠資料 此api可能最終會棄用.

```js
//送
{
    "jsonrpc": "2.0",
    "method": "getUsersProfile",
    "params": {
    	"userIds": ["c6e3cd69-9755-4506-8c3a-4c64b1ca1ebf"]
    },
    "id": 99
}
//收
{
    "jsonrpc": "2.0",
    "result": [
        {
            "id": "c6e3cd69-9755-4506-8c3a-4c64b1ca1ebf",
            "name": "Jim",
            "phoneno": "test123",
            "avatar": "http://..."
        }
    ],
    "error": null
    "id": 99,
}
```

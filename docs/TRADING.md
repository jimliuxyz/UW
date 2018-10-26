
https://uwbackend-dev.azurewebsites.net/api/trading

# 取得餘額

```js
//送
{
    "jsonrpc": "2.0",
    "method": "getBalances",
    "params": {},
    "id": 99
}
//收
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

# 查詢交易紀錄

```js
//送
{
    "jsonrpc": "2.0",
    "method": "getReceipts",
    "params": {
    	"fromDatetime": "2018-09-09T05:44:25.537037Z"
    },
    "id": 99
}
//收 (以換匯為例 CNY換USD)
{
    "id": 99,
    "jsonrpc": "2.0",
    "result": {
        "list": [
            {
                "id": "HO8tSTYvhEeeuWgl20DmTg",
                "datetime": "2018-09-09T05:44:25.537037Z",
                "currency": "CNY",  //此筆紀錄所屬幣別
                "message": "test",
                "statusCode": 0,    //0表示這筆交易已成功完成
                "statusMsg": "",
                "txType": 4,        //此筆紀錄為換匯交易
                "txParams": { //交易發起時的參數
                    "toCurrency": "USD",    //轉換幣別(只有在txType為4時才會出現)
                    "sender": "tempid-test-jim-123-3",  //付款人
                    "receiver": "tempid-test-jim-123-3",//收款人
                    "currency": "CNY",      //交易幣別
                    "amount": 10            //交易金額
                },
                "txResult": { //交易成功完成造成的金流
                    "outflow": true,    //出帳
                    "amount": 10,       //金額為10
                    "balance": 860
                }
            },
            {
                "id": "jKwEcgCx-EuGZH45I7XsPg",
                "datetime": "2018-09-09T05:44:25.538187Z",
                "currency": "USD",
                "message": "test",
                "statusCode": 0,
                "statusMsg": "",
                "txType": 4,
                "txParams": {
                    "toCurrency": "USD",
                    "sender": "tempid-test-jim-123-3",
                    "receiver": "tempid-test-jim-123-3",
                    "currency": "CNY",
                    "amount": 10
                },
                "txResult": {
                    "outflow": false,    //入帳
                    "amount": 1.4568446202370287,
                    "balance": 1018.938980063117
                }
            }
        ]
    },
    "error": null
}
```

#### 交易類型 (txType)
碼 | 描述
--- | ---
1 | 存款
2 | 提款
3 | 轉帳
4 | 換匯

#### 交易狀態 (txStatusCode)
碼 | 描述
--- | ---
0 | 完成
-1 | 失敗
-2 | 持續中/逾時 (未必失敗,成功後將以`通知`告知用戶端)


# 付款/轉帳

```js
//送
{
    "jsonrpc": "2.0",
    "method": "transfer",
    "params": {
        "currency": "cny",
        "amount": "1000",
        "toUserId": "bae84936-bbbe-46ca-bf8c-9127f3239fa2",
        "message": "..."
    },
    "error": null,
    "id": 99,
}
//收
{
    "jsonrpc": "2.0",
    "result": {
        "receiptId": "....",
        "statusCode": 0
    },
    "error": null,
    "id": 99,
}
```

# 收款

```js
//送
{
    "jsonrpc": "2.0",
    "method": "receive",
    "params": {
        "currency": "cny",
        "amount": "1000",
        "fromUserId": "bae84936-bbbe-46ca-bf8c-9127f3239fa2",
        "message": "..."
        "secureToken": "" //暫時留空
    },
    "error": null,
    "id": 99,
}
//收
{
    "jsonrpc": "2.0",
    "result": {
        "receiptId": "....",
        "statusCode": 0
    },
    "error": null,
    "id": 99,
}
```

當statusCode出現`逾時`,將以`通知`方式將交易結果回傳到用戶端

```js
//以apple為例 用以下格式通知
{
    "aps": {
        "alert": "message"
    },
    "custom": {
        "type": "TX_RECEIPT",
        "payload": {
            //以下格式同上述`查詢交易紀錄`的回傳
            "list": [
                {
                    "id": "HO8tSTYvhEeeuWgl20DmTg",
                    "datetime": "2018-09-09T05:44:25.537037Z",
                    "currency": "CNY",  //此筆紀錄所屬幣別
                    "message": "test",
                    "statusCode": 0,    //0表示這筆交易已成功完成
                    "statusMsg": "",
                    "txType": 4,        //此筆紀錄為換匯交易
                    "txParams": { //交易發起時的參數
                        "toCurrency": "USD",    //轉換幣別(只有在txType為4時才會出現)
                        "sender": "tempid-test-jim-123-3",  //付款人
                        "receiver": "tempid-test-jim-123-3",//收款人
                        "currency": "CNY",      //交易幣別
                        "amount": 10            //交易金額
                    },
                    "txResult": { //交易成功完成造成的金流
                        "outflow": true,    //出帳
                        "amount": 10,       //金額為10
                        "balance": 860
                    }
                }
            ]
        }
    }
}
```

# 存錢

```js
//送
{
    "jsonrpc": "2.0",
    "method": "deposit",
    "params": {
        "currency": "USD",
        "amount": "1000"
    }
    "error": null
    "id": 99,
}
//收
{
    "jsonrpc": "2.0",
    "result": {
        "receiptId": "....",
        "statusCode": 0
    },
    "error": null,
    "id": 99,
}
```

# 提錢

```js
//送
{
    "jsonrpc": "2.0",
    "method": "withdraw",
    "params": {
        "currency": "USD",
        "amount": "1000"
    }
    "error": null
    "id": 99,
}
//收
{
    "jsonrpc": "2.0",
    "result": {
        "receiptId": "....",
        "statusCode": 0
    },
    "error": null,
    "id": 99,
}
```

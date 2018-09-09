
# 自訂錯誤

> 尚未定義細部的錯誤 待有需求時再行定義

錯誤碼 | 訊息 | 描述
--------- | ------------- | ---
-32600 | Unauthorized | 身份驗證錯誤
-1001 | SMS resend limit Exceeded | 簡訊驗證碼超過重送次數
-1002 | Action failed | 動作(操作)失敗
-1003 | Passcode expired | 驗證碼已過時效
-1004 | Passcode verify limit Exceeded | 驗證碼超過驗證次數

# 標準錯誤

錯誤碼 | 訊息 | 描述
--------- | ------------- | ---
-32700 | Parse error - 語法解析錯誤 | 伺服端接收到無效的 JSON。該錯誤傳送於伺服器嘗試解析 JSON 文字
-32600 | Invalid Request - 無效請求 | 傳送的 JSON 內容不是一個有效的請求物件。
-32601 | Method not found - 找不到方法 | 該方法不存在或無效。
-32602 | Invalid params - 無效的參數 | 無效的方法參數。
-32603 | Internal error - 內部錯誤 | JSON-RPC 內部錯誤。
-32000 to -32099 | Server error - 伺服端錯誤 | 預留用於自訂的伺服器錯誤。

# 交易類型 (txType)
碼 | 描述
--- | ---
1 | 存款
2 | 提款
3 | 轉帳
4 | 換匯

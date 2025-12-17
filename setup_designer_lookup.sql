-- 設定 Designer 欄位的 OCX Lookup 到 CURdUsers 表格
-- 這個腳本會設定 Designer 欄位從 CURdUsers 表格中查詢 UserName

-- 1. 更新 CURdTableField 表格，設定 lookup 目標表格和結果欄位
UPDATE CURdTableField
SET
    OCXLKTableName = 'CURdUsers',      -- lookup 的目標表格
    OCXLKResultName = 'UserName'        -- 要顯示的欄位
WHERE
    TableName = 'EMOdProdInfo'
    AND FieldName = 'Designer';

-- 2. 檢查 CURdOCXTableFieldLK 中是否已存在設定
IF NOT EXISTS (
    SELECT 1 FROM CURdOCXTableFieldLK
    WHERE TableName = 'EMOdProdInfo' AND FieldName = 'Designer'
)
BEGIN
    -- 如果不存在，插入新設定
    INSERT INTO CURdOCXTableFieldLK (TableName, FieldName, KeyFieldName, KeySelfName)
    VALUES ('EMOdProdInfo', 'Designer', 'UserId', 'Designer');
END
ELSE
BEGIN
    -- 如果已存在，更新設定
    UPDATE CURdOCXTableFieldLK
    SET
        KeyFieldName = 'UserId',        -- CURdUsers 表格中的 key 欄位
        KeySelfName = 'Designer'        -- EMOdProdInfo 表格中存放 UserId 的欄位
    WHERE
        TableName = 'EMOdProdInfo'
        AND FieldName = 'Designer';
END

-- 3. 驗證設定
SELECT
    tf.TableName,
    tf.FieldName,
    tf.OCXLKTableName,
    tf.OCXLKResultName,
    lk.KeyFieldName,
    lk.KeySelfName
FROM CURdTableField tf
LEFT JOIN CURdOCXTableFieldLK lk
    ON tf.TableName = lk.TableName AND tf.FieldName = lk.FieldName
WHERE
    tf.TableName = 'EMOdProdInfo'
    AND tf.FieldName = 'Designer';

-- 4. 測試 lookup 查詢（顯示前 10 筆使用者資料）
SELECT TOP 10 UserId, UserName
FROM CURdUsers
ORDER BY UserId;

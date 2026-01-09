-- ========================================
-- EMOdProdInfo 頁面 Lookup 設定範例
-- ========================================
-- 此檔案展示如何為 EMOdProdInfo 相關表格配置 Lookup 功能
-- 配置後，頁面會自動從 SQL 辭典讀取並套用 Lookup

-- ========================================
-- 範例 1：一般 Lookup（實體欄位）
-- ========================================
-- 將代碼欄位轉換為名稱顯示
-- 例如：將 ProcessCode（製程碼）轉換為製程名稱

-- 查看現有配置
SELECT FieldName, LookupTable, LookupKeyField, LookupResultField
FROM CURdTableField
WHERE TableName = 'EMOdProdInfo'
  AND FieldName IN ('ProcessCode', 'Designer', 'ProductUsage');

-- 配置 ProcessCode 的 Lookup（假設有 CURmProcess 表）
UPDATE CURdTableField
SET LookupTable = 'CURmProcess',           -- Lookup 來源表
    LookupKeyField = 'ProcessCode',        -- 來源表的鍵值欄位
    LookupResultField = 'ProcessName'      -- 要顯示的欄位
WHERE TableName = 'EMOdProdInfo'
  AND FieldName = 'ProcessCode';

-- 配置 Designer 的 Lookup（從員工表查詢姓名）
UPDATE CURdTableField
SET LookupTable = 'CURmUser',              -- 員工資料表
    LookupKeyField = 'UserId',             -- 使用者ID
    LookupResultField = 'UserName'         -- 使用者姓名
WHERE TableName = 'EMOdProdInfo'
  AND FieldName = 'Designer';

-- ========================================
-- 範例 2：OCX Lookup（虛擬欄位/非實體欄位）
-- ========================================
-- 用於顯示非資料表實體欄位的資料
-- 例如：想顯示客戶名稱，但資料表只有客戶代碼

-- 在辭典中新增虛擬欄位（如果不存在）
IF NOT EXISTS (
    SELECT 1 FROM CURdTableField
    WHERE TableName = 'EMOdProdInfo'
      AND FieldName = 'CustomerName'
)
BEGIN
    INSERT INTO CURdTableField (
        TableName, FieldName, DisplayLabel,
        SerialNum, Visible, ReadOnly,
        OCXLKTableName, KeyFieldName, KeySelfName, OCXLKResultName
    )
    VALUES (
        'EMOdProdInfo',          -- 主表名稱
        'CustomerName',          -- 虛擬欄位名稱
        '客戶名稱',              -- 顯示標籤
        15,                      -- 排序序號
        1,                       -- 可見
        1,                       -- 唯讀
        'CURmCust',              -- OCX Lookup 來源表
        'CustNo',                -- 來源表的鍵值欄位
        'CustomerSname',         -- 主表的實體欄位（客戶代碼）
        'CustName'               -- 要顯示的欄位（客戶名稱）
    );
END;

-- ========================================
-- 範例 3：查詢所有已配置 Lookup 的欄位
-- ========================================
-- 查看 EMOdProdInfo 相關表格的 Lookup 配置

-- 一般 Lookup
SELECT
    TableName,
    FieldName,
    DisplayLabel,
    LookupTable,
    LookupKeyField,
    LookupResultField,
    '一般Lookup' AS LookupType
FROM CURdTableField
WHERE TableName IN (
    'EMOdProdInfo', 'EMOdLayerPress', 'EMOdProdTier', 'EMOdLayerRoute',
    'EMOdProdPOP', 'EMOdProdMills', 'EMOdProdMixedDtl', 'EMOdProdLog',
    'EMOdNotesLog', 'EMOdProdECNLog', 'EMOdPartMerge', 'EMOdProdLayer'
)
AND LookupTable IS NOT NULL
AND LookupTable <> ''
ORDER BY TableName, SerialNum;

-- OCX Lookup
SELECT
    TableName,
    FieldName,
    DisplayLabel,
    OCXLKTableName AS LookupTable,
    KeyFieldName,
    KeySelfName,
    OCXLKResultName,
    'OCX Lookup' AS LookupType
FROM CURdTableField
WHERE TableName IN (
    'EMOdProdInfo', 'EMOdLayerPress', 'EMOdProdTier', 'EMOdLayerRoute',
    'EMOdProdPOP', 'EMOdProdMills', 'EMOdProdMixedDtl', 'EMOdProdLog',
    'EMOdNotesLog', 'EMOdProdECNLog', 'EMOdPartMerge', 'EMOdProdLayer'
)
AND OCXLKTableName IS NOT NULL
AND OCXLKTableName <> ''
ORDER BY TableName, SerialNum;

-- ========================================
-- 範例 4：EMOdLayerPress（壓合明細）Lookup 配置
-- ========================================
-- 配置材料代碼的 Lookup

UPDATE CURdTableField
SET LookupTable = 'CURmMaterial',
    LookupKeyField = 'MaterialCode',
    LookupResultField = 'MaterialName'
WHERE TableName = 'EMOdLayerPress'
  AND FieldName = 'MaterialCode';

-- ========================================
-- 範例 5：EMOdProdLog（歷史記錄）Lookup 配置
-- ========================================
-- 這個已經在頁面中實作，但可以改用辭典配置

-- 作業類型（IOType）
UPDATE CURdTableField
SET LookupTable = 'CURmIOType',            -- 假設有作業類型對照表
    LookupKeyField = 'IOTypeCode',
    LookupResultField = 'IOTypeName'
WHERE TableName = 'EMOdProdLog'
  AND FieldName = 'IOType';

-- 異動狀態（AftStatus）
UPDATE CURdTableField
SET LookupTable = 'CURmStatus',            -- 假設有狀態對照表
    LookupKeyField = 'StatusCode',
    LookupResultField = 'StatusName'
WHERE TableName = 'EMOdProdLog'
  AND FieldName = 'AftStatus';

-- ========================================
-- 範例 6：檢查 Lookup API 是否正常運作
-- ========================================
-- 測試 Lookup API（在瀏覽器或 Postman 中測試）
-- GET /api/TableFieldLayout/LookupData?table=CURmCust&key=CustNo&result=CustName
--
-- 回傳格式應為：
-- [
--   { "key": "C001", "result0": "客戶A" },
--   { "key": "C002", "result0": "客戶B" }
-- ]

-- ========================================
-- 範例 7：批次配置多個欄位
-- ========================================
-- 針對常見的代碼欄位批次設定 Lookup

-- 使用者ID -> 使用者姓名
UPDATE CURdTableField
SET LookupTable = 'CURmUser',
    LookupKeyField = 'UserId',
    LookupResultField = 'UserName'
WHERE FieldName = 'UserId'
  AND TableName LIKE 'EMOd%'
  AND (LookupTable IS NULL OR LookupTable = '');

-- 料號 -> 品名（假設需要顯示品名）
UPDATE CURdTableField
SET OCXLKTableName = 'CURmPart',
    KeyFieldName = 'PartNum',
    KeySelfName = 'PartNum',
    OCXLKResultName = 'PartName'
WHERE FieldName = 'PartNumDisplay'        -- 虛擬欄位
  AND TableName LIKE 'EMOd%';

-- ========================================
-- 範例 8：多欄位結果 Lookup
-- ========================================
-- Lookup API 支援回傳多個欄位（用逗號分隔）
-- 例如：result=PartName,PartSpec 會回傳 result0 和 result1

UPDATE CURdTableField
SET LookupTable = 'CURmPart',
    LookupKeyField = 'PartNum',
    LookupResultField = 'PartName,PartSpec'  -- 多個欄位用逗號分隔
WHERE TableName = 'EMOdProdInfo'
  AND FieldName = 'PartNum';

-- 前端會收到：
-- [
--   { "key": "P001", "result0": "產品A", "result1": "規格A" }
-- ]
-- 目前前端只使用 result0，如需顯示多個欄位，需要修改前端程式碼

-- ========================================
-- 使用說明
-- ========================================
/*
1. 一般 Lookup（實體欄位）：
   - LookupTable：要查詢的對照表
   - LookupKeyField：對照表的鍵值欄位
   - LookupResultField：要顯示的結果欄位
   - 適用於：資料表中有實際欄位，需要轉換為名稱顯示

2. OCX Lookup（虛擬欄位）：
   - OCXLKTableName：要查詢的對照表
   - KeyFieldName：對照表的鍵值欄位
   - KeySelfName：主表中的實際欄位（用來取值）
   - OCXLKResultName：要顯示的結果欄位
   - 適用於：資料表中沒有此欄位，但想顯示關聯資料

3. 優先順序：
   - 前端會優先使用 OCX Lookup
   - 如果沒有 OCX Lookup，才使用一般 Lookup
   - 這樣可以讓虛擬欄位覆蓋實體欄位的顯示

4. 快取機制：
   - Lookup 資料會在前端快取
   - 相同的 table+key+result 組合只會載入一次
   - 頁面重新整理後快取會清空

5. 除錯：
   - 打開瀏覽器開發者工具的 Console 頁籤
   - 會顯示每個頁籤載入的 Lookup 資訊
   - 格式：[tabKey] FieldName: 一般 Lookup 已載入 (N 筆)
*/

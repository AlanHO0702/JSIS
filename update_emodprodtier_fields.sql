-- ====================================================================
-- 更新 EMOdProdInfo 相关表的字段显示设置
-- 1. 将英文字段名改为中文
-- 2. 隐藏主键字段 (PartNum, Revision, LayerId)
-- 3. 确保 Visible 设置正确
-- ====================================================================

USE PCBERP;
GO

PRINT '===== 更新 EMOdProdTier (压合方式) ====='

-- 根据旧系统设置，只显示 FuncId、CuThick、Notes 三个字段
-- 其他字段全部隐藏

-- 隐藏主键和序号字段
UPDATE TableFieldLayout
SET DisplayLabel = '料號', Visible = 0
WHERE TableName = 'EMOdProdTier' AND FieldName = 'PartNum';

UPDATE TableFieldLayout
SET DisplayLabel = '版次', Visible = 0
WHERE TableName = 'EMOdProdTier' AND FieldName = 'Revision';

UPDATE TableFieldLayout
SET DisplayLabel = '序號', Visible = 0
WHERE TableName = 'EMOdProdTier' AND FieldName = 'SerialNum';

UPDATE TableFieldLayout
SET DisplayLabel = '流水號', Visible = 0
WHERE TableName = 'EMOdProdTier' AND FieldName = 'Serial';

UPDATE TableFieldLayout
SET DisplayLabel = '層別', Visible = 0
WHERE TableName = 'EMOdProdTier' AND FieldName = 'TierId';

UPDATE TableFieldLayout
SET Visible = 0
WHERE TableName = 'EMOdProdTier' AND FieldName = 'LayerId';

-- 确保这三个字段显示（应该已经是 1，这里确认一下）
UPDATE TableFieldLayout
SET Visible = 1
WHERE TableName = 'EMOdProdTier' AND FieldName IN ('FuncId', 'CuThick', 'Notes');

PRINT '===== 更新 EMOdLayerPress (压合明细) ====='

-- 隐藏主键字段
UPDATE TableFieldLayout
SET Visible = 0
WHERE TableName = 'EMOdLayerPress' AND FieldName IN ('PartNum', 'Revision', 'LayerId');

PRINT '===== 更新 EMOdLayerRoute (途程内容) ====='

-- 隐藏主键字段
UPDATE TableFieldLayout
SET Visible = 0
WHERE TableName = 'EMOdLayerRoute' AND FieldName IN ('PartNum', 'Revision', 'LayerId');

PRINT ''
PRINT '===== 更新完成，查看结果 ====='
PRINT ''

-- 查看 EMOdProdTier 更新结果
PRINT '--- EMOdProdTier ---'
SELECT FieldName, DisplayLabel, Visible, SerialNum
FROM TableFieldLayout
WHERE TableName = 'EMOdProdTier'
ORDER BY SerialNum;

-- 查看 EMOdLayerPress 更新结果
PRINT ''
PRINT '--- EMOdLayerPress ---'
SELECT FieldName, DisplayLabel, Visible, SerialNum
FROM TableFieldLayout
WHERE TableName = 'EMOdLayerPress'
ORDER BY SerialNum;

-- 查看 EMOdLayerRoute 更新结果
PRINT ''
PRINT '--- EMOdLayerRoute ---'
SELECT FieldName, DisplayLabel, Visible, SerialNum
FROM TableFieldLayout
WHERE TableName = 'EMOdLayerRoute'
ORDER BY SerialNum;

GO

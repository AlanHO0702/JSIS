inherited frmFQCdFMEOutMain: TfrmFQCdFMEOutMain
  Caption = 'frmFQCdFMEOutMain'
  ClientHeight = 637
  ExplicitTop = -72
  ExplicitHeight = 664
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlInfo: TPanel
    Top = 613
    ExplicitTop = 613
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 597
    ExplicitTop = 597
  end
  inherited pgeBwsDtl: TPageControl
    Height = 554
    ExplicitHeight = 554
    inherited tabBrowse: TTabSheet
      ExplicitLeft = 4
      ExplicitTop = 6
      ExplicitWidth = 1020
      ExplicitHeight = 544
      inherited gridBrowse: TJSdDBGrid
        Height = 544
        ExplicitHeight = 544
      end
    end
    inherited tabDetail: TTabSheet
      ExplicitLeft = 4
      ExplicitTop = 6
      ExplicitWidth = 1020
      ExplicitHeight = 544
      inherited pgeMaster: TPageControl
        inherited tbshtMaster1: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
        inherited tbshtMaster2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
        inherited tbshtMaster3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
        inherited tbshtMaster4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
      end
      inherited pgeDetail: TPageControl
        Height = 334
        ExplicitHeight = 334
        inherited tbshtDetail1: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 302
          object Splitter2: TSplitter [0]
            Left = 0
            Top = 100
            Width = 1012
            Height = 4
            Cursor = crVSplit
            Align = alTop
            Color = clMedGray
            ParentColor = False
            ExplicitTop = 81
            ExplicitWidth = 977
          end
          inherited gridDetail1: TJSdDBGrid
            Height = 100
            Align = alTop
            ExplicitHeight = 100
          end
          object Page3: TPageControl
            Left = 0
            Top = 104
            Width = 1012
            Height = 198
            ActivePage = TabSheet2
            Align = alClient
            Style = tsFlatButtons
            TabOrder = 1
            object TabSheet1: TTabSheet
              Caption = 'QC'#32080#26524
              object JSdDBGrid1: TJSdDBGrid
                Left = 0
                Top = 0
                Width = 1004
                Height = 166
                IniAttributes.Delimiter = ';;'
                IniAttributes.UnicodeIniFile = False
                TitleColor = clBtnFace
                FixedCols = 0
                ShowHorzScrollBar = True
                Align = alClient
                DataSource = dsFQCdFMEOutResult
                Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgWordWrap]
                ParentShowHint = False
                ShowHint = True
                TabOrder = 0
                TitleAlignment = taLeftJustify
                TitleFont.Charset = ANSI_CHARSET
                TitleFont.Color = clWindowText
                TitleFont.Height = -13
                TitleFont.Name = #32048#26126#39636
                TitleFont.Style = []
                TitleLines = 1
                TitleButtons = True
                OnEnter = JSdDBGrid1Enter
                OnMouseDown = JSdDBGrid1MouseDown
                SortColumnClick = stColumnClick
              end
            end
            object TabSheet2: TTabSheet
              Caption = 'QC'#38917#30446
              ImageIndex = 1
              object Splitter3: TSplitter
                Left = 0
                Top = 97
                Width = 1004
                Height = 4
                Cursor = crVSplit
                Align = alTop
                Color = clMedGray
                ParentColor = False
                ExplicitTop = 91
              end
              object qryQCTypeSubDtl: TJSdDBGrid
                Left = 0
                Top = 101
                Width = 1004
                Height = 65
                IniAttributes.Delimiter = ';;'
                IniAttributes.UnicodeIniFile = False
                TitleColor = clBtnFace
                FixedCols = 0
                ShowHorzScrollBar = True
                Align = alClient
                DataSource = dsFQCdQCTypeSubDtl
                Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
                ParentShowHint = False
                ShowHint = True
                TabOrder = 0
                TitleAlignment = taLeftJustify
                TitleFont.Charset = ANSI_CHARSET
                TitleFont.Color = clWindowText
                TitleFont.Height = -13
                TitleFont.Name = #32048#26126#39636
                TitleFont.Style = []
                TitleLines = 1
                TitleButtons = True
                OnEnter = gridDetail1Enter
                OnMouseDown = JSdDBGrid1MouseDown
                SortColumnClick = stColumnClick
              end
              object gridFQCdFMEOutItem: TJSdDBGrid
                Left = 0
                Top = 0
                Width = 1004
                Height = 97
                IniAttributes.Delimiter = ';;'
                IniAttributes.UnicodeIniFile = False
                TitleColor = clBtnFace
                FixedCols = 0
                ShowHorzScrollBar = True
                Align = alTop
                DataSource = dsFQCdFMEOutItem
                Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
                ParentShowHint = False
                ShowHint = True
                TabOrder = 1
                TitleAlignment = taLeftJustify
                TitleFont.Charset = ANSI_CHARSET
                TitleFont.Color = clWindowText
                TitleFont.Height = -13
                TitleFont.Name = #32048#26126#39636
                TitleFont.Style = []
                TitleLines = 1
                TitleButtons = True
                OnEnter = gridFQCdFMEOutItemEnter
                OnMouseDown = gridFQCdFMEOutItemMouseDown
                SortColumnClick = stColumnClick
              end
            end
            object TabSheet3: TTabSheet
              Caption = #26009#34399#23660#24615
              ImageIndex = 2
              object gridMatInfoDtl: TJSdDBGrid
                Left = 0
                Top = 0
                Width = 1004
                Height = 166
                IniAttributes.Delimiter = ';;'
                IniAttributes.UnicodeIniFile = False
                TitleColor = clBtnFace
                FixedCols = 0
                ShowHorzScrollBar = True
                Align = alClient
                DataSource = dsMatInfoDtl
                Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
                ParentShowHint = False
                ShowHint = True
                TabOrder = 0
                TitleAlignment = taLeftJustify
                TitleFont.Charset = ANSI_CHARSET
                TitleFont.Color = clWindowText
                TitleFont.Height = -13
                TitleFont.Name = #32048#26126#39636
                TitleFont.Style = []
                TitleLines = 1
                TitleButtons = True
                OnMouseDown = gridFQCdFMEOutItemMouseDown
                SortColumnClick = stColumnClick
              end
            end
          end
        end
        inherited tbshtDetail2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 302
          inherited gridDetail2: TJSdDBGrid
            Height = 302
            ExplicitHeight = 302
          end
        end
        inherited tbshtDetail3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 302
          inherited gridDetail3: TJSdDBGrid
            Height = 302
            ExplicitHeight = 302
          end
        end
        inherited tbshtDetail4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 302
          inherited gridDetail4: TJSdDBGrid
            Height = 302
            ExplicitHeight = 302
          end
        end
        inherited tbshtDetail5: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 302
          inherited gridDetail5: TJSdDBGrid
            Height = 302
            ExplicitHeight = 302
          end
        end
        inherited tbshtDetail6: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 302
          inherited gridDetail6: TJSdDBGrid
            Height = 302
            ExplicitHeight = 302
          end
        end
        inherited tbshtDetail7: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 302
          inherited gridDetail7: TJSdDBGrid
            Height = 302
            ExplicitHeight = 302
          end
        end
        inherited tbshtDetail8: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 302
          inherited gridDetail8: TJSdDBGrid
            Height = 302
            ExplicitHeight = 302
          end
        end
      end
      inherited pnlTempBasDLLBottom: TPanel
        Top = 517
        OnDblClick = pnlTempBasDLLBottomDblClick
        ExplicitTop = 517
        inherited btnC7: TSpeedButton
          ExplicitLeft = 594
          ExplicitTop = -2
        end
        inherited btnC8: TSpeedButton
          ExplicitLeft = 685
        end
        object btnCopySpec: TSpeedButton
          Left = 765
          Top = 0
          Width = 85
          Height = 27
          Align = alLeft
          NumGlyphs = 2
          Visible = False
          OnClick = btnCopySpecClick
          ExplicitLeft = 852
          ExplicitTop = -2
          ExplicitHeight = 26
        end
        object btnEditResu: TSpeedButton
          Left = 850
          Top = 0
          Width = 85
          Height = 27
          Align = alLeft
          NumGlyphs = 2
          Visible = False
          OnClick = btnEditResuClick
          ExplicitLeft = 923
          ExplicitTop = 2
          ExplicitHeight = 26
        end
      end
    end
  end
  inherited qryExec: TADOQuery
    Left = 128
    Top = 128
  end
  inherited qryGetTranData: TADOQuery
    Left = 176
    Top = 144
  end
  inherited qryBrowse: TJSdTable
    Left = 486
    Top = 58
  end
  inherited qryDetail1: TJSdTable
    AfterOpen = qryDetail1AfterOpen
  end
  inherited dsBrowse: TDataSource
    Left = 490
    Top = 114
  end
  inherited qryDetail6: TJSdTable
    Top = 246
  end
  inherited dsDetail6: TDataSource
    Top = 294
  end
  inherited pmuPaperPaper: TJSdPopupMenu
    Left = 400
    Top = 104
  end
  inherited pwgSaveToExcel: TJSdGrid2Excel
    Left = 400
    Top = 59
  end
  object qryFQCdFMEOutItem: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    BeforeInsert = qryFQCdFMEOutItemBeforeInsert
    BeforeEdit = qryFQCdFMEOutItemBeforeEdit
    AfterPost = qryFQCdFMEOutItemAfterPost
    BeforeDelete = qryFQCdFMEOutItemBeforeDelete
    EnableBCD = False
    Parameters = <
      item
        Name = 'PaperNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'Item'
        Attributes = [paSigned]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'select * from FQCdFMEOutItem'
      'where PaperNum = :PaperNum'
      'and SubItem = :Item')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FQCdFMEOutItem'
    Left = 352
    Top = 384
    object qryFQCdFMEOutItemPaperNum: TStringField
      DisplayWidth = 16
      FieldName = 'PaperNum'
      Size = 16
    end
    object qryFQCdFMEOutItemSubItem: TIntegerField
      DisplayWidth = 10
      FieldName = 'SubItem'
    end
    object qryFQCdFMEOutItemQCTypeItem: TIntegerField
      DisplayWidth = 10
      FieldName = 'QCTypeItem'
    end
    object qryFQCdFMEOutItemTestItem: TStringField
      DisplayWidth = 64
      FieldName = 'TestItem'
      Size = 64
    end
    object qryFQCdFMEOutItemLk_TestItemName: TWideStringField
      FieldKind = fkLookup
      FieldName = 'Lk_TestItemName'
      LookupDataSet = qryFQCdTestItem
      LookupKeyFields = 'TestItem'
      LookupResultField = 'Comments'
      KeyFields = 'TestItem'
      Lookup = True
    end
    object qryFQCdFMEOutItemStdValue1: TWideStringField
      DisplayWidth = 256
      FieldName = 'StdValue1'
      Size = 256
    end
    object qryFQCdFMEOutItemGetValue1: TWideStringField
      DisplayWidth = 256
      FieldName = 'GetValue1'
      Size = 256
    end
    object qryFQCdFMEOutItemStdValue2: TWideStringField
      DisplayWidth = 256
      FieldName = 'StdValue2'
      Size = 256
    end
    object qryFQCdFMEOutItemGetValue2: TWideStringField
      DisplayWidth = 256
      FieldName = 'GetValue2'
      Size = 256
    end
    object qryFQCdFMEOutItemQCResult: TStringField
      DisplayWidth = 64
      FieldName = 'QCResult'
      Size = 64
    end
    object qryFQCdFMEOutItemQCCode: TStringField
      DisplayWidth = 12
      FieldName = 'QCCode'
      Size = 12
    end
    object qryFQCdFMEOutItemAQL: TStringField
      DisplayWidth = 64
      FieldName = 'AQL'
      Size = 64
    end
    object qryFQCdFMEOutItemLk_AQLName: TWideStringField
      DisplayWidth = 20
      FieldKind = fkLookup
      FieldName = 'Lk_AQLName'
      LookupDataSet = qryFQCdAQL
      LookupKeyFields = 'AQL'
      LookupResultField = 'AQLName'
      KeyFields = 'AQL'
      Lookup = True
    end
    object qryFQCdFMEOutItemNotes: TWideStringField
      DisplayWidth = 255
      FieldName = 'Notes'
      Size = 255
    end
    object qryFQCdFMEOutItemQCStatus: TIntegerField
      DisplayWidth = 10
      FieldName = 'QCStatus'
    end
    object qryFQCdFMEOutItemQCQnty: TFloatField
      DisplayWidth = 10
      FieldName = 'QCQnty'
    end
    object qryFQCdFMEOutItemSampleQnty: TFloatField
      DisplayWidth = 10
      FieldName = 'SampleQnty'
    end
    object qryFQCdFMEOutItemCheck1: TIntegerField
      DisplayWidth = 10
      FieldName = 'Check1'
    end
    object qryFQCdFMEOutItemCheck2: TIntegerField
      DisplayWidth = 10
      FieldName = 'Check2'
    end
    object qryFQCdFMEOutItemOperation1: TStringField
      DisplayWidth = 12
      FieldName = 'Operation1'
      Size = 12
    end
    object qryFQCdFMEOutItemOperation2: TStringField
      DisplayWidth = 12
      FieldName = 'Operation2'
      Size = 12
    end
    object qryFQCdFMEOutItemQCType: TStringField
      DisplayWidth = 64
      FieldName = 'QCType'
      Size = 64
    end
    object qryFQCdFMEOutItemAC: TFloatField
      FieldName = 'AC'
    end
    object qryFQCdFMEOutItemRE: TFloatField
      FieldName = 'RE'
    end
  end
  object dsFQCdFMEOutItem: TDataSource
    DataSet = qryFQCdFMEOutItem
    Left = 472
    Top = 384
  end
  object qryFQCdFMEOutResult: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    BeforeInsert = qryFQCdFMEOutResultBeforeInsert
    BeforeEdit = qryFQCdFMEOutResultBeforeEdit
    BeforePost = qryFQCdFMEOutResultBeforePost
    AfterPost = qryFQCdFMEOutResultAfterPost
    BeforeDelete = qryFQCdFMEOutResultBeforeDelete
    OnNewRecord = qryFQCdFMEOutResultNewRecord
    EnableBCD = False
    Parameters = <
      item
        Name = 'PaperNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'Item'
        Attributes = [paSigned]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'select * from FQCdFMEOutResult'
      'where PaperNum = :PaperNum'
      'and SubItem = :Item')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FQCdFMEOutResult'
    Left = 128
    Top = 440
    object qryFQCdFMEOutResultPaperNum: TStringField
      FieldName = 'PaperNum'
      Size = 16
    end
    object qryFQCdFMEOutResultSubItem: TIntegerField
      FieldName = 'SubItem'
    end
    object qryFQCdFMEOutResultItem: TIntegerField
      FieldName = 'Item'
    end
    object qryFQCdFMEOutResultQCReference: TStringField
      FieldName = 'QCReference'
      Size = 255
    end
    object qryFQCdFMEOutResultQCCode: TStringField
      FieldName = 'QCCode'
      OnValidate = qryFQCdFMEOutResultQCCodeValidate
      Size = 12
    end
    object qryFQCdFMEOutResultLk_QCCodeComments: TWideStringField
      FieldKind = fkLookup
      FieldName = 'Lk_QCCodeComments'
      LookupDataSet = qryFQCdQCCode
      LookupKeyFields = 'QCCode'
      LookupResultField = 'Comments'
      KeyFields = 'QCCode'
      Lookup = True
    end
    object qryFQCdFMEOutResultQCResult: TStringField
      FieldName = 'QCResult'
      Size = 64
    end
    object qryFQCdFMEOutResultLk_QCResultNotes: TWideStringField
      FieldKind = fkLookup
      FieldName = 'Lk_QCResultNotes'
      LookupDataSet = qryFQCdQCResult
      LookupKeyFields = 'QCResult'
      LookupResultField = 'Notes'
      KeyFields = 'QCResult'
      Lookup = True
    end
    object qryFQCdFMEOutResultQCStatus: TIntegerField
      FieldName = 'QCStatus'
    end
    object qryFQCdFMEOutResultLk_QCStatusName: TWideStringField
      FieldKind = fkLookup
      FieldName = 'Lk_QCStatusName'
      LookupDataSet = qryFQCdQCStatus
      LookupKeyFields = 'QCStatus'
      LookupResultField = 'QCStatusName'
      KeyFields = 'QCStatus'
      Size = 24
      Lookup = True
    end
    object qryFQCdFMEOutResultToStockId: TStringField
      FieldName = 'ToStockId'
      Size = 8
    end
    object qryFQCdFMEOutResultLk_ToStockName: TWideStringField
      FieldKind = fkLookup
      FieldName = 'Lk_ToStockName'
      LookupDataSet = qryMINdStockBasic
      LookupKeyFields = 'StockId'
      LookupResultField = 'StockName'
      KeyFields = 'ToStockId'
      Lookup = True
    end
    object qryFQCdFMEOutResultQCQnty: TFloatField
      FieldName = 'QCQnty'
    end
    object StringField9: TWideStringField
      FieldName = 'Notes'
      Size = 255
    end
    object qryFQCdFMEOutResultPosId: TStringField
      FieldName = 'PosId'
      Size = 12
    end
    object qryFQCdFMEOutResultUOMQnty: TFloatField
      FieldName = 'UOMQnty'
    end
    object qryFQCdFMEOutResultRatio: TFloatField
      FieldName = 'Ratio'
    end
    object qryFQCdFMEOutResultUnit: TStringField
      FieldName = 'Unit'
      Size = 4
    end
    object qryFQCdFMEOutResultUOM: TStringField
      FieldName = 'UOM'
      Size = 4
    end
    object qryFQCdFMEOutResultBatchNum: TStringField
      FieldName = 'BatchNum'
      Size = 24
    end
    object qryFQCdFMEOutResultVolumeNum: TStringField
      FieldName = 'VolumeNum'
      Size = 24
    end
    object qryFQCdFMEOutResultExpiredDate: TDateTimeField
      FieldName = 'ExpiredDate'
    end
    object qryFQCdFMEOutResultSizeLenth: TStringField
      DisplayWidth = 48
      FieldName = 'SizeLenth'
      Size = 48
    end
    object qryFQCdFMEOutResultSizeWidth: TStringField
      DisplayWidth = 48
      FieldName = 'SizeWidth'
      Size = 48
    end
    object qryFQCdFMEOutResultUseId: TStringField
      FieldName = 'UseId'
      Size = 8
    end
    object qryFQCdFMEOutResultQCType: TStringField
      FieldName = 'QCType'
      Size = 64
    end
  end
  object dsFQCdFMEOutResult: TDataSource
    DataSet = qryFQCdFMEOutResult
    Left = 240
    Top = 424
  end
  object qryFQCdQCStatus: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'select * from FQCdQCStatus(nolock)')
    Left = 288
    Top = 104
    object qryFQCdQCStatusQCStatus: TIntegerField
      FieldName = 'QCStatus'
    end
    object qryFQCdQCStatusQCStatusName: TWideStringField
      FieldName = 'QCStatusName'
      Size = 24
    end
  end
  object qryMINdStockBasic: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'select * from FMEV_MINdStockBasic(nolock)')
    Left = 288
    Top = 56
    object qryMINdStockBasicStockId: TStringField
      FieldName = 'StockId'
      FixedChar = True
      Size = 8
    end
    object qryMINdStockBasicStockName: TWideStringField
      FieldName = 'StockName'
      Size = 24
    end
    object qryMINdStockBasicStockType: TIntegerField
      FieldName = 'StockType'
    end
    object qryMINdStockBasicIsOut: TIntegerField
      FieldName = 'IsOut'
    end
    object qryMINdStockBasicIsMust: TIntegerField
      FieldName = 'IsMust'
    end
    object qryMINdStockBasicNotes: TWideStringField
      FieldName = 'Notes'
      Size = 255
    end
    object qryMINdStockBasicPOType: TIntegerField
      FieldName = 'POType'
    end
    object qryMINdStockBasicUseId: TStringField
      FieldName = 'UseId'
      Size = 16
    end
    object qryMINdStockBasicFromCus: TIntegerField
      FieldName = 'FromCus'
    end
  end
  object qryFQCdQCCode: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'select * from FQCdQCCode(nolock)')
    Left = 288
    Top = 200
    object qryFQCdQCCodeQCCode: TStringField
      FieldName = 'QCCode'
      Size = 12
    end
    object qryFQCdQCCodeComments: TWideStringField
      FieldName = 'Comments'
      Size = 255
    end
    object qryFQCdQCCodeQCStatus: TIntegerField
      FieldName = 'QCStatus'
    end
    object qryFQCdQCCodeStockId: TStringField
      FieldName = 'StockId'
      Size = 8
    end
  end
  object qryFQCdQCResult: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'select * from FQCdQCResult(nolock)')
    Left = 288
    Top = 152
    object qryFQCdQCResultQCResult: TStringField
      FieldName = 'QCResult'
      Size = 64
    end
    object qryFQCdQCResultNotes: TWideStringField
      FieldName = 'Notes'
      Size = 255
    end
  end
  object qryFQCdAQL: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'select * from FQCdAQL(nolock)')
    Left = 560
    Top = 136
    object qryFQCdAQLAQL: TStringField
      FieldName = 'AQL'
      Size = 8
    end
    object qryFQCdAQLAQLName: TWideStringField
      FieldName = 'AQLName'
      Size = 24
    end
  end
  object dsMatInfoDtl: TDataSource
    DataSet = qryMatInfoDtl
    Left = 456
    Top = 184
  end
  object qryMatInfoDtl: TJSdTable
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 384
    Top = 176
  end
  object qryFQCdQCTypeSubDtl: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <
      item
        Name = 'QCType'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 64
        Value = Null
      end
      item
        Name = 'QCTypeItem'
        Attributes = [paSigned]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'select * from FQCdQCTypeSubDtl'
      'where QCType = :QCType'
      'and Item = :QCTypeItem'
      'order by SerialNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'SerialNum'
    TableName = 'FQCdQCTypeSubDtl'
    Left = 384
    Top = 480
    object qryFQCdQCTypeSubDtlQCType: TStringField
      FieldName = 'QCType'
      Size = 64
    end
    object qryFQCdQCTypeSubDtlItem: TIntegerField
      FieldName = 'Item'
    end
    object qryFQCdQCTypeSubDtlSerialNum: TIntegerField
      FieldName = 'SerialNum'
    end
    object qryFQCdQCTypeSubDtlCheck1: TIntegerField
      FieldName = 'Check1'
    end
    object qryFQCdQCTypeSubDtlCheck2: TIntegerField
      FieldName = 'Check2'
    end
    object qryFQCdQCTypeSubDtlOperation1: TStringField
      FieldName = 'Operation1'
      Size = 12
    end
    object qryFQCdQCTypeSubDtlStdValue1: TStringField
      FieldName = 'StdValue1'
      Size = 256
    end
    object qryFQCdQCTypeSubDtlOperation2: TStringField
      FieldName = 'Operation2'
      Size = 12
    end
    object qryFQCdQCTypeSubDtlStdValue2: TStringField
      FieldName = 'StdValue2'
      Size = 256
    end
    object qryFQCdQCTypeSubDtlNOtes: TStringField
      FieldName = 'NOtes'
      Size = 255
    end
  end
  object dsFQCdQCTypeSubDtl: TDataSource
    DataSet = qryFQCdQCTypeSubDtl
    Left = 264
    Top = 480
  end
  object qryFQCdTestItem: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      'select TestItem,Comments from FQCdTestItem(nolock)')
    Left = 568
    Top = 392
  end
end

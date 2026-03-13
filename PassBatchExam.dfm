inherited frmPassBatchExam: TfrmPassBatchExam
  Caption = 'frmPassBatchExam'
  ClientHeight = 543
  ClientWidth = 975
  ExplicitWidth = 983
  ExplicitHeight = 570
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlInfo: TPanel
    Top = 521
    Width = 975
    Height = 22
    ExplicitTop = 521
    ExplicitWidth = 975
    ExplicitHeight = 22
    inherited btnGetParams: TSpeedButton
      Height = 22
    end
    inherited btnTempBasDLLDo: TButton
      Height = 22
      ExplicitHeight = 22
    end
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 499
    Width = 975
    ExplicitTop = 499
    ExplicitWidth = 975
    inherited pnlTempBasPeriod: TPanel
      inherited txtCurrPeriod: TPanel
        ExplicitLeft = 60
      end
    end
  end
  object pageMain: TPageControl [2]
    Left = 0
    Top = 0
    Width = 975
    Height = 543
    ActivePage = TabSheet1
    Align = alTop
    Style = tsButtons
    TabHeight = 1
    TabOrder = 2
    TabWidth = 1
    object TabSheet1: TTabSheet
      object gridSub: TJSdDBGrid
        Left = 0
        Top = 430
        Width = 967
        Height = 102
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsPassSub
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
        SortColumnClick = stColumnClick
      end
      object msProcSelect: TJSdMultSelect
        Left = 0
        Top = 87
        Width = 967
        Height = 343
        Align = alTop
        WidthSource = 585
        HeadingSource = #38928#36942#24115
        HeadingTarget = #27442#23529#26680#30340#36942#24115#21934
        ColorSource = clWindow
        ColorTarget = clWindow
        ToolAlign = taLeft
        SortTypeSource = stNone
        SortTypeTarget = stNone
        RowSelectSource = True
        RowSelectTarget = True
        SourceColumns = <
          item
            Caption = #35069#31243#20195#34399
            Width = 80
          end
          item
            Caption = #35069#31243#21517#31281
            Width = 100
          end
          item
            Caption = #21697#34399
            Width = 130
          end
          item
            Caption = #25209#34399
            Width = 150
          end
          item
            Caption = #25976#37327
            Width = 100
          end
          item
            Caption = #36942#24115#21934#21934#34399
            Width = 130
          end>
        TargetColumns = <>
        Distinct = False
        SelectMode = smMove
        DataSourceSource = dsPassGet
        OnSourceClick = msProcSelectSourceClick
        OnTargetClick = msProcSelectTargetClick
        AfterSelect = msProcSelectAfterSelect
        AfterRemove = msProcSelectAfterRemove
        SetupList = slSource
        ExplicitLeft = 80
      end
      object Panel1: TPanel
        Left = 0
        Top = 0
        Width = 967
        Height = 87
        Align = alTop
        BevelOuter = bvNone
        TabOrder = 2
        object JSdLabel1: TJSdLabel
          Left = 9
          Top = 5
          Width = 56
          Height = 13
          Caption = #36942#20837#35069#31243
        end
        object btFind: TSpeedButton
          Left = 345
          Top = 0
          Width = 60
          Height = 41
          Caption = #37325#21462
          Glyph.Data = {
            42010000424D4201000000000000760000002800000011000000110000000100
            040000000000CC00000000000000000000001000000010000000000000000000
            BF0000BF000000BFBF00BF000000BF00BF00BFBF0000C0C0C000808080000000
            FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00777777777777
            77777000000077777777777777777000000070000077777000007000000070B0
            00777770F0007000000070F000777770B0007000000070000000700000007000
            0000700B000000B0000070000000700F000700F0000070000000700B000700B0
            0000700000007700000000000007700000007770B00070B00077700000007770
            0000700000777000000077770007770007777000000077770B07770B07777000
            0000777700077700077770000000777777777777777770000000777777777777
            777770000000}
          Layout = blGlyphTop
          OnClick = btFindClick
        end
        object JSdLabel2: TJSdLabel
          Left = 9
          Top = 30
          Width = 28
          Height = 13
          Caption = #32218#21029
          Visible = False
        end
        object JSdLabel3: TJSdLabel
          Left = 9
          Top = 31
          Width = 70
          Height = 13
          Caption = #25209#34399'('#27169#31946')'
        end
        object btnRejectExam: TSpeedButton
          Left = 345
          Top = 46
          Width = 60
          Height = 41
          Caption = #36864#23529
          Glyph.Data = {
            76010000424D7601000000000000760000002800000020000000100000000100
            04000000000000010000130B0000130B00001000000000000000000000000000
            800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
            FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
            3333333333FFFFF3333333333999993333333333F77777FFF333333999999999
            3333333777333777FF3333993333339993333377FF3333377FF3399993333339
            993337777FF3333377F3393999333333993337F777FF333337FF993399933333
            399377F3777FF333377F993339993333399377F33777FF33377F993333999333
            399377F333777FF3377F993333399933399377F3333777FF377F993333339993
            399377FF3333777FF7733993333339993933373FF3333777F7F3399933333399
            99333773FF3333777733339993333339933333773FFFFFF77333333999999999
            3333333777333777333333333999993333333333377777333333}
          Layout = blGlyphTop
          NumGlyphs = 2
          OnClick = btnRejectExamClick
        end
        object btnExecute: TSpeedButton
          Left = 887
          Top = 40
          Width = 60
          Height = 41
          Caption = #23529#26680
          Glyph.Data = {
            36010000424D360100000000000076000000280000001E0000000C0000000100
            040000000000C000000000000000000000001000000000000000000000000000
            80000080000000808000800000008000800080800000C0C0C000808080000000
            FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00777778877777
            777777778877777777007777F00877777777777F7787777777007777F0008777
            7777777F7778777777007777F00008777777777F7777877777007777F0000087
            7777777F7777787777007777F00000087777777F7777778777007777F0000007
            7777777F7777787777007777F00000777777777F7777877777007777F0000777
            7777777F7778777777007777F00077777777777F7787777777007777F0077777
            7777777F7877777777007777FF7777777777777FF77777777700}
          Layout = blGlyphTop
          NumGlyphs = 2
          OnClick = btnExecuteClick
        end
        object cboProcCode: TJSdLookupCombo
          Left = 80
          Top = 1
          Width = 251
          Height = 25
          LkDataSource = dsUserProc
          LkColumnCount = 2
          cboColor = clWindow
          TextSize = 100
          Text = ''
          SelectOnly = False
          SortedOff = False
          TabOrder = 0
        end
        object cboLineId: TJSdLookupCombo
          Left = 80
          Top = 25
          Width = 251
          Height = 25
          LkDataSource = dsLineId
          LkColumnCount = 2
          cboColor = clWindow
          TextSize = 100
          Text = ''
          SelectOnly = False
          SortedOff = False
          Visible = False
          TabOrder = 1
        end
        object edtLotNum: TEdit
          Left = 80
          Top = 25
          Width = 251
          Height = 21
          TabOrder = 2
        end
      end
    end
  end
  inherited qryExec: TADOQuery
    Left = 688
    Top = 200
  end
  inherited qryGetTranData: TADOQuery
    Left = 800
    Top = 232
  end
  object qryUserProc: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    Parameters = <
      item
        Name = 'UserId'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdProcPrivateGet :UserId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdProcPrivateGet'
    Left = 360
    Top = 288
  end
  object qryPassSub: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    AfterOpen = qryPassSubAfterOpen
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassSubView :PaperNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdPassSubPaper'
    Left = 432
    Top = 288
  end
  object dsUserProc: TDataSource
    DataSet = qryUserProc
    Left = 336
    Top = 328
  end
  object dsPassSub: TDataSource
    DataSet = qryPassSub
    Left = 432
    Top = 336
  end
  object qryPassGet: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    Parameters = <
      item
        Name = 'UserId'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'ProcCode'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'LineId'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'LotNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassBatchExamGet :UserId, :ProcCode, :LineId, :LotNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 504
    Top = 288
  end
  object dsPassGet: TDataSource
    DataSet = qryPassGet
    Left = 504
    Top = 336
  end
  object qryExam: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    CommandTimeout = 480
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 20
        Value = Null
      end
      item
        Name = 'UserId'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec CURdPaperAction '
      #39'FMEdPassMain'#39','
      ':PaperNum,'
      ':UserId,'
      '1,'
      '1')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 576
    Top = 288
  end
  object qryLineId: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    Parameters = <>
    SQL.Strings = (
      'select LineId,LineName from FMEdLineBasic(nolock)')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdLineBasic'
    Left = 224
    Top = 176
  end
  object dsLineId: TDataSource
    DataSet = qryLineId
    Left = 208
    Top = 248
  end
end

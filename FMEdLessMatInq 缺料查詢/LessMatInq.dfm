inherited frmFMEdLessMatInq: TfrmFMEdLessMatInq
  Caption = 'frmFMEdLessMatInq'
  ClientHeight = 489
  ClientWidth = 1131
  ExplicitWidth = 1139
  ExplicitHeight = 516
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlInfo: TPanel
    Top = 469
    Width = 1131
    ExplicitTop = 469
    ExplicitWidth = 1131
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 447
    Width = 1131
    ExplicitTop = 447
    ExplicitWidth = 1131
  end
  object pnlMain: TPanel [2]
    Left = 0
    Top = 0
    Width = 1131
    Height = 481
    Align = alTop
    BevelInner = bvLowered
    TabOrder = 2
    object Splitter2: TSplitter
      Left = 2
      Top = 81
      Width = 1127
      Height = 4
      Cursor = crVSplit
      Align = alTop
      Color = clMedGray
      ParentColor = False
      ExplicitTop = 165
      ExplicitWidth = 973
    end
    object Panel1: TPanel
      Left = 2
      Top = 2
      Width = 1127
      Height = 79
      Align = alTop
      BevelOuter = bvNone
      TabOrder = 0
      object pnlCond: TPanel
        Left = 139
        Top = 0
        Width = 988
        Height = 79
        Align = alClient
        TabOrder = 0
        object btnInq: TSpeedButton
          Left = 745
          Top = 5
          Width = 45
          Height = 41
          Caption = #26597#35426
          Layout = blGlyphTop
          OnClick = btnInqClick
        end
        object btnMatDetail: TSpeedButton
          Left = 847
          Top = 5
          Width = 52
          Height = 32
          Caption = #19968#35261#34920
          Layout = blGlyphTop
          Visible = False
          OnClick = btnMatDetailClick
        end
        object btnToExcel: TSpeedButton
          Left = 796
          Top = 5
          Width = 45
          Height = 41
          ParentCustomHint = False
          BiDiMode = bdLeftToRight
          Caption = #21295#20986
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'Tahoma'
          Font.Style = []
          Glyph.Data = {
            76010000424D7601000000000000760000002800000020000000100000000100
            04000000000000010000130B0000130B00001000000000000000000000000000
            800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
            FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333303
            333333333333337FF3333333333333903333333333333377FF33333333333399
            03333FFFFFFFFF777FF3000000999999903377777777777777FF0FFFF0999999
            99037F3337777777777F0FFFF099999999907F3FF777777777770F00F0999999
            99037F773777777777730FFFF099999990337F3FF777777777330F00FFFFF099
            03337F773333377773330FFFFFFFF09033337F3FF3FFF77733330F00F0000003
            33337F773777777333330FFFF0FF033333337F3FF7F3733333330F08F0F03333
            33337F7737F7333333330FFFF003333333337FFFF77333333333000000333333
            3333777777333333333333333333333333333333333333333333}
          Layout = blGlyphTop
          NumGlyphs = 2
          ParentFont = False
          ParentShowHint = False
          ParentBiDiMode = False
          ShowHint = False
          OnClick = btnToExcelClick
        end
        object JSdLabel1: TJSdLabel
          Left = 22
          Top = 11
          Width = 42
          Height = 13
          Alignment = taRightJustify
          Caption = #36092#20837#20214
        end
        object JSdLabel2: TJSdLabel
          Left = 578
          Top = 33
          Width = 56
          Height = 13
          Alignment = taRightJustify
          Caption = #38656#27714#26085'<='
        end
        object JSdLabel3: TJSdLabel
          Left = 222
          Top = 33
          Width = 70
          Height = 13
          Alignment = taRightJustify
          Caption = #21697#21517'('#27169#31946')'
        end
        object JSdLabel4: TJSdLabel
          Left = 22
          Top = 33
          Width = 42
          Height = 13
          Alignment = taRightJustify
          Caption = #20027#20998#39006
        end
        object JSdLabel5: TJSdLabel
          Left = 222
          Top = 11
          Width = 70
          Height = 13
          Alignment = taRightJustify
          Caption = #26009#34399'('#27169#31946')'
        end
        object JSdLabel7: TJSdLabel
          Left = 578
          Top = 10
          Width = 56
          Height = 13
          Alignment = taRightJustify
          Caption = #38656#27714#26085'>='
        end
        object lblType: TJSdLabel
          Left = 8
          Top = 55
          Width = 56
          Height = 13
          Alignment = taRightJustify
          Caption = #32570#26009#26781#20214
        end
        object cbo_MatClass: TJSdLookupCombo
          Left = 64
          Top = 29
          Width = 150
          Height = 21
          LkColumnCount = 2
          cboColor = clWindow
          TextSize = 70
          Text = ''
          SelectOnly = False
          SortedOff = False
          TabOrder = 0
        end
        object cbo_MB: TJSdLookupCombo
          Left = 64
          Top = 7
          Width = 150
          Height = 21
          LkColumnCount = 2
          cboColor = clWindow
          TextSize = 70
          Text = ''
          SelectOnly = False
          SortedOff = False
          TabOrder = 1
          OnChange = cbo_MBChange
        end
        object cbo_PartNum: TJSdLookupCombo
          Left = 292
          Top = 7
          Width = 280
          Height = 21
          LkColumnCount = 2
          cboColor = clWindow
          TextSize = 140
          Text = ''
          SelectOnly = False
          SortedOff = False
          TabOrder = 2
        end
        object cboType: TJSdLookupCombo
          Left = 64
          Top = 51
          Width = 150
          Height = 21
          LkDataSource = dsType
          LkColumnCount = 2
          cboColor = clWindow
          TextSize = 70
          Text = ''
          SelectOnly = False
          SortedOff = False
          TabOrder = 3
        end
        object edtMatName: TEdit
          Left = 292
          Top = 29
          Width = 280
          Height = 21
          TabOrder = 4
        end
        object ww_InqDate: TwwDBDateTimePicker
          Left = 635
          Top = 7
          Width = 104
          Height = 21
          CalendarAttributes.Font.Charset = DEFAULT_CHARSET
          CalendarAttributes.Font.Color = clWindowText
          CalendarAttributes.Font.Height = -11
          CalendarAttributes.Font.Name = 'Tahoma'
          CalendarAttributes.Font.Style = []
          Epoch = 1950
          ShowButton = True
          TabOrder = 5
          DisplayFormat = 'yyyy/mm/dd'
        end
        object ww_InqDate2: TwwDBDateTimePicker
          Left = 635
          Top = 29
          Width = 104
          Height = 21
          CalendarAttributes.Font.Charset = DEFAULT_CHARSET
          CalendarAttributes.Font.Color = clWindowText
          CalendarAttributes.Font.Height = -11
          CalendarAttributes.Font.Name = 'Tahoma'
          CalendarAttributes.Font.Style = []
          Epoch = 1950
          ShowButton = True
          TabOrder = 6
          DisplayFormat = 'yyyy/mm/dd'
        end
      end
      object pnlCount: TPanel
        Left = 0
        Top = 0
        Width = 139
        Height = 79
        Align = alLeft
        Caption = '0 / 0'
        TabOrder = 1
      end
    end
    object pageBrowse: TPageControl
      Left = 2
      Top = 180
      Width = 1127
      Height = 156
      ActivePage = TabSheet1
      Align = alBottom
      TabOrder = 1
      Visible = False
      OnChange = pageBrowseChange
      object TabSheet1: TTabSheet
        Caption = #20027#20214
      end
      object TabSheet2: TTabSheet
        Caption = #26367#20195#26009
        ImageIndex = 1
        object Splitter3: TSplitter
          Left = 320
          Top = 0
          Width = 4
          Height = 127
          Color = clMedGray
          ParentColor = False
          ExplicitLeft = 416
          ExplicitTop = 3
          ExplicitHeight = 128
        end
        object grid_Browse2: TJSdDBGrid
          Left = 0
          Top = 0
          Width = 320
          Height = 127
          IniAttributes.Delimiter = ';;'
          IniAttributes.UnicodeIniFile = False
          TitleColor = clBtnFace
          FixedCols = 0
          ShowHorzScrollBar = True
          Align = alLeft
          DataSource = dsBrowse
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
        object JSdDBGrid1: TJSdDBGrid
          Left = 324
          Top = 0
          Width = 795
          Height = 127
          IniAttributes.Delimiter = ';;'
          IniAttributes.UnicodeIniFile = False
          TitleColor = clBtnFace
          FixedCols = 0
          ShowHorzScrollBar = True
          Align = alClient
          DataSource = dsDisplace
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
          SortColumnClick = stColumnClick
        end
      end
    end
    object pnlInqMatLessShowDLL: TPanel
      Left = 2
      Top = 336
      Width = 1127
      Height = 143
      Align = alBottom
      TabOrder = 2
      Visible = False
    end
    object grid_Browse: TJSdDBGrid
      Left = 2
      Top = 85
      Width = 1127
      Height = 95
      IniAttributes.Delimiter = ';;'
      IniAttributes.UnicodeIniFile = False
      TitleColor = clBtnFace
      FixedCols = 0
      ShowHorzScrollBar = True
      Align = alClient
      DataSource = dsBrowse
      Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
      ParentShowHint = False
      ShowHint = True
      TabOrder = 3
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
  end
  inherited qryExec: TADOQuery
    Left = 352
    Top = 352
  end
  inherited qryGetTranData: TADOQuery
    Left = 352
    Top = 304
  end
  object qryBrowse: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    AfterOpen = qryBrowseAfterOpen
    BeforeClose = qryBrowseBeforeClose
    AfterClose = qryBrowseAfterClose
    AfterScroll = qryBrowseAfterScroll
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 360
    Top = 96
  end
  object dsBrowse: TDataSource
    DataSet = qryBrowse
    Left = 432
    Top = 128
  end
  object qryMINdMatInfo: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      'select PartNum,MatName from MINdMatInfo(nolock)')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 520
    Top = 80
    object qryMINdMatInfoPartNum: TStringField
      FieldName = 'PartNum'
      Size = 24
    end
    object qryMINdMatInfoMatName: TWideStringField
      FieldName = 'MatName'
      Size = 120
    end
  end
  object dsMINdMatInfo: TDataSource
    DataSet = qryMINdMatInfo
    Left = 528
    Top = 128
  end
  object qryMINdMatClass: TJSdTable
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      
        'select MatClass,ClassName from MINdMatClass(nolock) order by Mat' +
        'Class')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 648
    Top = 88
  end
  object dsMINdMatClass: TDataSource
    DataSet = qryMINdMatClass
    Left = 656
    Top = 136
  end
  object qryMB: TJSdTable
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      
        'select Item=0,ItemName='#39#21542#39' union select Item=1,ItemName='#39#26159#39' unio' +
        'n select Item=255,ItemName='#39#19981#38480#39' ')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 768
    Top = 80
  end
  object dsMB: TDataSource
    DataSet = qryMB
    Left = 768
    Top = 128
  end
  object qryDisplace: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    BeforeOpen = qryDisplaceBeforeOpen
    AfterScroll = qryDisplaceAfterScroll
    EnableBCD = False
    Parameters = <
      item
        Name = 'PartNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'select * from FMEdV_LessMatDisplace(nolock)'
      'where DisplaceMat = :PartNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdV_LessMatDisplace'
    Left = 640
    Top = 248
    object qryDisplaceMatGroup: TStringField
      FieldName = 'MatGroup'
      Size = 24
    end
    object qryDisplacesKind: TStringField
      FieldName = 'sKind'
      ReadOnly = True
      Size = 4
    end
    object qryDisplaceMatCode: TStringField
      FieldName = 'MatCode'
      Size = 24
    end
    object qryDisplaceLk_MatName: TWideStringField
      FieldKind = fkLookup
      FieldName = 'Lk_MatName'
      LookupDataSet = qryMINdMatInfo
      LookupKeyFields = 'PartNum'
      LookupResultField = 'MatName'
      KeyFields = 'MatCode'
      Lookup = True
    end
    object qryDisplaceDisplaceMat: TStringField
      FieldName = 'DisplaceMat'
      Size = 24
    end
  end
  object dsDisplace: TDataSource
    DataSet = qryDisplace
    Left = 640
    Top = 304
  end
  object qryType: TJSdTable
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      'select Item=0, ItemName='#39#21482#31168#32570#26009#39
      'union select 1,'#39#21482#31168#36229#38989#20379#32102#39
      'union select 2,'#39#19981#38480#39)
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 864
    Top = 120
  end
  object dsType: TDataSource
    DataSet = qryType
    Left = 872
    Top = 168
  end
end

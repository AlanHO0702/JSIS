inherited frmFMEdLessMatInq2: TfrmFMEdLessMatInq2
  Caption = 'frmFMEdLessMatInq2'
  ClientHeight = 489
  ClientWidth = 1138
  Position = poDesigned
  ExplicitWidth = 1146
  ExplicitHeight = 516
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlInfo: TPanel
    Top = 469
    Width = 1138
    ExplicitTop = 469
    ExplicitWidth = 1138
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 447
    Width = 1138
    ExplicitTop = 447
    ExplicitWidth = 1138
  end
  object pnlMain: TPanel [2]
    Left = 0
    Top = 0
    Width = 1138
    Height = 481
    Align = alTop
    BevelInner = bvLowered
    TabOrder = 2
    object Splitter2: TSplitter
      Left = 2
      Top = 48
      Width = 1134
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
      Width = 1134
      Height = 46
      Align = alTop
      BevelOuter = bvNone
      TabOrder = 0
      object JSdLabel4: TJSdLabel
        Left = 4
        Top = 6
        Width = 28
        Height = 13
        Alignment = taRightJustify
        Caption = #20998#39006
      end
      object JSdLabel5: TJSdLabel
        Left = 209
        Top = 6
        Width = 70
        Height = 13
        Alignment = taRightJustify
        Caption = #26009#34399'('#27169#31946')'
      end
      object JSdLabel7: TJSdLabel
        Left = 576
        Top = 6
        Width = 56
        Height = 13
        Alignment = taRightJustify
        Caption = #38656#27714#26085'<='
      end
      object btnInq: TSpeedButton
        Left = 756
        Top = 1
        Width = 45
        Height = 41
        Caption = #26597#35426
        Layout = blGlyphTop
        OnClick = btnInqClick
      end
      object btnMatDetail: TSpeedButton
        Left = 1043
        Top = 0
        Width = 52
        Height = 32
        Caption = #19968#35261#34920
        Layout = blGlyphTop
        Visible = False
        OnClick = btnMatDetailClick
      end
      object JSdLabel1: TJSdLabel
        Left = 910
        Top = 10
        Width = 28
        Height = 13
        Alignment = taRightJustify
        Caption = #36092#20837
        Visible = False
      end
      object btnToExcel: TSpeedButton
        Left = 818
        Top = 1
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
      object JSdLabel2: TJSdLabel
        Left = 209
        Top = 27
        Width = 70
        Height = 13
        Alignment = taRightJustify
        Caption = #21697#21517'('#27169#31946')'
      end
      object JSdLabel3: TJSdLabel
        Left = 4
        Top = 27
        Width = 56
        Height = 13
        Alignment = taRightJustify
        Caption = #35330#21934#39006#21029
      end
      object JSdLabel6: TJSdLabel
        Left = 590
        Top = 27
        Width = 42
        Height = 13
        Alignment = taRightJustify
        Caption = #19981#33391#29575
      end
      object cbo_MatClass: TJSdLookupCombo
        Left = 34
        Top = 2
        Width = 159
        Height = 21
        LkColumnCount = 2
        cboColor = clWindow
        TextSize = 80
        Text = ''
        SelectOnly = False
        SortedOff = False
        TabOrder = 0
      end
      object cbo_PartNum: TJSdLookupCombo
        Left = 282
        Top = 2
        Width = 280
        Height = 21
        LkColumnCount = 2
        cboColor = clWindow
        TextSize = 140
        Text = ''
        SelectOnly = False
        SortedOff = False
        TabOrder = 1
      end
      object ww_InqDate: TwwDBDateTimePicker
        Left = 636
        Top = 2
        Width = 104
        Height = 21
        CalendarAttributes.Font.Charset = DEFAULT_CHARSET
        CalendarAttributes.Font.Color = clWindowText
        CalendarAttributes.Font.Height = -11
        CalendarAttributes.Font.Name = 'Tahoma'
        CalendarAttributes.Font.Style = []
        Epoch = 1950
        ShowButton = True
        TabOrder = 2
        DisplayFormat = 'yyyy/mm/dd'
      end
      object cbo_MB: TJSdLookupCombo
        Left = 944
        Top = 6
        Width = 93
        Height = 21
        LkColumnCount = 2
        cboColor = clWindow
        TextSize = 45
        Text = ''
        SelectOnly = False
        SortedOff = False
        Visible = False
        TabOrder = 3
      end
      object edtMatName: TEdit
        Left = 282
        Top = 24
        Width = 280
        Height = 21
        TabOrder = 4
      end
      object cbo_POType: TJSdLookupCombo
        Left = 59
        Top = 23
        Width = 134
        Height = 21
        LkColumnCount = 2
        cboColor = clWindow
        TextSize = 60
        Text = ''
        SelectOnly = False
        SortedOff = False
        TabOrder = 5
      end
      object edtNPLratio: TEdit
        Left = 636
        Top = 25
        Width = 104
        Height = 21
        TabOrder = 6
        Text = '0'
      end
    end
    object pageBrowse: TPageControl
      Left = 2
      Top = 52
      Width = 1134
      Height = 148
      ActivePage = TabSheet1
      Align = alClient
      TabOrder = 1
      OnChange = pageBrowseChange
      object TabSheet1: TTabSheet
        Caption = #20027#20214
        object grid_Browse: TJSdDBGrid
          Left = 0
          Top = 0
          Width = 1126
          Height = 119
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
      end
      object TabSheet2: TTabSheet
        Caption = #26367#20195#26009
        ImageIndex = 1
        TabVisible = False
        object Splitter3: TSplitter
          Left = 320
          Top = 0
          Width = 4
          Height = 119
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
          Height = 119
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
          Width = 802
          Height = 119
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
      Top = 200
      Width = 1134
      Height = 279
      Align = alBottom
      TabOrder = 2
      Visible = False
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
      'select MatClass,ClassName from MINdMatClass(nolock) '
      'where MB=0 and NeedInStock=1 order by MatClass')
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
    Left = 808
    Top = 80
  end
  object dsMB: TDataSource
    DataSet = qryMB
    Left = 816
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
  object qryFMEdPOType: TJSdTable
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      
        'select POType,POTypeName from FMEdPOType(nolock) where POType in' +
        '(0,1) union Select 255, '#39#19981#35373#38480#39'  order by POType')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 728
    Top = 80
  end
  object dsFMEdPOType: TDataSource
    DataSet = qryFMEdPOType
    Left = 728
    Top = 128
  end
end

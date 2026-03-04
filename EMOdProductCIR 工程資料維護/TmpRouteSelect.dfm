inherited dlgTmpRouteSelect: TdlgTmpRouteSelect
  Caption = #29983#29986#36884#31243#36984#25799
  ClientHeight = 482
  ClientWidth = 568
  ExplicitWidth = 576
  ExplicitHeight = 509
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlTool: TPanel
    Top = 430
    Width = 568
    ExplicitTop = 430
    ExplicitWidth = 568
    inherited Panel2: TPanel
      Left = 451
      ExplicitLeft = 451
    end
  end
  object pgeMaster: TPageControl
    Left = 0
    Top = 0
    Width = 568
    Height = 430
    ActivePage = TabSheet1
    Align = alClient
    TabOrder = 1
    object TabSheet1: TTabSheet
      Caption = #36884#31243#27169#22411
      object Splitter1: TSplitter
        Left = 240
        Top = 0
        Width = 5
        Height = 402
        ExplicitHeight = 448
      end
      object pgeDtl: TPageControl
        Left = 0
        Top = 0
        Width = 240
        Height = 402
        ActivePage = tabQuery
        Align = alLeft
        MultiLine = True
        TabOrder = 0
        object tabMaster: TTabSheet
          Caption = #27169#22411
          ExplicitLeft = 0
          ExplicitTop = 0
          ExplicitWidth = 0
          ExplicitHeight = 0
          object grdRouteMas: TJSdDBGrid
            Left = 0
            Top = 0
            Width = 232
            Height = 374
            ControlType.Strings = (
              'Status;CheckBox;1;0')
            Selected.Strings = (
              'TmpId'#9'8'#9#36884#31243#20195#30908
              'Notes'#9'50'#9#20633#35387)
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alClient
            DataSource = dsTmpRouteMas
            Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
            ParentShowHint = False
            ReadOnly = True
            ShowHint = True
            TabOrder = 0
            TitleAlignment = taLeftJustify
            TitleFont.Charset = DEFAULT_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -11
            TitleFont.Name = 'Tahoma'
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = False
            SortColumnClick = stColumnClick
          end
        end
        object tabQuery: TTabSheet
          Caption = #26781#20214#26597#35426
          ImageIndex = 1
          object btnSearch: TSpeedButton
            Left = 82
            Top = 107
            Width = 70
            Height = 26
            Caption = #30906#23450
            OnClick = btnSearchClick
          end
          object Label2: TJSdLabel
            Left = 19
            Top = 67
            Width = 48
            Height = 13
            Caption = #29376'    '#24907#65306
          end
          object Label3: TJSdLabel
            Left = 19
            Top = 11
            Width = 60
            Height = 13
            Caption = #36884#31243#20195#30908#65306
          end
          object Label4: TJSdLabel
            Left = 19
            Top = 37
            Width = 48
            Height = 13
            Caption = #20633'    '#35387#65306
          end
          object edtTmpId: TEdit
            Left = 84
            Top = 7
            Width = 77
            Height = 21
            TabOrder = 0
          end
          object edtNotes: TEdit
            Left = 84
            Top = 32
            Width = 146
            Height = 21
            TabOrder = 1
          end
          object rdoStatus: TRadioGroup
            Left = 84
            Top = 53
            Width = 146
            Height = 38
            Columns = 2
            Items.Strings = (
              #35373#35336#20013
              #20351#29992#20013)
            TabOrder = 2
          end
        end
      end
      object grdRouteDtl: TJSdDBGrid
        Left = 245
        Top = 0
        Width = 315
        Height = 402
        Selected.Strings = (
          'SerialNum'#9'4'#9#24207#34399
          'ProcCode'#9'8'#9#35069#31243#20195#30908
          'ProcName'#9'17'#9#35069#31243#21517#31281
          'FinishRate'#9'4'#9#23436#24037#27604#29575
          'Notes'#9'24'#9#20633#35387)
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsTmpRouteDtl
        Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
        ParentShowHint = False
        ReadOnly = True
        ShowHint = True
        TabOrder = 1
        TitleAlignment = taLeftJustify
        TitleFont.Charset = DEFAULT_CHARSET
        TitleFont.Color = clWindowText
        TitleFont.Height = -11
        TitleFont.Name = 'Tahoma'
        TitleFont.Style = []
        TitleLines = 1
        TitleButtons = False
        SortColumnClick = stColumnClick
      end
    end
    object TabSheet2: TTabSheet
      Caption = #29986#21697#22411#24907#20633#35387
      ImageIndex = 2
      ExplicitLeft = 0
      ExplicitTop = 0
      ExplicitWidth = 0
      ExplicitHeight = 0
      object Panel5: TPanel
        Left = 0
        Top = 0
        Width = 560
        Height = 402
        Align = alClient
        BevelOuter = bvNone
        ParentColor = True
        TabOrder = 0
        object Splitter2: TSplitter
          Left = 236
          Top = 0
          Height = 402
          Align = alRight
          ExplicitLeft = 294
          ExplicitTop = -2
        end
        object Splitter3: TSplitter
          Left = 285
          Top = 0
          Height = 402
          Align = alRight
          ExplicitLeft = 327
          ExplicitTop = -2
        end
        object Panel1: TPanel
          Left = 288
          Top = 0
          Width = 272
          Height = 402
          Align = alRight
          BevelInner = bvRaised
          BevelOuter = bvLowered
          Caption = 'Panel1'
          TabOrder = 0
          object Panel6: TPanel
            Left = 2
            Top = 2
            Width = 268
            Height = 31
            Align = alTop
            BevelOuter = bvNone
            Caption = #24050#36984#20839#23481
            ParentColor = True
            TabOrder = 0
          end
          object wwDBGrid1: TJSdDBGrid
            Left = 2
            Top = 219
            Width = 268
            Height = 181
            Selected.Strings = (
              'ProcCode'#9'8'#9#35069#31243
              'Notes'#9'32'#9#20633#35387)
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alClient
            DataSource = dsTmpDtl
            Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
            ParentShowHint = False
            ReadOnly = True
            ShowHint = True
            TabOrder = 1
            TitleAlignment = taLeftJustify
            TitleFont.Charset = DEFAULT_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -11
            TitleFont.Name = 'Tahoma'
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = False
            SortColumnClick = stColumnClick
          end
          object wwDBGrid2: TJSdDBGrid
            Left = 2
            Top = 33
            Width = 268
            Height = 186
            Selected.Strings = (
              'ItemName'#9'24'#9#39006#21029)
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alTop
            DataSource = dsTmp
            Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
            ParentShowHint = False
            ReadOnly = True
            ShowHint = True
            TabOrder = 2
            TitleAlignment = taLeftJustify
            TitleFont.Charset = DEFAULT_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -11
            TitleFont.Name = 'Tahoma'
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = False
            SortColumnClick = stColumnClick
          end
        end
        object Panel3: TPanel
          Left = 239
          Top = 0
          Width = 46
          Height = 402
          Align = alRight
          BevelInner = bvRaised
          BevelOuter = bvLowered
          TabOrder = 1
          object BitBtn4: TSpeedButton
            Left = 11
            Top = 125
            Width = 25
            Height = 25
            Glyph.Data = {
              76010000424D7601000000000000760000002800000020000000100000000100
              04000000000000010000120B0000120B00001000000000000000000000000000
              800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
              FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
              3333333333333333333333333333333333333333333333333333333333333333
              3333333333333FF3333333333333003333333333333F77F33333333333009033
              333333333F7737F333333333009990333333333F773337FFFFFF330099999000
              00003F773333377777770099999999999990773FF33333FFFFF7330099999000
              000033773FF33777777733330099903333333333773FF7F33333333333009033
              33333333337737F3333333333333003333333333333377333333333333333333
              3333333333333333333333333333333333333333333333333333333333333333
              3333333333333333333333333333333333333333333333333333}
            NumGlyphs = 2
            OnClick = BitBtn4Click
          end
          object BitBtn3: TSpeedButton
            Left = 11
            Top = 70
            Width = 25
            Height = 25
            Glyph.Data = {
              76010000424D7601000000000000760000002800000020000000100000000100
              04000000000000010000120B0000120B00001000000000000000000000000000
              800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
              FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
              3333333333333333333333333333333333333333333333333333333333333333
              3333333333333333333333333333333333333333333FF3333333333333003333
              3333333333773FF3333333333309003333333333337F773FF333333333099900
              33333FFFFF7F33773FF30000000999990033777777733333773F099999999999
              99007FFFFFFF33333F7700000009999900337777777F333F7733333333099900
              33333333337F3F77333333333309003333333333337F77333333333333003333
              3333333333773333333333333333333333333333333333333333333333333333
              3333333333333333333333333333333333333333333333333333}
            NumGlyphs = 2
            OnClick = BitBtn3Click
          end
        end
        object Panel4: TPanel
          Left = 0
          Top = 0
          Width = 236
          Height = 402
          Align = alClient
          BevelInner = bvRaised
          BevelOuter = bvLowered
          Caption = 'Panel1'
          TabOrder = 2
          object trvMas: TJSdTreeView
            Left = 2
            Top = 33
            Width = 232
            Height = 186
            Align = alTop
            Font.Charset = CHINESEBIG5_CHARSET
            Font.Color = clWindowText
            Font.Height = -12
            Font.Name = #32048#26126#39636
            Font.Style = []
            Indent = 19
            ParentFont = False
            TabOrder = 0
            OnChanging = trvMasChanging
            Items.NodeData = {
              0308000000220000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFB09884
              0F0000000001021F75A17B220000000000000000000000FFFFFFFFFFFFFFFFFF
              FFFFFF1098840F0200000001028765575B240000000100000000000000FFFFFF
              FFFFFFFFFFFFFFFFFF908C840F0200000001032171759E207D24000000020000
              0000000000FFFFFFFFFFFFFFFFFFFFFFFFF09B840F0000000001035500530049
              00280000000200000000000000FFFFFFFFFFFFFFFFFFFFFFFFB09B840F000000
              00010554006100690079006F00240000000100000000000000FFFFFFFFFFFFFF
              FFFFFFFFFF109C840F0100000001030967759E207D2400000002000000000000
              00FFFFFFFFFFFFFFFFFFFFFFFF309C840F020000000103386CDD52F06C2A0000
              000300000000000000FFFFFFFFFFFFFFFFFFFFFFFF909B840F00000000010679
              0065006C006C006F007700280000000300000000000000FFFFFFFFFFFFFFFFFF
              FFFFFF309A840F00000000010542006C00610063006B00220000000000000000
              000000FFFFFFFFFFFFFFFFFFFFFFFF5097840F06000000010210628B572A0000
              000100000000000000FFFFFFFFFFFFFFFFFFFFFFFF3096840F00000000010653
              004C004F0054006C51EE5D300000000100000000000000FFFFFFFFFFFFFFFFFF
              FFFFFF9093840F00000000010963006F006E006E006500630074006C51EE5D32
              0000000100000000000000FFFFFFFFFFFFFFFFFFFFFFFF908F840F0000000001
              0A63006F006E006E006500630074006C51EE5D31002800000001000000000000
              00FFFFFFFFFFFFFFFFFFFFFFFF508D840F00000000010556002D004300550054
              002C0000000100000000000000FFFFFFFFFFFFFFFFFFFFFFFF1090840F000000
              00010746006F00720020004B0036003000280000000100000000000000FFFFFF
              FFFFFFFFFFFFFFFFFF90F42E1D01000000010510628B575C4F6D6932002C0000
              000200000000000000FFFFFFFFFFFFFFFFFFFFFFFFD0F32E1D00000000010746
              006F00720020004B0036003000220000000000000000000000FFFFFFFFFFFFFF
              FFFFFFFFFF1093840F010000000102FA577F67220000000100000000000000FF
              FFFFFFFFFFFFFFFFFFFFFF90F02E1D000000000102E05E8F8922000000000000
              0000000000FFFFFFFFFFFFFFFFFFFFFFFF10F32E1D01000000010232960A712C
              0000000100000000000000FFFFFFFFFFFFFFFFFFFFFFFF30F32E1D0000000001
              0746006F00720020004B0036003000260000000000000000000000FFFFFFFFFF
              FFFFFFFFFFFFFF30F22E1D0100000001041062C1542C6E668A2C000000010000
              0000000000FFFFFFFFFFFFFFFFFFFFFFFF90F12E1D00000000010746006F0072
              0020004B0036003000220000000000000000000000FFFFFFFFFFFFFFFFFFFFFF
              FF50F22E1D040000000102636B47722C0000000100000000000000FFFFFFFFFF
              FFFFFFFFFFFFFFF0F32E1D00000000010746006F00720020004B00360030002C
              0000000100000000000000FFFFFFFFFFFFFFFFFFFFFFFFB0F12E1D0000000001
              0746006F0072002000410055004F003C0000000100000000000000FFFFFFFFFF
              FFFFFFFFFFFFFF70F02E1D00000000010F46006F0072002000410055004F0028
              00330035002B0035002F00380029002A0000000100000000000000FFFFFFFFFF
              FFFFFFFFFFFFFF50EF2E1D00000000010646006F0072002000004E2C82240000
              000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFD0ED2E1D00000000010355
              8784858592}
            DataFieldId = 'ItemId'
            DataFieldLevelNo = 'LevelNo'
            DataFieldSuperId = 'SuperId'
            DataFieldCaption = 'ItemName'
            DataFieldSort = 'ItemId'
            DataSource = dsMas
            SetupExpand = False
            SortFieldInt = False
          end
          object Panel7: TPanel
            Left = 2
            Top = 2
            Width = 232
            Height = 31
            Align = alTop
            BevelOuter = bvNone
            Caption = #36039#26009#20358#28304
            ParentColor = True
            TabOrder = 1
          end
          object wwDBGrid3: TJSdDBGrid
            Left = 2
            Top = 219
            Width = 232
            Height = 181
            Selected.Strings = (
              'ProcCode'#9'8'#9#35069#31243
              'Notes'#9'32'#9#20633#35387)
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alClient
            DataSource = dsDtl
            Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
            ParentShowHint = False
            ReadOnly = True
            ShowHint = True
            TabOrder = 2
            TitleAlignment = taLeftJustify
            TitleFont.Charset = DEFAULT_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -11
            TitleFont.Name = 'Tahoma'
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = False
            OnDblClick = wwDBGrid3DblClick
            SortColumnClick = stColumnClick
          end
        end
      end
    end
  end
  object qryMas: TADOQuery
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'Select *'
      'From EMOdProdStyleTree (nolock)'
      'Order by ItemId')
    Left = 52
    Top = 95
    object qryMasItemId: TStringField
      FieldName = 'ItemId'
      Origin = 'DBJSISDATA.EMOdProdStyleTree.ItemId'
      FixedChar = True
      Size = 8
    end
    object qryMasItemName: TWideStringField
      FieldName = 'ItemName'
      Origin = 'DBJSISDATA.EMOdProdStyleTree.ItemName'
      FixedChar = True
      Size = 24
    end
    object qryMasLevelNo: TIntegerField
      FieldName = 'LevelNo'
      Origin = 'DBJSISDATA.EMOdProdStyleTree.LevelNo'
    end
    object qryMasSuperId: TStringField
      FieldName = 'SuperId'
      Origin = 'DBJSISDATA.EMOdProdStyleTree.SuperId'
      FixedChar = True
      Size = 8
    end
    object qryMasItemNo: TIntegerField
      FieldName = 'ItemNo'
      Origin = 'DBJSISDATA.EMOdProdStyleTree.ItemNo'
    end
  end
  object dsMas: TDataSource
    DataSet = qryMas
    Left = 76
    Top = 95
  end
  object qryTmpMas: TJSdTable
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'Cond'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 255
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdRouteSelectMas :Cond')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'EMOdRouteSelectMas'
    Left = 128
    Top = 85
  end
  object dsTmpRouteMas: TDataSource
    DataSet = qryTmpMas
    Left = 156
    Top = 85
  end
  object qryTmpDtl: TJSdTable
    CursorType = ctStatic
    DataSource = dsTmpRouteMas
    Parameters = <
      item
        Name = 'TmpId'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 12
        Value = Null
      end>
    SQL.Strings = (
      'select * '
      'from dbo.EMOdTmpRouteDtl(nolock)'
      'where TmpId=:TmpId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    MasterSource = dsTmpRouteMas
    Left = 120
    Top = 149
    object qryTmpDtlSerialNum: TSmallintField
      DisplayLabel = #24207#34399
      DisplayWidth = 4
      FieldName = 'SerialNum'
    end
    object qryTmpDtlProcCode: TStringField
      DisplayLabel = #35069#31243#20195#30908
      DisplayWidth = 8
      FieldName = 'ProcCode'
      FixedChar = True
      Size = 8
    end
    object qryTmpDtlProcName: TWideStringField
      DisplayLabel = #35069#31243#21517#31281
      DisplayWidth = 17
      FieldKind = fkLookup
      FieldName = 'ProcName'
      LookupDataSet = qryProcBasic
      LookupKeyFields = 'ProcCode'
      LookupResultField = 'ProcName'
      KeyFields = 'ProcCode'
      Lookup = True
    end
    object qryTmpDtlFinishRate: TFloatField
      DisplayLabel = #23436#24037#27604#29575
      DisplayWidth = 4
      FieldName = 'FinishRate'
    end
    object qryTmpDtlNotes: TWideStringField
      DisplayLabel = #20633#35387
      DisplayWidth = 24
      FieldName = 'Notes'
      FixedChar = True
      Size = 255
    end
  end
  object dsTmpRouteDtl: TDataSource
    DataSet = qryTmpDtl
    Left = 164
    Top = 197
  end
  object qryDtl: TJSdTable
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'ItemId'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'select * from EMOdProdStyleTreeSub (nolock)'
      'Where ItemId = :ItemId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'EMOdRouteSelectDtl'
    Left = 52
    Top = 191
  end
  object dsDtl: TDataSource
    DataSet = qryDtl
    Left = 84
    Top = 191
  end
  object qryTmp: TJSdTable
    CursorType = ctStatic
    AfterScroll = qryTmpAfterScroll
    Parameters = <
      item
        Name = 'PartNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'Select *'
      'From EMOdProdStyleTreeTmp (nolock)'
      'Where PartNum = :PartNum'
      'And Revision = :Revision'
      ' '
      ' ')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'EMOdRouteSelectTmp'
    Left = 268
    Top = 71
  end
  object dsTmp: TDataSource
    DataSet = qryTmp
    Left = 300
    Top = 71
  end
  object qryTmpDtl2: TJSdTable
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'ItemId'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 2
        Value = Null
      end>
    SQL.Strings = (
      'select * from EMOdProdStyleTreeSub (nolock)'
      'Where ItemId = :ItemId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'EMOdRouteSelectTmpDtl'
    Left = 252
    Top = 183
  end
  object dsTmpDtl: TDataSource
    DataSet = qryTmpDtl2
    Left = 164
    Top = 151
  end
  object qryTmpIns: TADOQuery
    Parameters = <
      item
        Name = 'PartNum'
        DataType = ftString
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        DataType = ftString
        Size = 4
        Value = Null
      end
      item
        Name = 'ItemId'
        DataType = ftString
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      
        'Insert into EMOdProdStyleTreeTmp(PartNum, Revision, ItemId ,Item' +
        'Name)'
      'Select :PartNum, :Revision, ItemId ,ItemName'
      'From EMOdProdStyleTree (nolock)'
      'Where ItemId = :ItemId'
      ' '
      ' ')
    Left = 421
    Top = 40
  end
  object qryTmpDel: TADOQuery
    Parameters = <
      item
        Name = 'ItemId'
        DataType = ftString
        Size = 8
        Value = Null
      end
      item
        Name = 'PartNum'
        DataType = ftString
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        DataType = ftString
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'Delete EMOdProdStyleTreeTmp'
      'Where ItemId = :ItemId'
      'And PartNum = :PartNum'
      'And Revision = :Revision'
      ' '
      ' ')
    Left = 421
    Top = 88
  end
  object qryTmpDelAll: TADOQuery
    Parameters = <
      item
        Name = 'PartNum'
        DataType = ftString
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        DataType = ftString
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'Delete EMOdProdStyleTreeTmp'
      'Where PartNum = :PartNum'
      'And Revision = :Revision'
      ' '
      ' ')
    Left = 421
    Top = 136
  end
  object qryProcBasic: TADOQuery
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'SELECT ProcCode, ProcName'
      'FROM dbo.EMOdProcInfo(nolock)'
      'Order by ProcCode'
      ' ')
    Left = 52
    Top = 238
  end
  object qryChkTable: TADOQuery
    Parameters = <
      item
        Name = 'PartNum'
        DataType = ftString
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        DataType = ftString
        Size = 4
        Value = Null
      end
      item
        Name = 'ItemId'
        DataType = ftString
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'select FieldName from CURdTableField(nolock)'
      'where TableName='#39'EMOdRouteSelectMas'#39' and FieldName='#39'IsStop'#39
      ' '
      ' ')
    Left = 501
    Top = 40
  end
end

inherited dlgRouteInsNew: TdlgRouteInsNew
  Caption = #36884#31243#20839#23481#35722#26356
  ClientHeight = 345
  ClientWidth = 516
  ExplicitWidth = 524
  ExplicitHeight = 372
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlTool: TPanel
    Top = 293
    Width = 516
    ExplicitTop = 293
    ExplicitWidth = 516
    inherited Panel2: TPanel
      Left = 399
      ExplicitLeft = 399
      inherited btnOK: TBitBtn
        OnClick = btnOKClick
      end
      inherited btnCancel: TBitBtn
        OnClick = btnCancelClick
      end
    end
  end
  object Panel1: TPanel
    Left = 0
    Top = 0
    Width = 516
    Height = 41
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 1
    object Label3D12: TJSdLabel
      Tag = 1
      Left = 171
      Top = 14
      Width = 36
      Height = 13
      Alignment = taRightJustify
      Caption = #38542'    '#27573
    end
    object btnSerialUp: TSpeedButton
      Left = 374
      Top = 0
      Width = 40
      Height = 40
      Hint = #19978#31227#19968#26684
      Caption = #19978#31227
      Flat = True
      Glyph.Data = {
        1E010000424D1E010000000000007600000028000000180000000E0000000100
        040000000000A800000000000000000000001000000000000000000000000000
        80000080000000808000800000008000800080800000C0C0C000808080000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00777788888777
        777788888777777F00008777777F77778777777F00008777777F77778777777F
        00008777777F77778777777F00008777777F77778777777F00008777777F7777
        8777788700008888788777778888F00000000008F77777777778F00000000007
        F777777777777F00000000777F777777777777F00000077777F777777777777F
        00007777777F777777777777F00777777777F777777777777F77777777777F77
        7777}
      Layout = blGlyphTop
      ParentShowHint = False
      ShowHint = True
      OnClick = btnSerialUpClick
    end
    object btnSerialDown: TSpeedButton
      Left = 411
      Top = 0
      Width = 40
      Height = 40
      Hint = #19979#31227#19968#26684
      Caption = #19979#31227
      Flat = True
      Glyph.Data = {
        1E010000424D1E010000000000007600000028000000180000000E0000000100
        040000000000A800000000000000000000001000000000000000000000000000
        80000080000000808000800000008000800080800000C0C0C000808080000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00777777877777
        7777778777777777700877777777777877777777000087777777777787777770
        0000087777777777787777000000008777777777778770000000000877777777
        7778F00000000008F77F77778778FFFF00007FF7FFFF77778FF7777F00008777
        777F77778777777F00008777777F77778777777F00008777777F77778777777F
        00008777777F77778777777F00008777777F77778777777FFFFF7777777FFFFF
        7777}
      Layout = blGlyphTop
      ParentShowHint = False
      ShowHint = True
      OnClick = btnSerialDownClick
    end
    object DBNavigator1: TDBNavigator
      Left = 0
      Top = 0
      Width = 129
      Height = 41
      DataSource = dsRoute
      VisibleButtons = [nbInsert, nbDelete, nbPost, nbCancel]
      Align = alLeft
      ParentShowHint = False
      ShowHint = False
      TabOrder = 0
    end
    object edtLayer: TEdit
      Left = 213
      Top = 11
      Width = 76
      Height = 21
      Color = clMenu
      Enabled = False
      TabOrder = 1
    end
  end
  object wwDBGrid1: TwwDBGrid
    Left = 0
    Top = 41
    Width = 516
    Height = 252
    ControlType.Strings = (
      'ProcCode;CustomEdit;cboProcCode;T')
    Selected.Strings = (
      'SerialNum'#9'4'#9#38917#27425#9#9
      'ProcCode'#9'8'#9#20195#30908#9#9
      'ProcName'#9'16'#9#35069#31243#21517#31281#9'T'#9
      'Notes'#9'64'#9#20633#35387#9#9)
    IniAttributes.Delimiter = ';;'
    IniAttributes.UnicodeIniFile = False
    TitleColor = clBtnFace
    FixedCols = 0
    ShowHorzScrollBar = True
    Align = alClient
    DataSource = dsRoute
    TabOrder = 3
    TitleAlignment = taLeftJustify
    TitleFont.Charset = DEFAULT_CHARSET
    TitleFont.Color = clWindowText
    TitleFont.Height = -11
    TitleFont.Name = 'Tahoma'
    TitleFont.Style = []
    TitleLines = 1
    TitleButtons = False
  end
  object cboProcCode: TwwDBLookupCombo
    Left = 80
    Top = 104
    Width = 81
    Height = 21
    DropDownAlignment = taLeftJustify
    Selected.Strings = (
      'ProcCode'#9'8'#9'ProcCode'#9'F'
      'ProcName'#9'24'#9'ProcName'#9'F')
    DataField = 'ProcCode'
    DataSource = dsRoute
    LookupTable = qryProcInfo
    LookupField = 'ProcCode'
    TabOrder = 2
    AutoDropDown = False
    ShowButton = True
    PreciseEditRegion = False
    AllowClearKey = False
  end
  object qryProcInfo: TADOQuery
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'SELECT ProcCode, ProcName from EMOdProcInfo(nolock)')
    Left = 92
    Top = 122
    object qryProcInfoProcCode: TStringField
      DisplayWidth = 8
      FieldName = 'ProcCode'
      FixedChar = True
      Size = 8
    end
    object qryProcInfoProcName: TWideStringField
      DisplayWidth = 24
      FieldName = 'ProcName'
      Size = 24
    end
  end
  object dsProcInfo: TwwDataSource
    DataSet = qryProcInfo
    Left = 120
    Top = 121
  end
  object qryRoute: TJSdTable
    CursorType = ctStatic
    BeforeInsert = qryRouteBeforeInsert
    AfterInsert = qryRouteAfterInsert
    AfterPost = qryRouteAfterPost
    BeforeDelete = qryRouteBeforeDelete
    AfterDelete = qryRouteAfterDelete
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
        Size = 8
        Value = Null
      end
      item
        Name = 'LayerId'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'SpId'
        Attributes = [paSigned]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'select * from EMOdLayerRouteTemp(nolock) where PartNum=:PartNum '
      'and Revision =:Revision and LayerId=:LayerId and SpId=:SpId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'EMOdRouteNew'
    Left = 192
    Top = 128
    object qryRouteSerialNum: TWordField
      DisplayLabel = #38917#27425
      DisplayWidth = 4
      FieldName = 'SerialNum'
    end
    object qryRouteProcCode: TStringField
      DisplayLabel = #20195#30908
      DisplayWidth = 8
      FieldName = 'ProcCode'
      Size = 8
    end
    object qryRouteProcName: TWideStringField
      DisplayLabel = #35069#31243#21517#31281
      DisplayWidth = 16
      FieldKind = fkLookup
      FieldName = 'ProcName'
      LookupDataSet = qryProcInfo
      LookupKeyFields = 'ProcCode'
      LookupResultField = 'ProcName'
      KeyFields = 'ProcCode'
      Size = 24
      Lookup = True
    end
    object qryRouteNotes: TWideStringField
      DisplayLabel = #20633#35387
      DisplayWidth = 64
      FieldName = 'Notes'
      Size = 4000
    end
    object qryRoutePartNum: TStringField
      FieldName = 'PartNum'
      Visible = False
      Size = 24
    end
    object qryRouteRevision: TStringField
      FieldName = 'Revision'
      Visible = False
      Size = 8
    end
    object qryRouteLayerId: TStringField
      FieldName = 'LayerId'
      Visible = False
      Size = 8
    end
    object qryRouteFinishRate: TFloatField
      FieldName = 'FinishRate'
      Visible = False
    end
    object qryRouteIsNormal: TWideStringField
      FieldName = 'IsNormal'
      Visible = False
    end
    object qryRouteDepartId: TStringField
      FieldName = 'DepartId'
      Visible = False
      Size = 12
    end
    object qryRouteSpec: TWideStringField
      FieldName = 'Spec'
      Visible = False
      Size = 255
    end
    object qryRouteFilmNo: TWideStringField
      FieldName = 'FilmNo'
      Visible = False
      Size = 255
    end
    object qryRouteChangeNotes: TWideStringField
      FieldName = 'ChangeNotes'
      Visible = False
      Size = 255
    end
    object qryRoutePartSerial: TStringField
      FieldName = 'PartSerial'
      Visible = False
      Size = 8
    end
    object qryRouteProcSerial: TStringField
      FieldName = 'ProcSerial'
      Visible = False
      Size = 8
    end
    object qryRouteSortType: TStringField
      FieldName = 'SortType'
      Visible = False
      Size = 2
    end
    object qryRouteBefSETime: TFloatField
      FieldName = 'BefSETime'
      Visible = False
    end
    object qryRouteSPId: TIntegerField
      FieldName = 'SPId'
    end
  end
  object dsRoute: TwwDataSource
    DataSet = qryRoute
    Left = 224
    Top = 121
  end
  object qryPosChange: TADOQuery
    Parameters = <
      item
        Name = 'PartNum'
        Size = -1
        Value = Null
      end
      item
        Name = 'Revision'
        Size = -1
        Value = Null
      end
      item
        Name = 'LayerID'
        Size = -1
        Value = Null
      end
      item
        Name = 'Pos'
        Size = -1
        Value = Null
      end
      item
        Name = 'Direction'
        Size = -1
        Value = Null
      end
      item
        Name = 'SpId'
        Size = -1
        Value = Null
      end>
    SQL.Strings = (
      
        'exec EMOdRouteChangePos :PartNum, :Revision, :LayerID, :Pos, :Di' +
        'rection,'
      ' :SpId')
    Left = 400
    Top = 48
  end
  object qryReLoad: TADOQuery
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
        Size = 8
        Value = Null
      end
      item
        Name = 'LayerId'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'SerialNum'
        DataType = ftWord
        Precision = 3
        Size = 1
        Value = Null
      end>
    SQL.Strings = (
      'update EMOdLayerRouteTemp set SerialNum= SerialNum-1'
      'where PartNum=:PartNum and Revision=:Revision '
      'and LayerId=:LayerId and SerialNum>:SerialNum')
    Left = 400
    Top = 80
  end
  object qryCloseCheck: TADOQuery
    Parameters = <
      item
        Name = 'PartNum'
        Size = -1
        Value = Null
      end
      item
        Name = 'Revision'
        Size = -1
        Value = Null
      end
      item
        Name = 'LayerId'
        Size = -1
        Value = Null
      end
      item
        Name = 'SpId'
        Size = -1
        Value = Null
      end>
    SQL.Strings = (
      
        'exec EMOdRouteChangeCheck :PartNum, :Revision, :LayerId, :SpId, ' +
        '0')
    Left = 424
    Top = 216
  end
  object qryClear: TADOQuery
    Parameters = <
      item
        Name = 'SpId'
        Attributes = [paSigned]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end
      item
        Name = 'PartNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'revision'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'LayerId'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'delete EMOdLayerRouteTemp where SpId=:SpId and PartNum=:PartNum'
      'and Revision =:revision and LayerId=:LayerId')
    Left = 424
    Top = 264
  end
  object qryExec: TADOQuery
    EnableBCD = False
    Parameters = <>
    Left = 88
    Top = 184
  end
end

inherited dlgPressChange: TdlgPressChange
  Caption = #30090#27083#35722#26356
  ClientHeight = 438
  ClientWidth = 544
  ExplicitWidth = 552
  ExplicitHeight = 465
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlTool: TPanel
    Top = 386
    Width = 544
    ExplicitTop = 386
    ExplicitWidth = 544
    inherited Panel2: TPanel
      Left = 427
      ExplicitLeft = 427
      inherited btnOK: TBitBtn
        OnClick = btnOKClick
      end
    end
  end
  object Panel1: TPanel
    Left = 0
    Top = 0
    Width = 544
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
      DataSource = dsTmpPressMas
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
  object wwDBGrid1: TJSdDBGrid
    Left = 0
    Top = 41
    Width = 544
    Height = 345
    ControlType.Strings = (
      'MatClass;CustomEdit;cboMatClass;F')
    Selected.Strings = (
      'SerialNum'#9'6'#9#38917#27425#9'F'#9
      'MatClass'#9'12'#9#29289#26009#20998#39006#9'F'#9
      'ClassName'#9'20'#9#20998#39006#21517#31281#9'T'#9
      'Notes'#9'50'#9#20633#35387#9'F'#9)
    IniAttributes.Delimiter = ';;'
    IniAttributes.UnicodeIniFile = False
    TitleColor = clBtnFace
    FixedCols = 0
    ShowHorzScrollBar = True
    Align = alClient
    DataSource = dsTmpPressMas
    Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
    ParentShowHint = False
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
      
        'exec EMOdPressChangePos :PartNum, :Revision, :LayerID, :Pos, :Di' +
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
      'update EMOdLayerPressTemp set SerialNum= SerialNum-1'
      'where PartNum=:PartNum and Revision=:Revision '
      'and LayerId=:LayerId and SerialNum>:SerialNum')
    Left = 400
    Top = 96
  end
  object qryTmpPressMas: TJSdTable
    CursorType = ctStatic
    BeforeInsert = qryTmpPressMasBeforeInsert
    AfterInsert = qryTmpPressMasAfterInsert
    BeforeEdit = qryTmpPressMasBeforeEdit
    AfterPost = qryTmpPressMasAfterPost
    BeforeDelete = qryTmpPressMasBeforeDelete
    AfterDelete = qryTmpPressMasAfterDelete
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
      'select * from EMOdLayerPressTemp(nolock) where PartNum=:PartNum '
      'and Revision =:Revision and LayerId=:LayerId and SpId=:SpId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'EMOdPressChange'
    Left = 200
    Top = 120
    object qryTmpPressMasSerialNum: TWordField
      DisplayLabel = #38917#27425
      DisplayWidth = 6
      FieldName = 'SerialNum'
    end
    object qryTmpPressMasMatClass: TStringField
      DisplayLabel = #29289#26009#20998#39006
      DisplayWidth = 12
      FieldName = 'MatClass'
      FixedChar = True
      Size = 8
    end
    object qryTmpPressMasClassName: TWideStringField
      DisplayLabel = #20998#39006#21517#31281
      DisplayWidth = 20
      FieldKind = fkLookup
      FieldName = 'ClassName'
      LookupDataSet = qryMatClass
      LookupKeyFields = 'MatClass'
      LookupResultField = 'ClassName'
      KeyFields = 'MatClass'
      Size = 36
      Lookup = True
    end
    object qryTmpPressMasNotes: TWideStringField
      DisplayLabel = #20633#35387
      DisplayWidth = 50
      FieldName = 'Notes'
      Size = 255
    end
    object qryTmpPressMasPartNum: TStringField
      DisplayWidth = 12
      FieldName = 'PartNum'
      Visible = False
      FixedChar = True
      Size = 24
    end
    object qryTmpPressMasRevision: TStringField
      DisplayWidth = 4
      FieldName = 'Revision'
      Visible = False
      FixedChar = True
      Size = 4
    end
    object qryTmpPressMasLayerId: TStringField
      DisplayWidth = 8
      FieldName = 'LayerId'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryTmpPressMasBefLayer: TStringField
      DisplayWidth = 8
      FieldName = 'BefLayer'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryTmpPressMasSpId: TIntegerField
      DisplayWidth = 10
      FieldName = 'SpId'
      Visible = False
    end
    object qryTmpPressMasIsIn: TStringField
      FieldName = 'IsIn'
      Visible = False
      Size = 24
    end
  end
  object dsTmpPressMas: TwwDataSource
    DataSet = qryTmpPressMas
    Left = 224
    Top = 121
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
        Name = 'SpId'
        Size = -1
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdPressChangeCheck :PartNum, :Revision, :SpId, 0')
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
      'delete EMOdLayerPressTemp where SpId=:SpId and PartNum=:PartNum'
      'and Revision =:revision and LayerId=:LayerId')
    Left = 424
    Top = 264
  end
  object qryExec: TADOQuery
    EnableBCD = False
    Parameters = <>
    Left = 208
    Top = 208
  end
  object qryMatClass: TADOQuery
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'exec EMOdMatClassSelect 1'
      ''
      ' ')
    Left = 68
    Top = 82
  end
end

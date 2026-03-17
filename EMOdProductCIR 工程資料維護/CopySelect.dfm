inherited dlgCopySelect: TdlgCopySelect
  Caption = ''
  ClientHeight = 339
  ClientWidth = 871
  ExplicitWidth = 879
  ExplicitHeight = 366
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlTool: TPanel
    Top = 287
    Width = 871
    ExplicitTop = 287
    ExplicitWidth = 871
    inherited Panel2: TPanel
      Left = 754
      ExplicitLeft = 754
    end
  end
  object Panel1: TPanel
    Left = 0
    Top = 0
    Width = 871
    Height = 89
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 1
    object Label1: TLabel
      Left = 24
      Top = 16
      Width = 48
      Height = 14
      Caption = #20358#28304#21697#34399
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -12
      Font.Name = 'Tahoma'
      Font.Style = []
      ParentFont = False
    end
    object Label2: TLabel
      Left = 24
      Top = 40
      Width = 48
      Height = 14
      Caption = #20358#28304#29256#24207
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -12
      Font.Name = 'Tahoma'
      Font.Style = []
      ParentFont = False
    end
    object Label3: TLabel
      Left = 24
      Top = 64
      Width = 48
      Height = 14
      Caption = #20358#28304#23652#21029
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -12
      Font.Name = 'Tahoma'
      Font.Style = []
      ParentFont = False
    end
    object btFind: TSpeedButton
      Left = 239
      Top = 13
      Width = 60
      Height = 40
      Caption = '&F'#36039#26009#37325#21462
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
    object edtPartNum: TEdit
      Left = 96
      Top = 13
      Width = 121
      Height = 22
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -12
      Font.Name = 'Tahoma'
      Font.Style = []
      ParentFont = False
      TabOrder = 0
    end
    object edtRevision: TEdit
      Left = 96
      Top = 37
      Width = 121
      Height = 22
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -12
      Font.Name = 'Tahoma'
      Font.Style = []
      ParentFont = False
      TabOrder = 1
    end
    object cboLayerId: TJSdLookupCombo
      Left = 96
      Top = 61
      Width = 121
      Height = 21
      LkDataSource = dsLayerId
      LkColumnCount = 2
      cboColor = clWindow
      TextSize = 120
      SelectOnly = False
      SortedOff = False
      TabOrder = 2
      TabStop = True
      OnEnter = cboLayerIdEnter
    end
  end
  object msCopy: TJSdMultSelect
    Left = 0
    Top = 89
    Width = 871
    Height = 198
    Align = alClient
    WidthSource = 0
    ColorSource = clWindow
    ColorTarget = clWindow
    ToolAlign = taCenter
    SortTypeSource = stNone
    SortTypeTarget = stNone
    RowSelectSource = True
    RowSelectTarget = True
    SourceColumns = <
      item
        Caption = #23652#21029
        Width = 70
      end
      item
        Caption = #21517#31281
        Width = 90
      end
      item
        Caption = #20358#28304#23652#21029
        Width = 70
      end
      item
        Caption = #20358#28304#21517#31281
        Width = 90
      end
      item
        Caption = #20358#28304#36884#31243
        Width = 90
      end>
    TargetColumns = <>
    Distinct = False
    SelectMode = smMove
    DataSourceSource = dsTargetLayer
    SetupList = slSource
  end
  object qryTargetLayer: TADOQuery
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'PartNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 4
        Value = Null
      end
      item
        Name = 'LayerId'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'SourPartNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'SourRevision'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'Exec EMOdRouteCopySearch :PartNum ,:Revision ,:LayerId,'
      ':SourPartNum, :SourRevision')
    Left = 128
    Top = 182
    object qryTargetLayerLayerId: TStringField
      FieldName = 'LayerId'
      FixedChar = True
      Size = 8
    end
    object qryTargetLayerLayerName: TWideStringField
      FieldName = 'LayerName'
      Size = 24
    end
    object qryTargetLayerSourceLayerId: TStringField
      FieldName = 'SourceLayerId'
      ReadOnly = True
      Size = 8
    end
    object qryTargetLayerSourceName: TWideStringField
      FieldName = 'SourceName'
      Size = 24
    end
    object qryTargetLayerTmpRouteId: TStringField
      FieldName = 'TmpRouteId'
      ReadOnly = True
      Size = 12
    end
    object qryTargetLayerPartNum: TStringField
      FieldName = 'PartNum'
      FixedChar = True
      Size = 24
    end
    object qryTargetLayerRevision: TStringField
      FieldName = 'Revision'
      FixedChar = True
      Size = 4
    end
  end
  object dsTargetLayer: TDataSource
    DataSet = qryTargetLayer
    Left = 128
    Top = 222
  end
  object qryLayerId: TADOQuery
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'PartNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'select LayerId,LayerName --=LayerId+'#39#23652#39
      'from EMOdProdLayer(nolock)'
      'where PartNum= :PartNum '
      'and Revision= :Revision   '
      '  '
      '  ')
    Left = 312
    Top = 38
  end
  object dsLayerId: TDataSource
    DataSet = qryLayerId
    Left = 344
    Top = 38
  end
end

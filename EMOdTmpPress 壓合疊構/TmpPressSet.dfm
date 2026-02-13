inherited dlgTmpPressSet: TdlgTmpPressSet
  Caption = #22739#21512#26448#26009#35373#23450
  ClientHeight = 440
  ClientWidth = 771
  ExplicitWidth = 779
  ExplicitHeight = 467
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlTool: TPanel
    Top = 388
    Width = 771
    ExplicitTop = 388
    ExplicitWidth = 671
    inherited Panel2: TPanel
      Left = 654
      ExplicitLeft = 554
    end
  end
  object pnlUpDown: TPanel
    Left = 0
    Top = 0
    Width = 771
    Height = 44
    Align = alTop
    TabOrder = 1
    ExplicitWidth = 671
    object btnSerialUp: TSpeedButton
      Left = 406
      Top = 2
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
      Left = 446
      Top = 2
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
    object Label3D12: TJSdLabel
      Tag = 1
      Left = 23
      Top = 10
      Width = 30
      Height = 13
      Alignment = taRightJustify
      Caption = #38542'  '#27573
    end
    object btnChgName: TSpeedButton
      Left = 487
      Top = 2
      Width = 40
      Height = 40
      Hint = #26367#25563
      Caption = #26367#25563
      Flat = True
      Glyph.Data = {
        1E010000424D1E010000000000007600000028000000180000000E0000000100
        040000000000A800000000000000000000001000000000000000000000000000
        8000008000000080800080000000800080008080000080808000C0C0C0000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00888888FF8888
        8888FF8888888888800F88888888F00888888888000F88888888F00088888880
        000FFFFFFFFFF000088888000000000000000000008880000000000000000000
        0008700000000000000000000007870000000000000000000078887000087777
        7777800007888887000F88888888F00078888888700F88888888F00788888888
        8778888888888778888888888888888888888888888888888888888888888888
        8888}
      Layout = blGlyphTop
      ParentShowHint = False
      ShowHint = True
      OnClick = btnChgNameClick
    end
    object cboLayerId: TJSdLookupCombo
      Left = 62
      Top = 7
      Width = 140
      Height = 21
      LkDataSource = dsProdLayer
      LkColumnCount = 2
      cboColor = clWindow
      TextSize = 60
      Text = ''
      SelectOnly = False
      SortedOff = False
      Enabled = False
      TabOrder = 0
    end
  end
  object msSelects: TJSdMultSelect
    Left = 0
    Top = 44
    Width = 771
    Height = 344
    Align = alClient
    WidthSource = 0
    HeadingSource = #21487#29992#29289#26009#31278#39006
    HeadingTarget = #24050#36984#29289#26009#31278#39006
    ColorSource = clWindow
    ColorTarget = clWindow
    ToolAlign = taCenter
    SortTypeSource = stNone
    SortTypeTarget = stNone
    RowSelectSource = False
    RowSelectTarget = False
    SourceColumns = <
      item
        Caption = #39006#21029#21517#31281
        Width = 100
      end
      item
        Caption = #20839#23652
        Width = 60
      end
      item
        Caption = #39006#21029
      end
      item
        Caption = #20839#23652#21517#31281
        Width = 150
      end>
    TargetColumns = <
      item
        Caption = #29289#26009#21517#31281
        Width = 100
      end
      item
        Caption = #20839#23652
        Width = 60
      end
      item
        Caption = #39006#21029
      end
      item
        Caption = #20839#23652#21517#31281
        Width = 150
      end>
    Distinct = True
    SelectMode = smCopy
    DataSourceSource = dsMatClass
    DataSourceTarget = dsTmpPressDtl
    SetupList = slSource
    ExplicitWidth = 671
  end
  object qryMatClass: TADOQuery
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'exec EMOdMatClassSelectEx 1')
    Left = 28
    Top = 106
  end
  object dsMatClass: TDataSource
    DataSet = qryMatClass
    Left = 56
    Top = 106
  end
  object qryTmpPressDtl: TADOQuery
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'TmpId'
        DataType = ftString
        Size = 12
        Value = Null
      end
      item
        Name = 'LayerId'
        DataType = ftString
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdPressDefault :TmpId, :LayerId')
    Left = 212
    Top = 78
  end
  object dsTmpPressDtl: TDataSource
    DataSet = qryTmpPressDtl
    Left = 240
    Top = 78
  end
  object qryProdLayer: TADOQuery
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'SELECT Distinct LayerId, LayerName'
      'FROM dbo.EMOdProdLayer(nolock)'
      ''
      ' ')
    Left = 84
    Top = 174
    object qryProdLayerLayerId: TStringField
      FieldName = 'LayerId'
      Size = 8
    end
    object qryProdLayerLayerName: TWideStringField
      FieldName = 'LayerName'
      Size = 24
    end
  end
  object dsProdLayer: TDataSource
    DataSet = qryProdLayer
    Left = 112
    Top = 174
  end
end

inherited dlgTmpRouteSet: TdlgTmpRouteSet
  Caption = #29983#29986#36884#31243#35373#23450
  ClientHeight = 504
  ClientWidth = 387
  ExplicitWidth = 395
  ExplicitHeight = 531
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlTool: TPanel
    Top = 452
    Width = 387
    ExplicitTop = 452
    ExplicitWidth = 387
    inherited Panel2: TPanel
      Left = 270
      ExplicitLeft = 270
    end
  end
  object pnlUpDown: TPanel
    Left = 0
    Top = 0
    Width = 387
    Height = 97
    Align = alTop
    TabOrder = 1
    object btnSerialUp: TSpeedButton
      Left = 299
      Top = 54
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
      Left = 339
      Top = 54
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
    object Label1: TLabel
      Left = 12
      Top = 8
      Width = 48
      Height = 13
      Caption = #36215#22987#35069#31243
    end
    object Label2: TLabel
      Left = 12
      Top = 30
      Width = 48
      Height = 13
      Caption = #25130#27490#35069#31243
    end
    object Label3: TLabel
      Left = 138
      Top = 8
      Width = 80
      Height = 13
      Caption = #35069#31243#20195#30908'('#27169#31946')'
    end
    object Label4: TLabel
      Left = 138
      Top = 30
      Width = 80
      Height = 13
      Caption = #35069#31243#21517#31281'('#27169#31946')'
    end
    object btnSearch: TSpeedButton
      Left = 299
      Top = 5
      Width = 77
      Height = 40
      Hint = #37325#26032#25628#23563
      Caption = #37325#26032#25628#23563
      Flat = True
      Layout = blGlyphTop
      ParentShowHint = False
      ShowHint = True
      OnClick = btnSearchClick
    end
    object edtBProc: TEdit
      Left = 63
      Top = 5
      Width = 66
      Height = 21
      TabOrder = 0
    end
    object edtEProc: TEdit
      Left = 63
      Top = 27
      Width = 66
      Height = 21
      TabOrder = 1
    end
    object edtProcLike: TEdit
      Left = 221
      Top = 5
      Width = 66
      Height = 21
      TabOrder = 2
    end
    object edtProcNameLike: TEdit
      Left = 221
      Top = 27
      Width = 66
      Height = 21
      TabOrder = 3
    end
  end
  object msSelects: TJSdMultSelect
    Left = 0
    Top = 97
    Width = 387
    Height = 355
    Align = alClient
    WidthSource = 0
    HeadingSource = #30446#21069#24288#20839#35069#31243
    HeadingTarget = #36884#31243#24049#36984#35069#31243
    ColorSource = clWindow
    ColorTarget = clWindow
    ToolAlign = taCenter
    SortTypeSource = stNone
    SortTypeTarget = stNone
    RowSelectSource = False
    RowSelectTarget = False
    SourceColumns = <
      item
        Caption = #20195#30908
      end
      item
        Caption = #35069#31243#21517#31281
        Width = 100
      end>
    TargetColumns = <
      item
        Caption = #20195#30908
      end
      item
        Caption = #35069#31243#21517#31281
        Width = 100
      end>
    Distinct = True
    SelectMode = smMove
    DataSourceSource = dsProcBasic
    DataSourceTarget = dsTmpPressDtl
    SetupList = slSource
  end
  object qryTmpRouteDtl: TADOQuery
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'TmpId'
        DataType = ftString
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'SELECT t1.ProcCode, t2.ProcName'
      'FROM dbo.EMOdTmpRouteDtl t1(nolock),'
      '        dbo.EMOdProcInfo t2(nolock)'
      'where t1.ProcCode = t2.ProcCode'
      '   and TmpId=:TmpId'
      'Order by t1.SerialNum'
      ' ')
    Left = 228
    Top = 158
  end
  object dsTmpPressDtl: TDataSource
    DataSet = qryTmpRouteDtl
    Left = 288
    Top = 182
  end
  object qryMas: TADOQuery
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'SerialNum'
        DataType = ftInteger
        Size = 2
        Value = Null
      end>
    SQL.Strings = (
      'Select t1.ProcCode, t2.ProcName'
      'From EMOdProdStyleDtl t1(nolock),'
      '        EMOdProcInfo t2(nolock)'
      'Where t1.ProcCode = t2.ProcCode'
      '   And t1.SerialNum = :SerialNum '
      ' ')
    Left = 141
    Top = 231
  end
  object dsMas: TDataSource
    DataSet = qryMas
    Left = 189
    Top = 231
  end
  object qryProcBasic: TADOQuery
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'BProc'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'EProc'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'ProcLike'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'ProcNameLike'
        DataType = ftWideString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdProcChoice :BProc, :EProc, :ProcLike,'
      ':ProcNameLike')
    Left = 172
    Top = 62
  end
  object dsProcBasic: TDataSource
    DataSet = qryProcBasic
    Left = 200
    Top = 62
  end
end

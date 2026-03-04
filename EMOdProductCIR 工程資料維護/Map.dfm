inherited frmMap: TfrmMap
  Caption = 'frmMap'
  ClientHeight = 430
  ClientWidth = 532
  ExplicitWidth = 540
  ExplicitHeight = 457
  PixelsPerInch = 96
  TextHeight = 13
  object XFlowDrawBox1: TXFlowDrawBox [0]
    Left = 0
    Top = 0
    Width = 532
    Height = 378
    Align = alClient
    Font.Charset = ANSI_CHARSET
    Font.Color = clWindowText
    Font.Height = -21
    Font.Name = #26032#32048#26126#39636
    Font.Style = []
    ParentFont = False
    XRate = 1.000000000000000000
    YRate = 1.000000000000000000
    ExplicitWidth = 345
    ExplicitHeight = 217
  end
  inherited pnlTool: TPanel
    Top = 378
    Width = 532
    ExplicitTop = 378
    ExplicitWidth = 532
    inherited Panel2: TPanel
      Left = 415
      ExplicitLeft = 415
    end
  end
  object qryMapBmp: TADOQuery
    CursorType = ctStatic
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
      'from EMOdProdMap (nolock)'
      'where PartNum =:PartNum'
      '    and Revision =:Revision')
    Left = 54
    Top = 22
    object qryMapBmpSerialNum: TWordField
      FieldName = 'SerialNum'
    end
  end
  object qryExec: TADOQuery
    EnableBCD = False
    Parameters = <>
    Left = 120
    Top = 24
  end
  object qryMapBmp2: TADOQuery
    CursorType = ctStatic
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
      'Select  *'
      'from EMOdProdLayer (nolock)'
      'where PartNum =:PartNum'
      '    and Revision =:Revision')
    Left = 94
    Top = 166
    object qryMapBmp2LayerId: TStringField
      FieldName = 'LayerId'
      Size = 10
    end
  end
end

inherited dlgNotValueShow: TdlgNotValueShow
  Caption = #26410#37749#20837#20540#20043#24517#35201#27396#20301
  ClientHeight = 555
  ClientWidth = 436
  ExplicitWidth = 444
  ExplicitHeight = 582
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlTool: TPanel
    Top = 503
    Width = 436
    ExplicitTop = 503
    ExplicitWidth = 436
    inherited Panel2: TPanel
      Left = 319
      ExplicitLeft = 319
    end
    object Panel1: TPanel
      Left = 0
      Top = 0
      Width = 319
      Height = 52
      Align = alClient
      BevelInner = bvRaised
      BevelOuter = bvLowered
      Caption = #36884#31243#24050#35722#26356','#35531#35722#26356#29256#24207
      Font.Charset = CHINESEBIG5_CHARSET
      Font.Color = clRed
      Font.Height = -12
      Font.Name = #32048#26126#39636
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 1
      Visible = False
    end
  end
  object DBMemo1: TDBMemo
    Left = 0
    Top = 0
    Width = 436
    Height = 503
    Align = alClient
    DataField = 'S'
    DataSource = DataSource1
    ReadOnly = True
    TabOrder = 1
    ExplicitLeft = 192
    ExplicitTop = 264
    ExplicitWidth = 185
    ExplicitHeight = 89
  end
  object qryNotValueShow: TADOQuery
    CursorType = ctStatic
    LockType = ltBatchOptimistic
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
      'Exec EMOdNotValueShow'
      ':PartNum,'
      ':Revision'
      ' ')
    Left = 56
    Top = 88
    object qryNotValueShowIsError: TIntegerField
      FieldName = 'IsError'
    end
    object qryNotValueShowS: TStringField
      FieldName = 'S'
      Size = 8000
    end
  end
  object DataSource1: TDataSource
    DataSet = qryNotValueShow
    Left = 56
    Top = 128
  end
  object qryCheckLayerRoute: TADOQuery
    CursorType = ctStatic
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
      'exec EMOdCheckLayerRoute :PartNum, :Revision'
      ''
      ' ')
    Left = 56
    Top = 192
    object qryCheckLayerRouteS: TIntegerField
      FieldName = 'S'
    end
  end
end

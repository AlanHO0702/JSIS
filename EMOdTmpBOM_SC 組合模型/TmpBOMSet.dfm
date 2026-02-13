inherited dlgTmpBOMSet: TdlgTmpBOMSet
  Caption = 'dlgTmpBOMSet'
  ClientHeight = 541
  ClientWidth = 617
  ExplicitWidth = 625
  ExplicitHeight = 568
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlTool: TPanel
    Top = 489
    Width = 617
    ExplicitTop = 489
    ExplicitWidth = 617
    inherited Panel2: TPanel
      Left = 500
      ExplicitLeft = 500
    end
  end
  object mmoData: TMemo
    Left = 257
    Top = 36
    Width = 360
    Height = 417
    Align = alRight
    TabOrder = 1
    Visible = False
    OnClick = mmoDataClick
  end
  object pnlSetPress: TPanel
    Left = 0
    Top = 0
    Width = 617
    Height = 36
    Align = alTop
    BevelInner = bvRaised
    BevelOuter = bvLowered
    ParentColor = True
    TabOrder = 2
    object btnLayer: TJSdLabel
      Left = 12
      Top = 10
      Width = 24
      Height = 13
      Caption = #23652#25976
    end
    object btnPress: TJSdLabel
      Left = 94
      Top = 10
      Width = 24
      Height = 13
      Caption = #38542#25976
    end
    object Label1: TLabel
      Left = 257
      Top = 10
      Width = 48
      Height = 13
      Caption = #23652#21029#21517#31281
    end
    object spnLayer: TSpinEdit
      Left = 40
      Top = 6
      Width = 40
      Height = 22
      EditorEnabled = False
      MaxValue = 50
      MinValue = 4
      TabOrder = 0
      Value = 12
      OnChange = spnLayerChange
    end
    object spnPress: TSpinEdit
      Left = 121
      Top = 6
      Width = 40
      Height = 22
      EditorEnabled = False
      MaxValue = 50
      MinValue = 2
      TabOrder = 1
      Value = 6
      OnChange = spnPressChange
    end
    object edtOriName: TEdit
      Left = 308
      Top = 7
      Width = 121
      Height = 21
      TabOrder = 2
    end
    object Button1: TButton
      Left = 433
      Top = 5
      Width = 75
      Height = 25
      Caption = #20462#25913
      TabOrder = 3
      OnClick = Button1Click
    end
  end
  object pnlAll: TPanel
    Left = 0
    Top = 36
    Width = 257
    Height = 417
    Align = alClient
    BevelOuter = bvNone
    ParentColor = True
    PopupMenu = popTools
    TabOrder = 3
  end
  object pnlPress: TPanel
    Left = 0
    Top = 453
    Width = 617
    Height = 36
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    ParentColor = True
    TabOrder = 4
  end
  object qryTmpBOMDtl: TADOQuery
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'TmpId0'
        DataType = ftString
        Size = 12
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdTmpBomGetData  :TmpId0')
    Left = 216
    Top = 28
  end
  object qryTmpBOMIns: TADOQuery
    Parameters = <
      item
        Name = 'TmpId'
        DataType = ftString
        Size = 12
        Value = Null
      end
      item
        Name = 'IssLayer'
        DataType = ftString
        Size = 8
        Value = Null
      end
      item
        Name = 'Degree'
        DataType = ftInteger
        Size = 4
        Value = Null
      end
      item
        Name = 'FL'
        DataType = ftInteger
        Size = 4
        Value = Null
      end
      item
        Name = 'EL'
        DataType = ftInteger
        Size = 4
        Value = Null
      end
      item
        Name = 'AftFL'
        DataType = ftInteger
        Size = 4
        Value = Null
      end
      item
        Name = 'AftEL'
        DataType = ftInteger
        Size = 4
        Value = Null
      end
      item
        Name = 'LayerName'
        DataType = ftWideString
        NumericScale = 255
        Precision = 255
        Size = 48
        Value = Null
      end>
    SQL.Strings = (
      
        'exec EMOdTmpBomIns :TmpId, :IssLayer, :Degree, :FL, :EL,:AftFL, ' +
        ':AftEL,'
      ':LayerName')
    Left = 320
    Top = 92
  end
  object popTools: TPopupMenu
    Left = 288
    Top = 92
    object N1: TMenuItem
      Caption = #31227#38500
      OnClick = N1Click
    end
    object N2: TMenuItem
      Caption = #30059#38754#28165#38500
      OnClick = N2Click
    end
  end
  object qryCheck: TADOQuery
    Parameters = <
      item
        Name = 'TmpId'
        DataType = ftString
        Size = 2
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdCheckTmpBOMSet :TmpId'
      ''
      ' ')
    Left = 304
    Top = 148
  end
end

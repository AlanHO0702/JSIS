inherited frmProdECNPaper: TfrmProdECNPaper
  Caption = 'frmProdECNPaper'
  ClientHeight = 563
  ExplicitHeight = 590
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlInfo: TPanel
    Top = 539
    ExplicitTop = 539
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 528
    ExplicitTop = 528
  end
  inherited pgeBwsDtl: TPageControl
    Height = 485
    ExplicitHeight = 485
    inherited tabBrowse: TTabSheet
      ExplicitLeft = 4
      ExplicitTop = 6
      ExplicitWidth = 1020
      ExplicitHeight = 475
      inherited gridBrowse: TJSdDBGrid
        Height = 475
        ExplicitHeight = 475
      end
    end
    inherited tabDetail: TTabSheet
      ExplicitLeft = 4
      ExplicitTop = 6
      ExplicitWidth = 1020
      ExplicitHeight = 475
      inherited pgeMaster: TPageControl
        inherited tbshtMaster1: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
        inherited tbshtMaster2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
        inherited tbshtMaster3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
        inherited tbshtMaster4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
        inherited tbshtAttach: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 146
        end
      end
      inherited pgeDetail: TPageControl
        Height = 265
        ExplicitHeight = 265
        inherited tbshtDetail1: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 233
          inherited gridDetail1: TJSdDBGrid
            Height = 233
            Visible = False
            ExplicitHeight = 233
          end
          object ScrollBox1: TScrollBox
            Left = 0
            Top = 0
            Width = 1012
            Height = 233
            Align = alClient
            BevelInner = bvNone
            BevelOuter = bvNone
            TabOrder = 1
            object JSdLabel1: TJSdLabel
              Left = 50
              Top = 12
              Width = 28
              Height = 13
              Caption = #21697#34399
              DataField = 'PartNum'
              DataSource = dsDetail1
            end
            object JSdLabel2: TJSdLabel
              Left = 281
              Top = 12
              Width = 42
              Height = 13
              Caption = #21407#29256#24207
              DataField = 'Revision'
              DataSource = dsDetail1
            end
            object Label3: TJSdLabel
              Left = 281
              Top = 282
              Width = 42
              Height = 13
              Caption = #26032#29256#24207
              DataField = 'NewRevision'
              DataSource = dsDetail1
            end
            object Label7: TJSdLabel
              Left = 274
              Top = 305
              Width = 49
              Height = 13
              Caption = 'PNL'#25976#37327
              DataField = 'PNLCount'
              DataSource = dsDetail1
            end
            object Label5: TJSdLabel
              Left = 36
              Top = 282
              Width = 42
              Height = 13
              Caption = #26032#21697#34399
              DataField = 'NewPartNum'
              DataSource = dsDetail1
            end
            object Label6: TJSdLabel
              Left = 29
              Top = 305
              Width = 49
              Height = 13
              Caption = 'PCS'#25976#37327
              DataField = 'PCSCount'
              DataSource = dsDetail1
            end
            object Label10: TJSdLabel
              Left = 22
              Top = 328
              Width = 56
              Height = 13
              Caption = #20462#25913#20027#26088
              DataField = 'leitmotif'
              DataSource = dsDetail1
            end
            object Label4: TJSdLabel
              Left = 22
              Top = 351
              Width = 56
              Height = 13
              Caption = #20462#25913#20839#23481
              DataField = 'UpdateContent'
              DataSource = dsDetail1
            end
            object Label8: TJSdLabel
              Left = 22
              Top = 465
              Width = 56
              Height = 13
              Caption = #21332#36774#20839#23481
              DataField = 'Helpdictionary'
              DataSource = dsDetail1
            end
            object Label9: TJSdLabel
              Left = 29
              Top = 585
              Width = 49
              Height = 13
              Caption = #23565#25033'CCN'
              DataField = 'SourNum'
              DataSource = dsDetail1
            end
            object JSdLabel3: TJSdLabel
              Left = 22
              Top = 609
              Width = 56
              Height = 13
              Caption = #19979#26009#29256#26412
              DataField = 'DefaultRev'
              DataSource = dsDetail1
            end
            object DBEdit1: TDBEdit
              Left = 80
              Top = 9
              Width = 183
              Height = 21
              DataField = 'PartNum'
              DataSource = dsDetail1
              TabOrder = 0
              OnExit = DBEdit1Exit
            end
            object DBEdit2: TDBEdit
              Left = 323
              Top = 9
              Width = 110
              Height = 21
              DataField = 'Revision'
              DataSource = dsDetail1
              TabOrder = 1
              OnExit = DBEdit1Exit
            end
            object DBEdit3: TDBEdit
              Left = 323
              Top = 279
              Width = 110
              Height = 21
              DataField = 'NewRevision'
              DataSource = dsDetail1
              TabOrder = 2
            end
            object DBEdit8: TDBEdit
              Left = 323
              Top = 302
              Width = 110
              Height = 21
              DataField = 'PNLCount'
              DataSource = dsDetail1
              TabOrder = 3
            end
            object DBEdit4: TDBEdit
              Left = 80
              Top = 279
              Width = 183
              Height = 21
              DataField = 'NewPartNum'
              DataSource = dsDetail1
              TabOrder = 4
            end
            object DBEdit7: TDBEdit
              Left = 80
              Top = 302
              Width = 110
              Height = 21
              DataField = 'PCSCount'
              DataSource = dsDetail1
              TabOrder = 5
            end
            object DBEdit5: TDBEdit
              Left = 80
              Top = 325
              Width = 525
              Height = 21
              DataField = 'leitmotif'
              DataSource = dsDetail1
              TabOrder = 6
            end
            object DBMemo1: TDBMemo
              Left = 80
              Top = 348
              Width = 525
              Height = 114
              DataField = 'UpdateContent'
              DataSource = dsDetail1
              TabOrder = 7
            end
            object DBMemo2: TDBMemo
              Left = 80
              Top = 465
              Width = 525
              Height = 114
              DataField = 'Helpdictionary'
              DataSource = dsDetail1
              TabOrder = 8
            end
            object DBEdit10: TDBEdit
              Left = 80
              Top = 582
              Width = 121
              Height = 21
              Color = clScrollBar
              DataField = 'SourNum'
              DataSource = dsDetail1
              Enabled = False
              TabOrder = 9
            end
            object Panel2: TPanel
              Left = 226
              Top = 582
              Width = 379
              Height = 82
              BevelOuter = bvNone
              TabOrder = 10
            end
            object GroupBox1: TGroupBox
              Left = 18
              Top = 32
              Width = 665
              Height = 244
              Caption = #23660#24615#26126#32048
              TabOrder = 11
              object Label12: TJSdLabel
                Left = 32
                Top = 19
                Width = 28
                Height = 13
                Caption = #20998#39006
              end
              object Label11: TJSdLabel
                Left = 4
                Top = 39
                Width = 56
                Height = 12
                Alignment = taRightJustify
                AutoSize = False
                Caption = #29289#26009#23660#24615
                Visible = False
              end
              object btnGenNum: TSpeedButton
                Left = 62
                Top = 190
                Width = 91
                Height = 25
                Caption = #29986#29983#26009#34399
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
                OnClick = btnGenNumClick
              end
              object Label13: TJSdLabel
                Left = 23
                Top = 222
                Width = 37
                Height = 12
                AutoSize = False
                Caption = #21697#21517
              end
              object Label14: TJSdLabel
                Left = 370
                Top = 222
                Width = 182
                Height = 13
                Caption = '('#35531#22312#29986#29983#26009#34399#24460#25165#36664#20837#21697#21517')'
              end
              object cboSetClass: TJSdLookupCombo
                Left = 62
                Top = 15
                Width = 419
                Height = 21
                DataSource = dsDetail1
                DataField = 'MatClass'
                LkDataSource = dsSetNum
                LkColumnCount = 0
                cboColor = clWindow
                TextSize = 100
                Text = ''
                SelectOnly = False
                SortedOff = False
                Enabled = False
                TabOrder = 0
              end
              object grdAddData: TwwDBGrid
                Left = 62
                Top = 38
                Width = 419
                Height = 149
                ControlType.Strings = (
                  'A1;CustomEdit;cboSubAccId;F'
                  'DtlNumId;CustomEdit;cboDtlNumId;F'
                  'IsHand;CheckBox;1;0'
                  'IsMust;CheckBox;1;0')
                Selected.Strings = (
                  'DtlNumId'#9'12'#9#36984#25799#36039#26009#9#9
                  'DtlNumName'#9'24'#9#21517#31281#9#9
                  'NumId'#9'4'#9#23660#24615#9#9
                  'NumName'#9'24'#9#23660#24615#21517#31281#9#9
                  'IsHand'#9'8'#9#25163#21205#32102#20540#9#9
                  'IsMust'#9'8'#9#38656#35201#26377#20540#9#9)
                IniAttributes.Delimiter = ';;'
                IniAttributes.UnicodeIniFile = False
                TitleColor = clBtnFace
                FixedCols = 0
                ShowHorzScrollBar = True
                DataSource = dsAddData
                TabOrder = 1
                TitleAlignment = taLeftJustify
                TitleFont.Charset = ANSI_CHARSET
                TitleFont.Color = clWindowText
                TitleFont.Height = -13
                TitleFont.Name = #32048#26126#39636
                TitleFont.Style = []
                TitleLines = 1
                TitleButtons = False
              end
              object cboDtlNumId: TwwDBLookupCombo
                Left = 233
                Top = 116
                Width = 82
                Height = 21
                DropDownAlignment = taLeftJustify
                Selected.Strings = (
                  'DtlNumId'#9'12'#9'DtlNumId'#9'F'#9
                  'DtlNumName'#9'24'#9'DtlNumName'#9'F'#9)
                DataField = 'DtlNumId'
                DataSource = dsAddData
                LookupTable = qrySetNumSubDtl2
                LookupField = 'DtlNumId'
                TabOrder = 2
                AutoDropDown = False
                ShowButton = True
                PreciseEditRegion = False
                AllowClearKey = False
                OnBeforeDropDown = cboDtlNumIdBeforeDropDown
              end
              object edtMatName: TDBEdit
                Left = 62
                Top = 219
                Width = 295
                Height = 21
                DataField = 'MatName'
                DataSource = dsDetail1
                TabOrder = 3
              end
            end
            object DBCheckBox1: TDBCheckBox
              Left = 80
              Top = 608
              Width = 25
              Height = 17
              DataField = 'DefaultRev'
              DataSource = dsDetail1
              TabOrder = 12
              ValueChecked = '1'
              ValueUnchecked = '0'
            end
          end
        end
        inherited tbshtDetail2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 233
          inherited gridDetail2: TJSdDBGrid
            Height = 233
            ExplicitHeight = 233
          end
        end
        inherited tbshtDetail3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 233
          inherited gridDetail3: TJSdDBGrid
            Height = 233
            ExplicitHeight = 233
          end
        end
        inherited tbshtDetail4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 233
          inherited gridDetail4: TJSdDBGrid
            Height = 233
            ExplicitHeight = 233
          end
        end
        inherited tbshtDetail5: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 233
          inherited gridDetail5: TJSdDBGrid
            Height = 233
            ExplicitHeight = 233
          end
        end
        inherited tbshtDetail6: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 233
          inherited gridDetail6: TJSdDBGrid
            Height = 233
            ExplicitHeight = 233
          end
        end
        inherited tbshtDetail7: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 233
          inherited gridDetail7: TJSdDBGrid
            Height = 233
            ExplicitHeight = 233
          end
        end
        inherited tbshtDetail8: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 1012
          ExplicitHeight = 233
          inherited gridDetail8: TJSdDBGrid
            Height = 233
            ExplicitHeight = 233
          end
        end
      end
      inherited pnlTempBasDLLBottom: TPanel
        Top = 448
        ExplicitTop = 448
      end
    end
  end
  inherited qryExec: TADOQuery
    Left = 680
    Top = 128
  end
  inherited qryGetTranData: TADOQuery
    Left = 648
    Top = 128
  end
  inherited qryBrowse: TJSdTable
    Left = 678
    Top = 58
  end
  inherited qryDetail1: TJSdTable
    Left = 518
    Top = 150
  end
  inherited dsBrowse: TDataSource
    Left = 650
    Top = 58
  end
  inherited dsDetail1: TDataSource
    Left = 522
    Top = 192
  end
  inherited qryDetail2: TJSdTable
    Left = 486
    Top = 150
  end
  inherited qryDetail3: TJSdTable
    Top = 150
  end
  inherited qryDetail4: TJSdTable
    Left = 422
    Top = 150
  end
  inherited qryDetail5: TJSdTable
    Left = 390
    Top = 150
  end
  inherited qryDetail6: TJSdTable
    Left = 358
    Top = 150
  end
  inherited qryDetail7: TJSdTable
    Left = 326
    Top = 150
  end
  inherited qryDetail8: TJSdTable
    Left = 294
    Top = 150
  end
  inherited dsDetail2: TDataSource
    Left = 490
    Top = 192
  end
  inherited dsDetail3: TDataSource
    Top = 192
  end
  inherited dsDetail4: TDataSource
    Left = 426
    Top = 192
  end
  inherited dsDetail5: TDataSource
    Left = 394
    Top = 192
  end
  inherited dsDetail6: TDataSource
    Left = 362
    Top = 190
  end
  inherited dsDetail7: TDataSource
    Left = 330
    Top = 190
  end
  inherited dsDetail8: TDataSource
    Left = 298
    Top = 190
  end
  inherited pmuPaperPaper: TJSdPopupMenu
    Left = 576
    Top = 56
  end
  inherited pwgSaveToExcel: TJSdGrid2Excel
    Left = 544
    Top = 59
  end
  inherited qryExec2: TADOQuery
    Left = 576
    Top = 120
  end
  object qrySetNum: TADOQuery
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'Select t1.MatClass, t1.ClassName, '
      #9'MB=ISNULL(t1.MB, 0),'
      #9'IsForEMO=ISNULL(t2.IsForEMO, 0)'
      'From dbo.MINdMatClass t1(nolock),'
      #9'dbo.MGNdSetNumMain t2(nolock)'
      'where t1.MatClass=t2.SetClass'
      'order by t1.MatClass')
    Left = 448
    Top = 368
    object qrySetNumMatClass: TStringField
      FieldName = 'MatClass'
      FixedChar = True
      Size = 8
    end
    object qrySetNumClassName: TWideStringField
      FieldName = 'ClassName'
      Size = 36
    end
  end
  object dsSetNum: TwwDataSource
    DataSet = qrySetNum
    Left = 476
    Top = 368
  end
  object tblAddData: TJSdTable
    CursorType = ctStatic
    LockType = ltReadOnly
    BeforeInsert = tblAddDataBeforeInsert
    DataSource = dsBrowse
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paSigned]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'select * from dbo.EMOdECNSetNumAddData'
      'where PaperNum = :PaperNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'SpId'
    MasterSource = dsBrowse
    Left = 441
    Top = 428
    object tblAddDataDtlNumId: TStringField
      DisplayLabel = #36984#25799#36039#26009
      DisplayWidth = 12
      FieldName = 'DtlNumId'
      OnValidate = tblAddDataDtlNumIdValidate
      Size = 12
    end
    object tblAddDataDtlNumName: TWideStringField
      DisplayLabel = #21517#31281
      DisplayWidth = 24
      FieldName = 'DtlNumName'
      Size = 24
    end
    object tblAddDataNumId: TStringField
      DisplayLabel = #23660#24615
      DisplayWidth = 4
      FieldName = 'NumId'
      FixedChar = True
      Size = 1
    end
    object tblAddDataNumName: TWideStringField
      DisplayLabel = #23660#24615#21517#31281
      DisplayWidth = 24
      FieldName = 'NumName'
      Size = 24
    end
    object tblAddDataIsHand: TIntegerField
      DisplayLabel = #25163#21205#32102#20540
      DisplayWidth = 8
      FieldName = 'IsHand'
    end
    object tblAddDataIsMust: TIntegerField
      DisplayLabel = #38656#35201#26377#20540
      DisplayWidth = 8
      FieldName = 'IsMust'
    end
    object tblAddDataEnCode: TStringField
      DisplayLabel = #23660#24615#32232#30908
      DisplayWidth = 24
      FieldName = 'EnCode'
      Visible = False
      Size = 24
    end
    object tblAddDataSetClass: TStringField
      DisplayWidth = 12
      FieldName = 'SetClass'
      Visible = False
      Size = 12
    end
  end
  object dsAddData: TDataSource
    DataSet = tblAddData
    Left = 469
    Top = 432
  end
  object qryDtlNumName: TADOQuery
    CursorType = ctStatic
    Parameters = <>
    SQL.Strings = (
      'Select *'
      'From MGNdSetNumSubDtl (nolock)'
      'Order By SetClass')
    Left = 40
    Top = 120
    object StringField1: TStringField
      DisplayWidth = 12
      FieldName = 'DtlNumId'
      Size = 12
    end
    object WideStringField1: TWideStringField
      DisplayWidth = 24
      FieldName = 'DtlNumName'
      Size = 24
    end
    object StringField2: TStringField
      DisplayWidth = 12
      FieldName = 'SetClass'
      Visible = False
      Size = 12
    end
    object StringField3: TStringField
      DisplayWidth = 1
      FieldName = 'NumId'
      Visible = False
      FixedChar = True
      Size = 1
    end
    object StringField4: TStringField
      DisplayWidth = 24
      FieldName = 'EnCode'
      Visible = False
      Size = 24
    end
  end
  object qrySetNumSubDtl2: TADOQuery
    CursorType = ctStatic
    Parameters = <
      item
        Name = 'SetClass'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 12
        Value = Null
      end
      item
        Name = 'NumId'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 1
        Value = Null
      end>
    SQL.Strings = (
      'Select *'
      'From MGNdSetNumSubDtl (nolock)'
      'where SetClass = :SetClass'
      'and NumId = :NumId')
    Left = 40
    Top = 168
    object StringField5: TStringField
      DisplayWidth = 12
      FieldName = 'DtlNumId'
      Size = 12
    end
    object WideStringField2: TWideStringField
      DisplayWidth = 24
      FieldName = 'DtlNumName'
      Size = 24
    end
    object StringField6: TStringField
      DisplayWidth = 24
      FieldName = 'EnCode'
      Visible = False
      Size = 24
    end
    object StringField7: TStringField
      DisplayWidth = 12
      FieldName = 'SetClass'
      Visible = False
      Size = 12
    end
    object StringField8: TStringField
      DisplayWidth = 1
      FieldName = 'NumId'
      Visible = False
      FixedChar = True
      Size = 1
    end
  end
end

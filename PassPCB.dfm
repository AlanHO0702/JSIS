inherited frmFMEdPassPCB: TfrmFMEdPassPCB
  Caption = 'frmFMEdPassPCB'
  ClientHeight = 675
  ClientWidth = 1016
  Font.Height = -12
  ExplicitTop = -17
  ExplicitWidth = 1024
  ExplicitHeight = 702
  PixelsPerInch = 96
  TextHeight = 12
  inherited pnlInfo: TPanel
    Top = 655
    Width = 1016
    ExplicitTop = 655
    ExplicitWidth = 1016
    inherited btnGetParams: TSpeedButton
      ExplicitLeft = 1
      ExplicitTop = 5
      ExplicitHeight = 20
    end
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 633
    Width = 1016
    ExplicitTop = 633
    ExplicitWidth = 1016
  end
  object page_PassPCB_120827A: TPageControl [2]
    Left = 0
    Top = 0
    Width = 1016
    Height = 633
    ActivePage = tbsht_PassPCB_Lot
    Align = alTop
    TabOrder = 2
    OnChange = page_PassPCB_120827AChange
    object tbsht_PassPCB_Lot: TTabSheet
      Caption = #21934#25209#36942#24115
      object pnl_Center: TPanel
        Left = 0
        Top = 0
        Width = 1008
        Height = 605
        Align = alClient
        BevelOuter = bvNone
        TabOrder = 0
        object pnlXContainer: TPanel
          Left = 0
          Top = 0
          Width = 1008
          Height = 30
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 0
          object pnlDetail: TPanel
            Left = 0
            Top = 0
            Width = 914
            Height = 30
            Align = alLeft
            BevelOuter = bvNone
            TabOrder = 0
            object edt_DtlNotes: TDBText
              Left = 331
              Top = 8
              Width = 158
              Height = 26
              DataField = 'Notes'
              DataSource = dsDetail
            end
            object btnProcParamImport: TSpeedButton
              Left = 245
              Top = 0
              Width = 80
              Height = 30
              Align = alLeft
              Caption = #21295#20837#21443#25976#26126#32048
              OnClick = btnProcParamImportClick
              ExplicitLeft = 192
              ExplicitTop = 1
              ExplicitHeight = 28
            end
            object Label14: TLabel
              Left = 520
              Top = 12
              Width = 24
              Height = 12
              Caption = #21934#34399
            end
            object Label15: TLabel
              Left = 663
              Top = 12
              Width = 24
              Height = 12
              Caption = #38917#27425
            end
            object btnShowAttachment: TSpeedButton
              Left = 185
              Top = 0
              Width = 60
              Height = 30
              Align = alLeft
              Caption = #39023#31034#38468#20214
              Enabled = False
              Font.Charset = ANSI_CHARSET
              Font.Color = clWindowText
              Font.Height = -12
              Font.Name = #32048#26126#39636
              Font.Style = []
              NumGlyphs = 2
              ParentFont = False
              OnClick = btnShowAttachmentClick
              ExplicitLeft = 191
              ExplicitTop = 3
            end
            object btnExecute: TButton
              Left = 125
              Top = 0
              Width = 60
              Height = 30
              Align = alLeft
              Caption = #30906' '#35469
              TabOrder = 6
              OnClick = btnExecuteClick
            end
            object navDetail: TDBNavigator
              Left = 0
              Top = 0
              Width = 50
              Height = 30
              DataSource = dsDetail
              VisibleButtons = [nbInsert, nbDelete]
              Align = alLeft
              TabOrder = 0
              Visible = False
            end
            object navMain: TDBNavigator
              Left = 50
              Top = 0
              Width = 75
              Height = 30
              DataSource = dsMaster
              VisibleButtons = [nbPost, nbCancel, nbRefresh]
              Align = alLeft
              TabOrder = 1
            end
            object chkMOPrint: TCheckBox
              Left = 756
              Top = 0
              Width = 85
              Height = 30
              Align = alRight
              Caption = #21015#21360#22823#24037#21934
              TabOrder = 2
              Visible = False
            end
            object chkPaperPrint: TCheckBox
              Left = 841
              Top = 0
              Width = 73
              Height = 30
              Align = alRight
              Caption = #21934#25818#21015#21360
              Checked = True
              State = cbChecked
              TabOrder = 3
              Visible = False
            end
            object DBEdit11: TDBEdit
              Left = 548
              Top = 8
              Width = 110
              Height = 20
              Color = clBtnFace
              DataField = 'PaperNum'
              DataSource = dsDetail
              ReadOnly = True
              TabOrder = 4
            end
            object DBEdit12: TDBEdit
              Left = 691
              Top = 8
              Width = 25
              Height = 20
              Color = clBtnFace
              DataField = 'Item'
              DataSource = dsDetail
              ReadOnly = True
              TabOrder = 5
            end
          end
          object btnClose: TBitBtn
            Left = 920
            Top = 1
            Width = 72
            Height = 27
            Caption = #38626#38283
            Enabled = False
            Glyph.Data = {
              76010000424D7601000000000000760000002800000020000000100000000100
              04000000000000010000120B0000120B00001000000000000000000000000000
              800000800000008080008000000080008000808000007F7F7F00BFBFBF000000
              FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00330000000000
              03333377777777777F333301BBBBBBBB033333773F3333337F3333011BBBBBBB
              0333337F73F333337F33330111BBBBBB0333337F373F33337F333301110BBBBB
              0333337F337F33337F333301110BBBBB0333337F337F33337F333301110BBBBB
              0333337F337F33337F333301110BBBBB0333337F337F33337F333301110BBBBB
              0333337F337F33337F333301110BBBBB0333337F337FF3337F33330111B0BBBB
              0333337F337733337F333301110BBBBB0333337F337F33337F333301110BBBBB
              0333337F3F7F33337F333301E10BBBBB0333337F7F7F33337F333301EE0BBBBB
              0333337F777FFFFF7F3333000000000003333377777777777333}
            NumGlyphs = 2
            TabOrder = 1
            Visible = False
            OnClick = btnCloseClick
          end
        end
        object pnlMain: TPanel
          Left = 0
          Top = 30
          Width = 1008
          Height = 400
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 1
          object Label3D5: TLabel
            Tag = 1
            Left = 23
            Top = 35
            Width = 48
            Height = 12
            Alignment = taRightJustify
            Caption = #26009'    '#34399
          end
          object Label3D6: TLabel
            Tag = 1
            Left = 271
            Top = 35
            Width = 48
            Height = 12
            Caption = #29256'    '#24207
          end
          object Label3D9: TLabel
            Tag = 1
            Left = 23
            Top = 101
            Width = 48
            Height = 12
            Alignment = taRightJustify
            Caption = #22411'    '#29376
          end
          object Label3D10: TLabel
            Tag = 1
            Left = 23
            Top = 124
            Width = 48
            Height = 12
            Alignment = taRightJustify
            Caption = #21407#22987#25976#37327
          end
          object Label3D11: TLabel
            Tag = 1
            Left = 23
            Top = 79
            Width = 48
            Height = 12
            Alignment = taRightJustify
            Caption = #38542'    '#27573
          end
          object Label3D12: TLabel
            Tag = 1
            Left = 23
            Top = 57
            Width = 48
            Height = 12
            Alignment = taRightJustify
            Caption = #30446#21069#35069#31243
          end
          object Label3D13: TLabel
            Tag = 1
            Left = 271
            Top = 101
            Width = 48
            Height = 12
            Caption = #36942#24115#22411#29376
          end
          object lab_PQnty: TLabel
            Tag = 1
            Left = 5
            Top = 146
            Width = 66
            Height = 12
            Alignment = taRightJustify
            Caption = #36942#24115#25976#37327'XXX'
            Font.Charset = ANSI_CHARSET
            Font.Color = clBlue
            Font.Height = -12
            Font.Name = #32048#26126#39636
            Font.Style = []
            ParentFont = False
          end
          object Label3D15: TLabel
            Tag = 1
            Left = 271
            Top = 79
            Width = 48
            Height = 12
            Caption = #36942#24115#38542#27573
          end
          object Label3D16: TLabel
            Tag = 1
            Left = 271
            Top = 57
            Width = 48
            Height = 12
            Caption = #19979#31449#35069#31243
          end
          object lab_SQnty: TLabel
            Tag = 1
            Left = 11
            Top = 168
            Width = 60
            Height = 12
            Alignment = taRightJustify
            Caption = #25972#29255#22577#24290#25976
            Font.Charset = ANSI_CHARSET
            Font.Color = clRed
            Font.Height = -12
            Font.Name = #32048#26126#39636
            Font.Style = []
            ParentFont = False
          end
          object lab_UQnty: TLabel
            Tag = 1
            Left = 630
            Top = 291
            Width = 48
            Height = 12
            Alignment = taRightJustify
            Caption = #30041#23384#25976#37327
            Visible = False
          end
          object Label1: TLabel
            Left = 420
            Top = 34
            Width = 61
            Height = 12
            AutoSize = False
            Caption = #20801#25910'X'#22577#25976
          end
          object labLotNum: TLabel
            Left = 420
            Top = 71
            Width = 48
            Height = 12
            AutoSize = False
            Color = 16502782
            ParentColor = False
            Visible = False
          end
          object Label2: TLabel
            Left = 271
            Top = 14
            Width = 48
            Height = 12
            AutoSize = False
            Caption = #25490' '#29256' '#25976
          end
          object labLLPCS: TLabel
            Left = 325
            Top = 13
            Width = 89
            Height = 12
            AutoSize = False
            Color = 16502782
            ParentColor = False
          end
          object btnGetLot: TSpeedButton
            Left = 4
            Top = 7
            Width = 73
            Height = 22
            Caption = #21407#22987#25209#34399
            OnClick = btnGetLotClick
          end
          object lab_ScrapStrXOutQnty: TLabel
            Tag = 1
            Left = 340
            Top = 123
            Width = 84
            Height = 12
            Caption = #25972#22577#27798#37559#21934#22577#25976
            Visible = False
          end
          object btnTmp2Xout: TSpeedButton
            Left = 739
            Top = 262
            Width = 97
            Height = 22
            Caption = #24478#22810#22577#26126#32048#36681#20837
            Visible = False
            OnClick = btnTmp2XoutClick
          end
          object lab_TranPQnty: TLabel
            Tag = 1
            Left = 598
            Top = 312
            Width = 66
            Height = 12
            Alignment = taRightJustify
            Caption = #36942#24115#25976#37327'YYY'
            Font.Charset = ANSI_CHARSET
            Font.Color = clWindowText
            Font.Height = -12
            Font.Name = #32048#26126#39636
            Font.Style = []
            ParentFont = False
          end
          object lab_StkVoidQnty: TLabel
            Tag = 1
            Left = 458
            Top = 267
            Width = 60
            Height = 12
            Alignment = taRightJustify
            Caption = #22810#29986#19988#22577#24290
            Font.Charset = ANSI_CHARSET
            Font.Color = clRed
            Font.Height = -12
            Font.Name = #32048#26126#39636
            Font.Style = []
            ParentFont = False
          end
          object lab_TranMatQnty: TLabel
            Tag = 1
            Left = 842
            Top = 267
            Width = 72
            Height = 12
            Alignment = taRightJustify
            Caption = #25563#31639#26448#26009#25976#37327
          end
          object lab_LineId: TLabel
            Left = 420
            Top = 12
            Width = 29
            Height = 12
            AutoSize = False
            Caption = #32218#21029
          end
          object lab_MasterNotes: TLabel
            Tag = 1
            Left = 525
            Top = 185
            Width = 28
            Height = 12
            AutoSize = False
            Caption = #20633#35387
          end
          object lab_GoodPCS: TLabel
            Left = 172
            Top = 123
            Width = 125
            Height = 12
            AutoSize = False
            Caption = '0'
          end
          object btnEquipString: TSpeedButton
            Left = 625
            Top = 263
            Width = 73
            Height = 22
            Caption = #20316#26989#27231#21488
            Visible = False
            OnClick = btnEquipStringClick
          end
          object btnWorkUserString: TSpeedButton
            Left = 680
            Top = 308
            Width = 73
            Height = 22
            Caption = #20316#26989#20154#21729
            Visible = False
            OnClick = btnWorkUserStringClick
          end
          object lab_QC_UserId: TLabel
            Tag = 1
            Left = 715
            Top = 290
            Width = 48
            Height = 12
            Alignment = taRightJustify
            Caption = #27298#39511#20154#21729
            Visible = False
          end
          object Label8: TLabel
            Tag = 1
            Left = 364
            Top = 168
            Width = 60
            Height = 12
            Alignment = taRightJustify
            Caption = #30070#31449#37325#24037#25976
            Visible = False
          end
          object lab_RwkSQnty: TLabel
            Tag = 1
            Left = 11
            Top = 168
            Width = 60
            Height = 12
            Alignment = taRightJustify
            Caption = #37325#24037#22577#24290#25976
            Visible = False
          end
          object lab_RwkSQntySum: TLabel
            Tag = 1
            Left = 340
            Top = 145
            Width = 84
            Height = 12
            Alignment = taRightJustify
            Caption = #32047#31309#37325#24037#22577#24290#25976
            Visible = False
          end
          object lab_RwkPQnty: TLabel
            Tag = 1
            Left = 11
            Top = 146
            Width = 60
            Height = 12
            Alignment = taRightJustify
            Caption = #37325#24037#36942#24115#25976
            Visible = False
          end
          object lblMemoNotes: TLabel
            Tag = 1
            Left = 525
            Top = 0
            Width = 72
            Height = 12
            Alignment = taRightJustify
            Caption = #35069#31243#27880#24847#20107#38917
          end
          object Label16: TLabel
            Tag = 1
            Left = 525
            Top = 60
            Width = 48
            Height = 12
            Alignment = taRightJustify
            Caption = #35069#31243#21443#25976
          end
          object btnGetLotData: TButton
            Left = 228
            Top = 7
            Width = 37
            Height = 22
            Caption = #21295#20837
            TabOrder = 35
            OnClick = btnGetLotDataClick
          end
          object edtPartNum: TDBEdit
            Left = 77
            Top = 31
            Width = 190
            Height = 20
            TabStop = False
            CharCase = ecUpperCase
            Color = 16502782
            DataField = 'PartNum'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 0
          end
          object edtRevision: TDBEdit
            Left = 324
            Top = 30
            Width = 90
            Height = 20
            TabStop = False
            CharCase = ecUpperCase
            Color = 16502782
            DataField = 'Revision'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 1
          end
          object edtPOPName: TDBEdit
            Left = 77
            Top = 97
            Width = 90
            Height = 20
            TabStop = False
            Color = 16502782
            DataField = 'POPName'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 2
          end
          object edtLayerId: TDBEdit
            Left = 77
            Top = 75
            Width = 90
            Height = 20
            TabStop = False
            Color = 16502782
            DataField = 'LayerId'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 3
          end
          object edtProcName: TDBEdit
            Left = 167
            Top = 52
            Width = 100
            Height = 20
            TabStop = False
            BiDiMode = bdLeftToRight
            Color = 16502782
            DataField = 'ProcName'
            DataSource = dsDetail
            ParentBiDiMode = False
            ReadOnly = True
            TabOrder = 4
          end
          object edtQnty: TDBEdit
            Left = 77
            Top = 120
            Width = 90
            Height = 20
            TabStop = False
            Color = 16502782
            DataField = 'Qnty'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 5
          end
          object edtLotNum: TDBEdit
            Left = 78
            Top = 9
            Width = 152
            Height = 20
            CharCase = ecUpperCase
            DataField = 'LotNum'
            DataSource = dsDetail
            TabOrder = 6
            OnClick = edtLotNumClick
            OnKeyDown = edtLotNumKeyDown
          end
          object DBEdit1: TDBEdit
            Left = 324
            Top = 97
            Width = 90
            Height = 20
            TabStop = False
            Color = 16502782
            DataField = 'AftPOPName'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 7
          end
          object DBEdit2: TDBEdit
            Left = 324
            Top = 75
            Width = 90
            Height = 20
            TabStop = False
            Color = 16502782
            DataField = 'AftLayer'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 8
          end
          object DBEdit3: TDBEdit
            Left = 414
            Top = 52
            Width = 100
            Height = 20
            TabStop = False
            BiDiMode = bdLeftToRight
            Color = 16502782
            DataField = 'AftProcName'
            DataSource = dsDetail
            ParentBiDiMode = False
            ReadOnly = True
            TabOrder = 9
          end
          object edtPQnty: TDBEdit
            Left = 77
            Top = 142
            Width = 90
            Height = 20
            DataField = 'PQnty'
            DataSource = dsDetail
            TabOrder = 10
            OnClick = edtPQntyClick
            OnExit = edtPQntyExit
          end
          object edtSQnty: TDBEdit
            Left = 77
            Top = 164
            Width = 90
            Height = 20
            DataField = 'SQnty'
            DataSource = dsDetail
            TabOrder = 12
            OnChange = edtSQntyChange
            OnClick = edtSQntyClick
            OnExit = edtSQntyExit
          end
          object edtUQnty: TDBEdit
            Left = 684
            Top = 288
            Width = 25
            Height = 20
            DataField = 'UQnty'
            DataSource = dsDetail
            TabOrder = 13
            Visible = False
            OnClick = edtUQntyClick
          end
          object pnlDateCode: TPanel
            Left = 1
            Top = 208
            Width = 168
            Height = 25
            TabOrder = 14
            object Label3: TLabel
              Tag = 1
              Left = 15
              Top = 7
              Width = 48
              Height = 12
              Caption = 'DateCode'
            end
            object edtDateCode: TDBEdit
              Left = 76
              Top = 2
              Width = 90
              Height = 20
              DataField = 'DateCode'
              DataSource = dsDetail
              TabOrder = 0
              OnClick = edtXOutXXXClick
            end
          end
          object pnlXOutSum: TPanel
            Left = 808
            Top = 285
            Width = 140
            Height = 26
            TabOrder = 15
            object labXOutXXX: TLabel
              Tag = 1
              Left = 23
              Top = 7
              Width = 96
              Height = 12
              Caption = #26412#31449#26032#22686#21934#22577#25976#37327
            end
            object edtXOutXXX: TDBEdit
              Left = 124
              Top = 3
              Width = 90
              Height = 20
              DataField = 'XOutQnty'
              DataSource = dsDetail
              TabOrder = 0
              OnClick = edtXOutXXXClick
            end
          end
          object pnl_EquipId: TPanel
            Left = 831
            Top = 314
            Width = 117
            Height = 21
            Alignment = taLeftJustify
            BevelOuter = bvNone
            Caption = #35373#20633#20195#34399
            TabOrder = 16
            object JSdLookupCombo1: TJSdLookupCombo
              Left = 52
              Top = 0
              Width = 190
              Height = 21
              DataSource = dsDetail
              DataField = 'EquipId'
              LkDataSource = dsEquipProc
              LkColumnCount = 2
              cboColor = clWindow
              TextSize = 90
              Text = ''
              SelectOnly = False
              SortedOff = False
              TabOrder = 0
              OnDropdown = JSdLookupCombo1Dropdown
            end
          end
          object edtProcCode: TDBEdit
            Left = 77
            Top = 52
            Width = 90
            Height = 20
            TabStop = False
            BiDiMode = bdLeftToRight
            Color = 16502782
            DataField = 'ProcCode'
            DataSource = dsDetail
            ParentBiDiMode = False
            ReadOnly = True
            TabOrder = 17
          end
          object DBEdit5: TDBEdit
            Left = 324
            Top = 52
            Width = 90
            Height = 20
            TabStop = False
            BiDiMode = bdLeftToRight
            Color = 16502782
            DataField = 'AftProc'
            DataSource = dsDetail
            ParentBiDiMode = False
            ReadOnly = True
            TabOrder = 18
          end
          object DBEdit6: TDBEdit
            Left = 167
            Top = 75
            Width = 100
            Height = 20
            TabStop = False
            Color = 16502782
            DataField = 'LayerName'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 19
          end
          object DBEdit7: TDBEdit
            Left = 414
            Top = 75
            Width = 100
            Height = 20
            TabStop = False
            Color = 16502782
            DataField = 'AftLayerName'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 20
          end
          object edt_ScrapStrXOutQnty: TDBEdit
            Left = 430
            Top = 119
            Width = 89
            Height = 20
            DataField = 'ScrapStrXOutQnty'
            DataSource = dsDetail
            TabOrder = 21
            Visible = False
            OnClick = edtXOutXXXClick
            OnExit = edt_ScrapStrXOutQntyExit
          end
          object edtTranPQnty: TDBEdit
            Left = 664
            Top = 309
            Width = 29
            Height = 20
            DataField = 'TranPQnty'
            DataSource = dsDetail
            TabOrder = 11
            OnClick = edtPQntyClick
            OnExit = edtPQntyExit
          end
          object edtStkVoidPQnty: TDBEdit
            Left = 524
            Top = 267
            Width = 90
            Height = 20
            DataField = 'StkVoidPQnty'
            DataSource = dsDetail
            Font.Charset = ANSI_CHARSET
            Font.Color = clBlack
            Font.Height = -12
            Font.Name = #32048#26126#39636
            Font.Style = []
            ParentFont = False
            TabOrder = 22
            OnClick = edtPQntyClick
            OnExit = edtPQntyExit
          end
          object edtTranMatQnty: TDBEdit
            Left = 912
            Top = 267
            Width = 36
            Height = 20
            DataField = 'TranMatQnty'
            DataSource = dsDetail
            TabOrder = 23
            OnClick = edtSQntyClick
            OnExit = edtSQntyExit
          end
          object edt_LineId: TDBEdit
            Left = 448
            Top = 10
            Width = 66
            Height = 20
            TabStop = False
            BiDiMode = bdLeftToRight
            Color = 16502782
            DataField = 'StockId'
            DataSource = dsDetail
            ParentBiDiMode = False
            ReadOnly = True
            TabOrder = 24
          end
          object edtEquipString: TDBEdit
            Left = 698
            Top = 262
            Width = 35
            Height = 20
            DataField = 'EquipString'
            DataSource = dsDetail
            TabOrder = 25
            Visible = False
          end
          object edtWorkUserString: TDBEdit
            Left = 770
            Top = 307
            Width = 32
            Height = 20
            DataField = 'WorkUserString'
            DataSource = dsDetail
            TabOrder = 26
            Visible = False
          end
          object cbo_QC_UserId: TJSdLookupCombo
            Left = 769
            Top = 290
            Width = 33
            Height = 21
            DataSource = dsDetail
            DataField = 'QC_UserId'
            LkDataSource = dsUsers
            LkColumnCount = 2
            cboColor = clWindow
            TextSize = 100
            Text = ''
            NameText = '..'
            SelectOnly = False
            SortedOff = False
            Visible = False
            TabOrder = 27
          end
          object edt_MasterNotes: TDBMemo
            Left = 525
            Top = 200
            Width = 600
            Height = 50
            DataField = 'Notes'
            DataSource = dsMaster
            ScrollBars = ssBoth
            TabOrder = 28
          end
          object edtRevNum: TEdit
            Left = 167
            Top = 97
            Width = 100
            Height = 20
            Color = clMenu
            ReadOnly = True
            TabOrder = 29
          end
          object edtRwkQnty: TDBEdit
            Left = 430
            Top = 164
            Width = 90
            Height = 20
            DataField = 'RwkQnty'
            DataSource = dsDetail
            TabOrder = 30
            Visible = False
            OnClick = edtSQntyClick
            OnExit = edtSQntyExit
          end
          object edtRwkSQnty: TDBEdit
            Left = 77
            Top = 163
            Width = 90
            Height = 20
            DataField = 'RwkScrapQnty'
            DataSource = dsDetail
            TabOrder = 31
            Visible = False
            OnExit = edtSQntyExit
          end
          object DBEdit4: TDBEdit
            Left = 480
            Top = 30
            Width = 34
            Height = 20
            TabStop = False
            BiDiMode = bdLeftToRight
            Color = 16502782
            DataField = 'AllowXOutQnty'
            DataSource = dsAllowXOutQnty
            ParentBiDiMode = False
            ReadOnly = True
            TabOrder = 32
          end
          object edtRwkSQntySum: TDBEdit
            Left = 430
            Top = 141
            Width = 90
            Height = 20
            DataField = 'RwkSQntySum'
            DataSource = dsDetail
            ReadOnly = True
            TabOrder = 33
            Visible = False
            OnExit = edtSQntyExit
          end
          object edtRwkPQnty: TDBEdit
            Left = 77
            Top = 141
            Width = 90
            Height = 20
            DataField = 'RwkPQnty'
            DataSource = dsDetail
            TabOrder = 34
            Visible = False
            OnExit = edtRwkPQntyExit
          end
          object memoNotes: TMemo
            Left = 525
            Top = 18
            Width = 600
            Height = 40
            Lines.Strings = (
              '')
            ReadOnly = True
            TabOrder = 36
          end
          object JSdDBGrid3: TJSdDBGrid
            Left = 525
            Top = 110
            Width = 600
            Height = 70
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            DataSource = dsProcParamDtl
            Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
            ParentShowHint = False
            ShowHint = True
            TabOrder = 37
            TitleAlignment = taLeftJustify
            TitleFont.Charset = ANSI_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -12
            TitleFont.Name = #32048#26126#39636
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = True
            SortColumnClick = stColumnClick
          end
          object Panel6: TPanel
            Left = 525
            Top = 75
            Width = 600
            Height = 35
            BevelInner = bvLowered
            BevelOuter = bvNone
            TabOrder = 38
            object Panel5: TPanel
              Left = 1
              Top = 1
              Width = 598
              Height = 29
              Align = alTop
              BevelOuter = bvLowered
              TabOrder = 0
              object btnParamProcClear: TSpeedButton
                Left = 193
                Top = 1
                Width = 80
                Height = 27
                Align = alLeft
                Caption = #28165' '#38500
                OnClick = btnParamProcClearClick
                ExplicitLeft = 273
                ExplicitHeight = 28
              end
              object DBNavigator3: TDBNavigator
                Left = 1
                Top = 1
                Width = 192
                Height = 27
                DataSource = dsProcParamDtl
                VisibleButtons = [nbFirst, nbPrior, nbNext, nbLast, nbEdit, nbPost, nbCancel, nbRefresh]
                Align = alLeft
                TabOrder = 0
              end
            end
          end
        end
        object Panel4: TPanel
          Left = 0
          Top = 430
          Width = 1008
          Height = 175
          Align = alClient
          BevelOuter = bvNone
          TabOrder = 2
          ExplicitTop = 350
          ExplicitHeight = 315
          object splitter_XOutDefect: TSplitter
            Left = 0
            Top = -4
            Width = 1008
            Height = 4
            Cursor = crVSplit
            Align = alBottom
            Color = clMedGray
            ParentColor = False
            ExplicitTop = 0
          end
          object Panel9: TPanel
            Left = 0
            Top = 0
            Width = 1008
            Height = 175
            Align = alBottom
            TabOrder = 0
            ExplicitTop = 140
            object pnl_XOutDefect: TPanel
              Left = 330
              Top = 1
              Width = 900
              Height = 173
              Align = alLeft
              BevelOuter = bvNone
              Enabled = False
              TabOrder = 0
              object gridXOutDefect: TJSdDBGrid
                Left = 350
                Top = 25
                Width = 350
                Height = 148
                Selected.Strings = (
                  'SerialNum'#9'10'#9'SerialNum'#9#9
                  'ClassId'#9'8'#9'ClassId'#9#9
                  'Lk_ClassName'#9'20'#9'Lk_ClassName'#9#9
                  'DefectId'#9'8'#9'DefectId'#9#9
                  'Lk_DefectName'#9'20'#9'Lk_DefectName'#9#9
                  'DutyProc'#9'8'#9'DutyProc'#9#9
                  'Lk_DutyProcName'#9'20'#9'Lk_DutyProcName'#9#9
                  'Qnty'#9'10'#9'Qnty'#9#9
                  'PaperNum'#9'16'#9'PaperNum'#9#9
                  'Item'#9'10'#9'Item'#9#9
                  'CompanyId'#9'16'#9'CompanyId'#9#9
                  'QntyOri'#9'10'#9'QntyOri'#9#9)
                IniAttributes.Delimiter = ';;'
                IniAttributes.UnicodeIniFile = False
                TitleColor = clBtnFace
                FixedCols = 0
                ShowHorzScrollBar = True
                Align = alLeft
                DataSource = dsXOutDefect
                Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
                ParentShowHint = False
                ShowHint = True
                TabOrder = 0
                TitleAlignment = taLeftJustify
                TitleFont.Charset = ANSI_CHARSET
                TitleFont.Color = clWindowText
                TitleFont.Height = -12
                TitleFont.Name = #32048#26126#39636
                TitleFont.Style = []
                TitleLines = 1
                TitleButtons = True
                Visible = False
                SortColumnClick = stNone
              end
              object Panel1: TPanel
                Left = 0
                Top = 0
                Width = 900
                Height = 25
                Align = alTop
                BevelOuter = bvNone
                Caption = #21934#22577#32570#40670#26126#32048
                TabOrder = 1
                object DBNavigator1: TDBNavigator
                  Left = 0
                  Top = 0
                  Width = 240
                  Height = 25
                  DataSource = dsXOutDefect
                  Align = alLeft
                  TabOrder = 0
                end
              end
              object gridRwkSDefect: TJSdDBGrid
                Left = 0
                Top = 25
                Width = 350
                Height = 148
                Selected.Strings = (
                  'SerialNum'#9'4'#9#24207#34399#9'F'#9
                  'RwkProc'#9'6'#9#25552#20986#31449#9'T'#9
                  'Lk_RwkProcName'#9'15'#9#25552#20986#31449#21517#31281#9'T'#9
                  'ClassId'#9'11'#9#32570#40670#20998#39006#9'F'#9
                  'Lk_ClassName'#9'20'#9#32570#40670#20998#39006#21517#31281#9'F'#9
                  'DefectId'#9'11'#9#32570#40670#20195#34399#9'F'#9
                  'Lk_DefectName'#9'16'#9#32570#40670#21517#31281#9'F'#9
                  'DutyProc'#9'8'#9#36012#20219#35069#31243#9'F'#9
                  'Lk_DutyProcName'#9'15'#9#36012#20219#35069#31243#21517#31281#9'F'#9
                  'QntyOri'#9'8'#9#21407#22987#25976#37327#9'T'#9
                  'Qnty'#9'8'#9#22577#24290#25976#37327#9'F'#9)
                IniAttributes.Delimiter = ';;'
                IniAttributes.UnicodeIniFile = False
                TitleColor = clBtnFace
                FixedCols = 0
                ShowHorzScrollBar = True
                Align = alLeft
                DataSource = dsRwkSDefect
                Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
                ParentShowHint = False
                ShowHint = True
                TabOrder = 2
                TitleAlignment = taLeftJustify
                TitleFont.Charset = ANSI_CHARSET
                TitleFont.Color = clWindowText
                TitleFont.Height = -12
                TitleFont.Name = #32048#26126#39636
                TitleFont.Style = []
                TitleLines = 1
                TitleButtons = True
                Visible = False
                SortColumnClick = stNone
              end
            end
            object Panel7: TPanel
              Left = 1
              Top = 1
              Width = 329
              Height = 173
              Align = alLeft
              TabOrder = 1
              object pnl_FQC_XOut_TOL: TPanel
                Left = 1
                Top = 147
                Width = 327
                Height = 25
                Align = alBottom
                BevelOuter = bvNone
                TabOrder = 0
                Visible = False
                object Label5: TLabel
                  Left = 5
                  Top = 8
                  Width = 36
                  Height = 12
                  Caption = #21934#22577#20489
                end
                object Label6: TLabel
                  Left = 124
                  Top = 8
                  Width = 36
                  Height = 12
                  Caption = #24050#27798#37559
                end
                object Label7: TLabel
                  Left = 232
                  Top = 8
                  Width = 24
                  Height = 12
                  Caption = #39192#38989
                end
                object DBEdit8: TDBEdit
                  Left = 44
                  Top = 4
                  Width = 62
                  Height = 20
                  Color = clMenuBar
                  DataField = 'XOutTOL'
                  DataSource = dsFQC_XOutTOL
                  TabOrder = 0
                end
                object DBEdit9: TDBEdit
                  Left = 164
                  Top = 4
                  Width = 62
                  Height = 20
                  Color = clMenuBar
                  DataField = 'XOutStriked'
                  DataSource = dsFQC_XOutTOL
                  TabOrder = 1
                end
                object DBEdit10: TDBEdit
                  Left = 260
                  Top = 4
                  Width = 62
                  Height = 20
                  Color = clMenuBar
                  DataField = 'XOutSurplus'
                  DataSource = dsFQC_XOutTOL
                  TabOrder = 2
                end
              end
              object pnlXOut: TPanel
                Left = 1
                Top = 1
                Width = 327
                Height = 150
                Align = alTop
                BevelOuter = bvNone
                Enabled = False
                TabOrder = 1
                object Panel3: TPanel
                  Left = 0
                  Top = 0
                  Width = 327
                  Height = 23
                  Align = alTop
                  BevelOuter = bvNone
                  TabOrder = 0
                  object Label4: TLabel
                    Left = 71
                    Top = 5
                    Width = 114
                    Height = 12
                    Caption = #22810#22577#26126#32048#27284'('#21934'X'#22577#24290')'
                    Font.Charset = ANSI_CHARSET
                    Font.Color = clBlue
                    Font.Height = -12
                    Font.Name = #32048#26126#39636
                    Font.Style = []
                    ParentFont = False
                  end
                  object DBNavigator2: TDBNavigator
                    Left = 0
                    Top = 0
                    Width = 65
                    Height = 23
                    DataSource = dsLotXOut
                    VisibleButtons = [nbPost, nbCancel, nbRefresh]
                    Align = alLeft
                    TabOrder = 0
                  end
                end
                object wwDBGrid1: TwwDBGrid
                  Left = 0
                  Top = 23
                  Width = 325
                  Height = 127
                  ControlType.Strings = (
                    'isFullScrap;CheckBox;1;0')
                  Selected.Strings = (
                    'Item'#9'6'#9#24190#22577#9#9
                    'OrgQnty'#9'8'#9#21407#22987#32047#35336#29255#25976#9#9
                    'Qnty'#9'8'#9#33267#26412#31449#32047#35336#29255#25976#9'F'#9
                    'isFullScrap'#9'5'#9#25972#22577#9'F'#9)
                  IniAttributes.Delimiter = ';;'
                  IniAttributes.UnicodeIniFile = False
                  TitleColor = clBtnFace
                  FixedCols = 2
                  ShowHorzScrollBar = True
                  Align = alLeft
                  DataSource = dsLotXOut
                  TabOrder = 1
                  TitleAlignment = taLeftJustify
                  TitleFont.Charset = ANSI_CHARSET
                  TitleFont.Color = clWindowText
                  TitleFont.Height = -12
                  TitleFont.Name = #32048#26126#39636
                  TitleFont.Style = []
                  TitleLines = 1
                  TitleButtons = False
                  OnEnter = wwDBGrid1Enter
                end
              end
            end
          end
        end
        object Panel10: TPanel
          Left = 0
          Top = 290
          Width = 510
          Height = 135
          BevelOuter = bvNone
          Caption = 'Panel10'
          TabOrder = 3
          object pnlPassMrg: TPanel
            Left = 0
            Top = 0
            Width = 510
            Height = 135
            Align = alClient
            BevelOuter = bvNone
            Enabled = False
            TabOrder = 0
            ExplicitLeft = 2
            ExplicitTop = 2
            ExplicitWidth = 1006
            ExplicitHeight = 313
            object Panel2: TPanel
              Left = 0
              Top = 0
              Width = 510
              Height = 24
              Align = alTop
              BevelOuter = bvNone
              Caption = #25209#34399
              TabOrder = 0
              ExplicitWidth = 1006
              object btFind: TSpeedButton
                Left = 4
                Top = 0
                Width = 73
                Height = 22
                Caption = #36984#25799#25209#34399
                Enabled = False
                OnClick = btFindClick
              end
            end
            object gridQnty: TwwDBGrid
              Left = 0
              Top = 24
              Width = 510
              Height = 111
              Selected.Strings = (
                'Item'#9'4'#9#38917#27425#9#9
                'LotNum'#9'24'#9#25209#34399#9#9
                'LayerName'#9'10'#9#38542#27573#9#9
                'Qnty'#9'8'#9#21407#22987#25976#37327#9#9
                'PQnty'#9'8'#9#36942#24115#25976#9#9
                'SQnty'#9'8'#9#22577#24290#25976#9#9
                'UQnty'#9'8'#9#30041#23384#25976#9#9
                'XOutQnty'#9'8'#9#26412#31449#26032#22686#21934#22577#25976#9'F'#9
                'DateCode'#9'12'#9'DateCode'#9#9
                'GoodQntyPCS'#9'13'#9#23436#25104#33391#21697'PCS'#25976#9'F'#9)
              IniAttributes.Delimiter = ';;'
              IniAttributes.UnicodeIniFile = False
              TitleColor = clBtnFace
              FixedCols = 0
              ShowHorzScrollBar = True
              Align = alClient
              DataSource = dsDetail
              KeyOptions = [dgAllowDelete]
              ReadOnly = True
              TabOrder = 1
              TitleAlignment = taLeftJustify
              TitleFont.Charset = ANSI_CHARSET
              TitleFont.Color = clWindowText
              TitleFont.Height = -12
              TitleFont.Name = #32048#26126#39636
              TitleFont.Style = []
              TitleLines = 1
              TitleButtons = False
              ExplicitWidth = 1006
              ExplicitHeight = 289
            end
          end
        end
      end
    end
    object tbsht_PassPCB_Param: TTabSheet
      Caption = #35069#31243#21443#25976
      ImageIndex = 1
    end
    object tbsht_PassPCB_XOutNH: TTabSheet
      Caption = #23458#35069#21934#22577#20837#25351#23450#20489#20998#37197#34920
      ImageIndex = 2
      object JSdDBGrid1: TJSdDBGrid
        Left = 0
        Top = 29
        Width = 1008
        Height = 576
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsFMEdLotXOutDiv
        Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
        ParentShowHint = False
        ShowHint = True
        TabOrder = 0
        TitleAlignment = taLeftJustify
        TitleFont.Charset = ANSI_CHARSET
        TitleFont.Color = clWindowText
        TitleFont.Height = -12
        TitleFont.Name = #32048#26126#39636
        TitleFont.Style = []
        TitleLines = 1
        TitleButtons = True
        SortColumnClick = stColumnClick
      end
      object Panel8: TPanel
        Left = 0
        Top = 0
        Width = 1008
        Height = 29
        Align = alTop
        BevelOuter = bvLowered
        TabOrder = 1
        object DBNavigator4: TDBNavigator
          Left = 1
          Top = 1
          Width = 192
          Height = 27
          DataSource = dsFMEdLotXOutDiv
          VisibleButtons = [nbFirst, nbPrior, nbNext, nbLast, nbEdit, nbPost, nbCancel, nbRefresh]
          Align = alLeft
          TabOrder = 0
        end
      end
    end
  end
  inherited qryExec: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    Left = 864
    Top = 408
  end
  inherited qryGetTranData: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    Left = 752
    Top = 592
  end
  object qryPassSetXOut: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <
      item
        Name = 'LotNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassSetXOut :LotNum, :PaperNum')
    Left = 825
    Top = 508
  end
  object tblMaster: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    AfterOpen = tblMasterAfterOpen
    AfterInsert = tblMasterAfterInsert
    AfterEdit = tblMasterAfterEdit
    AfterPost = tblMasterAfterPost
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      'select t0.* from FMEdPassMain t0 where 1=1')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PaperNum'
    TableName = 'FMEdPassMain'
    Left = 413
    Top = 492
    object tblMasterPaperNum: TStringField
      FieldName = 'PaperNum'
      Size = 16
    end
    object tblMasterPaperDate: TDateTimeField
      FieldName = 'PaperDate'
    end
    object tblMasterNotes: TWideStringField
      FieldName = 'Notes'
      Size = 255
    end
    object tblMasterUserId: TStringField
      FieldName = 'UserId'
      Size = 16
    end
    object tblMasterBuildDate: TDateTimeField
      FieldName = 'BuildDate'
    end
    object tblMasterStatus: TIntegerField
      FieldName = 'Status'
    end
    object tblMasterFinished: TIntegerField
      FieldName = 'Finished'
    end
    object tblMasterCancelUser: TStringField
      FieldName = 'CancelUser'
      Size = 16
    end
    object tblMasterCancelDate: TDateTimeField
      FieldName = 'CancelDate'
    end
    object tblMasterFinishUser: TStringField
      FieldName = 'FinishUser'
      Size = 16
    end
    object tblMasterFinishDate: TDateTimeField
      FieldName = 'FinishDate'
    end
    object tblMasterUseId: TStringField
      FieldName = 'UseId'
      Size = 16
    end
    object tblMasterPaperId: TStringField
      FieldName = 'PaperId'
      Size = 32
    end
    object tblMasterMrgLotQnty: TFloatField
      FieldName = 'MrgLotQnty'
    end
    object tblMasterMrgLotNum: TStringField
      FieldName = 'MrgLotNum'
      Size = 24
    end
    object tblMasterFlowStatus: TIntegerField
      FieldName = 'FlowStatus'
    end
  end
  object qryIssue: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <
      item
        Name = 'LotNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdMOPrintData :LotNum'
      ' ')
    Left = 336
    Top = 480
  end
  object tblDetail: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    BeforeInsert = tblDetailBeforeInsert
    AfterInsert = tblDetailAfterInsert
    AfterEdit = tblDetailAfterEdit
    BeforePost = tblDetailBeforePost
    AfterPost = tblDetailAfterPost
    BeforeCancel = tblDetailBeforeCancel
    AfterCancel = tblDetailAfterCancel
    BeforeDelete = tblDetailBeforeDelete
    AfterScroll = tblDetailAfterScroll
    OnNewRecord = tblDetailNewRecord
    DataSource = dsMaster
    EnableBCD = False
    Parameters = <
      item
        Name = 'PaperNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'select * from dbo.FMEdPassSub'
      'where PaperNum = :PaperNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PaperNum;Item'
    MasterSource = dsMaster
    TableName = 'FMEdPassSub'
    DisplayLabel = #36942#24115#21934#26126#32048#27284
    Left = 389
    Top = 432
    object tblDetailItem: TIntegerField
      DisplayLabel = #38917#27425
      DisplayWidth = 4
      FieldName = 'Item'
      Origin = ';;;'
      Required = True
    end
    object tblDetailLotNum: TStringField
      Tag = 1
      DisplayLabel = #25209#34399
      DisplayWidth = 24
      FieldName = 'LotNum'
      Origin = ';;;'
      Required = True
      Size = 24
    end
    object tblDetailLayerName: TWideStringField
      Tag = 2
      DisplayLabel = #38542#27573
      DisplayWidth = 10
      FieldKind = fkLookup
      FieldName = 'LayerName'
      LookupDataSet = qryProdLayer
      LookupKeyFields = 'PartNum;Revision;LayerId'
      LookupResultField = 'LayerName'
      KeyFields = 'PartNum;Revision;LayerId'
      Origin = ';;;'
      Lookup = True
    end
    object tblDetailQnty: TFloatField
      Tag = 3
      DisplayLabel = #21407#22987#25976#37327
      DisplayWidth = 8
      FieldName = 'Qnty'
      Origin = ';;;'
      Required = True
    end
    object tblDetailPQnty: TFloatField
      Tag = 4
      DisplayLabel = #36942#24115#25976
      DisplayWidth = 8
      FieldName = 'PQnty'
      Origin = ';;;'
      OnValidate = tblDetailPQntyValidate
    end
    object tblDetailSQnty: TFloatField
      Tag = 5
      DisplayLabel = #22577#24290#25976
      DisplayWidth = 8
      FieldName = 'SQnty'
      Origin = ';;;'
      OnValidate = tblDetailSQntyValidate
    end
    object tblDetailUQnty: TFloatField
      Tag = 6
      DisplayLabel = #30041#23384#25976
      DisplayWidth = 8
      FieldName = 'UQnty'
      Origin = ';;;'
    end
    object tblDetailXOutQnty: TFloatField
      Tag = 7
      DisplayLabel = #26412#31449#26032#22686#21934#22577#25976
      DisplayWidth = 8
      FieldName = 'XOutQnty'
      Origin = ';;;'
      Required = True
    end
    object tblDetailDateCode: TStringField
      Tag = 8
      DisplayWidth = 12
      FieldName = 'DateCode'
      Origin = ';;;'
      FixedChar = True
      Size = 12
    end
    object tblDetailGoodQntyPCS: TFloatField
      Tag = 37
      DisplayLabel = #23436#25104#33391#21697'PCS'#25976
      DisplayWidth = 13
      FieldName = 'GoodQntyPCS'
    end
    object tblDetailTranPQnty: TFloatField
      DisplayWidth = 10
      FieldName = 'TranPQnty'
      Visible = False
    end
    object tblDetailStkVoidPQnty: TFloatField
      DisplayWidth = 10
      FieldName = 'StkVoidPQnty'
      Visible = False
    end
    object tblDetailTranMatQnty: TFloatField
      DisplayWidth = 10
      FieldName = 'TranMatQnty'
      Visible = False
    end
    object tblDetailiIsSinglePass: TIntegerField
      DisplayWidth = 10
      FieldName = 'iIsSinglePass'
      Visible = False
    end
    object tblDetailEquipString: TStringField
      DisplayWidth = 255
      FieldName = 'EquipString'
      Visible = False
      Size = 255
    end
    object tblDetailWorkUserString: TStringField
      DisplayWidth = 255
      FieldName = 'WorkUserString'
      Visible = False
      Size = 255
    end
    object tblDetailQC_UserId: TStringField
      DisplayWidth = 16
      FieldName = 'QC_UserId'
      Visible = False
      Size = 16
    end
    object tblDetailRwkQnty: TFloatField
      DisplayWidth = 10
      FieldName = 'RwkQnty'
      Visible = False
    end
    object tblDetailScrapStrXOutQnty: TFloatField
      Tag = 9
      DisplayWidth = 8
      FieldName = 'ScrapStrXOutQnty'
      Origin = ';;;'
      Visible = False
    end
    object tblDetailPartNum: TStringField
      Tag = 10
      DisplayLabel = #21697#34399'(F4)'
      DisplayWidth = 24
      FieldName = 'PartNum'
      Origin = ';;;'
      Required = True
      Visible = False
      FixedChar = True
      Size = 24
    end
    object tblDetailRevision: TStringField
      Tag = 11
      DisplayLabel = #29256#24207
      DisplayWidth = 8
      FieldName = 'Revision'
      Origin = ';;;'
      Required = True
      Visible = False
      FixedChar = True
      Size = 8
    end
    object tblDetailProcName: TWideStringField
      Tag = 12
      DisplayLabel = #35069#31243#21517#31281
      DisplayWidth = 12
      FieldKind = fkLookup
      FieldName = 'ProcName'
      LookupDataSet = qryProcBasic
      LookupKeyFields = 'proccode'
      LookupResultField = 'procname'
      KeyFields = 'ProcCode'
      Origin = ';;;'
      Visible = False
      Lookup = True
    end
    object tblDetailPOPName: TWideStringField
      Tag = 13
      DisplayLabel = #22411#29376
      DisplayWidth = 6
      FieldKind = fkLookup
      FieldName = 'POPName'
      LookupDataSet = qryPOP
      LookupKeyFields = 'POP'
      LookupResultField = 'POPName'
      KeyFields = 'POP'
      Origin = ';;;'
      Visible = False
      Lookup = True
    end
    object tblDetailNotes: TWideStringField
      Tag = 14
      DisplayLabel = #20633#35387
      DisplayWidth = 20
      FieldName = 'Notes'
      Origin = ';;;'
      Visible = False
      Size = 255
    end
    object tblDetailSourNum: TStringField
      Tag = 15
      DisplayWidth = 16
      FieldName = 'SourNum'
      Origin = ';;;'
      Visible = False
      Size = 16
    end
    object tblDetailSourItem: TIntegerField
      Tag = 16
      DisplayWidth = 10
      FieldName = 'SourItem'
      Origin = ';;;'
      Visible = False
    end
    object tblDetailProcCode: TStringField
      Tag = 17
      DefaultExpression = 
        'EMOdProcInfo;ProcCode;ProcCode, ProcName, LevelNo=0, SuperId='#39#39';' +
        ';;;'
      DisplayLabel = #36942#20986#35069#31243
      DisplayWidth = 8
      FieldName = 'ProcCode'
      Origin = ';;;'
      Required = True
      Visible = False
      OnChange = tblDetailProcCodeChange
      FixedChar = True
      Size = 8
    end
    object tblDetailPOP: TIntegerField
      Tag = 18
      DefaultExpression = 'EMOdPOP;POP;POP, POPName, FormalName;;;;'
      DisplayLabel = #22411#29376
      DisplayWidth = 4
      FieldName = 'POP'
      Origin = ';;;'
      Required = True
      Visible = False
    end
    object tblDetailStockId: TStringField
      Tag = 19
      DisplayLabel = 'stockId'
      DisplayWidth = 8
      FieldName = 'StockId'
      Origin = ';;;'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object tblDetailAftProc: TStringField
      Tag = 20
      DefaultExpression = 
        'EMOdProcInfo;ProcCode;ProcCode, ProcName, LevelNo=0, SuperId='#39#39';' +
        ';;;'
      DisplayLabel = #19979#31449#35069#31243
      DisplayWidth = 8
      FieldName = 'AftProc'
      Origin = ';;;'
      Required = True
      Visible = False
      FixedChar = True
      Size = 8
    end
    object tblDetailAftLayer: TStringField
      Tag = 21
      DefaultExpression = 'EMOdProdLayer;LayerId;Distinct LayerId, LayerName;;;;'
      DisplayWidth = 8
      FieldName = 'AftLayer'
      Origin = ';;;'
      Required = True
      Visible = False
      FixedChar = True
      Size = 8
    end
    object tblDetailAftPOP: TIntegerField
      Tag = 22
      DefaultExpression = 'EMOdPOP;POP;POP, POPName, FormalName;;;;'
      DisplayWidth = 10
      FieldName = 'AftPOP'
      Origin = ';;;'
      Required = True
      Visible = False
    end
    object tblDetailEquipId: TStringField
      Tag = 23
      DisplayWidth = 12
      FieldName = 'EquipId'
      Origin = ';;;'
      Visible = False
      FixedChar = True
      Size = 12
    end
    object tblDetailAftLayerName: TWideStringField
      Tag = 24
      DisplayLabel = #36942#24115#24460#38542#27573
      DisplayWidth = 10
      FieldKind = fkLookup
      FieldName = 'AftLayerName'
      LookupDataSet = qryProdLayer
      LookupKeyFields = 'PartNum;Revision;LayerId'
      LookupResultField = 'LayerName'
      KeyFields = 'PartNum;Revision;AftLayer'
      Origin = ';;;'
      Visible = False
      Lookup = True
    end
    object tblDetailAftProcName: TWideStringField
      Tag = 25
      DisplayLabel = #19979#31449#35069#31243
      DisplayWidth = 12
      FieldKind = fkLookup
      FieldName = 'AftProcName'
      LookupDataSet = qryProcBasic
      LookupKeyFields = 'ProcCode'
      LookupResultField = 'ProcName'
      KeyFields = 'AftProc'
      Origin = ';;;'
      Visible = False
      Lookup = True
    end
    object tblDetailLayerId: TStringField
      Tag = 9
      DefaultExpression = 'EMOdProdLayer;LayerId;Distinct LayerId, LayerName;;;;'
      DisplayLabel = #38542#27573
      DisplayWidth = 8
      FieldName = 'LayerId'
      Origin = ';;;'
      Required = True
      Visible = False
      FixedChar = True
      Size = 8
    end
    object tblDetailAftPOPName: TWideStringField
      Tag = 27
      DisplayLabel = #36942#24115#24460#22411#29376
      DisplayWidth = 10
      FieldKind = fkLookup
      FieldName = 'AftPOPName'
      LookupDataSet = qryPOP
      LookupKeyFields = 'POP'
      LookupResultField = 'POPName'
      KeyFields = 'AftPOP'
      Origin = ';;;'
      Visible = False
      Lookup = True
    end
    object tblDetailPaperNum: TStringField
      Tag = 28
      DisplayWidth = 16
      FieldName = 'PaperNum'
      Origin = ';;;'
      Required = True
      Visible = False
      Size = 16
    end
    object tblDetailRwkScrapQnty: TFloatField
      DisplayLabel = #37325#24037#22577#24290#25976
      FieldName = 'RwkScrapQnty'
    end
    object tblDetailRwkSQntySum: TFloatField
      FieldName = 'RwkSQntySum'
    end
    object tblDetailRwkPQnty: TFloatField
      FieldName = 'RwkPQnty'
    end
    object tblDetailRouteSerial: TIntegerField
      FieldName = 'RouteSerial'
    end
  end
  object qryPassECNLotNum: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassECNLotNum :PaperNum'
      ' ')
    Left = 649
    Top = 160
  end
  object qryPassSelectLot: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <
      item
        Name = 'LotNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'InStock'
        Attributes = [paSigned, paNullable]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'UserId'
        Size = -1
        Value = Null
      end>
    SQL.Strings = (
      
        'exec FMEdPassChoiceLotPCB  :LotNum , :InStock, :PaperNum, :UserI' +
        'd')
    Left = 245
    Top = 464
    object qryPassSelectLotLotNum: TStringField
      DisplayLabel = #25209#34399
      DisplayWidth = 24
      FieldName = 'LotNum'
      FixedChar = True
      Size = 24
    end
    object qryPassSelectLotPartNum: TStringField
      DisplayLabel = #21697#34399
      DisplayWidth = 24
      FieldName = 'PartNum'
      Size = 24
    end
    object qryPassSelectLotRevision: TStringField
      DisplayLabel = #29256#24207
      DisplayWidth = 8
      FieldName = 'Revision'
      Size = 8
    end
    object qryPassSelectLotProcName: TWideStringField
      DisplayLabel = #35069#31243#21517#31281
      DisplayWidth = 12
      FieldKind = fkLookup
      FieldName = 'ProcName'
      LookupDataSet = qryProcBasic
      LookupKeyFields = 'proccode'
      LookupResultField = 'procname'
      KeyFields = 'ProcCode'
      Lookup = True
    end
    object qryPassSelectLotLayerName: TWideStringField
      DisplayLabel = #38542#27573#21517#31281
      DisplayWidth = 8
      FieldKind = fkLookup
      FieldName = 'LayerName'
      LookupDataSet = qryProdLayer
      LookupKeyFields = 'LayerId'
      LookupResultField = 'LayerName'
      KeyFields = 'LayerId'
      Lookup = True
    end
    object qryPassSelectLotPnPName: TWideStringField
      DisplayLabel = #22411#29376
      DisplayWidth = 6
      FieldKind = fkLookup
      FieldName = 'POPName'
      LookupDataSet = qryPOP
      LookupKeyFields = 'POP'
      LookupResultField = 'POPName'
      KeyFields = 'POP'
      Lookup = True
    end
    object qryPassSelectLotRestQnty: TFloatField
      DisplayLabel = #23384#37327
      DisplayWidth = 6
      FieldName = 'Qnty'
    end
    object qryPassSelectLotPOP: TSmallintField
      DisplayLabel = #22411#29376
      DisplayWidth = 4
      FieldName = 'POP'
      Visible = False
    end
    object qryPassSelectLotProcCode: TStringField
      DisplayLabel = #25552#20986#35069#31243
      DisplayWidth = 8
      FieldName = 'ProcCode'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryPassSelectLotLayerId: TStringField
      FieldName = 'LayerId'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryPassSelectLotStockId: TStringField
      FieldName = 'StockId'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryPassSelectLotLotStatus: TIntegerField
      FieldName = 'LotStatus'
    end
    object qryPassSelectLotHalted: TIntegerField
      FieldName = 'Halted'
    end
    object qryPassSelectLotFtrHalt: TIntegerField
      FieldName = 'FtrHalt'
    end
    object qryPassSelectLotHaltProc: TStringField
      FieldName = 'HaltProc'
      FixedChar = True
      Size = 8
    end
    object qryPassSelectLotRouteSerial: TIntegerField
      FieldName = 'RouteSerial'
    end
    object qryPassSelectLotAftPOP: TIntegerField
      FieldName = 'AftPOP'
      Required = True
    end
    object qryPassSelectLotAftLayer: TStringField
      FieldName = 'AftLayer'
      Required = True
      FixedChar = True
      Size = 8
    end
    object qryPassSelectLotAftProc: TStringField
      FieldName = 'AftProc'
      Required = True
      FixedChar = True
      Size = 8
    end
    object qryPassSelectLotAftPOPName: TWideStringField
      DisplayLabel = #36942#24115#24460#22411#29376
      DisplayWidth = 10
      FieldKind = fkLookup
      FieldName = 'AftPOPName'
      LookupDataSet = qryPOP
      LookupKeyFields = 'POP'
      LookupResultField = 'POPName'
      KeyFields = 'AftPOP'
      Lookup = True
    end
    object qryPassSelectLotAftProcName: TWideStringField
      DisplayLabel = #19979#31449#35069#31243
      DisplayWidth = 12
      FieldKind = fkLookup
      FieldName = 'AftProcName'
      LookupDataSet = qryProcBasic
      LookupKeyFields = 'ProcCode'
      LookupResultField = 'ProcName'
      KeyFields = 'AftProc'
      Lookup = True
    end
    object qryPassSelectLotAftLayerName: TWideStringField
      DisplayLabel = #36942#24115#24460#38542#27573
      DisplayWidth = 10
      FieldKind = fkLookup
      FieldName = 'AftLayerName'
      LookupDataSet = qryProdLayer
      LookupKeyFields = 'LayerId'
      LookupResultField = 'LayerName'
      KeyFields = 'AftLayer'
      Lookup = True
    end
    object qryPassSelectLotPQnty: TFloatField
      FieldName = 'PQnty'
    end
    object qryPassSelectLotSQnty: TFloatField
      FieldName = 'SQnty'
    end
    object qryPassSelectLotUQnty: TFloatField
      FieldName = 'UQnty'
    end
    object qryPassSelectLotProcType: TIntegerField
      FieldName = 'ProcType'
    end
    object qryPassSelectLotMatchCount: TIntegerField
      FieldName = 'MatchCount'
    end
    object qryPassSelectLotLLPCS: TFloatField
      FieldName = 'LLPCS'
    end
    object qryPassSelectLotLPCS: TFloatField
      FieldName = 'LPCS'
    end
    object qryPassSelectLotIsStorage: TIntegerField
      FieldName = 'IsStorage'
    end
    object qryPassSelectLotAftProcType: TIntegerField
      FieldName = 'AftProcType'
    end
    object qryPassSelectLotMergeRoute: TIntegerField
      FieldName = 'MergeRoute'
    end
    object qryPassSelectLotIsPrePass: TIntegerField
      FieldName = 'IsPrePass'
    end
    object qryPassSelectLotIsDateCode: TIntegerField
      FieldName = 'IsDateCode'
    end
    object qryPassSelectLotIsXOut: TIntegerField
      FieldName = 'IsXOut'
    end
    object qryPassSelectLotPOType: TIntegerField
      FieldName = 'POType'
      ReadOnly = True
    end
    object qryPassSelectLotIsShowPQnty: TIntegerField
      FieldName = 'IsShowPQnty'
    end
    object qryPassSelectLotDateCode: TStringField
      FieldName = 'DateCode'
      FixedChar = True
      Size = 12
    end
    object qryPassSelectLotXOutNeedDefect: TIntegerField
      FieldName = 'XOutNeedDefect'
    end
    object qryPassSelectLotTranMatQnty: TFloatField
      FieldName = 'TranMatQnty'
    end
    object qryPassSelectLotDtlNotes: TStringField
      FieldName = 'DtlNotes'
      Size = 255
    end
    object qryPassSelectLotGoodPCS: TIntegerField
      FieldName = 'GoodPCS'
      ReadOnly = True
    end
    object qryPassSelectLotiNeedMUT_Equip: TIntegerField
      FieldName = 'iNeedMUT_Equip'
      ReadOnly = True
    end
    object qryPassSelectLotRevNum: TStringField
      FieldName = 'RevNum'
      Size = 8
    end
  end
  object tblLotXOut: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    BeforeInsert = tblLotXOutBeforeInsert
    AfterPost = tblLotXOutAfterPost
    BeforeDelete = tblLotXOutBeforeDelete
    DataSource = dsDetail
    EnableBCD = False
    Parameters = <
      item
        Name = 'PaperNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'LotNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'select * from FMEdLotXOutTmp '
      'where PaperNum = :PaperNum '
      'and LotNum = :LotNum')
    Left = 741
    Top = 132
    object IntegerField6: TIntegerField
      DisplayLabel = #24190#22577
      DisplayWidth = 6
      FieldName = 'Item'
      ReadOnly = True
      Required = True
    end
    object tblLotXOutOrgQnty: TFloatField
      DisplayLabel = #21407#22987#32047#35336#29255#25976
      DisplayWidth = 8
      FieldName = 'OrgQnty'
      ReadOnly = True
    end
    object IntegerField1: TFloatField
      DisplayLabel = #33267#26412#31449#32047#35336#29255#25976
      DisplayWidth = 8
      FieldName = 'Qnty'
      Required = True
    end
    object tblLotXOutisFullScrap: TIntegerField
      DisplayLabel = #25972#22577
      DisplayWidth = 6
      FieldName = 'isFullScrap'
      ReadOnly = True
    end
    object tblLotXOutPaperNum: TStringField
      DisplayWidth = 16
      FieldName = 'PaperNum'
      Visible = False
      Size = 16
    end
    object StringField1: TStringField
      DisplayLabel = #25209#34399
      DisplayWidth = 24
      FieldName = 'LotNum'
      Required = True
      Visible = False
      Size = 24
    end
  end
  object qryProcType: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'ProcCode'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      ' select t2.ProcType'
      '   from EMOdProcInfo t2(nolock)'
      '  where t2.ProcCode= :ProcCode'
      ' ')
    Left = 557
    Top = 552
  end
  object qryLotNum: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'PaperId'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        Direction = pdInputOutput
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'HeadParam'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'exec CURdGetPaperNum :PaperId, :PaperNum, :HeadParam'
      ' ')
    Left = 169
    Top = 496
  end
  object qryProcUser: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'ProcCode'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end
      item
        Name = 'UserId'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      ' select t1.*, t2.ProcType'
      '   from FMEdProcUser t1(nolock),'
      '        EMOdProcInfo t2(nolock)'
      '  where t1.ProcCode= t2.ProcCode'
      '    and t1.ProcCode= :ProcCode'
      '    and t1.UserId= :UserId'
      ' ')
    Left = 609
    Top = 580
  end
  object qryTmp2XOut: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassTmp2XOut :PaperNum')
    Left = 477
    Top = 492
  end
  object dsPassSelect: TwwDataSource
    DataSet = qryPassSelectLot
    Left = 737
    Top = 520
  end
  object qryAutoVoidPaper: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'UserId'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassAutoVoidPaper'
      ':PaperNum,'
      ':UserId')
    Left = 800
    Top = 416
  end
  object qryLotTo36: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'LotNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdLotTo36 :LotNum'
      ' ')
    Left = 737
    Top = 404
    object qryLotTo36ShortLotNum: TStringField
      FieldName = 'ShortLotNum'
      FixedChar = True
      Size = 5
    end
  end
  object qryEquipProc: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'ProcCode'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdAPS_EquipProc  :ProcCode')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdAPS_EquipProc'
    Left = 288
    Top = 416
    object qryEquipProcEquipId: TStringField
      FieldName = 'EquipId'
      ReadOnly = True
      Size = 12
    end
    object qryEquipProcEquipName: TWideStringField
      FieldName = 'EquipName'
      ReadOnly = True
      Size = 24
    end
    object qryEquipProcProcCode: TStringField
      FieldName = 'ProcCode'
      Size = 8
    end
  end
  object qryInStockPrint: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdInStockPrint :PaperNum'
      ' ')
    Left = 649
    Top = 452
  end
  object dsEquipProc: TDataSource
    DataSet = qryEquipProc
    Left = 504
    Top = 568
  end
  object dsDetail: TDataSource
    DataSet = tblDetail
    Left = 721
    Top = 456
  end
  object qryPassDivLotNum: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassDivLotNum :PaperNum'
      ' ')
    Left = 597
    Top = 396
  end
  object qryIsCheckProc: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    Parameters = <
      item
        Name = 'AftProc'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassIsCheckProc :AftProc'
      ' ')
    Left = 577
    Top = 448
  end
  object pmuPaperPaper: TJSdPopupMenu
    BatchPaper = False
    Main_btnPrint_Handle = 0
    PrintType = 0
    Left = 664
    Top = 568
  end
  object dsLotXOut: TwwDataSource
    DataSet = tblLotXOut
    Left = 809
    Top = 580
  end
  object dsMaster: TDataSource
    DataSet = tblMaster
    Left = 441
    Top = 444
  end
  object qryPOP: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'select POP, POPName, FormalName'
      'from dbo.EMOdPOP(nolock)')
    Left = 60
    Top = 480
  end
  object qryProcBasic: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'SELECT ProcCode, ProcName, LevelNo=0, SuperId='#39#39
      'FROM dbo.EMOdProcInfo(nolock)'
      'Order By ProcCode')
    Left = 108
    Top = 564
  end
  object dsProcBasic: TDataSource
    DataSet = qryProcBasic
    Left = 280
    Top = 532
  end
  object dsEquipId: TDataSource
    DataSet = qryEquipId
    Left = 528
    Top = 400
  end
  object qryEquipId: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'exec FMEdAPS_EquipInfo')
    Left = 176
    Top = 432
    object qryEquipIdEquipId: TStringField
      FieldName = 'EquipId'
      ReadOnly = True
      Size = 12
    end
    object qryEquipIdEquipName: TWideStringField
      FieldName = 'EquipName'
      ReadOnly = True
      Size = 24
    end
  end
  object dsProdLayer: TDataSource
    DataSet = qryProdLayer
    Left = 704
    Top = 188
  end
  object qryProdLayer: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'SELECT Distinct PartNum,Revision,LayerId, LayerName'
      'FROM dbo.EMOdProdLayer(nolock)'
      '')
    Left = 172
    Top = 556
    object qryProdLayerPartNum: TStringField
      FieldName = 'PartNum'
      Size = 24
    end
    object qryProdLayerRevision: TStringField
      FieldName = 'Revision'
      Size = 8
    end
    object qryProdLayerLayerId: TStringField
      FieldName = 'LayerId'
      FixedChar = True
      Size = 8
    end
    object qryProdLayerLayerName: TWideStringField
      FieldName = 'LayerName'
      Size = 24
    end
  end
  object dsPOP: TDataSource
    DataSet = qryPOP
    Left = 464
    Top = 576
  end
  object dsLotStatus: TwwDataSource
    DataSet = qryLotStatus
    Left = 700
    Top = 232
  end
  object qryLotStatus: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'SELECT LotStatus, LotStatusName'
      'FROM dbo.FMEdLotStatus(nolock)')
    Left = 64
    Top = 408
  end
  object qryPOType: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'SELECT POType, POTypeName'
      'FROM dbo.FMEdPOType(nolock)')
    Left = 376
    Top = 580
  end
  object dsPOType: TwwDataSource
    DataSet = qryPOType
    Left = 636
    Top = 508
  end
  object qryXOutDefect: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    BeforeInsert = qryXOutDefectBeforeInsert
    OnNewRecord = qryXOutDefectNewRecord
    DataSource = dsDetail
    EnableBCD = False
    Parameters = <
      item
        Name = 'PaperNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'Item'
        Attributes = [paSigned]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'select * from FMEdPassXOutDefect '
      'where PaperNum = :PaperNum '
      'and Item = :Item')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdPassXOutDefect'
    Left = 264
    Top = 352
    object qryXOutDefectSerialNum: TIntegerField
      DisplayWidth = 10
      FieldName = 'SerialNum'
    end
    object qryXOutDefectClassId: TStringField
      DisplayWidth = 8
      FieldName = 'ClassId'
      OnValidate = qryXOutDefectClassIdValidate
      Size = 8
    end
    object qryXOutDefectLk_ClassName: TWideStringField
      DisplayWidth = 20
      FieldKind = fkLookup
      FieldName = 'Lk_ClassName'
      LookupDataSet = qryClassId
      LookupKeyFields = 'ClassId'
      LookupResultField = 'ClassName'
      KeyFields = 'ClassId'
      Lookup = True
    end
    object qryXOutDefectDefectId: TStringField
      DisplayWidth = 8
      FieldName = 'DefectId'
      OnValidate = qryXOutDefectDefectIdValidate
      Size = 8
    end
    object qryXOutDefectLk_DefectName: TWideStringField
      DisplayWidth = 20
      FieldKind = fkLookup
      FieldName = 'Lk_DefectName'
      LookupDataSet = qryDefectId
      LookupKeyFields = 'DefectId'
      LookupResultField = 'DefectName'
      KeyFields = 'DefectId'
      Lookup = True
    end
    object qryXOutDefectDutyProc: TStringField
      DisplayWidth = 8
      FieldName = 'DutyProc'
      Size = 8
    end
    object qryXOutDefectLk_DutyProcName: TWideStringField
      DisplayWidth = 20
      FieldKind = fkLookup
      FieldName = 'Lk_DutyProcName'
      LookupDataSet = qryProcDepartMerge
      LookupKeyFields = 'DepartId'
      LookupResultField = 'DepartName'
      KeyFields = 'DutyProc'
      Lookup = True
    end
    object qryXOutDefectQnty: TFloatField
      DisplayWidth = 10
      FieldName = 'Qnty'
    end
    object qryXOutDefectPaperNum: TStringField
      DisplayWidth = 16
      FieldName = 'PaperNum'
      Size = 16
    end
    object qryXOutDefectItem: TIntegerField
      DisplayWidth = 10
      FieldName = 'Item'
    end
    object qryXOutDefectCompanyId: TStringField
      DisplayWidth = 16
      FieldName = 'CompanyId'
      Size = 16
    end
    object qryXOutDefectQntyOri: TFloatField
      DisplayWidth = 10
      FieldName = 'QntyOri'
    end
    object qryXOutDefectXOutDateCode: TStringField
      FieldName = 'XOutDateCode'
      Visible = False
      Size = 12
    end
  end
  object dsXOutDefect: TDataSource
    DataSet = qryXOutDefect
    Left = 344
    Top = 352
  end
  object qryClassId: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      'select ClassId,ClassName,ResProc from FQCdDefectClass(nolock)'
      'order by ClassId')
    Left = 136
    Top = 352
    object qryClassIdClassId: TStringField
      FieldName = 'ClassId'
      Size = 8
    end
    object qryClassIdClassName: TWideStringField
      FieldName = 'ClassName'
      Size = 24
    end
    object qryClassIdResProc: TWideStringField
      DisplayWidth = 24
      FieldName = 'ResProc'
      Size = 24
    end
  end
  object qryDefectId: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      'select DefectId,DefectName, ResProc  from FQCdDefectInfo(nolock)'
      'order by DefectId')
    Left = 56
    Top = 360
    object qryDefectIdDefectId: TStringField
      FieldName = 'DefectId'
      Size = 8
    end
    object qryDefectIdDefectName: TWideStringField
      FieldName = 'DefectName'
      Size = 24
    end
    object qryDefectIdResProc: TStringField
      FieldName = 'ResProc'
      Size = 24
    end
  end
  object qryTmp2XOutNewAdd: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <
      item
        Name = 'PaperNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 16
        Value = Null
      end
      item
        Name = 'LotNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'exec FMEdPassTmp2XOutNewAdd :PaperNum, :LotNum')
    Left = 237
    Top = 580
  end
  object qryProcDepartMerge: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    CommandTimeout = 600
    Parameters = <>
    SQL.Strings = (
      'select * from FQCdProcDepartMerge(nolock)')
    Left = 576
    Top = 504
  end
  object qryFQC_XOutTOL: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    LockType = ltReadOnly
    CommandTimeout = 600
    EnableBCD = False
    Parameters = <
      item
        Name = 'LotNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'select * from FQCdV_XOut_TOL(nolock) where LotNum =:LotNum')
    Left = 896
    Top = 456
  end
  object dsFQC_XOutTOL: TDataSource
    DataSet = qryFQC_XOutTOL
    Left = 904
    Top = 544
  end
  object dsProcParamDtl: TDataSource
    DataSet = qryProcParamDtl
    Left = 960
    Top = 176
  end
  object qryProcParamDtl: TJSdTable
    BeforeInsert = qryProcParamDtlBeforeInsert
    BeforeDelete = qryProcParamDtlBeforeDelete
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdPassProcParamDtl'
    Left = 856
    Top = 88
  end
  object qryFMEdLotXOutDiv: TJSdTable
    BeforeInsert = qryFMEdLotXOutDivBeforeInsert
    BeforeDelete = qryFMEdLotXOutDivBeforeDelete
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdLotXOutDiv'
    Left = 896
    Top = 216
  end
  object dsFMEdLotXOutDiv: TDataSource
    DataSet = qryFMEdLotXOutDiv
    Left = 904
    Top = 160
  end
  object qryUsers: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    Parameters = <>
    SQL.Strings = (
      'exec FMEdQCUserGet')
    Left = 128
    Top = 488
  end
  object dsUsers: TDataSource
    DataSet = qryUsers
    Left = 40
    Top = 552
  end
  object qryAllowXOutQnty: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    DataSource = dsDetail
    EnableBCD = False
    Parameters = <
      item
        Name = 'PartNum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      'select * from MINdMatInfo'
      'where PartNum = :PartNum ')
    Left = 893
    Top = 268
    object qryAllowXOutQntyAllowXOutQnty: TIntegerField
      FieldName = 'AllowXOutQnty'
    end
  end
  object dsAllowXOutQnty: TwwDataSource
    DataSet = qryAllowXOutQnty
    Left = 889
    Top = 316
  end
  object qryRwkSDefect: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    BeforeInsert = qryRwkSDefectBeforeInsert
    BeforeDelete = qryRwkSDefectBeforeDelete
    OnNewRecord = qryRwkSDefectNewRecord
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FMEdPassRwksDefect'
    Left = 440
    Top = 344
    object IntegerField2: TIntegerField
      DisplayLabel = #24207#34399
      DisplayWidth = 4
      FieldName = 'SerialNum'
    end
    object qryRwkSDefectRwkProc: TStringField
      DisplayLabel = #25552#20986#31449
      DisplayWidth = 6
      FieldName = 'RwkProc'
      Size = 16
    end
    object qryRwkSDefectLk_RwkProcName: TStringField
      DisplayLabel = #25552#20986#31449#21517#31281
      DisplayWidth = 15
      FieldKind = fkLookup
      FieldName = 'Lk_RwkProcName'
      LookupDataSet = qryProcCode
      LookupKeyFields = 'ProcCode'
      LookupResultField = 'ProcName'
      KeyFields = 'RwkProc'
      Size = 96
      Lookup = True
    end
    object StringField2: TStringField
      DisplayLabel = #32570#40670#20998#39006
      DisplayWidth = 11
      FieldName = 'ClassId'
      OnValidate = StringField2Validate
      Size = 8
    end
    object WideStringField1: TWideStringField
      DisplayLabel = #32570#40670#20998#39006#21517#31281
      DisplayWidth = 20
      FieldKind = fkLookup
      FieldName = 'Lk_ClassName'
      LookupDataSet = qryClassId
      LookupKeyFields = 'ClassId'
      LookupResultField = 'ClassName'
      KeyFields = 'ClassId'
      Lookup = True
    end
    object StringField3: TStringField
      DisplayLabel = #32570#40670#20195#34399
      DisplayWidth = 11
      FieldName = 'DefectId'
      OnValidate = StringField3Validate
      Size = 8
    end
    object WideStringField2: TWideStringField
      DisplayLabel = #32570#40670#21517#31281
      DisplayWidth = 16
      FieldKind = fkLookup
      FieldName = 'Lk_DefectName'
      LookupDataSet = qryDefectId
      LookupKeyFields = 'DefectId'
      LookupResultField = 'DefectName'
      KeyFields = 'DefectId'
      Lookup = True
    end
    object StringField4: TStringField
      DisplayLabel = #36012#20219#35069#31243
      DisplayWidth = 8
      FieldName = 'DutyProc'
      Size = 8
    end
    object WideStringField3: TWideStringField
      DisplayLabel = #36012#20219#35069#31243#21517#31281
      DisplayWidth = 15
      FieldKind = fkLookup
      FieldName = 'Lk_DutyProcName'
      LookupDataSet = qryProcDepartMerge
      LookupKeyFields = 'DepartId'
      LookupResultField = 'DepartName'
      KeyFields = 'DutyProc'
      Lookup = True
    end
    object FloatField2: TFloatField
      DisplayLabel = #21407#22987#25976#37327
      DisplayWidth = 8
      FieldName = 'QntyOri'
    end
    object FloatField1: TFloatField
      DisplayLabel = #22577#24290#25976#37327
      DisplayWidth = 8
      FieldName = 'Qnty'
    end
    object StringField5: TStringField
      DisplayWidth = 16
      FieldName = 'PaperNum'
      Visible = False
      Size = 16
    end
    object qryRwkSDefectLotNum: TWideStringField
      DisplayWidth = 24
      FieldName = 'LotNum'
      Visible = False
      Size = 24
    end
    object IntegerField3: TIntegerField
      DisplayWidth = 10
      FieldName = 'Item'
      Visible = False
    end
    object qryRwkSDefectRouteSerial: TIntegerField
      DisplayWidth = 10
      FieldName = 'RouteSerial'
      Visible = False
    end
  end
  object dsRwkSDefect: TDataSource
    DataSet = qryRwkSDefect
    Left = 520
    Top = 344
  end
  object qryProcCode: TADOQuery
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      'select ProcCode,ProcName from EMOdProcInfo(nolock)'
      'order by ProcCode')
    Left = 672
    Top = 352
    object qryProcCodeProcCode: TStringField
      FieldName = 'ProcCode'
      Size = 8
    end
    object qryProcCodeProcName: TStringField
      FieldName = 'ProcName'
      Size = 96
    end
  end
end

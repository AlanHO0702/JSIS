inherited frmProdLayerDLL: TfrmProdLayerDLL
  Caption = 'frmProdLayerDLL'
  ClientHeight = 626
  ExplicitHeight = 653
  PixelsPerInch = 96
  TextHeight = 13
  object Splitter2: TSplitter [0]
    Left = 0
    Top = 43
    Width = 1028
    Height = 5
    Cursor = crVSplit
    Align = alTop
    Color = clMedGray
    ParentColor = False
    ExplicitLeft = -56
    ExplicitTop = 92
    ExplicitWidth = 1089
  end
  inherited pnlInfo: TPanel
    Top = 602
    ExplicitTop = 602
    object lblLayerIdBack: TLabel [1]
      Left = 272
      Top = 8
      Width = 69
      Height = 13
      Caption = 'lblLayerIdBack'
      Visible = False
    end
    object lblRevisionBack: TLabel [2]
      Left = 392
      Top = 8
      Width = 72
      Height = 13
      Caption = 'lblRevisionBack'
      Visible = False
    end
    object lblPartNumBack: TLabel [3]
      Left = 520
      Top = 8
      Width = 73
      Height = 13
      Caption = 'lblPartNumBack'
      Visible = False
    end
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 533
    ExplicitTop = 533
  end
  inherited pgeBwsDtl: TPageControl
    Top = 48
    Height = 485
    ExplicitTop = 48
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
      inherited Splitter1: TSplitter
        Left = 254
        Top = 57
        Width = 5
        Height = 391
        Cursor = crHSplit
        Align = alLeft
        Visible = False
        ExplicitLeft = 404
        ExplicitTop = 70
        ExplicitWidth = 5
        ExplicitHeight = 379
      end
      inherited pgeMaster: TPageControl
        Top = 57
        Width = 254
        Height = 391
        ActivePage = tbshtMaster1
        Align = alLeft
        ExplicitTop = 57
        ExplicitWidth = 254
        ExplicitHeight = 391
        inherited tbshtMaster1: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 53
          ExplicitWidth = 246
          ExplicitHeight = 334
          inherited pnlMaster1: TScrollBox
            Width = 246
            Height = 334
            ExplicitWidth = 246
            ExplicitHeight = 334
            object sclPnlMaster: TScrollBar
              Left = 230
              Top = 0
              Width = 16
              Height = 334
              Align = alRight
              Kind = sbVertical
              PageSize = 0
              ParentShowHint = False
              ShowHint = False
              TabOrder = 0
              Visible = False
              OnScroll = sclPnlMasterScroll
            end
          end
        end
        inherited tbshtMaster2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 53
          ExplicitWidth = 246
          ExplicitHeight = 334
          inherited pnlMaster2: TScrollBox
            Width = 246
            Height = 334
            ExplicitWidth = 246
            ExplicitHeight = 334
          end
        end
        inherited tbshtMaster3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 53
          ExplicitWidth = 246
          ExplicitHeight = 334
          inherited pnlMaster3: TScrollBox
            Width = 246
            Height = 334
            ExplicitWidth = 246
            ExplicitHeight = 334
          end
        end
        inherited tbshtMaster4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 53
          ExplicitWidth = 246
          ExplicitHeight = 334
          inherited pnlMaster4: TScrollBox
            Width = 246
            Height = 334
            ExplicitWidth = 246
            ExplicitHeight = 334
          end
        end
        inherited tbshtAttach: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 53
          ExplicitWidth = 246
          ExplicitHeight = 334
          inherited pnlAttach: TPanel
            Width = 246
            Height = 334
            ExplicitWidth = 246
            ExplicitHeight = 334
            inherited pnlAttachBtn: TPanel
              Width = 246
              ExplicitWidth = 246
            end
            inherited dbgAttach: TJSdDBGrid
              Width = 246
              Height = 307
              ExplicitWidth = 246
              ExplicitHeight = 307
            end
          end
        end
      end
      inherited pgeDetail: TPageControl
        Left = 259
        Top = 57
        Width = 761
        Height = 391
        ExplicitLeft = 259
        ExplicitTop = 57
        ExplicitWidth = 761
        ExplicitHeight = 391
        inherited tbshtDetail1: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 753
          ExplicitHeight = 359
          object Splitter3: TSplitter [0]
            Left = 439
            Top = 32
            Width = 4
            Height = 327
            Align = alRight
            ExplicitLeft = 603
            ExplicitTop = 30
            ExplicitHeight = 316
          end
          inherited gridDetail1: TJSdDBGrid
            Top = 32
            Width = 439
            Height = 327
            ExplicitTop = 32
            ExplicitWidth = 439
            ExplicitHeight = 327
          end
          object pnlRouteTools: TPanel
            Left = 0
            Top = 0
            Width = 753
            Height = 32
            Align = alTop
            BevelInner = bvLowered
            ParentColor = True
            TabOrder = 1
            object btnChangeProc: TSpeedButton
              Left = 172
              Top = 2
              Width = 75
              Height = 28
              Caption = '&C'#36884#31243#30064#21205
              Flat = True
              OnClick = btnChangeProcClick
            end
            object btnBackupNotes: TSpeedButton
              Left = 327
              Top = 2
              Width = 75
              Height = 28
              Caption = #35079#35069#20633#35387
              Flat = True
              OnClick = btnBackupNotesClick
            end
            object btnPasteNotes: TSpeedButton
              Left = 397
              Top = 2
              Width = 75
              Height = 28
              Caption = #36028#19978#20633#35387
              Flat = True
              OnClick = btnPasteNotesClick
            end
            object btnNoteStyleTree: TSpeedButton
              Left = 249
              Top = 2
              Width = 75
              Height = 28
              Caption = '&C'#36884#31243#20633#35387
              Flat = True
              OnClick = btnNoteStyleTreeClick
            end
            object btnRouteChange: TSpeedButton
              Left = 485
              Top = 2
              Width = 108
              Height = 28
              Caption = #36884#31243#20839#23481#35722#26356
              Glyph.Data = {
                36030000424D3603000000000000360000002800000010000000100000000100
                1800000000000003000000000000000000000000000000000000C8B2BCC4B2C0
                C7ADC6CBACC6C5B1C2B8AFB7CCC2CCAE99A75954619D98B0D2C2CDCCADB2C3B4
                C0C4B3C0CAABC6BAACBCC4B1C2C3B0C3C0B0C5C9B0C4C6ACC0C5B8CBA8A1B47A
                7D8E8EA1AA717B8CB59BAED8B2C9BCAABEC3ADC0CFB0CCD4BBCCC4AFC6BFB1C5
                BEB2C2C1AEBED3B4CCB4A1B8747C8886939980786C909799838692BAAEC0CAC1
                DCCBBBD3B8A2BC998491C8B0C3C2B2C1BFAFBDCFB6CCB49FBA727581919C94A5
                9073FFDFB195806861666B464652827C847176844A3B47430000CBAFC3C1AEBD
                CEB9C9B9A0B7737A8E9BBDC1767364F9DCB1988A6E5869725786985280984D75
                88769CA9B897A27A0303BCA8BFCBB9CCB4A1B16F74817A94A496ABB69FA7A66F
                635D7A8F9B85AABE90CCE38ED4F685CCE7B9EFFFC4ADB56F0000C9BFD5AF9EB4
                727C8A9CC2CB55616F5B63756F7D824C5A687D8C9726242718242E364A5C5774
                8AB4DBEE665A60741E21AF99A9737D8C6F91A18EA4B09FB0B94B546097B0BA9C
                B7C12F45575C70837391A6CAF2F6C3DEEF59456D776D8F948F9555565F8DA1AF
                3A81992E8EA889A3B29CB5BC434E63B7CFDC8987A562738E88AABA89ADB48845
                9E6D16778992B0767B8CA698AC747A8B7594A932A0BD2292AE7A9DAEB4D4D98A
                89A79F13A6B805BDA300A89900A68018879E8DB1777E85B69FBADABECFAF9EB0
                767B897D8CA12C9CB91C91AC8CA2B5989FB49409A1FF07FFFF05FFB217B89F8E
                B07A7C87B2A5A8C8BBC6C3AFB4CCBBCAAB9FB6727B897A8DA2309CBD278CB389
                AAB797A6BE8E0CA19019A4918BA9787F85AD9CBCC7B4D2C5B0BAC3B3BFBCAFBE
                C4B9CCAEA3B27A7C857A89A22E9ABA2194A98AB0B5A19CB58686A67E7D8DABA3
                AECAB5D0C2A9C6C4B2BFBFB0C2C7AFC4C1AEBED1B8C6BAA2B17A7B897A8DA531
                9CBA39758FA1B8C0757D88B2A1B2CEB7CEC0ABC2CFAEC4C1AFC5D6ADC0C7ADC6
                C8AEC6C1AFBEC4BBCAB0A1B07B7987768EA57297AE6A747EB49DBAD0B7CABBB1
                BBC8B1BFCFAEC2C0B0C5CAAFC4BCB2C1BBB1C3C7ACCBBDACC3C8BBC7B0A3B07D
                7E8B7C7C89B3A5B6D1B5CAC6ACC0BBB2C4C8AEC4C4AEC6C5B0C3}
              Visible = False
              OnClick = btnRouteChangeClick
            end
            object btnProcBOMSet: TSpeedButton
              Left = 596
              Top = 2
              Width = 90
              Height = 28
              Caption = #35069#31243#26448#26009#35373#23450
              Visible = False
              OnClick = btnProcBOMSetClick
            end
            object DBNavigator2: TDBNavigator
              Left = 932
              Top = 2
              Width = 102
              Height = 28
              VisibleButtons = [nbPost, nbCancel, nbRefresh]
              Flat = True
              Hints.Strings = (
                #31532#19968#31558
                #19978#19968#31558
                #19979#19968#31558
                #26368#24460#19968#31558
                #26032#22686
                #21034#38500
                #20462#25913
                #20786#23384
                #25918#26820
                #37325#21462#36039#26009)
              ParentShowHint = False
              ShowHint = True
              TabOrder = 1
              Visible = False
            end
            object panRoute: TPanel
              Left = 2
              Top = 5
              Width = 55
              Height = 22
              Hint = #35531#25353#27492#34389', '#37325#26032#25235#21462#26032#22686#20043#36884#31243#20195#30908'!!'
              BevelOuter = bvNone
              Caption = #36884#31243#20195#30908
              ParentColor = True
              ParentShowHint = False
              ShowHint = True
              TabOrder = 0
            end
            object edtTmpPressId: TDBEdit
              Left = 59
              Top = 5
              Width = 107
              Height = 21
              DataField = 'TmpRouteId'
              DataSource = dsBrowse
              ReadOnly = True
              TabOrder = 2
            end
            object pnlUseNotes: TPanel
              Left = 708
              Top = 2
              Width = 188
              Height = 28
              BevelOuter = bvNone
              TabOrder = 3
              object btnUseNotes: TSpeedButton
                Left = 86
                Top = 0
                Width = 90
                Height = 28
                Caption = #29986#29983#27880#24847#20107#38917
                OnClick = btnUseNotesClick
              end
              object chkUseNotes: TDBCheckBox
                Left = 19
                Top = 6
                Width = 66
                Height = 17
                Caption = #24050#29986#29983
                DataField = 'IsUseNotes'
                DataSource = dsBrowse
                ReadOnly = True
                TabOrder = 0
              end
            end
          end
          object Panel1: TPanel
            Left = 443
            Top = 32
            Width = 310
            Height = 327
            Align = alRight
            BevelOuter = bvNone
            TabOrder = 2
            object Splitter8: TSplitter
              Left = 0
              Top = 224
              Width = 310
              Height = 4
              Cursor = crVSplit
              Align = alBottom
              ExplicitTop = 97
            end
            object Splitter4: TSplitter
              Left = 0
              Top = 164
              Width = 310
              Height = 4
              Cursor = crVSplit
              Align = alBottom
              ExplicitTop = 0
            end
            object dbgBOM: TJSdDBGrid
              Left = 0
              Top = 0
              Width = 310
              Height = 164
              IniAttributes.Delimiter = ';;'
              IniAttributes.UnicodeIniFile = False
              TitleColor = clBtnFace
              FixedCols = 0
              ShowHorzScrollBar = True
              Align = alClient
              DataSource = dsLayerBOM
              Options = [dgEditing, dgAlwaysShowEditor, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap]
              ParentShowHint = False
              ShowHint = True
              TabOrder = 0
              TitleAlignment = taLeftJustify
              TitleFont.Charset = ANSI_CHARSET
              TitleFont.Color = clWindowText
              TitleFont.Height = -13
              TitleFont.Name = #32048#26126#39636
              TitleFont.Style = []
              TitleLines = 1
              TitleButtons = True
              OnColEnter = dbgBOMColEnter
              SortColumnClick = stColumnClick
            end
            object pnlPath: TPanel
              Left = 0
              Top = 168
              Width = 310
              Height = 56
              Align = alBottom
              BevelOuter = bvNone
              TabOrder = 1
              object 附件: TJSdLabel
                Left = 6
                Top = 5
                Width = 28
                Height = 13
                Caption = #38468#20214
                DataField = 'NotesPath'
                DataSource = dsDetail1
              end
              object btnCMap: TSpeedButton
                Left = 6
                Top = 27
                Width = 76
                Height = 25
                Caption = #35079#35069#27284#26696
                OnClick = btnCMapClick
              end
              object btnCMapOpen: TSpeedButton
                Left = 88
                Top = 27
                Width = 76
                Height = 25
                Caption = #27298#35222
                OnClick = btnCMapOpenClick
              end
              object btnMapOpenTest: TSpeedButton
                Left = 192
                Top = 27
                Width = 76
                Height = 25
                Caption = #27298#35222'test'
                OnClick = btnMapOpenTestClick
              end
              object DBEdit1: TDBEdit
                Left = 38
                Top = 2
                Width = 267
                Height = 21
                DataField = 'NotesPath'
                DataSource = dsDetail1
                TabOrder = 0
              end
            end
            object pnlRouteNote: TPanel
              Left = 0
              Top = 228
              Width = 310
              Height = 99
              Align = alBottom
              BevelOuter = bvNone
              TabOrder = 2
              object Splitter5: TSplitter
                Left = 166
                Top = 0
                Width = 5
                Height = 99
                ExplicitLeft = 0
              end
              object DBMemo1: TDBMemo
                Left = 0
                Top = 0
                Width = 166
                Height = 99
                Align = alLeft
                DataField = 'Notes'
                DataSource = dsDetail1
                TabOrder = 0
              end
              object pnlNoteSep: TPanel
                Left = 171
                Top = 0
                Width = 139
                Height = 99
                Align = alClient
                BevelOuter = bvNone
                TabOrder = 1
              end
            end
          end
        end
        inherited tbshtDetail2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 753
          ExplicitHeight = 359
          inherited gridDetail2: TJSdDBGrid
            Width = 753
            Height = 359
            ExplicitWidth = 753
            ExplicitHeight = 359
          end
        end
        inherited tbshtDetail3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 753
          ExplicitHeight = 359
          inherited gridDetail3: TJSdDBGrid
            Width = 753
            Height = 359
            OnColExit = gridDetail3ColExit
            ExplicitWidth = 753
            ExplicitHeight = 359
          end
        end
        inherited tbshtDetail4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 753
          ExplicitHeight = 359
          inherited gridDetail4: TJSdDBGrid
            Width = 753
            Height = 359
            ExplicitWidth = 753
            ExplicitHeight = 359
          end
        end
        inherited tbshtDetail5: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 753
          ExplicitHeight = 359
          inherited gridDetail5: TJSdDBGrid
            Width = 753
            Height = 359
            ExplicitWidth = 753
            ExplicitHeight = 359
          end
        end
        inherited tbshtDetail6: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 753
          ExplicitHeight = 359
          inherited gridDetail6: TJSdDBGrid
            Width = 753
            Height = 359
            ExplicitWidth = 753
            ExplicitHeight = 359
          end
        end
        inherited tbshtDetail7: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 753
          ExplicitHeight = 359
          inherited gridDetail7: TJSdDBGrid
            Width = 753
            Height = 359
            ExplicitWidth = 753
            ExplicitHeight = 359
          end
        end
        inherited tbshtDetail8: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 753
          ExplicitHeight = 359
          inherited gridDetail8: TJSdDBGrid
            Width = 753
            Height = 359
            ExplicitWidth = 753
            ExplicitHeight = 359
          end
        end
        object TabSheetMas: TTabSheet
          Caption = 'TabSheetMas'
          ImageIndex = 8
          object pnlCopyMas: TPanel
            Left = 0
            Top = 0
            Width = 753
            Height = 359
            Align = alClient
            BevelOuter = bvNone
            TabOrder = 0
            object sclPnlMaster2: TScrollBar
              Left = 737
              Top = 0
              Width = 16
              Height = 359
              Align = alRight
              Kind = sbVertical
              PageSize = 0
              ParentShowHint = False
              ShowHint = False
              TabOrder = 0
              OnScroll = sclPnlMasterScroll
            end
          end
        end
      end
      inherited pnlTempBasDLLBottom: TPanel
        Top = 448
        ExplicitTop = 448
      end
      object pnlPartNum: TPanel
        Left = 0
        Top = 0
        Width = 1020
        Height = 57
        Align = alTop
        BevelInner = bvRaised
        BevelOuter = bvLowered
        TabOrder = 3
        object JSdLabel1: TJSdLabel
          Left = 37
          Top = 9
          Width = 28
          Height = 13
          Alignment = taRightJustify
          Caption = #21697#34399
        end
        object lblParam: TLabel
          Left = 876
          Top = 16
          Width = 126
          Height = 13
          Caption = 'Change Form Height'
          Visible = False
        end
        object lblCusId: TLabel
          Left = 665
          Top = 6
          Width = 14
          Height = 13
          Caption = 'JS'
          Visible = False
        end
        object lblNavSource: TLabel
          Left = 680
          Top = 11
          Width = 63
          Height = 13
          Caption = 'NavSource'
          Visible = False
        end
        object pgeFormType: TPageControl
          Left = 2
          Top = 30
          Width = 1016
          Height = 25
          ActivePage = tstSub
          Align = alBottom
          TabOrder = 0
          OnChange = pgeFormTypeChange
          object tstMain: TTabSheet
            Caption = #22522#26412#35215#26684
          end
          object tstSub: TTabSheet
            Caption = #34920#26684#36039#26009
            ImageIndex = 1
          end
        end
        object edtPnum: TDBEdit
          Left = 65
          Top = 6
          Width = 152
          Height = 21
          AutoSelect = False
          Color = clBtnFace
          DataField = 'PartNum'
          DataSource = dsBrowse
          ReadOnly = True
          TabOrder = 1
        end
        object edtRevision: TDBEdit
          Left = 219
          Top = 6
          Width = 37
          Height = 21
          AutoSelect = False
          Color = clBtnFace
          DataField = 'Revision'
          DataSource = dsBrowse
          ReadOnly = True
          TabOrder = 2
        end
        object edtLayerId: TDBEdit
          Left = 265
          Top = 6
          Width = 114
          Height = 21
          HelpType = htKeyword
          AutoSelect = False
          Color = clBtnFace
          DataField = 'LayerId'
          DataSource = dsBrowse
          ReadOnly = True
          TabOrder = 3
        end
        object DBEdit2: TDBEdit
          Left = 381
          Top = 6
          Width = 269
          Height = 21
          HelpType = htKeyword
          AutoSelect = False
          Color = clBtnFace
          DataField = 'LayerName'
          DataSource = dsBrowse
          ReadOnly = True
          TabOrder = 4
        end
      end
    end
  end
  inherited pnlTempBasDLLTop: TScrollBox
    object btExit: TSpeedButton [13]
      Left = 984
      Top = 0
      Width = 40
      Height = 39
      Align = alRight
      Caption = '&X'#38364#38281
      Glyph.Data = {
        36030000424D3603000000000000360000002800000010000000100000000100
        1800000000000003000000000000000000000000000000000000727282727078
        7C797DA4A2A7B1AFB77F8083C0C0BD8F8B88A8A0A89EA69A352C3AABA2AF9292
        929B99999B918E7474767F837EA1A2A0B9B8B8C4C3C1C0C2C2BCBEBCC2C2C1C6
        C4C3CBCBCB9092977B801B8D8E53BABABEC4CAC8B7B6B695938FBEC1B9D6D7D5
        DADBD5D3D3CED1D1D2D7D8D5CACACDBDBCBECDD2CF807D85BEBE149B9E004441
        2BB4BBBED6D9DACDCAC8928C94918F917475728D8D8D817F83797878ABACACC5
        C7C9D0D3CD8C8890B4AF12B3B40C5C5F00696A4B8B8A8D818284A8A59CB0B0A5
        A7AAA1A9AAA4B2B3A6B2B2A5585B53BCBFBECECECC8A8892ACAF11A8A80A807D
        045D540759504C91978FFFFFEBFFFFECFEFFEDFFFFEFEAE7D6CFC8BD615C5B58
        59595D5E5D464850B2B217AAA90A7B7A065F530D8A7D7AECECDDF3F1DDEEF0DA
        ECF5D8FFFFEEB8B1AB1C170E554E4BBDBCBEC4C7C77A8186B5B211ADAE04717E
        0256590A7B7276DDDACEF6F3DFFFFFEEE6E7D6E2E0DFB0A3A9485403474F21BA
        B5BCD2CDD1808685B5B310978B1E47471B505316777672DEDFCBFBFCEDB1B1B5
        534C5955535C302D42AEB11CA6A4094B4732CCCAC78D8A90AAA709908D3E4A4A
        39504A127B7572DFE0C899988C6667177B7C0C7E7D0A757A08D3D60CFFFB1687
        8906626541797880B6B61795A106596208585414787278DEDDD1576363A7A919
        EFF25DE2E66AF0F068F0F165F4F484E9F040565F0F585863BEBB1AABAA04807F
        0854540E737775DCDECDCCD2CB8D8C68766E5176775C5B5B48B5B446FCFB5A6A
        6A0E959B8B938D96B9AF0FB1A7097C780657560B777A6BDEE1CEFAFFE4EFF2ED
        E9E7E1F3F1EFAAA7B2A19F1B958C0D807E72D3D9D9817A86B5AE12B0AD097B7A
        025B570A7A7374DEDDD5F1F2DFF2F9DBEEF6DCFFFFEFB4B1AB3A3709545041BF
        C4C5B4BBB0AFB0AA848041B3B81C8B8F034D4E00797A73DEDDD2FCF3E2FAF3E2
        F0F1E8F5F7E7DDE2D4C3C7BE737572C4C6C6CECAC7CDD1D2C4C2BB979356C2D2
        2E616A0B707369DDDECEEFF8DFF8F3E3F5F2E6F1F7DEF3FBDDFFFFF1615B587A
        7E7A8381827A7C8286888779726A676C30605A259B9687F5F6E7}
      Layout = blGlyphTop
      Visible = False
      OnClick = btExitClick
      ExplicitLeft = 988
      ExplicitTop = 2
    end
    inherited pnl_PaperOrgTopNav120427: TPanel
      inherited nav2: TDBNavigator
        BeforeAction = nav2BeforeAction
      end
      inherited nav_120518A: TDBNavigator
        Left = 1
        Top = 19
        ExplicitLeft = 1
        ExplicitTop = 19
      end
    end
  end
  inherited qryExec: TADOQuery
    Left = 912
    Top = 80
  end
  inherited qryGetTranData: TADOQuery
    Left = 952
    Top = 120
  end
  inherited qryBrowse: TJSdTable
    Left = 934
    Top = 34
  end
  inherited qryDetail1: TJSdTable
    AfterEdit = qryDetail1AfterEdit
    Left = 846
    Top = 30
  end
  inherited dsBrowse: TDataSource
    Left = 906
    Top = 34
  end
  inherited dsDetail1: TDataSource
    Left = 850
    Top = 64
  end
  inherited qryDetail2: TJSdTable
    Left = 830
    Top = 30
  end
  inherited qryDetail3: TJSdTable
    Left = 814
    Top = 30
  end
  inherited qryDetail4: TJSdTable
    AfterPost = qryDetail4AfterPost
    Left = 798
    Top = 30
  end
  inherited qryDetail5: TJSdTable
    Left = 782
    Top = 30
  end
  inherited qryDetail6: TJSdTable
    Left = 766
    Top = 30
  end
  inherited qryDetail7: TJSdTable
    Left = 750
    Top = 30
  end
  inherited qryDetail8: TJSdTable
    Left = 734
    Top = 30
  end
  inherited dsDetail2: TDataSource
    Left = 834
    Top = 64
  end
  inherited dsDetail3: TDataSource
    Left = 818
    Top = 64
  end
  inherited dsDetail4: TDataSource
    Left = 802
    Top = 64
  end
  inherited dsDetail5: TDataSource
    Left = 786
    Top = 64
  end
  inherited dsDetail6: TDataSource
    Left = 770
    Top = 62
  end
  inherited dsDetail7: TDataSource
    Left = 754
    Top = 62
  end
  inherited dsDetail8: TDataSource
    Left = 738
    Top = 62
  end
  inherited pmuPaperPaper: TJSdPopupMenu
    Left = 40
    Top = 48
  end
  inherited pwgSaveToExcel: TJSdGrid2Excel
    Left = 16
    Top = 51
  end
  object qryNotesIN: TADOQuery
    Parameters = <
      item
        Name = 'Partnum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        DataType = ftString
        Size = 4
        Value = Null
      end
      item
        Name = 'LayerId'
        DataType = ftString
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdProcNotesIN :Partnum,:Revision,:LayerId'
      ' ')
    Left = 35
    Top = 167
  end
  object qryNotesOUT: TADOQuery
    Parameters = <
      item
        Name = 'Partnum'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'Revision'
        DataType = ftString
        Size = 4
        Value = Null
      end
      item
        Name = 'LayerId'
        DataType = ftString
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdProcNotesOUT :Partnum,:Revision,:LayerId'
      ' ')
    Left = 35
    Top = 211
  end
  object dsLayerBOM: TDataSource
    DataSet = tblLayerBOM
    Left = 170
    Top = 24
  end
  object tblLayerBOM: TJSdTable
    CursorLocation = clUseServer
    BeforeEdit = tblLayerBOMBeforeEdit
    AfterEdit = tblLayerBOMAfterEdit
    AfterPost = qryDetail1AfterPost
    DataSource = dsDetail1
    EnableBCD = False
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
        Size = 16
        Value = Null
      end>
    SQL.Strings = (
      'select t1.*,t2.MatName '
      'from dbo.EMOdLayerBOM t1,'
      'dbo.MINdMatInfo t2'
      'where t1.PartNum=:PartNum'
      'and t1.Revision= :Revision'
      'and t1.LayerId= :LayerId'
      'and t1.MatCode=t2.PartNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision;LayerId'
    MasterSource = dsDetail1
    TableName = 'dbo.EMOdLayerBOM'
    Left = 142
    Top = 26
  end
  object qryProdLayerBomdel: TADOQuery
    Parameters = <
      item
        Name = 'PartNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 12
        Value = Null
      end
      item
        Name = 'REVision'
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
        Name = 'ProcCode'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdProdLayerBomdel  :PartNum,:REVision,:LayerId,:ProcCode'
      '   ')
    Left = 39
    Top = 303
  end
  object qryProcCodeBOMSet: TADOQuery
    Parameters = <
      item
        Name = 'PartNum'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 12
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
        Name = 'ProcCode'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 12
        Value = Null
      end
      item
        Name = 'SerialNum'
        Attributes = [paSigned, paNullable]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end
      item
        Name = 'MatCode'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end
      item
        Name = 'MatName'
        Attributes = [paNullable]
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = 24
        Value = Null
      end>
    SQL.Strings = (
      
        'exec EMOdProcCodeBOMSet :PartNum, :Revision, :LayerId, :ProcCode' +
        ','
      ':SerialNum, :MatCode, :MatName'
      '  ')
    Left = 39
    Top = 355
  end
  object OpenDialog1: TOpenDialog
    Left = 244
    Top = 42
  end
  object qryPage: TADOQuery
    EnableBCD = False
    Parameters = <>
    Prepared = True
    Left = 1040
    Top = 96
  end
end

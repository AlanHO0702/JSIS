inherited frmProductDLL: TfrmProductDLL
  Caption = 'frmProductDLL'
  ClientHeight = 592
  ExplicitHeight = 619
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlInfo: TPanel
    Top = 568
    ExplicitTop = 568
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
      inherited Splitter1: TSplitter
        Top = 97
        ExplicitLeft = -3
        ExplicitTop = 79
        ExplicitWidth = 1081
      end
      object Splitter2: TSplitter [1]
        Left = 186
        Top = 102
        Width = 5
        Height = 346
        Color = clMedGray
        ParentColor = False
        ExplicitLeft = 113
        ExplicitTop = 183
        ExplicitHeight = 266
      end
      inherited pgeMaster: TPageControl
        Left = 191
        Top = 102
        Width = 61
        Height = 346
        ActivePage = tbshtMaster11
        Align = alClient
        TabOrder = 0
        OnChange = pgeMasterChange
        ExplicitLeft = 191
        ExplicitTop = 102
        ExplicitWidth = 61
        ExplicitHeight = 346
        inherited tbshtMaster1: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 253
          ExplicitWidth = 53
          ExplicitHeight = 89
          inherited pnlMaster1: TScrollBox
            Width = 53
            Height = 89
            ExplicitWidth = 53
            ExplicitHeight = 89
          end
        end
        inherited tbshtMaster2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 253
          ExplicitWidth = 53
          ExplicitHeight = 89
          inherited pnlMaster2: TScrollBox
            Width = 53
            Height = 89
            ExplicitWidth = 53
            ExplicitHeight = 89
          end
        end
        inherited tbshtMaster3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 253
          ExplicitWidth = 53
          ExplicitHeight = 89
          inherited pnlMaster3: TScrollBox
            Width = 53
            Height = 89
            ExplicitWidth = 53
            ExplicitHeight = 89
          end
        end
        inherited tbshtMaster4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 253
          ExplicitWidth = 53
          ExplicitHeight = 89
          inherited pnlMaster4: TScrollBox
            Width = 53
            Height = 89
            ExplicitWidth = 53
            ExplicitHeight = 89
          end
        end
        inherited tbshtAttach: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 253
          ExplicitWidth = 53
          ExplicitHeight = 89
          inherited pnlAttach: TPanel
            Width = 53
            Height = 89
            ExplicitWidth = 53
            ExplicitHeight = 89
            inherited pnlAttachBtn: TPanel
              Width = 53
              ExplicitWidth = 53
            end
            inherited dbgAttach: TJSdDBGrid
              Width = 53
              Height = 62
              ExplicitWidth = 53
              ExplicitHeight = 62
            end
          end
        end
        object tbshtMaster6: TTabSheet
          ImageIndex = 4
          TabVisible = False
          object pnlMaster6: TPanel
            Left = 0
            Top = 0
            Width = 53
            Height = 89
            Align = alClient
            BevelOuter = bvNone
            TabOrder = 0
          end
        end
        object tbshtMaster7: TTabSheet
          ImageIndex = 6
          object pnlMaster7: TScrollBox
            Left = 0
            Top = 0
            Width = 53
            Height = 89
            Align = alClient
            BorderStyle = bsNone
            TabOrder = 0
          end
        end
        object tbshtMaster8: TTabSheet
          ImageIndex = 7
          object pnlMaster8: TScrollBox
            Left = 0
            Top = 0
            Width = 53
            Height = 89
            Align = alClient
            BorderStyle = bsNone
            TabOrder = 0
          end
        end
        object tbshtMaster9: TTabSheet
          ImageIndex = 8
          object pnlMaster9: TScrollBox
            Left = 0
            Top = 0
            Width = 53
            Height = 89
            Align = alClient
            BorderStyle = bsNone
            TabOrder = 0
          end
        end
        object tbshtMaster10: TTabSheet
          ImageIndex = 9
          object pnlMaster10: TScrollBox
            Left = 0
            Top = 0
            Width = 53
            Height = 89
            Align = alClient
            BorderStyle = bsNone
            TabOrder = 0
          end
        end
        object tbshtMaster11: TTabSheet
          ImageIndex = 10
          object pnlMaster11: TScrollBox
            Left = 0
            Top = 0
            Width = 53
            Height = 89
            Align = alClient
            BorderStyle = bsNone
            TabOrder = 0
          end
          object DBMemo2: TDBMemo
            Left = 0
            Top = 0
            Width = 53
            Height = 89
            Align = alClient
            DataField = 'ProdHints'
            DataSource = dsBrowse
            ScrollBars = ssVertical
            TabOrder = 1
          end
        end
      end
      inherited pnlTempBasDLLBottom: TPanel [3]
        Top = 448
        TabOrder = 1
        OnDblClick = pnlTempBasDLLBottomDblClick
        ExplicitTop = 448
        inherited btnC4: TSpeedButton
          Caption = #24037#21934'P'#22294
          OnClick = btnC4Click
          ExplicitLeft = 346
          ExplicitTop = 1
          ExplicitHeight = 27
        end
        object btnCopyRoute: TSpeedButton
          Left = 765
          Top = 0
          Width = 85
          Height = 27
          Align = alLeft
          Caption = #36884#31243#35079#35069
          NumGlyphs = 2
          OnClick = btnCopyRouteClick
          ExplicitLeft = 806
          ExplicitTop = 2
          ExplicitHeight = 26
        end
        object btnFunction: TSpeedButton
          Left = 850
          Top = 0
          Width = 85
          Height = 27
          Align = alLeft
          Caption = #21151#33021
          NumGlyphs = 2
          OnClick = btnFunctionClick
          ExplicitLeft = 924
          ExplicitTop = -2
          ExplicitHeight = 26
        end
        object btnC9: TSpeedButton
          Left = 935
          Top = 0
          Width = 18
          Height = 27
          Align = alLeft
          NumGlyphs = 2
          Visible = False
          ExplicitLeft = 964
          ExplicitTop = 1
          ExplicitHeight = 26
        end
        object btnC10: TSpeedButton
          Left = 953
          Top = 0
          Width = 18
          Height = 27
          Align = alLeft
          NumGlyphs = 2
          Visible = False
          ExplicitLeft = 980
          ExplicitTop = 2
          ExplicitHeight = 26
        end
        object btnC11: TSpeedButton
          Left = 971
          Top = 0
          Width = 18
          Height = 27
          Align = alLeft
          NumGlyphs = 2
          Visible = False
          ExplicitLeft = 1000
          ExplicitTop = 1
          ExplicitHeight = 26
        end
        object btnC12: TSpeedButton
          Left = 989
          Top = 0
          Width = 18
          Height = 27
          Align = alLeft
          NumGlyphs = 2
          Visible = False
          ExplicitLeft = 1018
          ExplicitTop = 6
          ExplicitHeight = 26
        end
        object btnC13: TSpeedButton
          Left = 1007
          Top = 0
          Width = 18
          Height = 27
          Align = alLeft
          NumGlyphs = 2
          Visible = False
          ExplicitLeft = 1036
          ExplicitTop = 6
          ExplicitHeight = 26
        end
        object btnC14: TSpeedButton
          Left = 1025
          Top = 0
          Width = 18
          Height = 27
          Align = alLeft
          NumGlyphs = 2
          Visible = False
          ExplicitLeft = 1060
          ExplicitTop = -2
          ExplicitHeight = 26
        end
        object btnMU_Excel: TSpeedButton
          Left = 1043
          Top = 0
          Width = 85
          Height = 27
          Align = alLeft
          Caption = #21839#38988#22238#39243#21934
          NumGlyphs = 2
          OnClick = btnMU_ExcelClick
          ExplicitLeft = 1134
          ExplicitTop = 1
          ExplicitHeight = 26
        end
      end
      object pnlState: TPanel [4]
        Left = 0
        Top = 102
        Width = 186
        Height = 346
        Align = alLeft
        BevelOuter = bvNone
        TabOrder = 2
        object trvBOM: TJSdTreeView
          Left = 0
          Top = 0
          Width = 186
          Height = 197
          Align = alTop
          Images = ImageList1
          Indent = 19
          TabOrder = 0
          OnChange = trvBOMChange
          OnDblClick = trvBOMDblClick
          DataFieldId = 'LayerId'
          DataFieldLevelNo = 'Degree'
          DataFieldSuperId = 'AftLayerId'
          DataFieldCaption = 'LayerName'
          DataFieldSort = 'Sort'
          DataSource = dsProdLayer
          SetupExpand = True
          SortFieldInt = False
        end
        object pnlLayer: TPanel
          Left = 0
          Top = 197
          Width = 186
          Height = 28
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 1
          object btnLayer: TSpeedButton
            Left = 0
            Top = 0
            Width = 73
            Height = 28
            Align = alLeft
            Caption = #20839#37096#36039#26009
            Layout = blGlyphTop
            OnClick = btnLayerClick
            ExplicitLeft = 167
          end
        end
        object JSdDBGrid1: TJSdDBGrid
          Left = 0
          Top = 225
          Width = 186
          Height = 121
          IniAttributes.Delimiter = ';;'
          IniAttributes.UnicodeIniFile = False
          TitleColor = clBtnFace
          FixedCols = 0
          ShowHorzScrollBar = True
          Align = alClient
          DataSource = dsProdHIO
          Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
          ParentShowHint = False
          ShowHint = True
          TabOrder = 2
          TitleAlignment = taLeftJustify
          TitleFont.Charset = ANSI_CHARSET
          TitleFont.Color = clWindowText
          TitleFont.Height = -13
          TitleFont.Name = #32048#26126#39636
          TitleFont.Style = []
          TitleLines = 1
          TitleButtons = True
          SortColumnClick = stColumnClick
        end
      end
      inherited pgeDetail: TPageControl [5]
        Left = 20
        Top = 102
        Width = 1000
        Height = 346
        ActivePage = tbshtDetail3
        Align = alRight
        TabOrder = 3
        ExplicitLeft = 20
        ExplicitTop = 102
        ExplicitWidth = 1000
        ExplicitHeight = 346
        inherited tbshtDetail1: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 992
          ExplicitHeight = 314
          object Splitter5: TSplitter [0]
            Left = 0
            Top = 141
            Width = 992
            Height = 5
            Cursor = crVSplit
            Align = alTop
            Color = clMedGray
            ParentColor = False
            ExplicitLeft = 2
            ExplicitTop = 147
            ExplicitWidth = 665
          end
          inherited gridDetail1: TJSdDBGrid
            Top = 185
            Width = 992
            Height = 129
            ExplicitTop = 185
            ExplicitWidth = 992
            ExplicitHeight = 129
          end
          object pnlPressTools: TPanel
            Left = 0
            Top = 0
            Width = 992
            Height = 36
            Align = alTop
            Alignment = taLeftJustify
            BevelInner = bvLowered
            ParentColor = True
            TabOrder = 1
            object btnLayerPressUpdate: TSpeedButton
              Left = 297
              Top = 4
              Width = 90
              Height = 28
              Caption = #26448#26009#35373#23450
              OnClick = btnLayerPressUpdateClick
            end
            object btnLayerPressIns: TSpeedButton
              Left = 4
              Top = 4
              Width = 90
              Height = 28
              Caption = #36984#25799#22739#21512#27169#22411
              OnClick = btnLayerPressInsClick
            end
            object btnPressChange: TSpeedButton
              Left = 204
              Top = 4
              Width = 90
              Height = 28
              Caption = #30090#27083#35722#26356
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
              OnClick = btnPressChangeClick
            end
            object edtTmpPressId: TDBEdit
              Left = 97
              Top = 8
              Width = 104
              Height = 21
              DataField = 'TmpPressId'
              DataSource = dsBrowse
              ReadOnly = True
              TabOrder = 0
            end
            object chkJHCoreCom: TDBCheckBox
              Left = 433
              Top = 10
              Width = 97
              Height = 17
              Caption = #23458#25143#25351#23450
              TabOrder = 1
              ValueChecked = '1'
              ValueUnchecked = '0'
              Visible = False
            end
          end
          object Panel13: TPanel
            Left = 0
            Top = 146
            Width = 992
            Height = 39
            Align = alTop
            Alignment = taLeftJustify
            BevelInner = bvLowered
            ParentColor = True
            TabOrder = 2
            object SpeedButton3: TSpeedButton
              Left = 672
              Top = 36
              Width = 63
              Height = 33
              Caption = #22739#21512#20195#30908
              Glyph.Data = {
                42010000424D4201000000000000760000002800000011000000110000000100
                040000000000CC00000000000000000000001000000010000000000000000000
                BF0000BF000000BFBF00BF000000BF00BF00BFBF0000C0C0C000808080000000
                FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00777777777777
                777770000000777774444444444440000000777777777777777770000000778F
                744444777444400000007778F774477777447000000077778F77447777447000
                0000777778F774444444700000007777778F774477447000000077777778F774
                474470000000707770778F774444700000007087807778F77444700000007800
                0877778F774470000000770707777778F7777000000077070777777787777000
                0000778087777777777770000000777077777777777770000000777777777777
                777770000000}
              Visible = False
            end
            object btnProdUseMat: TSpeedButton
              Left = 4
              Top = 6
              Width = 90
              Height = 28
              Caption = #26448#26009#35373#23450
              OnClick = btnProdUseMatClick
            end
          end
          object dbgLayerPress: TJSdDBGrid
            Left = 0
            Top = 36
            Width = 992
            Height = 105
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alTop
            DataSource = dsLayerPress
            Options = [dgEditing, dgAlwaysShowEditor, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap]
            ParentShowHint = False
            ShowHint = True
            TabOrder = 3
            TitleAlignment = taLeftJustify
            TitleFont.Charset = ANSI_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -13
            TitleFont.Name = #32048#26126#39636
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = True
            SortColumnClick = stColumnClick
          end
        end
        inherited tbshtDetail2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 992
          ExplicitHeight = 314
          inherited gridDetail2: TJSdDBGrid
            Width = 992
            Height = 314
            ExplicitWidth = 992
            ExplicitHeight = 314
          end
        end
        inherited tbshtDetail3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 992
          ExplicitHeight = 314
          object Splitter3: TSplitter [0]
            Left = 225
            Top = 36
            Width = 5
            Height = 242
            Color = clMedGray
            ParentColor = False
            ExplicitLeft = 113
            ExplicitTop = 183
            ExplicitHeight = 266
          end
          inherited gridDetail3: TJSdDBGrid
            Top = 36
            Width = 225
            Height = 242
            Align = alLeft
            ExplicitTop = 36
            ExplicitWidth = 225
            ExplicitHeight = 242
          end
          object pnlMapTools: TPanel
            Left = 0
            Top = 278
            Width = 992
            Height = 36
            Align = alBottom
            BevelInner = bvLowered
            ParentColor = True
            ParentShowHint = False
            ShowHint = True
            TabOrder = 2
            Visible = False
            object Panel7: TPanel
              Left = 942
              Top = 2
              Width = 48
              Height = 32
              Align = alRight
              BevelOuter = bvNone
              TabOrder = 0
              OnDblClick = pnlMapToolsDblClick
            end
            object ToolBar1: TToolBar
              Left = 2
              Top = 2
              Width = 940
              Align = alClient
              ButtonHeight = 30
              Caption = 'ToolBar1'
              TabOrder = 1
              object pnlMapTool3: TPanel
                Left = 0
                Top = 0
                Width = 143
                Height = 30
                Align = alLeft
                BevelOuter = bvNone
                TabOrder = 0
                object lblWhatRev: TLabel
                  Left = 7
                  Top = 9
                  Width = 56
                  Height = 13
                  Caption = #28151#35009#29256#24207
                end
                object cboWhatRev: TwwDBLookupCombo
                  Left = 64
                  Top = 5
                  Width = 72
                  Height = 21
                  DropDownAlignment = taLeftJustify
                  DataField = 'ComboRev'
                  DataSource = dsBrowse
                  TabOrder = 0
                  AutoDropDown = False
                  ShowButton = True
                  PreciseEditRegion = False
                  AllowClearKey = False
                  OnEnter = cboWhatRevEnter
                  OnExit = cboWhatRevExit
                end
              end
              object pnlMapTool6: TPanel
                Left = 143
                Top = 0
                Width = 185
                Height = 30
                Align = alLeft
                BevelOuter = bvNone
                TabOrder = 1
                Visible = False
                object btnAllOutput: TSpeedButton
                  Left = 100
                  Top = 1
                  Width = 80
                  Height = 28
                  Caption = #20840#25976#36681#22294#27284
                  ParentShowHint = False
                  ShowHint = False
                  OnClick = btnAllOutputClick
                end
                object btnMapUpdate: TSpeedButton
                  Left = 2
                  Top = 1
                  Width = 96
                  Height = 28
                  Caption = #26356#26032#22294#29255#36039#26009
                  ParentShowHint = False
                  ShowHint = False
                  OnClick = btnMapUpdateClick
                end
              end
            end
          end
          object pnlXFlow: TPanel
            Left = 230
            Top = 36
            Width = 762
            Height = 242
            Align = alClient
            TabOrder = 3
            object XFlowDrawBox1: TXFlowDrawBox
              Left = 1
              Top = 1
              Width = 360
              Height = 240
              Align = alClient
              Font.Charset = ANSI_CHARSET
              Font.Color = clWindowText
              Font.Height = -32
              Font.Name = #26032#32048#26126#39636
              Font.Style = []
              ParentFont = False
              OnDblClick = XFlowDrawBox1DblClick
              XRate = 1.000000000000000000
              YRate = 1.000000000000000000
              ExplicitLeft = -174
              ExplicitWidth = 435
              ExplicitHeight = 178
            end
            object memoMap: TDBMemo
              Left = 361
              Top = 1
              Width = 400
              Height = 240
              Align = alRight
              DataField = 'MapData'
              DataSource = dsDetail3
              TabOrder = 0
              Visible = False
            end
            object meoMapData: TDBMemo
              Left = 87
              Top = 60
              Width = 262
              Height = 126
              DataField = 'MapData'
              DataSource = dsDetail3
              TabOrder = 1
              Visible = False
              WordWrap = False
            end
          end
          object pnlJPG: TPanel
            Left = 230
            Top = 36
            Width = 762
            Height = 242
            Align = alClient
            BevelOuter = bvNone
            TabOrder = 4
            object ImgPOP: TImage
              Left = 0
              Top = 0
              Width = 105
              Height = 105
              Stretch = True
            end
          end
          object pnlMapTools2: TPanel
            Left = 0
            Top = 0
            Width = 992
            Height = 36
            Align = alTop
            BevelInner = bvLowered
            ParentColor = True
            ParentShowHint = False
            ShowHint = True
            TabOrder = 1
            object pnlMapTool1: TPanel
              Left = 2
              Top = 2
              Width = 115
              Height = 32
              Align = alLeft
              BevelOuter = bvNone
              TabOrder = 0
              object btnAutoDraw: TSpeedButton
                Left = 4
                Top = 1
                Width = 106
                Height = 28
                Caption = #33258#21205#29986#29983#35009#26495#22294
                ParentShowHint = False
                ShowHint = False
                OnClick = btnAutoDrawClick
              end
            end
            object pnlMapTool2: TPanel
              Left = 117
              Top = 2
              Width = 117
              Height = 32
              Align = alLeft
              BevelOuter = bvNone
              TabOrder = 1
              object btnPrint: TSpeedButton
                Left = 5
                Top = 0
                Width = 106
                Height = 28
                Caption = #36681#22294#27284
                ParentShowHint = False
                ShowHint = False
                OnClick = btnPrintClick
              end
            end
            object pnlMapTool4: TPanel
              Left = 234
              Top = 2
              Width = 118
              Height = 32
              Align = alLeft
              BevelOuter = bvNone
              TabOrder = 2
              Visible = False
              object chkViewMapData: TCheckBox
                Left = 6
                Top = 7
                Width = 106
                Height = 17
                Caption = #26597#30475#21407#22987#36039#26009
                TabOrder = 0
              end
            end
            object pnlMapTool5: TPanel
              Left = 352
              Top = 2
              Width = 81
              Height = 32
              Align = alLeft
              BevelOuter = bvNone
              TabOrder = 3
              object btnSaveMapTmp: TSpeedButton
                Left = 6
                Top = 1
                Width = 72
                Height = 28
                Caption = #23384#28858#27169#29256
                ParentShowHint = False
                ShowHint = False
                OnClick = btnSaveMapTmpClick
              end
            end
          end
        end
        inherited tbshtDetail4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 992
          ExplicitHeight = 314
          object SplitterMerge: TSplitter [0]
            Left = 0
            Top = 145
            Width = 992
            Height = 3
            Cursor = crVSplit
            Align = alTop
            Visible = False
            ExplicitTop = 0
            ExplicitWidth = 103
          end
          object Splitter4: TSplitter [1]
            Left = 417
            Top = 148
            Width = 5
            Height = 166
            Color = clMedGray
            ParentColor = False
            ExplicitLeft = 443
            ExplicitTop = 116
            ExplicitHeight = 144
          end
          object pnlPartMergePrint: TPanel [2]
            Left = 0
            Top = 0
            Width = 992
            Height = 145
            Align = alTop
            BevelOuter = bvNone
            TabOrder = 0
            Visible = False
            object dbgPartMergePrint: TJSdDBGrid
              Left = 0
              Top = 0
              Width = 992
              Height = 145
              IniAttributes.Delimiter = ';;'
              IniAttributes.UnicodeIniFile = False
              TitleColor = clBtnFace
              FixedCols = 0
              ShowHorzScrollBar = True
              Align = alClient
              DataSource = dsPartMergePrint
              Options = [dgEditing, dgAlwaysShowEditor, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgWordWrap, dgFixedResizable, dgFixedEditable]
              ParentShowHint = False
              ReadOnly = True
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
              SortColumnClick = stColumnClick
            end
          end
          object grdHole: TwwDBGrid [3]
            Left = 422
            Top = 148
            Width = 570
            Height = 166
            ControlType.Strings = (
              'NumOfPCS;CheckBox;1;0'
              'NeedPTH;CheckBox;Y;N'
              'IsBaseHole;CheckBox;1;0'
              'IsLaserHole;CheckBox;1;0'
              'NPTH;CheckBox;Y;N')
            Selected.Strings = (
              'PartNum'#9'12'#9#27597#26009#34399#9'F'#9
              'Revision'#9'4'#9#29256#24207#9'F'#9)
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alClient
            DataSource = dsPartMatri
            ParentColor = True
            ReadOnly = True
            TabOrder = 1
            TitleAlignment = taLeftJustify
            TitleFont.Charset = ANSI_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -13
            TitleFont.Name = #32048#26126#39636
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = False
            object grdHoleIButton: TwwIButton
              Left = 0
              Top = 0
              Width = 13
              Height = 22
              AllowAllUp = True
              Visible = False
            end
          end
          inherited gridDetail4: TJSdDBGrid
            Top = 148
            Width = 417
            Height = 166
            Align = alLeft
            TabOrder = 2
            ExplicitTop = 148
            ExplicitWidth = 417
            ExplicitHeight = 166
          end
        end
        inherited tbshtDetail5: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 992
          ExplicitHeight = 314
          inherited gridDetail5: TJSdDBGrid
            Width = 992
            Height = 314
            ExplicitWidth = 992
            ExplicitHeight = 314
          end
        end
        inherited tbshtDetail6: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 992
          ExplicitHeight = 314
          object SplMemoDtl: TSplitter [0]
            Left = 745
            Top = 185
            Width = 4
            Height = 129
            ExplicitLeft = 614
            ExplicitHeight = 131
          end
          inherited gridDetail6: TJSdDBGrid
            Top = 185
            Width = 745
            Height = 129
            Align = alLeft
            ExplicitTop = 185
            ExplicitWidth = 745
            ExplicitHeight = 129
          end
          object pnlModifyTools: TPanel
            Left = 0
            Top = 0
            Width = 992
            Height = 30
            Align = alTop
            BevelInner = bvLowered
            ParentColor = True
            TabOrder = 1
            object btnModifyExl: TSpeedButton
              Left = 181
              Top = 3
              Width = 117
              Height = 26
              Caption = #36681'EXCEL'
              OnClick = btnModifyExlClick
            end
            object btnModifySet: TSpeedButton
              Left = 298
              Top = 3
              Width = 115
              Height = 26
              Caption = #35373#35722#23529#26680
              NumGlyphs = 2
              OnClick = btnModifySetClick
            end
            object Panel22: TJSdLabel
              Left = 14
              Top = 7
              Width = 161
              Height = 24
              AutoSize = False
              Caption = #23578#26410#35373#35336#23436#25104#38917#30446
            end
          end
          object Panel2: TPanel
            Left = 0
            Top = 30
            Width = 992
            Height = 155
            Align = alTop
            BevelOuter = bvNone
            TabOrder = 2
            object Splitter9: TSplitter
              Left = 0
              Top = 151
              Width = 992
              Height = 4
              Cursor = crVSplit
              Align = alBottom
              ExplicitTop = 152
              ExplicitWidth = 665
            end
            object Panel3: TPanel
              Left = 0
              Top = 121
              Width = 992
              Height = 30
              Align = alBottom
              BevelInner = bvLowered
              ParentColor = True
              TabOrder = 0
              Visible = False
              object JSdLabel2: TJSdLabel
                Left = 14
                Top = 9
                Width = 56
                Height = 13
                Caption = #20633#35387#38917#30446
              end
              object btnUpdNote: TSpeedButton
                Left = 94
                Top = 2
                Width = 115
                Height = 26
                Caption = #20462#25913
                NumGlyphs = 2
                Visible = False
                OnClick = btnUpdNoteClick
              end
              object NavUpdNote: TDBNavigator
                Left = 219
                Top = 5
                Width = 116
                Height = 19
                DataSource = dsDetail6
                VisibleButtons = [nbInsert, nbDelete, nbPost, nbCancel]
                Hints.Strings = (
                  #39318#31558
                  #19978#19968#31558
                  #19979#19968#31558
                  #26411#31558
                  #26032#22686#26126#32048
                  #21034#38500#26126#32048
                  #32232#36655#26126#32048
                  #26126#32048#23384#27284
                  #26126#32048#21462#28040
                  #26126#32048#26356#26032)
                ParentShowHint = False
                ShowHint = True
                TabOrder = 0
              end
            end
            object dbgModify: TJSdDBGrid
              Left = 0
              Top = 0
              Width = 992
              Height = 121
              IniAttributes.Delimiter = ';;'
              IniAttributes.UnicodeIniFile = False
              TitleColor = clBtnFace
              FixedCols = 0
              ShowHorzScrollBar = True
              Align = alClient
              DataSource = dsModify
              Options = [dgEditing, dgAlwaysShowEditor, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgFixedResizable, dgFixedEditable]
              ParentShowHint = False
              ShowHint = True
              TabOrder = 1
              TitleAlignment = taLeftJustify
              TitleFont.Charset = ANSI_CHARSET
              TitleFont.Color = clWindowText
              TitleFont.Height = -13
              TitleFont.Name = #32048#26126#39636
              TitleFont.Style = []
              TitleLines = 1
              TitleButtons = True
              SortColumnClick = stColumnClick
            end
          end
          object MemoUpdateDtl: TDBMemo
            Left = 749
            Top = 185
            Width = 243
            Height = 129
            Align = alClient
            DataField = 'Notes'
            DataSource = dsDetail6
            TabOrder = 3
          end
        end
        inherited tbshtDetail7: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 992
          ExplicitHeight = 314
          inherited gridDetail7: TJSdDBGrid
            Width = 992
            Height = 314
            DataSource = dsDetail7B
            ExplicitWidth = 992
            ExplicitHeight = 314
          end
        end
        inherited tbshtDetail8: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 992
          ExplicitHeight = 314
          inherited gridDetail8: TJSdDBGrid
            Width = 992
            Height = 314
            ExplicitWidth = 992
            ExplicitHeight = 314
          end
        end
        object tbshtDetail9: TTabSheet
          Caption = #35069#20316#26041#24335
          ImageIndex = 8
          object Splitter7: TSplitter
            Left = 265
            Top = 36
            Width = 5
            Height = 204
            Color = clMedGray
            ParentColor = False
            ExplicitLeft = 113
            ExplicitTop = 183
            ExplicitHeight = 266
          end
          object pnlTierTools: TPanel
            Left = 0
            Top = 0
            Width = 992
            Height = 36
            Align = alTop
            BevelInner = bvLowered
            ParentColor = True
            TabOrder = 0
            object btnTierIns: TSpeedButton
              Left = 5
              Top = 4
              Width = 112
              Height = 28
              Caption = #36681#20837#38928#35373
              OnClick = btnTierInsClick
            end
            object btnJHPressChg: TSpeedButton
              Left = 173
              Top = 4
              Width = 112
              Height = 28
              Caption = 'oz'#23565#35519
              Visible = False
              OnClick = btnJHPressChgClick
            end
          end
          object Panel14: TPanel
            Left = 0
            Top = 240
            Width = 992
            Height = 74
            Align = alBottom
            BevelInner = bvLowered
            TabOrder = 1
            Visible = False
            object btnCMap: TSpeedButton
              Left = 95
              Top = 8
              Width = 76
              Height = 25
              Caption = #19978#20659#27284#26696
              OnClick = btnCMapClick
            end
            object btnCMapOpen: TSpeedButton
              Left = 177
              Top = 8
              Width = 76
              Height = 25
              Caption = #27298#35222
              OnClick = btnCMapOpenClick
            end
            object Label129: TJSdLabel
              Left = 35
              Top = 40
              Width = 56
              Height = 13
              Alignment = taRightJustify
              Caption = #19968#27425#25104#22411
              DataField = 'CMapPath'
              DataSource = dsBrowse
            end
            object btnSMapOpen: TSpeedButton
              Left = 560
              Top = 8
              Width = 76
              Height = 25
              Caption = #27298#35222
              OnClick = btnCMapOpenClick
            end
            object btnSMap: TSpeedButton
              Left = 478
              Top = 8
              Width = 76
              Height = 25
              Caption = #19978#20659#27284#26696
              OnClick = btnCMapClick
            end
            object Label130: TJSdLabel
              Left = 417
              Top = 40
              Width = 56
              Height = 13
              Alignment = taRightJustify
              Caption = #20108#27425#25104#22411
              DataField = 'SMapPath'
              DataSource = dsBrowse
            end
            object DBEdit107: TDBEdit
              Left = 477
              Top = 37
              Width = 260
              Height = 21
              DataField = 'SMapPath'
              DataSource = dsBrowse
              TabOrder = 0
            end
            object DBEdit106: TDBEdit
              Left = 95
              Top = 37
              Width = 260
              Height = 21
              DataField = 'CMapPath'
              DataSource = dsBrowse
              TabOrder = 1
            end
          end
          object dbgProdTier: TJSdDBGrid
            Left = 0
            Top = 36
            Width = 265
            Height = 204
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alLeft
            DataSource = dsDetail9
            Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
            ParentShowHint = False
            ShowHint = True
            TabOrder = 2
            TitleAlignment = taLeftJustify
            TitleFont.Charset = ANSI_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -13
            TitleFont.Name = #32048#26126#39636
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = True
            SortColumnClick = stColumnClick
          end
          object pnlLayerMap: TPanel
            Left = 270
            Top = 36
            Width = 722
            Height = 204
            Align = alClient
            BevelOuter = bvNone
            TabOrder = 3
            object ScrollBox1: TScrollBox
              Left = 0
              Top = 0
              Width = 722
              Height = 204
              Align = alClient
              TabOrder = 0
              object ImgLayer: TImage
                Left = 0
                Top = 0
                Width = 105
                Height = 105
                AutoSize = True
              end
            end
          end
          object pnlXflow2: TPanel
            Left = 270
            Top = 36
            Width = 722
            Height = 204
            Align = alClient
            BevelOuter = bvNone
            TabOrder = 4
            object XFlowDrawBox2: TXFlowDrawBox
              Left = 0
              Top = 0
              Width = 722
              Height = 204
              Align = alClient
              Font.Charset = ANSI_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = #26032#32048#26126#39636
              Font.Style = []
              ParentFont = False
              XRate = 1.000000000000000000
              YRate = 1.000000000000000000
              ExplicitLeft = 271
              ExplicitTop = 36
              ExplicitWidth = 395
              ExplicitHeight = 206
            end
          end
        end
        object tbshtDetail10: TTabSheet
          Caption = #20839#23652#36884#31243
          ImageIndex = 9
          object Splitter6: TSplitter
            Left = 578
            Top = 32
            Width = 4
            Height = 282
            ExplicitLeft = 574
            ExplicitTop = 38
            ExplicitHeight = 284
          end
          object pnlRouteTools: TPanel
            Left = 0
            Top = 0
            Width = 992
            Height = 32
            Align = alTop
            BevelInner = bvLowered
            ParentColor = True
            TabOrder = 0
            object btnChangeProc: TSpeedButton
              Left = 152
              Top = 2
              Width = 75
              Height = 28
              Caption = '&C'#36884#31243#30064#21205
              Flat = True
              OnClick = btnChangeProcClick
            end
            object btnBackupNotes: TSpeedButton
              Left = 306
              Top = 2
              Width = 75
              Height = 28
              Caption = #35079#35069#20633#35387
              Flat = True
              OnClick = btnBackupNotesClick
            end
            object btnPasteNotes: TSpeedButton
              Left = 383
              Top = 2
              Width = 75
              Height = 28
              Caption = #36028#19978#20633#35387
              Flat = True
              OnClick = btnPasteNotesClick
            end
            object btnNotesStyleTree: TSpeedButton
              Left = 229
              Top = 2
              Width = 75
              Height = 28
              Caption = '&C'#36884#31243#20633#35387
              Flat = True
              OnClick = btnNotesStyleTreeClick
            end
            object btnRouteChange: TSpeedButton
              Left = 462
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
              OnClick = btnRouteChangeClick
            end
            object btnRouteBOMSet: TSpeedButton
              Left = 572
              Top = 2
              Width = 90
              Height = 28
              Caption = #35069#31243#26448#26009#35373#23450
              OnClick = btnRouteBOMSetClick
            end
            object DBNavigator2: TDBNavigator
              Left = 884
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
            object DBEdit1: TDBEdit
              Left = 57
              Top = 5
              Width = 94
              Height = 21
              DataField = 'TmpRouteId'
              DataSource = dsTmpRouteId
              ReadOnly = True
              TabOrder = 2
            end
            object pnlUseNotes: TPanel
              Left = 668
              Top = 2
              Width = 188
              Height = 28
              BevelOuter = bvNone
              TabOrder = 3
              Visible = False
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
                DataSource = dsTmpRouteId
                ReadOnly = True
                TabOrder = 0
                ValueChecked = '1'
                ValueUnchecked = '0'
              end
            end
          end
          object dbgRoute: TJSdDBGrid
            Left = 0
            Top = 32
            Width = 578
            Height = 282
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alLeft
            DataSource = dsDetail10
            Options = [dgEditing, dgAlwaysShowEditor, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap]
            ParentShowHint = False
            ShowHint = True
            TabOrder = 1
            TitleAlignment = taLeftJustify
            TitleFont.Charset = ANSI_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -13
            TitleFont.Name = #32048#26126#39636
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = True
            OnEnter = dbgRouteEnter
            SortColumnClick = stColumnClick
          end
          object Panel1: TPanel
            Left = 582
            Top = 32
            Width = 410
            Height = 282
            Align = alClient
            BevelOuter = bvNone
            TabOrder = 2
            object Splitter8: TSplitter
              Left = 0
              Top = 222
              Width = 410
              Height = 4
              Cursor = crVSplit
              Align = alBottom
              ExplicitTop = 125
              ExplicitWidth = 185
            end
            object Splitter10: TSplitter
              Left = 0
              Top = 72
              Width = 410
              Height = 4
              Cursor = crVSplit
              Align = alTop
              ExplicitLeft = -8
              ExplicitTop = 134
              ExplicitWidth = 310
            end
            object dbgBOM: TJSdDBGrid
              Left = 0
              Top = 76
              Width = 410
              Height = 146
              IniAttributes.Delimiter = ';;'
              IniAttributes.UnicodeIniFile = False
              TitleColor = clBtnFace
              FixedCols = 0
              ShowHorzScrollBar = True
              Align = alClient
              DataSource = dsDetail11
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
              OnEnter = dbgBOMEnter
              SortColumnClick = stColumnClick
            end
            object pnlPath: TPanel
              Left = 0
              Top = 226
              Width = 410
              Height = 56
              Align = alBottom
              BevelOuter = bvNone
              TabOrder = 1
              object lab_annex: TJSdLabel
                Left = 6
                Top = 5
                Width = 28
                Height = 13
                AutoSize = False
                Caption = #38468#20214
                DataField = 'NotesPath'
                DataSource = dsDetail10
              end
              object btnRouteNote: TSpeedButton
                Left = 6
                Top = 27
                Width = 76
                Height = 25
                Caption = #35079#35069#27284#26696
                Visible = False
                OnClick = btnRouteNoteClick
              end
              object btnOpenRouteNote: TSpeedButton
                Left = 88
                Top = 27
                Width = 76
                Height = 25
                Caption = #27298#35222
                OnClick = btnOpenRouteNoteClick
              end
              object DBEdit2: TDBEdit
                Left = 38
                Top = 2
                Width = 267
                Height = 21
                DataField = 'NotesPath'
                DataSource = dsDetail10
                TabOrder = 0
              end
            end
            object pnlRouteNote: TPanel
              Left = 0
              Top = 0
              Width = 410
              Height = 72
              Align = alTop
              BevelOuter = bvNone
              TabOrder = 2
              object Splitter11: TSplitter
                Left = 137
                Top = 0
                Width = 5
                Height = 72
                ExplicitLeft = 0
              end
              object DBMemo1: TDBMemo
                Left = 0
                Top = 0
                Width = 137
                Height = 72
                Align = alLeft
                DataField = 'Notes'
                DataSource = dsDetail10
                TabOrder = 0
              end
              object pnlNoteSep: TPanel
                Left = 142
                Top = 0
                Width = 268
                Height = 72
                Align = alClient
                BevelOuter = bvNone
                TabOrder = 1
              end
            end
          end
        end
        object tbshtDetail11: TTabSheet
          Caption = #29986#21697#24037#31243#22294
          ImageIndex = 10
          object pnlMap: TPanel
            Left = 0
            Top = 0
            Width = 992
            Height = 30
            Align = alTop
            BevelOuter = bvLowered
            ParentColor = True
            ParentShowHint = False
            ShowHint = True
            TabOrder = 0
            object btnViewMap: TSpeedButton
              Left = 1
              Top = 1
              Width = 125
              Height = 28
              Align = alLeft
              Caption = #27298#35222#22294#27284'('#38283#35222#31383')'
              OnClick = btnViewMapClick
              ExplicitLeft = 341
            end
            object btnToGlyph: TSpeedButton
              Left = 476
              Top = 1
              Width = 171
              Height = 28
              Align = alRight
              Caption = #27298#35222#22294#27284'('#36339#33267#22294#27284#38913#31844')'
              Visible = False
              ExplicitLeft = 477
            end
            object lblParam: TLabel
              Left = 223
              Top = 8
              Width = 126
              Height = 13
              Caption = 'Change Form Height'
              Visible = False
            end
            object Panel5: TPanel
              Left = 647
              Top = 1
              Width = 344
              Height = 28
              Align = alRight
              BevelOuter = bvNone
              TabOrder = 0
              object btnSaveMap: TSpeedButton
                Left = 167
                Top = 0
                Width = 170
                Height = 28
                Caption = #23384#20837#22294#27284#23436#25972#36335#24465#21450#27284#21517
                Visible = False
              end
              object navProdMap: TDBNavigator
                Left = 0
                Top = 0
                Width = 165
                Height = 28
                DataSource = dsMGNMap
                VisibleButtons = [nbInsert, nbDelete, nbPost, nbCancel, nbRefresh]
                Align = alLeft
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
                TabOrder = 0
                Visible = False
              end
            end
          end
          object grdMap: TJSdDBGrid
            Left = 0
            Top = 30
            Width = 992
            Height = 284
            IniAttributes.Delimiter = ';;'
            IniAttributes.UnicodeIniFile = False
            TitleColor = clBtnFace
            FixedCols = 0
            ShowHorzScrollBar = True
            Align = alClient
            DataSource = dsMGNMap
            Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
            ParentShowHint = False
            ShowHint = True
            TabOrder = 1
            TitleAlignment = taLeftJustify
            TitleFont.Charset = ANSI_CHARSET
            TitleFont.Color = clWindowText
            TitleFont.Height = -13
            TitleFont.Name = #32048#26126#39636
            TitleFont.Style = []
            TitleLines = 1
            TitleButtons = True
            SortColumnClick = stColumnClick
          end
        end
      end
      object pnlMaster5: TPanel
        Left = 0
        Top = 0
        Width = 1020
        Height = 97
        Align = alTop
        BevelInner = bvRaised
        BevelOuter = bvLowered
        TabOrder = 4
        object pgeFormType: TPageControl
          Left = 2
          Top = 70
          Width = 1016
          Height = 25
          ActivePage = tstMain
          Align = alBottom
          TabOrder = 0
          OnChange = pgeFormTypeChange
          object tstMain: TTabSheet
            Caption = #22522#26412#35215#26684
          end
          object tstSub: TTabSheet
            Caption = #20854#20182#26126#32048
            ImageIndex = 1
          end
        end
      end
      object pnlMills: TPanel
        Left = 154
        Top = 167
        Width = 250
        Height = 178
        BevelOuter = bvLowered
        TabOrder = 5
        Visible = False
        object Panel4: TPanel
          Left = 1
          Top = 1
          Width = 248
          Height = 28
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 0
          object lblWhere1: TLabel
            Left = 142
            Top = 9
            Width = 7
            Height = 13
            Caption = '0'
            Visible = False
          end
          object navMills: TDBNavigator
            Left = 0
            Top = 0
            Width = 100
            Height = 28
            DataSource = dsMills
            VisibleButtons = [nbInsert, nbDelete, nbPost, nbCancel]
            Align = alLeft
            Hints.Strings = (
              #39318#31558
              #19978#19968#31558
              #19979#19968#31558
              #26411#31558
              #26032#22686#26126#32048
              #21034#38500#26126#32048
              #32232#36655#26126#32048
              #26126#32048#23384#27284
              #26126#32048#21462#28040
              #26126#32048#26356#26032)
            ParentShowHint = False
            ShowHint = True
            TabOrder = 0
          end
        end
        object dbgMills: TJSdDBGrid
          Left = 1
          Top = 29
          Width = 248
          Height = 136
          IniAttributes.Delimiter = ';;'
          IniAttributes.UnicodeIniFile = False
          TitleColor = clBtnFace
          FixedCols = 0
          ShowHorzScrollBar = True
          Align = alTop
          DataSource = dsMills
          Options = [dgEditing, dgAlwaysShowEditor, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap]
          ParentShowHint = False
          ShowHint = True
          TabOrder = 1
          TitleAlignment = taLeftJustify
          TitleFont.Charset = ANSI_CHARSET
          TitleFont.Color = clWindowText
          TitleFont.Height = -13
          TitleFont.Name = #32048#26126#39636
          TitleFont.Style = []
          TitleLines = 1
          TitleButtons = True
          SortColumnClick = stColumnClick
        end
      end
      object pnlWriting: TPanel
        Left = 192
        Top = 234
        Width = 250
        Height = 178
        BevelOuter = bvLowered
        TabOrder = 6
        Visible = False
        object Panel6: TPanel
          Left = 1
          Top = 1
          Width = 248
          Height = 28
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 0
          object lblWhere2: TLabel
            Left = 141
            Top = 9
            Width = 7
            Height = 13
            Caption = '0'
            Visible = False
          end
          object navWriting: TDBNavigator
            Left = 0
            Top = 0
            Width = 100
            Height = 28
            DataSource = dsWriting
            VisibleButtons = [nbInsert, nbDelete, nbPost, nbCancel]
            Align = alLeft
            Hints.Strings = (
              #39318#31558
              #19978#19968#31558
              #19979#19968#31558
              #26411#31558
              #26032#22686#26126#32048
              #21034#38500#26126#32048
              #32232#36655#26126#32048
              #26126#32048#23384#27284
              #26126#32048#21462#28040
              #26126#32048#26356#26032)
            ParentShowHint = False
            ShowHint = True
            TabOrder = 0
          end
        end
        object dbgWriting: TJSdDBGrid
          Left = 1
          Top = 29
          Width = 248
          Height = 136
          IniAttributes.Delimiter = ';;'
          IniAttributes.UnicodeIniFile = False
          TitleColor = clBtnFace
          FixedCols = 0
          ShowHorzScrollBar = True
          Align = alTop
          DataSource = dsWriting
          Options = [dgEditing, dgAlwaysShowEditor, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap]
          ParentShowHint = False
          ShowHint = True
          TabOrder = 1
          TitleAlignment = taLeftJustify
          TitleFont.Charset = ANSI_CHARSET
          TitleFont.Color = clWindowText
          TitleFont.Height = -13
          TitleFont.Name = #32048#26126#39636
          TitleFont.Style = []
          TitleLines = 1
          TitleButtons = True
          SortColumnClick = stColumnClick
        end
      end
    end
  end
  inherited pnlTempBasDLLTop: TScrollBox
    inherited btnView: TSpeedButton
      ExplicitLeft = 232
    end
    inherited btnInq: TSpeedButton
      ExplicitLeft = 186
    end
    inherited btnRejExam: TSpeedButton
      ExplicitLeft = 508
    end
    inherited btnAdd: TSpeedButton
      ExplicitLeft = 278
    end
    inherited btnVoid: TSpeedButton
      ExplicitLeft = 416
    end
    inherited btnPrintPaper: TSpeedButton
      ExplicitLeft = 554
    end
    inherited btnKeepStatus: TSpeedButton
      ExplicitLeft = 370
    end
    inherited btnCompleted: TSpeedButton
      ExplicitLeft = 462
    end
    inherited btnUpdate: TSpeedButton
      ExplicitLeft = 324
    end
    inherited btnToExcel: TSpeedButton
      ExplicitLeft = 600
    end
    inherited btnLink: TSpeedButton
      ExplicitLeft = 646
    end
    inherited btnLog: TSpeedButton
      ExplicitLeft = 692
    end
    inherited btnPaperOrgTopFunction: TSpeedButton
      ExplicitLeft = 738
    end
    object btnFinish: TSpeedButton [13]
      Left = 834
      Top = 0
      Width = 46
      Height = 39
      Align = alLeft
      Glyph.Data = {
        36060000424D3606000000000000360400002800000020000000100000000100
        08000000000000020000130B0000130B0000000100000001000000000000707F
        7F00737F7F007F7F7F000000FF0000808000FFFFFF0000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000000000000000
        0000000000000000000000000000000000000000000000000000050505050505
        0505050505050505050505050505050506060606060505050505050505050504
        0404040405050505050505050505060303030303060606050505050505040404
        0404040404040505050505050503030305060603030306060505050504040405
        0400050504040405050505050303050503030606050303060605050404040504
        0404000505040404050505030305050303030306050503030605050404050504
        0404000505050404050505030605050303030306060505030606040405050404
        0404040005050504040503030605030303030303060505030306040405040404
        0404040005050504040503030603030303030303060605030306040402040400
        0504040400050504040503030303030305010303030605030306040304000505
        0505040400050504040503030303050505050303030606030306040405050505
        0505040404000504040503030606050505050303030306030305050404050505
        0505050404000404050505030506060505050503030306060605050404040505
        0505050504040004050505030305060605050505030303060605050504040405
        0505050504030400050505050303050606060606060303030606050505040404
        0404040404040304000505050503030305050503030303030306050505050504
        0404040405050505040400050505050303030303050505050303}
      Layout = blGlyphTop
      NumGlyphs = 2
      Visible = False
      OnClick = btnFinishClick
      ExplicitLeft = 1196
      ExplicitTop = 2
    end
    inherited pnl_PaperOrgTopNav120427: TPanel
      inherited nav1: TDBNavigator
        OnClick = nav1Click
      end
    end
  end
  object pnlFunction: TPanel [4]
    Left = 856
    Top = 300
    Width = 91
    Height = 195
    BevelInner = bvLowered
    ParentBackground = False
    TabOrder = 5
    object btnHideFunction: TSpeedButton
      Left = 3
      Top = 165
      Width = 85
      Height = 26
      Caption = #38364' '#38281
      NumGlyphs = 2
      OnClick = btnHideFunctionClick
    end
    object btnFunc2: TSpeedButton
      Left = 3
      Top = 30
      Width = 85
      Height = 26
      Caption = '----'
      Layout = blGlyphTop
      NumGlyphs = 2
      OnClick = btnC1Click
    end
    object btnFunc1: TSpeedButton
      Left = 3
      Top = 3
      Width = 85
      Height = 26
      Caption = '----'
      Layout = blGlyphTop
      OnClick = btnC1Click
    end
    object btnFunc3: TSpeedButton
      Left = 3
      Top = 57
      Width = 85
      Height = 26
      Caption = '----'
      Layout = blGlyphTop
      OnClick = btnC1Click
    end
    object btnFunc4: TSpeedButton
      Left = 3
      Top = 84
      Width = 85
      Height = 26
      Caption = '----'
      NumGlyphs = 2
      OnClick = btnC1Click
    end
    object btnFunc5: TSpeedButton
      Left = 3
      Top = 111
      Width = 85
      Height = 26
      Caption = '----'
      NumGlyphs = 2
      OnClick = btnC1Click
    end
    object btnFunc6: TSpeedButton
      Left = 3
      Top = 138
      Width = 85
      Height = 26
      Caption = '----'
      NumGlyphs = 2
      OnClick = btnC1Click
    end
  end
  inherited pnl_PaperOrgTopExTool120430: TPanel
    inherited btnExam: TSpeedButton
      ExplicitTop = 52
      ExplicitWidth = 88
    end
    inherited btnPrintList: TSpeedButton
      Visible = False
    end
  end
  inherited qryExec: TADOQuery
    Left = 1000
    Top = 248
  end
  inherited qryGetTranData: TADOQuery
    Left = 1000
    Top = 208
  end
  inherited qryBrowse: TJSdTable
    AfterClose = qryBrowseAfterClose
    Left = 902
    Top = 90
  end
  inherited qryDetail1: TJSdTable
    AfterEdit = qryDetail1AfterEdit
    Left = 822
    Top = 30
  end
  inherited dsBrowse: TDataSource
    Left = 930
    Top = 90
  end
  inherited dsDetail1: TDataSource
    Left = 826
    Top = 56
  end
  inherited qryDetail2: TJSdTable
    Left = 806
    Top = 30
  end
  inherited qryDetail3: TJSdTable
    AfterOpen = qryDetail3AfterOpen
    AfterScroll = qryDetail3AfterScroll
    Left = 790
    Top = 30
  end
  inherited qryDetail4: TJSdTable
    AfterEdit = qryDetail1AfterEdit
    Left = 774
    Top = 30
  end
  inherited qryDetail5: TJSdTable
    Left = 758
    Top = 30
  end
  inherited qryDetail6: TJSdTable
    AfterInsert = qryDetail6AfterInsert
    Left = 742
    Top = 30
  end
  inherited qryDetail7: TJSdTable
    AfterOpen = qryDetail7AfterOpen
    AfterClose = qryDetail7AfterClose
    Left = 726
    Top = 30
  end
  inherited qryDetail8: TJSdTable
    AfterOpen = qryDetail8AfterOpen
    AfterClose = qryDetail8AfterClose
    Left = 710
    Top = 30
  end
  inherited dsDetail2: TDataSource
    Left = 810
    Top = 56
  end
  inherited dsDetail3: TDataSource
    Left = 794
    Top = 56
  end
  inherited dsDetail4: TDataSource
    Left = 778
    Top = 56
  end
  inherited dsDetail5: TDataSource
    Left = 762
    Top = 56
  end
  inherited dsDetail6: TDataSource
    Left = 746
    Top = 54
  end
  inherited dsDetail7: TDataSource
    Left = 730
    Top = 54
  end
  inherited dsDetail8: TDataSource
    Left = 714
    Top = 54
  end
  inherited pmuPaperPaper: TJSdPopupMenu
    Left = 936
    Top = 32
  end
  inherited pwgSaveToExcel: TJSdGrid2Excel
    Left = 880
    Top = 11
  end
  inherited qryExec2: TADOQuery
    Left = 1008
    Top = 296
  end
  object tblProdLayer: TJSdTable
    CursorLocation = clUseServer
    CursorType = ctStatic
    BeforeInsert = qryDetail1BeforeInsert
    AfterInsert = qryDetail1AfterInsert
    BeforeEdit = qryDetail1BeforeEdit
    BeforeDelete = qryDetail1BeforeDelete
    DataSource = dsBrowse
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
      end>
    SQL.Strings = (
      'select PartNum,Revision,LayerId,Degree,AftLayerId,LayerName,'
      'Sort=Degree * 100 + FL'
      ' from EMOdProdLayer(nolock) where PartNum=:PartNum'
      'and Revision=:Revision'
      'order by Degree, FL')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision'
    MasterSource = dsBrowse
    TableName = 'EMOdProdLayer'
    Left = 286
    Top = 86
  end
  object dsProdLayer: TDataSource
    DataSet = tblProdLayer
    Left = 314
    Top = 88
  end
  object ImageList1: TImageList
    Left = 20
    Top = 32
    Bitmap = {
      494C010107008800880010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000002000000001002000000000000020
      000000000000000000000000000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000006B6B6B00000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000000000000000000000000000FFD6
      D600FFD6D600FFD6D600FFFFFF00F7F7F7009C9C9C009C9C9C00000000006B6B
      6B006B6B6B000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000C6C6C600C6C6C600C6C6C6000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF0000000000000000009C9C9C009C9C9C00FFD6
      D600FFD6D600FFD6D600FFFFFF00FFFFFF009C9C9C009C9C9C009C9C9C009C9C
      9C006B6B6B006B6B6B0000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C6008484840084848400C6C6C6000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF00000000009C9C9C009C9C9C009C9C9C000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00000000009C9C9C009C9C
      9C009C9C9C006B6B6B0000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000C6C6C600C6C6C600FFFF
      FF00FFFFFF00C6C6C6000000000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      0000000000008400000084000000000000009C9C9C0000000000FFFFFF00FFFF
      FF000000000000000000000000000000000000000000FFFFFF00FFFFFF000000
      00009C9C9C006B6B6B006B6B6B00000000000000000000000000000000000000
      0000000000000000000000FF000000FF000084848400C6C6C600C6C6C6000000
      0000C6C6C600C6C6C600C6C6C6000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000000000000FFFFFF0000000000FF8C
      AD00FF8CAD00FF8CAD00FF8CAD00FF8CAD00FF8CAD00FF8CAD0000000000FFFF
      FF00000000006B6B6B006B6B6B00000000000000000000000000000000000000
      0000000000000000000000FF000000FF0000848484000000000000000000C6C6
      C6008484840084848400C6C6C60000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000FFFFFF0000000000FF8CAD00FF8C
      AD00FF8CAD00FF8CAD00FF8CAD00FF4A4A00FF8CAD00FF8CAD00636363000000
      0000FFFFFF006B6B6B006B6B6B0000000000C6C6C60084848400848484000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF0000000000FFFFFF00000000009C9C9C00FF8C
      AD00F7F7F70063636300636363006363630063636300FF8CAD009C9C9C000000
      0000FFFFFF006B6B6B000000000000000000FFFFFF0000FFFF0000FFFF000000
      0000C6C6C6000000000000000000000000000000000000000000000000000000
      0000C6C6C600C6C6C600C6C6C6000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000000000000FFFFFF00000000009C9C
      9C009C9C9C009C9C9C009C9C9C009C9C9C009C9C9C009C9C9C0000000000FFFF
      FF0000000000000000000000000000000000FFFFFF0000FFFF0000FFFF000000
      0000C6C6C6000000000000FF000000FF000084848400C6C6C60000000000C6C6
      C6008484840084848400C6C6C60000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      0000000000008400000084000000000000000000000000000000FFFFFF00FFFF
      FF000000000000000000000000000000000000000000FFFFFF00FFFFFF000000
      00006B6B6B00000000000000000000000000FFFFFF0000FFFF0000FFFF000000
      0000000000000000000000FF000000FF00008484840000000000C6C6C600FFFF
      FF00FFFFFF00C6C6C600000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF0000000000C6C6C600C6C6
      C600000000006B6B6B000000000000000000FFFFFF0000FFFF0000FFFF000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C600C6C6C600C6C6C600C6C6C60000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFFFFF00C6C6
      C60000000000000000006B6B6B0000000000FFFFFF00FFFFFF00FFFFFF000000
      00000000000000000000000000000000000000000000C6C6C60000000000C6C6
      C6008484840084848400C6C6C6000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000006B6B
      6B000808080000000000000000006B6B6B000000000000000000000000000000
      0000000000000000000000FF000000FF00008484840000000000C6C6C600FFFF
      FF00FFFFFF00C6C6C600000000000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF0000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00006B6B6B000808080000000000000000000000000000000000000000000000
      0000000000000000000000FF000000FF000084848400C6C6C600000000000000
      0000C6C6C600C6C6C600C6C6C60000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      000000000000840000008400000000000000840000008400000084000000C6C6
      C600848400008484000084840000848400008484000084840000848400008484
      0000000000008400000084000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000006B6B6B0008080800000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C6008484840084848400C6C6C6000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF000000000084000000FFFFFF0084000000FFFF
      FF00FFFF0000FFFF0000FFFF0000FFFF0000FF00FF00FFFF0000FFFF0000FFFF
      000000000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF00FFFFFF00C6C6C6000000000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000840000008400000084000000FFFF
      FF00FFFF0000FFFF0000FF00FF00FF00FF00FF00FF00FFFF0000FFFF0000FFFF
      000000000000840000008400000000000000424D3E000000000000003E000000
      2800000040000000200000000100010000000000000100000000000000000000
      000000000000000000000000FFFFFF0000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000F03FFFFF00000000C007FFF100000000
      8003FFE1000000000003F803000000000001F811000000000001F82100000000
      0001084300000000000300310000000000070021000000008007084300000000
      E003082100000000FF81080100000000FFC0F80300000000FFE0F83100000000
      FFF0F86100000000FFF9FFE30000000000000000000000000000000000000000
      000000000000}
  end
  object qryDetail9: TJSdTable
    CursorLocation = clUseServer
    BeforeClose = qryDetail1BeforeClose
    BeforeInsert = qryDetail9BeforeInsert
    BeforeEdit = qryDetail1BeforeEdit
    BeforeDelete = qryDetail1BeforeDelete
    DataSource = dsBrowse
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
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'select * from EMOdProdTier where PartNum=:PartNum'
      'and Revision= :Revision'
      'and LayerId=:LayerId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    MasterSource = dsBrowse
    TableName = 'EMOdProdTier'
    Left = 694
    Top = 30
  end
  object dsDetail9: TDataSource
    DataSet = qryDetail9
    Left = 698
    Top = 54
  end
  object dsProdHIO: TwwDataSource
    DataSet = qryProdHIO
    Left = 312
    Top = 38
  end
  object qryProdAudit3: TADOQuery
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
      end
      item
        Name = 'Tag'
        DataType = ftInteger
        Size = 2
        Value = Null
      end
      item
        Name = 'IOType'
        DataType = ftInteger
        Size = 2
        Value = Null
      end
      item
        Name = 'UserId'
        DataType = ftString
        Size = 16
        Value = Null
      end
      item
        Name = 'Meno'
        DataType = ftString
        NumericScale = 255
        Precision = 255
        Size = -1
        Value = Null
      end>
    SQL.Strings = (
      
        'exec EMOdProdAudit :PartNum, :Revision, :Tag, :IOType, :UserId, ' +
        ':Meno'
      ' ')
    Left = 548
    Top = 294
  end
  object pgdProdModify: TJSdGrid2Excel
    PrintFileName = #21697#34399#20462#25913#32000#37636'.xls'
    PrintFileDir = 'c:\Report\'
    OutputType = otExcel
    Left = 64
    Top = 17
  end
  object qryMapXFlow: TADOQuery
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
      end
      item
        Name = 'MapKind'
        DataType = ftInteger
        Size = 2
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdGenMapXFlow :PartNum ,:Revision ,:MapKind')
    Left = 548
    Top = 338
    object qryMapXFlowMapData: TStringField
      FieldName = 'MapData'
      ReadOnly = True
      Size = 8000
    end
    object qryMapXFlowMapData2: TWideStringField
      FieldName = 'MapData2'
      ReadOnly = True
      Size = 8000
    end
    object qryMapXFlowStrMap: TStringField
      FieldName = 'StrMap'
      ReadOnly = True
      Size = 8000
    end
    object qryMapXFlowStrMap2: TMemoField
      FieldName = 'StrMap2'
      ReadOnly = True
      BlobType = ftMemo
    end
  end
  object qryProdLayer: TJSdTable
    CursorType = ctStatic
    DataSource = dsBrowse
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
      end>
    SQL.Strings = (
      ' Select PartNum, Revision, LayerId, LayerName, AftLayerId,'
      #9'IssLayer, Degree, FL, EL, TmpRouteId,'
      #9'StdPressCode, Film_PPM2, Film_ChkReceUp2, LayerNotes'
      '   From dbo.EMOdProdLayer t2 (Nolock)'
      '  Where t2.PartNum = :PartNum'
      '    And t2.Revision = :Revision')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    MasterSource = dsBrowse
    TableName = 'dbo.EMOdProdLayer_Short'
    Left = 372
    Top = 18
    object qryProdLayerPartNum: TStringField
      FieldName = 'PartNum'
      FixedChar = True
      Size = 24
    end
    object qryProdLayerRevision: TStringField
      FieldName = 'Revision'
      FixedChar = True
      Size = 4
    end
    object qryProdLayerLayerId: TStringField
      FieldName = 'LayerId'
      FixedChar = True
      Size = 8
    end
    object qryProdLayerLayerName: TStringField
      FieldName = 'LayerName'
      Size = 24
    end
    object qryProdLayerAftLayerId: TStringField
      FieldName = 'AftLayerId'
      FixedChar = True
      Size = 8
    end
    object qryProdLayerIssLayer: TIntegerField
      FieldName = 'IssLayer'
    end
    object qryProdLayerDegree: TIntegerField
      FieldName = 'Degree'
    end
    object qryProdLayerFL: TIntegerField
      FieldName = 'FL'
    end
    object qryProdLayerEL: TIntegerField
      FieldName = 'EL'
    end
    object qryProdLayerTmpRouteId: TStringField
      FieldName = 'TmpRouteId'
      FixedChar = True
      Size = 12
    end
    object qryProdLayerStdPressCode: TStringField
      FieldName = 'StdPressCode'
      FixedChar = True
      Size = 12
    end
    object qryProdLayerFilm_PPM2: TFloatField
      FieldName = 'Film_PPM2'
    end
    object qryProdLayerFilm_ChkReceUp2: TFloatField
      FieldName = 'Film_ChkReceUp2'
    end
    object qryProdLayerLayerNotes: TWideStringField
      FieldName = 'LayerNotes'
      Size = 1024
    end
  end
  object qryEditCheck: TADOQuery
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
      'Exec EMOdEditCheck'
      ':PartNum,'
      ':Revision'
      ' ')
    Left = 611
    Top = 352
    object qryEditCheckV: TIntegerField
      FieldName = 'V'
    end
  end
  object qryUserId: TADOQuery
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'select t1.UserId, t1.UserName '
      '  from CURdUsers t1(nolock)')
    Left = 208
    Top = 24
    object qryUsersUserId: TStringField
      DisplayLabel = #24037#34399
      DisplayWidth = 16
      FieldName = 'UserId'
      FixedChar = True
      Size = 16
    end
    object qryUsersUserName: TWideStringField
      DisplayLabel = #22995#21517
      DisplayWidth = 24
      FieldName = 'UserName'
      FixedChar = True
      Size = 24
    end
  end
  object dsUserId: TDataSource
    DataSet = qryUserId
    Left = 244
    Top = 24
  end
  object qryProdHIO: TJSdTable
    CursorLocation = clUseServer
    CursorType = ctStatic
    LockType = ltReadOnly
    BeforeInsert = qryDetail1BeforeInsert
    AfterInsert = qryDetail1AfterInsert
    BeforeEdit = qryDetail1BeforeEdit
    BeforeDelete = qryDetail1BeforeDelete
    DataSource = dsBrowse
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
      end>
    SQL.Strings = (
      'exec EMOdFormHisLog :PartNum, :Revision')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision'
    MasterSource = dsBrowse
    TableName = 'EMOdProdHIO'
    DisplayLabel = 'EMOdProdHIO'
    Left = 286
    Top = 38
  end
  object SavePictureDialog1: TSavePictureDialog
    Left = 20
    Top = 78
  end
  object OpenDialog1: TOpenDialog
    Left = 60
    Top = 66
  end
  object qryPartMatri: TADOQuery
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
      'select PartNum,Revision '
      'from dbo.EMOdPartMerge(nolock)'
      'where MPartNum=:PartNum'
      '  and MRevision=:Revision'
      ' ')
    Left = 140
    Top = 54
    object qryPartMatriPartNum: TStringField
      DisplayLabel = #27597#26009#34399
      DisplayWidth = 12
      FieldName = 'PartNum'
      FixedChar = True
      Size = 24
    end
    object qryPartMatriRevision: TStringField
      DisplayLabel = #29256#24207
      DisplayWidth = 4
      FieldName = 'Revision'
      FixedChar = True
      Size = 4
    end
  end
  object dsPartMatri: TDataSource
    DataSet = qryPartMatri
    Left = 120
    Top = 254
  end
  object qryDelete: TADOQuery
    DataSource = dsBrowse
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
      end
      item
        Name = 'UserId'
        DataType = ftString
        Size = 12
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdProdDeleteIns'
      ':PartNum,'
      ':Revision,'
      ':UserId')
    Left = 52
    Top = 246
  end
  object tblLayerPress: TJSdTable
    CursorLocation = clUseServer
    CursorType = ctStatic
    BeforeEdit = tblLayerPressBeforeEdit
    AfterEdit = qryDetail1AfterEdit
    BeforePost = tblLayerPressBeforePost
    AfterPost = tblLayerPressAfterPost
    DataSource = dsBrowse
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
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'select t1.*, MatClassName=t2.ClassName, t4.LayerName,'
      'MatUnit=t5.Unit'
      'from (EMOdLayerPress t1 Left Join MINdMatClass t2'
      'On t1.MatClass = t2.MatClass'
      'Left Join MINdMatInfo t5'
      'On t1.matcode = t5.PartNum)'
      ', EMOdVPressForDLL t3 Left Join EMOdTmpBOMDtl t4'
      'On t3.TmpBOMId=t4.TmpId '
      'and t3.BefLayer=t4.LayerId and t3.LayerId=t4.AftLayerId'
      'where t1.PartNum=:PartNum and t1.Revision=:Revision '
      'and t1.LayerId =:LayerId'
      'and t1.PartNum=t3.PartNum and t1.Revision=t3.Revision'
      'and t1.LayerId=t3.LayerId'
      'and t1.SerialNum=t3.SerialNum'
      'order by t1.PartNum, t1.Revision, t1.LayerId, t1.SerialNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision'
    MasterSource = dsBrowse
    TableName = 'EMOdLayerPress'
    Left = 366
    Top = 62
  end
  object dsLayerPress: TDataSource
    DataSet = tblLayerPress
    Left = 394
    Top = 64
  end
  object qryMatCode: TADOQuery
    Parameters = <
      item
        Name = 'partnum'
        DataType = ftString
        Size = 24
        Value = Null
      end
      item
        Name = 'revision'
        DataType = ftString
        Size = 4
        Value = Null
      end
      item
        Name = 'layerid'
        DataType = ftString
        Size = 8
        Value = Null
      end
      item
        Name = 'PartNum'
        DataType = ftString
        Size = 12
        Value = Null
      end
      item
        Name = 'Revision'
        DataType = ftString
        Size = 4
        Value = Null
      end
      item
        Name = 'Layerid'
        DataType = ftString
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'delete EMOdLayerPressBk'
      '  where partnum=:partnum'
      '  and revision=:revision'
      '  and layerid=:layerid'
      ''
      'insert into EMOdLayerPressBk'
      'select distinct PartNum,Revision,Layerid,SerialNum,MatCode'
      '  from EMOdLayerPress(nolock)'
      'where partnum=:PartNum'
      '  and revision=:Revision'
      '  and Layerid=:Layerid'
      ''
      ''
      ' ')
    Left = 47
    Top = 285
  end
  object qryLayerPress_New: TADOQuery
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
      end
      item
        Name = 'LayerId'
        DataType = ftString
        Size = 8
        Value = Null
      end>
    SQL.Strings = (
      'exec EMOdLayerPressView :PartNum, :Revision, :LayerId'
      ' ')
    Left = 371
    Top = 116
    object qryLayerPress_NewSerialNum: TSmallintField
      DisplayLabel = #24207
      DisplayWidth = 2
      FieldName = 'SerialNum'
    end
    object qryLayerPress_NewBefLayer: TStringField
      DisplayLabel = #20839#23652
      DisplayWidth = 4
      FieldName = 'BefLayer'
      FixedChar = True
      Size = 8
    end
    object qryLayerPress_NewValueName: TStringField
      DisplayLabel = #24288#21830
      DisplayWidth = 12
      FieldName = 'ValueName'
      FixedChar = True
      Size = 36
    end
    object qryLayerPress_NewIsNormal: TIntegerField
      DisplayLabel = #24120#20633#26009
      DisplayWidth = 6
      FieldName = 'IsNormal'
    end
    object qryLayerPress_NewIsTested: TIntegerField
      DisplayLabel = #24050#35430#20570
      DisplayWidth = 6
      FieldName = 'IsTested'
    end
    object qryLayerPress_NewIsSpeced: TIntegerField
      DisplayLabel = #39511#25910#27161#28310
      DisplayWidth = 8
      FieldName = 'IsSpeced'
    end
    object qryLayerPress_NewISULMark: TIntegerField
      DisplayLabel = 'UL'#35469#35657
      DisplayWidth = 6
      FieldName = 'ISULMark'
    end
    object qryLayerPress_NewGP: TSmallintField
      DisplayLabel = 'GP'#35469#35657
      DisplayWidth = 6
      FieldName = 'GP'
    end
    object qryLayerPress_NewICP: TSmallintField
      DisplayLabel = 'ICP'#35469#35657
      DisplayWidth = 7
      FieldName = 'ICP'
    end
    object qryLayerPress_NewGPIsHold: TSmallintField
      DisplayLabel = 'GP'#20572#29992
      DisplayWidth = 6
      FieldName = 'GPIsHold'
    end
    object qryLayerPress_NewUL: TSmallintField
      DisplayLabel = 'UL'#19981#36969#29992
      DisplayWidth = 8
      FieldName = 'UL'
    end
    object qryLayerPress_NewNotes: TWideStringField
      DisplayLabel = #20633#35387
      DisplayWidth = 6
      FieldName = 'Notes'
      FixedChar = True
      Size = 255
    end
    object qryLayerPress_NewUsage: TFloatField
      DisplayLabel = #20351#29992#29575
      DisplayWidth = 6
      FieldName = 'Usage'
      Visible = False
    end
    object qryLayerPress_NewMatClass: TStringField
      DisplayWidth = 3
      FieldName = 'MatClass'
      Visible = False
      FixedChar = True
      Size = 8
    end
  end
  object qryPressNull: TADOQuery
    Parameters = <
      item
        Name = 'N'
        DataType = ftInteger
        Size = 2
        Value = Null
      end
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
      'If :N = 2'
      'Begin'
      'Update EMOdProdLayer'
      'Set StdPressCode = Null'
      'Where PartNum = :PartNum'
      'And Revision = :Revision'
      'End'
      ' ')
    Left = 47
    Top = 325
  end
  object qryDetail11: TJSdTable
    CursorLocation = clUseServer
    BeforeClose = qryDetail1BeforeClose
    BeforeInsert = qryDetail9BeforeInsert
    BeforeEdit = qryDetail1BeforeEdit
    AfterEdit = qryDetail1AfterEdit
    AfterPost = qryDetail1AfterPost
    BeforeDelete = qryDetail1BeforeDelete
    DataSource = dsTmpRouteId
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
      'select t1.PartNum, t1.Revision, t1.LayerId, t1.ProcCode, '
      't1.SerialNum, t1.MatCode, t1.UseQnty, t1.UseBase, '
      't1.MatPos, t1.Notes, t1.BeDisplace, t1.BeSemiProd, '
      't1.SuperId, t1.isCustSupply, t1.StDScRate'
      'from dbo.EMOdLayerBOM t1'
      'where t1.PartNum=:PartNum'
      'and t1.Revision= :Revision'
      'and t1.LayerId= :LayerId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision;LayerId'
    MasterSource = dsTmpRouteId
    TableName = 'EMOdLayerBOM'
    Left = 662
    Top = 34
    object qryDetail11PartNum: TStringField
      DisplayWidth = 24
      FieldName = 'PartNum'
      Visible = False
      FixedChar = True
      Size = 24
    end
    object qryDetail11Revision: TStringField
      DisplayWidth = 4
      FieldName = 'Revision'
      Visible = False
      FixedChar = True
      Size = 4
    end
    object qryDetail11LayerId: TStringField
      DisplayWidth = 8
      FieldName = 'LayerId'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryDetail11ProcCode: TStringField
      DisplayWidth = 8
      FieldName = 'ProcCode'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryDetail11SerialNum: TWordField
      DisplayWidth = 10
      FieldName = 'SerialNum'
      Visible = False
    end
    object qryDetail11MatCode: TStringField
      DisplayWidth = 24
      FieldName = 'MatCode'
      Visible = False
      Size = 24
    end
    object qryDetail11UseQnty: TFloatField
      DisplayWidth = 10
      FieldName = 'UseQnty'
      Visible = False
    end
    object qryDetail11UseBase: TFloatField
      DisplayWidth = 10
      FieldName = 'UseBase'
      Visible = False
    end
    object qryDetail11MatPos: TWideStringField
      DisplayWidth = 255
      FieldName = 'MatPos'
      Visible = False
      Size = 255
    end
    object qryDetail11Notes: TWideStringField
      DisplayWidth = 255
      FieldName = 'Notes'
      Visible = False
      Size = 255
    end
    object qryDetail11BeDisplace: TIntegerField
      DisplayWidth = 10
      FieldName = 'BeDisplace'
      Visible = False
    end
    object qryDetail11BeSemiProd: TIntegerField
      DisplayWidth = 10
      FieldName = 'BeSemiProd'
      Visible = False
    end
    object qryDetail11SuperId: TStringField
      DisplayWidth = 12
      FieldName = 'SuperId'
      Visible = False
      FixedChar = True
      Size = 12
    end
    object qryDetail11isCustSupply: TIntegerField
      DisplayWidth = 10
      FieldName = 'isCustSupply'
      Visible = False
    end
    object qryDetail11StDScRate: TFloatField
      DisplayWidth = 10
      FieldName = 'StDScRate'
      Visible = False
    end
    object qryDetail11MatName: TWideStringField
      DisplayWidth = 120
      FieldKind = fkLookup
      FieldName = 'MatName'
      LookupDataSet = qryMatName
      LookupKeyFields = 'PartNum'
      LookupResultField = 'MatName'
      KeyFields = 'MatCode'
      Visible = False
      Size = 120
      Lookup = True
    end
    object qryDetail11Unit: TStringField
      FieldKind = fkLookup
      FieldName = 'Unit'
      LookupDataSet = qryUnit
      LookupKeyFields = 'PartNum'
      LookupResultField = 'Unit'
      KeyFields = 'MatCode'
      Size = 24
      Lookup = True
    end
  end
  object dsDetail11: TDataSource
    DataSet = qryDetail11
    Left = 666
    Top = 64
  end
  object qryDetail10: TJSdTable
    CursorLocation = clUseServer
    BeforeClose = qryDetail1BeforeClose
    BeforeInsert = qryDetail9BeforeInsert
    BeforeEdit = qryDetail1BeforeEdit
    AfterEdit = qryDetail1AfterEdit
    AfterPost = qryDetail10AfterPost
    BeforeDelete = qryDetail10BeforeDelete
    DataSource = dsTmpRouteId
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
      'select t1.*'
      'from dbo.EMOdLayerRoute t1'
      'where t1.PartNum=:PartNum'
      'and t1.Revision= :Revision'
      'and t1.LayerId= :LayerId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision;LayerId'
    MasterSource = dsTmpRouteId
    TableName = 'EMOdLayerRoute'
    Left = 678
    Top = 34
    object qryDetail10PartNum: TStringField
      DisplayWidth = 24
      FieldName = 'PartNum'
      Visible = False
      FixedChar = True
      Size = 24
    end
    object qryDetail10Revision: TStringField
      DisplayWidth = 4
      FieldName = 'Revision'
      Visible = False
      FixedChar = True
      Size = 4
    end
    object qryDetail10LayerId: TStringField
      DisplayWidth = 8
      FieldName = 'LayerId'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryDetail10SerialNum: TWordField
      DisplayWidth = 10
      FieldName = 'SerialNum'
      Visible = False
    end
    object qryDetail10ProcCode: TStringField
      DisplayWidth = 8
      FieldName = 'ProcCode'
      Visible = False
      FixedChar = True
      Size = 8
    end
    object qryDetail10Notes: TWideStringField
      DisplayWidth = 4000
      FieldName = 'Notes'
      Visible = False
      Size = 4000
    end
    object qryDetail10FinishRate: TFloatField
      DisplayWidth = 10
      FieldName = 'FinishRate'
      Visible = False
    end
    object qryDetail10IsNormal: TWideStringField
      DisplayWidth = 20
      FieldName = 'IsNormal'
      Visible = False
    end
    object qryDetail10DepartId: TStringField
      DisplayWidth = 12
      FieldName = 'DepartId'
      Visible = False
      FixedChar = True
      Size = 12
    end
    object qryDetail10Spec: TWideStringField
      DisplayWidth = 255
      FieldName = 'Spec'
      Visible = False
      Size = 255
    end
    object qryDetail10FilmNo: TWideStringField
      DisplayWidth = 255
      FieldName = 'FilmNo'
      Visible = False
      Size = 255
    end
    object qryDetail10ChangeNotes: TWideStringField
      DisplayWidth = 255
      FieldName = 'ChangeNotes'
      Visible = False
      Size = 255
    end
    object qryDetail10PartSerial: TStringField
      DisplayWidth = 12
      FieldName = 'PartSerial'
      Visible = False
      Size = 12
    end
    object qryDetail10ProcSerial: TStringField
      DisplayWidth = 8
      FieldName = 'ProcSerial'
      Visible = False
      Size = 8
    end
    object qryDetail10SortType: TStringField
      DisplayWidth = 4
      FieldName = 'SortType'
      Visible = False
      Size = 4
    end
    object qryDetail10BefSETime: TFloatField
      DisplayWidth = 10
      FieldName = 'BefSETime'
      Visible = False
    end
    object qryDetail10MoldPcs: TIntegerField
      DisplayWidth = 10
      FieldName = 'MoldPcs'
      Visible = False
    end
    object qryDetail10Item: TIntegerField
      DisplayWidth = 10
      FieldName = 'Item'
      Visible = False
    end
    object qryDetail10ProcName: TWideStringField
      DisplayWidth = 24
      FieldKind = fkLookup
      FieldName = 'ProcName'
      LookupDataSet = qryProcInfo
      LookupKeyFields = 'ProcCode'
      LookupResultField = 'ProcName'
      KeyFields = 'ProcCode'
      Visible = False
      Size = 24
      Lookup = True
    end
    object qryDetail10NotesPath: TWideStringField
      DisplayWidth = 255
      FieldName = 'NotesPath'
      Visible = False
      Size = 255
    end
    object qryDetail10Mark: TWideStringField
      FieldName = 'Mark'
      Visible = False
      Size = 4
    end
  end
  object dsDetail10: TDataSource
    DataSet = qryDetail10
    Left = 682
    Top = 64
  end
  object qryTmpRouteId: TJSdTable
    CursorLocation = clUseServer
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
      
        'select t1.PartNum, t1.Revision, t1.LayerId, t1.TmpRouteId, t1.Is' +
        'UseNotes'
      'from dbo.EMOdProdLayer t1'
      'where t1.PartNum=:PartNum'
      'and t1.Revision= :Revision'
      'and t1.LayerId= :LayerId')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision;LayerId'
    Left = 446
    Top = 42
    object qryTmpRouteIdIsUseNotes: TIntegerField
      FieldName = 'IsUseNotes'
    end
    object qryTmpRouteIdTmpRouteId: TStringField
      FieldName = 'TmpRouteId'
      Size = 24
    end
    object qryTmpRouteIdPartNum: TStringField
      FieldName = 'PartNum'
      Size = 24
    end
    object qryTmpRouteIdRevision: TStringField
      FieldName = 'Revision'
      Size = 8
    end
    object qryTmpRouteIdLayerId: TStringField
      FieldName = 'LayerId'
      Size = 24
    end
  end
  object dsTmpRouteId: TDataSource
    DataSet = qryTmpRouteId
    Left = 474
    Top = 40
  end
  object qryProcCodeBOMSet: TADOQuery
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
    Left = 551
    Top = 251
  end
  object qryCopyRoute: TADOQuery
    EnableBCD = False
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
        Size = 8
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
        Name = 'SourLayerId'
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
    Prepared = True
    SQL.Strings = (
      'exec EMOdRouteCopy :PartNum, :Revision, :LayerId, :SourLayerId,'
      ':SourPartNum, :SourRevision')
    Left = 760
    Top = 448
  end
  object qryMatName: TADOQuery
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'select t1.PartNum, t1.MatName '
      '  from MINdMatInfo t1(nolock)')
    Left = 216
    Top = 72
    object qryMatNamePartNum: TStringField
      FieldName = 'PartNum'
      Size = 24
    end
    object qryMatNameMatName: TWideStringField
      FieldName = 'MatName'
      Size = 120
    end
  end
  object qryProcInfo: TADOQuery
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'select t1.ProcCode, t1.ProcName '
      '  from EMOdProcInfo t1(nolock)')
    Left = 216
    Top = 120
  end
  object qryMap: TADOQuery
    CursorType = ctStatic
    LockType = ltBatchOptimistic
    DataSource = dsDetail3
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
      end
      item
        Name = 'SerialNum'
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'select MapData'
      'from EMOdProdMap(nolock)'
      'where PartNum= :PartNum'
      'and Revision= :Revision'
      'and SerialNum= :SerialNum')
    Left = 548
    Top = 386
  end
  object tblModify: TJSdTable
    CursorType = ctStatic
    LockType = ltReadOnly
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
      end>
    SQL.Strings = (
      'Exec EMOdNeedModifyConfirm :PartNum, :Revision')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'EMOdProdModify'
    Left = 446
    Top = 90
  end
  object dsModify: TDataSource
    DataSet = tblModify
    Left = 474
    Top = 88
  end
  object tblMills: TJSdTable
    CursorLocation = clUseServer
    AfterInsert = tblMillsAfterInsert
    DataSource = dsBrowse
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
      end>
    SQL.Strings = (
      'select t1.*'
      'from dbo.EMOdProdMills t1'
      'where t1.PartNum=:PartNum'
      'and t1.Revision= :Revision')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision'
    MasterSource = dsBrowse
    TableName = 'EMOdProdMills'
    Left = 534
    Top = 58
  end
  object dsMills: TDataSource
    DataSet = tblMills
    Left = 562
    Top = 56
  end
  object tblWriting: TJSdTable
    CursorLocation = clUseServer
    AfterInsert = tblWritingAfterInsert
    DataSource = dsBrowse
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
      end>
    SQL.Strings = (
      'select t1.*'
      'from dbo.EMOdProdWriting t1'
      'where t1.PartNum=:PartNum'
      'and t1.Revision= :Revision')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision'
    MasterSource = dsBrowse
    TableName = 'EMOdProdWriting'
    Left = 534
    Top = 106
  end
  object dsWriting: TDataSource
    DataSet = tblWriting
    Left = 562
    Top = 104
  end
  object qryDetail7B: TJSdTable
    CursorLocation = clUseServer
    LockType = ltReadOnly
    BeforeClose = qryDetail1BeforeClose
    BeforeInsert = qryDetail7BBeforeInsert
    AfterInsert = qryDetail7BAfterInsert
    AfterPost = qryDetail1AfterPost
    BeforeDelete = qryDetail1BeforeDelete
    DataSource = dsBrowse
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
      end>
    SQL.Strings = (
      'select * from EMOdProdECNLog'
      'where PartNum= :PartNum'
      'and Revision= :Revision')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    MasterSource = dsBrowse
    TableName = 'EMOdProdECNLog'
    Left = 790
    Top = 230
  end
  object qryDetail8B: TJSdTable
    CursorLocation = clUseServer
    LockType = ltReadOnly
    BeforeClose = qryDetail1BeforeClose
    BeforeInsert = qryDetail1BeforeInsert
    AfterInsert = qryDetail1AfterInsert
    BeforeEdit = qryDetail1BeforeEdit
    AfterPost = qryDetail1AfterPost
    BeforeDelete = qryDetail1BeforeDelete
    DataSource = dsBrowse
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
      end>
    SQL.Strings = (
      'select * from EMOdMailrecord(nolock)'
      'where PartNum= :PartNum'
      'and Revision= :Revision')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    MasterSource = dsBrowse
    TableName = 'EMOdMailrecord '
    Left = 758
    Top = 246
  end
  object dsDetail8B: TDataSource
    DataSet = qryDetail8B
    Left = 754
    Top = 278
  end
  object dsDetail7B: TDataSource
    DataSet = qryDetail7B
    Left = 786
    Top = 262
  end
  object tblMGNMap: TJSdTable
    CursorLocation = clUseServer
    LockType = ltReadOnly
    DataSource = dsBrowse
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
      'select * from MGNdProdMap(nolock)'
      'where PartNum= :PartNum')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    MasterSource = dsBrowse
    TableName = 'MGNdProdMap_EMO'
    Left = 726
    Top = 230
  end
  object dsMGNMap: TDataSource
    DataSet = tblMGNMap
    Left = 722
    Top = 262
  end
  object tblPartMergePrint: TJSdTable
    CursorLocation = clUseServer
    CursorType = ctStatic
    BeforeInsert = qryDetail1BeforeInsert
    AfterInsert = qryDetail1AfterInsert
    BeforeEdit = qryDetail1BeforeEdit
    AfterEdit = qryDetail1AfterEdit
    BeforeDelete = qryDetail1BeforeDelete
    DataSource = dsBrowse
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
      end>
    SQL.Strings = (
      'select * from EMOdPartMergePrint where PartNum=:PartNum'
      'and Revision=:Revision')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = 'PartNum;Revision'
    MasterSource = dsBrowse
    TableName = 'EMOdPartMergePrint'
    Left = 158
    Top = 62
  end
  object dsPartMergePrint: TDataSource
    DataSet = tblPartMergePrint
    Left = 154
    Top = 208
  end
  object qryPage: TADOQuery
    EnableBCD = False
    Parameters = <>
    Prepared = True
    Left = 1040
    Top = 96
  end
  object qryUnit: TADOQuery
    CursorType = ctStatic
    LockType = ltReadOnly
    Parameters = <>
    SQL.Strings = (
      'select t1.PartNum, t1.MatName, t1.Unit'
      '  from MINdMatInfo t1(nolock)')
    Left = 472
    Top = 376
    object StringField1: TStringField
      FieldName = 'PartNum'
      Size = 24
    end
    object WideStringField1: TWideStringField
      FieldName = 'MatName'
      Size = 120
    end
    object qryUnitUnit: TStringField
      FieldName = 'Unit'
      Size = 24
    end
  end
end

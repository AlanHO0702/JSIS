inherited frmFOSdOrderInq: TfrmFOSdOrderInq
  Caption = 'frmFOSdOrderInq'
  ClientHeight = 570
  ClientWidth = 982
  ExplicitWidth = 990
  ExplicitHeight = 597
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlInfo: TPanel
    Top = 550
    Width = 982
    ExplicitTop = 550
    ExplicitWidth = 982
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 528
    Width = 982
    ExplicitTop = 528
    ExplicitWidth = 982
  end
  inherited pnlLeft: TPanel
    Height = 528
  end
  inherited pnlForm: TPanel
    Width = 861
    Height = 528
    inherited pnlMain: TPanel
      Width = 859
      inherited SplTop: TSplitter
        Width = 855
      end
      inherited SplBottom: TSplitter
        Width = 855
      end
      inherited pnlBottom: TPanel
        Width = 855
        object pageDtl: TPageControl
          Left = 0
          Top = 0
          Width = 855
          Height = 156
          ActivePage = TabSheet2
          Align = alClient
          Style = tsFlatButtons
          TabOrder = 0
          ExplicitLeft = 2
          ExplicitTop = 324
          ExplicitWidth = 978
          ExplicitHeight = 236
          object TabSheet2: TTabSheet
            Caption = #20986#24288#21934#20027#27284
            ImageIndex = 1
            object grid_OrderMain: TJSdDBGrid
              Left = 0
              Top = 0
              Width = 847
              Height = 124
              IniAttributes.Delimiter = ';;'
              IniAttributes.UnicodeIniFile = False
              TitleColor = clBtnFace
              FixedCols = 0
              ShowHorzScrollBar = True
              Align = alClient
              DataSource = dsFOSdOrderMain
              Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
              ParentShowHint = False
              ShowHint = False
              TabOrder = 0
              TitleAlignment = taLeftJustify
              TitleFont.Charset = ANSI_CHARSET
              TitleFont.Color = clWindowText
              TitleFont.Height = -13
              TitleFont.Name = #32048#26126#39636
              TitleFont.Style = []
              TitleLines = 1
              TitleButtons = True
              OnDblClick = grid_OrderMainDblClick
              SortColumnClick = stColumnClick
            end
          end
          object TabSheet1: TTabSheet
            Caption = #22238#24288#21934#26126#32048
            object grid_Rec: TJSdDBGrid
              Left = 0
              Top = 0
              Width = 847
              Height = 124
              IniAttributes.Delimiter = ';;'
              IniAttributes.UnicodeIniFile = False
              TitleColor = clBtnFace
              FixedCols = 0
              ShowHorzScrollBar = True
              Align = alClient
              DataSource = dsFOSdReceiveSub
              Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
              ParentShowHint = False
              ShowHint = False
              TabOrder = 0
              TitleAlignment = taLeftJustify
              TitleFont.Charset = ANSI_CHARSET
              TitleFont.Color = clWindowText
              TitleFont.Height = -13
              TitleFont.Name = #32048#26126#39636
              TitleFont.Style = []
              TitleLines = 1
              TitleButtons = True
              OnDblClick = grid_RecDblClick
              SortColumnClick = stColumnClick
            end
          end
        end
      end
      inherited grid_Browse: TJSdDBGrid
        Width = 855
      end
      inherited pgeMaster: TPageControl
        Width = 855
        inherited tbshtMaster1: TTabSheet
          inherited pnlMaster1: TPanel
            Width = 847
          end
        end
        inherited tbshtMaster2: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 847
          ExplicitHeight = 161
          inherited pnlMaster2: TPanel
            Width = 847
            ExplicitWidth = 847
          end
        end
        inherited tbshtMaster3: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 847
          ExplicitHeight = 161
          inherited pnlMaster3: TPanel
            Width = 847
            ExplicitWidth = 847
          end
        end
        inherited tbshtMaster4: TTabSheet
          ExplicitLeft = 4
          ExplicitTop = 28
          ExplicitWidth = 847
          ExplicitHeight = 161
          inherited pnlMaster4: TPanel
            Width = 847
            ExplicitWidth = 847
          end
        end
      end
    end
  end
  inherited qryExec: TADOQuery
    Left = 864
    Top = 256
  end
  inherited qryGetTranData: TADOQuery
    Left = 864
    Top = 200
  end
  object qryFOSdOrderMain: TJSdTable
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FOSdOrderMain4Inq'
    Left = 320
    Top = 352
  end
  object qryFOSdReceiveSub: TJSdTable
    LockType = ltReadOnly
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    TableName = 'FOSdReceiveSub4Inq'
    Left = 416
    Top = 344
  end
  object dsFOSdOrderMain: TDataSource
    DataSet = qryFOSdOrderMain
    Left = 320
    Top = 400
  end
  object dsFOSdReceiveSub: TDataSource
    DataSet = qryFOSdReceiveSub
    Left = 416
    Top = 392
  end
end

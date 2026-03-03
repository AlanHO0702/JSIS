inherited frmFMEdPartNumViewer: TfrmFMEdPartNumViewer
  Caption = 'frmFMEdPartNumViewer'
  ClientHeight = 450
  ClientWidth = 767
  ExplicitWidth = 775
  ExplicitHeight = 477
  PixelsPerInch = 96
  TextHeight = 13
  inherited pnlInfo: TPanel
    Top = 430
    Width = 767
    ExplicitTop = 430
    ExplicitWidth = 767
    inherited btnGetParams: TSpeedButton
      Flat = False
    end
  end
  inherited pnlTempBasDLLbm: TPanel
    Top = 408
    Width = 767
    ExplicitTop = 408
    ExplicitWidth = 767
  end
  object Panel1: TPanel [2]
    Left = 0
    Top = 0
    Width = 767
    Height = 65
    Align = alTop
    TabOrder = 2
    object labPaperDate: TLabel
      Left = 25
      Top = 36
      Width = 56
      Height = 13
      Alignment = taRightJustify
      Caption = #21934#25818#26085#26399
    end
    object imgPaperDate: TImage
      Left = 207
      Top = 33
      Width = 13
      Height = 13
      AutoSize = True
      Picture.Data = {
        07544269746D6170DE000000424DDE0000000000000076000000280000000D00
        00000D0000000100040000000000680000000000000000000000100000001000
        0000000000000000BF0000BF000000BFBF00BF000000BF00BF00BFBF0000C0C0
        C000808080000000FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFF
        FF00777777777777700077777707777770007777770077777000777777060777
        7000770000066077700077066666660770007706666666607000770666666607
        7000770000066077700077777706077770007777770077777000777777077777
        70007777777777777000}
    end
    object Label3D2: TLabel
      Tag = 1
      Left = 25
      Top = 12
      Width = 56
      Height = 13
      Alignment = taRightJustify
      Caption = #21697#34399#29256#24207
    end
    object btFind: TSpeedButton
      Left = 537
      Top = 3
      Width = 40
      Height = 40
      Caption = '&F'#26597#35426
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
      Layout = blGlyphTop
      OnClick = btFindClick
    end
    object Label1: TLabel
      Tag = 1
      Left = 400
      Top = 12
      Width = 56
      Height = 13
      Alignment = taRightJustify
      Caption = 'DateCode'
    end
    object pnlXExit: TPanel
      Left = 707
      Top = 1
      Width = 59
      Height = 63
      Align = alRight
      BevelOuter = bvNone
      TabOrder = 4
    end
    object edtFPaperDate: TwwDBDateTimePicker
      Left = 87
      Top = 33
      Width = 114
      Height = 21
      CalendarAttributes.Font.Charset = DEFAULT_CHARSET
      CalendarAttributes.Font.Color = clWindowText
      CalendarAttributes.Font.Height = -11
      CalendarAttributes.Font.Name = 'MS Sans Serif'
      CalendarAttributes.Font.Style = []
      Epoch = 1950
      ImeMode = imSAlpha
      ShowButton = True
      TabOrder = 2
    end
    object edtEPaperDate: TwwDBDateTimePicker
      Left = 226
      Top = 33
      Width = 114
      Height = 21
      CalendarAttributes.Font.Charset = DEFAULT_CHARSET
      CalendarAttributes.Font.Color = clWindowText
      CalendarAttributes.Font.Height = -11
      CalendarAttributes.Font.Name = 'MS Sans Serif'
      CalendarAttributes.Font.Style = []
      Epoch = 1950
      ImeMode = imSAlpha
      ShowButton = True
      TabOrder = 3
    end
    object edtRevision: TEdit
      Left = 343
      Top = 6
      Width = 43
      Height = 21
      CharCase = ecUpperCase
      TabOrder = 0
    end
    object edtDateCode: TEdit
      Left = 459
      Top = 6
      Width = 58
      Height = 21
      CharCase = ecUpperCase
      TabOrder = 1
    end
    object edtPartnum: TJSdLookupCombo
      Left = 87
      Top = 8
      Width = 253
      Height = 21
      LkDataSource = dsPartNum
      LkColumnCount = 0
      cboColor = clWindow
      TextSize = 100
      Text = ''
      SelectOnly = False
      SortedOff = False
      TabOrder = 5
    end
  end
  object pagData: TPageControl [3]
    Left = 0
    Top = 65
    Width = 767
    Height = 343
    ActivePage = TabSheet1
    Align = alTop
    TabOrder = 3
    OnChange = pagDataChange
    object TabSheet1: TTabSheet
      Caption = #35069#31243#29694#24115
      object grdWIP: TJSdDBGrid
        Left = 0
        Top = 0
        Width = 759
        Height = 314
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsWIP
        Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
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
    object TabSheet2: TTabSheet
      Caption = #35069#20196#21934
      ImageIndex = 1
      object grdIssue: TJSdDBGrid
        Left = 0
        Top = 0
        Width = 759
        Height = 314
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsIssue
        Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
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
    object TabSheet3: TTabSheet
      Caption = #36942#24115#21934
      ImageIndex = 2
      object grdPass: TJSdDBGrid
        Left = 0
        Top = 0
        Width = 759
        Height = 314
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsPass
        Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
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
    object TabSheet4: TTabSheet
      Caption = #20837#24235#21934
      ImageIndex = 3
      object JSdDBGrid2: TJSdDBGrid
        Left = 0
        Top = 0
        Width = 759
        Height = 314
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsInFFG
        Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
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
    object TabSheet5: TTabSheet
      Caption = #22577#24290#36942#24115#21934
      ImageIndex = 4
      TabVisible = False
      object grdScrap: TJSdDBGrid
        Left = 0
        Top = 0
        Width = 759
        Height = 314
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsScrap
        Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
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
    object TabSheet6: TTabSheet
      Caption = #22577#24290#21028#23450#21934
      ImageIndex = 5
      TabVisible = False
      object grdMRBScrap: TJSdDBGrid
        Left = 0
        Top = 0
        Width = 759
        Height = 314
        IniAttributes.Delimiter = ';;'
        IniAttributes.UnicodeIniFile = False
        TitleColor = clBtnFace
        FixedCols = 0
        ShowHorzScrollBar = True
        Align = alClient
        DataSource = dsMRBScrap
        Options = [dgEditing, dgTitles, dgIndicator, dgColumnResize, dgColLines, dgRowLines, dgTabs, dgConfirmDelete, dgCancelOnExit, dgWordWrap, dgShowCellHint, dgFixedResizable, dgFixedEditable]
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
  end
  object qryWIP: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    CommandTimeout = 360
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 28
    Top = 238
  end
  object qryIssue: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    CommandTimeout = 360
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 28
    Top = 286
  end
  object qryPass: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    CommandTimeout = 360
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 28
    Top = 342
  end
  object dsWIP: TDataSource
    DataSet = qryWIP
    Left = 80
    Top = 248
  end
  object dsIssue: TDataSource
    DataSet = qryIssue
    Left = 88
    Top = 296
  end
  object dsPass: TDataSource
    DataSet = qryPass
    Left = 88
    Top = 352
  end
  object qryInFFG: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    CommandTimeout = 360
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 156
    Top = 246
  end
  object qryScrap: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    CommandTimeout = 360
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 164
    Top = 294
  end
  object qryMRBScrap: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    CommandTimeout = 360
    EnableBCD = False
    Parameters = <>
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 148
    Top = 342
  end
  object dsInFFG: TDataSource
    DataSet = qryInFFG
    Left = 216
    Top = 248
  end
  object dsScrap: TDataSource
    DataSet = qryScrap
    Left = 216
    Top = 296
  end
  object dsMRBScrap: TDataSource
    DataSet = qryMRBScrap
    Left = 216
    Top = 344
  end
  object qryPartNum: TJSdTable
    ConnectionString = 'FILE NAME=D:\MIS\Client\JSISData.udl'
    CursorType = ctStatic
    LockType = ltReadOnly
    CommandTimeout = 360
    EnableBCD = False
    Parameters = <>
    SQL.Strings = (
      'select PartNum, PartName from MINdV_MatInfoMB0')
    EnableUpdateLog = False
    LookupType = lkNone
    IndexFieldNames = ''
    Left = 292
    Top = 190
  end
  object dsPartNum: TDataSource
    DataSet = qryPartNum
    Left = 344
    Top = 200
  end
end

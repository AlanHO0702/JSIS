unit EMOdProdLayerWork;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, PaperOrgDLL, JSdGrid2Excel, Menus, JSdPopupMenu, DB,
  JSdTable, ADODB, StdCtrls, Mask, DBCtrls, JSdLabel, ExtCtrls, Grids, Wwdbigrd,
  Wwdbgrid, JSdDBGrid, ComCtrls, Buttons, Wwdatsrc, JSdLookupCombo,
  ShellAPI;

type
  TfrmEMOdProdLayerWork = class(TfrmPaperOrgDLL)
    // ===== 頂部 FormType Tab（主/子單切換）=====
    pgeFormType: TPageControl;
    tstMain: TTabSheet;
    tstSub: TTabSheet;

    // ===== Master 區 - 品號資訊顯示 =====
    pnlMasterInfo: TPanel;
    JSdLabel1: TJSdLabel;
    JSdLabel2: TJSdLabel;
    JSdLabel3: TJSdLabel;
    edtPartNum: TDBEdit;
    edtRevision: TDBEdit;
    lblPartNumBack: TLabel;
    lblRevisionBack: TLabel;
    lblParam: TLabel;
    lblCusId: TLabel;
    lblNavSource: TLabel;

    // ===== Master Tab 頁籤（動態頁籤用 Tb1~Tb11）=====
    pgeMasterWork: TPageControl;
    tbshtMasterWork1: TTabSheet;
    tbshtMasterWork2: TTabSheet;
    tbshtMasterWork3: TTabSheet;
    tbshtMasterWork4: TTabSheet;
    tbshtMasterWork5: TTabSheet;
    tbshtMasterWork6: TTabSheet;
    tbshtMasterWork7: TTabSheet;
    tbshtMasterWork8: TTabSheet;
    tbshtMasterWork9: TTabSheet;
    tbshtMasterWork10: TTabSheet;
    tbshtMasterWork11: TTabSheet;

    // ===== Master ScrollBox for dynamic fields =====
    pnlMasterWork1: TScrollBox;
    pnlMasterWork2: TScrollBox;
    pnlMasterWork3: TScrollBox;
    pnlMasterWork4: TScrollBox;
    pnlMasterWork5: TScrollBox;
    pnlMasterWork6: TScrollBox;
    pnlMasterWork7: TScrollBox;
    pnlMasterWork8: TScrollBox;
    pnlMasterWork9: TScrollBox;
    pnlMasterWork10: TScrollBox;
    pnlMasterWork11: TScrollBox;

    // ===== Browse Grid 捲軸 =====
    sclPnlMaster: TScrollBar;

    // ===== Detail Tab（動態頁籤，由 DB 產生）=====
    // 由 prcEMODetailSet 動態控制，此處預留 Detail1~6 佔位用

    // ===== 工具列 Toolbar =====
    pnlLayerTools: TPanel;
    btnChangeProc:   TSpeedButton;  // 換製程路線
    btnBackupNotes:  TSpeedButton;  // 備份工程備註
    btnPasteNotes:   TSpeedButton;  // 還原工程備註
    btnNoteStyleTree:TSpeedButton;  // 工程備註樣板
    btnRouteChange:  TSpeedButton;  // 路線異動
    btnProcBOMSet:   TSpeedButton;  // 製程BOM設定
    btnUseNotes:     TSpeedButton;  // 使用備註

    // ===== Navigator =====
    DBNavigator2: TDBNavigator;
    panRoute: TPanel;

    // ===== Route / BOM 區 =====
    pnlRouteTools: TPanel;
    dbgBOM: TJSdDBGrid;
    DBMemo1: TDBMemo;
    Splitter1: TSplitter;
    Splitter2: TSplitter;
    Splitter3: TSplitter;
    Splitter4: TSplitter;
    Splitter5: TSplitter;
    Panel1: TPanel;

    // ===== 路線備註 =====
    pnlRouteNote: TPanel;
    pnlNoteSep: TPanel;

    // ===== UseNotes 核取 =====
    pnlUseNotes: TPanel;
    chkUseNotes: TDBCheckBox;

    // ===== 附件路徑 =====
    pnlPath: TPanel;
    lblAnnex: TJSdLabel;
    DBEdit1: TDBEdit;
    DBEdit2: TDBEdit;
    btnCMap:        TSpeedButton;
    btnCMapOpen:    TSpeedButton;
    OpenDialog1:    TOpenDialog;

    // ===== Query =====
    qryNotesIN:          TADOQuery;
    qryNotesOUT:         TADOQuery;
    dsLayerBOM:          TDataSource;
    tblLayerBOM:         TJSdTable;
    qryProdLayerWorkDel: TADOQuery;
    qryProcCodeBOMSet:   TADOQuery;
    qryPage:             TADOQuery;

    // ===== Scrollbar 2 =====
    sclPnlMaster2: TScrollBar;

    // ===== 離開按鈕 =====
    btExit: TSpeedButton;

    // ===== Events =====
    procedure btnGetParamsClick(Sender: TObject);
    procedure pgeFormTypeChange(Sender: TObject);
    procedure pgeDetailChange(Sender: TObject);
    procedure pgeMasterWorkChange(Sender: TObject);

    procedure btnChangeProcClick(Sender: TObject);
    procedure btnNoteStyleTreeClick(Sender: TObject);
    procedure btnBackupNotesClick(Sender: TObject);
    procedure btnPasteNotesClick(Sender: TObject);
    procedure btnRouteChangeClick(Sender: TObject);
    procedure btnProcBOMSetClick(Sender: TObject);
    procedure btnUseNotesClick(Sender: TObject);

    procedure btnCMapClick(Sender: TObject);
    procedure btnCMapOpenClick(Sender: TObject);

    procedure btExitClick(Sender: TObject);

    procedure tblLayerBOMBeforeEdit(DataSet: TDataSet);
    procedure tblLayerBOMAfterEdit(DataSet: TDataSet);

    procedure btnUpdateClick(Sender: TObject);
    procedure btnSaveHeightClick(Sender: TObject);
    procedure btnKeepStatusClick(Sender: TObject);
    procedure btnViewClick(Sender: TObject);

    procedure qryDetail1AfterEdit(DataSet: TDataSet);
    procedure qryDetail1AfterInsert(DataSet: TDataSet);
    procedure qryDetail1BeforeInsert(DataSet: TDataSet);
    procedure qryDetail1BeforeDelete(DataSet: TDataSet);
    procedure qryDetail1AfterPost(DataSet: TDataSet);
    procedure qryDetail4AfterPost(DataSet: TDataSet);
    procedure qryBrowseAfterPost(DataSet: TDataSet);
    procedure qryBrowseAfterScroll(DataSet: TDataSet);

    procedure dbgBOMColEnter(Sender: TObject);
    procedure gridDetail1Enter(Sender: TObject);

    procedure sclPnlMasterScroll(Sender: TObject; ScrollCode: TScrollCode;
      var ScrollPos: Integer);

    procedure btnC1Click(Sender: TObject);
    procedure nav2BeforeAction(Sender: TObject; Button: TNavigateBtn);

    procedure pnl_PaperOrgTopRecCounterDblClick(Sender: TObject); override;

  private
    procedure ParseFormatW(sInput, sDim: WideString;
      var sParam: array of WideString; iPara: Integer);
  public
    var iTimeOut, iScrollHeight: Integer;
    var iStatus, iNeedRefresh: Integer;
    var sSearchStr: String;
    var iDtlActivePage: Integer;
    var iBefInsItem, iInsUseNav: Integer;

    procedure AuditCheck;
    procedure LockButton(bStatus: Boolean);
    procedure UpdateDesigner;
    procedure SetScroll;
    procedure SetFieldParent;
    procedure JSdLookupComboSubEnter(Sender: TObject);
    procedure prcEMODetailSet;
  end;

var
  frmEMOdProdLayerWork: TfrmEMOdProdLayerWork;

implementation

uses TmpRouteSelect, RouteInsNew, LayerRouteSet, unit_DLL;

{$R *.dfm}

// ============================================================================
// pgeFormTypeChange - 主/子單 Tab 切換
// ============================================================================
procedure TfrmEMOdProdLayerWork.pgeFormTypeChange(Sender: TObject);
var sNowPart, sNowRev, sNowLayer: String;
begin
  inherited;
  if pgeFormType.ActivePage = tstMain then
  begin
    pgeDetail.Align   := alRight;
    pgeDetail.Visible := False;
    pgeMasterWork.Visible := True;
    pgeMasterWork.Align   := alClient;
    if iNeedRefresh = 1 then
    begin
      sNowPart  := dsBrowse.DataSet.FieldByName('PartNum').AsString;
      sNowRev   := dsBrowse.DataSet.FieldByName('Revision').AsString;
      sNowLayer := dsBrowse.DataSet.FieldByName('LayerId').AsString;
      qryBrowse.Close;
      qryBrowse.Open;
      qryBrowse.Locate('PartNum;Revision;LayerId',
        VarArrayOf([sNowPart, sNowRev, sNowLayer]), [loPartialKey]);
      iNeedRefresh := 0;
    end;
  end
  else
  begin
    pgeMasterWork.Align   := alRight;
    pgeMasterWork.Visible := False;
    pgeDetail.Visible := True;
    pgeDetail.Align   := alClient;
  end;
  pgeDetailChange(Sender);
end;

// ============================================================================
// pgeDetailChange - Detail Tab 切換時同步 Navigator DataSource
// ============================================================================
procedure TfrmEMOdProdLayerWork.pgeDetailChange(Sender: TObject);
var iPage: Integer;
begin
  inherited;
  // 依 qryPage 確認目前頁對應 KindItem
  qryPage.Locate('SerialNum', pgeDetail.ActivePageIndex + 1, [loCaseInsensitive]);
  iPage := qryPage.FieldByName('KindItem').AsInteger;
  if iPage = 0 then
  begin
    nav1.DataSource := dsBrowse;
    nav2.DataSource := dsBrowse;
  end
  else
  begin
    if FindComponent('dsDetail' + IntToStr(iPage)) <> nil then
    begin
      if sNowMode = 'UPDATE' then
        nav1.DataSource := TDataSource(FindComponent('dsDetail' + IntToStr(iPage)));
      nav2.DataSource := TDataSource(FindComponent('dsDetail' + IntToStr(iPage)));
    end;
  end;
end;

// ============================================================================
// pgeMasterWorkChange - Master Tab 切換
// ============================================================================
procedure TfrmEMOdProdLayerWork.pgeMasterWorkChange(Sender: TObject);
begin
  inherited;
end;

// ============================================================================
// pnl_PaperOrgTopRecCounterDblClick - 展開時判斷 CombineMas 參數
// ============================================================================
procedure TfrmEMOdProdLayerWork.pnl_PaperOrgTopRecCounterDblClick(Sender: TObject);
begin
  inherited;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
      + ' and ParamId=''CombineMas'' and IsNull(Value,'''')<>''''');
    Open;
    // 若有設定 CombineMas 則顯示合併 Master Tab
    if RecordCount > 0 then
    begin
      tbshtMasterWork1.Caption := FieldByName('Value').AsString;
      tstMain.TabVisible := False;
      tstSub.TabVisible  := False;
    end;
    qryExec.Close;
  end;
end;

// ============================================================================
// btnGetParamsClick - 作業初始化（從 DB 抓參數、動態頁籤設定）
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnGetParamsClick(Sender: TObject);
var sPaperMasTb1Caption, sPaperMasTb2Caption,
    sPaperMasTb3Caption, sPaperMasTb4Caption,
    sPaperMasTb5Caption, sPaperMasTb6Caption,
    sPaperMasTb7Caption, sPaperMasTb8Caption,
    sPaperMasTb9Caption, sPaperMasTb10Caption,
    sPaperMasTb11Caption: String;
    sPaperTopTb1Caption, sPaperTopTb2Caption: String;
    iMasHeight, iDetHeight: Integer;
    sList: TStringList;
    sFontSize: String;
    FontSize: Integer;
begin
  inherited;

  // 初始化預設值
  iNeedRefresh     := 0;
  iDtlActivePage   := 0;
  iBefInsItem      := 0;
  iInsUseNav       := 0;
  iMasHeight       := 0;
  iDetHeight       := 0;
  FontSize         := 100;

  // 動態頁籤 Caption 預設（由 DB 覆蓋）
  sPaperMasTb1Caption  := tbshtMasterWork1.Caption;
  sPaperMasTb2Caption  := tbshtMasterWork2.Caption;
  sPaperMasTb3Caption  := tbshtMasterWork3.Caption;
  sPaperMasTb4Caption  := tbshtMasterWork4.Caption;
  sPaperMasTb5Caption  := tbshtMasterWork5.Caption;
  sPaperMasTb6Caption  := tbshtMasterWork6.Caption;
  sPaperMasTb7Caption  := tbshtMasterWork7.Caption;
  sPaperMasTb8Caption  := tbshtMasterWork8.Caption;
  sPaperMasTb9Caption  := tbshtMasterWork9.Caption;
  sPaperMasTb10Caption := tbshtMasterWork10.Caption;
  sPaperMasTb11Caption := tbshtMasterWork11.Caption;
  sPaperTopTb1Caption  := tstMain.Caption;
  sPaperTopTb2Caption  := tstSub.Caption;

  // FontSize 設定
  if FileExists(DLLGetTempPathStr + 'JSIS\FontSize.txt') then
  begin
    sList := TStringList.Create;
    try
      sList.LoadFromFile(DLLGetTempPathStr + 'JSIS\FontSize.txt');
      if sList.Count > 0 then
      begin
        sFontSize := sList.Strings[0];
        FontSize  := StrToIntDef(sFontSize, 100);
      end;
    finally
      sList.Free;
    end;
  end;

  // 讀取 CURdOCXItemOtherRule 的 Item 參數
  with qryExec do
  begin
    if Active then Close;
    SQL.Clear;
    SQL.Add('select RuleId, DLLValue from CURdOCXItemOtherRule(nolock)');
    SQL.Add(' where ItemId=''' + sItemId + '''');
    Open;
  end;

  if qryExec.RecordCount > 0 then
  begin
    if qryExec.Locate('RuleId', 'PaperMasTb1Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork1.TabVisible := True;
      tbshtMasterWork1.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb2Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork2.TabVisible := True;
      tbshtMasterWork2.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb3Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork3.TabVisible := True;
      tbshtMasterWork3.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb4Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork4.TabVisible := True;
      tbshtMasterWork4.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb5Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork5.TabVisible := True;
      tbshtMasterWork5.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb6Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork6.TabVisible := True;
      tbshtMasterWork6.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb7Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork7.TabVisible := True;
      tbshtMasterWork7.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb8Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork8.TabVisible := True;
      tbshtMasterWork8.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb9Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork9.TabVisible := True;
      tbshtMasterWork9.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb10Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork10.TabVisible := True;
      tbshtMasterWork10.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperMasTb11Caption', [loCaseInsensitive]) then
    begin
      tbshtMasterWork11.TabVisible := True;
      tbshtMasterWork11.Caption    := Trim(qryExec.Fields[1].AsString);
    end;
    if qryExec.Locate('RuleId', 'PaperTopTb1Caption', [loCaseInsensitive]) then
      sPaperTopTb1Caption := Trim(qryExec.Fields[1].AsString);
    if qryExec.Locate('RuleId', 'PaperTopTb2Caption', [loCaseInsensitive]) then
      sPaperTopTb2Caption := Trim(qryExec.Fields[1].AsString);
    if qryExec.Locate('RuleId', 'PaperMasterHeight', [loCaseInsensitive]) then
      iMasHeight := qryExec.Fields[1].AsInteger;
    if qryExec.Locate('RuleId', 'PaperDetailHeight', [loCaseInsensitive]) then
      iDetHeight := qryExec.Fields[1].AsInteger;

    tstMain.Caption := sPaperTopTb1Caption;
    tstSub.Caption  := sPaperTopTb2Caption;
  end;

  // 套用高度
  if iMasHeight > 0 then
    pnlMasterInfo.Height := Round(iMasHeight * (FontSize / 100));

  // 動態 Detail 頁籤設定
  prcEMODetailSet;

  // 開啟所需 Query
  tblLayerBOM.Close;
  tblLayerBOM.Open;

  LockButton(True);

  // 切換到主單
  pgeFormTypeChange(Sender);
end;

// ============================================================================
// prcEMODetailSet - 由 DB 動態設定 Detail 頁籤（對應 EMOdGetDetailSet SP）
// ============================================================================
procedure TfrmEMOdProdLayerWork.prcEMODetailSet;
begin
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('exec EMOdGetDetailSet ''' + sItemId + ''',''' + sLanguageId + '''');
    Open;
  end;
  // 透過 inherited 的 prcEMODetailSet 動態產生頁籤
  inherited prcEMODetailSet;
end;

// ============================================================================
// qryBrowseAfterScroll - Browse 捲動後刷新關聯資料
// ============================================================================
procedure TfrmEMOdProdLayerWork.qryBrowseAfterScroll(DataSet: TDataSet);
begin
  inherited;
  UpdateDesigner;
end;

// ============================================================================
// qryBrowseAfterPost - Browse 儲存後刷新
// ============================================================================
procedure TfrmEMOdProdLayerWork.qryBrowseAfterPost(DataSet: TDataSet);
begin
  inherited;
  UpdateDesigner;
end;

// ============================================================================
// UpdateDesigner - 刷新設計者/狀態顯示
// ============================================================================
procedure TfrmEMOdProdLayerWork.UpdateDesigner;
begin
  // 預留：可於此更新頁首狀態列或設計者欄位
end;

// ============================================================================
// AuditCheck - 審核狀態檢查（唯讀保護）
// ============================================================================
procedure TfrmEMOdProdLayerWork.AuditCheck;
begin
  // 若單據處於審核狀態則提示並 Abort
  if sNowMode <> 'UPDATE' then
  begin
    MsgDlgJS('目前非編輯模式，無法執行此操作', mtWarning, [mbOK], 0);
    Abort;
  end;
end;

// ============================================================================
// LockButton - 依審核狀態鎖定/解鎖工具列按鈕
// ============================================================================
procedure TfrmEMOdProdLayerWork.LockButton(bStatus: Boolean);
begin
  btnChangeProc.Enabled    := bStatus;
  btnBackupNotes.Enabled   := bStatus;
  btnPasteNotes.Enabled    := bStatus;
  btnNoteStyleTree.Enabled := bStatus;
  btnRouteChange.Enabled   := bStatus;
  btnProcBOMSet.Enabled    := bStatus;
  btnUseNotes.Enabled      := bStatus;
end;

// ============================================================================
// SetScroll - 設定捲軸範圍
// ============================================================================
procedure TfrmEMOdProdLayerWork.SetScroll;
begin
  // 依 Master 面板高度動態調整
  if iScrollHeight > 0 then
    sclPnlMaster.Max := iScrollHeight;
end;

// ============================================================================
// SetFieldParent - 動態欄位父容器設定（DB 驅動的欄位配置）
// ============================================================================
procedure TfrmEMOdProdLayerWork.SetFieldParent;
begin
  // 預留：搭配動態欄位產生邏輯
end;

// ============================================================================
// JSdLookupComboSubEnter - LookupCombo 子項目 Enter 事件
// ============================================================================
procedure TfrmEMOdProdLayerWork.JSdLookupComboSubEnter(Sender: TObject);
begin
  inherited;
end;

// ============================================================================
// ParseFormatW - 解析格式化寬字串
// ============================================================================
procedure TfrmEMOdProdLayerWork.ParseFormatW(sInput, sDim: WideString;
  var sParam: array of WideString; iPara: Integer);
var i, iPos: Integer;
begin
  for i := 0 to iPara - 1 do
  begin
    iPos := Pos(sDim, sInput);
    if iPos > 0 then
    begin
      sParam[i] := Copy(sInput, 1, iPos - 1);
      sInput    := Copy(sInput, iPos + Length(sDim), Length(sInput));
    end
    else
    begin
      sParam[i] := sInput;
      Break;
    end;
  end;
end;

// ============================================================================
// sclPnlMasterScroll - Master 捲軸事件
// ============================================================================
procedure TfrmEMOdProdLayerWork.sclPnlMasterScroll(Sender: TObject;
  ScrollCode: TScrollCode; var ScrollPos: Integer);
begin
  inherited;
end;

// ============================================================================
// qryDetail1 事件
// ============================================================================
procedure TfrmEMOdProdLayerWork.qryDetail1AfterEdit(DataSet: TDataSet);
begin
  inherited;
  nav2.DataSource := dsDetail1;
end;

procedure TfrmEMOdProdLayerWork.qryDetail1BeforeDelete(DataSet: TDataSet);
var sSQL: String;
begin
  inherited;
  if ((TJSdTable(DataSet).FindField('Item') = nil)
    or (TJSdTable(DataSet).FindField('PaperNum') = nil)) then
  begin
    sSQL := 'exec EMOdDLLdoDelete ''' + TJSdTable(DataSet).TableName + ''','
      + '''' + qryBrowse.FieldByName('PartNum').AsString + ''','
      + '''' + qryBrowse.FieldByName('Revision').AsString + ''','
      + '''' + qryBrowse.FieldByName('LayerId').AsString + ''','
      + DataSet.FieldByName('SerialNum').AsString;
    unit_DLL.OpenSQLDLL(qryExec, 'EXEC', sSQL);
    DataSet.Close;
    DataSet.Open;
    Abort;
  end;
end;

procedure TfrmEMOdProdLayerWork.qryDetail1BeforeInsert(DataSet: TDataSet);
begin
  inherited;
  if DataSet.FindField('Item') = nil then
    DataSet.Tag := GetMaxSerialNumDLL(DataSet, 'SerialNum') + 1;

  if TJSdTable(DataSet).TableName = 'EMOdLayerHole' then
  begin
    if not (DataSet.FindField('DecSerial') = nil) then
      iBefInsItem := DataSet.FieldByName('DecSerial').AsInteger;
  end;
end;

procedure TfrmEMOdProdLayerWork.qryDetail1AfterInsert(DataSet: TDataSet);
var bReadOnly: Boolean;
    iSe, iMax: Integer;
begin
  inherited;
  if DataSet.FindField('Item') = nil then
  begin
    bReadOnly := DataSet.FieldByName('SerialNum').ReadOnly;
    if bReadOnly then DataSet.FieldByName('SerialNum').ReadOnly := False;
    DataSet.FieldByName('SerialNum').AsInteger := DataSet.Tag;
    if bReadOnly then DataSet.FieldByName('SerialNum').ReadOnly := True;
  end;

  if TJSdTable(DataSet).TableName = 'EMOdLayerHole' then
  begin
    if not (DataSet.FindField('Item') = nil) then
    begin
      with qryExec do
      begin
        Close;
        SQL.Clear;
        SQL.Add('select SerialNum=Max(SerialNum)+1 from EMOdLayerHole(nolock) '
          + 'where PartNum=''' + qryBrowse.FieldByName('PartNum').AsString + ''''
          + 'and Revision=''' + qryBrowse.FieldByName('Revision').AsString + ''''
          + 'and LayerId=''' + qryBrowse.FieldByName('LayerId').AsString + '''');
        Open;
        if RecordCount = 0 then iSe := 1
        else iSe := FieldByName('SerialNum').AsInteger;
        bReadOnly := DataSet.FieldByName('SerialNum').ReadOnly;
        if bReadOnly then DataSet.FieldByName('SerialNum').ReadOnly := False;
        DataSet.FieldByName('SerialNum').AsInteger := iSe;
        if bReadOnly then DataSet.FieldByName('SerialNum').ReadOnly := True;
      end;
    end;

    if not (DataSet.FindField('DecSerial') = nil) then
    begin
      with qryExec do
      begin
        Close;
        SQL.Clear;
        SQL.Add('select DecSerial=Max(DecSerial) from EMOdLayerHole(nolock) '
          + 'where PartNum=''' + qryBrowse.FieldByName('PartNum').AsString + ''''
          + 'and Revision=''' + qryBrowse.FieldByName('Revision').AsString + ''''
          + 'and LayerId=''' + qryBrowse.FieldByName('LayerId').AsString + '''');
        Open;
        iMax := FieldByName('DecSerial').AsInteger;
      end;
      bReadOnly := DataSet.FieldByName('DecSerial').ReadOnly;
      if bReadOnly then DataSet.FieldByName('DecSerial').ReadOnly := False;
      if iBefInsItem > 0 then
      begin
        if (iMax = iBefInsItem) and (iInsUseNav = 0) then
          DataSet.FieldByName('DecSerial').AsFloat := iBefInsItem + 1
        else
          DataSet.FieldByName('DecSerial').AsFloat := iBefInsItem - 0.5;
      end
      else
        DataSet.FieldByName('DecSerial').AsFloat := 1;
      iInsUseNav := 0;
      if bReadOnly then DataSet.FieldByName('DecSerial').ReadOnly := True;
    end;
  end;
end;

procedure TfrmEMOdProdLayerWork.qryDetail1AfterPost(DataSet: TDataSet);
begin
  inherited;
  UpdateDesigner;
end;

procedure TfrmEMOdProdLayerWork.qryDetail4AfterPost(DataSet: TDataSet);
begin
  inherited;
  if TJSdTable(DataSet).TableName = 'EMOdLayerHole' then
    iNeedRefresh := 1;
  UpdateDesigner;
end;

// ============================================================================
// nav2BeforeAction - Navigator 動作前處理
// ============================================================================
procedure TfrmEMOdProdLayerWork.nav2BeforeAction(Sender: TObject;
  Button: TNavigateBtn);
begin
  inherited;
  if nav2.DataSource.DataSet is TJSdTable then
  begin
    if TJSdTable(nav2.DataSource.DataSet).TableName = 'EMOdLayerHole' then
    begin
      iInsUseNav := 0;
      if Button = nbInsert then iInsUseNav := 1;
    end;
  end;
end;

// ============================================================================
// Toolbar 按鈕事件
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnUpdateClick(Sender: TObject);
begin
  inherited;
end;

procedure TfrmEMOdProdLayerWork.btnSaveHeightClick(Sender: TObject);
begin
  inherited;
end;

procedure TfrmEMOdProdLayerWork.btnKeepStatusClick(Sender: TObject);
begin
  inherited;
end;

procedure TfrmEMOdProdLayerWork.btnViewClick(Sender: TObject);
begin
  inherited;
end;

procedure TfrmEMOdProdLayerWork.btnC1Click(Sender: TObject);
begin
  inherited;
end;

// ============================================================================
// btnChangeProcClick - 換製程路線
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnChangeProcClick(Sender: TObject);
var TmpRouteId: String;
    sNowPart, sNowRev, sNowLayer: String;
begin
  inherited;
  AuditCheck;
  sNowPart  := dsBrowse.DataSet.FieldByName('PartNum').AsString;
  sNowRev   := dsBrowse.DataSet.FieldByName('Revision').AsString;
  sNowLayer := dsBrowse.DataSet.FieldByName('LayerId').AsString;
  TmpRouteId := dsBrowse.DataSet.FieldByName('TmpRouteId').AsString;

  Application.CreateForm(TdlgTmpRouteSelect, dlgTmpRouteSelect);
  dlgTmpRouteSelect.sConnectStr := sConnectStr;
  dlgTmpRouteSelect.prcDoSetConnOCX;

  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
      + ' and ParamId=''TmpRouteActiveType'' and Value=''1''');
    Open;
    if RecordCount > 0 then dlgTmpRouteSelect.iTmpActive := 1
    else dlgTmpRouteSelect.iTmpActive := 0;
  end;

  with dlgTmpRouteSelect do
  begin
    if iTimeOut > 0 then
    begin
      qryMas.CommandTimeout    := iTimeOut;
      qryTmpIns.CommandTimeout := iTimeOut;
      qryTmpDel.CommandTimeout := iTimeOut;
    end;
    qryProcBasic.Close;
    qryProcBasic.Open;
    CurrPartNum := sNowPart;
    CurrRevision := sNowRev;
    With qryTmp do
    Begin
      Parameters.ParamByName('PartNum').Value  := CurrPartNum;
      Parameters.ParamByName('Revision').Value := CurrRevision;
    End;
    qryTmpDtl.Close; qryTmpDtl.Open;
    qryMas.Close;    qryMas.Open;
    qryDtl.Close;    qryDtl.Open;
    qryTmp.Close;    qryTmp.Open;
    qryTmpDtl2.Close; qryTmpDtl2.Open;
    btnSearchClick(Sender);
    pgeMaster.ActivePageIndex := 0;
    pgeDtl.ActivePageIndex    := iDtlActivePage;
    pgeMaster.Pages[1].TabVisible := False;
    ShowModal;
    if ModalResult = mrOK then
    begin
      with qryExec do
      begin
        Close;
        SQL.Clear;
        SQL.Add('exec EMOdInsLayerRoute ''' + sNowPart + ''',''' + sNowRev + ''','''
          + sNowLayer + ''',''' + qryTmpMas.FieldByName('TmpId').AsString + '''');
        Open;
      end;
      UpdateDesigner;
      qryBrowse.Close;
      qryBrowse.Open;
      qryDetail1.Close;
      qryDetail1.Open;
      qryBrowse.Locate('PartNum;Revision;LayerId',
        VarArrayOf([sNowPart, sNowRev, sNowLayer]), [loPartialKey]);
    end;
  end;
end;

// ============================================================================
// btnNoteStyleTreeClick - 工程備註樣板
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnNoteStyleTreeClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
  Application.CreateForm(TdlgTmpRouteSelect, dlgTmpRouteSelect);
  dlgTmpRouteSelect.sConnectStr := sConnectStr;
  dlgTmpRouteSelect.prcDoSetConnOCX;
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
      + ' and ParamId=''TmpRouteActiveType'' and Value=''1''');
    Open;
    if RecordCount > 0 then dlgTmpRouteSelect.iTmpActive := 1
    else dlgTmpRouteSelect.iTmpActive := 0;
  end;
  with dlgTmpRouteSelect do
  begin
    qryProcBasic.Close; qryProcBasic.Open;
    CurrPartNum  := dsBrowse.DataSet.FieldByName('PartNum').AsString;
    CurrRevision := dsBrowse.DataSet.FieldByName('Revision').AsString;
    With qryTmp do
    Begin
      Parameters.ParamByName('PartNum').Value  := CurrPartNum;
      Parameters.ParamByName('Revision').Value := CurrRevision;
    End;
    qryTmpDtl.Close; qryTmpDtl.Open;
    qryMas.Close;    qryMas.Open;
    qryDtl.Close;    qryDtl.Open;
    qryTmp.Close;    qryTmp.Open;
    qryTmpDtl2.Close; qryTmpDtl2.Open;
    btnSearchClick(Sender);
    pgeMaster.ActivePageIndex := 1;
    pgeMaster.Pages[0].TabVisible := False;
    ShowModal;
    if ModalResult = mrOK then
    begin
      with qryExec do
      begin
        Close;
        SQL.Clear;
        SQL.Add('Exec EMOdProcNotesInsert '''
          + dsBrowse.DataSet.FieldByName('PartNum').AsString + ''', '''
          + dsBrowse.DataSet.FieldByName('Revision').AsString + ''', '''
          + dsBrowse.DataSet.FieldByName('LayerId').AsString + '''');
        ExecSQL;
      end;
      UpdateDesigner;
    end;
  end;
end;

// ============================================================================
// btnBackupNotesClick - 備份工程備註
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnBackupNotesClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
  with qryNotesIN do
  begin
    Parameters.ParamByName('PartNum').Value  := dsBrowse.DataSet.FieldByName('PartNum').AsString;
    Parameters.ParamByName('Revision').Value := dsBrowse.DataSet.FieldByName('Revision').AsString;
    Parameters.ParamByName('LayerId').Value  := dsBrowse.DataSet.FieldByName('LayerId').AsString;
    ExecSQL;
  end;
  MsgDlgJS('已備份工程備註', mtInformation, [mbOK], 0);
end;

// ============================================================================
// btnPasteNotesClick - 還原工程備註
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnPasteNotesClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
  with qryNotesOUT do
  begin
    Parameters.ParamByName('PartNum').Value  := dsBrowse.DataSet.FieldByName('PartNum').AsString;
    Parameters.ParamByName('Revision').Value := dsBrowse.DataSet.FieldByName('Revision').AsString;
    Parameters.ParamByName('LayerId').Value  := dsBrowse.DataSet.FieldByName('LayerId').AsString;
    ExecSQL;
  end;
  UpdateDesigner;
  qryDetail1.Refresh;
  MsgDlgJS('已還原工程備註', mtInformation, [mbOK], 0);
end;

// ============================================================================
// btnRouteChangeClick - 路線異動
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnRouteChangeClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
  Application.CreateForm(TdlgRouteInsNew, dlgRouteInsNew);
  dlgRouteInsNew.sConnectStr := sConnectStr;
  dlgRouteInsNew.prcDoSetConnOCX;
  with dlgRouteInsNew do
  begin
    BPartNum  := dsBrowse.DataSet.FieldByName('PartNum').AsString;
    BRevision := dsBrowse.DataSet.FieldByName('Revision').AsString;
    BLayerId  := dsBrowse.DataSet.FieldByName('LayerId').AsString;
    ShowModal;
  end;
  qryDetail1.Close;
  qryDetail1.Open;
end;

// ============================================================================
// btnProcBOMSetClick - 製程BOM設定
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnProcBOMSetClick(Sender: TObject);
var i: Integer;
    iRange: Integer;
begin
  inherited;
  if qryDetail1.RecordCount <= 0 then Exit;

  AuditCheck;
  Application.CreateForm(TdlgLayerRouteSet, dlgLayerRouteSet);
  dlgLayerRouteSet.sConnectStr := sConnectStr;
  dlgLayerRouteSet.prcDoSetConnOCX;

  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) '
      + 'where SystemId=''EMO'' and ParamId=''MatClassSelDefault''');
    Open;
    if FieldByName('Value').AsString <> '' then
    begin
      dlgLayerRouteSet.cboClassMat.Visible := True;
      dlgLayerRouteSet.cboClassMat.Text    := FieldByName('Value').AsString;
      dlgLayerRouteSet.sClassMat           := FieldByName('Value').AsString;
    end;
  end;

  with dlgLayerRouteSet do
  begin
    if iTimeOut > 0 then
    begin
      qryProdPressMat.CommandTimeout := iTimeOut;
      qryLayerPress.CommandTimeout   := iTimeOut;
      qryProdLayer.CommandTimeout    := iTimeOut;
    end;
    qryProdLayer.Close;
    qryProdLayer.Open;
    qry2MatClass.Close;
    qry2MatClass.Parameters.ParamByName('ClassMat').Value := sClassMat;
    qry2MatClass.Open;
    qryClassMat.Close;
    qryClassMat.Open;

    iRange := qry2MatClass.RecordCount div 4;
    if iRange > 3 then
    begin
      dlgLayerRouteSet.Height      := dlgLayerRouteSet.Height + (13 * (iRange - 3));
      pnlTOP.Height                := pnlTOP.Height + (13 * (iRange - 3));
      rdoMatClass.Height           := rdoMatClass.Height + (13 * (iRange - 3));
    end;

    BPartNum  := qryDetail1.FieldByName('PartNum').AsString;
    BRevision := qryDetail1.FieldByName('Revision').AsString;
    BLayerId  := qryDetail1.FieldByName('LayerId').AsString;
    BProcCode := qryDetail1.FieldByName('ProcCode').AsString;
    btFindClick(Sender);
    ShowModal;

    if ModalResult = mrOK then
    begin
      with qryProdLayerWorkDel do
      begin
        Close;
        Parameters.ParamByName('PartNum').Value  := dsDetail1.DataSet.FieldByName('PartNum').AsString;
        Parameters.ParamByName('Revision').Value := dsDetail1.DataSet.FieldByName('Revision').AsString;
        Parameters.ParamByName('LayerId').Value  := dsDetail1.DataSet.FieldByName('LayerId').AsString;
        Parameters.ParamByName('ProcCode').Value := dsDetail1.DataSet.FieldByName('ProcCode').AsString;
        ExecSQL;
      end;
      with qryProcCodeBOMSet do
      begin
        for i := 0 to msSelects.TargetItems.Count - 1 do
        begin
          Parameters.ParamByName('PartNum').Value  := dsDetail1.DataSet.FieldByName('PartNum').AsString;
          Parameters.ParamByName('Revision').Value := dsDetail1.DataSet.FieldByName('Revision').AsString;
          Parameters.ParamByName('LayerId').Value  := dsDetail1.DataSet.FieldByName('LayerId').AsString;
          Parameters.ParamByName('ProcCode').Value := dsDetail1.DataSet.FieldByName('ProcCode').AsString;
          Parameters.ParamByName('SerialNum').Value := i + 1;
          Parameters.ParamByName('MatCode').Value  := msSelects.TargetItems[i].Caption;
          Parameters.ParamByName('MatName').Value  := msSelects.TargetItems[i].SubItems[0];
          ExecSQL;
        end;
      end;
      UpdateDesigner;
    end;
  end;
  tblLayerBOM.Close;
  tblLayerBOM.Open;
end;

// ============================================================================
// btnUseNotesClick - 使用備註開關
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnUseNotesClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
end;

// ============================================================================
// btnCMapClick / btnCMapOpenClick - 層別圖面路徑設定/開啟
// ============================================================================
procedure TfrmEMOdProdLayerWork.btnCMapClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
  if OpenDialog1.Execute then
  begin
    if sNowMode = 'UPDATE' then
      dsBrowse.DataSet.FieldByName('CMapPath').AsString := OpenDialog1.FileName;
  end;
end;

procedure TfrmEMOdProdLayerWork.btnCMapOpenClick(Sender: TObject);
var sPath: String;
begin
  inherited;
  sPath := Trim(dsBrowse.DataSet.FieldByName('CMapPath').AsString);
  if sPath <> '' then
    ShellExecute(Handle, 'open', PChar(sPath), nil, nil, SW_SHOWNORMAL)
  else
    MsgDlgJS('尚未設定圖面路徑', mtWarning, [mbOK], 0);
end;

// ============================================================================
// tblLayerBOM 事件
// ============================================================================
procedure TfrmEMOdProdLayerWork.tblLayerBOMBeforeEdit(DataSet: TDataSet);
begin
  inherited;
  AuditCheck;
end;

procedure TfrmEMOdProdLayerWork.tblLayerBOMAfterEdit(DataSet: TDataSet);
begin
  inherited;
end;

// ============================================================================
// Grid Enter 事件
// ============================================================================
procedure TfrmEMOdProdLayerWork.dbgBOMColEnter(Sender: TObject);
begin
  inherited;
end;

procedure TfrmEMOdProdLayerWork.gridDetail1Enter(Sender: TObject);
begin
  inherited;
  nav2.DataSource := dsDetail1;
end;

// ============================================================================
// btExitClick - 離開按鈕
// ============================================================================
procedure TfrmEMOdProdLayerWork.btExitClick(Sender: TObject);
begin
  inherited;
  Close;
end;

end.
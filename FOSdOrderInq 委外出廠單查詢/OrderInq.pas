unit OrderInq;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic, DB, ADODB, Buttons, ExtCtrls, ComCtrls, Grids, Wwdbigrd,
  Wwdbgrid, JSdDBGrid, StdCtrls, JSdLabel, JSdLookupCombo, wwdbdatetimepicker,
  JSdTable, JSdGrid2Excel, InqBase;

type
  TfrmFOSdOrderInq = class(TfrmSPOdInqBase)
    qryFOSdOrderMain: TJSdTable;
    qryFOSdReceiveSub: TJSdTable;
    dsFOSdOrderMain: TDataSource;
    dsFOSdReceiveSub: TDataSource;
    pageDtl: TPageControl;
    TabSheet2: TTabSheet;
    grid_OrderMain: TJSdDBGrid;
    TabSheet1: TTabSheet;
    grid_Rec: TJSdDBGrid;
    procedure btnGetParamsClick(Sender: TObject);
    procedure btnInqClick(Sender: TObject);
    procedure qryBrowseAfterOpen(DataSet: TDataSet);
    //procedure grid_BrowseDblClick(Sender: TObject);
    procedure grid_RecDblClick(Sender: TObject);
    procedure grid_OrderMainDblClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnHeightClick(Sender: TObject);
    procedure btnPaperTraceClick(Sender: TObject);
  private
    procedure prcPaperTraceFOS(tTbl:TJSdTable;sPaperId:string);
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmFOSdOrderInq: TfrmFOSdOrderInq;

implementation

uses LinkShowDLL, unit_DLL;

{$R *.dfm}

procedure TfrmFOSdOrderInq.btnGetParamsClick(Sender: TObject);
var sSQL:string;
    sPaperMasterHeight,sPaperDetailHeight:string;
    iPaperMasterHeight,iPaperDetailHeight:integer;
begin
  inherited;
//
  sSQL:='';

  if unit_DLL.funGetSelectSQL(qryExec,'','FOSdOrderMain4Inq',sSQL) then
  begin
    sSQL:=sSQL+' and t0.PaperNum = :PaperNum';

    with qryFOSdOrderMain do
      begin
        close;
        sql.Clear;
        sql.Add(sSQL);
        DataSource:=dsBrowse;
      end;
  end;
//
  sSQL:='';

  if unit_DLL.funGetSelectSQL(qryExec,'','FOSdReceiveSub4Inq',sSQL) then
  begin
    sSQL:=sSQL+' and t0.SourNum = :PaperNum and t0.SourItem = :Item'+
    ' and t0.PaperNum not in(select PaperNum from FOSdReceiveMain(nolock) where Finished=2)';

    with qryFOSdReceiveSub do
      begin
        close;
        sql.Clear;
        sql.Add(sSQL);
        DataSource:=dsBrowse;
      end;
  end;

//
  pageDtl.ActivePageIndex:=1;
//
  //if LowerCase(sUserId)<>'admin' then btnSaveHeight.Visible:=false;
//
  with qryExec do
  begin
    if active then close;
    sql.Clear;
    sql.Add('select RuleId,DLLValue from CURdOCXItemOtherRule(nolock) where ItemId='+
      ''''+sItemId+'''');
    open;
  end;

  if qryExec.RecordCount>0 then
    begin
      if qryExec.Locate('RuleId', 'PaperMasterHeight', [loCaseInsensitive]) then
        sPaperMasterHeight:=trim(qryExec.Fields[1].AsString);

      if qryExec.Locate('RuleId', 'PaperDetailHeight', [loCaseInsensitive]) then
        sPaperDetailHeight:=trim(qryExec.Fields[1].AsString);
    end;

  iPaperMasterHeight:=0;

  if sPaperMasterHeight<>'' then
    begin
      try
        iPaperMasterHeight:=strtoint(sPaperMasterHeight);
      except
      end;

       if iPaperMasterHeight>0 then pgeMaster.Height:=iPaperMasterHeight;
    end;

  iPaperDetailHeight:=0;

  if sPaperDetailHeight<>'' then
      begin
        try
            iPaperDetailHeight:=strtoint(sPaperDetailHeight);
        except
        end;

        if iPaperDetailHeight>0 then
          pnlBottom.Height:=iPaperDetailHeight;
      end;

  //2015.11.18 add for Bill-20151112-03
  prcStoreFieldNeed_Def(self,qryExec);
end;

procedure TfrmFOSdOrderInq.btnInqClick(Sender: TObject);
begin
  inherited;
  grid_Browse.SetFocus;
end;

procedure TfrmFOSdOrderInq.btnPaperTraceClick(Sender: TObject);
begin
  //inherited;
  prcPaperTraceFOS(TJSdTable(grid_Browse.DataSource.DataSet),'FOSdOrderMain');
end;

procedure TfrmFOSdOrderInq.btnHeightClick(Sender: TObject);
var iMasterHeight,iDetailHeight:integer;
begin
  //inherited;
  iMasterHeight:=pgeMaster.Height;
  iDetailHeight:=pnlBottom.Height;

  with qryExec do
    begin
      if Active then  close;
      sql.Clear;
      sql.Add('exec CURdLayerHeightSave '+''''+sItemId+''''+','+
                inttostr(iMasterHeight)+','+
                inttostr(iDetailHeight)
                );
      ExecSQL;
      close;
    end;

  MsgDlgJS('已儲存設定',mtInformation,[mbOk],0);
end;

procedure TfrmFOSdOrderInq.FormCreate(Sender: TObject);
begin
  inherited;
  qryBrowse.LookupType:=lkJoinSQL;//2012.2.24 add
end;

{procedure TfrmFOSdOrderInq.grid_BrowseDblClick(Sender: TObject);
begin
  inherited;
  prcPaperTraceFOS(TJSdTable(grid_Browse.DataSource.DataSet),'FOSdOrderMain');
end;}

procedure TfrmFOSdOrderInq.grid_RecDblClick(Sender: TObject);
begin
  inherited;
  prcPaperTraceFOS(TJSdTable(grid_Rec.DataSource.DataSet),'FOSdReceiveMain');
end;

procedure TfrmFOSdOrderInq.grid_OrderMainDblClick(Sender: TObject);
begin
  inherited;
  prcPaperTraceFOS(TJSdTable(grid_OrderMain.DataSource.DataSet),'FOSdOrderMain');
end;

procedure TfrmFOSdOrderInq.prcPaperTraceFOS(tTbl:TJSdTable;sPaperId:string);
var sPaperNum,sItemName,sSystemId,sClassName,sItemId,sOCXTemplate:string;
    sSQL:string;
begin
  inherited;

  if tTbl.Active=false then
    begin
      MsgDlgJS('沒有單據',mtError,[mbOk],0);
      exit;
    end;

  if tTbl.RecordCount=0 then
    begin
      MsgDlgJS('沒有單據',mtError,[mbOk],0);
      exit;
    end;

  if sPaperId='' then
    begin
      MsgDlgJS('沒有單據種類',mtError,[mbOk],0);
      exit;
    end;

  sPaperNum:=tTbl.FieldByName('PaperNum').Asstring;

  if sPaperNum='' then
    begin
      MsgDlgJS('沒有單據號碼',mtError,[mbOk],0);
      exit;
    end;

  sClassName:=sPaperId+'.dll';

  sSQL:='exec CURdOCXItemIdByFromType '+''''+sPaperId+''''+','+''''+sPaperNum+'''';

  with qryExec do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qryExec.RecordCount=0 then
    begin
      MsgDlgJS('此單據種類查無程式項目',mtError,[mbOk],0);
      exit;
    end;

  sItemId :=qryExec.FieldByName('ItemId').AsString;
  sItemName:=qryExec.FieldByName('ItemName').AsString;
  sSystemId:=qryExec.FieldByName('SystemId').AsString;
  sOCXTemplate:=qryExec.FieldByName('OCXTemplete').AsString;

  if sItemId='' then
    begin
      MsgDlgJS('此單據種類查無程式項目',mtError,[mbOk],0);
      exit;
    end;

  Application.CreateForm(TfrmLinkShowDLL,frmLinkShowDLL);
  frmLinkShowDLL.Caption:=sItemName;

  unit_DLL.funCallDLL(
    qryExec,//qryExec:TADOQuery;
    nil,//fStartForm:TForm;
    2,//iCallType:integer;//0 from MainForm, 1 from DLL, 2 from Flow , 3 PaperTrace
    false,//bShowModal:boolean;
    sItemId,//sItemId,
    sItemName,//sItemName,
    sClassName,//sClassName,
    sSystemId+'^'+sLanguageId,//sSystemId,
    sServerName,//sServerName,
    sDBName,//sDBName,
    sUserId,//sUserId,
    sBUId,//sBUId,
    sUseId,//sUseId,
    sPaperId,//sPaperId,
    sPaperNum,//sPaperNum,
    sGlobalId,//sGlobalId  :string;
    frmLinkShowDLL.pnlMain,//tOtherParent:TWinControl;
    '',//sServerPath:string;
    '',//sLocalPath:string;
    sLoginSvr,//sLoginSvr:string;
    sLoginDB,//sLoginDB:string;
    //true, //bLocalTemCopy:boolean
    //6
    sOCXTemplate,
    0, //iDtlItem
    '',//sTranGlobalId
    sTempBasJSISpw //2012.06.01 add for SS Bill-20120531-01
    );

  frmLinkShowDLL.hide;
  frmLinkShowDLL.ShowModal;
end;

procedure TfrmFOSdOrderInq.qryBrowseAfterOpen(DataSet: TDataSet);
begin
  inherited;

  if qryFOSdOrderMain.SQL.Text<>'' then
    if qryFOSdOrderMain.DataSource=dsBrowse then
       if qryFOSdOrderMain.Active=false then
            qryFOSdOrderMain.Open;

  if qryFOSdReceiveSub.SQL.Text<>'' then
    if qryFOSdReceiveSub.DataSource=dsBrowse then
       if qryFOSdReceiveSub.Active=false then
            qryFOSdReceiveSub.Open;
end;

end.

unit LessMatInq;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic, DB, ADODB, Buttons, ExtCtrls, StdCtrls,
  wwdbdatetimepicker, JSdLookupCombo, JSdLabel, Grids, Wwdbigrd, Wwdbgrid,
  JSdDBGrid, ComCtrls, JSdTable;

type
  TfrmFMEdLessMatInq = class(TfrmTempPublic)
    pnlMain: TPanel;
    Panel1: TPanel;
    JSdLabel4: TJSdLabel;
    JSdLabel5: TJSdLabel;
    JSdLabel7: TJSdLabel;
    btnInq: TSpeedButton;
    cbo_MatClass: TJSdLookupCombo;
    cbo_PartNum: TJSdLookupCombo;
    ww_InqDate: TwwDBDateTimePicker;
    Splitter2: TSplitter;
    btnMatDetail: TSpeedButton;
    pageBrowse: TPageControl;
    TabSheet1: TTabSheet;
    TabSheet2: TTabSheet;
    grid_Browse: TJSdDBGrid;
    pnlInqMatLessShowDLL: TPanel;
    qryBrowse: TJSdTable;
    dsBrowse: TDataSource;
    Splitter3: TSplitter;
    grid_Browse2: TJSdDBGrid;
    JSdLabel1: TJSdLabel;
    cbo_MB: TJSdLookupCombo;
    qryMINdMatInfo: TJSdTable;
    dsMINdMatInfo: TDataSource;
    qryMINdMatClass: TJSdTable;
    dsMINdMatClass: TDataSource;
    qryMB: TJSdTable;
    dsMB: TDataSource;
    qryDisplace: TJSdTable;
    dsDisplace: TDataSource;
    qryDisplaceMatGroup: TStringField;
    qryDisplaceDisplaceMat: TStringField;
    qryDisplaceMatCode: TStringField;
    qryDisplacesKind: TStringField;
    qryMINdMatInfoPartNum: TStringField;
    qryMINdMatInfoMatName: TWideStringField;
    qryDisplaceLk_MatName: TWideStringField;
    JSdDBGrid1: TJSdDBGrid;
    ww_InqDate2: TwwDBDateTimePicker;
    JSdLabel2: TJSdLabel;
    lblType: TJSdLabel;
    qryType: TJSdTable;
    dsType: TDataSource;
    cboType: TJSdLookupCombo;
    JSdLabel3: TJSdLabel;
    edtMatName: TEdit;
    btnToExcel: TSpeedButton;
    pnlCond: TPanel;
    pnlCount: TPanel;
    procedure btnGetParamsClick(Sender: TObject);
    procedure btnInqClick(Sender: TObject);
    procedure btnMatDetailClick(Sender: TObject);
    procedure qryBrowseAfterScroll(DataSet: TDataSet);
    procedure pageBrowseChange(Sender: TObject);
    procedure qryBrowseAfterOpen(DataSet: TDataSet);
    procedure qryBrowseBeforeClose(DataSet: TDataSet);
    procedure qryDisplaceBeforeOpen(DataSet: TDataSet);
    procedure qryDisplaceAfterScroll(DataSet: TDataSet);
    procedure btnTempBasDLLDoClick(Sender: TObject);
    procedure cbo_MBChange(Sender: TObject);
    procedure btnToExcelClick(Sender: TObject);
    procedure qryBrowseAfterClose(DataSet: TDataSet);
  private
    procedure prcClearChildDLL;
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmFMEdLessMatInq: TfrmFMEdLessMatInq;
  hCall:THandle;
implementation

uses unit_DLL, commParent;

{$R *.dfm}

procedure TfrmFMEdLessMatInq.btnGetParamsClick(Sender: TObject);
begin
  inherited;
  qryMINdMatInfo.Open;
  qryMINdMatClass.Open;
  qryMB.Open;

//不能設在屬性表，因在FormCreate就會自動連線
  cbo_MatClass.LkDataSource:=dsMINdMatClass;
  cbo_PartNum.LkDataSource:=dsMINdMatInfo;
  cbo_MB.LkDataSource:=dsMB;

  cbo_MB.text:='1';  //2022.10.07 預設值改為1

  hCall:=0;

  pageBrowse.ActivePageIndex:=0;

//----------2015.11.18 add for Bill-20151112-03
prcStoreFieldNeed_Def(self,qryExec);
  //2019.05.06
  qryBrowse.TableName:='FMEdLessMatInq';
  qryBrowse.LookupType:=lkLookupTable;
  qryType.Close;
  qryType.Open;
  cboType.Text :='0';
//----------
end;

procedure TfrmFMEdLessMatInq.prcClearChildDLL;
var iTempBasDLLBtnHandle:THandle;
begin
  iTempBasDLLBtnHandle:=pnlInqMatLessShowDLL.Tag;
  if iTempBasDLLBtnHandle<>0 then
    begin
      SendMessage(iTempBasDLLBtnHandle,WM_LBUTTONDOWN, 0, 0);
      SendMessage(iTempBasDLLBtnHandle,WM_LBUTTONUP, 0, 0);
    end;

  if hCall<>0 then FreeLibrary(hCall);

  hCall:=0;
  pnlInqMatLessShowDLL.Tag:=0;
end;

procedure TfrmFMEdLessMatInq.btnInqClick(Sender: TObject);
var iMB:integer;
    sDate, sDate2:String;//2019.05.06
begin
  inherited;

  prcClearChildDLL;

  if cbo_MB.text='' then iMB:=255
  else if not((cbo_MB.text='0') or (cbo_MB.text='1') or (cbo_MB.text='255')) then iMB:=255
  else iMB:=strtoint(cbo_MB.Text);

  //2019.05.06
  if ww_InqDate.Text='' then
    sDate:='1900/01/01'
  else
    sDate:=ww_InqDate.Text;
  if ww_InqDate2.Text='' then
    sDate2:='1900/01/01'
  else
    sDate2:=ww_InqDate2.Text;

  with qryBrowse do
    begin
      close;
      SQL.Clear;
      SQL.Add('exec MINdLookMatInfoDtl_Std ''' + cbo_PartNum.Text + ''','
        +'1,''' + sDate + ''',''' + sDate2+ ''','
        +''''+cbo_MatClass.Text + ''',' + IntToStr(iMB)
        +',''' + sUseId + ''',0,'''','''',0,'''+ edtMatName.Text +''',0,'
        +cboType.Text);
      open;
      {close;
      Parameters.ParamByName('PartNum').Value:=cbo_PartNum.Text;
      Parameters.ParamByName('iInqKind').Value:=1;

      if ww_InqDate.Text='' then
        Parameters.ParamByName('InqDate').Value:='1900/01/01'
      else
        Parameters.ParamByName('InqDate').Value:=ww_InqDate.Text;

      Parameters.ParamByName('MatClass').Value:=cbo_MatClass.Text;
      Parameters.ParamByName('MB').Value:=iMB;
      Parameters.ParamByName('UseId').Value:=sUseId;
      open;}
    end;
end;

procedure TfrmFMEdLessMatInq.btnMatDetailClick(Sender: TObject);
var sPartNum:string;
begin
  inherited;

  prcClearChildDLL;

  case pageBrowse.ActivePageIndex of
    0:sPartNum:=qryBrowse.FieldByName('PartNum').AsString;
    1:if grid_Browse2.Focused then
        sPartNum:=qryBrowse.FieldByName('PartNum').AsString
      else
        sPartNum:=qryDisplace.FieldByName('MatCode').AsString;
  end;

 if sPartNum='' then
   begin
     MsgDlgJS('沒有料號',mtError,[mbOk],0);
     exit;
   end;

 hCall:=
  unit_DLL.funCallDLL(
    qryExec,//qryExec:TADOQuery;
    nil,//fStartForm:TForm;
    2,//iCallType:integer;//0 from MainForm, 1 from DLL, 2 from Flow , 3 PaperTrace
    false,//bShowModal:boolean;
    'MINdMatDetail3.dll',//sItemId,
    '',//sItemName,
    'MINdMatDetail3.dll',//sClassName,
    'MIN'+'^'+sLanguageId,//sSystemId,
    sServerName,//sServerName,
    sDBName,//sDBName,
    sUserId,//sUserId,
    sBUId,//sBUId,
    sUseId,//sUseId,
    '',//sPaperId,
    sPartNum,//sPaperNum,
    sGlobalId,//sGlobalId  :string;
    frmFMEdLessMatInq.pnlInqMatLessShowDLL,//tOtherParent:TWinControl;
    '',//sServerPath:string;
    '',//sLocalPath:string;
    sLoginSvr,//sLoginSvr:string;
    sLoginDB,//sLoginDB:string;
    //true, //bLocalTemCopy:boolean
    //6
    '',//sOCXTemplate,
    0, //iDtlItem
    '',//sTranGlobalId
    sTempBasJSISpw //2012.06.01 add for SS Bill-20120531-01
    );

end;

procedure TfrmFMEdLessMatInq.btnTempBasDLLDoClick(Sender: TObject);
begin
  inherited;
  prcClearChildDLL;

end;

procedure TfrmFMEdLessMatInq.pageBrowseChange(Sender: TObject);
begin
  inherited;
  prcClearChildDLL;
end;

procedure TfrmFMEdLessMatInq.qryBrowseAfterClose(DataSet: TDataSet);
begin
  inherited;
  pnlCount.Caption:='0 / 0';
end;

procedure TfrmFMEdLessMatInq.qryBrowseAfterOpen(DataSet: TDataSet);
begin
  inherited;
  with qryDisplace do begin close; open; end;
end;

procedure TfrmFMEdLessMatInq.qryBrowseAfterScroll(DataSet: TDataSet);
var i:integer; pnl:TPanel;
begin
  inherited;
  prcClearChildDLL;
  with qryDisplace do begin close; open; end;

  i:=DataSet.RecNo;
  if i<0 then i:=0;
    pnlCount.Caption:=inttostr(i)+' / '+ inttostr(DataSet.RecordCount);
end;

procedure TfrmFMEdLessMatInq.qryBrowseBeforeClose(DataSet: TDataSet);
begin
  inherited;
  qryDisplace.Close;
end;

procedure TfrmFMEdLessMatInq.qryDisplaceAfterScroll(DataSet: TDataSet);
begin
  inherited;
  prcClearChildDLL;
end;

procedure TfrmFMEdLessMatInq.qryDisplaceBeforeOpen(DataSet: TDataSet);
begin
  inherited;
  qryDisplace.Parameters.ParamByName('PartNum').Value:=
    qryBrowse.FieldByName('PartNum').AsString;
end;

//2019.05.08
procedure TfrmFMEdLessMatInq.cbo_MBChange(Sender: TObject);
begin
  inherited;
  with qryMINdMatClass do
  begin
    qryMINdMatClass.Close;
    SQL.Clear;
    if ((cbo_MB.Text='0') or (cbo_MB.Text='1')) then
    begin
      SQL.Add('select MatClass,ClassName from MINdMatClass(nolock) '
        +'where MB='+cbo_MB.Text+' order by MatClass');
    end
    else
    begin
      SQL.Add('select MatClass,ClassName from MINdMatClass(nolock) '
        +'order by MatClass');
    end;
    Open;
  end;
end;

procedure TfrmFMEdLessMatInq.btnToExcelClick(Sender: TObject);
begin
  inherited;
  DsExport(self,qryBrowse);
end;

end.

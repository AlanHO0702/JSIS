unit LessMatInq2;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic, DB, ADODB, Buttons, ExtCtrls, StdCtrls,
  wwdbdatetimepicker, JSdLookupCombo, JSdLabel, Grids, Wwdbigrd, Wwdbgrid,
  JSdDBGrid, ComCtrls, JSdTable ,UCrpeUtl;

type
  TfrmFMEdLessMatInq2 = class(TfrmTempPublic)
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
    btnToExcel: TSpeedButton;
    JSdLabel2: TJSdLabel;
    edtMatName: TEdit;
    JSdLabel3: TJSdLabel;
    cbo_POType: TJSdLookupCombo;
    qryFMEdPOType: TJSdTable;
    dsFMEdPOType: TDataSource;
    edtNPLratio: TEdit;
    JSdLabel6: TJSdLabel;
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
    procedure btnToExcelClick(Sender: TObject);
  private
    procedure prcClearChildDLL;
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmFMEdLessMatInq2: TfrmFMEdLessMatInq2;
  hCall:THandle;
implementation

uses unit_DLL, commParent;

{$R *.dfm}

procedure TfrmFMEdLessMatInq2.btnGetParamsClick(Sender: TObject);
var sMatClass:string;  //2020.06.30
begin
  inherited;
  qryMINdMatInfo.Open;
  qryMINdMatClass.Open;
  qryMB.Open;

//不能設在屬性表，因在FormCreate就會自動連線
  cbo_MatClass.LkDataSource:=dsMINdMatClass;
  cbo_POType.LkDataSource:=dsFMEdPOType;
  cbo_PartNum.LkDataSource:=dsMINdMatInfo;
  cbo_MB.LkDataSource:=dsMB;

  cbo_MB.text:='255';
  cbo_POType.text:='255';

  hCall:=0;

  pageBrowse.ActivePageIndex:=0;

  //----------2015.11.18 add for Bill-20151112-03
  prcStoreFieldNeed_Def(self,qryExec);
  //----------

  qryBrowse.TableName:='FMEdLessMatInq2';
  qryBrowse.LookupType:=lkLookupTable;

  //2020.06.30
  //----------------------------------------------------
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select MatClass from MINdMatClass where NeedInStock = 1 and MB = 0');
    Open;
    sMatClass:=FieldByName('MatClass').AsString;
  end;
  cbo_MatClass.Text := sMatClass;
  //----------------------------------------------------

end;

procedure TfrmFMEdLessMatInq2.prcClearChildDLL;
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

procedure TfrmFMEdLessMatInq2.btnInqClick(Sender: TObject);
var iMB:integer;
    sDate:String;
begin
  inherited;

  //2019.02.22
  if not IsNumeric(edtNPLratio.Text) then
   begin
     MsgDlgJS('不良率請輸入數字',mtError,[mbOk],0);
     exit;
   end;


  prcClearChildDLL;

  if cbo_MB.text='' then iMB:=255
  else if not((cbo_MB.text='0') or (cbo_MB.text='1') or (cbo_MB.text='255')) then iMB:=255
  else iMB:=strtoint(cbo_MB.Text);

  if ww_InqDate.Text='' then
    sDate:='1900/01/01'
  else
    sDate:=ww_InqDate.Text;



  with qryBrowse do
    begin
      close;
      SQL.Clear;
      SQL.Add('exec MINdLookMatInfoDtl_MUT ''' + cbo_PartNum.Text + ''','
        +'4,''' + sDate + ''',''' + cbo_MatClass.Text + ''',' + IntToStr(iMB)
        +',''' + sUseId + ''',0,'''','''',1,''' + edtMatName.Text + ''',0,'''',0,0,'''','''',''' + cbo_POType.Text + ''',''' + edtNPLratio.Text + '''');
      open;
    end;
end;

procedure TfrmFMEdLessMatInq2.btnMatDetailClick(Sender: TObject);
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
    frmFMEdLessMatInq2.pnlInqMatLessShowDLL,//tOtherParent:TWinControl;
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

procedure TfrmFMEdLessMatInq2.btnTempBasDLLDoClick(Sender: TObject);
begin
  inherited;
  prcClearChildDLL;

end;

procedure TfrmFMEdLessMatInq2.pageBrowseChange(Sender: TObject);
begin
  inherited;
  prcClearChildDLL;
end;

procedure TfrmFMEdLessMatInq2.qryBrowseAfterOpen(DataSet: TDataSet);
begin
  inherited;
  with qryDisplace do begin close; open; end;
end;

procedure TfrmFMEdLessMatInq2.qryBrowseAfterScroll(DataSet: TDataSet);
var i:integer; pnl:TPanel;
begin
  inherited;
  prcClearChildDLL;
  with qryDisplace do begin close; open; end;
end;

procedure TfrmFMEdLessMatInq2.qryBrowseBeforeClose(DataSet: TDataSet);
begin
  inherited;
  qryDisplace.Close;
end;

procedure TfrmFMEdLessMatInq2.qryDisplaceAfterScroll(DataSet: TDataSet);
begin
  inherited;
  prcClearChildDLL;
end;

procedure TfrmFMEdLessMatInq2.qryDisplaceBeforeOpen(DataSet: TDataSet);
begin
  inherited;
  qryDisplace.Parameters.ParamByName('PartNum').Value:=
    qryBrowse.FieldByName('PartNum').AsString;
end;

procedure TfrmFMEdLessMatInq2.btnToExcelClick(Sender: TObject);
begin
  inherited;
  DsExport(self,qryBrowse);
end;

end.

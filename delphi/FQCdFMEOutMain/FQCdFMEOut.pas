unit FQCdFMEOut;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, PaperOrgDLL, JSdGrid2Excel, Menus, JSdPopupMenu, DB, JSdTable, ADODB,
  StdCtrls, Mask, DBCtrls, JSdLabel, ExtCtrls, Grids, Wwdbigrd, Wwdbgrid,
  JSdDBGrid, ComCtrls, Buttons;

type
  TfrmFQCdFMEOutMain = class(TfrmPaperOrgDLL)
    Splitter2: TSplitter;
    qryFQCdFMEOutItem: TJSdTable;
    dsFQCdFMEOutItem: TDataSource;
    qryFQCdFMEOutItemPaperNum: TStringField;
    qryFQCdFMEOutItemSubItem: TIntegerField;
    qryFQCdFMEOutItemQCTypeItem: TIntegerField;
    qryFQCdFMEOutItemTestItem: TStringField;
    qryFQCdFMEOutItemStdValue1: TWideStringField;
    qryFQCdFMEOutItemGetValue1: TWideStringField;
    qryFQCdFMEOutItemStdValue2: TWideStringField;
    qryFQCdFMEOutItemGetValue2: TWideStringField;
    qryFQCdFMEOutItemQCResult: TStringField;
    qryFQCdFMEOutItemQCCode: TStringField;
    qryFQCdFMEOutItemNotes: TWideStringField;
    qryFQCdFMEOutItemQCStatus: TIntegerField;
    Page3: TPageControl;
    TabSheet1: TTabSheet;
    TabSheet2: TTabSheet;
    qryFQCdFMEOutResult: TJSdTable;
    qryFQCdFMEOutResultPaperNum: TStringField;
    qryFQCdFMEOutResultSubItem: TIntegerField;
    qryFQCdFMEOutResultQCResult: TStringField;
    qryFQCdFMEOutResultQCCode: TStringField;
    StringField9: TWideStringField;
    qryFQCdFMEOutResultQCStatus: TIntegerField;
    dsFQCdFMEOutResult: TDataSource;
    JSdDBGrid1: TJSdDBGrid;
    qryFQCdFMEOutResultItem: TIntegerField;
    qryFQCdFMEOutResultQCReference: TStringField;
    qryFQCdFMEOutResultToStockId: TStringField;
    qryFQCdQCStatus: TADOQuery;
    qryFQCdQCStatusQCStatus: TIntegerField;
    qryFQCdQCStatusQCStatusName: TWideStringField;
    qryMINdStockBasic: TADOQuery;
    qryMINdStockBasicStockId: TStringField;
    qryMINdStockBasicStockName: TWideStringField;
    qryMINdStockBasicStockType: TIntegerField;
    qryMINdStockBasicIsOut: TIntegerField;
    qryMINdStockBasicIsMust: TIntegerField;
    qryMINdStockBasicNotes: TWideStringField;
    qryMINdStockBasicPOType: TIntegerField;
    qryMINdStockBasicUseId: TStringField;
    qryMINdStockBasicFromCus: TIntegerField;
    qryFQCdFMEOutResultLk_QCStatusName: TWideStringField;
    qryFQCdFMEOutResultLk_ToStockName: TWideStringField;
    qryFQCdQCCode: TADOQuery;
    qryFQCdQCCodeQCCode: TStringField;
    qryFQCdQCCodeComments: TWideStringField;
    qryFQCdQCCodeQCStatus: TIntegerField;
    qryFQCdQCCodeStockId: TStringField;
    qryFQCdFMEOutResultLk_QCCodeComments: TWideStringField;
    qryFQCdQCResult: TADOQuery;
    qryFQCdQCResultQCResult: TStringField;
    qryFQCdQCResultNotes: TWideStringField;
    qryFQCdFMEOutResultLk_QCResultNotes: TWideStringField;
    qryFQCdFMEOutResultQCQnty: TFloatField;
    qryFQCdFMEOutItemQCQnty: TFloatField;
    qryFQCdFMEOutItemSampleQnty: TFloatField;
    qryFQCdFMEOutResultPosId: TStringField;
    qryFQCdFMEOutItemCheck1: TIntegerField;
    qryFQCdFMEOutItemCheck2: TIntegerField;
    qryFQCdFMEOutItemOperation1: TStringField;
    qryFQCdFMEOutItemOperation2: TStringField;
    qryFQCdFMEOutResultUOMQnty: TFloatField;
    qryFQCdFMEOutResultRatio: TFloatField;
    qryFQCdFMEOutResultUnit: TStringField;
    qryFQCdFMEOutResultUOM: TStringField;
    qryFQCdFMEOutResultBatchNum: TStringField;
    qryFQCdFMEOutResultVolumeNum: TStringField;
    qryFQCdFMEOutResultExpiredDate: TDateTimeField;
    qryFQCdFMEOutResultSizeLenth: TStringField;
    qryFQCdFMEOutResultSizeWidth: TStringField;
    qryFQCdFMEOutItemAQL: TStringField;
    qryFQCdAQL: TADOQuery;
    qryFQCdAQLAQL: TStringField;
    qryFQCdAQLAQLName: TWideStringField;
    qryFQCdFMEOutItemLk_AQLName: TWideStringField;
    qryFQCdFMEOutResultUseId: TStringField;
    TabSheet3: TTabSheet;
    dsMatInfoDtl: TDataSource;
    qryMatInfoDtl: TJSdTable;
    gridMatInfoDtl: TJSdDBGrid;
    Splitter3: TSplitter;
    qryQCTypeSubDtl: TJSdDBGrid;
    qryFQCdQCTypeSubDtl: TJSdTable;
    qryFQCdQCTypeSubDtlQCType: TStringField;
    qryFQCdQCTypeSubDtlItem: TIntegerField;
    qryFQCdQCTypeSubDtlSerialNum: TIntegerField;
    qryFQCdQCTypeSubDtlCheck1: TIntegerField;
    qryFQCdQCTypeSubDtlCheck2: TIntegerField;
    qryFQCdQCTypeSubDtlOperation1: TStringField;
    qryFQCdQCTypeSubDtlStdValue1: TStringField;
    qryFQCdQCTypeSubDtlOperation2: TStringField;
    qryFQCdQCTypeSubDtlStdValue2: TStringField;
    qryFQCdQCTypeSubDtlNOtes: TStringField;
    dsFQCdQCTypeSubDtl: TDataSource;
    qryFQCdFMEOutItemQCType: TStringField;
    qryFQCdTestItem: TADOQuery;
    qryFQCdFMEOutItemLk_TestItemName: TWideStringField;
    btnCopySpec: TSpeedButton;
    qryFQCdFMEOutResultQCType: TStringField;
    btnEditResu: TSpeedButton;
    gridFQCdFMEOutItem: TJSdDBGrid;
    qryFQCdFMEOutItemAC: TFloatField;
    qryFQCdFMEOutItemRE: TFloatField;
    procedure btnGetParamsClick(Sender: TObject);
    procedure gridFQCdFMEOutItemEnter(Sender: TObject);
    procedure gridFQCdFMEOutItemMouseDown(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure gridDetail1Enter(Sender: TObject);
    procedure gridDetail1MouseDown(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure JSdDBGrid1Enter(Sender: TObject);
    procedure JSdDBGrid1MouseDown(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure qryFQCdFMEOutResultBeforeInsert(DataSet: TDataSet);
    procedure qryFQCdFMEOutResultNewRecord(DataSet: TDataSet);
    procedure qryFQCdFMEOutResultQCCodeValidate(Sender: TField);
    procedure qryFQCdFMEOutResultBeforePost(DataSet: TDataSet);
    procedure qryFQCdFMEOutResultBeforeEdit(DataSet: TDataSet);
    procedure qryFQCdFMEOutResultBeforeDelete(DataSet: TDataSet);
    procedure qryFQCdFMEOutItemBeforeEdit(DataSet: TDataSet);
    procedure qryFQCdFMEOutItemBeforeDelete(DataSet: TDataSet);
    procedure qryFQCdFMEOutItemBeforeInsert(DataSet: TDataSet);
    procedure qryFQCdFMEOutResultAfterPost(DataSet: TDataSet);
    procedure qryDetail1AfterOpen(DataSet: TDataSet);
    procedure btnSaveHeightClick(Sender: TObject);
    procedure btnCopySpecClick(Sender: TObject);
    procedure btnEditResuClick(Sender: TObject);
    procedure pnlTempBasDLLBottomDblClick(Sender: TObject);
    procedure qryFQCdFMEOutItemAfterPost(DataSet: TDataSet);
  private
    function funChkResultCanAftEdit: boolean;

    { Private declarations }
  public
    { Public declarations }
    bQCChkResult2Qnty:boolean;
    bQCChkResultEditIQC:boolean;//2010.11.3 add for YX Bill-20101101-2
    bQCColseAllowEdit:boolean;//2011.4.13 add for YX Bill-20110408-1
    bIQCbyStandard:boolean;//2012.06.19 add for Bill-20120616-01
    bIQCAddforOK:boolean;//2012.06.19 add for Bill-20120616-01
  end;

var
  frmFQCdFMEOutMain: TfrmFQCdFMEOutMain;

implementation

uses unit_DLL, InputData;

{$R *.dfm}

procedure TfrmFQCdFMEOutMain.btnCopySpecClick(Sender: TObject);
begin
  inherited;

  //2010.6.30 disable for YX RA10062315-3

  {if funCheckPaper4EngDesign(
    qryBrowse,//tblTable:TJSdTable;
    CanbUpdate,//CanbUpdate:integer;
    CanbLockUserEdit,//CanbLockUserEdit:integer;
    sUserId,//sUserId:string;
    sNowMode//sNowMode:string
    )=false then
    begin
      exit;
    end;}

  with qryFQCdFMEOutItem do if (state in [dsEdit, dsInsert]) then post;

   if qryFQCdFMEOutItem.RecordCount=0 then
     begin
       MsgDlgJS('沒有「QC項目」', mtError, [mbOk], 0);
       exit;
     end;

  if qryFQCdQCTypeSubDtl.RecordCount=0 then
     begin
       MsgDlgJS('沒有「檢驗規格」', mtError, [mbOk], 0);
       exit;
     end;

  with qryExec do
    begin
      if active then close;
      sql.Clear;
      sql.Add('exec FQCdFMEOutCopySpec '+
        ''''+qryFQCdFMEOutItem.FieldByName('PaperNum').AsString+''''+','+
        qryFQCdFMEOutItem.FieldByName('SubItem').AsString+','+
        qryFQCdFMEOutItem.FieldByName('QCTypeItem').AsString+','+
        ''''+qryFQCdFMEOutItem.FieldByName('QCType').AsString+''''+','+
        qryFQCdQCTypeSubDtl.FieldByName('SerialNum').AsString
        );
      ExecSQL;
      close;
    end;

  qryFQCdFMEOutItem.Refresh;
end;

procedure TfrmFQCdFMEOutMain.btnEditResuClick(Sender: TObject);
begin
  if CanbUpdate=0 then
  	begin
     		MsgDlgJS('您沒有「編修」的權限', mtError, [mbOk], 0);
     		abort;
  	end;

  if sNowMode<>'UPDATE' then
    if funChkResultCanAftEdit=false then abort;
end;

procedure TfrmFQCdFMEOutMain.btnGetParamsClick(Sender: TObject);
begin
  inherited;

  qryMINdStockBasic.Open;
  qryFQCdQCStatus.Open;
  qryFQCdQCResult.Open;
  qryFQCdQCCode.Open;
  qryFQCdAQL.Open;
  qryFQCdTestItem.Open;

  qryFQCdFMEOutResult.Close;
  qryFQCdFMEOutResult.TableName:='FQCdFMEOutResult'+inttostr(PowerType);
  qryFQCdFMEOutResult.DataSource:=dsDetail1;
  if qryDetail1.Active then qryFQCdFMEOutResult.Open;

  qryFQCdFMEOutItem.Close;
  qryFQCdFMEOutItem.TableName:='FQCdFMEOutItem'+inttostr(PowerType);
  qryFQCdFMEOutItem.DataSource:=dsDetail1;
  if qryDetail1.Active then qryFQCdFMEOutItem.Open;

  if qryDetail1.SQL.Text<>'' then
    begin
      qryMatInfoDtl.SQL.Clear;
      qryMatInfoDtl.SQL.Add(
      'Select * from dbo.MGNdVMatInfoDtl(nolock) where PartNum = :PartNum');
      qryMatInfoDtl.TableName:='FQC_MGNdMatInfoDtl';
      qryMatInfoDtl.DataSource:=dsDetail1;

      if qryDetail1.Active then qryMatInfoDtl.Open;
    end;

  qryFQCdQCTypeSubDtl.close;
  qryFQCdQCTypeSubDtl.DataSource:=dsFQCdFMEOutItem;
  if qryFQCdFMEOutItem.Active then qryFQCdQCTypeSubDtl.Open;

  Page3.ActivePageIndex:=0;

  bQCChkResult2Qnty:=unit_DLL.funDLLSysParamsGet(qryExec,'FQC','QCChkResult2Qnty')='1';

  //2010.11.3 add for YX Bill-20101101-2
  bQCChkResultEditIQC:=unit_DLL.funDLLSysParamsGet(qryExec,'FQC','QCChkResultEditIQC')='1';

  //2011.4.13 add for YX Bill-20110408-1
  bQCColseAllowEdit:=unit_DLL.funDLLSysParamsGet(qryExec,'FQC','QCColseAllowEdit')='1';

  //2012.06.19 add for Bill-20120616-01
  bIQCbyStandard:=unit_DLL.funDLLSysParamsGet(qryExec,'FQC','IQCbyStandard')='1';

  bIQCAddforOK:=unit_DLL.funDLLSysParamsGet(qryExec,'FQC','IQCAddforOK')='1';

  unit_DLL.prcGrdHeightSet(gridDetail1,sItemId,'LOAD',qryExec);

  unit_DLL.prcStoreFieldNeed_Def(self,qryExec);

  //2019.08.05 Fix 非必要的輔助功能，先關閉
  qryQCTypeSubDtl.Visible := False;
  Splitter3.Visible := False;
  gridFQCdFMEOutItem.Align := alClient;
end;


procedure TfrmFQCdFMEOutMain.btnSaveHeightClick(Sender: TObject);
begin
  inherited;
  unit_DLL.prcGrdHeightSet(gridDetail1,sItemId,'SAVE',qryExec);
end;

procedure TfrmFQCdFMEOutMain.gridDetail1Enter(Sender: TObject);
begin
  inherited;
  if sNowMode='UPDATE' then nav1.DataSource:=TJSdDBGrid(Sender).DataSource;
  nav2.DataSource:=TJSdDBGrid(Sender).DataSource;
end;

procedure TfrmFQCdFMEOutMain.gridDetail1MouseDown(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
begin
  inherited;
  if sNowMode='UPDATE' then nav1.DataSource:=TJSdDBGrid(Sender).DataSource;
  nav2.DataSource:=TJSdDBGrid(Sender).DataSource;
end;

procedure TfrmFQCdFMEOutMain.gridFQCdFMEOutItemEnter(Sender: TObject);
begin
  inherited;
  if sNowMode='UPDATE' then nav1.DataSource:=TJSdDBGrid(Sender).DataSource;
  nav2.DataSource:=TJSdDBGrid(Sender).DataSource;
end;

procedure TfrmFQCdFMEOutMain.gridFQCdFMEOutItemMouseDown(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
begin
  inherited;
  if sNowMode='UPDATE' then nav1.DataSource:=TJSdDBGrid(Sender).DataSource;
  nav2.DataSource:=TJSdDBGrid(Sender).DataSource;
end;

procedure TfrmFQCdFMEOutMain.JSdDBGrid1Enter(Sender: TObject);
begin
  inherited;
  if sNowMode='UPDATE' then
    nav1.DataSource:=TJSdDBGrid(Sender).DataSource;

  nav2.DataSource:=TJSdDBGrid(Sender).DataSource;
end;

procedure TfrmFQCdFMEOutMain.JSdDBGrid1MouseDown(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
begin
  inherited;
  if sNowMode='UPDATE' then
    nav1.DataSource:=TJSdDBGrid(Sender).DataSource;

  nav2.DataSource:=TJSdDBGrid(Sender).DataSource;
end;

procedure TfrmFQCdFMEOutMain.pnlTempBasDLLBottomDblClick(Sender: TObject);
var sTestPW:string;
begin
  inherited;
  if sUserId='Admin' then
  begin
     Application.CreateForm(TdlgInputData,dlgInputData);
     dlgInputData.ShowModal;
     sTestPW:=dlgInputData.Edit1.Text;

     if sTestPW='js987' then
        showmessage(qryBrowse.ConnectionString);
  end;
end;

procedure TfrmFQCdFMEOutMain.qryDetail1AfterOpen(DataSet: TDataSet);
begin
  inherited;
  if qryFQCdFMEOutResult.DataSource=dsDetail1 then
    with qryFQCdFMEOutResult do begin close; open; end;

  if qryFQCdFMEOutItem.DataSource=dsDetail1 then
    with qryFQCdFMEOutItem do begin close; open; end;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutItemBeforeDelete(DataSet: TDataSet);
begin
  //inherited;
  abort;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutItemBeforeEdit(DataSet: TDataSet);
begin
  inherited;
  //if sNowMode<>'UPDATE' then abort; //2010.6.30 disable for YX RA10062315-3

  if qryBrowse.Active=false then abort;

  if qryBrowse.FieldByName('Finished').AsInteger=2 then
  	begin
     		MsgDlgJS('單據「已作廢」不可再異動', mtError, [mbOk], 0);
     		abort;
  	end;

  //2011.4.13 add for YX Bill-20110408-1
  if qryBrowse.FieldByName('Finished').AsInteger=4 then
    if bQCColseAllowEdit=false then
  	  begin
     		MsgDlgJS('單據「已結案」不可再異動', mtError, [mbOk], 0);
     		abort;
  	  end;

  if CanbUpdate=0 then
  	begin
     		MsgDlgJS('您沒有「編修」的權限', mtError, [mbOk], 0);
     		abort;
  	end;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutItemBeforeInsert(DataSet: TDataSet);
begin
  //inherited;
  abort;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutResultAfterPost(DataSet: TDataSet);
var bk2,bk3:Tbookmark;
begin
  bk2:=qryDetail1.GetBookmark;
  bk3:=DataSet.GetBookmark;

  DataSet.Refresh;

  if qryDetail1.LockType<>ltReadOnly then qryDetail1.refresh;

  qryDetail1.GotoBookmark(bk2);
  qryDetail1.FreeBookmark(bk2);

  DataSet.GotoBookmark(bk3);
  DataSet.FreeBookmark(bk3);
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutResultBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  if sNowMode<>'UPDATE' then
    if funChkResultCanAftEdit=false then abort;

  if CanbUpdate=0 then
  	begin
     		MsgDlgJS('您沒有「編修」的權限', mtError, [mbOk], 0);
     		abort;
  	end;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutResultBeforeEdit(DataSet: TDataSet);
begin
  inherited;
  if sNowMode<>'UPDATE' then
    if funChkResultCanAftEdit=false then abort;

  if qryDetail1.Active=false then  abort;
  if qryDetail1.State in[dsInsert,dsEdit] then  qryDetail1.Post;

  if CanbUpdate=0 then
  	begin
     		MsgDlgJS('您沒有「編修」的權限', mtError, [mbOk], 0);
     		abort;
  	end;
end;

function TfrmFQCdFMEOutMain.funChkResultCanAftEdit:boolean;
var bAllow:boolean;//2010.11.3 add for YX Bill-20101101-2
begin
  result:=false;

  //2011.4.13 add for YX Bill-20110408-1
  if (qryBrowse.FieldByName('Finished').AsInteger=2) then
       begin
         MsgDlgJS('「已作廢」的單據，不可修改', mtError, [mbOk], 0);
         exit;
       end;

  //2011.4.13 add for YX Bill-20110408-1
  if (qryBrowse.FieldByName('Finished').AsInteger in[0,3]) then
       begin
         MsgDlgJS('「作業中」或是「審核中」的單據可直接修改，不須做「事後修改」', mtError, [mbOk], 0);
         exit;
       end;

  bAllow:=false;

  if (bQCChkResult2Qnty=false) and (PowerType=2) then bAllow:=true;

  //2010.11.3 add for YX Bill-20101101-2
  if bAllow=false then
     if (bQCChkResult2Qnty=false)
        and (bQCChkResultEditIQC)
        and (PowerType=1)
        then
        bAllow:=true;

  //2011.4.13 add for YX Bill-20110408-1
  if bAllow then
    if (qryBrowse.FieldByName('Finished').AsInteger=4) and (bQCColseAllowEdit=false) then
       begin
         MsgDlgJS('單據「已結案」，不可修改', mtError, [mbOk], 0);
         exit;
       end;

  if bAllow then
    //2011.4.13 disable for YX Bill-20110408-1
    //if qryBrowse.FieldByName('Finished').AsInteger<>1 then
    //   begin
    //     MsgDlgJS('只有「已確認」的單據才可做事後修改', mtError, [mbOk], 0);
    //   end
    //   else
       begin
         nav2.Enabled:=true;
         nav2.DataSource:=dsFQCdFMEOutResult;
         result:=true;
       end;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutResultBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  if sNowMode<>'UPDATE' then
    if funChkResultCanAftEdit=false then abort;

  if qryDetail1.Active=false then  abort;
  if qryDetail1.State in[dsInsert,dsEdit] then  qryDetail1.Post;
  if qryDetail1.RecordCount=0 then abort;

//2010.11.3 add 'if...' for YX Bill-20101101-2
if not((bQCChkResult2Qnty=false) and (bQCChkResultEditIQC) and (PowerType=1)) then
begin
  if not (bIQCAddforOK) then
  begin
    if qryDetail1.FieldByName('QCStatus').IsNull=false then //因 AsInteger 會讓null視同0,故須先判斷
      if qryDetail1.FieldByName('QCStatus').AsInteger=0 then
        begin
          MsgDlgJS('明細檔的QC狀態是「合格」，不須輸入QC結果',mtError,[mbOk],0);
          abort;
        end;
  end;
end;

  if CanbUpdate=0 then
  	begin
     		MsgDlgJS('您沒有「編修」的權限', mtError, [mbOk], 0);
     		abort;
  	end;

  DataSet.Tag:=GetMaxSerialNumDLL(DataSet, 'Item')+1;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutResultBeforePost(DataSet: TDataSet);
begin
  inherited;
  if trim(DataSet.FieldByName('QCCode').AsString)='' then
    begin
      MsgDlgJS('必須輸入QC代碼',mtError,[mbOk],0);
      abort;
    end;

  if qryFQCdQCCode.Locate(
      'QCCode',
      DataSet.FieldByName('QCCode').AsString,
      [loCaseInsensitive])=false then
    begin
      MsgDlgJS('輸入的QC代碼有誤',mtError,[mbOk],0);
      abort;
    end;

  if DataSet.FieldByName('QCStatus').Value=null then
    begin
      MsgDlgJS('自動更新QC狀態失敗',mtError,[mbOk],0);
      abort;
    end;

  {if DataSet.FieldByName('QCQnty').AsFloat<=0 then
    begin
      MsgDlgJS('品檢數量必須大於 0',mtError,[mbOk],0);
      abort;
    end;}
if PowerType in[0,1] then
begin
  if (DataSet.FieldByName('QCStatus').asinteger<>1) and
     (bIQCbyStandard=false) //2012.06.19 add for Bill-20120616-01
  then
  begin

    if trim(DataSet.FieldByName('ToStockId').AsString)='' then
      begin
        MsgDlgJS('必須輸入倉別',mtError,[mbOk],0);
        abort;
      end;

    if qryMINdStockBasic.Locate(
      'StockId',
      DataSet.FieldByName('ToStockId').AsString,
      [loCaseInsensitive])=false then
      begin
        MsgDlgJS('輸入的倉別有誤',mtError,[mbOk],0);
        abort;
      end;

    if trim(DataSet.FieldByName('PosId').AsString)='' then
      begin
        MsgDlgJS('必須輸入庫位',mtError,[mbOk],0);
        abort;
      end;

   end;//if (DataSet.FieldByName('QCStatus').asinteger<>1) and (bIQCbyStandard=false) then
end; //if PowerType in[0,1] then

  if DataSet.FieldByName('Ratio').AsFloat<=0 then //2010.7.19 add
     DataSet.FieldByName('Ratio').AsFloat:=1;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutResultNewRecord(DataSet: TDataSet);
var bReadOnly:boolean;
begin
  inherited;
  DataSet.FieldByName('Item').AsInteger:= DataSet.Tag;

  with DataSet do
    begin
      FieldByName('PaperNum').AsString
        :=qryDetail1.FieldByName('PaperNum').AsString;
      FieldByName('SubItem').Value
        :=qryDetail1.FieldByName('Item').Value;
      FieldByName('PosId').AsString
        :=qryDetail1.FieldByName('PosId').AsString;
      FieldByName('Ratio').Value
        :=qryDetail1.FieldByName('Ratio').Value;

      FieldByName('UseId').AsString  //2010.5.3 add
        :=qryBrowse.FieldByName('UseId').AsString;
    end;

  bReadOnly:=DataSet.FieldByName('Unit').ReadOnly;
  if bReadOnly then DataSet.FieldByName('Unit').ReadOnly:=false;
  DataSet.FieldByName('Unit').AsString
    :=DataSet.DataSource.DataSet.FieldByName('Unit').AsString;
  if bReadOnly then DataSet.FieldByName('Unit').ReadOnly:=true;

  bReadOnly:=DataSet.FieldByName('UOM').ReadOnly;
  if bReadOnly then DataSet.FieldByName('UOM').ReadOnly:=false;
  DataSet.FieldByName('UOM').AsString
    :=DataSet.DataSource.DataSet.FieldByName('UOM').AsString;
  if bReadOnly then DataSet.FieldByName('UOM').ReadOnly:=true;
  {
  DataSet.FieldByName('BatchNum').Value
    :=DataSet.DataSource.DataSet.FieldByName('BatchNum').Value;

  DataSet.FieldByName('VolumeNum').Value
    :=DataSet.DataSource.DataSet.FieldByName('VolumeNum').Value;

  DataSet.FieldByName('ExpiredDate').Value
    :=DataSet.DataSource.DataSet.FieldByName('ExpiredDate').Value;

  DataSet.FieldByName('SizeLenth').Value
    :=DataSet.DataSource.DataSet.FieldByName('SizeLenth').Value;

  DataSet.FieldByName('SizeWidth').Value
    :=DataSet.DataSource.DataSet.FieldByName('SizeWidth').Value;
  }

  if DataSet.FieldByName('Ratio').AsFloat<=0 then //2010.7.19 add
     DataSet.FieldByName('Ratio').AsFloat:=1;

  //2010.8.17 add for YX Bill-20100716-1
  bReadOnly:=DataSet.FieldByName('QCType').ReadOnly;
  if bReadOnly then DataSet.FieldByName('QCType').ReadOnly:=false;
  DataSet.FieldByName('QCType').AsString
    :=DataSet.DataSource.DataSet.FieldByName('QCType').AsString;
  if bReadOnly then DataSet.FieldByName('QCType').ReadOnly:=true;
end;

procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutResultQCCodeValidate(Sender: TField);
begin
  inherited;
  if qryFQCdQCCode.Locate('QCCode',Sender.AsString,[loCaseInsensitive]) then
    begin
      //qryFQCdFMEOutResult.FieldByName('QCStatus').readonly:=false;
      qryFQCdFMEOutResult.FieldByName('QCStatus').AsInteger
        :=qryFQCdQCCode.FieldByName('QCStatus').AsInteger;
      //qryFQCdFMEOutResult.FieldByName('QCStatus').readonly:=true;

      qryFQCdFMEOutResult.FieldByName('ToStockId').Asstring
        :=qryFQCdQCCode.FieldByName('StockId').Asstring;
    end;{
    else
    begin
      qryFQCdFMEOutResult.FieldByName('QCStatus').readonly:=false;
      qryFQCdFMEOutResult.FieldByName('QCStatus').value:=null;
      qryFQCdFMEOutResult.FieldByName('QCStatus').readonly:=true;

      qryFQCdFMEOutResult.FieldByName('ToStockId').value:=null;
    end;  }

end;

//2019.08.08 add for New Column
procedure TfrmFQCdFMEOutMain.qryFQCdFMEOutItemAfterPost(DataSet: TDataSet);
var bk:TBookMark;
begin
  inherited;
  bk:=qryFQCdFMEOutItem.GetBookmark;
  qryFQCdFMEOutItem.Close;
  qryFQCdFMEOutItem.Open;
  qryFQCdFMEOutItem.GotoBookmark(bk);
  qryFQCdFMEOutItem.FreeBookmark(bk);
end;

end.

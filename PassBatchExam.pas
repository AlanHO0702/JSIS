unit PassBatchExam;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic, DB, ADODB, StdCtrls, Buttons, ExtCtrls, Grids, Wwdbigrd,
  Wwdbgrid, JSdDBGrid, JSdMultSelect, JSdLabel, JSdLookupCombo, ComCtrls,
  JSdTable;

type
  TfrmPassBatchExam = class(TfrmTempPublic)
    pageMain: TPageControl;
    TabSheet1: TTabSheet;
    gridSub: TJSdDBGrid;
    msProcSelect: TJSdMultSelect;
    Panel1: TPanel;
    JSdLabel1: TJSdLabel;
    cboProcCode: TJSdLookupCombo;
    qryUserProc: TJSdTable;
    qryPassSub: TJSdTable;
    dsUserProc: TDataSource;
    dsPassSub: TDataSource;
    qryPassGet: TJSdTable;
    btFind: TSpeedButton;
    dsPassGet: TDataSource;
    qryExam: TJSdTable;
    JSdLabel2: TJSdLabel;
    JSdLabel3: TJSdLabel;
    cboLineId: TJSdLookupCombo;
    edtLotNum: TEdit;
    qryLineId: TJSdTable;
    dsLineId: TDataSource;
    btnRejectExam: TSpeedButton;
    btnExecute: TSpeedButton;
    procedure btnGetParamsClick(Sender: TObject);
    procedure btFindClick(Sender: TObject);
    procedure msProcSelectSourceClick(Sender: TObject);
    procedure msProcSelectTargetClick(Sender: TObject);
    procedure btnExecuteClick(Sender: TObject);
    procedure qryPassSubAfterOpen(DataSet: TDataSet);
    procedure msProcSelectAfterRemove(Sender: TObject);
    procedure msProcSelectAfterSelect(Sender: TObject);
    procedure btnRejectExamClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmPassBatchExam: TfrmPassBatchExam;

implementation

uses unit_DLL;

{$R *.dfm}

procedure TfrmPassBatchExam.btFindClick(Sender: TObject);
begin
  inherited;
  qryPassSub.Close;
  gridSub.Visible:=false;

  msProcSelect.TargetItems.Clear;

  with qryPassGet do
    begin
      close;
      Parameters.ParamByName('UserId').Value:=sUserId;
      Parameters.ParamByName('ProcCode').Value:=cboProcCode.Text;
      Parameters.ParamByName('LineId').Value:=cboLineId.Text; //2016.11.24 add for JH
      Parameters.ParamByName('LotNum').Value:=edtLotNum.Text; //2016.11.24 add for JH
      Open;
    end;

  msProcSelect.Setup(slSource);
end;

procedure TfrmPassBatchExam.btnExecuteClick(Sender: TObject);
var CurrPaperNum:string; i:integer;
begin
  inherited;

  if msProcSelect.TargetItems.Count=0 then
    begin
      MsgDlgJS('請選入過帳單', mtError, [mbOk], 0);
      exit;
    end;

  //2022.12.21 預過帳權限檢查
  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Add('exec CURdItemsCheck '+
    ''''+sUserId+''+''','''+'0''');
  qryExec.Open;

    if qryExec.RecordCount=0 then
      Begin
        MsgDlgJS('沒有「預過帳」送審的權限', mtError, [mbOk], 0);
        exit;
      End;

  qryExec.Close;

  qryPassSub.Close;
  gridSub.Visible:=false;

  for i:=msProcSelect.TargetItems.Count-1 downto 0 do
      begin
        CurrPaperNum:=msProcSelect.TargetItems.Item[i].SubItems[4];

        with qryExam do
          begin
            close;
            Parameters.ParamByName('PaperNum').Value:=CurrPaperNum;
            Parameters.ParamByName('UserId').Value:=sUserId;
            ExecSQL;
          end;

        msProcSelect.TargetItems.Item[i].Delete;
      end;

      edtLotNum.Text:='';

  MsgDlgJS('已完成', mtInformation, [mbOk], 0)
end;

procedure TfrmPassBatchExam.btnGetParamsClick(Sender: TObject);
begin
  inherited;

  with qryUserProc do
    begin
      close;
      Parameters.ParamByName('UserId').Value:=sUserId;
      open;
    end;

  qryLineId.Open;//2016.11.24 add

 msProcSelect.TargetColumns:=msProcSelect.SourceColumns;

  //2023.03.16  送審權限
  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Add('exec CURdItemsCheck '+
    ''''+sUserId+''+''','''+'0''');
  qryExec.Open;

    if qryExec.RecordCount=0 then
      Begin
        btnExecute.Enabled:=false;
      End;

  qryExec.Close;

  //2023.03.16  退審權限
  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Add('exec CURdItemsCheck '+
    ''''+sUserId+''+''','''+'1''');
  qryExec.Open;

    if qryExec.RecordCount=0 then
      Begin
        btnRejectExam.Enabled:=false;
      End;

  qryExec.Close;

end;

procedure TfrmPassBatchExam.btnRejectExamClick(Sender: TObject);
var sNowPassNum,sSQL:string; i:integer;
begin
  inherited;

  if msProcSelect.SourceItems.Count=0 then
    begin
      MsgDlgJS('沒有「預過帳」的資料', mtError, [mbOk], 0);
      exit;
    end;

  //2022.12.21 預過帳權限檢查
  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Add('exec CURdItemsCheck '+
    ''''+sUserId+''+''','''+'1''');
  qryExec.Open;

    if qryExec.RecordCount=0 then
      Begin
        MsgDlgJS('沒有「預過帳」退審的權限', mtError, [mbOk], 0);
        exit;
      End;

  qryExec.Close;

  sNowPassNum:='';

  for i := 0 to msProcSelect.SourceItems.Count - 1 do
    begin
      if msProcSelect.SourceItems.Item[i].Selected then
        begin
          sNowPassNum:=msProcSelect.SourceItems.Item[i].SubItems[4];
          break;
        end;
    end;

  sNowPassNum:=trim(sNowPassNum);

  if sNowPassNum='' then
    begin
      MsgDlgJS('請先點選要退審的「預過帳」單號', mtError, [mbOk], 0);
      exit;
    end
  else
    begin
      if MsgDlgJS('確定要將單號 '+sNowPassNum+' 退審嗎？',
        mtConfirmation, [mbYes,mbNo], 0)=mrNo then  exit;
    end;

  sSQL:='';
  sSQL:='exec CURdPaperAction '+
    ''''+'FMEdPassMain'+''''+','+
    ''''+sNowPassNum+''''+','+
    ''''+sUserId+''''+',0,2';

  unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);

  btFind.Click;
end;

procedure TfrmPassBatchExam.FormCreate(Sender: TObject);
begin
  inherited;
  qryPassSub.LookupType:=lkJoinSQL;//2012.08.16 add
end;

procedure TfrmPassBatchExam.msProcSelectAfterRemove(Sender: TObject);
begin
  inherited;
  gridSub.Visible:=false;
end;

procedure TfrmPassBatchExam.msProcSelectAfterSelect(Sender: TObject);
begin
  inherited;
  gridSub.Visible:=false;
end;

procedure TfrmPassBatchExam.msProcSelectSourceClick(Sender: TObject);
var CurrPaperNum:string; i:integer;
begin
  inherited;
  for i := 0 to msProcSelect.SourceItems.Count - 1 do
    if msProcSelect.SourceItems.Item[i].Selected then
      begin
        CurrPaperNum:=msProcSelect.SourceItems.Item[i].SubItems[4];
        break;
      end;

  with qryPassSub do
    begin
      close;
      Parameters.ParamByName('PaperNum').Value:=CurrPaperNum;
      open;
    end;
end;

procedure TfrmPassBatchExam.msProcSelectTargetClick(Sender: TObject);
var CurrPaperNum:string; i:integer;
begin
  inherited;
  for i := 0 to msProcSelect.TargetItems.Count - 1 do
    if msProcSelect.TargetItems.Item[i].Selected then
      begin
        CurrPaperNum:=msProcSelect.TargetItems.Item[i].SubItems[4];
        break;
      end;

  with qryPassSub do
    begin
      close;
      Parameters.ParamByName('PaperNum').Value:=CurrPaperNum;
      open;
    end;

end;

procedure TfrmPassBatchExam.qryPassSubAfterOpen(DataSet: TDataSet);
begin
  inherited;
  if not gridSub.Visible then gridSub.Visible:=true;
end;

end.

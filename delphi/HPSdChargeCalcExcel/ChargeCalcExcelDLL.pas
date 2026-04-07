unit ChargeCalcExcelDLL;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic, DB, ADODB, Buttons, ExtCtrls,JSdMultSelect,JSdLookupCombo,
  Wwdatsrc, JSdTable, StdCtrls, Grids, Wwdbigrd, Wwdbgrid, JSdDBGrid, ComCtrls,
  DBCtrls, ToolWin, TempDlgDLL;

type
  TfrmHPSdChargeCalcExcelDLL = class(TfrmTempPublic)
    btnOK: TBitBtn;
    procedure btnGetParamsClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmHPSdChargeCalcExcelDLL: TfrmHPSdChargeCalcExcelDLL;

implementation

Uses Unit_DLL;

{$R *.dfm}

procedure TfrmHPSdChargeCalcExcelDLL.btnGetParamsClick(Sender: TObject);
var SQLStmts:WideString;
    sSpName, sPaperNum:String;
    sParamList:array of WideString;
begin
  inherited;
  if qryGetTranData.Locate('SeqNum', 1, [loPartialKey]) then
  sSpName := qryGetTranData.Fields[1].AsString;
  if qryGetTranData.Locate('SeqNum', 2, [loPartialKey]) then
  sPaperNum := qryGetTranData.Fields[1].AsString;
  SetLength(sParamList,1);
  sParamList[0]:= sPaperNum;

  try
    qryExec:= TADOQuery.Create(nil);
    qryExec.CommandTimeout := 9600;
    qryExec.EnableBCD:=false;//2010.6.23 add for 解決報表轉出mdb時，decimal DataType 變成 Memo的問題

    SQLStmts:= Proc2QueryDLL(sSpName, sParamList,sConnectStr);
    Query2DataSetDLL(SQLStmts, qryExec,sConnectStr);

    DataSet2ExcelDLL(qryExec, '', sSpName, '', 0,sConnectStr);
  finally
    if assigned(qryExec) then qryExec.Free;
    self.Close;
  end;////產生檔案
end;

end.

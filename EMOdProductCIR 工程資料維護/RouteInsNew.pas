unit RouteInsNew;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, Grids, Wwdbigrd, Wwdbgrid,
  DBCtrls, JSdLabel, DB, Wwdatsrc, ADODB, JSdTable, JSdDBGrid, wwdblook, unit_DLL;

type
  TdlgRouteInsNew = class(TfrmTempDlgDLL)
    qryProcInfo: TADOQuery;
    qryProcInfoProcCode: TStringField;
    qryProcInfoProcName: TWideStringField;
    dsProcInfo: TwwDataSource;
    qryRoute: TJSdTable;
    dsRoute: TwwDataSource;
    qryPosChange: TADOQuery;
    qryReLoad: TADOQuery;
    qryCloseCheck: TADOQuery;
    qryClear: TADOQuery;
    Panel1: TPanel;
    Label3D12: TJSdLabel;
    btnSerialUp: TSpeedButton;
    btnSerialDown: TSpeedButton;
    DBNavigator1: TDBNavigator;
    edtLayer: TEdit;
    qryExec: TADOQuery;
    qryRoutePartNum: TStringField;
    qryRouteRevision: TStringField;
    qryRouteLayerId: TStringField;
    qryRouteSerialNum: TWordField;
    qryRouteProcCode: TStringField;
    qryRouteNotes: TWideStringField;
    qryRouteFinishRate: TFloatField;
    qryRouteIsNormal: TWideStringField;
    qryRouteDepartId: TStringField;
    qryRouteSpec: TWideStringField;
    qryRouteFilmNo: TWideStringField;
    qryRouteChangeNotes: TWideStringField;
    qryRoutePartSerial: TStringField;
    qryRouteProcSerial: TStringField;
    qryRouteSortType: TStringField;
    qryRouteBefSETime: TFloatField;
    qryRouteProcName: TWideStringField;
    cboProcCode: TwwDBLookupCombo;
    wwDBGrid1: TwwDBGrid;
    qryRouteSPId: TIntegerField;
    procedure btnCancelClick(Sender: TObject);
    procedure btnOKClick(Sender: TObject);
    procedure btnSerialDownClick(Sender: TObject);
    procedure btnSerialUpClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure qryRouteAfterDelete(DataSet: TDataSet);
    procedure qryRouteAfterInsert(DataSet: TDataSet);
    procedure qryRouteAfterPost(DataSet: TDataSet);
    procedure qryRouteBeforeDelete(DataSet: TDataSet);
    procedure qryRouteBeforeInsert(DataSet: TDataSet);
  private
    { Private declarations }
  public
    procedure GetData(sPartNum, sRevision, sLayerId :String);
    procedure PosChange(Direction :Integer);
    function GetMaxSerialNum(tblDset: TDataset; sFieldName: string):integer;
    var CurrPartNum, CurrRevision, sUpdateUser{2012.03.08} : String;
    var iSpId : Integer;
    { Public declarations }
  end;

var
  dlgRouteInsNew: TdlgRouteInsNew;

implementation

uses NewNameEdit;

{$R *.dfm}

procedure TdlgRouteInsNew.btnCancelClick(Sender: TObject);
begin
  inherited;
  Close;
end;

procedure TdlgRouteInsNew.btnOKClick(Sender: TObject);
var sTmpId, sSql, sNotes : String;
    iNeedIns : Integer;
begin
  inherited;
  sTmpId:='';
  iNeedIns:=0;
  with dsRoute.DataSet do
     if state in [dsEdit, dsInsert] then Post;
  with qryCloseCheck do
  begin
    close;
    Parameters.ParamByName('PartNum').Value:= CurrPartNum;
    Parameters.ParamByName('Revision').Value:= CurrRevision;
    Parameters.ParamByName('LayerId').Value:= trim(edtLayer.text);
    Parameters.ParamByName('SpId').Value:= iSpId;
    Open;
    if FieldByName('RouteId').AsString<>'' then
    begin
      sTmpId:=FieldByName('RouteId').AsString;
      iNeedIns:=0;
    end
    else
    begin
      Application.createForm(TdlgNewNameEdit, dlgNewNameEdit);
      with dlgNewNameEdit do
      begin
        Caption:='新設途程代碼';
        Label3.Caption:=trim(FieldByName('OldRouteId').AsString)+')';
        edtNotes.Text:=trim(FieldByName('Notes').AsString);
        Showmodal;
        if modalResult=mrok then
        begin
          sTmpId:=trim(Edit1.Text);
          iNeedIns:=1;
          sNotes:=trim(edtNotes.Text);
        end
      end;
    end;
    if sTmpId<>'' then
    begin
      sSql:='';
      sSql:='exec EMOdRouteChangeUpdate '''+CurrPartNum+''', '''
              +CurrRevision+''', '''+trim(edtLayer.text)+''', '''
              +sTmpId+''', '+ IntToStr(iNeedIns)+', '
              +IntToStr(iSpId)+',N'''+sNotes+'''';
      with qryExec do
      begin
        Close;
        SQL.Clear;
        SQL.Add(sSql);
      end;
      try
        qryExec.ExecSQL;
      except
      on E:Exception do MsgDlgJS(E.Message, mtWarning, [mbok], 0);
      end;
      {sErr:=SQLExecute(sSQL);
      if sErr<>'' then
      begin
        ShowMessage(sErr);
        abort;
      end;}

      //2012.03.08
      with qryExec do
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('exec EMOdUpdateDesigner '''
                +CurrPartNum+''','''
                +CurrRevision+''','''
                +sUpdateUser+'''');
        ExecSql;
      end;

      MsgDlgJS('變更儲存完畢。',mtInformation,[mbOk],0);
      Self.Close;
    end
    else
      abort;
  end;
end;

procedure TdlgRouteInsNew.btnSerialDownClick(Sender: TObject);
begin
  inherited;
  with dsRoute.DataSet do
     if state in [dsEdit, dsInsert] then Post;
  PosChange(1);
end;

procedure TdlgRouteInsNew.btnSerialUpClick(Sender: TObject);
begin
  inherited;
  with dsRoute.DataSet do
     if state in [dsEdit, dsInsert] then Post;
  PosChange(0);
end;

procedure TdlgRouteInsNew.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  inherited;
  with qryClear do
  begin
    Parameters.ParamByName('PartNum').Value:= CurrPartNum;
    Parameters.ParamByName('Revision').Value:= CurrRevision;
    Parameters.ParamByName('LayerId').Value:= trim(edtLayer.Text);
    Parameters.ParamByName('SpId').Value:= iSpId;
    ExecSql;
  end;
end;

procedure TdlgRouteInsNew.PosChange(Direction :Integer);
var iSerial : Integer;
begin
  iSerial := qryRoute.FieldByName('SerialNum').AsInteger;
  with qryPosChange do
  begin
    Close;
    Parameters.ParamByName('PartNum').Value:= CurrPartNum;
    Parameters.ParamByName('Revision').Value:= CurrRevision;
    Parameters.ParamByName('LayerId').Value:= trim(edtLayer.Text);
    Parameters.ParamByName('Pos').Value:=iSerial;
    Parameters.ParamByName('Direction').Value:=Direction;
    Parameters.ParamByName('SpId').Value:=iSpId;
    ExecSql;
  end;
  GetData(CurrPartNum, CurrRevision, trim(edtLayer.Text));
  if ((Direction=0) and (iSerial<>1)) then iSerial:=iSerial-1;
  if Direction=1 then iSerial:=iSerial+1;

  qryRoute.Locate('SerialNum' ,iSerial ,[loPartialKey])
end;

procedure TdlgRouteInsNew.qryRouteAfterDelete(DataSet: TDataSet);
begin
  inherited;
  with qryReLoad do
  begin
    Close;
    Parameters.ParamByName('PartNum').Value := CurrPartNum;
    Parameters.ParamByName('Revision').Value := CurrRevision;
    Parameters.ParamByName('LayerId').Value := trim(edtLayer.Text);
    Parameters.ParamByName('SerialNum').Value:=DataSet.Tag;
    ExecSql;
  end;
  GetData(CurrPartNum, CurrRevision, trim(edtLayer.Text));
end;

procedure TdlgRouteInsNew.qryRouteAfterInsert(DataSet: TDataSet);
begin
  inherited;
  DataSet.fieldbyname('SpId').Asinteger := iSpId;
  DataSet.fieldbyname('SerialNum').Asinteger := DataSet.Tag;
  DataSet.fieldbyname('PartNum').AsString := CurrPartNum;
  DataSet.fieldbyname('Revision').AsString := CurrRevision;
  DataSet.fieldbyname('LayerId').AsString := trim(edtLayer.Text);
  DataSet.fieldbyname('FinishRate').AsFloat := 1;
  DataSet.fieldbyname('IsNormal').AsString := '';
  DataSet.fieldbyname('SortType').AsString := 'FS';
  DataSet.fieldbyname('BefSETime').AsFloat := 0;
end;

procedure TdlgRouteInsNew.qryRouteAfterPost(DataSet: TDataSet);
begin
  inherited;
  dataset.Refresh;
end;

procedure TdlgRouteInsNew.qryRouteBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  DataSet.Tag:=qryRoute.FieldByName('SerialNum').AsInteger;
end;

procedure TdlgRouteInsNew.qryRouteBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  with dsRoute.DataSet do
    if state in [dsEdit, dsInsert] then Post;
  DataSet.Tag:=GetMaxSerialNum(DataSet, 'SerialNum')+1;
end;

procedure TdlgRouteInsNew.GetData(sPartNum, sRevision, sLayerId :String);
begin
  CurrPartNum := sPartNum;
  CurrRevision := sRevision;
  with qryRoute do
  begin
    close;
    Parameters.ParamByName('PartNum').Value:= sPartNum;
    Parameters.ParamByName('Revision').Value:= sRevision;
    Parameters.ParamByName('LayerId').Value:= sLayerId;
    Parameters.ParamByName('SpId').Value:= iSpId;
    Open;
  end;
  qryProcInfo.Close;
  qryProcInfo.Open;
end;

function TdlgRouteInsNew.GetMaxSerialNum(tblDset: TDataset; sFieldName: string):integer;
var Max: integer;
    bk: TBookmark;
begin
   Max:=0;
   with tblDset do
   begin
      DisableControls;
      bk:= GetBookmark;
      Open;
      First;
      while not eof do
      begin
         if fieldbyName(sFieldName).AsInteger > Max then
            Max:= fieldbyName(sFieldName).AsInteger;
         next;
      end;
      GotoBookmark(bk);
      FreeBookmark(bk);
      EnableControls;
   end;
   Result:= Max;
end;

end.

unit PressChange;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, Grids, Wwdbigrd, Wwdbgrid,
  DBCtrls, JSdLabel, Wwdatsrc, DB, ADODB, wwdblook, JSdTable, JSdDBGrid, unit_DLL;

type
  TdlgPressChange = class(TfrmTempDlgDLL)
    qryPosChange: TADOQuery;
    qryReLoad: TADOQuery;
    qryTmpPressMas: TJSdTable;
    qryTmpPressMasSerialNum: TWordField;
    qryTmpPressMasMatClass: TStringField;
    qryTmpPressMasClassName: TWideStringField;
    qryTmpPressMasNotes: TWideStringField;
    qryTmpPressMasPartNum: TStringField;
    qryTmpPressMasRevision: TStringField;
    qryTmpPressMasLayerId: TStringField;
    qryTmpPressMasBefLayer: TStringField;
    qryTmpPressMasSpId: TIntegerField;
    qryTmpPressMasIsIn: TStringField;
    dsTmpPressMas: TwwDataSource;
    qryCloseCheck: TADOQuery;
    qryClear: TADOQuery;
    Panel1: TPanel;
    Label3D12: TJSdLabel;
    btnSerialUp: TSpeedButton;
    btnSerialDown: TSpeedButton;
    DBNavigator1: TDBNavigator;
    edtLayer: TEdit;
    wwDBGrid1: TJSdDBGrid;
    qryExec: TADOQuery;
    qryMatClass: TADOQuery;
    procedure btnOKClick(Sender: TObject);
    procedure btnSerialDownClick(Sender: TObject);
    procedure btnSerialUpClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure qryTmpPressMasAfterDelete(DataSet: TDataSet);
    procedure qryTmpPressMasAfterInsert(DataSet: TDataSet);
    procedure qryTmpPressMasAfterPost(DataSet: TDataSet);
    procedure qryTmpPressMasBeforeDelete(DataSet: TDataSet);
    procedure qryTmpPressMasBeforeEdit(DataSet: TDataSet);
    procedure qryTmpPressMasBeforeInsert(DataSet: TDataSet);
  private
    function GetMaxSerialNum(tblDset: TDataset; sFieldName: string):integer;
    { Private declarations }
  public
    procedure GetData(sPartNum, sRevision, sLayerId :String);
    procedure PosChange(Direction :Integer);
    var CurrPartNum, CurrRevision, sUpdateUser{2012.03.08} : String;
    var iSpId : Integer;
    { Public declarations }
  end;

var
  dlgPressChange: TdlgPressChange;

implementation

uses NewNameEdit;

{$R *.dfm}

procedure TdlgPressChange.btnOKClick(Sender: TObject);
var sTmpId, sSql, sNotes: String;
    iNeedIns : Integer;
begin
  inherited;
  sTmpId:='';
  iNeedIns:=0;
  with dsTmpPressMas.DataSet do
     if state in [dsEdit, dsInsert] then Post;
  with qryCloseCheck do
  begin
    close;
    Parameters.ParamByName('PartNum').Value:= CurrPartNum;
    Parameters.ParamByName('Revision').Value:= CurrRevision;
    Parameters.ParamByName('SpId').Value:= iSpId;
    Open;
    if FieldByName('TmpId').AsString<>'' then
    begin
      sTmpId:=FieldByName('TmpId').AsString;
      iNeedIns:=0;
    end
    else
    begin
      Application.createForm(TdlgNewNameEdit, dlgNewNameEdit);
      with dlgNewNameEdit do
      begin
        Caption:='新設壓合疊構';
        Label3.Caption:=trim(FieldByName('OldTmpId').AsString)+')';
        edtNotes.Text:=FieldByName('Notes').AsWideString;
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
      sSql:='exec EMOdPressChangeUpdate '''+CurrPartNum+''', '''
            +CurrRevision+''', '''+trim(edtLayer.text)+''', '''+sTmpId+''', '
            + IntToStr(iNeedIns)+', '+IntToStr(iSpId)+',N'''+sNotes+'''';
      try
        with qryExec do
        begin
          Close;
          SQL.Clear;
          SQL.Add(sSql);
          ExecSql;
        end;
      except
        on E:Exception do
        begin
          MsgDlgJS(E.Message, mtWarning, [mbok], 0);
          abort;
        end;
      end;
      {if sErr<>'' then
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

procedure TdlgPressChange.btnSerialDownClick(Sender: TObject);
begin
  inherited;
  with dsTmpPressMas.DataSet do
     if state in [dsEdit, dsInsert] then Post;
  PosChange(1);
end;

procedure TdlgPressChange.btnSerialUpClick(Sender: TObject);
begin
  inherited;
  with dsTmpPressMas.DataSet do
     if state in [dsEdit, dsInsert] then Post;
  PosChange(0);
end;

procedure TdlgPressChange.FormClose(Sender: TObject; var Action: TCloseAction);
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

procedure TdlgPressChange.GetData(sPartNum, sRevision, sLayerId :String);
begin
  CurrPartNum := sPartNum;
  CurrRevision := sRevision;
  with qryTmpPressMas do
  begin
    close;
    Parameters.ParamByName('PartNum').Value:= sPartNum;
    Parameters.ParamByName('Revision').Value:= sRevision;
    Parameters.ParamByName('LayerId').Value:= sLayerId;
    Parameters.ParamByName('SpId').Value:= iSpId;
    Open;
  end;
end;

procedure TdlgPressChange.PosChange(Direction :Integer);
var iSerial : Integer;
begin
  iSerial := qryTmpPressMas.FieldByName('SerialNum').AsInteger;
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

  qryTmpPressMas.Locate('SerialNum' ,iSerial ,[loPartialKey])
end;

procedure TdlgPressChange.qryTmpPressMasAfterDelete(DataSet: TDataSet);
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

procedure TdlgPressChange.qryTmpPressMasAfterInsert(DataSet: TDataSet);
begin
  inherited;
  DataSet.fieldbyname('SpId').Asinteger := iSpId;
  DataSet.fieldbyname('SerialNum').Asinteger := DataSet.Tag;
  DataSet.fieldbyname('PartNum').AsString := CurrPartNum;
  DataSet.fieldbyname('Revision').AsString := CurrRevision;
  DataSet.fieldbyname('LayerId').AsString := trim(edtLayer.Text);
  DataSet.fieldbyname('BefLayer').AsString := '';
  DataSet.fieldbyname('isIn').AsInteger := 0;
end;

procedure TdlgPressChange.qryTmpPressMasAfterPost(DataSet: TDataSet);
begin
  inherited;
  dataset.Refresh;
end;

procedure TdlgPressChange.qryTmpPressMasBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  DataSet.Tag:=qryTmpPressMas.FieldByName('SerialNum').AsInteger;
end;

procedure TdlgPressChange.qryTmpPressMasBeforeEdit(DataSet: TDataSet);
begin
  inherited;
  if trim(qryTmpPressMas.FieldByName('MatClass').AsString)='L' then
  begin
    MsgDlgJS('層別關聯不可修改!!!',mtWarning, [mbOk],0);
    abort;
  end;
end;

procedure TdlgPressChange.qryTmpPressMasBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  with dsTmpPressMas.DataSet do
     if state in [dsEdit, dsInsert] then Post;
  DataSet.Tag:=GetMaxSerialNum(DataSet, 'SerialNum')+1;
end;

function TdlgPressChange.GetMaxSerialNum(tblDset: TDataset; sFieldName: string):integer;
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

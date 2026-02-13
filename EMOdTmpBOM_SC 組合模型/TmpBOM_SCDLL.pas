unit TmpBOM_SCDLL;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic, DB, ADODB, Buttons, ExtCtrls, JSdMultSelect, JSdLookupCombo,
  Wwdatsrc, JSdTable, StdCtrls, Grids, Wwdbigrd, Wwdbgrid, JSdDBGrid, ComCtrls,
  DBCtrls, ToolWin, TempBasDLL, wwdblook, MasDtlDLL, JSdGrid2Excel, JSdTreeView,
  JSdLabel;

type
  TNowStatus = (nBrowse, nEdit);

type
  TfrmEMOdTmpBOM_SCDLL = class(TfrmMasDtlDLL)
    trvBOM: TJSdTreeView;
    pnlBtnGroup: TPanel;
    btChange: TSpeedButton;
    btnForm: TSpeedButton;
    btSaveAs: TSpeedButton;
    Bevel3: TBevel;
    pnlSave: TPanel;
    Label1: TJSdLabel;
    edtTmpIdNew: TEdit;
    qryChkTmp: TADOQuery;
    Splitter1: TSplitter;
    procedure btnGetParamsClick(Sender: TObject);
    procedure btChangeClick(Sender: TObject);
    procedure qryMaster1BeforeEdit(DataSet: TDataSet);
    procedure qryMaster1AfterScroll(DataSet: TDataSet);
    procedure btnFormClick(Sender: TObject);
    procedure btSaveAsClick(Sender: TObject);
    procedure btnUpdateClick(Sender: TObject);
    procedure btnSaveHeightClick(Sender: TObject);
    procedure qryMaster1BeforePost(DataSet: TDataSet);
    procedure btnBrowseClick(Sender: TObject);
    procedure btnC1Click(Sender: TObject);
    procedure qryDetail1BeforeEdit(DataSet: TDataSet);
    procedure qryMaster1AfterPost(DataSet: TDataSet);

  private
    { Private declarations }
  public
    var iActiveType: Integer;
    { Public declarations }
  end;

var
  frmEMOdTmpBOM_SCDLL: TfrmEMOdTmpBOM_SCDLL;

implementation

uses TmpBOMSet, unit_DLL;

{$R *.dfm}

procedure TfrmEMOdTmpBOM_SCDLL.btnGetParamsClick(Sender: TObject);
var sPaperMasterHeight: String;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  inherited;
  //2011.11.21
  btnUpdate.Align:=alRight;
  btnUpdate.Align:=alLeft;
  pnlBtnGroup.Align:=alRight;
  pnlBtnGroup.Align:=alLeft;
  //on Create
  //SetFormatDB(TCustomADODataset(tblTmpDtl), 'EMOdProdLayer');
  with qryExec do
  begin
    iActiveType:=0;
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) '
       +'where SystemId=''EMO'' and ParamId=''TmpRouteActiveType''');
    Open;
    if FieldByName('Value').AsString='1' then
      iActiveType:=1;

    //儲存高度
    if active then qryExec.close;
    sql.Clear;
    sql.Add('select RuleId,DLLValue from CURdOCXItemOtherRule(nolock) where ItemId='+
      ''''+sItemId+'''');
    open;
    if qryExec.RecordCount>0 then
    begin
      if qryExec.Locate('RuleId', 'PaperMasterHeight', [loCaseInsensitive]) then
        sPaperMasterHeight:=trim(qryExec.Fields[1].AsString);
      if sPaperMasterHeight<>'' then
      begin
        try
          gridMaster1.Width:=strtoint(sPaperMasterHeight);
        except
        end;
      end;
      //if qryExec.RecordCount>0 end
      if qryExec.Locate('RuleId', 'DtlHeight', [loCaseInsensitive]) then
        sPaperMasterHeight:=trim(qryExec.Fields[1].AsString);
        if sPaperMasterHeight<>'' then
        begin
          try
            gridDetail1.Height:=strtoint(sPaperMasterHeight);
          except
          end;
        end;
    end;
  end;
  //BrowseData(tblTmpMas, True);
  //BrowseData(tblTmpDtl, True);
  with trvBOM do
  begin
    Setup;
    FullExpand;
    if Items.count >0 then
      Select(Items.item[0]);
  end;
  prcStoreFieldNeed_Def(self,qryExec); //for 強制大寫
  {//2020.12.15
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sList:=TstringList.Create;
      sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sList.Strings[0];
      sList.Free;
      FontSize := StrToInt(sFontSize);

      //2020.12.15
      if FontSize<>100 then
      begin
        pnlLeft.Width:=Round(pnlLeft.Width * FontSize / 100);
      end;
  end;}
end;

procedure TfrmEMOdTmpBOM_SCDLL.btnSaveHeightClick(Sender: TObject);
var iMasterHeight: Integer;
begin
  //inherited;
  //比照繼承
  iMasterHeight:=gridMaster1.Width;
  with qryExec do
  begin
    if Active then qryExec.close;
    sql.Clear;
    sql.Add('exec CURdLayerHeightSave '+''''+sItemId+''''+','+
                inttostr(iMasterHeight)+',0'
                );
    ExecSQL;
    qryExec.close;
    SQL.Clear;
    SQL.Add('update CURdOCXItemOtherRule set DLLValue='''
        +IntToStr(gridDetail1.Height)+''' where ItemId='''
        +sItemId+''' and RuleId=''DtlHeight''');
    ExecSql;
  end;
  MsgDlgJS('已儲存設定',mtInformation,[mbOk],0);
end;

procedure TfrmEMOdTmpBOM_SCDLL.btChangeClick(Sender: TObject);
var sCanEnter:String;
begin
  inherited;
  if qryMaster1.LockType=ltReadOnly then
  begin
    MsgDlgJS('請先按下修改按鈕!', mtError, [mbOk], 0);
    exit;
  end;
   with qryMaster1 do
  begin
     if state in [dsEdit, dsInsert] then
     begin
        MsgDlgJS('不可異動,請先儲存該代碼!', mtError, [mbOk], 0);
        exit;
     end;
  end;
  qryChkTmp.Close;
  qryChkTmp.Parameters.ParamByName('TmpId').Value := qryMaster1.FieldByName('TmpId').AsString;
  qryChkTmp.Open;

  with qryMaster1 do
  begin
     //if state in [dsEdit, dsInsert] then post;
     if fieldbyname('TmpId').isnull then
     begin
        MsgDlgJS('請先建立樣板主檔!', mtError, [mbOk], 0);
        exit;
     end;
     if qryChkTmp.RecordCount >=1 then
     begin
        if iActiveType=1 then
          MsgDlgJS('組合模型已審核，不可修改!', mtError, [mbOk], 0)
        else
          MsgDlgJS('組合模型已使用，不可修改!', mtError, [mbOk], 0);
        exit;
     end;
  end;
  //100504 add
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) '
            +'where SystemId=''EMO'' and ParamId=''UpdateBOMNum''');
    Open;
    sCanEnter:=FieldByName('Value').AsString;
  end;
  Application.CreateForm(TdlgTmpBOMSet, dlgTmpBOMSet);
  dlgTmpBOMSet.sConnectStr:=sConnectStr;
  dlgTmpBOMSet.prcDoSetConnOCX;
  try
  with dlgTmpBOMSet do
  begin
      //Create
      btnCreateCoor;
      //add 100504
      if sCanEnter='1' then
      begin
        spnLayer.EditorEnabled:=True;
        spnPress.EditorEnabled:=True;
      end;
      //Origin
      TmpId:= qryMaster1.Fieldbyname('TmpId').AsString;
      with qryExec do
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('select L=Max(EL), Degree=Max(Degree)+1 '
            +'from EMOdTmpBOMDtl(nolock) '
            +'where TmpId='''+TmpId+'''');
        Open;
        if FieldByName('L').AsInteger>12 then
          spnLayer.Value:= FieldByName('L').AsInteger;
        if FieldByName('Degree').AsInteger>6 then
          spnPress.Value:= FieldByName('Degree').AsInteger;

        qryExec.Close;
        SQL.Clear;
        SQL.Add('select Value '
            +'from CURdSysParams(nolock) '
            +'where SystemId=''EMO'' '
            +'and ParamId=''ChangeLayerName''');
        Open;
        if FieldByName('Value').AsString<>'1' then
        begin
          Label1.Visible:=False;
          Button1.Visible:=False;
          edtOriName.Visible:=False;
        end;
      end;
      DB2Memo(Sender);
      Memo2Controls(Sender);
      ShowModal;
      if ModalResult = mrOk then
      begin
        with qryExec do
        begin
          qryExec.Close;
          SQL.Clear;
          SQL.Add('DELETE dbo.EMOdTmpBOMDtl where TmpId='''
              +qryMaster1.FieldbyName('TmpId').AsString+'''' );
          execsql;
        Memo2DB(Sender);
          qryExec.Close;
          SQL.Clear;
          SQL.Add('exec EMOdTmpBomDegree '''
              +qryMaster1.FieldbyName('TmpId').AsString+'''' );
          execsql;
        end;
        qryMaster1.Close;
        qryMaster1.Open;
        trvBOM.Setup;
        trvBOM.FullExpand;
      end;
    end;
  finally
    dlgTmpBOMSet.Free;
  end;
end;

procedure TfrmEMOdTmpBOM_SCDLL.qryDetail1BeforeEdit(DataSet: TDataSet);
begin
  inherited;
  qryChkTmp.Close;
  qryChkTmp.Parameters.ParamByName('TmpId').Value := qryMaster1.FieldByName('TmpId').AsString;
  qryChkTmp.Open;

  if qryChkTmp.RecordCount >=1 then
  begin
    if iActiveType=1 then
      MsgDlgJS('此模型已審核，不得修改!!!',mtWarning, [mbOk],0);
    abort;
  end;
end;

procedure TfrmEMOdTmpBOM_SCDLL.qryMaster1BeforeEdit(DataSet: TDataSet);
begin
  inherited;
  qryChkTmp.Close;
  qryChkTmp.Parameters.ParamByName('TmpId').Value := qryMaster1.FieldByName('TmpId').AsString;
  qryChkTmp.Open;

  if qryChkTmp.RecordCount >=1 then
  begin
    if iActiveType=1 then
      MsgDlgJS('此模型已審核，不得修改!!!',mtWarning, [mbOk],0)
    else
      MsgDlgJS('已有審核料號使用此模型，不得修改!!!',mtWarning, [mbOk],0);
    qryMaster1.Cancel;
    abort;
  end;
end;

procedure TfrmEMOdTmpBOM_SCDLL.qryMaster1BeforePost(DataSet: TDataSet);
begin
  inherited;
  //2011.09.29
  if qryMaster1.FindField('UserId')<>nil then
  begin
    if qryMaster1.FieldByName('UserId').AsString='' then
    begin
      with qryExec do
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('exec EMOdTmpUpdateUserId '''+ qryMaster1.FieldByName('TmpId').AsString +''','
            +''''+ sUserId +''',''EMOdTmpBOMMas''');
        ExecSql;
      end;
    end;
  end;
end;

procedure TfrmEMOdTmpBOM_SCDLL.qryMaster1AfterPost(DataSet: TDataSet);
var sOriTmpId: String;
begin
  sOriTmpId:=qryMaster1.FieldByName('TmpId').AsString;
  inherited;
  qryMaster1.Locate('TmpId', sOriTmpId, [loCaseInsensitive]); //2024.09.27
end;

procedure TfrmEMOdTmpBOM_SCDLL.qryMaster1AfterScroll(DataSet: TDataSet);
var i:integer;
begin
  inherited;
  trvBOM.Setup;
  trvBOM.FullExpand;
end;

procedure TfrmEMOdTmpBOM_SCDLL.btnFormClick(Sender: TObject);
begin
  inherited;
  with qryMaster1 do
  begin
     if state in [dsEdit, dsInsert] then post;
     if fieldbyname('TmpId').isnull then
     begin
        MsgDlgJS('請先建立樣板主檔!', mtError, [mbOk], 0);
        exit;
     end;
  end;

  Application.CreateForm(TdlgTmpBOMSet, dlgTmpBOMSet);
  dlgTmpBOMSet.sConnectStr:=sConnectStr;
  dlgTmpBOMSet.prcDoSetConnOCX;
  try
    with dlgTmpBOMSet do
    begin
      //Create
      btnCreateCoor;
      //Origin
      TmpId:= qryMaster1.Fieldbyname('TmpId').AsString;
      with qryExec do
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('select L=Max(EL), Degree=Max(Degree)+1 '
            +'from EMOdTmpBOMDtl(nolock) '
            +'where TmpId='''+TmpId+'''');
        Open;
        spnLayer.Value:= FieldByName('L').AsInteger;
        if FieldByName('Degree').AsInteger>6 then
          spnPress.Value:= FieldByName('Degree').AsInteger;

        Label1.Visible:=False;
        Button1.Visible:=False;
        edtOriName.Visible:=False;
      end;
      DB2Memo(Sender);
      Memo2Controls(Sender);
      spnLayer.Enabled:=False;
      spnPress.Enabled:=False;
      btnOk.Visible:=False;
      iReadOnly:=1;
      ShowModal;
    end;
  finally
    dlgTmpBOMSet.Free;
  end;
end;

procedure TfrmEMOdTmpBOM_SCDLL.btSaveAsClick(Sender: TObject);
begin
  inherited;
  if qryMaster1.LockType=ltReadOnly then
  begin
    MsgDlgJS('請先按下修改按鈕!', mtError, [mbOk], 0);
    exit;
  end;
  with qryMaster1 do
  begin
     if state in [dsEdit, dsInsert] then post;
     if fieldbyname('TmpId').isnull then
     begin
        MsgDlgJS('請先建立樣板主檔!', mtError, [mbOk], 0);
        exit;
     end;
  end;
  If edtTmpIdNew.Text = '' then
  Begin
    MsgDlgJS('請輸入新代碼!', mtError, [mbOk], 0);
    exit;
  End;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdTmpBOMCopy '''+qryMaster1.fieldbyname('TmpId').AsString
              +''','''+edtTmpIdNew.Text+'''');
    try
    begin
      Open;
      MsgDlgJS(FieldByName('ResultStr').AsString,mtInformation,[mbOK],0);
      qryDetail1.Close;
      qryMaster1.Close;
      qryMaster1.Open;
      qryMaster1.Locate('TmpId',edtTmpIdNew.Text,[loCaseInsensitive]);
      qryDetail1.Open;
    end;
    except
      on E:Exception do MsgDlgJS(E.Message, mtWarning, [mbok], 0);
    end;
  end;
end;

procedure TfrmEMOdTmpBOM_SCDLL.btnUpdateClick(Sender: TObject);
var sOriTmpId: String;
begin
  inherited;
  sOriTmpId:=qryMaster1.FieldByName('TmpId').AsString;
  pnlBtnGroup.Align:=alRight;
  pnlBtnGroup.Align:=alLeft;
  //100830
    //btUpdate.Enabled:=False;
    //unit_DLL.prcChangeShowMode(pnl_NowMode,'UPDATE');
  qryMaster1.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
end;

procedure TfrmEMOdTmpBOM_SCDLL.btnBrowseClick(Sender: TObject);
var sOriTmpId: String;
begin
  inherited;
  sOriTmpId:=qryMaster1.FieldByName('TmpId').AsString;
  btnUpdate.Align:=alRight;
  btnUpdate.Align:=alLeft;
  pnlBtnGroup.Align:=alRight;
  pnlBtnGroup.Align:=alLeft;
    //btUpdate.Enabled:=False;
    //unit_DLL.prcChangeShowMode(pnl_NowMode,'BROWSE');
  qryMaster1.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
end;

procedure TfrmEMOdTmpBOM_SCDLL.btnC1Click(Sender: TObject);
var sOriTmpId: String;
begin
  sOriTmpId:=qryMaster1.FieldByName('TmpId').AsString;
  inherited;
  qryMaster1.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
end;

end.


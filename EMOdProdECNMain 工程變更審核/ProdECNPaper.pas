unit ProdECNPaper;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, PaperOrgDLL{, WinSkinData}, JSdGrid2Excel, Menus, JSdPopupMenu, DB,
  JSdTable, ADODB, StdCtrls, Mask, DBCtrls, JSdLabel, ExtCtrls, Grids, Wwdbigrd,
  Wwdbgrid, JSdDBGrid, ComCtrls, Buttons, Wwdatsrc, wwdblook, JSdLookupCombo;

{type
    TevCusBtnClickEvent2 = class
      oldCusBtnClickEvent2: TNotifyEvent;
      procedure prcCusBtnClick2(sender: TObject);
    end; }

type
  TfrmProdECNPaper = class(TfrmPaperOrgDLL)
    qrySetNum: TADOQuery;
    qrySetNumMatClass: TStringField;
    dsSetNum: TwwDataSource;
    tblAddData: TJSdTable;
    tblAddDataDtlNumId: TStringField;
    tblAddDataDtlNumName: TWideStringField;
    tblAddDataNumId: TStringField;
    tblAddDataNumName: TWideStringField;
    tblAddDataIsHand: TIntegerField;
    tblAddDataIsMust: TIntegerField;
    tblAddDataEnCode: TStringField;
    tblAddDataSetClass: TStringField;
    dsAddData: TDataSource;
    qryDtlNumName: TADOQuery;
    StringField1: TStringField;
    WideStringField1: TWideStringField;
    StringField2: TStringField;
    StringField3: TStringField;
    StringField4: TStringField;
    qrySetNumSubDtl2: TADOQuery;
    StringField5: TStringField;
    WideStringField2: TWideStringField;
    StringField6: TStringField;
    StringField7: TStringField;
    StringField8: TStringField;
    ScrollBox1: TScrollBox;
    JSdLabel1: TJSdLabel;
    JSdLabel2: TJSdLabel;
    Label3: TJSdLabel;
    Label7: TJSdLabel;
    Label5: TJSdLabel;
    Label6: TJSdLabel;
    Label10: TJSdLabel;
    Label4: TJSdLabel;
    Label8: TJSdLabel;
    Label9: TJSdLabel;
    DBEdit1: TDBEdit;
    DBEdit2: TDBEdit;
    DBEdit3: TDBEdit;
    DBEdit8: TDBEdit;
    DBEdit4: TDBEdit;
    DBEdit7: TDBEdit;
    DBEdit5: TDBEdit;
    DBMemo1: TDBMemo;
    DBMemo2: TDBMemo;
    DBEdit10: TDBEdit;
    Panel2: TPanel;
    GroupBox1: TGroupBox;
    Label12: TJSdLabel;
    Label11: TJSdLabel;
    btnGenNum: TSpeedButton;
    Label13: TJSdLabel;
    Label14: TJSdLabel;
    cboSetClass: TJSdLookupCombo;
    grdAddData: TwwDBGrid;
    cboDtlNumId: TwwDBLookupCombo;
    edtMatName: TDBEdit;
    DBCheckBox1: TDBCheckBox;
    JSdLabel3: TJSdLabel;
    qrySetNumClassName: TWideStringField;
    procedure btnGetParamsClick(Sender: TObject);
    procedure qryDetail1BeforePost(DataSet: TDataSet);
    procedure btnCompletedClick(Sender: TObject);
    procedure tblAddDataBeforeInsert(DataSet: TDataSet);
    procedure tblAddDataDtlNumIdValidate(Sender: TField);
    procedure cboDtlNumIdBeforeDropDown(Sender: TObject);
    procedure btnGenNumClick(Sender: TObject);
    procedure DBEdit1Exit(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure btnKeepStatusClick(Sender: TObject);
    procedure qryBrowseAfterOpen(DataSet: TDataSet);
    procedure qryBrowseBeforeClose(DataSet: TDataSet);
  private
    { Private declarations }
  public
    procedure InsOldData;
    var Spid, iIncRev :Integer;
    //sCurrOldPN: String;
    sCurrPartNum: String; //判斷是否觸發自動流程用
    sCusId: String;
    procedure DoClassChange;
    procedure GetRev_TCI;
    { Public declarations }
  end;

var
  frmProdECNPaper: TfrmProdECNPaper;

implementation

uses unit_DLL;

{$R *.dfm}

{procedure TevCusBtnClickEvent2.prcCusBtnClick2(sender: TObject);
begin
  frmBillPaper.prcCustBtnRun(TSpeedButton(Sender).Name);
end;}

procedure TfrmProdECNPaper.btnGenNumClick(Sender: TObject);
var sStr, sNewNum, sRev, sMsg:String;
    i: Integer;
begin
  inherited;
  if qryBrowse.LockType<>ltOptimistic then
    abort;
  if tblAddData.State in [dsEdit, dsInsert] then
    tblAddData.Post;
  //2012.08.17 增加參數，是否自動進版
  with qryExec do
  begin
    if iIncRev=0 then
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('exec EMOdGenSetNumAdd '''
        +qryBrowse.FieldByName('PaperNum').AsString
        +''', 1 ,'''+cboSetClass.Text+''', 1');
      Open;
      sNewNum:=FieldByName('Result').AsString;
      {bReadOnly:=edtGenNum.ReadOnly; //2007.11.19 JS需求單07111404
      edtGenNum.ReadOnly:=false;     //2007.11.19 JS需求單07111404
      edtGenNum.Text := FieldByName('Result').AsString;
      edtGenNum.ReadOnly:=bReadOnly; //2007.11.19 JS需求單07111404}
    end
    else
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('exec EMOdGenSetNumAddIncRev '
        +''''+qryBrowse.FieldByName('PaperNum').AsString+''','
        +''''+DBEdit1.Text+''','
        +''''+DBEdit2.Text+'''');
      Open;
      sNewNum:=FieldByName('NewPartNum').AsString;
      sStr:=FieldByName('MatName').AsString;
      sRev:=FieldByName('NewRevision').AsString;
    end;
  end;

  if iIncRev=0 then
  begin
    //0808
    sStr:='';
    //sStr:=edtMatName.Text;   不累加
    with dsAddData.DataSet do
    begin
      First;
      for i := 0 to tblAddData.RecordCount - 1 do
      begin
        sStr:=sStr + grdAddData.GetFieldValue(1) + ' ';
        Next;
      end;
    end;
  end;

  with qryDetail1 do
  begin
    edit;
    FieldByName('NewPartNum').Value:= sNewNum;
    FieldByName('MatName').Value:= sStr;
    if iIncRev=1 then
      FieldByName('NewRevision').Value:= sRev;
    Post;
  end;

  if iIncRev=1 then
    sMsg:='新版序 '+Trim(sRev)+' 已產生。'
  else
    sMsg:='新品號 '+Trim(sNewNum)+' 已產生。';

  MsgDlgJS(sMsg,mtInformation,[mbOK],0);
end;

procedure TfrmProdECNPaper.btnGetParamsClick(Sender: TObject);
//var i:integer;
//evCusBtnClickEvent:TevCusBtnClickEvent2;
var //2020.12.23
    i:integer;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  inherited;
  {for i := 1 to 8 do
    begin
      TSpeedButton(FindComponent('btnC'+inttostr(i))).OnClick:=nil;
    end;

  for i := 1 to 8 do
    begin
      evCusBtnClickEvent:=TevCusBtnClickEvent2.Create;
      evCusBtnClickEvent.oldCusBtnClickEvent2
        :=TSpeedButton(FindComponent('btnC'+inttostr(i))).OnClick;
      TSpeedButton(FindComponent('btnC'+inttostr(i))).OnClick
        :=evCusBtnClickEvent.prcCusBtnClick2;
      if assigned(evCusBtnClickEvent.oldCusBtnClickEvent2) then
        evCusBtnClickEvent.oldCusBtnClickEvent2(TSpeedButton(FindComponent('btnC'+inttostr(i))));
    end; }
  sCurrPartNum:='';
  qrySetNum.Close;
  qrySetNum.Open;
  cboSetClass.Setup;
  //prcStoreFieldNeed_Def(self,qryExec); //for 強制大寫
  iIncRev:=0; //2012.08.17
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'' '
      +'and ParamId=''ECNIncRev'' and Value=''1''');
    Open;
    if RecordCount>0 then
      iIncRev:=1;
    if iIncRev=1 then
      btnGenNum.Visible:=False;

    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'' '
          +'and ParamId=''CusId''');
    Open;
    sCusId:=FieldByName('Value').AsString;
  end;
  //客制
  if sCusId='MUT' then
  begin
    Label6.Visible:=False;
    DBEdit7.Visible:=False;
    Label7.Visible:=False;
    DBEdit8.Visible:=False;
  end;

  //2020.12.23
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sList:=TstringList.Create;
      sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sList.Strings[0];
      sList.Free;
      FontSize := StrToInt(sFontSize);
  end
  else
  begin
      FontSize := 100;
  end;
  if FontSize<>100 then
  begin
    ScrollBox1.ScaleBy(70, FontSize);
    grdAddData.ScaleBy(FontSize, 100);
    grdAddData.Width:=Round(grdAddData.Width * 100 / FontSize);
    grdAddData.Height:=Round(grdAddData.Height * 100 / FontSize);
    for i := 0 to ComponentCount - 1 do
       if Components[i] is TJSdLabel then
       begin
         if (TJSdLabel(Components[i]).Parent = ScrollBox1)
          or (TJSdLabel(Components[i]).Parent = GroupBox1) then
         begin
           TJSdLabel(Components[i]).Width:=
             Round(TJSdLabel(Components[i]).Width * FontSize / 70);
           TJSdLabel(Components[i]).Height:=
             Round(TJSdLabel(Components[i]).Height * FontSize / 70);
         end;
       end;
  end;
end;

procedure TfrmProdECNPaper.btnKeepStatusClick(Sender: TObject);
begin
  inherited;
  //cboSetClass.Text:='';
  sCurrPartNum:='';
end;

procedure TfrmProdECNPaper.cboDtlNumIdBeforeDropDown(Sender: TObject);
begin
  inherited;
  with qrySetNumSubDtl2 do
    begin
      close;
      Parameters.ParamByName('SetClass').Value
        :=tblAddData.FieldByName('SetClass').AsString;
      Parameters.ParamByName('NumId').Value
        :=tblAddData.FieldByName('NumId').AsString;
      open;
    end;
end;

procedure TfrmProdECNPaper.DoClassChange;
var sSQL: String;
begin
  inherited;
  //原本是放在下拉改變的事件中，但是此作業不能用來創造成品以外類型，也鎖住下拉了
    sSQL:= '';
    sSQL:= 'exec MGNdGenSetNumTable '''
          + cboSetClass.Text+''','
          + ''''+trim(sCurrPartNum)+'''';//trim(sCurrOldPN)+'''';
    with qryExec do
    begin
      Close;
      SQL.Clear;
      SQL.Add(sSQL);
      Open;
      Spid:= FieldByName('Spid').AsInteger;

      //2011.10.05
      qryExec.Close;
      SQL.Clear;
      SQL.Add('delete EMOdECNSetNumAddData where PaperNum='''
          +qryBrowse.FieldByName('PaperNum').AsString+ '''');
      ExecSql;
    end;
end;

procedure TfrmProdECNPaper.DBEdit1Exit(Sender: TObject);
begin
  inherited;
  if qryBrowse.LockType=ltOptimistic then
    InsOldData;
end;

procedure TfrmProdECNPaper.btnAddClick(Sender: TObject);
begin
  //DBEdit1.Text:='';
  //DBEdit2.Text:='';
  //edtMatName.Text:='';
  //cboSetClass.Text:='';
  inherited;
end;

procedure TfrmProdECNPaper.btnCompletedClick(Sender: TObject);
var sErr, sSQL: String;
begin
  //Check
  if qryDetail1.State in [dsEdit, dsInsert] then
    qryDetail1.Post;

  if cboSetClass.Text='' then
  begin
    MsgDlgJS('分類不得為空',mtWarning,[mbOK],0);
    Exit;
  end;
  if ((edtMatName.Text='') and (iIncRev=0)) then //2020.03.03 iIncRev=1時，品名是照來源
  begin
    MsgDlgJS('請輸入品名',mtWarning,[mbOK],0);
    Exit;
  end;

  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select PartNum from EMOdProdInfo(nolock) where Rtrim(PartNum)= '
        +''''+trim(qryDetail1.FieldByName('NewPartNum').AsString)+''' and Rtrim(Revision)='
        +''''+trim(qryDetail1.FieldByName('NewRevision').AsString)+'''');
    Open;
    if RecordCount>0 then
    begin
      if MsgDlgJS('品號 '+Trim(qryDetail1.FieldByName('NewPartNum').AsString)+' 版序 '
        +Trim(qryDetail1.FieldByName('NewRevision').AsString)+' 已存在，確定要覆蓋？'
        ,mtConfirmation,[mbOK, mbCancel],0)<>mrOk then
      Exit;
    end;
  end;
  inherited;
  sCurrPartNum:='';
end;

procedure TfrmProdECNPaper.qryBrowseAfterOpen(DataSet: TDataSet);
begin
  inherited;
  tblAddData.Close;
  tblAddData.LockType:= qryBrowse.LockType;
  tblAddData.Open;
end;

procedure TfrmProdECNPaper.qryBrowseBeforeClose(DataSet: TDataSet);
begin
  inherited;
  if tblAddData.State in [dsEdit, dsInsert] then
    tblAddData.Post;
end;

procedure TfrmProdECNPaper.qryDetail1BeforePost(DataSet: TDataSet);
begin
  inherited;
  with qryDetail1 do
  begin
    if State in [dsEdit, dsInsert] then
    begin
      FieldByName('PartNum').AsString := UpperCase(FieldByName('PartNum').AsString);
      FieldByName('Revision').AsString := UpperCase(FieldByName('Revision').AsString);
      FieldByName('NewRevision').AsString := UpperCase(FieldByName('NewRevision').AsString);
    end;
  end;
end;

procedure TfrmProdECNPaper.tblAddDataBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  abort;
end;

procedure TfrmProdECNPaper.tblAddDataDtlNumIdValidate(Sender: TField);
begin
  inherited;
  qryDtlNumName.Close;
  qryDtlNumName.Filter := 'SetClass = ''' + tblAddData.FieldByName('SetClass').AsString + ''''
    + ' And NumId = ''' + tblAddData.FieldByName('NumId').AsString + ''''
    + ' And DtlNumId = ''' + tblAddData.FieldByName('DtlNumId').AsString + '''';
  qryDtlNumName.Filtered := True;
  qryDtlNumName.Open;

  tblAddData.FieldByName('DtlNumName').AsString := qryDtlNumName.FieldByName('DtlNumName').AsString;
  tblAddData.FieldByName('EnCode').AsString := qryDtlNumName.FieldByName('EnCode').AsString;
end;

procedure TfrmProdECNPaper.InsOldData;
var sMatClass: String;
//var sErr, sSQL : String;
begin
  if ((DBEdit1.text='') or (DBEdit2.text='')) then
    exit;
  if iIncRev=0 then
  begin
    if sCurrPartNum=trim(DBEdit1.text) then
      exit;
  end;

  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select MatClass, MatName from MINdMatInfo(nolock) where PartNum= '''
        +trim(qryDetail1.FieldByName('PartNum').AsString)+''' and Revision= ''0''');
    Open;
    sMatClass:=FieldByName('MatClass').AsString;
    edtMatName.Text:= FieldByName('MatName').AsString;
  end;
  //if (trim(cboSetClass.Text)<>trim(sMatClass)) then
  if sMatClass<>'' then
  begin
    cboSetClass.Text:=sMatClass;
    DoClassChange;
  end;

  if sMatClass<>'' then
  begin
    with qryExec do
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('exec EMOdUpdateAddData '''+qryBrowse.FieldByName('PaperNum').AsString
          +''',1'
          +','''+trim(qryDetail1.FieldByName('Partnum').AsString)
          +''', ''0'', '''+trim(cboSetClass.Text)+''', '+IntToStr(SPId));
      Open;
      //tblAddData.Refresh;
      tblAddData.Close;
      tblAddData.Open;
    end;
  end;
  sCurrPartNum:=trim(DBEdit1.text);

  //客制
  if sCusId='MUT' then
  begin
    btnGenNumClick(Self);
  end;
  if (sCusId='TCI') and (PowerType=0) then
  begin
    GetRev_TCI;
  end;
end;

procedure TfrmProdECNPaper.GetRev_TCI;
var sNewNum, sRev:string;
begin
  if qryBrowse.LockType<>ltOptimistic then
    abort;
  if tblAddData.State in [dsEdit, dsInsert] then
    tblAddData.Post;

  with qryExec do
  begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('exec EMOdGenSetNum_TCI '
        +''''+qryBrowse.FieldByName('PaperNum').AsString+''','
        +''''+DBEdit1.Text+''','
        +''''+DBEdit2.Text+'''');
      Open;
      sNewNum:=FieldByName('NewPartNum').AsString;
      sRev:=FieldByName('NewRevision').AsString;
  end;

  with qryDetail1 do
  begin
    edit;
    FieldByName('NewPartNum').Value:= sNewNum;
    FieldByName('NewRevision').Value:= sRev;
    Post;
  end;

end;

end.
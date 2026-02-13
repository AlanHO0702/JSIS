unit TmpRouteDLL;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic{, WinSkinData}, JSdGrid2Excel, Menus, JSdPopupMenu, DB,
  JSdTable, ADODB, StdCtrls, Mask, DBCtrls, JSdLabel, ExtCtrls, Grids, Wwdbigrd,
  Wwdbgrid, JSdDBGrid, ComCtrls, Buttons, Wwdatsrc, JSdTreeView, ToolWin,
  ImgList, JSdMultSelect, SingleGridDLL, System.ImageList;

type
  TfrmTmpRouteDLL = class(TfrmSingleGridDLL)
    dsTmpMas: TDataSource;
    dsTmpDtl: TDataSource;
    ImageList1: TImageList;
    tblTmpMas: TJSdTable;
    pnlAll: TPanel;
    pgeMaster: TPageControl;
    TabSheet1: TTabSheet;
    grdData: TJSdDBGrid;
    Splitter1: TSplitter;
    tblTmpDtl: TJSdTable;
    TabSheet2: TTabSheet;
    rdoStatus: TRadioGroup;
    edtTmpId: TEdit;
    edtNotes: TEdit;
    chkNotes: TJSdLabel;
    btnOK: TSpeedButton;
    chkStatus: TJSdLabel;
    chkTmpId: TJSdLabel;
    pnlClient: TPanel;
    grdRouteDtl: TJSdDBGrid;
    Splitter2: TSplitter;
    tblTmpDtlTmpId: TStringField;
    tblTmpDtlSerialNum: TWordField;
    tblTmpDtlProcCode: TStringField;
    tblTmpDtlNotes: TWideStringField;
    tblTmpDtlFinishRate: TFloatField;
    tblTmpDtlProcName: TWideStringField;
    pnlNotes: TPanel;
    DBMemo1: TDBMemo;
    Panel2: TPanel;
    Panel3: TPanel;
    navDtl: TDBNavigator;
    qryProcInfo: TADOQuery;
    pnlRouteNote: TPanel;
    pnlNoteSep: TPanel;
    sptNote: TSplitter;
    dbnBrowse: TDBNavigator;
    Bevel3: TBevel;
    btChange: TSpeedButton;
    btSaveAs: TSpeedButton;
    pnlSave: TPanel;
    Label1: TJSdLabel;
    edtTmpIdNew: TEdit;
    Panel5: TPanel;
    Panel6: TPanel;
    pnl_CountDtl: TPanel;
    pnl_Count2: TPanel;
    Panel7: TPanel;
    btnArrowDtl: TSpeedButton;
    btnArrowMas: TSpeedButton;
    btnSaveHeight: TSpeedButton;
    pnlBtnGroup: TPanel;
    procedure btnGetParamsClick(Sender: TObject);
    procedure btChangeClick(Sender: TObject);
    procedure tblTmpMasBeforeEdit(DataSet: TDataSet);
    procedure btnOKClick(Sender: TObject);
    //procedure BrowseData(tblDset: TDataset; bRefresh: Boolean);
    function IIFString(bYes:Boolean; str1, str2: WideString): WideString;
    procedure btSaveAsClick(Sender: TObject);
    procedure tblTmpDtlBeforeDelete(DataSet: TDataSet);
    procedure tblTmpDtlBeforeInsert(DataSet: TDataSet);
    procedure tblTmpDtlBeforeEdit(DataSet: TDataSet);
    procedure tblTmpMasAfterClose(DataSet: TDataSet);
    procedure tblTmpMasAfterScroll(DataSet: TDataSet);
    procedure tblTmpDtlAfterClose(DataSet: TDataSet);
    procedure tblTmpDtlAfterScroll(DataSet: TDataSet);
    procedure grdRouteDtlEnter(Sender: TObject);
    procedure grdDataEnter(Sender: TObject);
    procedure btnSaveHeightClick(Sender: TObject);
    procedure btnUpdateClick(Sender: TObject);
    procedure tblTmpMasBeforePost(DataSet: TDataSet);
    procedure btnBrowseClick(Sender: TObject);
    procedure btnC1Click(Sender: TObject);
    procedure DBMemo1Enter(Sender: TObject);
    procedure tblTmpDtlAfterInsert(DataSet: TDataSet);
    procedure tblTmpMasBeforeDelete(DataSet: TDataSet);
  private
    { Private declarations }
  public
    //2012.04.12 Timeout Fail
    var iTimeOut:Integer;
    var iActiveType, iNewItem: Integer;
    procedure Add2Filter(var sFilter  :string; const sCondition :string);
    { Public declarations }
  end;

var
  frmTmpRouteDLL: TfrmTmpRouteDLL;

implementation

uses TmpRouteSet, unit_DLL;

{$R *.dfm}

procedure TfrmTmpRouteDLL.btChangeClick(Sender: TObject);
var i: integer;
begin
  inherited;
  if tblTmpMas.LockType=ltReadOnly then
  begin
    MsgDlgJS('請先按下修改按鈕!', mtError, [mbOk], 0);
    exit;
  end;

  with tblTmpMas do
  begin
     if state in [dsEdit, dsInsert] then post;
     if fieldbyname('TmpId').isnull then
     begin
        MsgDlgJS('請先建立模型主檔!', mtError, [mbOk], 0);
        exit;
     end;
     if fieldbyname('Status').AsInteger = 1 then
     begin
        if iActiveType=1 then
          MsgDlgJS('途程模型已審核，不可修改!', mtError, [mbOk], 0)
        else
          MsgDlgJS('途程模型已使用，不可修改!', mtError, [mbOk], 0);
        exit;
     end;
  end;
  Application.CreateForm(TdlgTmpRouteSet, dlgTmpRouteSet);
  dlgTmpRouteSet.sConnectStr:=sConnectStr;
  dlgTmpRouteSet.prcDoSetConnOCX;
  try
    with dlgTmpRouteSet do
    begin
      btnSearchClick(Sender);
      with qryTmpRouteDtl do
      begin
         Close;
         Parameters.ParamByName('TmpId').Value:= tblTmpMas.FieldbyName('TmpId').AsString;
         Open;
      end;
      //BrowseData(frmMain.qryPressMat, false);
      msSelects.Setup(slAll);
      ShowModal;
      if ModalResult = mrOk then
      begin
         with qryExec do
         begin
            //2012.04.06 add
            qryExec.Close;
            SQL.Clear;
            SQL.Add('exec EMOdTmpRouteHoldNote '''
                    +tblTmpMas.FieldbyName('TmpId').AsString+''',0');
            ExecSql;

            qryExec.Close;
            SQL.Clear;
            SQL.Add('DELETE dbo.EMOdTmpRouteDtl FROM EMOdTmpRouteMas t1(nolock),'
              +'EMOdTmpRouteDtl t2 where t1.TmpId = '''
              +tblTmpMas.FieldbyName('TmpId').AsString+''' and t1.TmpId = t2.TmpId '
              +'and t1.status = 0');
            execsql;

            for i:= 0 to msSelects.TargetItems.Count-1 do
            begin
               qryExec.Close;
               SQL.Clear;
               SQL.Add('if (select status from dbo.EMOdTmpRouteMas(nolock) '
                  +'where TmpId='''+tblTmpMas.FieldbyName('TmpId').AsString+''')=0 '
                  +'begin insert into dbo.EMOdTmpRouteDtl'
                  +'(TmpId, SerialNum, ProcCode, FinishRate) '
                  +'values('''+tblTmpMas.FieldbyName('TmpId').AsString+''','
                  +IntToStr(i+1)+', '''+msSelects.TargetItems[i].Caption+''', 1) end');
               execsql;
            end;

            //2012.04.06 add
            qryExec.Close;
            SQL.Clear;
            SQL.Add('exec EMOdTmpRouteHoldNote '''
                    +tblTmpMas.FieldbyName('TmpId').AsString+''',1');
            ExecSql;
         end;
         tblTmpDtl.Close;
         tblTmpDtl.Open;
      end;
    end;
  finally
    dlgTmpRouteSet.Free;
  end;
end;

procedure TfrmTmpRouteDLL.btnBrowseClick(Sender: TObject);
var sOriTmpId: String;
begin
  inherited;
  sOriTmpId:=tblTmpMas.FieldByName('TmpId').AsString;
  btnUpdate.Align:=alRight;
  btnUpdate.Align:=alLeft;
  pnlBtnGroup.Align:=alRight;
  pnlBtnGroup.Align:=alLeft;
    tblTmpMas.Close;
    tblTmpMas.LockType:=ltReadOnly; //100830
    tblTmpMas.Open;
    tblTmpDtl.Close;
    tblTmpDtl.LockType:=ltReadOnly;
    tblTmpDtl.Open;
    //btUpdate.Enabled:=False;
    //unit_DLL.prcChangeShowMode(pnl_NowMode,'BROWSE');
  tblTmpMas.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
  DBMemo1.ReadOnly:=true;
end;

procedure TfrmTmpRouteDLL.btnC1Click(Sender: TObject);
var sOriTmpId: String;
begin
  //2011.09.29 按鈕發動時，必須讓背後的主檔跟到畫面。
  tblEasyDB.Close;
  tblEasyDB.Open;
  if not (tblEasyDB.Locate('TmpId', tblTmpMas.FieldByName('TmpId').AsString, [loCaseInsensitive])) then
  begin
    MsgDlgJS('主檔對應失敗，請退出此作業重新進行!', mtError, [mbOk], 0);
    exit;
  end;
  sOriTmpId:=tblTmpMas.FieldByName('TmpId').AsString;
  inherited;
  tblTmpMas.Close;
  tblTmpMas.Open;
  tblTmpMas.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
end;

procedure TfrmTmpRouteDLL.btnGetParamsClick(Sender: TObject);
var sPaperMasterHeight, sPaperDetailHeight: String;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  inherited;
  //2011.11.21
  tblTmpMas.LookupType:=lkJoinSQL;
  //tblTmpDtl.LookupType:=lkLookupTable; 2013.10.11 Field裡有指定fkLookup時，query不能用lkLookupTable模式
  btnUpdate.Align:=alRight;
  btnUpdate.Align:=alLeft;
  pnlBtnGroup.Align:=alRight;
  pnlBtnGroup.Align:=alLeft;
  //on Create
  //SetFormatDB(TCustomADODataset(tblTmpDtl), 'EMOdLayerRoute');
  pgeMaster.ActivePageIndex:= 0;
  if not tblTmpMas.active then
  tblTmpMas.Open;
  if not tblTmpDtl.active then
  tblTmpDtl.Open;
  qryProcInfo.Close;
  qryProcInfo.Open;
  //BrowseData(tblTmpMas, True);
  //BrowseData(tblTmpDtl, True);
  prcStoreFieldNeed_Def(self,qryExec); //for 強制大寫
  //2011.09.27 途程備註預設欄寬
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) '
       +'where SystemId=''EMO'' and ParamId=''RouteNoteWidth''');
    Open;
    if FieldByName('Value').AsString<>'' then
      DBMemo1.Width:=FieldByName('Value').AsInteger
    else
    begin
      DBMemo1.Align:=alClient;
      pnlNoteSep.Visible:=False;
    end;
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
      if qryExec.Locate('RuleId', 'PaperDetailHeight', [loCaseInsensitive]) then
        sPaperDetailHeight:=trim(qryExec.Fields[1].AsString);
      if sPaperMasterHeight<>'' then
      begin
        try
          pgeMaster.Width:=strtoint(sPaperMasterHeight);
        except
        end;
      end;
      if sPaperDetailHeight<>'' then
      begin
        try
          pnlNotes.Height:=strtoint(sPaperDetailHeight);
        except
        end;
      end;
      //if qryExec.RecordCount>0 end
    end;
  end;
  btnArrowMas.Visible:=True;
  btnArrowDtl.Visible:=not(btnArrowMas.Visible);
  //2012.04.12 Timeout Fail
  iTimeOut:=0;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
            +' and ParamId=''TimeOutSec''');
    Open;
    if FieldByName('Value').AsInteger>0 then
    begin
      iTimeOut:=FieldByName('Value').AsInteger;
    end;
  end;
  //2020.12.15
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
        pgeMaster.Width:=Round(pgeMaster.Width * FontSize / 100);
      end;
  end;
end;

procedure TfrmTmpRouteDLL.btnOKClick(Sender: TObject);
var sSQLComd: string;
begin
   inherited;
   sSQLComd:='';
   if edtTmpId.text<>'' then
   begin
      //frmMain.Add2Filter(sSQLComd,'TmpId >= '''+ edtTmpId.text+'''');
      //frmMain.Add2Filter(sSQLComd,'TmpId < '''+ edtTmpId.text+char(65535)+'''');
      Add2Filter(sSQLComd,'t1.TmpId like ''%'+ edtTmpId.text+'%'''); //2010.02.23
   end;
   if edtNotes.text<>'' then
   begin
      //frmMain.Add2Filter(sSQLComd,'Notes >= '''+ edtNotes.text+'''');
      //frmMain.Add2Filter(sSQLComd,'Notes < '''+ edtNotes.text+char(65535)+'''');
      Add2Filter(sSQLComd,'t1.Notes like N''%'+ edtNotes.text+'%'''); //2010.02.23
   end;
   if ((rdoStatus.itemindex>-1) and (rdoStatus.itemindex<2)) then
      Add2Filter(sSQLComd,'t1.Status = ''' +  inttostr(rdoStatus.itemindex) + '''');

  if sSQLComd<>'' then sSQLComd:=' and '+sSQLComd; //2010.02.23

  with tblTmpMas do
  begin
    if State in [dsEdit,dsInsert] then Post;
    Close;
    SQL.Clear;
    SQL.Add('select t1.*, t2.StatusName from EMOdTmpRouteMas t1 '
            +'Left Join EMOdTmpRouteStatus t2 On t1.Status=t2.Status '
            +'where 1=1'+sSQLComd);
     {filter:= sSQLComd;
     filtered:= true;
     if Active then
        refresh
     else
        open;}
    Open;
  end;
  pgeMaster.ActivePage := TabSheet1;
end;

procedure TfrmTmpRouteDLL.btSaveAsClick(Sender: TObject);
var i: integer;
begin
  inherited;
  if tblTmpMas.LockType=ltReadOnly then
  begin
    MsgDlgJS('請先按下修改按鈕!', mtError, [mbOk], 0);
    exit;
  end;

  with tblTmpMas do
  begin
     if fieldbyname('TmpId').isnull then
     begin
        MsgDlgJS('請先參考模型主檔!', mtError, [mbOk], 0);
        exit;
     end;
     if state in [dsEdit, dsInsert] then post;
  end;

  With qryExec do
  Begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('Select CNT=Count(*) From EMOdTmpRouteMas(nolock) Where TmpId = '
      +''''+edtTmpIdNew.Text+'''');
    Open;
    if FieldByName('CNT').AsInteger>0 then
    begin
        MsgDlgJS('已存在該代碼!', mtError, [mbOk], 0);
        exit;
    end;
  End;

  If edtTmpIdNew.Text = '' then
  Begin
    MsgDlgJS('請輸入新代碼!', mtError, [mbOk], 0);
    exit;
  End
  Else
  Begin
    Application.CreateForm(TdlgTmpRouteSet, dlgTmpRouteSet);
    dlgTmpRouteSet.sConnectStr:=sConnectStr;
    dlgTmpRouteSet.prcDoSetConnOCX;
    try
    with dlgTmpRouteSet do
    begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryTmpRouteDtl.CommandTimeout:=iTimeOut;
        qryMas.CommandTimeout:=iTimeOut;
        qryProcBasic.CommandTimeout:=iTimeOut;
      end;
      qryProcBasic.Close;
      qryProcBasic.Open;
      with qryTmpRouteDtl do
      begin
         Close;
         Parameters.ParamByName('TmpId').Value:= tblTmpMas.FieldbyName('TmpId').AsString;
         Open;
      end;
      //BrowseData(frmMain.qryPressMat, false);
      msSelects.Setup(slAll);
      //2020.03.09 Add
      btnSearchClick(Sender);
      ShowModal;
      if ModalResult = mrOk then
      begin
        With qryExec do
        Begin
          qryExec.Close;
          SQL.Clear;
          SQL.Add('insert into EMOdTmpRouteMas(TmpId,Notes,Status,IsStop) Select '''
            +edtTmpIdNew.Text+''', '''', 0, 0');
          ExecSql;

          for i:= 0 to msSelects.TargetItems.Count-1 do
          begin
            qryExec.Close;
            SQL.Clear;
            SQL.Add('if (select status from dbo.EMOdTmpRouteMas(nolock) where '
              +'TmpId = '''+edtTmpIdNew.Text+''') = 0 begin insert into '
              +'dbo.EMOdTmpRouteDtl(TmpId, SerialNum, ProcCode, FinishRate) '
              +'values('''+edtTmpIdNew.Text+''', '+IntToStr(i+1)+', '''
              +msSelects.TargetItems[i].Caption+''', 1) end');
            execsql;
          end;
        end;
        tblTmpDtl.Close;
        tblTmpMas.Close;
        tblTmpMas.Open;
        tblTmpMas.Locate('TmpId',edtTmpIdNew.Text,[loCaseInsensitive]);
        tblTmpDtl.Open;
        //2011.10.07 for BuildDate Update
        tblTmpMas.Edit;
        tblTmpMas.Post;
      end;
    end;
  finally
    dlgTmpRouteSet.Free;
  end;
  End;
end;

procedure TfrmTmpRouteDLL.DBMemo1Enter(Sender: TObject);
begin
  inherited;
  if dbnBrowse.DataSource<>dsTmpDtl then
    dbnBrowse.DataSource:=dsTmpDtl;
end;

procedure TfrmTmpRouteDLL.btnSaveHeightClick(Sender: TObject);
var iWidth: Integer;
    iMasterHeight, iDetailHeight: Integer;
begin
  inherited;
  //2011.09.27 途程備註預設欄寬
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) '
       +'where SystemId=''EMO'' and ParamId=''RouteNoteWidth''');
    Open;
    if FieldByName('Value').AsString<>'' then
    begin
      iWidth:=FieldByName('Value').AsInteger;
      if DBMemo1.Width<>iWidth then
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('update CURdSysParams set Value='''+IntToStr(DBMemo1.Width)
              +''' where SystemId=''EMO'' and ParamId=''RouteNoteWidth''');
        ExecSql;
      end;
    end;
  end;
  //比照繼承
  iMasterHeight:=pgeMaster.Width;
  iDetailHeight:=pnlNotes.Height;
  with qryExec do
  begin
    if Active then qryExec.close;
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

procedure TfrmTmpRouteDLL.btnUpdateClick(Sender: TObject);
var sOriTmpId: String;
begin
  inherited;
  sOriTmpId:=tblTmpMas.FieldByName('TmpId').AsString;
  pnlBtnGroup.Align:=alRight;
  pnlBtnGroup.Align:=alLeft;
    tblTmpMas.Close;
    tblTmpMas.LockType:=ltOptimistic; //100830
    tblTmpMas.Open;
    tblTmpDtl.Close;
    tblTmpDtl.LockType:=ltOptimistic;
    tblTmpDtl.Open;
    //btUpdate.Enabled:=False;
    //unit_DLL.prcChangeShowMode(pnl_NowMode,'UPDATE');
  tblTmpMas.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
  DBMemo1.ReadOnly:=False;
end;

procedure TfrmTmpRouteDLL.grdDataEnter(Sender: TObject);
begin
  inherited;
  btnArrowMas.Visible:=True;
  btnArrowDtl.Visible:=not(btnArrowMas.Visible);
  if dbnBrowse.DataSource<>dsTmpMas then
    dbnBrowse.DataSource:=dsTmpMas;
end;

procedure TfrmTmpRouteDLL.grdRouteDtlEnter(Sender: TObject);
begin
  inherited;
  btnArrowMas.Visible:=False;
  btnArrowDtl.Visible:=not(btnArrowMas.Visible);
  if dbnBrowse.DataSource<>dsTmpDtl then
    dbnBrowse.DataSource:=dsTmpDtl;
end;

procedure TfrmTmpRouteDLL.tblTmpDtlBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  //2011.10.04 abort;
end;

procedure TfrmTmpRouteDLL.tblTmpDtlBeforeEdit(DataSet: TDataSet);
begin
  inherited;
  with tblTmpMas do
  begin
     if state in [dsEdit, dsInsert] then post;
     if fieldbyname('TmpId').isnull then
     begin
        MsgDlgJS('請先建立模型主檔!', mtError, [mbOk], 0);
        abort;
     end;
     if fieldbyname('Status').AsInteger = 1 then
     begin
        if iActiveType=1 then
          MsgDlgJS('途程模型已審核，不可修改!', mtError, [mbOk], 0)
        else
          MsgDlgJS('途程模型已使用，不可修改!', mtError, [mbOk], 0);
        abort;
     end;
  end;
end;

procedure TfrmTmpRouteDLL.tblTmpDtlAfterClose(DataSet: TDataSet);
begin
  inherited;
  pnl_CountDtl.Caption:='0 /0';
end;

procedure TfrmTmpRouteDLL.tblTmpDtlAfterScroll(DataSet: TDataSet);
var i:integer;
begin
  inherited;
  i:=tblTmpDtl.RecNo;
  if i<0 then i:=0;
  pnl_CountDtl.Caption:=inttostr(i)+' / '+ inttostr(tblTmpDtl.RecordCount);
end;

procedure TfrmTmpRouteDLL.tblTmpDtlBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  //2011.10.04 abort;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select SerialNum=Max(SerialNum) from dbo.EMOdTmpRouteDtl(nolock)'
      +' where TmpId='''+tblTmpMas.FieldByName('TmpId').AsString+'''');
    Open;
    iNewItem:=FieldByName('SerialNum').AsInteger+1;
  end;
end;

procedure TfrmTmpRouteDLL.tblTmpDtlAfterInsert(DataSet: TDataSet);
begin
  inherited;
  //2011.10.04
  tblTmpDtl.fieldbyname('SerialNum').Asinteger:= iNewItem;
end;

procedure TfrmTmpRouteDLL.tblTmpMasAfterClose(DataSet: TDataSet);
begin
  inherited;
  pnl_Count2.Caption:='0 /0';
end;

procedure TfrmTmpRouteDLL.tblTmpMasAfterScroll(DataSet: TDataSet);
var i:integer;
begin
  inherited;
  i:=tblTmpMas.RecNo;
  if i<0 then i:=0;
  pnl_Count2.Caption:=inttostr(i)+' / '+ inttostr(tblTmpMas.RecordCount);
end;

procedure TfrmTmpRouteDLL.tblTmpMasBeforeDelete(DataSet: TDataSet);
begin
  if DataSet.FieldByName('Status').AsInteger = 1 then
  begin
    MsgDlgJS('途程模型已審核，不可修改!', mtError, [mbOk], 0);
    abort;
  end;
  with qryExec do
  begin
    qryExec.Close;
    sql.Clear;
    sql.Add('delete EMOdTmpRouteMas where TmpId='''
      +tblTmpMas.FieldByName('TmpId').AsString+'''');
    ExecSql;
    tblTmpMas.Close;
    tblTmpMas.Open;
    tblTmpDtl.Close;
    tblTmpDtl.Open;
  end;
  abort;
  //inherited;
end;

procedure TfrmTmpRouteDLL.tblTmpMasBeforeEdit(DataSet: TDataSet);
begin
  inherited;
  if DataSet.FieldByName('Status').AsInteger = 1 then
  begin
    tblTmpMas.Cancel;
    abort;
  end;
end;

procedure TfrmTmpRouteDLL.tblTmpMasBeforePost(DataSet: TDataSet);
begin
  inherited;
  //2011.09.29
  if tblTmpMas.FindField('UserId')<>nil then
  begin
    if tblTmpMas.FieldByName('UserId').AsString='' then
    begin
      with qryExec do
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('exec EMOdTmpUpdateUserId '''+ tblTmpMas.FieldByName('TmpId').AsString +''','
            +''''+ sUserId +''',''EMOdTmpRouteMas''');
        ExecSql;
      end;
    end;
  end;
end;

procedure TfrmTmpRouteDLL.Add2Filter(var sFilter  :string; const sCondition :string);
begin
   sFilter := sFilter + IIFString(sFilter='', '', ' AND ') + sCondition;
end;

function TfrmTmpRouteDLL.IIFString(bYes:Boolean; str1, str2: WideString): WideString;
begin
  if bYes then
    Result := (str1)
  else
    Result := (str2);
end;

{ 2010.03.01 反而會造成資料辭典出不來
procedure TfrmTmpBOMDLL.BrowseData(tblDset: TDataset; bRefresh: Boolean);
var bk: TBookMark;
    oldActive: Boolean;
begin
  if tblDset = nil then exit;
    try
      try
        tblDset.DisableControls;
        oldActive:= tblDset.Active;
        if oldActive then bk:= tblDset.GetBookMark;
        if (bRefresh) then
        begin
           if tblDset is TADOQuery then
           begin
              TADOQuery(tblDset).Close;
              TADOQuery(tblDset).Open;
           end
           else if tblDset is TADOTable then
           begin
              if TADOTable(tblDset).MasterSource=nil then
              begin
                TADOTable(tblDset).Close;
                TADOTable(tblDset).Open;
              end
              else
              begin
                if TADOTable(tblDset).Active then
                   TADOTable(tblDset).Refresh
                else
                begin
                  TADOTable(tblDset).Close;
                  TADOTable(tblDset).Open;
                end;
              end;
           end;
        end
        else
           tblDset.Open;
        try
           if oldActive then tblDset.GotoBookMark(bk);
        except
           tblDset.next;
        end;
      except
        raise;
      end;
    finally
      if oldActive then tblDset.FreeBookMark(bk);
      tblDset.EnableControls;
    end;
end; }

end.

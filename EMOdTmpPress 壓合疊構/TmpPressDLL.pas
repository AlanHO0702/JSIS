unit TmpPressDLL;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic{, WinSkinData}, JSdGrid2Excel, Menus, JSdPopupMenu, DB,
  JSdTable, ADODB, StdCtrls, Mask, DBCtrls, JSdLabel, ExtCtrls, Grids, Wwdbigrd,
  Wwdbgrid, JSdDBGrid, ComCtrls, Buttons, Wwdatsrc, JSdTreeView, ToolWin,
  ImgList, JSdMultSelect, SingleGridDLL, System.ImageList;

type
  TfrmTmpPressDLL = class(TfrmSingleGridDLL)
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
    grdRouteDtl: TJSdDBGrid;
    trvLayerPress: TJSdTreeView;
    Splitter2: TSplitter;
    tblTmpBomDtl: TADOTable;
    tblTmpBomDtlTmpId: TStringField;
    tblTmpBomDtlLayerId: TStringField;
    tblTmpBomDtlAftLayerId: TStringField;
    tblTmpBomDtlIssLayer: TIntegerField;
    tblTmpBomDtlDegree: TIntegerField;
    tblTmpBomDtlFL: TIntegerField;
    tblTmpBomDtlEL: TIntegerField;
    dsTmpBomDtl: TwwDataSource;
    Panel2: TPanel;
    btnChangeRoute: TSpeedButton;
    tblTmpDtlTmpId: TStringField;
    tblTmpDtlLayerId: TStringField;
    tblTmpDtlSerialNum: TWordField;
    tblTmpDtlBefLayer: TStringField;
    tblTmpDtlMatClass: TStringField;
    tblTmpDtlmatcode: TStringField;
    tblTmpDtlMatName: TWideStringField;
    tblTmpDtlNotes: TStringField;
    tblTmpDtlClassName: TWideStringField;
    qryMatClass: TADOQuery;
    tblTmpBomDtlSort: TIntegerField;
    tblTmpBomDtlLayerName: TWideStringField;
    btChange: TSpeedButton;
    btSaveAs: TSpeedButton;
    Bevel3: TBevel;
    dbnBrowse: TDBNavigator;
    pnlSave: TPanel;
    Label1: TJSdLabel;
    edtTmpIdNew: TEdit;
    Panel3: TPanel;
    Panel4: TPanel;
    pnl_Count2: TPanel;
    pnl_CountDtl: TPanel;
    Panel5: TPanel;
    btnArrowDtl: TSpeedButton;
    btnArrowMas: TSpeedButton;
    btnSaveHeight: TSpeedButton;
    pnlBtnGroup: TPanel;
    qryDtlBOMName: TADOQuery;
    tblTmpDtlFullLayerId: TStringField;
    tblTmpDtlLayerName: TWideStringField;
    procedure btnGetParamsClick(Sender: TObject);
    procedure btChangeClick(Sender: TObject);
    procedure btnOKClick(Sender: TObject);
    //procedure BrowseData(tblDset: TDataset; bRefresh: Boolean);
    function IIFString(bYes:Boolean; str1, str2: WideString): WideString;
    procedure btSaveAsClick(Sender: TObject);
    procedure tblTmpMasBeforeDelete(DataSet: TDataSet);
    procedure tblTmpMasAfterScroll(DataSet: TDataSet);
    procedure trvLayerPressChanging(Sender: TObject; Node: TTreeNode;
      var AllowChange: Boolean);
    procedure btnChangeRouteClick(Sender: TObject);
    procedure tblTmpMasTmpBOMIdValidate(Sender: TField);
    procedure grdRouteDtlEnter(Sender: TObject);
    procedure grdDataEnter(Sender: TObject);
    procedure tblTmpBomDtlCalcFields(DataSet: TDataSet);
    procedure tblTmpMasAfterClose(DataSet: TDataSet);
    procedure tblTmpDtlAfterClose(DataSet: TDataSet);
    procedure tblTmpDtlAfterScroll(DataSet: TDataSet);
    procedure btnUpdateClick(Sender: TObject);
    procedure btnSaveHeightClick(Sender: TObject);
    procedure tblTmpMasBeforePost(DataSet: TDataSet);
    procedure btnBrowseClick(Sender: TObject);
    procedure btnC1Click(Sender: TObject);
    procedure tblTmpDtlBeforePost(DataSet: TDataSet);
    procedure tblTmpMasAfterPost(DataSet: TDataSet);
  private
    procedure OpenBOMDetail(sLayer: String);
    { Private declarations }
  public
    //2012.04.12 Timeout Fail
    var iTimeOut:Integer;
    var iActiveType: Integer;
    CurrLayer: string;
    iChangeLayerName: Integer;
    procedure Add2Filter(var sFilter  :string; const sCondition :string);
    { Public declarations }
  end;

var
  frmTmpPressDLL: TfrmTmpPressDLL;

implementation

uses TmpPressSet, TmpBOMSelect, unit_DLL;

{$R *.dfm}

procedure TfrmTmpPressDLL.btChangeClick(Sender: TObject);
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
        MsgDlgJS('請先建立樣板主檔!', mtError, [mbOk], 0);
        exit;
     end;
     if fieldbyname('Status').AsInteger = 1 then
     begin
        if iActiveType=1 then
          MsgDlgJS('壓合模型已審核，不可修改!', mtError, [mbOk], 0)
        else
          MsgDlgJS('壓合模型已使用，不可修改!', mtError, [mbOk], 0);
        exit;
     end;
  end;
  Application.CreateForm(TdlgTmpPressSet, dlgTmpPressSet);
  dlgTmpPressSet.sConnectStr:=sConnectStr;
  dlgTmpPressSet.prcDoSetConnOCX;
//  try
    with dlgTmpPressSet do
    begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryMatClass.CommandTimeout:=iTimeOut;
        qryTmpPressDtl.CommandTimeout:=iTimeOut;
        qryProdLayer.CommandTimeout:=iTimeOut;
      end;
      qryProdLayer.Close;
      qryProdLayer.Open;
      with qryTmpPressDtl do
      begin
         Close;
         Parameters.ParamByName('TmpId').Value:= tblTmpMas.FieldbyName('TmpId').AsString;
         Parameters.ParamByName('LayerId').Value:= CurrLayer;
         Open;
      end;
      qryMatClass.Close;
      qryMatClass.Open;
      msSelects.Setup(slAll);
      if iChangeLayerName<>1 then
      begin
        msSelects.SourceColumns[3].Width:=0;
        msSelects.TargetColumns[3].Width:=0;
      end;
      cboLayerId.Text:= CurrLayer;
      ShowModal;
      if ModalResult = mrOk then
      begin
        with qryExec do
        begin
          qryExec.Close;
          SQL.Clear;
          SQL.Add('DELETE dbo.EMOdTmpPressDtl where TmpId='''
            +tblTmpMas.FieldbyName('TmpId').AsString+''' and LayerId='''
            +CurrLayer+'''');
          execsql;
          //InsertDtl
          for i:= 0 to msSelects.TargetItems.Count-1 do
          begin
            qryExec.Close;
            SQL.Clear;
            SQL.Add('exec EMOdTmpPressDtlIns '
              +''''+tblTmpMas.FieldbyName('TmpId').AsString+''','
              +''''+CurrLayer+''','
              +IntToStr(i+1)+','
              +''''+msSelects.TargetItems[i].subitems[0]+''','
              +''''+msSelects.TargetItems[i].subitems[1]+'''');
            execsql;
          end;
        end;
        tblTmpDtl.Close;
        tblTmpDtl.Open;
      end;
    end;
//  finally
//    dlgTmpPressSet.Free;
//  end;
         with qryExec do
         begin
            qryExec.Close;
            SQL.Clear;
            SQL.Add('exec EMOdCheckTmpPressSet '''
                  +tblTmpMas.FieldbyName('TmpId').AsString+'''');
            execsql;
         end;
end;

procedure TfrmTmpPressDLL.btnBrowseClick(Sender: TObject);
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
  tblTmpBomDtl.Close;
  tblTmpBomDtl.Open;
  tblTmpMas.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
    //btUpdate.Enabled:=False;
    //unit_DLL.prcChangeShowMode(pnl_NowMode,'BROWSE');
end;

procedure TfrmTmpPressDLL.btnC1Click(Sender: TObject);
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
  //2016.12.08 Fix
  CurrLayer:='L0~0';

  tblTmpMas.Close;
  tblTmpMas.Open;
  tblTmpMas.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
end;

procedure TfrmTmpPressDLL.btnChangeRouteClick(Sender: TObject);
var sBOMId: string;
begin
  inherited;
  if tblTmpMas.LockType=ltReadOnly then
  begin
    MsgDlgJS('請先按下修改按鈕!', mtError, [mbOk], 0);
    exit;
  end;
  if tblTmpMas.State in [dsEdit,dsInsert] then
    tblTmpMas.Post;

  Application.createForm(TdlgTmpBOMSelect, dlgTmpBOMSelect);
  dlgTmpBOMSelect.sConnectStr:=sConnectStr;
  dlgTmpBOMSelect.prcDoSetConnOCX;
  //審核機制
  with qryExec do
  begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
              +' and ParamId=''TmpRouteActiveType'' and Value=''1''');
      Open;
      if RecordCount>0 then
        dlgTmpBOMSelect.iTmpActive:=1
      else
        dlgTmpBOMSelect.iTmpActive:=0;
  end;
  with dlgTmpBOMSelect do
  begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryTmpDtl.CommandTimeout:=iTimeOut;
      end;
     qryTmpMas.lookupType:=lkJoinSQL;
     //OnCreate
     //改給搜尋按鈕觸發以發動審核機制
     //qryTmpMas.Close;
     //qryTmpMas.Open;
     dlgTmpBOMSelect.SpeedButton1Click(Sender);
     qryTmpDtl.Close;
     qryTmpDtl.Open;
     with trvBOM do
     begin
       Setup;
       FullExpand;
       if Items.count >0 then
         Select(Items.item[0]);
     end;
     //Ori
     Showmodal;
     if modalResult=mrok then
     begin
       sBomId:= qryTmpMas.FieldbyName('TmpId').AsString;
       with tblTmpMas do
       begin
         //加一個MESSAGE,以免誤將結構變更,以致所有內層資料全部被刪除
           if MsgDlgJS('確定變更結構?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
           begin
             edit;
             Fieldbyname('TmpBOMId').asstring:= sBomId;
             Post;
             tblTmpBomDtl.Close;
             tblTmpBomDtl.Open;
             with qryExec do
             begin
                qryExec.Close;
                SQL.Clear;
                SQl.Add('exec EMOdTmpPressByBOM '''
                        +tblTmpMas.FieldbyName('TmpId').AsString+'''');
                execsql;
             end;
             with trvLayerPress do
             begin
                Setup;
                FullExpand;
                if Items.count >0 then
                   Select(Items.item[0]);
             end;
           end
           else
              exit;
       end;
     end;
  end;
end;

procedure TfrmTmpPressDLL.btnGetParamsClick(Sender: TObject);
var sPaperMasterHeight, sPaperDetailHeight: String;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  inherited;
  //2011.11.21
  tblTmpMas.LookupType:=lkJoinSQL;
  btnUpdate.Align:=alRight;
  btnUpdate.Align:=alLeft;
  pnlBtnGroup.Align:=alRight;
  pnlBtnGroup.Align:=alLeft;
  //on Create
  //SetFormatDB(TCustomADODataset(tblTmpDtl), 'EMOdLayerRoute');
  pgeMaster.ActivePageIndex:= 0;
  qryMatClass.Close;
  qryMatClass.Open;
  qryDtlBOMName.Close;
  qryDtlBOMName.Open;
  tblTmpMas.Close;
  tblTmpMas.Open;
  tblTmpDtl.Close;
  tblTmpDtl.Open;
  tblTmpBomDtl.Close;
  tblTmpBomDtl.Open;
  iChangeLayerName:=0;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
          +' and ParamId=''ChangeLayerName''');
    Open;
    if FieldByName('Value').AsString='1' then
      iChangeLayerName:=1;
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
          trvLayerPress.Width:=strtoint(sPaperDetailHeight);
        except
        end;
      end;
      //if qryExec.RecordCount>0 end
    end;
  end;
  //BrowseData(tblTmpMas, True);
  //BrowseData(tblTmpDtl, True);
  with trvLayerPress do
  begin
    Setup;
    FullExpand;
    if Items.count >0 then
       Select(Items.item[0]);
  end;
  prcStoreFieldNeed_Def(self,qryExec); //for 強制大寫
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
        trvLayerPress.Width:=Round(trvLayerPress.Width * FontSize / 100);
      end;
  end;
end;

procedure TfrmTmpPressDLL.btnOKClick(Sender: TObject);
var sSQLComd: string;
begin
   inherited;
   sSQLComd:='';
   if edtTmpId.text<>'' then
   begin
      //frmMain.Add2Filter(sSQLComd,'TmpId >= '''+ edtTmpId.text+'''');
      //frmMain.Add2Filter(sSQLComd,'TmpId < '''+ edtTmpId.text+char(65535)+'''');
      Add2Filter(sSQLComd,'TmpId like ''%'+ edtTmpId.text+'%'''); //2010.02.23
   end;
   if edtNotes.text<>'' then
   begin
      //frmMain.Add2Filter(sSQLComd,'Notes >= '''+ edtNotes.text+'''');
      //frmMain.Add2Filter(sSQLComd,'Notes < '''+ edtNotes.text+char(65535)+'''');
      Add2Filter(sSQLComd,'Notes like N''%'+ edtNotes.text+'%'''); //2010.02.23
   end;
   if ((rdoStatus.itemindex>-1) and (rdoStatus.itemindex<2)) then
      Add2Filter(sSQLComd,'Status = ''' +  inttostr(rdoStatus.itemindex) + '''');

  if sSQLComd<>'' then sSQLComd:=' Where '+sSQLComd; //2010.02.23
  with tblTmpMas do
  begin
    if State in [dsEdit,dsInsert] then Post;
    Close;
    SQL.Clear;
    SQL.Add('select * from dbo.EMOdTmpPressMas'+sSQLComd);
     {filter:= sSQLComd;
     filtered:= true;
     if Active then
        refresh
     else
        open; }
     Open;
  end;
  pgeMaster.ActivePage := TabSheet1;
  tblTmpBomDtl.Close;
  tblTmpBomDtl.Open;
end;

procedure TfrmTmpPressDLL.btnSaveHeightClick(Sender: TObject);
var iMasterHeight, iDetailHeight: Integer;
begin
  inherited;
  //比照繼承
  iMasterHeight:=pgeMaster.Width;
  iDetailHeight:=trvLayerPress.Width;
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

procedure TfrmTmpPressDLL.btnUpdateClick(Sender: TObject);
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
  tblTmpBomDtl.Close;
  tblTmpBomDtl.Open;
  tblTmpMas.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);
    //btUpdate.Enabled:=False;
    //unit_DLL.prcChangeShowMode(pnl_NowMode,'UPDATE');
end;

procedure TfrmTmpPressDLL.btSaveAsClick(Sender: TObject);
var i : Integer;
    sBOMId, sNotes : String;
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
    SQL.Add('Select CNT=Count(*) From EMOdTmpPressMas(nolock) Where TmpId ='''
        +edtTmpIdNew.Text+'''');
    Open;
    If FieldByName('CNT').AsInteger >0 then
    Begin
        MsgDlgJS('已存在該代碼!', mtError, [mbOk], 0);
        exit;
    End;
  End;

  If edtTmpIdNew.Text = '' then
  Begin
    MsgDlgJS('請輸入新代碼!', mtError, [mbOk], 0);
    exit;
  End
  Else
  Begin
    sBOMId:=tblTmpMas.FieldByName('TmpBOMId').AsString;
    sNotes:=tblTmpMas.FieldByName('Notes').AsString;
    Application.CreateForm(TdlgTmpPressSet, dlgTmpPressSet);
    dlgTmpPressSet.sConnectStr:=sConnectStr;
    dlgTmpPressSet.prcDoSetConnOCX;
    try
    with dlgTmpPressSet do
    begin
      qryProdLayer.Close;
      qryProdLayer.Open;
      with qryTmpPressDtl do
      begin
         Close;
         Parameters.ParamByName('TmpId').Value:= tblTmpMas.FieldbyName('TmpId').AsString;
         Parameters.ParamByName('LayerId').Value:= CurrLayer;
         Open;
      end;
      qryMatClass.Close;
      qryMatClass.Open;
      msSelects.Setup(slAll);
      cboLayerId.Text:= CurrLayer;
      ShowModal;
      if ModalResult = mrOk then
      begin
        With qryExec do
        Begin
          qryExec.Close;
          SQL.Clear;
          SQL.Add('insert into EMOdTmpPressMas(TmpId,TmpBOMId,Notes,Status) Select '
            +''''+edtTmpIdNew.Text+''','
            +''''+sBOMId+''','
            +''''+sNotes+''', 0');
          ExecSql;

          for i:= 0 to msSelects.TargetItems.Count-1 do
          begin
            qryExec.Close;
            SQL.Clear;
            SQL.Add('exec EMOdTmpPressDtlIns '
              +''''+edtTmpIdNew.Text+''','
              +''''+CurrLayer+''','
              +IntToStr(i+1)+','
              +''''+msSelects.TargetItems[i].subitems[0]+''','
              +''''+msSelects.TargetItems[i].subitems[1]+'''');
            execsql;
          end;
        End;
        tblTmpDtl.Close;
        tblTmpBomDtl.Close;
        tblTmpMas.Close;
        tblTmpMas.Open;
        tblTmpMas.Locate('TmpId',edtTmpIdNew.Text,[loCaseInsensitive]);
        tblTmpDtl.Open;
        tblTmpBomDtl.Open;
      end;
    end;
    finally
      dlgTmpPressSet.Free;
    end;
  End;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdCheckTmpPressSet '''
            +tblTmpMas.FieldbyName('TmpId').AsString+'''');
    execsql;
  end;
end;

procedure TfrmTmpPressDLL.grdDataEnter(Sender: TObject);
begin
  inherited;
  if dbnBrowse.DataSource<>dsTmpMas then
    dbnBrowse.DataSource:=dsTmpMas;
  btnArrowMas.Visible:=(dbnBrowse.DataSource=dsTmpMas);
  btnArrowDtl.Visible:=not(btnArrowMas.Visible);
end;

procedure TfrmTmpPressDLL.grdRouteDtlEnter(Sender: TObject);
begin
  inherited;
  dbnBrowse.DataSource:=dsTmpDtl;
  btnArrowMas.Visible:=(dbnBrowse.DataSource=dsTmpMas);
  btnArrowDtl.Visible:=not(btnArrowMas.Visible);
end;

procedure TfrmTmpPressDLL.tblTmpBomDtlCalcFields(DataSet: TDataSet);
begin
  inherited;
  with tblTmpBomDtl do
  begin
    FieldByName('Sort').Value:=FieldByName('Degree').AsInteger * 100
      +FieldByName('FL').AsInteger;
  end;
end;

procedure TfrmTmpPressDLL.tblTmpDtlAfterClose(DataSet: TDataSet);
begin
  inherited;
  pnl_CountDtl.Caption:='0 /0';
end;

procedure TfrmTmpPressDLL.tblTmpDtlAfterScroll(DataSet: TDataSet);
var i:integer;
begin
  inherited;
  i:=tblTmpDtl.RecNo;
  if i<0 then i:=0;
  pnl_CountDtl.Caption:=inttostr(i)+' / '+ inttostr(tblTmpDtl.RecordCount);
end;

procedure TfrmTmpPressDLL.tblTmpDtlBeforePost(DataSet: TDataSet);
begin
  inherited;
  if trim(tblTmpDtl.FieldByName('FullLayerId').AsString)<>
        trim(tblTmpMas.FieldByName('TmpBOMId').AsString)+
        trim(tblTmpDtl.FieldByName('BefLayer').AsString) then
    tblTmpDtl.FieldByName('FullLayerId').Value:=
        trim(tblTmpMas.FieldByName('TmpBOMId').AsString)+
        trim(tblTmpDtl.FieldByName('BefLayer').AsString)
end;

procedure TfrmTmpPressDLL.tblTmpMasAfterClose(DataSet: TDataSet);
begin
  inherited;
  pnl_Count2.Caption:='0 /0';
end;

procedure TfrmTmpPressDLL.tblTmpMasAfterPost(DataSet: TDataSet);
var sOriTmpId: String;
begin
    sOriTmpId:=tblTmpMas.FieldByName('TmpId').AsString;
  inherited;
   tblTmpMas.Locate('TmpId', sOriTmpId, [loCaseInsensitive]);   //2024.09.27游標
end;

procedure TfrmTmpPressDLL.tblTmpMasAfterScroll(DataSet: TDataSet);
var i:integer;
begin
  inherited;
  trvLayerPress.setup;
  trvLayerPress.FullExpand;
  OpenBOMDetail(CurrLayer);
  i:=tblTmpMas.RecNo;
  if i<0 then i:=0;
  pnl_Count2.Caption:=inttostr(i)+' / '+ inttostr(tblTmpMas.RecordCount);
end;

procedure TfrmTmpPressDLL.tblTmpMasBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  if DataSet.FieldByName('Status').AsInteger = 1 then
  begin
    if iActiveType=1 then
      MsgDlgJS('壓合模型已審核，不可異動!', mtError, [mbOk], 0)
    else
      MsgDlgJS('壓合模型已使用，不可刪除!', mtError, [mbOk], 0);
    tblTmpMas.Cancel;
    abort;
  end;
end;

procedure TfrmTmpPressDLL.tblTmpMasBeforePost(DataSet: TDataSet);
begin
  inherited;
  if tblTmpMas.FieldByName('TmpBOMId').AsString='' then
    tblTmpMas.FieldByName('TmpBOMId').AsString:='----';
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
            +''''+ sUserId +''',''EMOdTmpPressMas''');
        ExecSql;
      end;
    end;
  end;
end;

procedure TfrmTmpPressDLL.tblTmpMasTmpBOMIdValidate(Sender: TField);
begin
  inherited;
  trvLayerPress.setup;
  trvLayerPress.FullExpand;
end;

procedure TfrmTmpPressDLL.trvLayerPressChanging(Sender: TObject;
  Node: TTreeNode; var AllowChange: Boolean);
begin
  inherited;
  CurrLayer:= TNodeData(Node.Data^).Id;
  OpenBOMDetail(CurrLayer);
end;

procedure TfrmTmpPressDLL.OpenBOMDetail(sLayer: String);
var sSQLComd: string;
begin
  inherited;
  sSQLComd:='';
  Add2Filter(sSQLComd, 'TmpId ='''+ trim(tblTmpMas.FieldByName('TmpId').AsString)+'''');
  Add2Filter(sSQLComd, 'LayerId ='''+ sLayer+'''');
  tblTmpDtl.Close;
  with tblTmpDtl do
  begin
     //filter:= sSQLComd;
     //filtered:= true;
     SQL.Clear;
     SQL.Add('select t1.* from EMOdTmpPressDtl t1'
            +' where t1.TmpId= :TmpId'
            +' And '+sSQLComd);
     //Open;
  end;
  tblTmpDtl.Open;
end;

procedure TfrmTmpPressDLL.Add2Filter(var sFilter  :string; const sCondition :string);
begin
   sFilter := sFilter + IIFString(sFilter='', '', ' AND ') + sCondition;
end;

function TfrmTmpPressDLL.IIFString(bYes:Boolean; str1, str2: WideString): WideString;
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

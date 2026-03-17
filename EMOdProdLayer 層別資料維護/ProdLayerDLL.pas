unit ProdLayerDLL;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, PaperOrgDLL{, WinSkinData}, JSdGrid2Excel, Menus, JSdPopupMenu, DB,
  JSdTable, ADODB, StdCtrls, Mask, DBCtrls, JSdLabel, ExtCtrls, Grids, Wwdbigrd,
  Wwdbgrid, JSdDBGrid, ComCtrls, Buttons, Wwdatsrc, JSdLookupCombo,
  ShellAPI;

{type
    TevCusBtnClickEvent2 = class
      oldCusBtnClickEvent2: TNotifyEvent;
      procedure prcCusBtnClick2(sender: TObject);
    end; }

type
  TfrmProdLayerDLL = class(TfrmPaperOrgDLL)
    pnlPartNum: TPanel;
    JSdLabel1: TJSdLabel;
    pgeFormType: TPageControl;
    tstMain: TTabSheet;
    tstSub: TTabSheet;
    edtPnum: TDBEdit;
    edtRevision: TDBEdit;
    edtLayerId: TDBEdit;
    Splitter2: TSplitter;
    pnlRouteTools: TPanel;
    btnChangeProc: TSpeedButton;
    btnBackupNotes: TSpeedButton;
    btnPasteNotes: TSpeedButton;
    btnNoteStyleTree: TSpeedButton;
    btnRouteChange: TSpeedButton;
    panRoute: TPanel;
    DBNavigator2: TDBNavigator;
    edtTmpPressId: TDBEdit;
    qryNotesIN: TADOQuery;
    qryNotesOUT: TADOQuery;
    dsLayerBOM: TDataSource;
    tblLayerBOM: TJSdTable;
    Splitter3: TSplitter;
    btnProcBOMSet: TSpeedButton;
    qryProdLayerBomdel: TADOQuery;
    qryProcCodeBOMSet: TADOQuery;
    btExit: TSpeedButton;
    Panel1: TPanel;
    dbgBOM: TJSdDBGrid;
    DBMemo1: TDBMemo;
    Splitter8: TSplitter;
    pnlPath: TPanel;
    Splitter4: TSplitter;
    附件: TJSdLabel;
    DBEdit1: TDBEdit;
    btnCMap: TSpeedButton;
    btnCMapOpen: TSpeedButton;
    OpenDialog1: TOpenDialog;
    btnMapOpenTest: TSpeedButton;
    DBEdit2: TDBEdit;
    pnlRouteNote: TPanel;
    pnlNoteSep: TPanel;
    Splitter5: TSplitter;
    lblLayerIdBack: TLabel;
    lblRevisionBack: TLabel;
    lblPartNumBack: TLabel;
    lblParam: TLabel;
    sclPnlMaster: TScrollBar;
    lblCusId: TLabel;
    lblNavSource: TLabel;
    TabSheetMas: TTabSheet;
    pnlCopyMas: TPanel;
    sclPnlMaster2: TScrollBar;
    qryPage: TADOQuery;
    pnlUseNotes: TPanel;
    btnUseNotes: TSpeedButton;
    chkUseNotes: TDBCheckBox;
    //procedure btnGetParamsClick(Sender: TObject);
    procedure pgeFormTypeChange(Sender: TObject);
    procedure btnGetParamsClick(Sender: TObject);
    procedure btnChangeProcClick(Sender: TObject);
    procedure btnNoteStyleTreeClick(Sender: TObject);
    procedure btnBackupNotesClick(Sender: TObject);
    procedure btnPasteNotesClick(Sender: TObject);
    procedure btnRouteChangeClick(Sender: TObject);
    procedure btnProcBOMSetClick(Sender: TObject);
    procedure tblLayerBOMBeforeEdit(DataSet: TDataSet);
    procedure tblLayerBOMAfterEdit(DataSet: TDataSet);
    procedure btnUpdateClick(Sender: TObject);
    procedure qryDetail1AfterEdit(DataSet: TDataSet);
    procedure btExitClick(Sender: TObject);
    procedure qryDetail4AfterPost(DataSet: TDataSet);
    procedure qryDetail1BeforeDelete(DataSet: TDataSet);
    procedure qryDetail1AfterInsert(DataSet: TDataSet);
    procedure qryDetail1BeforeInsert(DataSet: TDataSet);
    procedure btnCMapClick(Sender: TObject);
    procedure btnCMapOpenClick(Sender: TObject);
    procedure btnMapOpenTestClick(Sender: TObject);
    procedure dbgBOMColEnter(Sender: TObject);
    procedure gridDetail1Enter(Sender: TObject);
    procedure btnSaveHeightClick(Sender: TObject);
    procedure btnKeepStatusClick(Sender: TObject);
    procedure qryDetail1AfterPost(DataSet: TDataSet);
    procedure qryBrowseAfterPost(DataSet: TDataSet);
    procedure btnViewClick(Sender: TObject);
    procedure sclPnlMasterScroll(Sender: TObject; ScrollCode: TScrollCode;
      var ScrollPos: Integer);
    procedure btnC1Click(Sender: TObject);
    procedure pgeDetailChange(Sender: TObject);
    procedure gridDetail3ColExit(Sender: TObject);
    procedure pnl_PaperOrgTopRecCounterDblClick(Sender: TObject); override;
    procedure btnUseNotesClick(Sender: TObject); //2012.10.24
    procedure nav2BeforeAction(Sender: TObject; Button: TNavigateBtn);
  private
    procedure ParseFormatW(sInput, sDim: WideString;
      var sParam: array of WideString; iPara: integer);
    { Private declarations }
  public
    //2012.04.12 Timeout Fail
    var iTimeOut, iScrollHeight :Integer;
    procedure AuditCheck;
    procedure LockButton(bStatus:Boolean);
    procedure UpdateDesigner; //2012.03.08
    procedure SetScroll; //2012.04.26
    procedure SetFieldParent; //2012.05.08
    procedure JSdLookupComboSubEnter(Sender: TObject);
    procedure prcEMODetailSet; //2012.10.24 add
    var iStatus, iNeedRefresh, iNeedChkDesigner {2012.03.08}:Integer;
        sSeahchStr, sDesignerPN, sDesignerRev {2012.03.08}:String;
        iDtlActivePage :Integer;
        iBefInsItem, iInsUseNav :Integer; //2013.04.15 add for MUT
    { Public declarations }
  end;

var
  frmProdLayerDLL: TfrmProdLayerDLL;

implementation

uses TmpRouteSelect, RouteInsNew, LayerRouteSet, unit_DLL;

{$R *.dfm}

{procedure TevCusBtnClickEvent2.prcCusBtnClick2(sender: TObject);
begin
  frmJourPaper.prcCustBtnRun(TSpeedButton(Sender).Name);
end;}

procedure TfrmProdLayerDLL.pgeDetailChange(Sender: TObject);
var iPage:Integer;
begin
  inherited;
  //2012.10.24 頁籤序的對應改由 qryPage 決定,繼承重拉
  qryPage.Locate('SerialNum',pgeDetail.ActivePageIndex+1,[loCaseInsensitive]);
  iPage:=qryPage.FieldByName('KindItem').AsInteger;
  if iPage=0 then
  BEGIN
      nav1.DataSource:=dsBrowse;
      nav2.DataSource:=dsBrowse;
  END
  ELSE
  BEGIN
      if FindComponent('dsDetail'+inttostr(iPage))<>nil then
      begin
        if sNowMode='UPDATE' then
        begin
          nav1.DataSource:=
            TDataSource(
              FindComponent('dsDetail'+inttostr(iPage))
              );
        end;

        nav2.DataSource:=
            TDataSource(
              FindComponent('dsDetail'+inttostr(iPage))
              );
      end;
  END;
end;

procedure TfrmProdLayerDLL.pgeFormTypeChange(Sender: TObject);
var sNowPart, sNowRev, sNowLayer: String;
begin
  inherited;
  if pgeFormType.ActivePage=tstMain then
  begin
    pgeDetail.Align:=alRight;
    pgeDetail.Visible:=False;
    pgeMaster.Visible:=True;
    pgeMaster.Align:=alClient;
    if iNeedRefresh=1 then
    begin
      sNowPart:=dsBrowse.DataSet.FieldByname('PartNum').AsString;
      sNowRev:=dsBrowse.DataSet.FieldByname('Revision').AsString;
      sNowLayer:=dsBrowse.DataSet.FieldByname('LayerId').AsString;
      qryBrowse.Close;
      qryBrowse.Open;
      qryBrowse.Locate('PartNum;revision;LayerId' ,
        VarArrayOf([sNowPart,sNowRev,sNowLayer]),[loPartialKey]);
      iNeedRefresh:=0;
    end;
  end
  else
  begin
    pgeMaster.Align:=alRight;
    pgeMaster.Visible:=False;
    pgeDetail.Visible:=True;
    pgeDetail.Align:=alClient;
  end;
  pgeDetailChange(Sender);
end;

procedure TfrmProdLayerDLL.pnl_PaperOrgTopRecCounterDblClick(Sender: TObject);
begin
  inherited;
  //2012.10.24 這時候 qryExec 已經連線，Pnl畫面還沒有開始產生
  TabSheetMas.TabVisible:=False;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''+
      ' and ParamId=''CombineMas'' and IsNull(Value,'''')<>''''');
    Open;
    if RecordCount>0 then
    begin
      TabSheetMas.TabVisible:=True;
      TabSheetMas.Caption:=FieldByName('Value').AsString;
      pnlMaster1.Name:='pnlMaster1_Ori';
      pnlCopyMas.Name:='pnlMaster1';
      tstMain.TabVisible:=False;
      tstSub.TabVisible:=False;
    end;
    qryExec.Close;
  end;
end;

procedure TfrmProdLayerDLL.qryBrowseAfterPost(DataSet: TDataSet);
begin
  inherited;
  //2012.03.08 add
  UpdateDesigner;
end;

procedure TfrmProdLayerDLL.qryDetail1AfterEdit(DataSet: TDataSet);
begin
  inherited;
  nav2.DataSource:=dsDetail1;
end;

procedure TfrmProdLayerDLL.qryDetail1BeforeDelete(DataSet: TDataSet);
var sSQL:string;
begin
  inherited;
  if ((TJSdTable(DataSet).FindField('Item')=nil)
        or
      (TJSdTable(DataSet).FindField('PaperNum')=nil)) then
  begin
    sSQL:='exec EMOdDLLdoDelete '+''''+TJSdTable(DataSet).TableName+''''+','+
    ''''+qryBrowse.FieldByName('PartNum').AsString+''''+','+
    ''''+qryBrowse.FieldByName('Revision').AsString+''''+','+
    ''''+qryBrowse.FieldByName('LayerId').AsString+''''+','+
    DataSet.FieldByName('SerialNum').AsString;
    unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);
    DataSet.Close;
    DataSet.Open;
    abort;
  end;
end;

procedure TfrmProdLayerDLL.qryDetail1BeforeInsert(DataSet: TDataSet);
begin
  inherited;
  if DataSet.FindField('Item')=nil then
    DataSet.Tag:=GetMaxSerialNumDLL(DataSet, 'SerialNum')+1;

  //2013.04.15 add for MUT
  //----------------------------------------------------------------------------
  if TJSdTable(DataSet).TableName='EMOdLayerHole' then
  begin
    if not (DataSet.FindField('DecSerial')=nil) then
      iBefInsItem :=DataSet.fieldbyname('DecSerial').AsInteger;
  end;
  //----------------------------------------------------------------------------
end;

procedure TfrmProdLayerDLL.qryDetail1AfterInsert(DataSet: TDataSet);
var bReadOnly:boolean;
    iSe: Integer;
    iMax: Integer;//2013.04.15
begin
  inherited;
  if DataSet.FindField('Item')=nil then
  begin
    bReadOnly:=DataSet.fieldbyname('SerialNum').ReadOnly;

    if bReadOnly then
      DataSet.fieldbyname('SerialNum').ReadOnly:=false;

    DataSet.fieldbyname('SerialNum').Asinteger:= DataSet.Tag;

    if bReadOnly then
      DataSet.fieldbyname('SerialNum').ReadOnly:=true;
  end;

  //2012.08.17 add =============================================================
  if TJSdTable(DataSet).TableName='EMOdLayerHole' then
  begin
    if not (DataSet.FindField('Item')=nil) then //CMT 開打 Item 當刀序
    begin
      //這時 SerialNum 會失去取號功能
      with qryExec do
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('select SerialNum=Max(SerialNum)+1 from EMOdLayerHole(nolock) '
            +'where PartNum='''+qryBrowse.FieldByName('PartNum').AsString+''''
            +'and Revision='''+qryBrowse.FieldByName('Revision').AsString+''''
            +'and LayerId='''+qryBrowse.FieldByName('LayerId').AsString+'''');
        Open;
        if RecordCount=0 then
          iSe:=1
        else
          iSe:=FieldByName('SerialNum').AsInteger;

        bReadOnly:=DataSet.fieldbyname('SerialNum').ReadOnly;
        if bReadOnly then
          DataSet.fieldbyname('SerialNum').ReadOnly:=false;
        DataSet.fieldbyname('SerialNum').Asinteger:= iSe;

        if bReadOnly then
          DataSet.fieldbyname('SerialNum').ReadOnly:=true;
      end;
    end;

    //2013.04.15 add DecSerial for MUT
    //--------------------------------------------------------------------------
    if not (DataSet.FindField('DecSerial')=nil) then
    begin
      with qryExec do
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('select DecSerial=Max(DecSerial) from EMOdLayerHole(nolock) '
            +'where PartNum='''+qryBrowse.FieldByName('PartNum').AsString+''''
            +'and Revision='''+qryBrowse.FieldByName('Revision').AsString+''''
            +'and LayerId='''+qryBrowse.FieldByName('LayerId').AsString+'''');
        Open;
        iMax:=FieldByName('DecSerial').AsInteger;
      end;
      bReadOnly:=DataSet.fieldbyname('DecSerial').ReadOnly;
      if bReadOnly then
        DataSet.fieldbyname('DecSerial').ReadOnly:=false;

      if iBefInsItem>0 then
      begin
        if ((iMax=iBefInsItem) and (iInsUseNav=0)) then
          DataSet.fieldbyname('DecSerial').AsFloat:= iBefInsItem + 1
        else
          DataSet.fieldbyname('DecSerial').AsFloat:= iBefInsItem - 0.5;
      end
      else
      begin
        DataSet.fieldbyname('DecSerial').AsFloat:= 1;
      end;
      iInsUseNav:=0;

      if bReadOnly then
        DataSet.fieldbyname('DecSerial').ReadOnly:=true;
    end;
    //--------------------------------------------------------------------------
  end;
  //============================================================================
end;

procedure TfrmProdLayerDLL.nav2BeforeAction(Sender: TObject;
  Button: TNavigateBtn);
begin
  inherited;
  if nav2.DataSource.DataSet is TJSdTable then
  begin
    if TJSdTable(nav2.DataSource.DataSet).TableName='EMOdLayerHole' then
    begin
      iInsUseNav:=0;
      if Button=nbInsert then
        iInsUseNav:=1;
    end;
  end;
end;

procedure TfrmProdLayerDLL.qryDetail1AfterPost(DataSet: TDataSet);
begin
  inherited;
  //2012.03.08 add
  UpdateDesigner;
end;

procedure TfrmProdLayerDLL.qryDetail4AfterPost(DataSet: TDataSet);
begin
  inherited;
  if qryDetail4.TableName='EMOdLayerHole' then
    iNeedRefresh:=1;//因為有總孔數更新需求
  //2012.03.08 add
  UpdateDesigner;
end;

procedure TfrmProdLayerDLL.btnProcBOMSetClick(Sender: TObject);
var i: integer;
    //NewString: string;
    //ClickedOK: Boolean;
    iRange: Integer;
begin
  inherited;
  if qryDetail1.RecordCount <=0 then
    exit;

   AuditCheck;
   Application.createForm(TdlgLayerRouteSet, dlgLayerRouteSet);
   dlgLayerRouteSet.sConnectStr:=sConnectStr;
   dlgLayerRouteSet.prcDoSetConnOCX;
   //2011.09.28
   with qryExec do
   begin
     qryExec.Close;
     SQL.Clear;
     SQL.Add('select Value from CURdSysParams(nolock) '
		   +'where SystemId=''EMO'' and ParamId=''MatClassSelDefault''');
     Open;
     if FieldByName('Value').AsString<>'' then
     begin
       dlgLayerRouteSet.cboClassMat.visible:=True;
       dlgLayerRouteSet.cboClassMat.Text:=FieldByName('Value').AsString;
       dlgLayerRouteSet.sClassMat:=FieldByName('Value').AsString;
     end;
   end;

   with dlgLayerRouteSet do
   begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryProdPressMat.CommandTimeout:=iTimeOut;
        qryLayerPress.CommandTimeout:=iTimeOut;
        qryProdLayer.CommandTimeout:=iTimeOut;
        qry2MatClass.CommandTimeout:=iTimeOut;
        qryClassMat.CommandTimeout:=iTimeOut;
      end;
    qryProdLayer.Close;
    qryProdLayer.Open;
    qry2MatClass.Close;
    qry2MatClass.Parameters.ParamByName('ClassMat').Value:=sClassMat;
    qry2MatClass.Open;
    qryClassMat.Close;
    qryClassMat.Open;
    //Create
    iRange:=qry2MatClass.RecordCount div 4;
    if iRange>3 then
    begin
      dlgLayerRouteSet.Height:= dlgLayerRouteSet.Height+(13*(iRange-3));
      pnlTOP.Height:=pnlTOP.Height+(13*(iRange-3));
      rdoMatClass.Height:=rdoMatClass.Height+(13*(iRange-3));
    end;
    //Create end

    BPartNum:= qryDetail1.FieldByname('PartNum').asstring;
    BRevision:= qryDetail1.FieldByname('Revision').asstring;
    BLayerId:= qryDetail1.FieldByname('LayerId').AsString;
    BProcCode:= qryDetail1.FieldByname('ProcCode').AsString;
    dlgLayerRouteSet.btFindClick(Sender);
    Showmodal;
      if modalResult=mrok then
      begin
         with qryProdLayerBomdel do
         begin
            Close;
            Parameters.Parambyname('PartNum').Value:= dsDetail1.DataSet.FieldByname('PartNum').asstring;
            Parameters.Parambyname('Revision').Value:= dsDetail1.DataSet.FieldByname('Revision').asstring;
            Parameters.ParambyName('LayerId').Value:= dsDetail1.DataSet.FieldByname('LayerId').AsString;
            Parameters.ParambyName('ProcCode').Value:= dsDetail1.DataSet.FieldByname('ProcCode').AsString;
            execsql;
         end;
         with qryProcCodeBOMSet do
         begin
            for i:= 0 to msSelects.TargetItems.Count-1 do
            begin
               Parameters.Parambyname('PartNum').Value:= dsDetail1.DataSet.FieldByname('PartNum').asstring;
               Parameters.Parambyname('Revision').Value:= dsDetail1.DataSet.FieldByname('Revision').asstring;
               Parameters.ParambyName('LayerId').Value:= dsDetail1.DataSet.FieldByname('LayerId').AsString;
               Parameters.ParambyName('ProcCode').Value:= dsDetail1.DataSet.FieldByname('ProcCode').AsString;
               Parameters.ParamByName('SerialNum').Value := i+1;
               Parameters.ParamByName('MatCode').Value:= msSelects.TargetItems[i].Caption;
               Parameters.ParamByName('MatName').Value:= msSelects.TargetItems[i].SubItems[0];
               execsql;
            end;
         end;
         //2012.03.08
         UpdateDesigner;
      end;
   end;
   tblLayerBOM.close;
   tblLayerBOM.open;
end;

procedure TfrmProdLayerDLL.btnBackupNotesClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
  with qryNotesIN do
  begin
    Parameters.Parambyname('PartNum').Value:= dsBrowse.DataSet.FieldByname('PartNum').AsString;
    Parameters.Parambyname('Revision').Value:= dsBrowse.DataSet.FieldByname('Revision').AsString;
    Parameters.Parambyname('LayerId').Value:= dsBrowse.DataSet.FieldByname('LayerId').AsString;
    execsql;
  end;
  MsgDlgJS('已儲存備註完畢',mtInformation,[mbOK],0);
end;

procedure TfrmProdLayerDLL.btnChangeProcClick(Sender: TObject);
var TmpRouteId:String;
    sNowPart, sNowRev, sNowLayer: String;
begin
  inherited;
  AuditCheck;
  sNowPart:=dsBrowse.DataSet.FieldByname('PartNum').AsString;
  sNowRev:= dsBrowse.DataSet.FieldByname('Revision').AsString;
  sNowLayer:= dsBrowse.DataSet.FieldByname('LayerId').AsString;

   TmpRouteId :=dsBrowse.DataSet.FieldByname('TmpRouteId').AsString;
   Application.createForm(TdlgTmpRouteSelect, dlgTmpRouteSelect);
   dlgTmpRouteSelect.sConnectStr:=sConnectStr;
   dlgTmpRouteSelect.prcDoSetConnOCX;
   //審核機制
   with qryExec do
   begin
       qryExec.Close;
       SQL.Clear;
       SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
              +' and ParamId=''TmpRouteActiveType'' and Value=''1''');
       Open;
       if RecordCount>0 then
         dlgTmpRouteSelect.iTmpActive:=1
       else
         dlgTmpRouteSelect.iTmpActive:=0;
   end;
   with dlgTmpRouteSelect do
   begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryMas.CommandTimeout:=iTimeOut;
        qryTmpIns.CommandTimeout:=iTimeOut;
        qryTmpDel.CommandTimeout:=iTimeOut;
        qryTmpDelAll.CommandTimeout:=iTimeOut;
        qryProcBasic.CommandTimeout:=iTimeOut;
        qryTmpMas.CommandTimeout:=iTimeOut;
        qryTmpDtl.CommandTimeout:=iTimeOut;
        qryDtl.CommandTimeout:=iTimeOut;
      end;
      //Create
      qryProcBasic.Close;
      qryProcBasic.Open;
      CurrPartNum:= dsBrowse.DataSet.FieldByname('PartNum').AsString;
      CurrRevision:= dsBrowse.DataSet.FieldByname('Revision').AsString;

      With qryTmp do
      Begin
        Parameters.ParamByName('PartNum').Value:= CurrPartNum;
        Parameters.ParamByName('Revision').Value:= CurrRevision;
      End;

      qryTmpDtl.Close;
      qryTmpDtl.Open;
      qryMas.Close;
      qryMas.Open;
      qryDtl.Close;
      qryDtl.Open;
      qryTmp.Close;
      qryTmp.Open;
      qryTmpDtl2.Close;
      qryTmpDtl2.Open;
      //Create end
      dlgTmpRouteSelect.btnSearchClick(Sender);

      pgeMaster.ActivePageIndex := 0;
      pgeDtl.ActivePageIndex := iDtlActivePage;
      pgeMaster.Pages[1].TabVisible := false;
      Showmodal;
      if modalResult=mrok then
      begin
         //變更途程
         with qryExec do
         begin
           qryExec.Close;
           SQL.Clear;
           SQL.Add('exec EMOdInsLayerRoute '''+sNowPart+''','''+sNowRev+''','''
              +sNowLayer+''','''+qryTmpMas.FieldByname('TmpId').AsString+'''');
           Open;
         end;
         //2012.03.08
         UpdateDesigner;

         qryBrowse.Close;
         qryBrowse.Open;
         qryDetail1.Close;
         qryDetail1.Open;
         qryBrowse.Locate('PartNum;revision;LayerId' ,
                      VarArrayOf([sNowPart,sNowRev,sNowLayer]) ,[loPartialKey]);
      end;
   end;
   //qryBrowse.Refresh;
end;

procedure TfrmProdLayerDLL.btnPasteNotesClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
  with qryNotesOUT do
  begin
    Parameters.Parambyname('PartNum').Value:= dsBrowse.DataSet.FieldByname('PartNum').AsString;
    Parameters.Parambyname('Revision').Value:= dsBrowse.DataSet.FieldByname('Revision').AsString;
    Parameters.Parambyname('LayerId').Value:= dsBrowse.DataSet.FieldByname('LayerId').AsString;
    execsql;
  end;
  //2012.03.08
  UpdateDesigner;

  qryDetail1.Refresh;
  MsgDlgJS('已還原備註完畢',mtInformation,[mbOK],0);
end;

procedure TfrmProdLayerDLL.btnMapOpenTestClick(Sender: TObject);
begin
  inherited;
  showmessage(IntToStr(qryBrowse.FieldByName('ProcRemark').Size));
end;

procedure TfrmProdLayerDLL.btnNoteStyleTreeClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
   Application.createForm(TdlgTmpRouteSelect, dlgTmpRouteSelect);
   dlgTmpRouteSelect.sConnectStr:=sConnectStr;
   dlgTmpRouteSelect.prcDoSetConnOCX;
   //審核機制
   with qryExec do
   begin
       qryExec.Close;
       SQL.Clear;
       SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
              +' and ParamId=''TmpRouteActiveType'' and Value=''1''');
       Open;
       if RecordCount>0 then
         dlgTmpRouteSelect.iTmpActive:=1
       else
         dlgTmpRouteSelect.iTmpActive:=0;
   end;
   with dlgTmpRouteSelect do
   begin
      //Create
      qryProcBasic.Close;
      qryProcBasic.Open;
      CurrPartNum:= dsBrowse.DataSet.FieldByname('PartNum').AsString;
      CurrRevision:= dsBrowse.DataSet.FieldByname('Revision').AsString;

      With qryTmp do
      Begin
        Parameters.ParamByName('PartNum').Value:= CurrPartNum;
        Parameters.ParamByName('Revision').Value:= CurrRevision;
      End;

      qryTmpDtl.Close;
      qryTmpDtl.Open;
      qryMas.Close;
      qryMas.Open;
      qryDtl.Close;
      qryDtl.Open;
      qryTmp.Close;
      qryTmp.Open;
      qryTmpDtl2.Close;
      qryTmpDtl2.Open;
      //Create end
      dlgTmpRouteSelect.btnSearchClick(Sender);

      pgeMaster.ActivePageIndex := 1;
      pgeMaster.Pages[0].TabVisible := false;
      Showmodal;
      if modalResult=mrOK then
      begin
         //新增途程備註
         With qryExec do
         Begin
           qryExec.Close;
           SQL.Clear;
           SQL.Add('Exec EMOdProcNotesInsert '''+
              dsBrowse.DataSet.FieldByname('PartNum').AsString+''', '''+
              dsBrowse.DataSet.FieldByname('Revision').AsString+''', '''+
              dsBrowse.DataSet.FieldByname('LayerId').AsString+'''');
           Execsql;
         End;
         //2012.03.08
         UpdateDesigner;

         qryDetail1.Close;
         qryDetail1.Open;
         With qryTmpDelAll do
         Begin
           Parameters.Parambyname('PartNum').Value := dsBrowse.DataSet.FieldByname('PartNum').AsString;
           Parameters.Parambyname('Revision').Value:= dsBrowse.DataSet.FieldByname('Revision').AsString;
           Execsql;
         End;
      end;
    end;
end;

procedure TfrmProdLayerDLL.tblLayerBOMAfterEdit(DataSet: TDataSet);
begin
  inherited;
  nav2.DataSource:= dsLayerBOM;
end;

procedure TfrmProdLayerDLL.tblLayerBOMBeforeEdit(DataSet: TDataSet);
begin
  inherited;
  AuditCheck;
end;

procedure TfrmProdLayerDLL.btExitClick(Sender: TObject);
begin
  inherited;
  //若在主檔呼叫之下，直接關閉，只關掉自己，承載的LinkShowDLL 沒有關掉
  Close;
end;

procedure TfrmProdLayerDLL.btnC1Click(Sender: TObject);
var sTableType, sUseField:String;
begin
  //2012.05.11 add
  //Test ShowMessage(qryDetail3.FieldByName('SerialNum').AsString);
  //============================================================================
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select SpName from CURdOCXItemCustButton(nolock) where ItemId='''
          +sItemId+''' and ButtonName='''+TSpeedButton(Sender).Name+''' '
          +'and SpName=''EMOdInsLayerHoleSumRow''');
    Open;
    if RecordCount>0 then
    begin
    //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
      qryExec.Close;
      SQL.Clear;
      SQL.Add('select TableKind from CURdOCXTableSetUp(nolock) '
          +'where TableName=''EMOdLayerHole'' '
          +'and ItemId='''+sItemId+''' ');
      Open;
      sTableType:='';
      sTableType:=FieldByName('TableKind').AsString;
      if ((sTableType<>'')
          and
          (Self.FindComponent('qry'+sTableType)<>nil)) then
      begin
        sTableType:='qry'+sTableType;
        //2012.08.17
        qryExec.Close;
        SQL.Clear;
        SQL.Add('select FieldName from CURdTableField(nolock) where '
          +'TableName=''EMOdLayerHole'' and FieldName=''Item'' and Visible=1');
        Open;
        if RecordCount>0 then
          sUseField:='Item'
        else
          sUseField:='SerialNum';

        qryExec.Close;
        SQL.Clear;
        SQL.Add('exec EMOdGetLayerHoleSpecial 0,'
          +''''+qryBrowse.FieldByName('PartNum').asString+''','
          +''''+qryBrowse.FieldByName('Revision').asString+''','
          +''''+qryBrowse.FieldByName('LayerId').asString+''','
          +IntToStr(TDataSet(
              Self.FindComponent(sTableType)).FieldByName(sUseField).AsInteger)+','
          +''''+sUserId+'''' );
        //ShowMessage(SQL.Text);
        ExecSql;
      end;
    //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    end;
  end;
  //============================================================================
  inherited;
end;

procedure TfrmProdLayerDLL.btnCMapClick(Sender: TObject);
var sFileName:String;
begin
  inherited;
  AuditCheck;
  if qryDetail1.RecordCount=0 then abort;
  if OpenDialog1.Execute then
  begin
    sFileName :=OpenDialog1.FileName;
    if not (qryDetail1.State in [dsInsert, dsEdit]) then qryDetail1.Edit;
    qryDetail1.FieldByName('NotesPath').AsString := sFileName;
  end;
end;

procedure TfrmProdLayerDLL.btnCMapOpenClick(Sender: TObject);
begin
  inherited;
  ShellExecute(Handle,'open',PChar
        (qryDetail1.FieldByName('NotesPath').AsString),nil,nil,SW_SHOW);
end;

procedure TfrmProdLayerDLL.btnGetParamsClick(Sender: TObject);
//var i:integer;
//evCusBtnClickEvent:TevCusBtnClickEvent2;
var sDataArray: Array[0..4] of WideString;
    sNewSelectSQL, sCusId, sSubCusId:String;
    //2020.12.23
    i:integer;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  inherited;
  //2012.10.24 add 比照主檔
  prcEMODetailSet;

  //2012.04.25 補 Grid Title
  //============================================================================
  {with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select FieldName, FieldNote from CURdTableField(nolock) where '
        +'TableName='''+sRealTableNameMas1+'''');
    Open;
    if RecordCount>0 then
    begin
      for i := 0 to gridBrowse.FieldCount - 1 do
      begin
        sTmpField:=gridBrowse.Fields[i].FieldName;
        if Locate('FieldName', sTmpField, [loPartialKey]) then
        begin
          gridBrowse.Fields[i].DisplayLabel:=FieldByName('FieldNote').AsString;
        end;
      end;
    end;
  end;}
  //============================================================================
  iNeedChkDesigner:=1;
  sDesignerPN:='';
  sDesignerRev:='';//2012.03.08
  sSeahchStr:='';

  //系統參數 *******************************************************************
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdGetParamSelect '''+sLanguageId+'''');
    Open;
    //2011.09.27 途程備註預設欄寬
    if Locate('ParamId', 'RouteNoteWidth', [loCaseInsensitive]) then
      DBMemo1.Width:=FieldByName('Value').AsInteger
    else
    begin
      DBMemo1.Align:=alClient;
      pnlNoteSep.Visible:=False;
    end;
    if Locate('ParamId', 'CusId', [loCaseInsensitive]) then
    begin
      sCusId:=FieldByName('Value').AsString;
      //2012.05.07
      lblCusId.Caption:=sCusId;
    end;
    //2012.04.12 Timeout Fail
    iTimeOut:=0;
    if Locate('ParamId', 'TimeOutSec', [loCaseInsensitive]) then
      iTimeOut:=FieldByName('Value').AsInteger;
    //2012.04.26 ScrollBar
    if Locate('ParamId', 'MaxHeight', [loCaseInsensitive]) then
    begin
      iScrollHeight:=FieldByName('Value').AsInteger + 10;
    end
    else
    begin
      sclPnlMaster.Visible:=False;
      sclPnlMaster2.Visible:=False;
    end;

    //2013.01.08
    sSubCusId:='';
    qryExec.close;
    sql.Clear;
    sql.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
        +' and ParamId=''SubCusId''');
    open;
    if RecordCount>0 then
        sSubCusId:=FieldByName('Value').AsString;
    qryExec.close;
  end;
  //系統參數 end ***************************************************************

  if sCusId='MUT' then
  begin
    btnBackupNotes.Visible:=False;
    btnPasteNotes.Visible:=False;
    tbshtDetail1.TabVisible:=False;
  end;
  iNeedRefresh:=0;
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
    end;}
  pgeFormTypeChange(Sender);
  tblLayerBOM.Close;
  tblLayerBOM.Open;
  LockButton(True);
  //主檔呼叫用
  if iCallType=2 then
  begin
    ParseFormatW(sPaperNum, ',', sDataArray, 5);
    sNewSelectSQL:='';
    sNewSelectSQL:=sSelectSQLMas1
          +' and t0.PartNum='''+sDataArray[0]+''''
          +' and t0.Revision='''+sDataArray[1]+''''
          +' and t0.LayerId='''+sDataArray[2]+''''
          +' '
          +unit_DLL.funGetFilterSQL(qryexec,sItemId,'Master1')
          +' '
          +unit_DLL.funGetOrderByField(qryExec,sItemId,'Master1');
    sPaperNum:=sDataArray[0];//復原
    with qryBrowse do
    begin
      Close;
      SQL.Clear;
      SQL.Add(sNewSelectSQL);
      Open;
    end;
    pnlTempBasDLLTop.Visible:=True;
    pnlTempBasDLLBottom.Visible:=True;
    btnInq.Visible:=False;
    if sDataArray[3]='0' then
      btnUpdate.Visible:=False;
    SetViewIsDetail;
    if sDataArray[3]='1' then
      btnUpdate.Visible:=True;
    if sDataArray[4]='1' then
      btnUpdateClick(Sender);
    //2012.04.26
    SetScroll;
  end;
  if sCusId='MUT' then
  begin
    //2011.11.22 這裡指定才有效
    pgeDetail.ActivePageIndex:=iDtlActivePage;
    pgeDetailChange(Sender);
    //2012.08.07 要有明確動作觸發，不然 Nav 都會錯亂
    lblNavSource.Caption:=nav2.DataSource.Name;
    //2012.05.08
    SetFieldParent; //先只在 MUT 試跑
  end;
  //prcStoreFieldNeed_Def(self,qryExec); //for 強制大寫

  //2013.01.08 for CMT
  if ((sCusId<>'MUT') or (UPPERCASE(sSubCusId)<>'C')) then
    pnlUseNotes.Visible:=False;

  //2020.12.15 Fix
  btnCompleted.Visible:=False;
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
    pnlMaster1.ScaleBy(FontSize, 100);
    pnlMaster2.ScaleBy(FontSize, 100);
    pnlMaster3.ScaleBy(FontSize, 100);
    pnlMaster4.ScaleBy(FontSize, 100);

    pnlRouteTools.ScaleBy(70, FontSize);
    pnlPath.ScaleBy(70, FontSize);
    附件.Width:= Round(28 * FontSize / 100);
    附件.Height := Round(13 * (FontSize/100));
  end;
end;

procedure TfrmProdLayerDLL.btnKeepStatusClick(Sender: TObject);
begin
  inherited;
  { 2012.05.09 改在"切換"生效
  //2011.10.04 在修改時，會限定單一料號(以免資料跑掉)，
  //當修改完回流覽畫面時，補回原有搜尋條件
  if ((lblPartNumBack.Caption<>'') and (lblPartNumBack.Caption<>'lblPartNumBack')) then
  begin
    qryBrowse.Locate('PartNum;revision;LayerId' ,
        VarArrayOf([lblPartNumBack.Caption,lblRevisionBack.Caption,
          lblLayerIdBack.Caption]),[loPartialKey]);
    lblPartNumBack.Caption:='';
  end; }
end;

procedure TfrmProdLayerDLL.btnRouteChangeClick(Sender: TObject);
var NowPartNum, NowRevision, NowLayer: String;
begin
  inherited;
  if qryBrowse.State in [dsEdit ,dsInsert] then qryBrowse.Post;
  AuditCheck;
  NowPartNum:=qryBrowse.FieldByName('PartNum').AsString;
  NowRevision:=qryBrowse.FieldByName('revision').AsString;
  NowLayer:=qryBrowse.FieldByName('LayerId').AsString;
  Application.createForm(TdlgRouteInsNew, dlgRouteInsNew);
  dlgRouteInsNew.sConnectStr:=sConnectStr;
  dlgRouteInsNew.prcDoSetConnOCX;
  with dlgRouteInsNew, qryBrowse do
  begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryProcInfo.CommandTimeout:=iTimeOut;
        qryPosChange.CommandTimeout:=iTimeOut;
        qryReLoad.CommandTimeout:=iTimeOut;
        qryCloseCheck.CommandTimeout:=iTimeOut;
        qryClear.CommandTimeout:=iTimeOut;
        qryExec.CommandTimeout:=iTimeOut;
        qryRoute.CommandTimeout:=iTimeOut;
      end;
    sUpdateUser:=sUserId;
    qryExec.Close;
    qryExec.SQL.Clear;
    qryExec.SQL.Add('exec EMOdProdRouteBefChg '''+NowPartNum+''', '''
          +NowRevision+''', '''+NowLayer+'''');
    qryExec.Open;
    iSpId:=qryExec.FieldByName('SpId').AsInteger;
    GetData(FieldByName('PartNum').AsString,FieldByName('revision').AsString,
            FieldByName('LayerId').AsString);
    edtLayer.Text:= FieldByName('LayerId').AsString;
    Showmodal;
  end;
  qryBrowse.Close;
  qryBrowse.Open;
  qryBrowse.Locate('Partnum;Revision;LayerId' ,
        VarArrayOf([NowPartNum,NowRevision,NowLayer]) ,[loPartialKey]);
  qryDetail1.close;
  qryDetail1.Open;
end;

procedure TfrmProdLayerDLL.btnSaveHeightClick(Sender: TObject);
var iWidth: Integer;
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
end;

procedure TfrmProdLayerDLL.btnUpdateClick(Sender: TObject);
var iPageIndex, iLock, iSourceIndex:Integer;
    //2012.05.07
    sTmpField: String;
    sUpdatePartRev:String; //05.09
begin
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select Status from EMOdProdInfo(nolock) where PartNum='''
            +qryBrowse.FieldByName('PartNum').AsString+''' and Revision='''
            +qryBrowse.FieldByName('Revision').AsString+'''');
    Open;
    iStatus:=FieldByName('Status').AsInteger;
  end;
  AuditCheck;
  {if iStatus>0 then
  begin
    MsgDlgJS('品號已審核，不可變更!!!',mtWarning, [mbOk],0);
    abort;
  end;}
  iPageIndex:=pgeDetail.ActivePageIndex;
  //2011.10.04 修正"修改"時料號會跑掉
  lblPartNumBack.Caption:= qryBrowse.FieldByName('PartNum').AsString;
  lblRevisionBack.Caption:= qryBrowse.FieldByName('Revision').AsString;
  lblLayerIdBack.Caption:= qryBrowse.FieldByName('LayerId').AsString;
  //2012.05.09 還是有時會跑出"被開啟的物件"錯誤訊息；改為與主檔一樣方法
  if qryBrowse.RecordCount>1 then
  begin
    sUpdatePartRev:=sNoOrderByMasSQL
        +' and t0.PartNum='''+lblPartNumBack.Caption+''''
        +' and t0.Revision ='''+lblRevisionBack.Caption+''''
        +' and t0.LayerId='''+lblLayerIdBack.Caption+'''';
    qryBrowse.Close;
    //101001
    sSeahchStr:=qryBrowse.SQL.Text;
    qryBrowse.SQL.Clear;
    qryBrowse.SQL.Add(sUpdatePartRev);
    qryBrowse.Open;
  end;
  inherited;
  {//2011.10.04 修正"修改"時料號會跑掉 part2
  if not qryBrowse.Locate('PartNum;revision;LayerId' ,
        VarArrayOf([lblPartNumBack.Caption,lblRevisionBack.Caption,
          lblLayerIdBack.Caption]),[loPartialKey]) then
  begin
    ShowMessage('層別指定失敗，請單獨查詢此料號層別後再進行修改!!');
    Abort;
  end;}
  LockButton(False);
  pgeDetail.ActivePageIndex:=iPageIndex;
  //2012.10.24 頁籤序的對應改由 qryPage 決定
  qryPage.Locate('SerialNum',iPageIndex+1,[loCaseInsensitive]);
  iSourceIndex:=qryPage.FieldByName('KindItem').AsInteger;
  if iSourceIndex=0 then
  BEGIN
      nav1.DataSource:= dsBrowse;
      nav2.DataSource:= dsBrowse;
  END
  ELSE
  BEGIN
      if sNowMode='UPDATE' then
        begin
          nav1.DataSource:=
            TDataSource(
              FindComponent('dsDetail'+inttostr(iSourceIndex{pgeDetail.ActivePageIndex+1}))
              );
        end;

       nav2.DataSource:=
            TDataSource(
              FindComponent('dsDetail'+inttostr(iSourceIndex{pgeDetail.ActivePageIndex+1}))
              );
  END;
  //2012.05.07
  //============================================================================
  if lblCusId.Caption='MUT' then
  begin
    with qryExec do
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('EMOdLayerLock_MUT '''+lblPartNumBack.Caption+''','''
        +lblRevisionBack.Caption+''','''
        +lblLayerIdBack.Caption+'''');
      Open;
      if RecordCount>0 then
      begin
        first;
        while not eof do
        begin
          //ShowMessage(qryExec.FieldByName('ColName').AsString);
          sTmpField:=FieldByName('ColName').AsString;
          iLock:=FieldByName('LockCol').AsInteger;
          if sTmpField<>'' then
          begin
            if qryBrowse.FindField(sTmpField)<>nil then
            begin
              if iLock=1 then
                qryBrowse.FieldByName(sTmpField).ReadOnly:=True
              else
                qryBrowse.FieldByName(sTmpField).ReadOnly:=False;
            {Test if qryBrowse.FieldByName('Film_PPM').ReadOnly=True then
              ShowMessage('Yes')
            else
              ShowMessage('No');}
            end;
          end;
          next;
        end;
      end;
      qryExec.Close;
    end;
  end;//MUT end
  //============================================================================
end;

procedure TfrmProdLayerDLL.btnUseNotesClick(Sender: TObject);
var iCover: Integer;
    sNowPart, sNowRev, sNowLayer: String;
begin
  inherited;
  AuditCheck;
  sNowPart:=dsBrowse.DataSet.FieldByname('PartNum').AsString;
  sNowRev:= dsBrowse.DataSet.FieldByname('Revision').AsString;
  sNowLayer:= dsBrowse.DataSet.FieldByname('LayerId').AsString;
  with qryExec do
  begin
    iCover:=0;
    //先做是否已取過的檢查
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select IsUseNotes=IsNull(IsUseNotes, 0) from EMOdProdLayer(nolock)'
      +' where PartNum='''+qryBrowse.FieldByName('PartNum').AsString +''''
      +' and Revision='''+qryBrowse.FieldByName('Revision').AsString +''''
      +' and LayerId='''+qryBrowse.FieldByName('LayerId').AsString+'''');
    Open;
    if FieldByName('IsUseNotes').AsInteger=1 then
    begin
      if MsgDlgJS('此層別已產生過，是否覆蓋？',
               mtConfirmation,[mbYes, mbNo], 0) = mrYes then iCover:=1
      else
        Exit;
    end;

    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdGetFactorMo '''+qryBrowse.FieldByName('PartNum').AsString
          +''','''+qryBrowse.FieldByName('Revision').AsString
          +''','''+qryBrowse.FieldByName('LayerId').AsString+''','
          +IntToStr(iCover));
    try
      Open;
    except
      on E:Exception do
      begin
        MsgDlgJS(E.Message, mtWarning, [mbok], 0);
        qryExec.Close;
        Exit;
      end;
    end;
    MsgDlgJS(StringReplace(qryExec.FieldByName('ReturnValue').AsString,
        'MESGE:','',[rfReplaceAll]),mtInformation,[mbOK],0);
    qryExec.Close;
    UpdateDesigner;
    qryBrowse.Close;
    qryBrowse.Open;
    qryDetail1.Close;
    qryDetail1.Open;
    qryBrowse.Locate('PartNum;revision;LayerId' ,
                      VarArrayOf([sNowPart,sNowRev,sNowLayer]) ,[loPartialKey]);
  end;
end;

procedure TfrmProdLayerDLL.dbgBOMColEnter(Sender: TObject);
begin
  inherited;
  if nav2.DataSource<>dsLayerBOM then
    nav2.DataSource:=dsLayerBOM;
end;

procedure TfrmProdLayerDLL.btnViewClick(Sender: TObject);
begin
  inherited;
  //2012.04.25
  if lblParam.Caption<>'1' then
  begin
    pgeBwsDtl.Height:=pgeBwsDtl.Height;//+5;
    lblParam.Caption:='1';
    SetScroll;
  end;
  //2012.05.09 在修改時，會限定單一料號(以免資料跑掉)，
  //當修改完回流覽畫面時，補回原有搜尋條件
  if sSeahchStr<>'' then
  begin
    qryBrowse.Close;
    qryBrowse.SQL.Clear;
    qryBrowse.SQL.Add(sSeahchStr);
    qryBrowse.Open;
    sSeahchStr:='';
    qryBrowse.Locate('PartNum;Revision;LayerId' ,
          VarArrayOf([lblPartNumBack.Caption,lblRevisionBack.Caption,
            lblLayerIdBack.Caption]) ,[loPartialKey]);
  end;
  //2012.08.03 add
  if ViewStatus=vDetail then
  begin
    if lblCusId.Caption='MUT' then
      pgeDetail.ActivePageIndex:=iDtlActivePage;
  end;
end;

procedure TfrmProdLayerDLL.SetScroll;
begin
  //04.26 ScrollBar
  //從主檔呼叫，第一次 iScrollHeight 會是零
  if ((pnlMaster1.Height>0) and (iScrollHeight>0)) then
  begin
    if iScrollHeight>pnlMaster1.Height then
    begin
        sclPnlMaster.Max:= (iScrollHeight-pnlMaster1.Height) div 2;
        sclPnlMaster2.Max:= (iScrollHeight-pnlMaster1.Height) div 2;
        //ShowMessage(IntToStr(sclPnlMaster.Max));
        iScrollHeight:=0;
    end
    else
    begin
        sclPnlMaster.Visible:=False;
        sclPnlMaster2.Visible:=False;
    end;
  end;
end;

procedure TfrmProdLayerDLL.sclPnlMasterScroll(Sender: TObject;
  ScrollCode: TScrollCode; var ScrollPos: Integer);
var iChangeLen: Integer;
begin
  inherited;
  //2012.04.26
  //ScrollBar 按一次會發生兩次 OnScroll，第一次是按前位置，第二次是按後位置
  if ScrollPos<>iScrollHeight then
  begin
    iChangeLen:= iScrollHeight - ScrollPos;
    pnlMaster1.ScrollBy(0, iChangeLen * 2);
    iScrollHeight := ScrollPos; //借用來記原始位置
  end;
end;

procedure TfrmProdLayerDLL.gridDetail1Enter(Sender: TObject);
begin
  inherited;
  if nav2.DataSource<>TJSdDBGrid(sender).DataSource then
    nav2.DataSource:=TJSdDBGrid(sender).DataSource;
end;

procedure TfrmProdLayerDLL.gridDetail3ColExit(Sender: TObject);
begin
  inherited;
  //2012.08.07 add
  if qryDetail3.TableName='EMOdLayerHole' then
  begin
    //from SPOdOrderMain
    if ((gridDetail3.selectedField.fullName = 'NeedPTH')
        or (gridDetail3.selectedField.fullName = 'DigHole') ) then
    begin
      if qryDetail3.Active then
      begin
          if ((qryDetail3.State in [dsEdit]) or (qryDetail3.State in [dsInsert]))
          then
            qryDetail3.Post;
      end;
    end;
  end;
end;

procedure TfrmProdLayerDLL.AuditCheck;
begin
  inherited;
  if iStatus>0 then
  begin
    with qryExec do
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('exec EMOdAuditUserCheck '''+dsBrowse.DataSet.FieldByname('PartNum').AsString
          +''','''+dsBrowse.DataSet.FieldByname('Revision').AsString+''','''
          +sUserId+'''');
      Open;
      if FieldByName('ReturnStr').AsString<>'' then
      begin
        MsgDlgJS(FieldByName('ReturnStr').AsString,mtWarning, [mbOk],0);
        abort;
      end
    end;
  end;
end;

procedure TfrmProdLayerDLL.LockButton(bStatus: Boolean);
//var i:Integer;
begin
  {for i:= 0 to Self.componentcount-1 do
  begin
    if Self.components[i] is TSpeedButton then
    begin
      if bStatus=True then
        TSpeedButton(Self.components[i]).Enabled:=False
      else
        TSpeedButton(Self.components[i]).Enabled:=True;
    end;
  end;}
  //用迴圈會把所有功能都停掉，改為手動指定
  if bStatus=True then
  begin
    btnMapOpenTest.Enabled:=False;
    btnNoteStyleTree.Enabled:=False;
    btnChangeProc.Enabled:=False;
    btnBackupNotes.Enabled:=False;
    btnPasteNotes.Enabled:=False;
    btnRouteChange.Enabled:=False;
    btnProcBOMSet.Enabled:=False;
    btnUseNotes.Enabled:=False;
    dbgBOM.ReadOnly:=True;
    DBMemo1.ReadOnly:=True;
  end
  else
  begin
    btnMapOpenTest.Enabled:=True;
    btnNoteStyleTree.Enabled:=True;
    btnChangeProc.Enabled:=True;
    btnBackupNotes.Enabled:=True;
    btnPasteNotes.Enabled:=True;
    btnRouteChange.Enabled:=True;
    btnProcBOMSet.Enabled:=True;
    btnUseNotes.Enabled:=True;
    dbgBOM.ReadOnly:=False;
    DBMemo1.ReadOnly:=False;
  end;
end;

procedure TfrmProdLayerDLL.ParseFormatW(sInput, sDim: WideString; var sParam : array of WideString; iPara:integer);
var   i, iPos: integer;
begin
  for i:= 0 to (iPara-1) do sParam[i] := '';

  for i:= 0 to (iPara-1) do
  begin
    iPos := Pos(sDim, sInput);
    if iPos = 0 then iPos := (Length(sInput)+1);
    sParam[i] := Copy(sInput, 1, iPos-1);
    sInput := TrimRight(TrimLeft(Copy(sInput, iPos+1, Length(sInput))));
    if Length(sInput) < 1 then exit;
  end;
end;

procedure TfrmProdLayerDLL.UpdateDesigner;
begin
  //2012.03.08 add
  if (  trim(qryBrowse.FieldByname('PartNum').AsString)
       +trim(qryBrowse.FieldByname('Revision').AsString)  )
     <> (trim(sDesignerPN)+trim(sDesignerRev)) then
  begin
    iNeedChkDesigner:=1;
    sDesignerPN:=trim(qryBrowse.FieldByname('PartNum').AsString);
    sDesignerRev:=trim(qryBrowse.FieldByname('Revision').AsString)
  end;

  if iNeedChkDesigner=1 then
  begin
    with qryExec do
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('exec EMOdUpdateDesigner '''
                +qryBrowse.FieldByname('PartNum').AsString+''','''
                +qryBrowse.FieldByname('Revision').AsString+''','''
                +sUserId+'''');
      ExecSql;
    end;
    iNeedChkDesigner:=0; //只檢查一次就好
  end;
end;

procedure TfrmProdLayerDLL.SetFieldParent;
var tComp:TComponent;
    i, iCnt: Integer;
begin
  with qryExec do
  begin
    iCnt:=0;
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select distinct CNT=COUNT(t1.ToFieldName) from '
      +'EMOdFieldParentDataDtl t1(nolock), EMOdFieldParentDataMas t2(nolock) '
      +'where t1.TableName=t2.TableName and t1.FieldName=t2.FieldName '
      +'and t2.TableName='''+qryBrowse.TableName+'''');
    Open;
    iCnt:=FieldByName('Cnt').AsInteger;
    if iCnt>0 then
    begin
        for i := 0 to iCnt - 1 do
        begin
          tComp:=nil;
          tComp:=Self.FindComponent('cboSub_'+IntToStr(i+1));
          if tComp<>nil then
            if tComp is TJSdLookupCombo then
              TJSdLookupCombo(tComp).OnEnter:=JSdLookupComboSubEnter;
        end;
    end;
  end;
end;

procedure TfrmProdLayerDLL.JSdLookupComboSubEnter(Sender: TObject);
var sSQL, SuperText, newSQL: WideString;
    qryLK: TADOQuery;
    dsLK: TDataSource;
    tComp2:TComponent;
begin
  SuperText:='';
  tComp2:=nil;
  tComp2:=Self.FindComponent({StringReplace(TJSdLookupCombo(Sender).Name,
                              'cboSub','cboMas',[rfReplaceAll, rfIgnoreCase])}
                              TJSdLookupCombo(Sender).SuperId);
  if tComp2<>nil then
    if tComp2 is TDBComboBox then
      SuperText:=TDBComboBox(tComp2).Text;
  sSQL:= TJSdLookupCombo(Sender).SQLCmd;
  newSQL:= StringReplace(sSQL, '@@@@@', SuperText, [rfReplaceAll]);
  qryLK:= TADOQuery.Create(Self);
  qryLK.ConnectionString := sConnectStr;
        qryLK.CommandTimeout := 480;
        qryLK.LockType := ltReadOnly;
  qryLK.SQL.clear;
  qryLK.SQL.add(newSQL);

  dsLK:= TdataSource.Create(Self);
  dsLK.DataSet:= qryLK;

  TJSdLookupCombo(Sender).LkDataSource:= dsLK;
  qryLK.Open;
end;

procedure TfrmProdLayerDLL.prcEMODetailSet; //2012.09.17 add
begin
  inherited;
  if TabSheetMas.TabVisible=True then
    TabSheetMas.PageIndex:=0;
  with qryPage do
  begin
    //對照表
    qryPage.Close;
    SQL.Clear;
    SQL.Add('exec EMOdProdFormSet '''+sItemId+'''');
    Open;
    First;
    while not Eof do
    begin
      //注意內層主檔可能併入
      if FieldByName('KindItem').AsInteger>0 then
      BEGIN
        TTabSheet(Self.FindComponent('tbshtDetail'
            +FieldByName('KindItem').AsString)).PageIndex
          :=FieldByName('SerialNum').AsInteger-1;
        if FieldByName('IsHide').AsInteger=1 then
          TTabSheet(Self.FindComponent('tbshtDetail'
            +FieldByName('KindItem').AsString)).TabVisible:=False;
      END;
      Next;
    end;
    iDtlActivePage:=FieldByName('ActivePage').AsInteger;
  end;//qryPage end
end;

end.

unit ProductDLL;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, PaperOrgDLL{, WinSkinData}, JSdGrid2Excel, Menus, JSdPopupMenu, DB,
  JSdTable, ADODB, StdCtrls, Mask, DBCtrls, JSdLabel, ExtCtrls, Grids, Wwdbigrd,
  Wwdbgrid, JSdDBGrid, ComCtrls, Buttons, Wwdatsrc, XFlowDrawBox, JSdTreeView,
  ImgList, JSdLookupCombo, XFlowUtils, IdMessage, ExtDlgs, ShellApi, JSdMultSelect,
  JSdComboBox, wwdblook, ToolWin, JSdReport, System.ImageList;

{type
    TevCusBtnClickEvent2 = class
      oldCusBtnClickEvent2: TNotifyEvent;
      procedure prcCusBtnClick2(sender: TObject);
    end; }

type
  TfrmProductDLL = class(TfrmPaperOrgDLL)
    pnlState: TPanel;
    Splitter2: TSplitter;
    Splitter4: TSplitter;
    tbshtDetail9: TTabSheet;
    pnlTierTools: TPanel;
    btnTierIns: TSpeedButton;
    Panel14: TPanel;
    btnCMap: TSpeedButton;
    btnCMapOpen: TSpeedButton;
    Label129: TJSdLabel;
    btnSMapOpen: TSpeedButton;
    btnSMap: TSpeedButton;
    Label130: TJSdLabel;
    DBEdit107: TDBEdit;
    DBEdit106: TDBEdit;
    dbgProdTier: TJSdDBGrid;
    XFlowDrawBox2: TXFlowDrawBox;
    pnlMapTools: TPanel;
    btnAutoDraw: TSpeedButton;
    btnPrint: TSpeedButton;
    btnMapUpdate: TSpeedButton;
    chkViewMapData: TCheckBox;
    XFlowDrawBox1: TXFlowDrawBox;
    trvBOM: TJSdTreeView;
    tblProdLayer: TJSdTable;
    dsProdLayer: TDataSource;
    ImageList1: TImageList;
    qryDetail9: TJSdTable;
    dsDetail9: TDataSource;
    pnlMaster5: TPanel;
    pgeFormType: TPageControl;
    tstMain: TTabSheet;
    tstSub: TTabSheet;
    dsProdHIO: TwwDataSource;
    qryProdAudit3: TADOQuery;
    pnlModifyTools: TPanel;
    btnModifyExl: TSpeedButton;
    btnModifySet: TSpeedButton;
    Panel22: TJSdLabel;
    pgdProdModify: TJSdGrid2Excel;
    qryMapXFlow: TADOQuery;
    qryMapXFlowMapData: TStringField;
    qryMapXFlowMapData2: TWideStringField;
    qryMapXFlowStrMap: TStringField;
    qryProdLayer: TJSdTable;
    qryProdLayerPartNum: TStringField;
    qryProdLayerRevision: TStringField;
    qryProdLayerLayerId: TStringField;
    qryProdLayerLayerName: TStringField;
    qryProdLayerAftLayerId: TStringField;
    qryProdLayerIssLayer: TIntegerField;
    qryProdLayerDegree: TIntegerField;
    qryProdLayerFL: TIntegerField;
    qryProdLayerEL: TIntegerField;
    qryProdLayerTmpRouteId: TStringField;
    qryProdLayerStdPressCode: TStringField;
    qryProdLayerFilm_PPM2: TFloatField;
    qryProdLayerFilm_ChkReceUp2: TFloatField;
    qryProdLayerLayerNotes: TWideStringField;
    qryEditCheck: TADOQuery;
    qryEditCheckV: TIntegerField;
    qryUserId: TADOQuery;
    qryUsersUserId: TStringField;
    qryUsersUserName: TWideStringField;
    dsUserId: TDataSource;
    pnlPressTools: TPanel;
    btnLayerPressUpdate: TSpeedButton;
    btnLayerPressIns: TSpeedButton;
    btnPressChange: TSpeedButton;
    edtTmpPressId: TDBEdit;
    qryProdHIO: TJSdTable;
    Splitter3: TSplitter;
    SavePictureDialog1: TSavePictureDialog;
    OpenDialog1: TOpenDialog;
    qryPartMatri: TADOQuery;
    qryPartMatriPartNum: TStringField;
    qryPartMatriRevision: TStringField;
    dsPartMatri: TDataSource;
    grdHole: TwwDBGrid;
    grdHoleIButton: TwwIButton;
    qryDelete: TADOQuery;
    tblLayerPress: TJSdTable;
    dsLayerPress: TDataSource;
    Splitter5: TSplitter;
    Panel13: TPanel;
    SpeedButton3: TSpeedButton;
    btnProdUseMat: TSpeedButton;
    qryMatCode: TADOQuery;
    qryLayerPress_New: TADOQuery;
    qryLayerPress_NewSerialNum: TSmallintField;
    qryLayerPress_NewBefLayer: TStringField;
    qryLayerPress_NewValueName: TStringField;
    qryLayerPress_NewIsNormal: TIntegerField;
    qryLayerPress_NewIsTested: TIntegerField;
    qryLayerPress_NewIsSpeced: TIntegerField;
    qryLayerPress_NewISULMark: TIntegerField;
    qryLayerPress_NewGP: TSmallintField;
    qryLayerPress_NewICP: TSmallintField;
    qryLayerPress_NewGPIsHold: TSmallintField;
    qryLayerPress_NewUL: TSmallintField;
    qryLayerPress_NewNotes: TWideStringField;
    qryLayerPress_NewUsage: TFloatField;
    qryLayerPress_NewMatClass: TStringField;
    qryPressNull: TADOQuery;
    pnlLayer: TPanel;
    btnLayer: TSpeedButton;
    tbshtDetail10: TTabSheet;
    pnlRouteTools: TPanel;
    btnChangeProc: TSpeedButton;
    btnBackupNotes: TSpeedButton;
    btnPasteNotes: TSpeedButton;
    btnNotesStyleTree: TSpeedButton;
    btnRouteChange: TSpeedButton;
    btnRouteBOMSet: TSpeedButton;
    DBNavigator2: TDBNavigator;
    panRoute: TPanel;
    DBEdit1: TDBEdit;
    dbgRoute: TJSdDBGrid;
    Splitter6: TSplitter;
    qryDetail11: TJSdTable;
    dsDetail11: TDataSource;
    qryDetail10: TJSdTable;
    dsDetail10: TDataSource;
    qryTmpRouteId: TJSdTable;
    dsTmpRouteId: TDataSource;
    qryProcCodeBOMSet: TADOQuery;
    dbgLayerPress: TJSdDBGrid;
    btnCopyRoute: TSpeedButton;
    qryCopyRoute: TADOQuery;
    Splitter7: TSplitter;
    qryDetail11PartNum: TStringField;
    qryDetail11Revision: TStringField;
    qryDetail11LayerId: TStringField;
    qryDetail11ProcCode: TStringField;
    qryDetail11SerialNum: TWordField;
    qryDetail11MatCode: TStringField;
    qryDetail11UseQnty: TFloatField;
    qryDetail11UseBase: TFloatField;
    qryDetail11MatPos: TWideStringField;
    qryDetail11Notes: TWideStringField;
    qryDetail11BeDisplace: TIntegerField;
    qryDetail11BeSemiProd: TIntegerField;
    qryDetail11SuperId: TStringField;
    qryDetail11isCustSupply: TIntegerField;
    qryDetail11StDScRate: TFloatField;
    qryDetail11MatName: TWideStringField;
    qryMatName: TADOQuery;
    qryMatNamePartNum: TStringField;
    qryMatNameMatName: TWideStringField;
    qryDetail10PartNum: TStringField;
    qryDetail10Revision: TStringField;
    qryDetail10LayerId: TStringField;
    qryDetail10SerialNum: TWordField;
    qryDetail10ProcCode: TStringField;
    qryDetail10Notes: TWideStringField;
    qryDetail10FinishRate: TFloatField;
    qryDetail10IsNormal: TWideStringField;
    qryDetail10DepartId: TStringField;
    qryDetail10Spec: TWideStringField;
    qryDetail10FilmNo: TWideStringField;
    qryDetail10ChangeNotes: TWideStringField;
    qryDetail10PartSerial: TStringField;
    qryDetail10ProcSerial: TStringField;
    qryDetail10SortType: TStringField;
    qryDetail10BefSETime: TFloatField;
    qryDetail10MoldPcs: TIntegerField;
    qryDetail10Item: TIntegerField;
    qryDetail10ProcName: TWideStringField;
    qryProcInfo: TADOQuery;
    Panel1: TPanel;
    dbgBOM: TJSdDBGrid;
    DBMemo1: TDBMemo;
    Splitter8: TSplitter;
    qryMap: TADOQuery;
    Panel2: TPanel;
    Panel3: TPanel;
    JSdLabel2: TJSdLabel;
    dbgModify: TJSdDBGrid;
    tblModify: TJSdTable;
    dsModify: TDataSource;
    Splitter9: TSplitter;
    pnlMills: TPanel;
    Panel4: TPanel;
    navMills: TDBNavigator;
    dbgMills: TJSdDBGrid;
    pnlWriting: TPanel;
    Panel6: TPanel;
    navWriting: TDBNavigator;
    dbgWriting: TJSdDBGrid;
    tblMills: TJSdTable;
    dsMills: TDataSource;
    tblWriting: TJSdTable;
    dsWriting: TDataSource;
    lblWhere1: TLabel;
    lblWhere2: TLabel;
    qryDetail7B: TJSdTable;
    qryDetail8B: TJSdTable;
    dsDetail8B: TDataSource;
    dsDetail7B: TDataSource;
    qryDetail10NotesPath: TWideStringField;
    pnlPath: TPanel;
    lab_annex: TJSdLabel;
    btnRouteNote: TSpeedButton;
    btnOpenRouteNote: TSpeedButton;
    DBEdit2: TDBEdit;
    Splitter10: TSplitter;
    tbshtMaster6: TTabSheet;
    pnlMaster6: TPanel;
    pnlXFlow: TPanel;
    memoMap: TDBMemo;
    meoMapData: TDBMemo;
    pnlJPG: TPanel;
    ImgPOP: TImage;
    pnlLayerMap: TPanel;
    ScrollBox1: TScrollBox;
    ImgLayer: TImage;
    btnAllOutput: TSpeedButton;
    pnlXflow2: TPanel;
    tbshtDetail11: TTabSheet;
    tblMGNMap: TJSdTable;
    dsMGNMap: TDataSource;
    pnlMap: TPanel;
    btnViewMap: TSpeedButton;
    btnToGlyph: TSpeedButton;
    Panel5: TPanel;
    btnSaveMap: TSpeedButton;
    navProdMap: TDBNavigator;
    grdMap: TJSdDBGrid;
    pnlPartMergePrint: TPanel;
    SplitterMerge: TSplitter;
    dbgPartMergePrint: TJSdDBGrid;
    tblPartMergePrint: TJSdTable;
    dsPartMergePrint: TDataSource;
    Panel7: TPanel;
    lblWhatRev: TLabel;
    cboWhatRev: TwwDBLookupCombo;
    ToolBar1: TToolBar;
    pnlMapTool1: TPanel;
    pnlMapTool3: TPanel;
    pnlMapTool2: TPanel;
    pnlMapTool4: TPanel;
    pnlMapTool5: TPanel;
    pnlMapTool6: TPanel;
    btnSaveMapTmp: TSpeedButton;
    btnFunction: TSpeedButton;
    pnlFunction: TPanel;
    btnHideFunction: TSpeedButton;
    btnFunc2: TSpeedButton;
    btnFunc1: TSpeedButton;
    btnFunc3: TSpeedButton;
    btnFunc4: TSpeedButton;
    btnFunc5: TSpeedButton;
    btnFunc6: TSpeedButton;
    pnlRouteNote: TPanel;
    pnlNoteSep: TPanel;
    Splitter11: TSplitter;
    btnFinish: TSpeedButton;
    JSdDBGrid1: TJSdDBGrid;
    btnC9: TSpeedButton;
    btnC10: TSpeedButton;
    btnC11: TSpeedButton;
    btnC12: TSpeedButton;
    btnC13: TSpeedButton;
    btnC14: TSpeedButton;
    qryDetail10Mark: TWideStringField;
    lblParam: TLabel;
    MemoUpdateDtl: TDBMemo;
    SplMemoDtl: TSplitter;
    qryPage: TADOQuery;
    btnMU_Excel: TSpeedButton;
    chkUseNotes: TDBCheckBox;
    pnlUseNotes: TPanel;
    btnUseNotes: TSpeedButton;
    qryTmpRouteIdIsUseNotes: TIntegerField;
    qryTmpRouteIdTmpRouteId: TStringField;
    qryTmpRouteIdPartNum: TStringField;
    qryTmpRouteIdRevision: TStringField;
    qryTmpRouteIdLayerId: TStringField;
    qryMapXFlowStrMap2: TMemoField;
    btnUpdNote: TSpeedButton;
    NavUpdNote: TDBNavigator;
    chkJHCoreCom: TDBCheckBox;
    btnJHPressChg: TSpeedButton;
    qryUnit: TADOQuery;
    StringField1: TStringField;
    WideStringField1: TWideStringField;
    qryDetail11Unit: TStringField;
    qryUnitUnit: TStringField;
    tbshtMaster7: TTabSheet;
    pnlMaster7: TScrollBox;
    tbshtMaster8: TTabSheet;
    tbshtMaster9: TTabSheet;
    tbshtMaster10: TTabSheet;
    tbshtMaster11: TTabSheet;
    pnlMaster8: TScrollBox;
    pnlMaster11: TScrollBox;
    pnlMaster9: TScrollBox;
    pnlMaster10: TScrollBox;
    pnlMapTools2: TPanel;
    DBMemo2: TDBMemo;
    procedure btnGetParamsClick(Sender: TObject);
    procedure pgeFormTypeChange(Sender: TObject);
    procedure qryBrowseAfterScroll(DataSet: TDataSet);
    procedure btnModifyExlClick(Sender: TObject);
    procedure pgeDetailChange(Sender: TObject);
    procedure btnAutoDrawClick(Sender: TObject);
    procedure trvBOMDblClick(Sender: TObject);
    procedure XFlowDrawBox1DblClick(Sender: TObject);
    procedure btnPressChangeClick(Sender: TObject);
    procedure btnLayerPressInsClick(Sender: TObject);
    procedure btnPrintClick(Sender: TObject);
    procedure btnMapUpdateClick(Sender: TObject);
    procedure btnModifySetClick(Sender: TObject);
    procedure btnTierInsClick(Sender: TObject);
    procedure btnCMapClick(Sender: TObject);
    procedure btnCMapOpenClick(Sender: TObject);
    procedure qryDetail3AfterScroll(DataSet: TDataSet);
    procedure btnUpdateClick(Sender: TObject);
    procedure btnVoidClick(Sender: TObject);
    procedure btnC1Click(Sender: TObject);
    procedure btnLayerPressUpdateClick(Sender: TObject);
    procedure btnProdUseMatClick(Sender: TObject);
    procedure tblLayerPressBeforeEdit(DataSet: TDataSet);
    procedure btnRejExamClick(Sender: TObject);
    procedure btnExamClick(Sender: TObject);
    procedure btnLayerClick(Sender: TObject);
    procedure btnKeepStatusClick(Sender: TObject);
    procedure btnCompletedClick(Sender: TObject);
    procedure btnChangeProcClick(Sender: TObject);
    procedure btnNotesStyleTreeClick(Sender: TObject);
    procedure btnBackupNotesClick(Sender: TObject);
    procedure btnPasteNotesClick(Sender: TObject);
    procedure btnRouteChangeClick(Sender: TObject);
    procedure btnRouteBOMSetClick(Sender: TObject);
    procedure trvBOMChange(Sender: TObject; Node: TTreeNode);
    procedure btnCopyRouteClick(Sender: TObject);
    procedure qryDetail9BeforeInsert(DataSet: TDataSet);
    procedure qryDetail3AfterOpen(DataSet: TDataSet);
    procedure qryDetail6AfterInsert(DataSet: TDataSet);
    procedure qryBrowseAfterClose(DataSet: TDataSet);
    procedure qryBrowseAfterOpen(DataSet: TDataSet);
    procedure qryBrowseAfterPost(DataSet: TDataSet);
    procedure pgeMasterChange(Sender: TObject);
    procedure tblMillsAfterInsert(DataSet: TDataSet);
    procedure tblWritingAfterInsert(DataSet: TDataSet);
    procedure qryDetail7AfterClose(DataSet: TDataSet);
    procedure qryDetail7AfterOpen(DataSet: TDataSet);
    procedure qryDetail8AfterClose(DataSet: TDataSet);
    procedure qryDetail8AfterOpen(DataSet: TDataSet);
    procedure qryDetail1BeforeDelete(DataSet: TDataSet);
    procedure btnSaveHeightClick(Sender: TObject);
    procedure pnlTempBasDLLBottomDblClick(Sender: TObject);
    procedure btnRouteNoteClick(Sender: TObject);
    procedure btnOpenRouteNoteClick(Sender: TObject);
    procedure btnViewClick(Sender: TObject);
    procedure qryDetail7BAfterInsert(DataSet: TDataSet);
    procedure qryDetail7BBeforeInsert(DataSet: TDataSet);
    procedure pnlMapToolsDblClick(Sender: TObject);
    procedure btnAllOutputClick(Sender: TObject);
    procedure btnViewMapClick(Sender: TObject);
    procedure btnInqClick(Sender: TObject);
    procedure cboWhatRevEnter(Sender: TObject);
    procedure cboWhatRevExit(Sender: TObject);
    procedure tblLayerPressBeforePost(DataSet: TDataSet);
    procedure tblLayerPressAfterPost(DataSet: TDataSet);
    procedure qryDetail1AfterEdit(DataSet: TDataSet);
    procedure dbgBOMEnter(Sender: TObject);
    procedure dbgRouteEnter(Sender: TObject);
    procedure btnSaveMapTmpClick(Sender: TObject);
    procedure btnFunctionClick(Sender: TObject);
    procedure btnHideFunctionClick(Sender: TObject);
    procedure btnFinishClick(Sender: TObject);
    procedure qryDetail1AfterPost(DataSet: TDataSet);
    procedure qryDetail10BeforeDelete(DataSet: TDataSet);
    procedure qryDetail10AfterPost(DataSet: TDataSet);
    procedure btnPrintPaperClick(Sender: TObject);
    procedure nav1Click(Sender: TObject; Button: TNavigateBtn);
    procedure btnMU_ExcelClick(Sender: TObject);
    procedure btnUseNotesClick(Sender: TObject);
    procedure btnUpdNoteClick(Sender: TObject);
    procedure btnJHPressChgClick(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure btnC4Click(Sender: TObject);
    {procedure sclPnlMasterScroll(Sender: TObject; ScrollCode: TScrollCode;
      var ScrollPos: Integer);}
   private
    CurrLayer: string;
    PowerType1 : integer;
    sPressMapCut : string;
    EditPartNum, EditRevision: String;
    iNeedAct: Integer;
    sCusId, sSubCusId: String;
    //2010.09.04 add
    sRealMapPath: String;
    //100929
    iUpdateECNLog: Integer;
    sSeahchStr: String;
    //101221 add
    iMGNMap: Integer;
    //2011.04.29 add
    sPressMatCode: String;
    //2011.05.19 add
    iPressWarn: Integer;
    function CopyFileStr(sSrc, sDest: String): Boolean;
    function GetTempPathStr : WideString;
    function StatusCheck(Sender: TObject): Integer; //�����U�ק�e���s���i�ϥ�
    { Private declarations }
  public
    //2012.04.12 Timeout Fail
    var iTimeOut:Integer;
    procedure ProdAudit(Sender: TObject);virtual;
    procedure AuditCheck; //�]�p�����A�u���f�֪̥i���
    procedure AuditCheckModify; //�P�W�A���]�ܼf�ֻs�@
    procedure GetNewMapData(iKind: Integer);
    procedure ShowLayerMap;
    procedure ShowProdMap;
    procedure LockButton(bStatus:Boolean);
    procedure AuditSetting;
    procedure OpenRoute;
    procedure ScrollAct;
    procedure ShowRealMap(iKind: Integer);
    procedure MapOpen;
    procedure PressMapOpen;
    procedure UpdateDesigner; //2012.03.08
    procedure ClearAskTmp(NowSPId: Integer); //2012.06.07
    procedure prcEMODetailSet; //2012.09.17 add
    //2016.09.10
    procedure PressSetDefault;
    procedure PressSet_JH;
    //procedure SetScroll; //2020.08.05
    var iNeedChkDesigner {2012.03.08}:Integer;
        sDesignerPN, sDesignerRev {2012.03.08}:String;
        //iScrollHeight :Integer; //2020.08.05
    { Public declarations }
  end;

var
  frmProductDLL: TfrmProductDLL;

var
  MailPort: Integer;
  MailHost: string;

implementation

uses unit_DLL, NotValueShow, MapEdit, Map, PressChange, TmpPressSelect,
     LinkShowDLL, LayerPressSet, ProdUseMatSet, TmpRouteSelect, TmpRouteSet,
     RouteInsNew, LayerRouteSet, CopySelect, {unit_MIS,} NewNameEdit,
     LayerPressSet_JH,
     //2022.12.08 �u��P��
     ComObj;

{$R *.dfm}

{procedure TevCusBtnClickEvent2.prcCusBtnClick2(sender: TObject);
begin
  frmJourPaper.prcCustBtnRun(TSpeedButton(Sender).Name);
end;}

//2022.12.08 �u��P��
procedure TfrmProductDLL.btnC4Click(Sender: TObject);
var
ExcelApp,Excelall : Variant;
//PageSetup : Variant;
Filename : string;
Cancel : boolean;
Copies : integer;
begin

  Cancel:=false;

  with qryExec do
  begin
     qryExec.Close;
     SQL.Clear;
     SQL.Add('select PMapPath from EMOdProdInfo '+
             'where PartNum='+''''+qryBrowse.FieldByName('PartNum').AsString+''''+
             ' and Revision='+''''+qryBrowse.FieldByName('Revision').AsString+'''');
     Open;
  end;

  if qryExec.RecordCount<0 then abort;

  Filename:=qryExec.FieldByName('PMapPath').AsString;

  if FileExists(Filename)=false then
  begin
      MessageDlg('���Ƹ�P���ɤ��s�b!!', mtError, [mbOk], 0);
      abort;
  end;

  {with qryExec do
  begin
     qryExec.Close;
     SQL.Clear;
     SQL.Add('select LotCount=count(LotNum) from FMEdIssuePlanLot '+
             'where PaperNum='+''''+qryBrowse.FieldByName('PaperNum').AsString+''''+
             ' and LayerId='+''''+'L0~0'+'''');
     Open;
  end;

  if qryExec.RecordCount<0 then abort;}

  Copies:=1;
  //Copies:=qryExec.FieldByName('LotCount').AsInteger;

  try

    try

      if MessageDlg('�Х��]�w�q���w�]�L�����C'#10+
                    '�t�αN�}���ɮרѹw���C'#10+
                    '���U Yes �}�l�C�L�F���U No �����C�L', mtConfirmation,
                    [mbYes, mbNo], 0)=mrYes then
      begin
        //�Ы�OLE��HEXCEL Application
        ExcelApp:=CreateOleObject('Excel.Application');
        ExcelApp.Visible:=true; // ���Word�{��
        ExcelApp.DisplayAlerts:=False;
        ExcelApp.Workbooks.Open(Filename);
        Excelall := ExcelApp.Workbooks.Open(Filename);//�i�C�L����SHEET
        //ExcelApp.quit;  2021
        //ExcelApp.Select;
        //���s�]�w�̤j���,�קKword���X�T��
        {PageSetup := ExcelApp.ActiveSheet.PageSetup;
        PageSetup.LeftMargin:='0.5'; // ��ڦ�m-��
        PageSetup.RightMargin:='0.5'; // ��ڦ�m-�k
        PageSetup.TopMargin:= '0.5'; // ��ڦ�m-�W
        PageSetup.BottomMargin:= '0.5'; // ��ڦ�m-�U
        ExcelApp.Save;} // �s��,�קK����Word�S���X�T��    2021
        //MessageDlg('�}�l�C�L!', mtInformation, [mbOk], 0);
        //�C�L����Sheet
        Excelall.PrintOut(
                            EmptyParam,//�_�l����
                            EmptyParam,//��������
                            Copies,//�C�L����
                            true,//�O�_�w���C�L
                            EmptyParam//�L�����W��
                          );
        //ExcelApp.ActiveSheet.PrintOut(1, True, True);//�@�Τ���sheet�a�ѼơA�����w���C�L
        //sleep(5000);
      end
      else
      begin
        Cancel:=true;
        ExcelApp.Quit;
        ExcelApp:=Unassigned;
        //exit;
      end;

    finally
      ExcelApp.quit;
      ExcelApp:=Unassigned; //����VARIANT�ܶq
      MessageDlg('�C�L����!', mtInformation, [mbOk], 0);
    end;
  except
  on e: Exception do
  begin
    Screen.Cursor := crDefault;

    if Cancel then
    begin
      MessageDlg('�����C�L!', mtInformation, [mbOk], 0);
    end
    else
    begin
      MessageDlg('�C�L����!', mtInformation, [mbOk], 0);
      ExcelApp.Quit;
      //���ѥN���S�F��i�H����A���F�|����
      //ExcelApp:=Unassigned;
    end;


    Exit;
  end;
  end;


end;


procedure TfrmProductDLL.btnModifySetClick(Sender: TObject);
begin
  inherited;
  AuditCheckModify;
  With qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('Exec EMOdModifyModify '''
        +qryBrowse.FieldByName('PartNum').AsString+''','''
        +qryBrowse.FieldByName('Revision').AsString+''','''
        +sUserId+'''');
    ExecSql;
  end;
  with tblModify do
  begin
    Close;
    Parameters.ParamByName('PartNum').Value :=
        qryBrowse.FieldByName('PartNum').AsString;
    Parameters.ParamByName('Revision').Value :=
        qryBrowse.FieldByName('Revision').AsString;
    Open;
  end;
  //2012.03.08
  UpdateDesigner;

  //2014.09.12
  qryProdHIO.Close;
  qryProdHIO.Open;
end;

procedure TfrmProductDLL.btnModifyExlClick(Sender: TObject);
begin
  inherited;
  tblModify.Close;
  tblModify.Open;
  //pgdProdModify.Title := '�~���ק����'+ edtPnum.text + '-' + edtRevision.text;
  pgdProdModify.SaveToFile;
end;

//2022.11.30
procedure TfrmProductDLL.btnAddClick(Sender: TObject);
begin
  //inherited;
  btnExamClick(Sender);
end;

procedure TfrmProductDLL.btnAllOutputClick(Sender: TObject);
var bmp: TImage;
    Re:TRect;
    toFile, NPartNum, NRevision: string;
    //iCount:Integer; //���ե�
begin
  inherited;
  //YX �M��
  with qryExec do
  begin

    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) '
         +'where SystemId=''EMO'' and ParamId=''JpgPath''');
    Open;
    sRealMapPath:=FieldByName('Value').AsString;
  end;
  //iCount:=1;
  //�����q��w���Ƹ��}�l
  //while iCount<6 do
  while not qryBrowse.Eof do
  begin
    pgeDetail.ActivePage:=tbshtDetail3;
    NPartNum:=qryBrowse.FieldByName('PartNum').AsString;
    NRevision:=qryBrowse.FieldByName('Revision').AsString;
    if qryBrowse.FieldByName('MapType').AsInteger=0 then
    BEGIN
      //����ƪ���
      while qryDetail3.FieldByName('SerialNum').AsInteger<=3 do
      begin
        toFile:= trim(sRealMapPath)+trim(NPartNum)+trim(NRevision)+'_';
        if qryDetail3.FieldByName('SerialNum').AsInteger=1 then
          toFile:=toFile+'panel.jpg'
        else
          toFile:=toFile+'CUT.jpg';
        bmp:= TImage.Create(self);
        try
          bmp.Width := Round(XFlowDrawBox1.Width*0.65);//�L��
          bmp.Height := XFlowDrawBox1.Height;//�L��
          Re.Left:=0;
          Re.Top:=0;
          Re.Right:=XFlowDrawBox1.width;
          Re.Bottom:=XFlowDrawBox1.Height;
          bmp.Canvas.CopyRect(Re, XFlowDrawBox1.Canvas, Re);
          //ShowMessage(toFile);
          if not FileExists(toFile) then
            bmp.Picture.SaveToFile(toFile);
        finally
          bmp.free;
        end;
        qryDetail3.Next;
      end;
      //�A�����X��
      pgeDetail.ActivePage:=tbshtDetail9;
      toFile:= trim(sRealMapPath)+trim(NPartNum)+trim(NRevision)+'_stackup.jpg';
      bmp:= TImage.Create(self);
      try
        {ShowMessage(IntToStr(tblLayerPress.RecordCount));
        iCount:=iCount+1;
        if iCount=5 then
          qryBrowse.Last;}
        with qryExec do
        begin
          qryExec.Close;
          SQL.Clear;
          SQL.Add('select * from EMOdLayerPress(nolock) where PartNum='''+NPartNum+''''
                +' and Revision='''+NRevision+''' and LayerId=''L0~0''');
          Open;
        end;
        if qryExec.RecordCount>5 then
          bmp.Height := Round(XFlowDrawBox2.Height)//�L��
        else
          bmp.Height := Round(XFlowDrawBox2.Height*0.5);//�L��
        bmp.Width := Round(XFlowDrawBox2.Width*0.9);//�L��
        Re.Left:=0;
        Re.Top:=0;
        Re.Right:=XFlowDrawBox2.width;
        Re.Bottom:=XFlowDrawBox2.Height;
        bmp.Canvas.CopyRect(Re, XFlowDrawBox2.Canvas, Re);
        //ShowMessage(toFile);
        if not FileExists(toFile) then
          bmp.Picture.SaveToFile(toFile);
      finally
        bmp.free;
      end;
    END;
    qryBrowse.Next;
    //iCount:=iCount+1;
  end;
end;

procedure TfrmProductDLL.btnAutoDrawClick(Sender: TObject);
begin
  inherited;
  //2020.05.13 �۰ʦs��
  unit_DLL.prcSaveALL(frmProductDLL);

  AuditCheck;
  GetNewMapData(1);
  GetNewMapData(3);
  qryDetail3.Close;
  qryDetail3.Open;
  //2012.03.08
  UpdateDesigner;

  //2016.07.27
  if sCusId='YX' then
    qryBrowse.Refresh;

  //2020.05.08
  if sCusId='TCI' then
  begin
    qryDetail4.Close;
    qryDetail4.Open;
  end;
end;

procedure TfrmProductDLL.btnC1Click(Sender: TObject);
var iNeedInEdit:Integer;
    sBtnName, sNowLayer:String;
    iBtnResult:integer; //2012.12.24
begin
  iBtnResult:=0;
  iNeedInEdit:=StatusCheck(Sender);
  if iNeedInEdit=1 then
    AuditCheck;
  EditPartNum:=qryBrowse.FieldByName('PartNum').AsString;
  EditRevision:=qryBrowse.FieldByName('Revision').AsString;
  sBtnName:=TSpeedButton(Sender).name;
  //2011.11.02 ��h�u��
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select ItemId from CURdOCXItemCustButton(nolock) '
        +'where ItemId='''+sItemId+''' '
        +'and ButtonName='''+sBtnName+''' '
        +'and OCXName=''EMOdMoPrint.dll'' '
        +'and CustCaptionEN=''SingleLayer''');
    Open;
    if RecordCount>0 then
    begin
      if not Assigned(trvBOM.Selected) then Exit;
      sNowLayer:=TNodeData(trvBOM.Selected.Data^).Id;
      qryExec.Close;
      SQL.Clear;
      SQL.Add('exec EMOdPrintSPIdIns '''+sNowLayer+''','''+sUserId+''',0');
      Open;
    end;
    //2012.12.24     2013.01.08 ���G�T�w�襤�����s��i����
    //============================================================
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select SPName from CURdOCXItemCustButton(nolock) '
        +'where ItemId='''+sItemId+''' '
        +'and ButtonName='''+sBtnName+''' '
        +'and SPName=''EMOdGetFactorMo''');
    Open;
    if RecordCount>0 then
    begin
      iBtnResult:=1;
      if not qryDetail10.Active then
      begin
        MsgDlgJS('�Х������ܳ~�{����!!',mtWarning,[mbOK],0);
        abort;
      end;
    end;
    qryExec.Close;
    //============================================================
  end;
  inherited;
  if ((qryBrowse.FieldByName('PartNum').AsString<>EditPartNum)
      or
       (qryBrowse.FieldByName('Revision').AsString<>EditRevision))   then
  begin
    if qryBrowse.Locate('PartNum;Revision' ,
          VarArrayOf([EditPartNum,EditRevision]) ,[loPartialKey])=false then
      MsgDlgJS('��ƫ��w���ѡA�аh�X�@�~!!',mtWarning,[mbOK],0);
  end;
  tblProdLayer.Close;
  tblProdLayer.Open;
  qryProdHIO.Refresh;
  //2011.05.19 �ζq���� �n��s
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select ItemId from CURdOCXItemCustButton(nolock) '
        +'where ItemId='''+sItemId+''' '
        +'and ButtonName='''+sBtnName+''' '
        +'and SpName=''EMOdReCalcUsage''');
    Open;
    if RecordCount>0 then
    begin
      tblLayerPress.Close;
      tblLayerPress.Open;
      qryDetail11.Close;
      qryDetail11.Open;
    end;
  end;
  if iBtnResult=1 then //2012.12.24
  begin
    qryDetail10.Close;
    qryDetail10.Open;
  end;
end;

procedure TfrmProductDLL.btnCMapClick(Sender: TObject);
var sFileName, sJpgFile :String;
begin
  inherited;
  AuditCheck;
  if OpenDialog1.Execute then
  begin
    sFileName :=ExtractFileName(OpenDialog1.FileName);
    With qryExec do //�����ˬd
    Begin
      Close;
      SQL.Clear;
      SQL.Add('exec EMOdCheckFileName '''+sFileName+'''');
      Open;
    End;
    sJpgFile:= GetTempPathStr+ sFileName;
    if LowerCase(OpenDialog1.FileName)<>LowerCase(sJpgFile) then
       if not CopyFileStr(OpenDialog1.FileName, sJpgFile) then
          MsgDlgJS('�ɮ׽ƻs����!!!',mtWarning, [mbOk],0);
    if not (qryBrowse.State in [dsInsert ,dsEdit]) then
      qryBrowse.Edit;
    if Sender = btnCMap then
      qryBrowse.FieldByName('CMapPath').AsString := sJpgFile;
    if Sender = btnSMap then
      qryBrowse.FieldByName('SMapPath').AsString := sJpgFile;
  end;
end;

procedure TfrmProductDLL.btnCMapOpenClick(Sender: TObject);
begin
  inherited;
  if Sender = btnCMapOpen then
    ShellExecute(Handle,'open',PChar
        (qryBrowse.FieldByName('CMapPath').AsString),nil,nil,SW_SHOW);
  if Sender = btnSMapOpen then
    ShellExecute(Handle,'open',PChar
        (qryBrowse.FieldByName('SMapPath').AsString),nil,nil,SW_SHOW);
end;

procedure TfrmProductDLL.btnCompletedClick(Sender: TObject);
var istatus: integer;
begin
  //inherited;
  //0317 �אּ�e�f�\��
  //�W�[�@���ˬd�����o�Ѧ����s�i��ĤG���f��
  istatus := StrToInt(qryBrowse.FieldByName('Status').AsString) + 1;
  if istatus<>1 then
  begin
    MsgDlgJS('���A�D�]�p���A�L�k�e�f!!!',mtWarning, [mbOk],0);
    Abort;
  end;
  //2012.05.03 add
  if sNowMode='UPDATE' then
    btnKeepStatusClick(Sender);

  btnAdd.Enabled:=btnExam.Enabled;  //2022.11.30
  //2012.03.15
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdUpdateDesigner '''
                +qryBrowse.FieldByname('PartNum').AsString+''','''
                +qryBrowse.FieldByname('Revision').AsString+''','''
                +sUserId+''',2');
    ExecSql;
  end;
  btnExamClick(Sender);
end;

procedure TfrmProductDLL.btnCopyRouteClick(Sender: TObject);
var i:Integer;
    sNowPart, sNowRev, sNowLayer:String;
begin
  inherited;
  if qryBrowse.RecordCount<1 then
    Exit;
  StatusCheck(Sender);
  AuditCheck;
  sNowPart:= dsBrowse.DataSet.FieldByname('PartNum').AsString;
  sNowRev:= dsBrowse.DataSet.FieldByname('Revision').AsString;
  if not Assigned(trvBOM.Selected) then
    sNowLayer:= CurrLayer
  else
    sNowLayer:=TNodeData(trvBOM.Selected.Data^).Id;

   Application.createForm(TdlgCopySelect, dlgCopySelect);
   dlgCopySelect.sConnectStr:=sConnectStr;
   dlgCopySelect.prcDoSetConnOCX;
   with dlgCopySelect do
   begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryTargetLayer.CommandTimeout:=iTimeOut;
        qryLayerId.CommandTimeout:=iTimeOut;
      end;
     NowPN:=sNowPart;
     NowRev:=sNowRev;
     msCopy.TargetColumns:=msCopy.SourceColumns;
     qryLayerId.Close;
     qryLayerId.Open;
     edtPartNum.Text:= sNowPart;
     edtRevision.Text:= sNowRev;
     cboLayerId.Text:= sNowLayer;
      with qryTargetLayer do
      begin
         Close;
         Parameters.Parambyname('PartNum').Value:= NowPN;
         Parameters.Parambyname('Revision').Value:= NowRev;
         Parameters.ParambyName('LayerId').Value:= sNowLayer;
         Parameters.ParamByName('SourPartNum').Value:= edtPartNum.Text;
         Parameters.ParamByName('SourRevision').Value:= edtRevision.Text;
         Open;
      end;
      msCopy.Setup(slAll);
      Showmodal;
      if modalResult=mrok then
      begin
         with qryCopyRoute do
         begin
            for i:= 0 to msCopy.TargetItems.Count-1 do
            begin
               Close;
               Parameters.Parambyname('PartNum').Value:= sNowPart;
               Parameters.Parambyname('Revision').Value:= sNowRev;
               Parameters.ParambyName('LayerId').Value:=
                    msCopy.TargetItems[i].caption;
               Parameters.ParamByName('SourLayerId').Value:=
                    msCopy.TargetItems[i].SubItems[1];
               Parameters.Parambyname('SourPartNum').Value:= edtPartNum.Text;
               Parameters.Parambyname('SourRevision').Value:= edtRevision.Text;
               Open;
            end;
         end;
         //2012.03.08
         UpdateDesigner;
      end;
   end;
   qryTmpRouteId.Close;
   qryTmpRouteId.Open;
   qryDetail10.Close;
   qryDetail10.Open;
   qryDetail11.Close;
   qryDetail11.Open;
end;

procedure TfrmProductDLL.btnExamClick(Sender: TObject);
var //istatus: integer;
    Part, Rev:String;
    frmSaveMapX: TfrmMap;
    sNeedMap: String;
begin
   if qryBrowse.RecordCount<1 then
     Exit;
   //inherited;
   //2012.05.03 add
   if sNowMode='UPDATE' then
     btnKeepStatusClick(Sender);

   //2022.11.30
   if (qryBrowse.FieldByName('Status').AsInteger<>1)
    and (TSpeedButton(Sender).Name='btnAdd')
   then
   begin
      MessageDlg('���A�D�]�p�����A�L�k�e�f!!!',mtWarning, [mbOk],0);
      Abort;
   end;

   // btnUsage.Click;
   //0317 �אּ�f��(�ĤG���q)�\��
    Part := dsBrowse.DataSet.FieldByname('PartNum').AsString;
    Rev := dsBrowse.DataSet.FieldByname('Revision').AsString;
    With qryExec do //�����ˬd
    Begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('select Value=IsNull(Value,'''') from CURdSysParams(nolock)'
             +' where SystemId=''EMO'''
             +'and ParamId=''NeedMap''');
      Open;
      sNeedMap:=FieldByName('Value').AsString;

      qryExec.Close;
      SQL.Clear;
      SQL.Add('Exec EMOdFieldCheck '''+Part+''','''+Rev+''',''1''');
      Open;

    End;
    //istatus := StrToInt(qryBrowse.FieldByName('Status').AsString) + 1;
    //Ĳ�o�ƮƳq��  ���E���X���ݭn 20080307
    { If tblMaster.FieldByName('Status').AsString = '0' then
    Begin
      btnMailClick(sender);
    End; }

{    If StrToInt(tblMaster.FieldByName('Status').AsString) <> tag
    then
       ShowMessage('�L�k�f��!!!')
    else
    begin
      IF (istatus =2)then
      Begin
        With qryEditCheck do //�����ˬd
        Begin
          close;
          ParamByName('Partnum').AsString:=Part;
          ParamByName('Revision').AsString:=Rev;
          open;
          B:= FieldByName('V').AsInteger;
        END;
      End;   }

      With qryEditCheck do //�����ˬd
      Begin
        Close;
        Parameters.ParamByName('Partnum').Value:=Part;
        Parameters.ParamByName('Revision').Value:=Rev;
        Open;

      END;

      //2016.11.30 add for JH
      if sCusId='YX' then
      begin
        with qryExec do
        begin
          qryExec.Close;
          SQL.Clear;
          SQL.Add('exec EMOd_JH_XCalcChk '''+Part+''','''+Rev+'''');
          Open;
          if FieldByName('RtnType').AsInteger=1 then
          begin
            If MsgDlgJS(FieldByName('RtnValue').AsString,
              mtConfirmation,[mbNo,mbYes],0) = mrNo then
            begin
              Exit;
            end;
          end
          else if FieldByName('RtnType').AsInteger=2 then
          begin
            MsgDlgJS(FieldByName('RtnValue').AsString,mtWarning,[mbOK],0);
            Exit;
          end;
        end;
      end;

      If qryEditCheck.FieldByName('V').AsInteger > 0 then
      Begin
        If MsgDlgJS('��涵�جO�_�ק粒��',mtConfirmation,[mbYes,mbNo],0)
              = mrYes then
        begin
          if sNeedMap='1' then
          begin
            try
              frmSaveMapX:= TfrmMap.Create(Self);
              frmSaveMapX.sConnectStr:=sConnectStr;
              frmSaveMapX.prcDoSetConnOCX;
              frmSaveMapX.show;
              frmSaveMapX.Save2Map(qryBrowse.FieldByName('PartNum').AsString,
                  qryBrowse.FieldByName('Revision').AsString);
            finally
              frmSaveMapX.Free;
            end;
          end;
          ProdAudit(Sender);
        End;
      End
      Else
      Begin
          if sNeedMap='1' then
          begin
            try
              frmSaveMapX:= TfrmMap.Create(Self);
              frmSaveMapX.sConnectStr:=sConnectStr;
              frmSaveMapX.prcDoSetConnOCX;
              frmSaveMapX.show;
              frmSaveMapX.Save2Map(qryBrowse.FieldByName('PartNum').AsString,
                  qryBrowse.FieldByName('Revision').AsString);
            finally
              frmSaveMapX.Free;
            end;
          end;
        ProdAudit(Sender);

      End;
  //�f�ְh�f�����ҥ�
  AuditSetting;

  //2014.09.12
  qryDetail10.Close;
  qryDetail10.Open;



end;

procedure TfrmProductDLL.btnFinishClick(Sender: TObject);
begin
  inherited;

  //2022.11.30
  if qryBrowse.FieldByName('Status').AsInteger<>2 then
  begin
    MessageDlg('���A�D�w�f�֡A�L�k�e�f!!!',mtWarning, [mbOk],0);
    Abort;
  end;

  EditPartNum:=qryBrowse.FieldByName('PartNum').AsString;
  EditRevision:=qryBrowse.FieldByName('Revision').AsString;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdOtherStatusChg '''+
        qryBrowse.FieldByname('PartNum').asstring+''','''+
        qryBrowse.FieldByname('Revision').asstring+''',3,'''+
        sUserId+'''');
    Open;
    qryBrowse.Close;
    qryBrowse.Open;
    //2012.02.04
    qryProdHIO.Close;
    qryProdHIO.Open;
  end;
  if ((qryBrowse.FieldByName('PartNum').AsString<>EditPartNum)
      or
       (qryBrowse.FieldByName('Revision').AsString<>EditRevision))   then
  begin
    if qryBrowse.Locate('PartNum;Revision' ,
          VarArrayOf([EditPartNum,EditRevision]) ,[loPartialKey])=false then
      MsgDlgJS('��ƫ��w���ѡA�аh�X�@�~!!',mtWarning,[mbOK],0);
  end;
end;

procedure TfrmProductDLL.btnFunctionClick(Sender: TObject);
begin
  inherited;
  pnlFunction.Top:=pnlTempBasDLLBottom.Top - pnlFunction.Height +44;
  pnlFunction.Left:=btnFunction.Left;
  pnlFunction.BringToFront;
end;

procedure TfrmProductDLL.btnGetParamsClick(Sender: TObject);
//var i:integer;
//evCusBtnClickEvent:TevCusBtnClickEvent2;
var sPaperDtlTb7Caption,
    sPaperDtlTb8Caption,
    sPaperDtlTb9Caption,
    sPaperDtlTb10Caption,
    sPaperTopTb1Caption,
    sPaperTopTb2Caption,
    sMillsPos, sWritingPos, sRe :String;
    iSerial, iMasHeight, iDetHeight, iHideUseMat, iPartMergePrint, i:Integer;
    //2012.02.03
    iRouteHeight, iRouteWidth, iUpdateWidth:Integer;
    //2012.04.25
    sTmp :String;
    iUseFunc:Integer;
    //2020.03.11
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  inherited;
  
  //2016.05.05
  btnExam.Visible:=True;

  //2012.09.18 add
  prcEMODetailSet;

  //2012.04.25 �� Grid Title
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

  // �w�]�ȩw�q ****************************************************************
  iNeedChkDesigner:=1;
  sDesignerPN:='';
  sDesignerRev:=''; //2012.03.08
  //2011.11.21 LookupType
  qryDetail7B.LookupType:=lkLookupTable;
  qryDetail8B.LookupType:=lkLookupTable;
  qryDetail9.LookupType:=lkLookupTable;
  //qryDetail10.LookupType:=lkLookupTable; 2013.10.11 ��fkLookup��쪺Table�A���n��LookupType
  //qryDetail11.LookupType:=lkLookupTable; 2013.10.11 ��fkLookup��쪺Table�A���n��LookupType
  tblWriting.LookupType:=lkJoinSQL;
  tblMills.LookupType:=lkJoinSQL;
  tblModify.LookupType:=lkJoinSQL;
  tblLayerPress.LookupType:=lkLookupTable;
  tblPartMergePrint.LookupType:=lkJoinSQL;
  qryProdHIO.LookupType:=lkJoinSQL;
  //100617 ���F�i�H�d�ߨ�~�{�B�s�{�A�NDetail7��8 ����(��LayerInfo �P LayerRoute)
  //Grid��ܪ��OECN�P�ƮưO��(��n���OREADONLY ��Table)
  iNeedAct:=1;
  //100609 �d�ߥ[�J�w�]��(�ק���)
  sSeahchStr:='';
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdInqRange '''+sLanguageId+'''');
    ExecSql;
  end;
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
        evCusBtnClickEvent.oldCusBtnClickEvent2(TSpeedButton
                (FindComponent('btnC'+inttostr(i))));
    end;}
  btnCopyRoute.Align:=alLeft;
  CurrLayer:='L0~0';
  pgeFormTypeChange(Sender);
  sRealMapPath:='';
  // �w�]�ȩw�q end ************************************************************


  // �}�� **********************************************************************
  tblProdLayer.Close;
  tblProdLayer.Open;
  qryUserId.Close;
  qryUserId.Open;
  qryProdHIO.Close;
  qryProdHIO.Open;
  qryDetail9.Close;
  qryDetail9.Open;
  qryMatName.Close;
  qryMatName.Open;
  qryProcInfo.Close;
  qryProcInfo.Open;
  qryMap.Close;
  qryMap.Open;
  qryUnit.Close; //2020.06.04  �s�{�ήƳ��
  qryUnit.Open;  //2020.06.04
  LockButton(True);
  //�f�ְh�f�����ҥ�(Must After LockButton)
  AuditSetting;
  // �}�� end ******************************************************************


  //Item �Ѽ� ******************************************************************
  with qryExec do
  begin
    if active then qryExec.close;
    sql.Clear;
    sql.Add('select RuleId,DLLValue from CURdOCXItemOtherRule(nolock) '
            +' where ItemId='+''''+sItemId+'''');
    open;
  end;
  //����Title
  sPaperDtlTb7Caption:=tbshtDetail7.Caption;
  sPaperDtlTb8Caption:=tbshtDetail8.Caption;
  sPaperDtlTb9Caption:=tbshtDetail9.Caption;
  sPaperDtlTb10Caption:=tbshtDetail10.Caption;
  sPaperTopTb1Caption:=tstMain.Caption;
  sPaperTopTb2Caption:=tstSub.Caption;
  if qryExec.RecordCount>0 then
  begin
    if qryExec.Locate('RuleId', 'PaperDtlTb7Caption', [loCaseInsensitive]) then
      sPaperDtlTb7Caption:=trim(qryExec.Fields[1].AsString);
    if qryExec.Locate('RuleId', 'PaperDtlTb8Caption', [loCaseInsensitive]) then
      sPaperDtlTb8Caption:=trim(qryExec.Fields[1].AsString);
    if qryExec.Locate('RuleId', 'PaperDtlTb9Caption', [loCaseInsensitive]) then
      sPaperDtlTb9Caption:=trim(qryExec.Fields[1].AsString);
    if qryExec.Locate('RuleId', 'PaperDtlTb10Caption', [loCaseInsensitive]) then
      sPaperDtlTb10Caption:=trim(qryExec.Fields[1].AsString);
    if qryExec.Locate('RuleId', 'PaperTopTb1Caption', [loCaseInsensitive]) then
      sPaperTopTb1Caption:=trim(qryExec.Fields[1].AsString);
    if qryExec.Locate('RuleId', 'PaperTopTb2Caption', [loCaseInsensitive]) then
      sPaperTopTb2Caption:=trim(qryExec.Fields[1].AsString);
    //�ѤMpnl
    if qryExec.Locate('RuleId', 'tblMillsLocate', [loCaseInsensitive]) then
      sMillsPos:=trim(qryExec.Fields[1].AsString);
    if qryExec.Locate('RuleId', 'tblWritingLocate', [loCaseInsensitive]) then
      sWritingPos:=trim(qryExec.Fields[1].AsString);
    //���׳]�w�n�վ�
    if qryExec.Locate('RuleId', 'PaperMasterHeight', [loCaseInsensitive]) then
      iMasHeight:= qryExec.Fields[1].AsInteger;
    if qryExec.Locate('RuleId', 'PaperDetailHeight', [loCaseInsensitive]) then
      iDetHeight:= qryExec.Fields[1].AsInteger;
    //QU �Ƽt�W��
    if qryExec.Locate('RuleId', 'PaperMasTb6Caption', [loCaseInsensitive]) then
    begin
      tbshtMaster6.TabVisible:=True;
      tbshtMaster6.Visible:=True;
      tbshtMaster6.Caption:=trim(qryExec.Fields[1].AsString);
    end;
    //2023.04.06 new page
    if qryExec.Locate('RuleId', 'PaperMasTb7Caption', [loCaseInsensitive]) then
    begin
      tbshtMaster7.TabVisible:=True;
      tbshtMaster7.Visible:=True;
      tbshtMaster7.Caption:=trim(qryExec.Fields[1].AsString);
    end;
    //2023.04.06 new page
    if qryExec.Locate('RuleId', 'PaperMasTb8Caption', [loCaseInsensitive]) then
    begin
      tbshtMaster8.TabVisible:=True;
      tbshtMaster8.Visible:=True;
      tbshtMaster8.Caption:=trim(qryExec.Fields[1].AsString);
    end;
    //2023.04.06 new page
    if qryExec.Locate('RuleId', 'PaperMasTb9Caption', [loCaseInsensitive]) then
    begin
      tbshtMaster9.TabVisible:=True;
      tbshtMaster9.Visible:=True;
      tbshtMaster9.Caption:=trim(qryExec.Fields[1].AsString);
    end;
    //2023.04.06 new page
    if qryExec.Locate('RuleId', 'PaperMasTb10Caption', [loCaseInsensitive]) then
    begin
      tbshtMaster10.TabVisible:=True;
      tbshtMaster10.Visible:=True;
      tbshtMaster10.Caption:=trim(qryExec.Fields[1].AsString);
    end;
    //2023.04.06 new page
    if qryExec.Locate('RuleId', 'PaperMasTb11Caption', [loCaseInsensitive]) then
    begin
      tbshtMaster11.TabVisible:=True;
      tbshtMaster11.Visible:=True;
      tbshtMaster11.Caption:=trim(qryExec.Fields[1].AsString);
    end;
    //2012.02.03 Route Page Size
    if qryExec.Locate('RuleId', 'RouteLeftWidth', [loCaseInsensitive]) then
      iRouteWidth := qryExec.Fields[1].AsInteger;
    if qryExec.Locate('RuleId', 'RouteRightHeight', [loCaseInsensitive]) then
      iRouteHeight:= qryExec.Fields[1].AsInteger;
    //2012.08.17
    if qryExec.Locate('RuleId', 'UpdateLeftWidth', [loCaseInsensitive]) then
      iUpdateWidth:= qryExec.Fields[1].AsInteger;
    //2020.03.20
    if qryExec.Locate('RuleId', 'Detail4ForOther', [loCaseInsensitive]) then
    begin
      if qryExec.Fields[1].AsString = '1' then
      begin
        grdHole.Visible := False;
        Splitter4.Visible := False;
        SplitterMerge.Visible := False;
        gridDetail4.Align := alClient;
      end;
    end;


    tbshtDetail7.Caption:=sPaperDtlTb7Caption;
    tbshtDetail8.Caption:=sPaperDtlTb8Caption;
    tbshtDetail9.Caption:=sPaperDtlTb9Caption;
    tbshtDetail10.Caption:=sPaperDtlTb10Caption;
    tstMain.Caption:=sPaperTopTb1Caption;
    tstSub.Caption:=sPaperTopTb2Caption;
  end;
  //����Title end
  //�ѤMpnl
  if sMillsPos<>'' then
  begin
      iSerial:=0;
      while iSerial<3 do//pos(';',sMillsPos)>0 do
      begin
        sRe:='';
        if pos(';',sMillsPos)>0 then
          sRe:=copy(sMillsPos,1,pos(';',sMillsPos)-1)
        else
          sRe:=sMillsPos;
        if trim(sRe)<>'' then
        begin
          if iSerial=0 then
            lblWhere1.Caption:=sRe
          else if iSerial=1 then
            pnlMills.Left:=StrToInt(sRe)
          else
            pnlMills.Top:=StrToInt(sRe);
        end;
        sMillsPos:=trim(copy(sMillsPos,pos(';',sMillsPos)+1,length(sMillsPos)));
        iSerial:=iSerial+1;
      end;
  end;
  if sWritingPos<>'' then
  begin
      iSerial:=0;
      while iSerial<3 do//pos(';',sWritingPos)>0 do
      begin
        sRe:='';
        if pos(';',sWritingPos)>0 then
          sRe:=copy(sWritingPos,1,pos(';',sWritingPos)-1)
        else
          sRe:=sWritingPos;
        if trim(sRe)<>'' then
        begin
          if iSerial=0 then
            lblWhere2.Caption:=sRe
          else if iSerial=1 then
            pnlWriting.Left:=StrToInt(sRe)
          else
            pnlWriting.Top:=StrToInt(sRe);
        end;
        sWritingPos:=trim(copy(sWritingPos,pos(';',sWritingPos)+1,length(sWritingPos)));
        iSerial:=iSerial+1;
      end;
  end;
  //�ѤMpnl end

  //2020.03.11
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

  //���׽վ�
  if iMasHeight>0 then
    pnlMaster5.Height:=Round(iMasHeight*(FontSize/100));
  if iDetHeight>0 then
    dbgLayerPress.Height:=iDetHeight;
  sPressMapCut:='';
  //2012.02.03
  if iRouteWidth>0 then
    dbgRoute.Width:= iRouteWidth;
  if iRouteHeight>0 then
    pnlRouteNote.Height:= iRouteHeight;
  //2012.08.17
  if iUpdateWidth>0 then
    gridDetail6.Width:= iUpdateWidth;
  //Item �Ѽ� end **************************************************************


  //�t�ΰѼ� *******************************************************************
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdGetParamSelect ''''');
    Open;
    if Locate('ParamId', 'Host', [loCaseInsensitive]) then
      MailHost := FieldByName('Value').AsString;
    if Locate('ParamId', 'Port', [loCaseInsensitive]) then
      MailPort := FieldByName('Value').asInteger;
    //100929
    iUpdateECNLog:=0;
    if Locate('ParamId', 'UpdateECNLog', [loCaseInsensitive]) then
      iUpdateECNLog := FieldByName('Value').AsInteger;
    if Locate('ParamId', 'PressMapCut', [loCaseInsensitive]) then
      sPressMapCut := FieldByName('Value').AsString;
    if Locate('ParamId', 'CusId', [loCaseInsensitive]) then
      sCusId := FieldByName('Value').AsString;
        //---------------------------------
        //2011.04.22 add
        if sCusId<>'QU' then
        begin
          //lblWhatRev.Visible:=False;
          //cboWhatRev.Visible:=False;
          pnlMapTool3.Visible:=False;
        end;
        //2011.10.14 add(�]�tbtnFinish)
        if sCusId='MUT' then
        begin
          btnFinish.Visible:=btnExam.Visible;
          btnFinish.Caption:='�֭�';
          //�Ȩ�A�����Ϋ��s����
          btnModifyExl.Visible:=False;
          btnBackupNotes.Visible:=False;
          btnPasteNotes.Visible:=False;
          //2012.09.18 ���ǭ������ç令�� Sp EMOdProdFormSet �M�w
          //tbshtDetail5.TabVisible:=False;
          //tbshtDetail8.TabVisible:=False;
          //2012.08.17 �]�ܼf������
          pnlModifyTools.Visible:=False;
          dbgModify.Visible:=False;
          Panel2.Height:=Panel3.Height +5;
        end;
        if sCusId='NIS' then
        begin
          btnCMap.Visible:=False;
          btnSMap.Visible:=False;
          btnFinish.Visible:=btnExam.Visible;
        end;

        //2022.11.30 �אּ�зǪ�
        btnFinish.Visible:=btnExam.Visible;
        btnFinish.Enabled:=btnExam.Enabled;
        btnFinish.Caption:='�֭�';
        //---------------------------------
    //Hide UseMat
    iHideUseMat:=0;
    if Locate('ParamId', 'HideFstPanel', [loCaseInsensitive]) then
      iHideUseMat:=FieldByName('Value').AsInteger;
        //---------------------------------
        if iHideUseMat=1 then
        begin
          Panel13.Visible:=False;
          Splitter5.Visible:=False;
          gridDetail1.Visible:=False;
          dbgLayerPress.Align:=alClient;
        end
        //2013.08.20 add Display For NIS
        else if iHideUseMat=2 then
        begin
          qryDetail1.DataSource:=dsLayerPress;
          qryDetail1.MasterSource:=dsLayerPress;
          SpeedButton3.Visible:=False;
          btnProdUseMat.Visible:=False;
          //2020.12.15 Panel �]����
          Panel13.Visible:=False;
        end;
        //---------------------------------
    //2010.09.04
    if Locate('ParamId', 'JpgPath', [loCaseInsensitive]) then
      sRealMapPath:=FieldByName('Value').AsString;
    //2010.09.04 end
    //NH �ݨD�A�Ѳ��~���c�a�u�{��
    iMGNMap:=0;
    if Locate('ParamId', 'MGNMap', [loCaseInsensitive]) then
      iMGNMap:=FieldByName('Value').AsInteger;
        //---------------------------------
        if iMGNMap<>1 then
          tbshtDetail11.TabVisible:=False;
        //---------------------------------
    //NH �ݨD �֪��t�}Table���@
    iPartMergePrint:=0;
    if Locate('ParamId', 'PartMergePrint', [loCaseInsensitive]) then
      iPartMergePrint:=FieldByName('Value').AsInteger;
        //---------------------------------
        if iPartMergePrint=1 then
        begin
          pnlPartMergePrint.Visible:=True;
          SplitterMerge.Visible:=True;
        end;
        //---------------------------------
    //2011.08.22 �u�{�ϼҪ�
    if Locate('ParamId', 'MapTmp', [loCaseInsensitive]) then
    begin
      if FieldByName('Value').AsString<>'1' then
        pnlMapTool5.Visible:=False;
    end;
    //2011.09.27 �~�{�Ƶ��w�]��e
    sTmp:='';
    if Locate('ParamId', 'RouteNoteWidth', [loCaseInsensitive]) then
      sTmp:=FieldByName('Value').AsString;
        //---------------------------------
        if sTmp<>'' then
          DBMemo1.Width:=FieldByName('Value').AsInteger
        else
        begin
          DBMemo1.Align:=alClient;
          pnlNoteSep.Visible:=False;
        end;
        //---------------------------------
    //2012.04.12 Timeout Fail
    iTimeOut:=0;
    if Locate('ParamId', 'TimeOutSec', [loCaseInsensitive]) then
      iTimeOut:=FieldByName('Value').AsInteger;

    //2020.08.05 ScrollBar
    {if Locate('ParamId', 'MaxHeight', [loCaseInsensitive]) then
    begin
      iScrollHeight:=FieldByName('Value').AsInteger + 10;
    end
    else
    begin
      sclPnlMaster.Visible:=False;
      //sclPnlMaster2.Visible:=False;
    end;}

  end;
  //�t�ΰѼ� end ***************************************************************

  //2012.09.21
  sSubCusId:='';
  with qryExec do
  begin
      qryExec.close;
      sql.Clear;
      sql.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
        +' and ParamId=''SubCusId''');
      open;
      if RecordCount>0 then
        sSubCusId:=FieldByName('Value').AsString;
      qryExec.close;
  end;
  //2011.08.23 Function Panel
  //=========================================================================
  btnFunction.Align:=alRight;
  pnlFunction.SendToBack;
  btnFunction.Align:=alLeft;
  iUseFunc:=0;
  //�\����s�A�W���ಾ
  for i := 1 to TPanel(pnlFunction).ControlCount - 1 do
  begin
    if TSpeedButton(FindComponent('btnC'+IntToStr(i+8))).Caption<>'' then
    begin
      TSpeedButton(FindComponent('btnFunc'+IntToStr(i))).Caption:=
        TSpeedButton(FindComponent('btnC'+IntToStr(i+8))).Caption;
      TSpeedButton(FindComponent('btnFunc'+IntToStr(i))).Hint:=
        TSpeedButton(FindComponent('btnC'+IntToStr(i+8))).Hint;
      TSpeedButton(FindComponent('btnFunc'+IntToStr(i))).ShowHint:=
        TSpeedButton(FindComponent('btnC'+IntToStr(i+8))).ShowHint;
      TSpeedButton(FindComponent('btnC'+IntToStr(i+8))).Name:='btnHide'+IntToStr(i);
      TSpeedButton(FindComponent('btnHide'+IntToStr(i))).Visible:=False;
      TSpeedButton(FindComponent('btnFunc'+IntToStr(i))).Name:='btnC'+IntToStr(i+8);
      iUseFunc:=1;
    end;
  end;
  if iUseFunc=0 then
    btnFunction.Visible:=False;
  //=========================================================================
  tblLayerPress.Close;
  tblLayerPress.Open;
  //prcStoreFieldNeed_Def(self,qryExec); //for �j��j�g

  //2013.01.07 for MUT
  btnMU_Excel.Align:=alLeft;
  if ((sCusId<>'MUT') or (sSubCusId='C')) then
    btnMU_Excel.Visible:=False;
  //2013.01.08 for CMT
  if ((sCusId<>'MUT') or (UPPERCASE(sSubCusId)<>'C')) then
    pnlUseNotes.Visible:=False;

  btnModifySet.Enabled := btnUpdate.Enabled;

  if sCusId='YX' then
  begin
    btnUpdNote.Visible := True;
    Panel3.Visible := True;
    chkJHCoreCom.Visible:=True;
    chkJHCoreCom.DataSource:=dsBrowse;
    chkJHCoreCom.DataField:='PlusCompen';
    btnJHPressChg.Visible := True;
  end;

  //2017.07.31
  if ((sCusId='MUT') and (sSubCusId='C')) then
  begin
    with qryExec do
    begin
        qryExec.close;
        sql.Clear;
        sql.Add('exec EMOdLayerPressDLLStr');
        open;
        if RecordCount>0 then
        begin
          tblLayerPress.Close;
          tblLayerPress.SQL.Clear;
          tblLayerPress.SQL.Add(qryExec.FieldByName('Value').AsString);
        end;
        qryExec.close;
    end;
  end;

  //2020.01.03
  if sCusId='ECO' then
  begin
    pgeFormType.ActivePage:= tstSub;
    tstMain.TabVisible := False;
    pnlState.Visible := False;
    tbshtDetail9.TabVisible := False;
  end;

  //2020.08.04
  if FontSize<>100 then
  begin
    pgeDetail.ScaleBy(100,FontSize);
    pnlMapTools.ScaleBy(100,FontSize);
    //2020.12.15 add fix
    pnlMapTool1.Width:= Round(115 * FontSize / 100);
    btnAutoDraw.Width:= Round(106 * FontSize / 100);
    pnlMapTool2.Width:= Round(117 * FontSize / 100);
    btnPrint.Width:= Round(106 * FontSize / 100);
    pnlMapTool3.Width:= Round(143 * FontSize / 100);
    pnlMapTool4.Width:= Round(118 * FontSize / 100);
    chkViewMapData.Width:= Round(106 * FontSize / 100);
    pnlMapTool5.Width:= Round(81 * FontSize / 100);
    btnSaveMapTmp.Width:= Round(72 * FontSize / 100);
    pnlModifyTools.ScaleBy(100,FontSize);
    pnlPath.ScaleBy(100,FontSize);
    lab_annex.Width := Round(lab_annex.Width * (FontSize/100));
    lab_annex.Height := Round(lab_annex.Height * (FontSize/100));
    Panel22.Width := Round(Panel22.Width * (FontSize/100));
    Panel22.Height := Round(Panel22.Height * (FontSize/100));
  end;

  //2020.08.05
  //SetScroll;

  //2022.11.30 �f�֫��s�h�X��
  btnAdd.Enabled:=btnExam.Enabled;
  btnAdd.Visible:=btnExam.Visible;
  btnAdd.Caption:=btnExam.Caption;
  btnAdd.Glyph:=btnExam.Glyph;
end;

procedure TfrmProductDLL.btnHideFunctionClick(Sender: TObject);
begin
  inherited;
  pnlFunction.SendToBack;
end;

procedure TfrmProductDLL.btnInqClick(Sender: TObject);
begin
  //2012.04.25 add
  if (ViewStatus = vDetail) then
    btnViewClick(Sender);
  inherited;
  //2011.01.10 NH �j�M�ɭn��l�Ƹ��@�ֱa�X�� ���ե�
  {showMessage(sMas1);
  showMessage(sDtl1);
  showMessage(sSub1);}
end;

procedure TfrmProductDLL.btnJHPressChgClick(Sender: TObject);
var sTmpStr:String;
begin
  inherited;
  //2016.11.24
  sTmpStr:='exec EMOdPressChgFoil_JH ''' +
      dsBrowse.DataSet.FieldByname('PartNum').asstring + ''',''' +
      dsBrowse.DataSet.FieldByname('Revision').asstring +'''';

  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add(sTmpStr);
    ExecSql;
  end;
  MsgDlgJS('��է���!',mtInformation,[mbOk],0);
  ShowLayerMap;
end;

procedure TfrmProductDLL.btnKeepStatusClick(Sender: TObject);
begin
  LockButton(True);
  inherited;
  //�f�ְh�f�����ҥ�
  AuditSetting;

  btnAdd.Enabled:=btnExam.Enabled;  //2022.11.30
  {qryBrowse.First; 100716 �ק����w��@�Ƹ�
  if qryBrowse.Locate('PartNum;Revision' ,
      VarArrayOf([EditPartNum,EditRevision]) ,[loPartialKey])=false then
    ShowMessage('��ƫ��w���ѡA�аh�X�@�~!!');}
end;

procedure TfrmProductDLL.btnLayerClick(Sender: TObject);
begin
  inherited;
  trvBOMDblClick(Sender);
end;

procedure TfrmProductDLL.btnLayerPressInsClick(Sender: TObject);
var sPartNum,sRevision:String;
begin
  inherited;
  if qryBrowse.State in [dsEdit ,dsInsert] then qryBrowse.Post;
  AuditCheck;
  sPartNum:=dsBrowse.DataSet.FieldByname('PartNum').asstring;
  sRevision:=dsBrowse.DataSet.FieldByname('Revision').asstring;
  if qryProdLayer.Active then
    if qryProdLayer.RecordCount<=0 then
    begin
      MsgDlgJS('�Х��]�w[�զX�ҫ�]!!!',mtWarning, [mbOk],0);
      abort;
    end;

   Application.createForm(TdlgTmpPressSelect, dlgTmpPressSelect);
   dlgTmpPressSelect.sConnectStr:=sConnectStr;
   dlgTmpPressSelect.prcDoSetConnOCX;
   //�f�־���
     with qryExec do
     begin
       qryExec.Close;
       SQL.Clear;
       SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
              +' and ParamId=''TmpRouteActiveType'' and Value=''1''');
       Open;
       if RecordCount>0 then
         dlgTmpPressSelect.iTmpActive:=1
       else
         dlgTmpPressSelect.iTmpActive:=0;
     end;

   with dlgTmpPressSelect do
   begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryTmpBomDtl.CommandTimeout:=iTimeOut;
        Query1.CommandTimeout:=iTimeOut;
        qryMatClass.CommandTimeout:=iTimeOut;
        qryTmpMas.CommandTimeout:=iTimeOut;
        qryTmpDtl.CommandTimeout:=iTimeOut;
      end;
      qryMatClass.Close;
      qryMatClass.Open;
      dlgTmpPressSelect.edtTmpBomId.text:=
            dsBrowse.DataSet.FieldByname('TmpBomId').asstring;
      btnBrowseClick(Sender);
      qryTmpMas.Close;
      qryTmpMas.Open;
      qryTmpBOMDtl.Close;
      qryTmpBOMDtl.Open;
      //Create
      DBGrid2.Visible := False;
      with trvLayerPress do
      begin
        Setup;
        FullExpand;
        if Items.count >0 then
          Select(Items.item[0]);
      end;
      //Create end
      btnBrowseClick(Sender);
      Showmodal;
      if modalResult=mrok then
      begin
         with qryExec do
         begin
           Close;
           SQL.Clear;
           SQL.Add('exec EMOdInsLayerPress '''+qryTmpMas.FieldByname('TmpId').asstring
              +''', '''+sPartNum+''', '''+sRevision+'''');
           execsql;
           //dsBrowse.DataSet.Refresh;
         end;
         //2012.03.08
         UpdateDesigner;
      end;
   end;
   qryBrowse.Refresh;
   tblLayerPress.Close;
   tblLayerPress.Open;
   {100716 �ק����w��@�Ƹ�
   if qryBrowse.Locate('PartNum;Revision' ,
      VarArrayOf([sPartNum,sRevision]) ,[loPartialKey])=false then
     ShowMessage('��ƫ��w���ѡA�аh�X�@�~!!');}
end;

procedure TfrmProductDLL.btnLayerPressUpdateClick(Sender: TObject);
begin
  inherited;
  AuditCheck;
  if qryProdLayer.Active then
    if qryProdLayer.RecordCount<=0 then
    begin
      MsgDlgJS('�Х��]�w[�զX�ҫ�]!!!',mtWarning, [mbOk],0);
      abort;
    end;

  if sCusId='YX' then
    PressSet_JH
  else
    PressSetDefault;
end;

procedure TfrmProductDLL.PressSetDefault;
var i: integer;
    iRange, iGrdHeight, iPressCount: integer;
    NowLayer: String;
begin
   if not Assigned(trvBOM.Selected) then
     NowLayer:= CurrLayer
   else
     NowLayer:=TNodeData(trvBOM.Selected.Data^).Id;

   //2016.09.10
   Application.createForm(TdlgLayerPressSet, dlgLayerPressSet);
   dlgLayerPressSet.sConnectStr:=sConnectStr;
   dlgLayerPressSet.prcDoSetConnOCX;
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
       dlgLayerPressSet.cboClassMat.visible:=True;
       dlgLayerPressSet.cboClassMat.Text:=FieldByName('Value').AsString;
       dlgLayerPressSet.sClassMat:=FieldByName('Value').AsString;
     end;
   end;

   with dlgLayerPressSet do
   begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryLayerPress.CommandTimeout:=iTimeOut;
        qrySupplier.CommandTimeout:=iTimeOut;
        qryMatCodeCheck.CommandTimeout:=iTimeOut;
        qryLayerCheck.CommandTimeout:=iTimeOut;
        qryPressMat.CommandTimeout:=iTimeOut;
        qryProdLayer.CommandTimeout:=iTimeOut;
        qryMatClass.CommandTimeout:=iTimeOut;
        qryClassMat.CommandTimeout:=iTimeOut;
        tblPress.CommandTimeout:=iTimeOut;
        qryLayerPress.CommandTimeout:=iTimeOut;
        qryProdLayer.CommandTimeout:=iTimeOut;
        qryClassMat.CommandTimeout:=iTimeOut;
        //2016.07.22
        qryPressMat_JH.CommandTimeout:=iTimeOut;
      end;
      //2016.07.22
      //======================================
      //2023.08.02 cancel
      {if sCusId<>'YX' then
      begin
        pnlCond_JH.Visible:=False;
        btnSort.Visible:=False;
      end;
      if sDefClass='' then
        pnlCond_JH.Visible:=False;  }
      //======================================
      dlgLayerPressSet.qryProdLayer.Close;
      dlgLayerPressSet.qryProdLayer.Open;
      qryMatClass.Close;
      qryMatClass.Parameters.ParamByName('ClassMat').Value:=sClassMat;
      qryMatClass.Open;
      qryClassMat.Close;
      qryClassMat.Open;
      tblPress.Close;
      tblPress.Parameters.ParamByName('PartNum').Value:=
            dsBrowse.DataSet.FieldByname('PartNum').asstring;
      tblPress.Parameters.ParamByName('Revision').Value:=
            dsBrowse.DataSet.FieldByname('Revision').asstring;
      tblPress.Parameters.ParamByName('LayerId').Value:=NowLayer;
      tblPress.Open;
      //Create
      //�e���վ�
      iRange:=qryMatClass.RecordCount div 4;
      if iRange>3 then
      begin
        dlgLayerPressSet.Height:= dlgLayerPressSet.Height+(13*(iRange-3));
        pnlTOP.Height:=pnlTOP.Height+(13*(iRange-3));
        cboClassMat.Height:=cboClassMat.Height+(13*(iRange-3));  //2023.08.02 rdoMatClass->cboClassMat
        dbgPress.Height:=dbgPress.Height+(13*(iRange-3));
      end;
        //100508 �[�J�k��grid�P�_
        iPressCount:=tblPress.RecordCount;
        //100913 add �W��
        if iPressCount>16 then iPressCount:=16;
        if (iPressCount * 20)+23 > dbgPress.Height then
        begin
        if(iPressCount>8)then
        iPressCount:=8;
          iGrdHeight:=((iPressCount * 20)+23) - dbgPress.Height;
          dlgLayerPressSet.Height:=dlgLayerPressSet.Height + iGrdHeight;
          pnlTOP.Height:=pnlTOP.Height + iGrdHeight;
          cboClassMat.Height:=cboClassMat.Height + iGrdHeight;  //2023.08.02 rdoMatClass->cboClassMat
          dbgPress.Height:=dbgPress.Height + iGrdHeight;
        end;

      with qryMatCode do
      begin
         Parameters.Parambyname('PartNum').Value:=
              dsBrowse.DataSet.FieldByname('PartNum').asstring;
         Parameters.Parambyname('Revision').Value:=
              dsBrowse.DataSet.FieldByname('Revision').asstring;
         Parameters.ParambyName('LayerId').Value:= NowLayer;
         execsql;
      end;
      with qryLayerPress do
      begin
         Close;
         Parameters.Parambyname('PartNum').Value:=
              dsBrowse.DataSet.FieldByname('PartNum').asstring;
         Parameters.Parambyname('Revision').Value:=
              dsBrowse.DataSet.FieldByname('Revision').asstring;
         Parameters.ParambyName('LayerId').Value:= NowLayer;
         Open;
      end;
      msSelects.Setup(slAll);
      cboLayerId.Text:= NowLayer;
      pCurrPartNum:=dsBrowse.DataSet.FieldByname('PartNum').asstring;
      pCurrRevision:= dsBrowse.DataSet.FieldByname('Revision').asstring;
      pCurrLayerId:=NowLayer;
      Showmodal;
      if modalResult=mrok then
      //2011.05.02 notes when update here, need check procedure EMOdEditPressBack
      begin
        for i:= 0 to msSelects.TargetItems.Count-1 do
        Begin
          With qryMatCodeCheck do
          Begin
            Close;
            Parameters.ParamByName('MatCode').Value:=
                  msSelects.TargetItems[i].Caption;
            Open;
            If FieldByName('N').AsInteger = 1 then
            Begin
              MsgDlgJS('�L�k�ϥ����{�ɽs�X�μȰ�������,�Ч�s',
                  mtInformation,[mbOk],0);
              exit;
            End;
          End;
        End;
        //100508 ��g�k
        with qryExec do
        begin
          for i:= 0 to msSelects.TargetItems.Count-1 do
          begin
            qryExec.Close;
            SQL.Clear;
            SQL.Add('exec EMOdLayerPressModify '''
                  +dsBrowse.DataSet.FieldByname('PartNum').asstring+''', '''
                  +dsBrowse.DataSet.FieldByname('Revision').asstring+''', '''
                  +NowLayer+''', '
                  +IntToStr(i+1)+', '''
                  +msSelects.TargetItems[i].Caption+''', N'''
                  +msSelects.TargetItems[i].SubItems[0]+'''');
            execsql;
          end;
          qryLayerPress_New.Close;
          qryLayerPress_New.Open;
          qryExec.Close;
          SQL.Clear;
          SQL.Add('exec EMOdLayerPressTest '''
            +dsBrowse.DataSet.FieldByname('PartNum').asstring+''', '''
            +dsBrowse.DataSet.FieldByname('Revision').asstring+''', '''
            +NowLayer+'''');
          open;
          if fieldbyname('Y').AsString = 'Y' then
          begin
            MsgDlgJS('�󴫧��Ʀb�w�o�Ʊ��p�U�ݳq���˫~�H���μf�֪�',
                mtInformation,[mbOk],0);
          end;
        end;
        With qryPressNull do
        Begin
          Parameters.Parambyname('PartNum').Value:=
            dsBrowse.DataSet.FieldByname('PartNum').asstring;
          Parameters.Parambyname('Revision').Value:=
            dsBrowse.DataSet.FieldByname('Revision').asstring;
          Parameters.Parambyname('N').Value := 2;
          ExecSql;
        End;
         //2012.03.08
         UpdateDesigner;
        {qryProdLayer.Close;
        qryProdLayer.Open;}

        //2016.07.28
        if sCusId='YX' then
        begin
          qryDetail3.Close;
          qryDetail3.Open;
        end;
      end;
   end;
   tblLayerPress.Close;
   tblLayerPress.Open;
end;

procedure TfrmProductDLL.PressSet_JH;
var i, i_JH: integer;
    iRange, iGrdHeight, iPressCount: integer;
    NowLayer: String;
begin
   if not Assigned(trvBOM.Selected) then
     NowLayer:= CurrLayer
   else
     NowLayer:=TNodeData(trvBOM.Selected.Data^).Id;

   //2016.09.10
   Application.createForm(TdlgLayerPressSet_JH, dlgLayerPressSet_JH);
   dlgLayerPressSet_JH.sConnectStr:=sConnectStr;
   dlgLayerPressSet_JH.prcDoSetConnOCX;
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
       dlgLayerPressSet_JH.cboClassMat.visible:=True;
       dlgLayerPressSet_JH.cboClassMat.Text:=FieldByName('Value').AsString;
       dlgLayerPressSet_JH.sClassMat:=FieldByName('Value').AsString;
       //2016.07.22
       if sCusId='YX' then
         dlgLayerPressSet_JH.sDefClass:= dlgLayerPressSet_JH.sClassMat;
     end;
   end;

   with dlgLayerPressSet_JH do
   begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryLayerPress.CommandTimeout:=iTimeOut;
        qrySupplier.CommandTimeout:=iTimeOut;
        qryMatCodeCheck.CommandTimeout:=iTimeOut;
        qryLayerCheck.CommandTimeout:=iTimeOut;
        qryPressMat.CommandTimeout:=iTimeOut;
        qryProdLayer.CommandTimeout:=iTimeOut;
        qryMatClass.CommandTimeout:=iTimeOut;
        qryClassMat.CommandTimeout:=iTimeOut;
        tblPress.CommandTimeout:=iTimeOut;
        qryLayerPress.CommandTimeout:=iTimeOut;
        qryProdLayer.CommandTimeout:=iTimeOut;
        qryClassMat.CommandTimeout:=iTimeOut;
        //2016.07.22
        qryPressMat_JH.CommandTimeout:=iTimeOut;
      end;
      //2016.07.22
      //======================================
      if sCusId<>'YX' then
      begin
        pnlCond_JH.Visible:=False;
        btnSort.Visible:=False;
      end;
      if sDefClass='' then
        pnlCond_JH.Visible:=False;

      if pnlCond_JH.Visible=True then
      begin
        dsPressMat.DataSet :=qryPressMat_JH;
        i_JH:=1;
        while i_JH<=13 do
        begin
          TJSdLabel(dlgLayerPressSet_JH.FindComponent('lblCond'
                    +IntToStr(i_JH))).Visible :=False;
          TJSdLookupCombo(dlgLayerPressSet_JH.FindComponent('JSdLookupCombo'
                    +IntToStr(i_JH))).Visible :=False;
          inc(i_JH);
        end;

        with qryExec do
        begin
          qryExec.Close;
          SQL.Clear;
          SQL.Add('exec EMOdPressCondSet_JH');
          Open;
          if RecordCount>0 then
          begin
            First;
            while not eof do
            begin
              i_JH:=FieldByName('SerialNum').AsInteger;
              if ((i_JH>0) and (i_JH<14)) then
              begin
                if FieldByName('NumName').AsString<>'' then
                begin
                  TJSdLabel(dlgLayerPressSet_JH.FindComponent('lblCond'
                    +IntToStr(i_JH))).Caption:=FieldByName('NumName').AsString;
                  TJSdLabel(dlgLayerPressSet_JH.FindComponent('lblCond'
                    +IntToStr(i_JH))).Visible :=True;
                  TJSdLookupCombo(dlgLayerPressSet_JH.FindComponent('JSdLookupCombo'
                    +IntToStr(i_JH))).Visible :=True;
                  if FieldByName('SelectStr').AsString<>'' then
                  begin
                    TADOQuery(dlgLayerPressSet_JH.FindComponent('qryCond'
                      +IntToStr(i_JH))).Close;
                    TADOQuery(dlgLayerPressSet_JH.FindComponent('qryCond'
                      +IntToStr(i_JH))).SQL.Clear;
                    TADOQuery(dlgLayerPressSet_JH.FindComponent('qryCond'
                      +IntToStr(i_JH))).SQL.Add(FieldByName('SelectStr').AsString);
                    TADOQuery(dlgLayerPressSet_JH.FindComponent('qryCond'
                      +IntToStr(i_JH))).Open;
                  end;
                end;
              end;
              next;
            end;
          end;//if RecordCount>0 then
        end;
      end;
      //======================================
      dlgLayerPressSet_JH.qryProdLayer.Close;
      dlgLayerPressSet_JH.qryProdLayer.Open;
      qryMatClass.Close;
      qryMatClass.Parameters.ParamByName('ClassMat').Value:=sClassMat;
      qryMatClass.Open;
      qryClassMat.Close;
      qryClassMat.Open;
      tblPress.Close;
      tblPress.Parameters.ParamByName('PartNum').Value:=
            dsBrowse.DataSet.FieldByname('PartNum').asstring;
      tblPress.Parameters.ParamByName('Revision').Value:=
            dsBrowse.DataSet.FieldByname('Revision').asstring;
      tblPress.Parameters.ParamByName('LayerId').Value:=NowLayer;
      tblPress.Open;
      //Create
      //�e���վ�
      iRange:=qryMatClass.RecordCount div 4;
      if iRange>3 then
      begin
        dlgLayerPressSet_JH.Height:= dlgLayerPressSet_JH.Height+(13*(iRange-3));
        pnlTOP.Height:=pnlTOP.Height+(13*(iRange-3));
        rdoMatClass.Height:=rdoMatClass.Height+(13*(iRange-3));
        dbgPress.Height:=dbgPress.Height+(13*(iRange-3));
      end;
        //100508 �[�J�k��grid�P�_
        iPressCount:=tblPress.RecordCount;
        //100913 add �W��
        if iPressCount>16 then iPressCount:=16;
        if (iPressCount * 20)+23 > dbgPress.Height then
        begin
          iGrdHeight:=((iPressCount * 20)+23) - dbgPress.Height;
          dlgLayerPressSet_JH.Height:=dlgLayerPressSet_JH.Height + iGrdHeight;
          pnlTOP.Height:=pnlTOP.Height + iGrdHeight;
          rdoMatClass.Height:=rdoMatClass.Height + iGrdHeight;
          dbgPress.Height:=dbgPress.Height + iGrdHeight;
        end;

      //2016.07.22
      if pnlCond_JH.Visible=True then
      begin
        if dbgPress.Height>pnlCond_JH.Height then
        begin
          //0725
          if (dbgPress.Height - 45)>115 then
            pnlCond_JH.Height := dbgPress.Height - 45;
        end
        else
        begin
          iGrdHeight:=pnlCond_JH.Height - dbgPress.Height;
          dlgLayerPressSet_JH.Height:=dlgLayerPressSet_JH.Height + iGrdHeight;
          pnlTOP.Height:=pnlTOP.Height + iGrdHeight;
          rdoMatClass.Height:=rdoMatClass.Height + iGrdHeight;
          dbgPress.Height:=dbgPress.Height + iGrdHeight;
        end;
      end;

      with qryMatCode do
      begin
         Parameters.Parambyname('PartNum').Value:=
              dsBrowse.DataSet.FieldByname('PartNum').asstring;
         Parameters.Parambyname('Revision').Value:=
              dsBrowse.DataSet.FieldByname('Revision').asstring;
         Parameters.ParambyName('LayerId').Value:= NowLayer;
         execsql;
      end;
      with qryLayerPress do
      begin
         Close;
         Parameters.Parambyname('PartNum').Value:=
              dsBrowse.DataSet.FieldByname('PartNum').asstring;
         Parameters.Parambyname('Revision').Value:=
              dsBrowse.DataSet.FieldByname('Revision').asstring;
         Parameters.ParambyName('LayerId').Value:= NowLayer;
         Open;
      end;
      msSelects.Setup(slAll);
      cboLayerId.Text:= NowLayer;
      pCurrPartNum:=dsBrowse.DataSet.FieldByname('PartNum').asstring;
      pCurrRevision:= dsBrowse.DataSet.FieldByname('Revision').asstring;
      pCurrLayerId:=NowLayer;
      Showmodal;
      if modalResult=mrok then
      //2011.05.02 notes when update here, need check procedure EMOdEditPressBack
      begin
        for i:= 0 to msSelects.TargetItems.Count-1 do
        Begin
          With qryMatCodeCheck do
          Begin
            Close;
            Parameters.ParamByName('MatCode').Value:=
                  msSelects.TargetItems[i].Caption;
            Open;
            If FieldByName('N').AsInteger = 1 then
            Begin
              MsgDlgJS('�L�k�ϥ����{�ɽs�X�μȰ�������,�Ч�s',
                  mtInformation,[mbOk],0);
              exit;
            End;
          End;
        End;
        //100508 ��g�k
        with qryExec do
        begin
          for i:= 0 to msSelects.TargetItems.Count-1 do
          begin
            qryExec.Close;
            SQL.Clear;
            SQL.Add('exec EMOdLayerPressModify '''
                  +dsBrowse.DataSet.FieldByname('PartNum').asstring+''', '''
                  +dsBrowse.DataSet.FieldByname('Revision').asstring+''', '''
                  +NowLayer+''', '
                  +IntToStr(i+1)+', '''
                  +msSelects.TargetItems[i].Caption+''', N'''
                  +msSelects.TargetItems[i].SubItems[0]+'''');
            execsql;
          end;
          qryLayerPress_New.Close;
          qryLayerPress_New.Open;
          qryExec.Close;
          SQL.Clear;
          SQL.Add('exec EMOdLayerPressTest '''
            +dsBrowse.DataSet.FieldByname('PartNum').asstring+''', '''
            +dsBrowse.DataSet.FieldByname('Revision').asstring+''', '''
            +NowLayer+'''');
          open;
          if fieldbyname('Y').AsString = 'Y' then
          begin
            MsgDlgJS('�󴫧��Ʀb�w�o�Ʊ��p�U�ݳq���˫~�H���μf�֪�',
                mtInformation,[mbOk],0);
          end;
        end;
        With qryPressNull do
        Begin
          Parameters.Parambyname('PartNum').Value:=
            dsBrowse.DataSet.FieldByname('PartNum').asstring;
          Parameters.Parambyname('Revision').Value:=
            dsBrowse.DataSet.FieldByname('Revision').asstring;
          Parameters.Parambyname('N').Value := 2;
          ExecSql;
        End;
         //2012.03.08
         UpdateDesigner;
        {qryProdLayer.Close;
        qryProdLayer.Open;}

        //2016.07.28
        if sCusId='YX' then
        begin
          qryDetail3.Close;
          qryDetail3.Open;
        end;
      end;
   end;
   tblLayerPress.Close;
   tblLayerPress.Open;
end;

procedure TfrmProductDLL.btnMapUpdateClick(Sender: TObject);
var frmSaveMapX: TfrmMap;
begin
  inherited;
  if qryBrowse.RecordCount<1 then
    Exit;
  AuditCheck;
  try
    frmSaveMapX:= TfrmMap.Create(Self);
    frmSaveMapX.sConnectStr:=sConnectStr;
    frmSaveMapX.prcDoSetConnOCX;
    frmSaveMapX.show;
    frmSaveMapX.Save2Map(qryBrowse.FieldByName('PartNum').AsString,
        qryBrowse.FieldByName('Revision').AsString);
  finally
    frmSaveMapX.Free;
  end;
  MsgDlgJS('��s����',mtInformation,[mbOK],0);
end;

procedure TfrmProductDLL.btnMU_ExcelClick(Sender: TObject);
var qryProc: TJSdTable;
    sdbPath, SQLStmts, LocaldbName: WideString;
    sProcName, sChkLayer: String;
begin
  inherited;
  //2013.01.03
  sProcName:='EMOd�˫~�s�@�^�X��';
  qryProc:= TJSdTable.Create(nil);
  qryProc.EnableBCD:=false;//2010.6.23 add for �ѨM������Xmdb�ɡAdecimal DataType �ܦ� Memo�����D

  sdbPath:= DLLGetTempPathStr+sBUID+'\';

  if not Assigned(trvBOM.Selected) then
  begin
    MsgDlgJS('�Х���ܼh�O!!',mtWarning, [mbOk],0);
    Exit;
  end;
  sChkLayer:=TNodeData(trvBOM.Selected.Data^).Id;

  SQLStmts:= Proc2QueryDLL(sProcName,
      [qryBrowse.FieldByName('PartNum').AsString,
      qryBrowse.FieldByName('Revision').AsString,
      sChkLayer],
      sConnectStr);
  Query2DataSetDLL(SQLStmts, qryProc, sConnectStr);

  DataSet2ExcelDLL_EMO(qryProc, LocaldbName, sProcName, 'EMOd�˫~�s�@�^�X��',
    0, sConnectStr, qryExec2);
end;

procedure TfrmProductDLL.btnOpenRouteNoteClick(Sender: TObject);
begin
  inherited;
  ShellExecute(Handle,'open',PChar
        (qryDetail10.FieldByName('NotesPath').AsString),nil,nil,SW_SHOW);
end;

procedure TfrmProductDLL.btnPressChangeClick(Sender: TObject);
var sPartNum, sRevision, sPressLayer:String;
begin
  inherited;
  if qryBrowse.State in [dsEdit ,dsInsert] then qryBrowse.Post;
  AuditCheck;
  if qryProdLayer.Active then
    if qryProdLayer.RecordCount<=0 then
    begin
      MsgDlgJS('�Х��]�w[�զX�ҫ�]!!!',mtWarning, [mbOk],0);
      abort;
    end;
  sPartNum:=qryBrowse.FieldByName('PartNum').AsString;
  sRevision:=qryBrowse.FieldByName('revision').AsString;
  if not Assigned(trvBOM.Selected) then
    sPressLayer:= CurrLayer
  else
    sPressLayer:=TNodeData(trvBOM.Selected.Data^).Id;

  Application.createForm(TdlgPressChange, dlgPressChange);
  dlgPressChange.sConnectStr:=sConnectStr;
  dlgPressChange.prcDoSetConnOCX;
  with dlgPressChange, qryProdLayer do
  begin
    //2012.04.12 Timeout Fail
    if iTimeOut>0 then
    begin
      qryPosChange.CommandTimeout:=iTimeOut;
      qryReLoad.CommandTimeout:=iTimeOut;
      qryCloseCheck.CommandTimeout:=iTimeOut;
      qryClear.CommandTimeout:=iTimeOut;
      qryExec.CommandTimeout:=iTimeOut;
      qryMatClass.CommandTimeout:=iTimeOut;
      qryTmpPressMas.CommandTimeout:=iTimeOut;
    end;
    sUpdateUser:=sUserId;
    qryMatClass.Close;
    qryMatClass.Open;
    with qryExec do
    begin
      Close;
      SQL.Clear;
      SQL.Add('exec EMOdProdPressBefChg '''+sPartNum+''','''+sRevision
        +''','''+sPressLayer+'''');
      Open;
      iSpId:=FieldByName('SpId').AsInteger;
    end;
    GetData(qryBrowse.FieldByName('PartNum').AsString,
            qryBrowse.FieldByName('revision').AsString,
            sPressLayer);
    edtLayer.Text:= sPressLayer;
    Showmodal;
  end;
  qryBrowse.Refresh;
  tblLayerPress.Close;
  tblLayerPress.Open;
  {100716 �ק����w��@�Ƹ�
  if qryBrowse.Locate('PartNum;Revision' ,
      VarArrayOf([sPartNum,sRevision]) ,[loPartialKey])=false then
    ShowMessage('��ƫ��w���ѡA�аh�X�@�~!!');}
end;

procedure TfrmProductDLL.btnPrintClick(Sender: TObject);
var bmp: TImage;
    Re:TRect;
    toFile: string;
begin
  inherited;
   if SavePictureDialog1.Execute then
   begin
     toFile:= SavePictureDialog1.FileName;
     bmp:= TImage.Create(self);
     try
       bmp.Width := XFlowDrawBox1.Width;
       bmp.Height := XFlowDrawBox1.Height;
       Re.Left:=0;
       Re.Top:=0;
       Re.Right:=XFlowDrawBox1.width;
       Re.Bottom:=XFlowDrawBox1.Height;
       bmp.Canvas.CopyRect(Re, XFlowDrawBox1.Canvas, Re);
       bmp.Picture.SaveToFile(toFile);
     finally
       bmp.free;
     end;
   end;
end;

procedure TfrmProductDLL.btnPrintPaperClick(Sender: TObject);
var iShowTitle: integer;
    tLink: TLinkType;
    tDisplay: TDisplayType;
    JsRpt: TJSdReport;
    sRptName: String;
begin
   iShowTitle:= 0;
   tDisplay := dtActiveX;
   tLink:= ltAccess;
   hMain_btnPrint_Handle:=0;

   try
     JsRpt:= TJSdReport.Create(nil);
     JsRpt.ReportTitle:= '';
     JsRpt.ReportFileName:='EMOdPrintPaper.rpt';
     sRptName:=JsRpt.ReportFileName;
     with JsRpt do
     begin
        ReportServer := unit_DLL.funReportServerGet(qryExec,sUseId,'');;
        //LinkType := tLink;
        DisplayType := tDisplay;
     end;
     JsRpt.LinkType:=tLink;
     sConnectStr:=qryExec.ConnectionString;

     unit_DLL.ShowWinReportDLL(JsRpt,'EMOdPrintPaper',sUseId,sUserId,'',sRptName,'',
          2,iShowTitle,[qryBrowse.FieldbyName('PartNum').AsString+';'
            +qryBrowse.FieldbyName('Revision').AsString],nil,sConnectStr,
            hMain_btnPrint_Handle,
          '',1);
   finally
     JsRpt.Free;
   end;
end;

procedure TfrmProductDLL.btnProdUseMatClick(Sender: TObject);
var i: integer;
    sSQL, sErr: string;
    iRange: Integer;
begin
  inherited;
  AuditCheck;
   Application.createForm(TdlgProdUseMatSet, dlgProdUseMatSet);
   dlgProdUseMatSet.sConnectStr:=sConnectStr;
   dlgProdUseMatSet.prcDoSetConnOCX;
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
       dlgProdUseMatSet.cboClassMat.visible:=True;
       dlgProdUseMatSet.cboClassMat.Text:=FieldByName('Value').AsString;
       dlgProdUseMatSet.sClassMat:=FieldByName('Value').AsString;
     end;
   end;

   with dlgProdUseMatSet do
   begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryProdPressMat.CommandTimeout:=iTimeOut;
        qryLayerPress.CommandTimeout:=iTimeOut;
        qrySupplier.CommandTimeout:=iTimeOut;
        qry2MatClass.CommandTimeout:=iTimeOut;
        qryProdLayer.CommandTimeout:=iTimeOut;
        qryClassMat.CommandTimeout:=iTimeOut;
      end;
      qry2MatClass.Close;
      qry2MatClass.Parameters.ParamByName('ClassMat').Value:=sClassMat;
      qry2MatClass.Open;
      qryClassMat.Close;
      qryClassMat.Open;
      qryProdLayer.Close;
      qryProdLayer.Open;

      //Create
      iRange:=qry2MatClass.RecordCount div 4;
      if iRange>3 then
      begin
        dlgProdUseMatSet.Height:= dlgProdUseMatSet.Height+(13*(iRange-3));
        pnlTOP.Height:=pnlTOP.Height+(13*(iRange-3));
        rdoMatClass.Height:=rdoMatClass.Height+(13*(iRange-3));
      end;

      BPartNum:= qryBrowse.FieldByname('PartNum').asstring;
      BRevision:= qryBrowse.FieldByname('Revision').asstring;
      dlgProdUseMatSet.btFindClick(Sender);
      Showmodal;
      if modalResult=mrok then
      begin
        with qryExec do
        begin
          qryDetail1.Close;
          sErr:= '';
          sSQL:= 'exec EMOdProdUseMatDel '''+BPartNum+''','''+BRevision+'''';
          {sErr:= SQLExecute(sSQL);
          if sErr<>'' then
          begin
            frmMain.meuSystem.JSdMessageDlg(sErr);
            Exit;
          end;}
          qryExec.Close;
          SQL.Clear;
          SQL.Add(sSQL);
          try
            qryExec.ExecSQL;
          except
            on E:Exception do MsgDlgJS(E.Message, mtWarning, [mbok], 0);
          end;
          for i:= 0 to msSelects.TargetItems.Count-1 do
          begin
            sErr:= '';
            sSQL:= 'exec EMOdProdUseMatIns '''+BPartNum+''','''+BRevision+''','
                  +inttostr(i+1)+','''+msSelects.TargetItems[i].Caption+'''';
            {sErr:= SQLExecute(sSQL);
            if sErr<>'' then
            begin
              frmMain.meuSystem.JSdMessageDlg(sErr);
              Exit;
            end;}
            qryExec.Close;
            SQL.Clear;
            SQL.Add(sSQL);
            try
              qryExec.ExecSQL;
            except
              on E:Exception do MsgDlgJS(E.Message, mtWarning, [mbok], 0);
            end;
          end;
        end;
        qryDetail1.Open;
      end;
   end;
end;

procedure TfrmProductDLL.btnRejExamClick(Sender: TObject);
var istatus: integer;
    NowPartNum, NowRevision: String;
begin
  //inherited;
  istatus:= StrToInt(qryBrowse.FieldByName('Status').AsString);
  if istatus=0 then
    abort;
  if ((istatus=2) and (CanbAudit<>1)) then
  begin
    MsgDlgJS('�~���w�f�֡A���i�h�f!!!',mtWarning, [mbOk],0);
    Abort;
  end;
  If MsgDlgJS('�T�w�h�f?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
  Begin
    NowPartNum:=qryBrowse.FieldByName('Partnum').AsString;
    NowRevision:=qryBrowse.FieldByName('Revision').AsString;
    //����h�f�y�{
    with qryProdAudit3 do
    begin
      Parameters.ParamByName('Partnum').Value:=NowPartNum;
      Parameters.ParamByName('Revision').Value:=NowRevision;
      Parameters.ParamByName('Tag').Value:=PowerType;
      Parameters.ParamByName('IOType').Value:=6;
      Parameters.ParamByName('UserId').Value:=sUserId;
      Parameters.ParamByName('Meno').Value:='';
      execsql;
    end;

        //������
    with qryExec do
    begin
      Close;
      SQL.Text := 'select count(*) as Cnt from FMEdIssueMain ' +
                  'where PartNum = :PartNum and Revision = :Revision and Finished in(1,4)';

      Parameters.ParamByName('PartNum').Value := NowPartNum;
      Parameters.ParamByName('Revision').Value := NowRevision;
      // ����d��
      Open;
      if FieldByName('Cnt').AsInteger > 0 then
      begin
        MsgDlgJS('���~�����Ǥw�}�߻s�O��A�ЧY�ɼf��!!!', mtWarning, [mbOK], 0);
      end;

      Close;
    end;

    iNeedAct:=0;
    qryBrowse.Close;
    qryBrowse.Open;
    if qryBrowse.Locate('PartNum;Revision' ,
      VarArrayOf([NowPartNum,NowRevision]) ,[loPartialKey])=false then
      MsgDlgJS('��ƫ��w���ѡA�аh�X�@�~!!',mtWarning,[mbOK],0);
    iNeedAct:=1;
    ScrollAct;
    qryProdHIO.Close;
    qryProdHIO.Open;
  End;
  //�f�ְh�f�����ҥ�
  AuditSetting;
end;

procedure TfrmProductDLL.btnRouteChangeClick(Sender: TObject);
var NowPartNum, NowRevision, NowLayer: String;
    iDoMulSel, i, iMax, iSPId:Integer;
begin
  inherited;
  if qryBrowse.State in [dsEdit ,dsInsert] then qryBrowse.Post;
  AuditCheck;
  NowPartNum:=qryBrowse.FieldByName('PartNum').AsString;
  NowRevision:=qryBrowse.FieldByName('revision').AsString;
  if not Assigned(trvBOM.Selected) then
    NowLayer:= CurrLayer
  else
    NowLayer:=TNodeData(trvBOM.Selected.Data^).Id;
  //2012.04.09 add
  iDoMulSel:=0;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
            +' and ParamId=''RouteChangeNewForm'' and Value=''1''');
    Open;
    if RecordCount>0 then
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('select Value=@@SPId');
      Open;
      iSPId:=FieldByName('Value').AsInteger;
      iDoMulSel:=1;
    end;
  end;
  //04.09 By Param
  if iDoMulSel=1 then
  begin
      Application.createForm(TdlgTmpRouteSet, dlgTmpRouteSet);
      dlgTmpRouteSet.sConnectStr:=sConnectStr;
      dlgTmpRouteSet.prcDoSetConnOCX;
      with dlgTmpRouteSet do
      begin
        //2012.04.12 Timeout Fail
        if iTimeOut>0 then
        begin
          qryTmpRouteDtl.CommandTimeout:=iTimeOut;
          qryMas.CommandTimeout:=iTimeOut;
          qryProcBasic.CommandTimeout:=iTimeOut;
        end;
        btnSearchClick(Sender);
        with qryTmpRouteDtl do
        begin
           Close;
           Parameters.ParamByName('PartNum').Value:= NowPartNum;
           Parameters.ParamByName('Revision').Value:= NowRevision;
           Parameters.ParamByName('LayerId').Value:= NowLayer;
           Open;
        end;
        //BrowseData(frmMain.qryPressMat, false);
        msSelects.Setup(slAll);
        ShowModal;
        if ModalResult = mrOk then
        begin
          iMax:=msSelects.TargetItems.Count-1;
          with qryExec do
          begin
            for i:= 0 to iMax do
            begin
              qryExec.Close;
              SQL.Clear;
              SQL.Add('exec EMOdLayerRouteUpdateNew '''+NowPartNum+''','
                +''''+NowRevision+''','''+NowLayer+''','
                +''''+msSelects.TargetItems[i].Caption+''','+IntToStr(i)+','
                +IntToStr(iMax)+','+IntToStr(iSPId));
              Open;//2012.05.04
            end;
            //�ˬd�O�_���N�X
            qryExec.Close;
            SQL.Clear;
            SQL.Add('exec EMOdRouteChangeCheck '''+NowPartNum+''','
                +''''+NowRevision+''','''+NowLayer+''',0,2');
            Open;
          end;
        end;//mrOK end
      end;
  end
  else
  begin
  //Origin =====================================================================
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
    with qryExec do
    begin
      Close;
      SQL.Clear;
      SQL.Add('exec EMOdProdRouteBefChg '''
        +NowPartNum+''', '''+NowRevision+''', '''+NowLayer+'''');
      Open;
      iSpId:=FieldByName('SpId').AsInteger;
    end;
    GetData(FieldByName('PartNum').AsString,FieldByName('revision').AsString,
            NowLayer);
    edtLayer.Text:= NowLayer;
    Showmodal;
  end;
  //Origin =====================================================================
  end;
  qryTmpRouteId.Close;
  qryTmpRouteId.Open;
  qryDetail10.Close;
  qryDetail10.Open;
end;

procedure TfrmProductDLL.btnRouteNoteClick(Sender: TObject);
var sFileName:String;
begin
  inherited;
  AuditCheck;
  if qryDetail10.RecordCount=0 then abort;
  if OpenDialog1.Execute then
  begin
    sFileName :=OpenDialog1.FileName;
    if not (qryDetail10.State in [dsInsert, dsEdit]) then qryDetail10.Edit;
    qryDetail10.FieldByName('NotesPath').AsString := sFileName;
  end;
end;

procedure TfrmProductDLL.btnSaveHeightClick(Sender: TObject);
var iWidth: Integer;
begin
  inherited;
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('update CURdOCXItemOtherRule set DLLValue='''
        +IntToStr(pnlMaster5.Height)+''' where ItemId='''
        +sItemId+''' and RuleId=''PaperMasterHeight''');
    ExecSql;
    Close;
    SQL.Clear;
    SQL.Add('update CURdOCXItemOtherRule set DLLValue='''
        +IntToStr(dbgLayerPress.Height)+''' where ItemId='''
        +sItemId+''' and RuleId=''PaperDetailHeight''');
    ExecSql;
    //2012.02.03 Add for Route Page size
    Close;
    SQL.Clear;
    SQL.Add('update CURdOCXItemOtherRule set DLLValue='''
        +IntToStr(dbgRoute.Width)+''' where ItemId='''
        +sItemId+''' and RuleId=''RouteLeftWidth''');
    ExecSql;
    Close;
    SQL.Clear;
    SQL.Add('update CURdOCXItemOtherRule set DLLValue='''
        +IntToStr(pnlRouteNote.Height)+''' where ItemId='''
        +sItemId+''' and RuleId=''RouteRightHeight''');
    ExecSql;
    //2012.08.17 add
    Close;
    SQL.Clear;
    SQL.Add('update CURdOCXItemOtherRule set DLLValue='''
        +IntToStr(gridDetail6.Width)+''' where ItemId='''
        +sItemId+''' and RuleId=''UpdateLeftWidth''');
    ExecSql;
  end;
  //2011.09.27 �~�{�Ƶ��w�]��e
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

procedure TfrmProductDLL.btnSaveMapTmpClick(Sender: TObject);
begin
  inherited;
  Application.createForm(TdlgNewNameEdit, dlgNewNameEdit);
  with dlgNewNameEdit do
  begin
    Caption:='�s�]�u�{�ϼ˪��N�X';
    Label2.Visible:=False;
    Label3.Caption:='(�N�X���12�r���A�Ҫ������п�J��U��Ƶ�)';
    Showmodal;
    if modalResult=mrok then
    begin
      if Edit1.Text='' then
      begin
          MsgDlgJS('�˪��N�X����J!!', mtError, [mbOk], 0);
          Exit;
      end;
      with qryExec do
      begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('select TmpId from EMOdTmpProdMap(nolock) where TmpId='
            +''''+Edit1.Text+'''');
        Open;
        if FieldByName('TmpId').AsString<>'' then
        begin
          MsgDlgJS('�˪��N�X����!!', mtError, [mbOk], 0);
          Exit;
        end;
        //if OK
        qryExec.Close;
        SQL.Clear;
        SQL.Add('exec EMOdTmpMapIns '''
          +qryBrowse.FieldByname('PartNum').AsString+''','''
          +qryBrowse.FieldByname('Revision').AsString+''','
          +IntToStr(qryDetail3.FieldByName('SerialNum').AsInteger)+','''
          +Edit1.Text+''',N'''+edtNotes.Text+''','''+sUserId+'''');
        Open;
      if FieldByName('ResultStr').AsString<>'' then
        MsgDlgJS(FieldByName('ResultStr').AsString,mtInformation,[mbOK],0);
      end;
    end;//mrOK end
  end;//dlg end
end;

procedure TfrmProductDLL.btnTierInsClick(Sender: TObject);
var sProcStr:String;
begin
  inherited;
  AuditCheck;
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('exec EMOdProdTierIns '''+dsBrowse.DataSet.FieldByname('PartNum').AsString
        +''','''+dsBrowse.DataSet.FieldByname('Revision').AsString+'''');
    execsql;
  end;
  qryDetail9.Close;
  qryDetail9.Open;

  if sCusId='YX' then
    sProcStr:='EMOdLayerPressMap_YX'
  else
    sProcStr:='EMOdLayerPressMap';
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec '+sProcStr+' '''+qryBrowse.FieldByname('PartNum').asstring
        +''','''+qryBrowse.FieldByname('Revision').asstring
        +''','''+CurrLayer+'''');
    open;
  end;
  if qryBrowse.FieldByName('MapType').AsInteger=0 then
  begin
    XFlowDrawBox2.XRate:= 1.7*XFlowDrawBox2.Width/self.Width;
    XFlowDrawBox2.YRate:= 1.7*XFlowDrawBox2.Width/self.Width;
    XFlowDrawBox2.Content := qryExec.FieldByName('MapData_1').asstring
                            +qryExec.FieldByName('MapData_2').asstring
                            +qryExec.FieldByName('MapData_3').asstring;
  end;
end;

procedure TfrmProductDLL.btnUpdateClick(Sender: TObject);
var iPageIndex, iSourceIndex:Integer;
    sUpdatePartRev:String;
begin
  //101021 ���b
  if qryBrowse.RecordCount<1 then
    Exit;
  AuditCheck;
  EditPartNum:=qryBrowse.FieldByName('PartNum').AsString;
  EditRevision:=qryBrowse.FieldByName('Revision').AsString;
  iPageIndex:=pgeDetail.ActivePageIndex;
  //100716 ���w��@����
  sUpdatePartRev:=sNoOrderByMasSQL+' and t0.PartNum='''+EditPartNum+''''
                  +' and t0.Revision ='''+EditRevision+'''';
  //�]���֩w���|��Filter�POrderBy
  qryBrowse.Close;
  //101001
  sSeahchStr:=qryBrowse.SQL.Text;
  qryBrowse.SQL.Clear;
  qryBrowse.SQL.Add(sUpdatePartRev);
  qryBrowse.Open;
  inherited;
  LockButton(False);
  {100716 �ק����w��@�Ƹ�
  qryBrowse.First;
  if qryBrowse.Locate('PartNum;Revision' ,
      VarArrayOf([EditPartNum,EditRevision]) ,[loPartialKey])=false then
    ShowMessage('��ƫ��w���ѡA�аh�X�@�~!!');}
  pgeDetail.ActivePageIndex:=iPageIndex;
  //2012.09.18 ���ҧǪ�������� qryPage �M�w
  qryPage.Locate('SerialNum',iPageIndex+1,[loCaseInsensitive]);
  iSourceIndex:=qryPage.FieldByName('KindItem').AsInteger;
  if sNowMode='UPDATE' then
    begin
      nav1.DataSource:=
        TDataSource(
          FindComponent('dsDetail'+inttostr(iSourceIndex))
          );
    end;
   nav2.DataSource:=
        TDataSource(
          FindComponent('dsDetail'+inttostr(iSourceIndex))
          );
end;

procedure TfrmProductDLL.btnUpdNoteClick(Sender: TObject);
begin
  inherited;
  if qryDetail6.LockType=ltReadonly then
  begin
    qryDetail6.Close;
    qryDetail6.LockType:=ltOptimistic;
    qryDetail6.Open;
    NavUpdNote.Visible:=True;
  end;
end;

procedure TfrmProductDLL.btnUseNotesClick(Sender: TObject);
var NotesLayer: String;
    iCover: Integer;
    sNowPart, sNowRev, sNowLayer: String;
begin
  inherited;
  AuditCheck;
  sNowPart:=dsBrowse.DataSet.FieldByname('PartNum').AsString;
  sNowRev:= dsBrowse.DataSet.FieldByname('Revision').AsString;
  with qryExec do
  begin
    if not Assigned(trvBOM.Selected) then NotesLayer:='L0~0'
    else
      NotesLayer:=TNodeData(trvBOM.Selected.Data^).Id;

    iCover:=0;
    //�����O�_�w���L���ˬd
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select IsUseNotes=IsNull(IsUseNotes, 0) from EMOdProdLayer(nolock)'
      +' where PartNum='''+qryBrowse.FieldByName('PartNum').AsString +''''
      +' and Revision='''+qryBrowse.FieldByName('Revision').AsString +''''
      +' and LayerId='''+NotesLayer+'''');
    Open;
    if FieldByName('IsUseNotes').AsInteger=1 then
    begin
      if MsgDlgJS('���h�O�w���͹L�A�O�_�л\�H',
               mtConfirmation,[mbYes, mbNo], 0) = mrYes then iCover:=1
      else
        Exit;
    end;

    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdGetFactorMo '''+qryBrowse.FieldByName('PartNum').AsString
          +''','''+qryBrowse.FieldByName('Revision').AsString
          +''','''+NotesLayer+''','+IntToStr(iCover));
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
    qryDetail10.Close;
    qryDetail10.Open;
    qryTmpRouteId.Close;
    qryTmpRouteId.Open;
  end;
end;

procedure TfrmProductDLL.btnViewClick(Sender: TObject);
begin
  inherited;
  if ((pgeBwsDtl.ActivePageIndex=1) and (iNeedAct=1)) then
    ScrollAct;

  //2020.08.05
 { if lblParam.Caption<>'1' then
  begin
    pgeBwsDtl.Height:=pgeBwsDtl.Height;//+5;
    lblParam.Caption:='1';
    SetScroll;
  end;}

  //101001 �o�O�b�ק�ɡA�|���w��@�Ƹ�(�H�K��ƶ]��)�A
  //���ק粒�^�y���e���ɡA�ɦ^�즳�j�M����
  if sSeahchStr<>'' then
  begin
    qryBrowse.Close;
    qryBrowse.SQL.Clear;
    qryBrowse.SQL.Add(sSeahchStr);
    qryBrowse.Open;
    sSeahchStr:='';
    qryBrowse.Locate('PartNum;Revision' ,
          VarArrayOf([EditPartNum,EditRevision]) ,[loPartialKey]);
  end;
  //04.25
  if lblParam.Caption<>'1' then
  begin
    pgeBwsDtl.Height:=pgeBwsDtl.Height+10;
    lblParam.Caption:='1';
  end;
end;

procedure TfrmProductDLL.btnViewMapClick(Sender: TObject);
var sFileName :String;
begin
  inherited;
  if qryBrowse.RecordCount=0 then abort;

  if tblMGNMap.Active=false then abort;
  if tblMGNMap.RecordCount=0 then abort;

  if trim(tblMGNMap.FieldByName('CrossName').Aswidestring)='' then
    begin
      MsgDlgJS('�S�����ɪ�������|���ɦW', mtError, [mbOk], 0);
      Exit;
    end;
  sFileName :=tblMGNMap.FieldByName('CrossName').AswideString;
  if not FileExists(sFileName) then
     begin
       MsgDlgJS('���ɤ��s�b����|�ҫ����ؿ���', mtError, [mbOk], 0);
       exit;
     end;
  ShellExecute(Handle,'OPEN',pchar(sFileName),nil,nil,SW_SHOW);
end;

procedure TfrmProductDLL.btnVoidClick(Sender: TObject);
var sPartNum, sRevision: string;
begin
  //inherited;
  if MsgDlgJS('�T�w�R�������O�� ?' , mtConfirmation,[mbYes, mbNo], 0) = mrYes then
  begin
    if (ViewStatus = vDetail) {and (dsBrowse.DataSet.Active)} then
    begin
       MsgDlgJS('�Ф������s���e���A�@�o!!',mtInformation,[mbOk],0);
       {sPartNum:= qryBrowse.FieldByName('PartNum').AsString;
       sRevision:= qryBrowse.FieldByName('Revision').AsString;
       with qryDelete do
       begin
         Parameters.ParamByName('PartNum').Value := sPartNum;
         Parameters.ParamByName('Revision').Value := sRevision;
         Parameters.ParamByName('UserId').Value:=sUserId;
         try
           Open;
         except
           on E:Exception do
           begin
             MsgDlgJS(E.Message, mtWarning, [mbok], 0);
             Exit;
           end;
         end;
       End;
       qryBrowse.Refresh; }
    end
    else
    begin
       sPartNum:= qryBrowse.FieldByName('PartNum').AsString;
       sRevision:= qryBrowse.FieldByName('Revision').AsString;
       with qryDelete do
       begin
         qryDelete.Close;
         Parameters.ParamByName('PartNum').Value := sPartNum;
         Parameters.ParamByName('Revision').Value := sRevision;
         Parameters.ParamByName('UserId').Value:= sUserId;
         Open;
         qryDelete.Close;
       End;
       qryBrowse.Close;
       qryBrowse.Open;
    end;
  end;
  //�f�ְh�f�����ҥ�
  AuditSetting;
end;

procedure TfrmProductDLL.cboWhatRevEnter(Sender: TObject);
begin
  inherited;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Revision from EMOdProdInfo(nolock) where PartNum='''
          +qryBrowse.FieldByName('PartNum').AsString+''' and Revision<>'''
          +qryBrowse.FieldByName('Revision').AsString+''' ');
    Open;
    cboWhatRev.LookupTable:=qryExec;
    cboWhatRev.LookupField:='Revision';
  end;
end;

procedure TfrmProductDLL.cboWhatRevExit(Sender: TObject);
begin
  inherited;
  qryExec.Close;
  cboWhatRev.LookupField:='';
end;

procedure TfrmProductDLL.pgeDetailChange(Sender: TObject);
var iPage:Integer;
begin
  inherited;
  //2012.09.18 ���ҧǪ�������� qryPage �M�w,�~�ӭ���
  qryPage.Locate('SerialNum',pgeDetail.ActivePageIndex+1,[loCaseInsensitive]);
  iPage:=qryPage.FieldByName('KindItem').AsInteger;
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
  //2012.09.18 end

  if pgeDetail.ActivePage = tbshtDetail1 then   //���Ƹ��
  begin
    //2020.01.03
    if sCusId<>'ECO' then
      trvBOM.SetFocus;
    {tblLayerPress.Close;
    tblLayerPress.Open;}
  end;
  if pgeDetail.ActivePage = tbshtDetail3 then   //���O��
  begin
    MapOpen;
  end;
  if pgeDetail.ActivePage = tbshtDetail4 then   //�֪����
  begin
    qryPartMatri.Close;
    qryPartMatri.ParaMeters.ParamByName('PartNum').Value:=
        qryBrowse.FieldByname('PartNum').asstring;
    qryPartMatri.ParaMeters.ParamByName('Revision').Value:=
        qryBrowse.FieldByname('Revision').asstring;
    qryPartMatri.Open;
    if pnlPartMergePrint.Visible=True then
    begin
      if not tblPartMergePrint.Active then
      begin
        tblPartMergePrint.Close;
        tblPartMergePrint.Open;
      end;
    end;
  end;
  if pgeDetail.ActivePage=tbshtDetail6 then
  begin
    with tblModify do
    begin
      Close;
      Parameters.ParamByName('PartNum').Value :=
          qryBrowse.FieldByName('PartNum').AsString;
      Parameters.ParamByName('Revision').Value :=
          qryBrowse.FieldByName('Revision').AsString;
      Open;
    end;
  end;
  //100929
  if pgeDetail.ActivePage = tbshtDetail7 then   //ECN
  begin
    nav2.DataSource:=dsDetail7B;
  end;
  if pgeDetail.ActivePage = tbshtDetail9 then   //�h�O�s�@�覡
  begin
    PressMapOpen;
  end;
  if pgeDetail.ActivePage = tbshtDetail10 then   //���h
  begin
    OpenRoute;
    if not qryDetail10.Active then
    begin
      qryDetail10.Close;
      qryDetail10.Open;
      qryDetail11.Close;
      qryDetail11.Open;
    end;
  end;
  if pgeDetail.ActivePage = tbshtDetail11 then   //���~���c�u�{��
  begin
    if not tblMGNMap.Active then
    begin
      tblMGNMap.Close;
      tblMGNMap.Open;
    end;
  end;
end;

procedure TfrmProductDLL.qryBrowseAfterScroll(DataSet: TDataSet);
begin
  inherited;
  if ((iNeedAct=1) and (pgeBwsDtl.ActivePage=tabDetail)) then
    ScrollAct;
end;

procedure TfrmProductDLL.ScrollAct;
begin
  trvBOM.Setup;
  trvBOM.FullExpand;
  {if qryBrowse.RecordCount>0 then
  begin
    qryProdHIO.Close;
    qryProdHIO.Parameters.ParamByName('PartNum').Value:=
        qryBrowse.FieldByName('PartNum').AsString;
    qryProdHIO.Parameters.ParamByName('Revision').Value:=
        qryBrowse.FieldByName('Revision').AsString;
    qryProdHIO.Open;
  end;}
  if pgeDetail.ActivePage=tbshtDetail3 then
  begin
    MapOpen;
  end;
  if pgeDetail.ActivePage=tbshtDetail9 then
  begin
    PressMapOpen;
  end;
  if pgeDetail.ActivePage=tbshtDetail6 then
  begin
    with tblModify do
    begin
      Close;
      Parameters.ParamByName('PartNum').Value :=
          qryBrowse.FieldByName('PartNum').AsString;
      Parameters.ParamByName('Revision').Value :=
          qryBrowse.FieldByName('Revision').AsString;
      Open;
    end;
  end;
  {if pgeDetail.ActivePage=tbshtDetail1 then
  begin
    tblLayerPress.Close;
    tblLayerPress.Open;
    //20100422
    if Assigned(trvBOM.Selected) then
    begin
      tblLayerPress.Close;
      tblLayerPress.Parameters.ParamByName('LayerId').Value:=
          TNodeData(trvBOM.Selected.Data^).Id;
      tblLayerPress.Open;
    end;
  end;}
  if pgeDetail.ActivePage=tbshtDetail10 then
  begin
    OpenRoute;
  end;
  {if pgeDetail.ActivePage=tbshtDetail11 then
  begin
    tblMGNMap.Close;
    tblMGNMap.Open;
  end;}
end;

procedure TfrmProductDLL.pgeFormTypeChange(Sender: TObject);
begin
  inherited;
  if pgeFormType.ActivePage=tstMain then
  begin
    pgeDetail.Align:=alRight;
    pgeDetail.Visible:=False;
    pgeMaster.Visible:=True;
    pgeMaster.Align:=alClient;
  end
  else
  begin
    pgeMaster.Align:=alRight;
    pgeMaster.Visible:=False;
    pgeDetail.Visible:=True;
    pgeDetail.Align:=alClient;
    //100607 add
    pnlMills.Visible:=False;
    pnlWriting.Visible:=False;
  end;
end;

procedure TfrmProductDLL.pgeMasterChange(Sender: TObject);
begin
  inherited;
  with pnlMills do
  begin
    if lblWhere1.Caption<>'0' then
    begin
      if pgeMaster.ActivePageIndex+1=StrToInt(lblWhere1.Caption) then
        Visible:=True
      else
        Visible:=False;
    end;
  end;
  with pnlWriting do
  begin
    if lblWhere2.Caption<>'0' then
    begin
      if pgeMaster.ActivePageIndex+1=StrToInt(lblWhere2.Caption) then
        Visible:=True
      else
        Visible:=False;
    end;
  end;
end;

procedure TfrmProductDLL.pnlMapToolsDblClick(Sender: TObject);
begin
  inherited; //��������ɹw�]����ܡA�I�|�U�~�X�{
  if btnAllOutput.Hint='' then
    btnAllOutput.Hint:='To JPG'
  else
  begin
    pnlMapTool6.Visible:=True;
    //���o�ʤ��M�Ĥ@�ӨӤ����ܩ���
    pnlXFlow.Color:=clWindow;
    XFlowDrawBox1.Color:=clWindow;
    pnlXFlow2.Color:=clWindow;
    XFlowDrawBox2.Color:=clWindow;
  end;
end;

procedure TfrmProductDLL.pnlTempBasDLLBottomDblClick(Sender: TObject);
begin
  inherited;
  ShowMessage(qryBrowse.FieldByName('PartNum').AsString
            +qryBrowse.FieldByName('Revision').AsString);
  ShowMessage(EditPartNum + EditRevision);
end;

procedure TfrmProductDLL.qryBrowseAfterClose(DataSet: TDataSet);
begin
  inherited;
  //100607 add
  tblMills.Close;
  tblWriting.Close;
end;

procedure TfrmProductDLL.qryBrowseAfterOpen(DataSet: TDataSet);
begin
  inherited;
  //100607 add
  tblMills.Open;
  tblWriting.Open;
end;

procedure TfrmProductDLL.qryBrowseAfterPost(DataSet: TDataSet);
begin
  inherited;
  //100607 add
  if tblMills.State in [dsEdit,dsInsert] then tblMills.Post;
  if tblWriting.State in [dsEdit,dsInsert] then tblWriting.Post;
  //2012.03.08 add
  UpdateDesigner;

  btnAdd.Enabled:=btnExam.Enabled;  //2022.11.30
end;

procedure TfrmProductDLL.qryDetail6AfterInsert(DataSet: TDataSet);
var iNewSerial: Integer;
begin
  inherited;
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select Serial=Max(SerialNum) from EMOdNeedModifyPart(nolock)');
    Open;
    iNewSerial:=FieldByName('Serial').AsInteger;
  end;
  qryDetail6.FieldByName('SerialNum').Value:=iNewSerial+1;
end;

procedure TfrmProductDLL.qryDetail7AfterClose(DataSet: TDataSet);
begin
  inherited;
  qryDetail7B.Close;
end;

procedure TfrmProductDLL.qryDetail7AfterOpen(DataSet: TDataSet);
begin
  inherited;
  if qryBrowse.Active then
    qryDetail7B.Open;
end;

procedure TfrmProductDLL.qryDetail7BAfterInsert(DataSet: TDataSet);
begin
  inherited;
  if DataSet.FindField('SerialNum')<>nil then
  begin
    DataSet.fieldbyname('SerialNum').Asinteger:= DataSet.Tag;
  end;
end;

procedure TfrmProductDLL.qryDetail7BBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  with dsBrowse.DataSet do
     if state in [dsEdit, dsInsert] then Post;
  //add
  if DataSet.FindField('SerialNum')<>nil then
    DataSet.Tag:=GetMaxSerialNumDLL(DataSet, 'SerialNum')+1;
end;

procedure TfrmProductDLL.qryDetail8AfterClose(DataSet: TDataSet);
begin
  inherited;
  qryDetail8B.Close;
end;

procedure TfrmProductDLL.qryDetail8AfterOpen(DataSet: TDataSet);
begin
  inherited;
  if qryBrowse.Active then
    qryDetail8B.Open;
end;

procedure TfrmProductDLL.qryDetail10AfterPost(DataSet: TDataSet);
begin
  inherited;
  //2012.03.08 add
  UpdateDesigner;
  //2012.03.22 add
  qryDetail10.Refresh;
end;

procedure TfrmProductDLL.qryDetail10BeforeDelete(DataSet: TDataSet);
begin
  inherited;
  //2012.03.21 ��~�{�@����������]
  //if sCusId='MUT' then
  //begin
    with qryExec do
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('exec EMOdLayerRouteDelRecord '''+sUserId+''','''+
          qryDetail10.FieldByName('PartNum').AsString+''','''+
          qryDetail10.FieldByName('Revision').AsString+''','''+
          qryDetail10.FieldByName('LayerId').AsString+''','+
          qryDetail10.FieldByName('SerialNum').AsString+','''+
          qryDetail10.FieldByName('ProcCode').AsString+'''');
      ExecSql;
    end;
  //end;
end;

procedure TfrmProductDLL.qryDetail1AfterEdit(DataSet: TDataSet);
var dsName:String;
begin
  inherited;
  //2012.04.20 �b�s�����A�U�A���I�U�����A�i�H��� detail �����e
  if not (qryBrowse.LockType=ltOptimistic) then
  begin
    TJSdTable(DataSet).CancelUpdates;
    abort;
  end;

  //2011.05.13 add
  dsName:= TJSdTable(DataSet).Name;
  dsName:='ds'+
    stringreplace(
      stringreplace(dsName,'qry','',[rfIgnoreCase])
    ,'tbl','',[rfIgnoreCase]);
  if nav2.DataSource<>TDataSource(FindComponent(dsName)) then
    nav2.DataSource:=TDataSource(FindComponent(dsName));
end;

procedure TfrmProductDLL.qryDetail1AfterPost(DataSet: TDataSet);
begin
  inherited;
  //2012.03.08 add
  UpdateDesigner;
end;

procedure TfrmProductDLL.qryDetail1BeforeDelete(DataSet: TDataSet);
//var sSQL:string;
begin
  inherited;
  //100630 �����έק�A�S���� Layer �ɤ@�˥X�{���D
  {if ((TJSdTable(DataSet).FindField('Item')=nil)
        or
      (TJSdTable(DataSet).FindField('PaperNum')=nil)) then
  begin
    sSQL:='exec EMOdDLLdoDelete '+''''+TJSdTable(DataSet).TableName+''''+','+
    ''''+qryBrowse.FieldByName('PartNum').AsString+''''+','+
    ''''+qryBrowse.FieldByName('Revision').AsString+''''+','+
    ''''''+','+
    DataSet.FieldByName('SerialNum').AsString;
    unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);
    DataSet.Close;
    DataSet.Open;
    abort;
  end;}
end;

procedure TfrmProductDLL.qryDetail3AfterOpen(DataSet: TDataSet);
begin
  inherited;
  if qryBrowse.FieldByName('MapType').AsInteger=0 then
  begin
    qryMap.Close;
    qryMap.Open;
  end
  else
    ShowRealMap(qryDetail3.FieldByName('MapKindNo').AsInteger);
end;

procedure TfrmProductDLL.qryDetail3AfterScroll(DataSet: TDataSet);
begin
  inherited;
  if qryBrowse.FieldByName('MapType').AsInteger=0 then
    ShowProdMap
  else
    ShowRealMap(qryDetail3.FieldByName('MapKindNo').AsInteger);
end;

procedure TfrmProductDLL.qryDetail9BeforeInsert(DataSet: TDataSet);
begin
  inherited;
  if unit_DLL.funCheckSameUser(
      CanbLockUserEdit,
      qryBrowse.FieldByName('UserId').AsString,
      sUserId
      )=false then abort;
  { //����,�n�O�d  if unit_DLL.funTableEventByParams(
    CanbLockUserEdit,//CanbLockUserEdit:integer;
    sUserId,//sUserId:string;
    qryBrowse.FieldByName('UserId').AsString,//sPaperUserId:string;
    DataSet,//:TDataSet;
    'BeforeInsert'//sEventKind:string
    )=false then abort;}
  with dsBrowse.DataSet do
     if state in [dsEdit, dsInsert] then Post;

  //DataSet.Tag:=GetMaxSerialNumDLL(DataSet, 'Item')+1;
end;

{procedure TfrmProductDLL.SetScroll;
begin
  //2020.08.05 ScrollBar
  //�q�D�ɩI�s�A�Ĥ@�� iScrollHeight �|�O�s
  showmessage(inttostr(pnlMaster1.Height));
  showmessage(inttostr(iScrollHeight));
  if ((pnlMaster1.Height>0) and (iScrollHeight>0)) then
  begin
    if iScrollHeight>pnlMaster1.Height then
    begin
        sclPnlMaster.Max:= (iScrollHeight-pnlMaster1.Height) div 2;
        //sclPnlMaster2.Max:= (iScrollHeight-pnlMaster1.Height) div 2;
        ShowMessage(IntToStr(sclPnlMaster.Max));
        iScrollHeight:=0;
    end
    else
    begin
        sclPnlMaster.Visible:=False;
        //sclPnlMaster2.Visible:=False;
    end;
  end;
end;

procedure TfrmProductDLL.sclPnlMasterScroll(Sender: TObject;
  ScrollCode: TScrollCode; var ScrollPos: Integer);
var iChangeLen: Integer;
begin
  inherited;
  //2012.04.26
  //ScrollBar ���@���|�o�ͨ⦸ OnScroll�A�Ĥ@���O���e��m�A�ĤG���O�����m
  if ScrollPos<>iScrollHeight then
  begin
    iChangeLen:= iScrollHeight - ScrollPos;
    pnlMaster1.ScrollBy(0, iChangeLen * 2);
    iScrollHeight := ScrollPos; //�ɥΨӰO��l��m
  end;
end; }

procedure TfrmProductDLL.tblLayerPressBeforeEdit(DataSet: TDataSet);
begin
  inherited;
  AuditCheck;
  //2011.04.29
  sPressMatCode:=tblLayerPress.FieldByName('matcode').AsString;
end;

procedure TfrmProductDLL.tblLayerPressBeforePost(DataSet: TDataSet);
begin
  inherited;
  //2011.04.29
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdEditPressBack '''+
            qryBrowse.FieldByName('PartNum').AsString+''', '''+
            qryBrowse.FieldByName('Revision').AsString+''', '''+
            tblLayerPress.FieldByName('LayerId').AsString+''', '''+
            tblLayerPress.FieldByName('MatCode').AsString+''', '''+
            sPressMatCode+'''');
    Open;
    if ((FieldByName('OutPutStr').AsString<>'') and (iPressWarn=0)) then
    begin
      MsgDlgJS(FieldByName('OutPutStr').AsString,mtWarning,[mbOK],0);
      iPressWarn:=1;
    end;
  end;
end;

procedure TfrmProductDLL.tblLayerPressAfterPost(DataSet: TDataSet);
begin
  inherited;
  //0502
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdLayerPressModifyInner '''+
            qryBrowse.FieldByName('PartNum').AsString+''', '''+
            qryBrowse.FieldByName('Revision').AsString+''', '''+
            tblLayerPress.FieldByName('LayerId').AsString+''', '+
            tblLayerPress.FieldByName('SerialNum').AsString+', '''+
            tblLayerPress.FieldByName('MatCode').AsString+''', '''+
            sPressMatCode+''' ');
    Open;
    if tblLayerPress.FieldByName('MatCode').AsString<>sPressMatCode then
    begin
      tblLayerPress.Close;
      tblLayerPress.Open;
    end;
  end;
  //2012.03.08 add
  UpdateDesigner;
end;

procedure TfrmProductDLL.tblMillsAfterInsert(DataSet: TDataSet);
var iNewSerial: Integer;
begin
  inherited;
  //070607 add
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select Serial=Max(SerialNum) from EMOdProdMills(nolock) '
          +'where PartNum='''+qryBrowse.FieldByName('PartNum').AsString+''' '
          +'and Revision='''+qryBrowse.FieldByName('Revision').AsString+''' ');
    Open;
    iNewSerial:=FieldByName('Serial').AsInteger;
  end;
  tblMills.FieldByName('SerialNum').Value:=iNewSerial+1;
end;

procedure TfrmProductDLL.tblWritingAfterInsert(DataSet: TDataSet);
var iNewSerial: Integer;
begin
  inherited;
  //070607 add
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select Serial=Max(SerialNum) from EMOdProdWriting(nolock) '
          +'where PartNum='''+qryBrowse.FieldByName('PartNum').AsString+''' '
          +'and Revision='''+qryBrowse.FieldByName('Revision').AsString+''' ');
    Open;
    iNewSerial:=FieldByName('Serial').AsInteger;
  end;
  tblWriting.FieldByName('SerialNum').Value:=iNewSerial+1;
end;

procedure TfrmProductDLL.trvBOMChange(Sender: TObject; Node: TTreeNode);
begin
  inherited;
  if pgeDetail.ActivePage = tbshtDetail10 then
    OpenRoute;
  if pgeDetail.ActivePage = tbshtDetail1 then
  begin
    //20100422
    if Assigned(trvBOM.Selected) then
    begin
      tblLayerPress.Close;
      tblLayerPress.Parameters.ParamByName('LayerId').Value:=
          TNodeData(trvBOM.Selected.Data^).Id;
      tblLayerPress.Open;
    end;
  end;
  //2023.03.14 �s�@�Ϥ]�n�̷Ӽh�O
  if pgeDetail.ActivePage = tbshtDetail9 then
  begin
    //20100422
    if Assigned(trvBOM.Selected) then
    begin
      qryDetail9.Close;
      qryDetail9.Parameters.ParamByName('LayerId').Value:=
          TNodeData(trvBOM.Selected.Data^).Id;
      qryDetail9.Open;
    end;
  end;

  //���@�����|�ܰ� CurrLayer:=TNodeData(trvBOM.Selected.Data^).Id;
  if pgeDetail.ActivePage = tbshtDetail9 then
  begin
    PressMapOpen;
  end;
end;

procedure TfrmProductDLL.trvBOMDblClick(Sender: TObject);
var sNewItemName,sNewSystemId,sNewItemId,sOCXTemplate,sNewPaperId:string;
    sNewClassName:string;
    sSQL:string;
    NowPartNum, NowRevision, NowLayerId, NowWrite, NowClick: String;
//var sSQLComd:string;
//    bRead: Boolean;
//    ProdStatus: Integer;
begin
  inherited;
  if not Assigned(trvBOM.Selected) then exit;

  NowPartNum:=qryBrowse.FieldByName('PartNum').AsString;
  NowRevision:=qryBrowse.FieldByName('Revision').AsString;
  NowLayerId:=TNodeData(trvBOM.Selected.Data^).Id;
  if ((btnUpdate.Enabled=True) or (btnKeepStatus.Enabled=True)) then
    NowWrite:='1'
  else
    NowWrite:='0';
  if sNowMode='UPDATE' then
    NowClick:='1'
  else
    NowClick:='0';

  sNewPaperId:='EMOdProdLayer'; //��ʫ��w
  sNewClassName:= 'EMOdProdLayer.dll';
  sSQL:='exec CURdOCXItemIdByFromType ''EMOdProdLayer'',''''';
  with qryExec do
  begin
      qryExec.close;
      sql.Clear;
      sql.Add(sSQL);
      open;
      sNewItemId :=qryExec.FieldByName('ItemId').AsString;
      sNewItemName:=qryExec.FieldByName('ItemName').AsString;
      sNewSystemId:=qryExec.FieldByName('SystemId').AsString;
      sOCXTemplate:=qryExec.FieldByName('OCXTemplete').AsString;
  end;

  Application.CreateForm(TfrmLinkShowDLL,frmLinkShowDLL);
  frmLinkShowDLL.Caption:=sNewItemName;

  unit_DLL.funCallDLL(
    qryExec,//qryExec:TADOQuery;
    nil,//fStartForm:TForm;
    2,//iCallType:integer;//0 from MainForm, 1 from DLL, 2 from Flow , 3 PaperTrace
    false,//bShowModal:boolean;
    sNewItemId,//sItemId,
    sNewItemName,//sItemName,
    sNewClassName,//sClassName,
    sNewSystemId,//sSystemId,
    sServerName,//sServerName,
    sDBName,//sDBName,
    sUserId,//sUserId,
    sBUId,//sBUId,
    sUseId,//sUseId,
    sNewPaperId,//sPaperId,//sPaperId,
    NowPartNum+','+NowRevision+','+NowLayerId+','+NowWrite+','+NowClick,//sPaperNum,//sPaperNum,
    sGlobalId,//sGlobalId  :string;
    frmLinkShowDLL.pnlMain,//tOtherParent:TWinControl;
    '',//sServerPath:string;
    '',//sLocalPath:string;
    sLoginSvr,//sLoginSvr:string;
    sLoginDB,//sLoginDB:string;
    '',//sOCXTemplate,
    0, //iDtlItem
    '',//sTranGlobalId
    sTempBasJSISpw
    );

  frmLinkShowDLL.hide;
  if sCusId='MUT' then
  begin
    //2012.09.21
    if uppercase(sSubCusId)='C' then
      frmLinkShowDLL.ShowModal
    else
      frmLinkShowDLL.Show;
    //2011.09.30 �i�঳�M�I
  end
  else
    frmLinkShowDLL.ShowModal;
end;

procedure TfrmProductDLL.XFlowDrawBox1DblClick(Sender: TObject);
var iMapKindNo :integer;
    sSQL, sConText: String;
begin
  inherited;
  if sNowMode='BROWSE' then
    Exit;
  iMapKindNo := qryDetail3.FieldByName('MapKindNo').AsInteger;

  Application.CreateForm(TfrmMapEdit, frmMapEdit);
  with frmMapEdit do
  begin
     if qryDetail3.FieldByName('SerialNum').AsInteger=9 then
     begin
       ClientWidth:=733;
       ClientHeight:=483;
     end;
     TextToControls(frmMapEdit, XFlowDrawBox1.Content);
     Showmodal;
     if modalresult=mrok then
     begin
       sConText:= StringReplace(ControlsToText(frmMapEdit),'''','''''',
                    [rfReplaceAll, rfIgnoreCase]);
       qryDetail3.close;
       //showmessage(sConText);
       sSQL:= 'update EMOdProdMap '
         +'set MapData=N'+ ''''+sConText+''''
         +' where Partnum ='+ ''''+qryBrowse.FieldByname('PartNum').AsString+''''
         +' and Revision ='+ ''''+qryBrowse.FieldByname('Revision').AsString+''''
         +' and SerialNum ='+ inttostr(iMapKindNo);
       with qryExec do
       begin
         qryExec.Close;
         SQL.Clear;
         SQL.Add(sSQL);
         try
           ExecSql;
           qryExec.Close;
         except
           on E: Exception do
           begin
             MsgDlgJS(E.Message, mtInformation, [mbOk], 0);
             Exit;
           end;
         end;
       end;
       //showmessage(sSQL);
       qryDetail3.Open;
     end;
  end;
end;

procedure TfrmProductDLL.ProdAudit(Sender: TObject);
var Part, Rev, LsmColorNow, PcbLayerNow, LsmColorOld, PcbLayerOld{, sSQLComd}:String; //2025.08.05�P�_�e��o���C��O�_�ܧ�
    {istatus ,}IsAudit, ChkSPId, iPass, iANS, iSe: integer;
Begin
  Part := dsBrowse.DataSet.FieldByname('PartNum').AsString;
  Rev := dsBrowse.DataSet.FieldByname('Revision').AsString;
  LsmColorNow := dsBrowse.DataSet.FieldByName('LsmColor').AsString;
  PcbLayerNow := dsBrowse.DataSet.FieldByName('PcbLayer').AsString;
  PowerType1:=StrToInt(qryBrowse.FieldByName('Status').AsString);
  //istatus := StrToInt(qryBrowse.FieldByName('Status').AsString) + 1;
  IsAudit := 0;

  //�o���ˬd�o���O�_���ܧ�
  with qryExec do
    begin
      Close;
      SQL.Text :=
        'SELECT TOP 1 LsmColor, PcbLayer ' +
        'FROM EMOdProdInfo ' +
        'WHERE PartNum = :PartNum ' +
        '  AND Revision <> :Revision ' +
        '  AND LsmColor IS NOT NULL ' +
        '  AND PcbLayer IS NOT NULL ' +
        'ORDER BY Revision DESC';
      Parameters.ParamByName('PartNum').Value := Part;
      Parameters.ParamByName('Revision').Value := Rev;
      Open;

      if (qryBrowse.FieldByName('Status').AsInteger = 0) and (not Eof) then
      begin
        LsmColorOld := FieldByName('LsmColor').AsString;
        PcbLayerOld := FieldByName('PcbLayer').AsString;

        if (LsmColorNow <> LsmColorOld) or (PcbLayerNow <> PcbLayerOld) then
        begin
          if MsgDlgJS(
            '���~���o���C��P�e�@�����P�A�O�_���n�e�f�H',
            mtConfirmation, [mbYes, mbNo], 0) <> mrYes then
          begin
            Exit;  // �����e�f
          end;
        end;
      end;
    end;


  //�u�X�f�֪`�N����
    Application.createForm(TdlgNotValueShow, dlgNotValueShow);
    dlgNotValueShow.sConnectStr:=sConnectStr;
    dlgNotValueShow.prcDoSetConnOCX;
    With dlgNotValueShow do
    Begin
      //2012.04.12 Timeout Fail
      if iTimeOut>0 then
      begin
        qryNotValueShow.CommandTimeout:=iTimeOut;
        qryCheckLayerRoute.CommandTimeout:=iTimeOut;
      end;
    //Highlight�ƶ�
      With qryNotValueShow do
      Begin
        Close;
        Parameters.ParamByName('Partnum').Value:=Part;
        Parameters.ParamByName('Revision').Value:=Rev;
        Open;
      End;
{     //�O�_������   2006007 EDIT
      With qryCheckLayerRoute do
      Begin
        Close;
        ParamByName('Partnum').AsString:=Part;
        ParamByName('Revision').AsString:=Rev;
        Open;
        If FieldByName('S').AsString = '1' then
          Panel1.Visible := True;
      End;     }
      if (qryNotValueShow.FieldByName('IsError').AsInteger >=1)
            //2010.03.10 No Open Or (qryCheckLayerRoute.FieldByName('S').AsString = '1') then
      then
      begin //�P�_Show
        IsAudit := 0;
        dlgNotValueShow.Showmodal;
        if ModalResult = mrOK then
        begin
          if qryNotValueShow.FieldByName('IsError').AsInteger <= 0 then
            IsAudit := 1;
        end;
        //�f��
        With qryExec do
        Begin
          Close;
          SQL.Clear;
          SQL.Add('Delete EMOdTmp Where PartNum = '''+Part+''' '
              +'And Revision = '''+Rev+'''');
          ExecSql;
        End;
        //�f�ֵ���
      end  //�P�_Show
      else
      begin
        dlgNotValueShow.Free;
        IsAudit := 1;
      end;
      if IsAudit = 1 then
      begin
        //2012.04.10
        //======================================================================
        with qryExec do
        begin
          qryExec.Close;
          SQL.Clear;
          SQL.Add('exec EMOdProductAuditChkValue '''+Part+''','''+Rev+'''');
          Open;
          ChkSPId:=FieldByName('SPId').AsInteger;
          qryExec.Close;
          SQL.Clear;
          SQL.Add('select t1.*, t2.DoAskStr from EMOdFactorAsk t1(nolock), '
              +'EMOdFactor t2(nolock) where t1.SPId='+IntToStr(ChkSPId)+' '
              +'and t1.FactorId=t2.FactorId order by t1.SerialNum');
          Open;
          if RecordCount>0 then
          BEGIN
            iSe:=1;
            iPass:=1;
            qryExec2.Close;
            qryExec2.SQL.Clear;
            qryExec2.SQL.Add('select Se=Max(SerialNum)+1 from EMOdFactorHis(nolock) '
              +'where PartNum='''+Part+''' and Revision='''+Rev+'''');
            qryExec2.Open;
            if qryExec2.FieldByName('Se').AsInteger>0 then
              iSe:=qryExec2.FieldByName('Se').AsInteger;

            first;
            while not eof do
            begin
              //2012.06.04 add
              if FieldByName('AskPassType').AsInteger=0 then
              BEGIN
                  if MsgDlgJS(FieldByName('LayerStr').AsString+
                        FieldByName('DoAskStr').AsString,
                        mtConfirmation, [mbYes, mbNo], 0)=mrYes then
                  begin
                    iANS:=1;
                  end
                  else
                  begin
                    iANS:=2;
                    iPass:=0;
                  end;
              END
              ELSE if FieldByName('AskPassType').AsInteger=1 then
              BEGIN
                  if MsgDlgJS(FieldByName('LayerStr').AsString+
                        FieldByName('DoAskStr').AsString,
                        mtConfirmation, [mbYes, mbNo], 0)=mrYes then
                  begin
                    iPass:=0;
                    iANS:=1;
                  end
                  else
                  begin
                    iANS:=2;
                  end;
              END
              //2012.07.04 add
              else
              begin
                MsgDlgJS(FieldByName('LayerStr').AsString+
                        FieldByName('DoAskStr').AsString,
                        mtConfirmation, [mbOK], 0);
                iANS:=1;
              end;

              qryExec2.Close;
              qryExec2.SQL.Clear;
              qryExec2.SQL.Add('exec EMOdFactorHisIns '''+Part+''','''+Rev
                +''','+IntToStr(FieldByName('SerialNum').AsInteger)+','
                +IntToStr(ChkSPId)+','+IntToStr(iSe)+','''+sUserId+''','
                +IntToStr(iANS));
              qryExec2.ExecSQL;

              if iPass=0 then
              begin
                ClearAskTmp(ChkSPId);
                Exit;
              end;

              next;
            end;
            ClearAskTmp(ChkSPId);
          END;
          qryExec.Close;
        end;
        //======================================================================
        With qryProdAudit3 do
        Begin
          Active := False;
          Parameters.ParamByName('Partnum').Value:=Part;
          Parameters.ParamByName('Revision').Value:=Rev;
          Parameters.ParamByName('Tag').Value:=PowerType1;
          Parameters.ParamByName('IOType').Value:=1;
          Parameters.ParamByName('UserId').Value:=sUserId;
          Parameters.ParamByName('Meno').Value:='';
          Execsql;      //�令Open�|Error
          //Rev := qryProdAudit3.FieldByName('NewRevision').AsString;
          //�p���ǥ[�@�X,�f�֫���ܷs���ǵe��
          {sSQLComd:=sSQLComd+' And t1.PartNum = ' + '''' + Part + '''';
          sSQLComd:=sSQLComd+' And t1.Revision = ' + '''' + Rev + '''';
          With qryBrowse do
          Begin
            Close;
            Parameters.ParamByName('Cond').Value := sSQLComd;
            Open;
          End; }
        End;
      end;
      iNeedAct:=0;
      qryBrowse.Close;
      qryBrowse.Open;
      if qryBrowse.Locate('PartNum;Revision' ,
           VarArrayOf([Part,Rev]) ,[loPartialKey])=false then
        MsgDlgJS('��ƫ��w���ѡA�аh�X�@�~!!',mtWarning,[mbOK],0);
      //��Open �n�w�� qryBrowse.Open;
      iNeedAct:=1;
      ScrollAct;
      qryProdHIO.Close;
      qryProdHIO.Open;
      // �e�f������A���sŪ�����A
      qryBrowse.Close;
      qryBrowse.Open;
      //2025.09.17  �e�f�Q�פU��A�^����e�����
      qryBrowse.Locate('PartNum;Revision',
      VarArrayOf([Trim(part), Trim(rev)]), [loCaseInsensitive]);

    //�e�f�q�]�p���ܦ��O�]�p���� �p�w���}���s�O��N���O�_�ߧY�f��
    with qryExec do
    begin
      Close;

      SQL.Text := 'select count(*) as Cnt from FMEdIssueMain ' +
                  'where PartNum = :PartNum and Revision = :Revision and Finished in(1,4)';

      Parameters.ParamByName('PartNum').Value := Part;
      Parameters.ParamByName('Revision').Value := Rev;

      // ����d��
      Open;
      if FieldByName('Cnt').AsInteger > 0 then
      if qryBrowse.Locate('PartNum;Revision', VarArrayOf([Part, Rev]), [loPartialKey]) then
      begin
        if qryBrowse.FieldByName('Status').AsInteger = 1 then
        begin
          if MsgDlgJS('���~�����Ǥw���}�ߪ��s�O��A�O�_�ߧY�f��?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
          begin
            ProdAudit(Self);  // �I�s�ۤv����f���޿�
            Exit;             // �����쥻�e�f�y�{
          end;
        end;
      end;
      Close;
    end;
  End;
End;

procedure TfrmProductDLL.AuditCheck;
begin
  inherited;
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec EMOdAuditUserCheck '''+dsBrowse.DataSet.FieldByname('PartNum').AsString
            +''','''+dsBrowse.DataSet.FieldByname('Revision').AsString
            +''','''+sUserId+'''');
    Open;
    if FieldByName('ReturnStr').AsString<>'' then
    begin
      MsgDlgJS(FieldByName('ReturnStr').AsString,mtWarning, [mbOk],0);
      abort;
    end
  end;
end;

procedure TfrmProductDLL.AuditCheckModify;
var sSP:String;
begin
  inherited;
  if sCusId='VIC' then
    sSP:='EMOdAuditUserCheck_Modify'
  else
    sSP:='EMOdAuditUserCheck';

  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec '+sSP+' '''+dsBrowse.DataSet.FieldByname('PartNum').AsString
            +''','''+dsBrowse.DataSet.FieldByname('Revision').AsString
            +''','''+sUserId+'''');
    Open;
    if FieldByName('ReturnStr').AsString<>'' then
    begin
      MsgDlgJS(FieldByName('ReturnStr').AsString,mtWarning, [mbOk],0);
      abort;
    end
  end;
end;

function TfrmProductDLL.StatusCheck(Sender: TObject): Integer;
var ibNeedInEdit:Integer;
begin
  inherited;
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('select bNeedInEdit from CURdOCXItemCustButton(nolock)'
           +' where ItemId='''+sItemId+''''
           +' and ButtonName='+''''+TSpeedButton(Sender).Name+''''
           +' and bVisible=1');
    Open;
    ibNeedInEdit:=FieldByName('bNeedInEdit').AsInteger;
  end;
  if TSpeedButton(Sender).Name='btnCopyRoute' then
    ibNeedInEdit:=1;
  if ((btnLayerPressIns.Enabled=False) and (ibNeedInEdit=1)) then //Not Edit
  begin
    MsgDlgJS('�~���D�ק襤�A���i�ܧ�!!!',mtWarning, [mbOk],0);
    abort;
  end;
  Result:= ibNeedInEdit;
end;

procedure TfrmProductDLL.GetNewMapData(iKind: Integer);
var MapStr, sPartNum, sRevision: String;
    bCheck: Boolean;
begin
  inherited;
  sPartNum := qryBrowse.FieldByname('PartNum').AsString;
  sRevision := qryBrowse.FieldByname('Revision').AsString;
  qryDetail3.Close;
  with qryMapXFlow do
  begin
    Close;
    Parameters.ParamByName('PartNum').Value := sPartNum;
    Parameters.ParamByName('Revision').Value := sRevision;
    Parameters.ParamByName('MapKind').Value := iKind;
    Open;
    MapStr:='';
    MapStr:=MapStr+FieldByName('MapData').AsString
            +FieldByName('StrMap').AsString
            +FieldByName('StrMap2').AsString
            +FieldByName('MapData2').AsString;//��r�N�|�b�̥~��
  end;
  with qryDetail3 do
  begin
    Open;
    bCheck:=Locate('PartNum;Revision;MapKindNo',VarArrayOf([sPartNum,sRevision,iKind]),[loPartialKey]);
    if bCheck<>true then
    begin
      MsgDlgJS('�Ϥ��ͦ�����!!!',mtWarning, [mbOk],0);
      Exit;
    end
    else
    begin
      Edit;
      FieldByName('MapData').AsString:=MapStr;
      qryMapXFlow.Close; //Post�e Master�|Refresh�A�������|�X��
      Post;
    end;
  end;
end;

procedure TfrmProductDLL.ShowLayerMap;
var sLayerMap, sNowLayer: String;
    sProcStr:String;
begin
  inherited;
  if sPressMapCut='1' then
  begin
    if not Assigned(trvBOM.Selected) then
      sNowLayer:= CurrLayer
    else
      sNowLayer:=TNodeData(trvBOM.Selected.Data^).Id;
  end
  else
    sNowLayer:= CurrLayer;

  if sCusId='YX' then
    sProcStr:='EMOdLayerPressMap_YX'
  else
    sProcStr:='EMOdLayerPressMap';

  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('exec '+sProcStr+' '''+qryBrowse.FieldByname('PartNum').asstring
        +''','''+qryBrowse.FieldByname('Revision').asstring
        +''','''+sNowLayer+'''');
    open;
    sLayerMap:= FieldByName('MapData_1').asstring
              + FieldByName('MapData_2').asstring
              + FieldByName('MapData_3').asstring;
  end;
  XFlowDrawBox2.XRate:= 1.7*XFlowDrawBox2.Width/self.Width;
  XFlowDrawBox2.YRate:= 1.7*XFlowDrawBox2.Width/self.Width;
  XFlowDrawBox2.Content := '';
  XFlowDrawBox2.Content := sLayerMap;
end;

procedure TfrmProductDLL.ShowProdMap;
begin
  inherited;
  try
    if chkViewMapData.Checked then
    begin
      memoMap.Visible:=true;
    end
    else
    begin
      XFlowDrawBox1.XRate:= 1.0*XFlowDrawBox1.Width/self.Width;
      XFlowDrawBox1.YRate:= 1.0*XFlowDrawBox1.Width/self.Width;
      XFlowDrawBox1.Content := qryMap.FieldByName('MapData').asstring;
      XFlowDrawBox1.Refresh;
    end;
  except
  //MsgDlgJS('��ܤu�{�ϮɡA�o�Ͳ��`!',mtInformation,[mbOk],0);
  end;
end;

procedure TfrmProductDLL.btnChangeProcClick(Sender: TObject);
var TmpRouteId:String;
    sNowPart, sNowRev, sNowLayer: String;
begin
  inherited;
  AuditCheck;
  sNowPart:=dsBrowse.DataSet.FieldByname('PartNum').AsString;
  sNowRev:= dsBrowse.DataSet.FieldByname('Revision').AsString;
  if not Assigned(trvBOM.Selected) then
    sNowLayer:= CurrLayer
  else
    sNowLayer:=TNodeData(trvBOM.Selected.Data^).Id;

   TmpRouteId :=qryTmpRouteId.FieldByname('TmpRouteId').AsString;
   Application.createForm(TdlgTmpRouteSelect, dlgTmpRouteSelect);
   dlgTmpRouteSelect.sConnectStr:=sConnectStr;
   dlgTmpRouteSelect.prcDoSetConnOCX;
   //�f�־���
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

      pgeMaster.ActivePageIndex := 0;
      pgeDtl.ActivePageIndex := 1;
      pgeMaster.Pages[1].TabVisible := false;
      Showmodal;
      if modalResult=mrok then
      begin
         //�ܧ�~�{
         with qryExec do
         begin
           Close;
           SQL.Clear;
           SQL.Add('exec EMOdInsLayerRoute '''+sNowPart+''','''+sNowRev+''','
                +''''+sNowLayer+''','''+qryTmpMas.FieldByname('TmpId').AsString+'''');
           Open;
         end;
         //2012.03.08
         UpdateDesigner;

         qryTmpRouteId.Close;
         qryTmpRouteId.Open;
         qryDetail10.Close;
         qryDetail10.Open;
      end;
   end;
end;

procedure TfrmProductDLL.btnBackupNotesClick(Sender: TObject);
var sNowLayer: String;
begin
  inherited;
  AuditCheck;
  if not Assigned(trvBOM.Selected) then
    sNowLayer:= CurrLayer
  else
    sNowLayer:=TNodeData(trvBOM.Selected.Data^).Id;
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('exec EMOdProcNotesIN '''+dsBrowse.DataSet.FieldByname('PartNum').AsString
        +''','''+dsBrowse.DataSet.FieldByname('Revision').AsString
        +''','''+sNowLayer+'''');
    Execsql;
  end;
  MsgDlgJS('�w�x�s�Ƶ�����',mtInformation,[mbOK],0);
end;

procedure TfrmProductDLL.btnPasteNotesClick(Sender: TObject);
var sNowLayer: String;
begin
  inherited;
  AuditCheck;
  if not Assigned(trvBOM.Selected) then
    sNowLayer:= CurrLayer
  else
    sNowLayer:=TNodeData(trvBOM.Selected.Data^).Id;
  with qryExec do
  begin
    Close;
    SQL.Clear;
    SQL.Add('exec EMOdProcNotesOUT '''
        +dsBrowse.DataSet.FieldByname('PartNum').AsString
        +''','''+dsBrowse.DataSet.FieldByname('Revision').AsString
        +''','''+sNowLayer+'''');
    execsql;
  end;
  //2012.03.08
  UpdateDesigner;

  qryDetail10.Refresh;
  MsgDlgJS('�w�٭�Ƶ�����',mtInformation,[mbOK],0);
end;

procedure TfrmProductDLL.btnRouteBOMSetClick(Sender: TObject);
var i: integer;
    //NewString: string;
    //ClickedOK: Boolean;
    iRange: Integer;
begin
  inherited;
  if qryDetail10.RecordCount <=0 then
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

    BPartNum:= qryDetail10.FieldByname('PartNum').asstring;
    BRevision:= qryDetail10.FieldByname('Revision').asstring;
    BLayerId:= qryDetail10.FieldByname('LayerId').AsString;
    BProcCode:= qryDetail10.FieldByname('ProcCode').AsString;
    dlgLayerRouteSet.btFindClick(Sender);
    Showmodal;
      if modalResult=mrok then
      begin
         with qryExec do
         begin
            Close;
            SQL.Clear;
            SQL.Add('exec EMOdProdLayerBomdel '''
              + dsDetail10.DataSet.FieldByname('PartNum').asstring+''','''
              + dsDetail10.DataSet.FieldByname('Revision').asstring+''','''
              +dsDetail10.DataSet.FieldByname('LayerId').AsString+''','''
              +dsDetail10.DataSet.FieldByname('ProcCode').AsString+'''');
            execsql;
         end;
         with qryProcCodeBOMSet do
         begin
            for i:= 0 to msSelects.TargetItems.Count-1 do
            begin
               Parameters.Parambyname('PartNum').Value:= dsDetail10.DataSet.FieldByname('PartNum').asstring;
               Parameters.Parambyname('Revision').Value:= dsDetail10.DataSet.FieldByname('Revision').asstring;
               Parameters.ParambyName('LayerId').Value:= dsDetail10.DataSet.FieldByname('LayerId').AsString;
               Parameters.ParambyName('ProcCode').Value:= dsDetail10.DataSet.FieldByname('ProcCode').AsString;
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
   qryDetail11.close;
   qryDetail11.open;
end;

procedure TfrmProductDLL.btnNotesStyleTreeClick(Sender: TObject);
var sNowLayer: String;
    NowPartNum, NowRevision: String;
begin
  inherited;
  AuditCheck;
  if not Assigned(trvBOM.Selected) then
    sNowLayer:= CurrLayer
  else
    sNowLayer:=TNodeData(trvBOM.Selected.Data^).Id;
  NowPartNum:= dsBrowse.DataSet.FieldByname('PartNum').AsString;
  NowRevision:= dsBrowse.DataSet.FieldByname('Revision').AsString;

   Application.createForm(TdlgTmpRouteSelect, dlgTmpRouteSelect);
   dlgTmpRouteSelect.sConnectStr:=sConnectStr;
   dlgTmpRouteSelect.prcDoSetConnOCX;
   //�f�־���
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
      CurrPartNum := NowPartNum;
      CurrRevision := NowRevision;
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
         //�s�W�~�{�Ƶ�
         With qryExec do
         Begin
           Close;
           SQL.Clear;
           SQL.Add('Exec EMOdProcNotesInsert '''+NowPartNum+''', '''+NowRevision
              +''', '''+sNowLayer+'''');
           Execsql;
         End;
         //2012.03.08
         UpdateDesigner;

         qryDetail10.Close;
         qryDetail10.Open;
         With qryExec do
         Begin
           Close;
           SQL.Clear;
           SQL.Add('Delete EMOdTmp Where PartNum = '''+NowPartNum+''' '
              +'And Revision = '''+NowRevision+'''');
           Execsql;
         End;
      end;
    end;
end;

function TfrmProductDLL.CopyFileStr(sSrc, sDest: String): Boolean;
var sDirToCreate: String;
    bCopyRslt: Boolean;
begin
  Result := True;
  try
    sDirToCreate:= ExtractFilePath(sDest);
    ForceDirectories(sDirToCreate);
    bCopyRslt:= CopyFile(Pchar(sSrc), Pchar(sDest), false);
    if not bCopyRslt then
      Result := false;
  except
    Result := false;
  end;
end;

procedure TfrmProductDLL.dbgBOMEnter(Sender: TObject);
begin
  inherited;
  if nav2.DataSource<>dsDetail11 then
    nav2.DataSource:=dsDetail11;
end;

procedure TfrmProductDLL.dbgRouteEnter(Sender: TObject);
begin
  inherited;
  if nav2.DataSource<>dsDetail10 then
    nav2.DataSource:=dsDetail10;
end;

function TfrmProductDLL.GetTempPathStr : WideString;
{var PCr : Array[0..1024] of char;
    Len : DWORD;}
//2022.12.08
var sTmpStr:String;
begin
  //2022.12.08 �令��Ѽ�(�������ɰѼƤ@�P)
  sTmpStr:='';
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams(nolock) '
         +'where SystemId=''EMO'' and ParamId=''JpgPath''');
    Open;
    sTmpStr:=FieldByName('Value').AsString;
  end;
  Result:=sTmpStr;
  //Result:= 'C:\JSdTemp\'; //2010.03.08 �I�s GetTempPath �o�Ͳ��`
  //Result:= 'C:\JSdTemp\';
  {Len:=SizeOf(PCr);
  if GetTempPath(Len, PCr)>0 then
    Result:= StrPas(PCr)
  else
    Result:='';}
end;

procedure TfrmProductDLL.LockButton(bStatus: Boolean);
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
  //�ΰj��|��Ҧ��\�ೣ�����A�אּ��ʫ��w
  if bStatus=True then
  begin
    btnLayerPressIns.Enabled:=False;
    btnPressChange.Enabled:=False;
    btnLayerPressUpdate.Enabled:=False;
    chkJHCoreCom.Enabled:=False;
    btnAutoDraw.Enabled:=False;
    //btnModifySet.Enabled:=False;
    btnTierIns.Enabled:=False;
    btnJHPressChg.Enabled:=False;
    btnCMap.Enabled:=False;
    btnSMap.Enabled:=False;
    btnProdUseMat.Enabled:=False;
    dbgLayerPress.ReadOnly:=True;
    dbgProdTier.ReadOnly:=True;
    dbgRoute.ReadOnly:=True;
    dbgBOM.ReadOnly:=True;
    btnChangeProc.Enabled:=False;
    btnNotesStyleTree.Enabled:=False;
    btnBackupNotes.Enabled:=False;
    btnPasteNotes.Enabled:=False;
    btnRouteChange.Enabled:=False;
    btnRouteBOMSet.Enabled:=False;
    btnCopyRoute.Enabled:=False;
    btnUseNotes.Enabled:=False;
    DBMemo1.ReadOnly:=True;
    //100607 add
    navMills.Enabled:=False;
    navWriting.Enabled:=False;
    dbgMills.ReadOnly:=True;
    dbgWriting.ReadOnly:=True;
    dbgPartMergePrint.ReadOnly:=True;
    cboWhatRev.Enabled:=False;
    {edtCnum.Enabled:=False;
    DBEdit144.Enabled:=False;
    cboDoType.Enabled:=False;
    DBEdit126.Enabled:=False;
    DBEdit127.Enabled:=False;}
    if iUpdateECNLog=1 then //100929
    begin
      qryDetail7B.Close;
      qryDetail7B.LockType:=ltReadOnly;
      qryDetail7B.Open;
    end;
    //2011.05.19
    iPressWarn:=0;
  end
  else
  begin
    btnLayerPressIns.Enabled:=True;
    btnPressChange.Enabled:=True;
    btnLayerPressUpdate.Enabled:=True;
    chkJHCoreCom.Enabled:=True;
    btnAutoDraw.Enabled:=True;
    //btnModifySet.Enabled:=True;
    btnTierIns.Enabled:=True;
    btnJHPressChg.Enabled:=True;
    btnCMap.Enabled:=True;
    btnSMap.Enabled:=True;
    btnProdUseMat.Enabled:=True;
    dbgLayerPress.ReadOnly:=False;
    dbgProdTier.ReadOnly:=False;
    dbgRoute.ReadOnly:=False;
    dbgBOM.ReadOnly:=False;
    btnChangeProc.Enabled:=True;
    btnNotesStyleTree.Enabled:=True;
    btnBackupNotes.Enabled:=True;
    btnPasteNotes.Enabled:=True;
    btnRouteChange.Enabled:=True;
    btnRouteBOMSet.Enabled:=True;
    btnCopyRoute.Enabled:=True;
    btnUseNotes.Enabled:=True;
    DBMemo1.ReadOnly:=False;
    //100607 add
    navMills.Enabled:=True;
    navWriting.Enabled:=True;
    dbgMills.ReadOnly:=False;
    dbgWriting.ReadOnly:=False;
    dbgPartMergePrint.ReadOnly:=False;
    cboWhatRev.Enabled:=True;
    if iUpdateECNLog=1 then //100929
    begin
      qryDetail7B.Close;
      qryDetail7B.LockType:=ltOptimistic;
      qryDetail7B.Open;
    end;
  end;
end;


procedure TfrmProductDLL.AuditSetting;

begin
  //�f�ְh�f�����ҥ�
  {if btnRejExam.Visible=True then
    btnRejExam.Enabled:=True;
  if btnExam.Visible=True then
    btnExam.Enabled:=True;}
  //2012.07.11
  btnFinish.Enabled:= btnRejExam.Enabled;
end;


procedure TfrmProductDLL.OpenRoute;

var sNowLayer :String;

begin
  if not Assigned(trvBOM.Selected) then
    sNowLayer:= CurrLayer
  else
    sNowLayer:=TNodeData(trvBOM.Selected.Data^).Id;
  with qryTmpRouteId do
  begin
    Close;
    Parameters.Parambyname('PartNum').Value:= qryBrowse.FieldByName('PartNum').AsString;
    Parameters.Parambyname('Revision').Value:= qryBrowse.FieldByName('Revision').AsString;
    Parameters.Parambyname('LayerId').Value:= sNowLayer;
    Open;
  end;
end;


procedure TfrmProductDLL.ShowRealMap(iKind: Integer);

var sMapName:String;

    sMapPath:String;

    jJPG: TImage;

    SX, SY: Integer;

    RX, RY: Double;

begin

  ImgPOP.Visible:=False;

  ImgLayer.Visible:=False;

  if iKind=0 then

    sMapName:='stackup'

  else if iKind=1 then

    sMapName:='panel'

  else if iKind=3 then

    sMapName:='CUT'

  else

    sMapName:='';


  if sMapName<>'' then

  begin

    sMapPath:=trim(sRealMapPath)

            +trim(qryBrowse.FieldByName('PartNum').AsString)

            +trim(qryBrowse.FieldByName('Revision').AsString)

            +'_'

            +trim(sMapName)

            +'.jpg';

    if FileExists(sMapPath) then
    begin
      if iKind=0 then
      begin
        ImgLayer.Picture.LoadFromFile(sMapPath);
        ImgLayer.Visible:=True;
      end
      else
      begin
        jJPG:=TImage.Create(tabBrowse);
        jJPG.Visible:=False;
        jJPG.AutoSize:=True;
        jJPG.Picture.LoadFromFile(sMapPath);
        SX:=jJPG.Width;
        SY:=jJPG.Height;
        RX:=pnlJPG.Width / SX;
        RY:=pnlJPG.Height / SY;
        //��Һ⧹
        if RY>RX then
        begin
          ImgPOP.Width:=pnlJPG.Width;
          ImgPOP.Height:=Round(SY * RX);
        end
        else
        begin
          ImgPOP.Width:=Round(SX * RY);
          ImgPOP.Height:=pnlJPG.Height;
        end;
        ImgPOP.Picture.LoadFromFile(sMapPath);
        ImgPOP.Visible:=True;
        jJPG.Free;
      end;
    end;
  end;
end;

procedure TfrmProductDLL.MapOpen;
var iMapType:Integer;
begin
  iMapType:=qryBrowse.FieldByName('MapType').AsInteger;
  //2010.09.04
  if iMapType=0 then
  begin
    pnlJPG.Visible:=False;
    pnlXFlow.Visible:=True;
    //pnlMapTools.Visible:=True;
    pnlMapTool1.Visible:=True;
    pnlMapTool2.Visible:=True;
    pnlMapTool3.Visible:=True;
  end
  else
  begin
    pnlJPG.Visible:=True;
    pnlXFlow.Visible:=False;
    //pnlMapTools.Visible:=False;
    pnlMapTool1.Visible:=False;
    pnlMapTool2.Visible:=False;
    pnlMapTool3.Visible:=False;
  end;
  if iMapType=0 then
  begin
    try
      XFlowDrawBox1.XRate:= 1.0*XFlowDrawBox1.Width/self.Width;
      XFlowDrawBox1.YRate:= 1.0*XFlowDrawBox1.Width/self.Width;
    finally
      ShowProdMap;
    end;
  end
  else
    ShowRealMap(qryDetail3.FieldByName('MapKindNo').AsInteger);
end;

procedure TfrmProductDLL.nav1Click(Sender: TObject; Button: TNavigateBtn);
begin
  inherited;
  //ShowMessage(nav1.DataSource.DataSet.Name);
end;

procedure TfrmProductDLL.PressMapOpen;
var iPressMapType:Integer;
begin
  iPressMapType:=qryBrowse.FieldByName('MapType').AsInteger;
  if iPressMapType=0 then
  begin
    pnlXFlow2.Visible:=True;
    XFlowDrawBox2.Visible:=True;
    pnlLayerMap.Visible:=False;
    ShowLayerMap;
  end
  else
  begin
    pnlXFlow2.Visible:=False;
    XFlowDrawBox2.Visible:=False;
    pnlLayerMap.Visible:=True;
    ShowRealMap(0);
  end;
end;

procedure TfrmProductDLL.UpdateDesigner;
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
                +sUserId+''',1');
      ExecSql;
    end;
    iNeedChkDesigner:=0; //�u�ˬd�@���N�n
  end;
end;

procedure TfrmProductDLL.ClearAskTmp(NowSPId: Integer);
begin
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('delete EMOdFactorAsk where SPId='+IntToStr(NowSPId));
    ExecSql;
    qryExec.Close;
  end;
end;

procedure TfrmProductDLL.prcEMODetailSet; //2012.09.17 add
begin
  inherited;
  with qryPage do
  begin
    //��Ӫ�
    qryPage.Close;
    SQL.Clear;
    SQL.Add('exec EMOdProdFormSet '''+sItemId+'''');
    Open;
    First;
    while not Eof do
    begin
      TTabSheet(Self.FindComponent('tbshtDetail'
          +FieldByName('KindItem').AsString)).PageIndex
        :=FieldByName('SerialNum').AsInteger-1;
      if FieldByName('IsHide').AsInteger=1 then
        TTabSheet(Self.FindComponent('tbshtDetail'
          +FieldByName('KindItem').AsString)).TabVisible:=False;
      Next;
    end;
  end;//qryPage end
end;

end.

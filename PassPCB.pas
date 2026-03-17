unit PassPCB;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic, DB, ADODB, Buttons, ExtCtrls, Menus, JSdPopupMenu,
  Wwdatsrc, JSdTable, DBCtrls, ComCtrls, StdCtrls, ToolWin, JSdLookupCombo,
  Grids, Wwdbigrd, Wwdbgrid, Mask,JSdMultSelect, JSdDBGrid,Math
  ,shellapi //2013.09.04 add for NIS Bill-20130815-03
  ;

type
  TfrmFMEdPassPCB = class(TfrmTempPublic)
    qryPassSetXOut: TADOQuery;
    tblMaster: TJSdTable;
    tblMasterPaperNum: TStringField;
    tblMasterPaperDate: TDateTimeField;
    tblMasterNotes: TWideStringField;
    tblMasterUserId: TStringField;
    tblMasterBuildDate: TDateTimeField;
    tblMasterStatus: TIntegerField;
    tblMasterFinished: TIntegerField;
    tblMasterCancelUser: TStringField;
    tblMasterCancelDate: TDateTimeField;
    tblMasterFinishUser: TStringField;
    tblMasterFinishDate: TDateTimeField;
    tblMasterUseId: TStringField;
    tblMasterPaperId: TStringField;
    tblMasterMrgLotQnty: TFloatField;
    tblMasterMrgLotNum: TStringField;
    qryIssue: TADOQuery;
    tblDetail: TJSdTable;
    tblDetailItem: TIntegerField;
    tblDetailLotNum: TStringField;
    tblDetailLayerName: TWideStringField;
    tblDetailQnty: TFloatField;
    tblDetailPQnty: TFloatField;
    tblDetailSQnty: TFloatField;
    tblDetailUQnty: TFloatField;
    tblDetailXOutQnty: TFloatField;
    tblDetailDateCode: TStringField;
    tblDetailLayerId: TStringField;
    tblDetailPartNum: TStringField;
    tblDetailRevision: TStringField;
    tblDetailProcName: TWideStringField;
    tblDetailPOPName: TWideStringField;
    tblDetailNotes: TWideStringField;
    tblDetailSourNum: TStringField;
    tblDetailSourItem: TIntegerField;
    tblDetailProcCode: TStringField;
    tblDetailPOP: TIntegerField;
    tblDetailStockId: TStringField;
    tblDetailAftProc: TStringField;
    tblDetailAftLayer: TStringField;
    tblDetailAftPOP: TIntegerField;
    tblDetailEquipId: TStringField;
    tblDetailAftLayerName: TWideStringField;
    tblDetailAftProcName: TWideStringField;
    tblDetailAftPOPName: TWideStringField;
    tblDetailPaperNum: TStringField;
    qryPassECNLotNum: TADOQuery;
    qryPassSelectLot: TADOQuery;
    qryPassSelectLotLotNum: TStringField;
    qryPassSelectLotPartNum: TStringField;
    qryPassSelectLotRevision: TStringField;
    qryPassSelectLotProcName: TWideStringField;
    qryPassSelectLotLayerName: TWideStringField;
    qryPassSelectLotPnPName: TWideStringField;
    qryPassSelectLotRestQnty: TFloatField;
    qryPassSelectLotPOP: TSmallintField;
    qryPassSelectLotProcCode: TStringField;
    qryPassSelectLotLayerId: TStringField;
    qryPassSelectLotStockId: TStringField;
    qryPassSelectLotLotStatus: TIntegerField;
    qryPassSelectLotHalted: TIntegerField;
    qryPassSelectLotFtrHalt: TIntegerField;
    qryPassSelectLotHaltProc: TStringField;
    qryPassSelectLotRouteSerial: TIntegerField;
    qryPassSelectLotAftPOP: TIntegerField;
    qryPassSelectLotAftLayer: TStringField;
    qryPassSelectLotAftProc: TStringField;
    qryPassSelectLotAftPOPName: TWideStringField;
    qryPassSelectLotAftProcName: TWideStringField;
    qryPassSelectLotAftLayerName: TWideStringField;
    qryPassSelectLotPQnty: TFloatField;
    qryPassSelectLotSQnty: TFloatField;
    qryPassSelectLotUQnty: TFloatField;
    qryPassSelectLotProcType: TIntegerField;
    qryPassSelectLotMatchCount: TIntegerField;
    qryPassSelectLotLLPCS: TFloatField;
    qryPassSelectLotLPCS: TFloatField;
    qryPassSelectLotIsStorage: TIntegerField;
    qryPassSelectLotAftProcType: TIntegerField;
    qryPassSelectLotMergeRoute: TIntegerField;
    qryPassSelectLotIsPrePass: TIntegerField;
    qryPassSelectLotIsDateCode: TIntegerField;
    qryPassSelectLotIsXOut: TIntegerField;
    qryPassSelectLotPOType: TIntegerField;
    qryPassSelectLotIsShowPQnty: TIntegerField;
    qryPassSelectLotDateCode: TStringField;
    tblLotXOut: TADOQuery;
    IntegerField6: TIntegerField;
    IntegerField1: TFloatField;
    StringField1: TStringField;
    tblLotXOutPaperNum: TStringField;
    qryProcType: TADOQuery;
    qryLotNum: TADOQuery;
    qryProcUser: TADOQuery;
    qryTmp2XOut: TADOQuery;
    dsPassSelect: TwwDataSource;
    qryAutoVoidPaper: TADOQuery;
    qryLotTo36: TADOQuery;
    qryLotTo36ShortLotNum: TStringField;
    qryEquipProc: TJSdTable;
    qryEquipProcEquipId: TStringField;
    qryEquipProcEquipName: TWideStringField;
    qryInStockPrint: TADOQuery;
    dsEquipProc: TDataSource;
    dsDetail: TDataSource;
    qryPassDivLotNum: TADOQuery;
    qryIsCheckProc: TADOQuery;
    pmuPaperPaper: TJSdPopupMenu;
    dsLotXOut: TwwDataSource;
    dsMaster: TDataSource;
    qryPOP: TADOQuery;
    qryProcBasic: TADOQuery;
    dsProcBasic: TDataSource;
    dsEquipId: TDataSource;
    qryEquipId: TADOQuery;
    qryEquipIdEquipId: TStringField;
    qryEquipIdEquipName: TWideStringField;
    dsProdLayer: TDataSource;
    qryProdLayer: TADOQuery;
    dsPOP: TDataSource;
    dsLotStatus: TwwDataSource;
    qryLotStatus: TADOQuery;
    qryPOType: TADOQuery;
    dsPOType: TwwDataSource;
    qryEquipProcProcCode: TStringField;
    tblMasterFlowStatus: TIntegerField;
    tblDetailScrapStrXOutQnty: TFloatField;
    qryXOutDefect: TJSdTable;
    dsXOutDefect: TDataSource;
    qryPassSelectLotXOutNeedDefect: TIntegerField;
    qryXOutDefectPaperNum: TStringField;
    qryXOutDefectItem: TIntegerField;
    qryXOutDefectSerialNum: TIntegerField;
    qryXOutDefectClassId: TStringField;
    qryXOutDefectDefectId: TStringField;
    qryXOutDefectDutyProc: TStringField;
    qryXOutDefectQnty: TFloatField;
    qryXOutDefectCompanyId: TStringField;
    qryClassId: TADOQuery;
    qryDefectId: TADOQuery;
    qryClassIdClassId: TStringField;
    qryClassIdClassName: TWideStringField;
    qryDefectIdDefectId: TStringField;
    qryDefectIdDefectName: TWideStringField;
    qryXOutDefectLk_ClassName: TWideStringField;
    qryXOutDefectLk_DefectName: TWideStringField;
    qryXOutDefectLk_DutyProcName: TWideStringField;
    tblLotXOutOrgQnty: TFloatField;
    qryClassIdResProc: TWideStringField;
    qryTmp2XOutNewAdd: TADOQuery;
    qryProcDepartMerge: TADOQuery;
    qryFQC_XOutTOL: TADOQuery;
    dsFQC_XOutTOL: TDataSource;
    tblDetailTranPQnty: TFloatField;
    qryProdLayerPartNum: TStringField;
    qryProdLayerRevision: TStringField;
    qryProdLayerLayerId: TStringField;
    qryProdLayerLayerName: TWideStringField;
    tblDetailStkVoidPQnty: TFloatField;
    tblDetailTranMatQnty: TFloatField;
    qryPassSelectLotTranMatQnty: TFloatField;
    tblDetailiIsSinglePass: TIntegerField;
    qryPassSelectLotDtlNotes: TStringField;
    page_PassPCB_120827A: TPageControl;
    tbsht_PassPCB_Lot: TTabSheet;
    pnl_Center: TPanel;
    pnlXContainer: TPanel;
    pnlDetail: TPanel;
    btnExecute: TButton;
    edt_DtlNotes: TDBText;
    navDetail: TDBNavigator;
    navMain: TDBNavigator;
    chkMOPrint: TCheckBox;
    chkPaperPrint: TCheckBox;
    btnClose: TBitBtn;
    pnlMain: TPanel;
    Label3D5: TLabel;
    Label3D6: TLabel;
    Label3D9: TLabel;
    Label3D10: TLabel;
    Label3D11: TLabel;
    Label3D12: TLabel;
    Label3D13: TLabel;
    lab_PQnty: TLabel;
    Label3D15: TLabel;
    Label3D16: TLabel;
    lab_SQnty: TLabel;
    lab_UQnty: TLabel;
    Label1: TLabel;
    labLotNum: TLabel;
    Label2: TLabel;
    labLLPCS: TLabel;
    btnGetLot: TSpeedButton;
    lab_ScrapStrXOutQnty: TLabel;
    btnTmp2Xout: TSpeedButton;
    btnGetLotData: TButton;
    lab_TranPQnty: TLabel;
    lab_StkVoidQnty: TLabel;
    lab_TranMatQnty: TLabel;
    lab_LineId: TLabel;
    lab_MasterNotes: TLabel;
    edtPartNum: TDBEdit;
    edtRevision: TDBEdit;
    edtPOPName: TDBEdit;
    edtLayerId: TDBEdit;
    edtProcName: TDBEdit;
    edtQnty: TDBEdit;
    edtLotNum: TDBEdit;
    DBEdit1: TDBEdit;
    DBEdit2: TDBEdit;
    DBEdit3: TDBEdit;
    edtPQnty: TDBEdit;
    edtSQnty: TDBEdit;
    edtUQnty: TDBEdit;
    pnlDateCode: TPanel;
    Label3: TLabel;
    edtDateCode: TDBEdit;
    pnlXOutSum: TPanel;
    labXOutXXX: TLabel;
    edtXOutXXX: TDBEdit;
    pnl_EquipId: TPanel;
    JSdLookupCombo1: TJSdLookupCombo;
    edtProcCode: TDBEdit;
    DBEdit5: TDBEdit;
    DBEdit6: TDBEdit;
    DBEdit7: TDBEdit;
    edt_ScrapStrXOutQnty: TDBEdit;
    edtTranPQnty: TDBEdit;
    edtStkVoidPQnty: TDBEdit;
    edtTranMatQnty: TDBEdit;
    edt_LineId: TDBEdit;
    tbsht_PassPCB_Param: TTabSheet;
    Panel4: TPanel;
    dsProcParamDtl: TDataSource;
    qryProcParamDtl: TJSdTable;
    btnProcParamImport: TSpeedButton;
    DBEdit11: TDBEdit;
    DBEdit12: TDBEdit;
    Label14: TLabel;
    Label15: TLabel;
    qryXOutDefectXOutDateCode: TStringField;
    tbsht_PassPCB_XOutNH: TTabSheet;
    JSdDBGrid1: TJSdDBGrid;
    qryFMEdLotXOutDiv: TJSdTable;
    dsFMEdLotXOutDiv: TDataSource;
    Panel8: TPanel;
    DBNavigator4: TDBNavigator;
    btnShowAttachment: TSpeedButton;
    lab_GoodPCS: TLabel;
    qryPassSelectLotGoodPCS: TIntegerField;
    btnEquipString: TSpeedButton;
    btnWorkUserString: TSpeedButton;
    edtEquipString: TDBEdit;
    edtWorkUserString: TDBEdit;
    tblDetailEquipString: TStringField;
    tblDetailWorkUserString: TStringField;
    qryPassSelectLotiNeedMUT_Equip: TIntegerField;
    lab_QC_UserId: TLabel;
    cbo_QC_UserId: TJSdLookupCombo;
    tblDetailQC_UserId: TStringField;
    qryUsers: TADOQuery;
    dsUsers: TDataSource;
    edt_MasterNotes: TDBMemo;
    qryPassSelectLotRevNum: TStringField;
    edtRevNum: TEdit;
    edtRwkQnty: TDBEdit;
    Label8: TLabel;
    tblDetailRwkQnty: TFloatField;
    tblDetailGoodQntyPCS: TFloatField;
    qryXOutDefectQntyOri: TFloatField;
    lab_RwkSQnty: TLabel;
    edtRwkSQnty: TDBEdit;
    tblLotXOutisFullScrap: TIntegerField;
    DBEdit4: TDBEdit;
    qryAllowXOutQnty: TADOQuery;
    dsAllowXOutQnty: TwwDataSource;
    qryAllowXOutQntyAllowXOutQnty: TIntegerField;
    tblDetailRwkScrapQnty: TFloatField;
    lab_RwkSQntySum: TLabel;
    edtRwkSQntySum: TDBEdit;
    tblDetailRwkSQntySum: TFloatField;
    edtRwkPQnty: TDBEdit;
    tblDetailRwkPQnty: TFloatField;
    lab_RwkPQnty: TLabel;
    qryRwkSDefect: TJSdTable;
    IntegerField2: TIntegerField;
    StringField2: TStringField;
    WideStringField1: TWideStringField;
    StringField3: TStringField;
    WideStringField2: TWideStringField;
    StringField4: TStringField;
    WideStringField3: TWideStringField;
    FloatField1: TFloatField;
    StringField5: TStringField;
    IntegerField3: TIntegerField;
    FloatField2: TFloatField;
    dsRwkSDefect: TDataSource;
    tblDetailRouteSerial: TIntegerField;
    qryRwkSDefectLotNum: TWideStringField;
    qryRwkSDefectRouteSerial: TIntegerField;
    qryRwkSDefectRwkProc: TStringField;
    qryProcCode: TADOQuery;
    qryProcCodeProcCode: TStringField;
    qryProcCodeProcName: TStringField;
    qryRwkSDefectLk_RwkProcName: TStringField;
    qryDefectIdResProc: TStringField;
    lblMemoNotes: TLabel;
    memoNotes: TMemo;
    JSdDBGrid3: TJSdDBGrid;
    Label16: TLabel;
    Panel6: TPanel;
    Panel5: TPanel;
    btnParamProcClear: TSpeedButton;
    DBNavigator3: TDBNavigator;
    splitter_XOutDefect: TSplitter;
    Panel10: TPanel;
    Panel9: TPanel;
    pnl_XOutDefect: TPanel;
    gridXOutDefect: TJSdDBGrid;
    Panel1: TPanel;
    DBNavigator1: TDBNavigator;
    gridRwkSDefect: TJSdDBGrid;
    Panel7: TPanel;
    pnl_FQC_XOut_TOL: TPanel;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    DBEdit8: TDBEdit;
    DBEdit9: TDBEdit;
    DBEdit10: TDBEdit;
    pnlXOut: TPanel;
    Panel3: TPanel;
    Label4: TLabel;
    DBNavigator2: TDBNavigator;
    wwDBGrid1: TwwDBGrid;
    pnlPassMrg: TPanel;
    Panel2: TPanel;
    btFind: TSpeedButton;
    gridQnty: TwwDBGrid;
    procedure tblDetailAfterEdit(DataSet: TDataSet);
    procedure tblDetailAfterInsert(DataSet: TDataSet);
    procedure tblDetailAfterPost(DataSet: TDataSet);
    procedure tblDetailBeforeInsert(DataSet: TDataSet);
    procedure tblMasterAfterEdit(DataSet: TDataSet);
    procedure tblMasterAfterInsert(DataSet: TDataSet);
    procedure tblMasterAfterPost(DataSet: TDataSet);
    procedure tblDetailBeforePost(DataSet: TDataSet);
    procedure tblDetailPQntyValidate(Sender: TField);
    procedure tblDetailSQntyValidate(Sender: TField);
    procedure tblDetailProcCodeChange(Sender: TField);
    procedure tblLotXOutAfterPost(DataSet: TDataSet);
    procedure tblLotXOutBeforeInsert(DataSet: TDataSet);
    procedure btnGetLotClick(Sender: TObject);
    procedure edtLotNumClick(Sender: TObject);
    procedure prcImport(Sender: TObject);
    procedure edtLotNumKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure btnGetParamsClick(Sender: TObject);
    procedure edtPQntyClick(Sender: TObject);
    procedure edtPQntyExit(Sender: TObject);
    procedure edtSQntyClick(Sender: TObject);
    procedure edtSQntyExit(Sender: TObject);
    procedure edtUQntyClick(Sender: TObject);
    procedure edtXOutXXXClick(Sender: TObject);
    procedure btFindClick(Sender: TObject);
    procedure tblMasterAfterOpen(DataSet: TDataSet);
    procedure JSdLookupCombo1Dropdown(Sender: TObject);
    procedure btnExecuteClick(Sender: TObject);
    procedure btnCloseClick(Sender: TObject);
    procedure tblLotXOutBeforeDelete(DataSet: TDataSet);
    procedure qryXOutDefectNewRecord(DataSet: TDataSet);
    procedure btnTmp2XoutClick(Sender: TObject);
    procedure qryXOutDefectClassIdValidate(Sender: TField);
    procedure btnGetLotDataClick(Sender: TObject);
    procedure qryXOutDefectBeforeInsert(DataSet: TDataSet);
    procedure wwDBGrid1Enter(Sender: TObject);
    procedure tblDetailNewRecord(DataSet: TDataSet);
    procedure qryProcParamDtlBeforeDelete(DataSet: TDataSet);
    procedure qryProcParamDtlBeforeInsert(DataSet: TDataSet);
    procedure btnProcParamImportClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnParamProcClearClick(Sender: TObject);
    procedure edtSQntyChange(Sender: TObject);
    procedure tblDetailAfterCancel(DataSet: TDataSet);
    procedure tblDetailBeforeCancel(DataSet: TDataSet);
    procedure tblDetailAfterScroll(DataSet: TDataSet);
    procedure tblDetailBeforeDelete(DataSet: TDataSet);
    procedure qryFMEdLotXOutDivBeforeInsert(DataSet: TDataSet);
    procedure qryFMEdLotXOutDivBeforeDelete(DataSet: TDataSet);
    procedure page_PassPCB_120827AChange(Sender: TObject);
    procedure btnShowAttachmentClick(Sender: TObject);
    procedure btnEquipStringClick(Sender: TObject);
    procedure btnWorkUserStringClick(Sender: TObject);
    procedure edt_ScrapStrXOutQntyExit(Sender: TObject);
    procedure edtRwkPQntyExit(Sender: TObject);
    procedure StringField2Validate(Sender: TField);
    procedure qryRwkSDefectNewRecord(DataSet: TDataSet);
    procedure qryRwkSDefectBeforeInsert(DataSet: TDataSet);
    procedure qryRwkSDefectBeforeDelete(DataSet: TDataSet);
    procedure qryXOutDefectDefectIdValidate(Sender: TField);
    procedure StringField3Validate(Sender: TField);

  private
    { Private declarations }
    eOrgPQnty:Extended; //2010.10.20 add for YX Bill-20101012-2
    CurrLotNum:String;
    sPassSelectDlgMode:string;//2011.2.11 add
    sSinglePassShowQCUser:string;//2017.06.23 add for CMT
    procedure ShowXOut;
    function PassMrgSelect(Sender: TObject): Boolean;
    procedure prcInitial;
    procedure prcShowFQC_XOutTOL;
    procedure prcShowOverPQnty(bShow: boolean);
    procedure prcProcParamDtlOpen(sPaperNum:string);//2012.09.12 add for NH Bill-20120704-05
    function funProcParamDtlCheck: boolean;//2012.09.12 add for NH Bill-20120704-05
    procedure prcProcParamDtlDo(sKind: string);//2012.09.12 add for NH Bill-20120704-05
    function funProcParamExists: boolean;
    procedure prcFMEdLotXOutDivOpen(sPaperNum: string); //2012.09.12 add for NH Bill-20120704-05
    //function funChkWIPInStk2AssInStk: boolean;
    function funCheckAttachment: boolean;
  public
    { Public declarations }
    bUseXoutYX:boolean;
    bUseMergeYX:boolean;
    CurrPaperNum: string;
    bNewCancel: Boolean;
    CurrPaperType: integer;
    bPassUseTranPQnty:boolean; //2011.6.15 add for NH Bill-20110609-2
    sDefStkIdOverProd:string;//2011.6.15 add for NH Bill-20110609-2
    RwkStatus:integer;//2021.04.26
    CusId:string;
    iDutyProc:integer;
    iDlgMode, iFocusMode:Integer; //2022.04.25
  end;

var
  frmFMEdPassPCB: TfrmFMEdPassPCB;

implementation

uses unit_DLL, PassSelect, PassMrgSelect, commParent, ShowDLLForm, ShowMsg4Copy,
  unit_ERP, EquipSelectMUT,WorkerSelectMUT;

var bIsShowPQnty,bIsCNC,bIsIssue:boolean;
    sRunSQLAfterAdd:string;
    sPrePass:string;
    bWIPInStk2AssInStk:boolean;
    iMinPOP:integer;//2010.7.8 add
    bUseLineId:boolean;//2012.1.2 add
    bPassUseMasterNotes:boolean;//2012.3.21 add for NH Bill-20120314-02
    bPassUseNHParam:boolean;//2012.08.27 add for NH Bill-20120704-05
    bCmpValueAftPass:boolean;//2012.08.27 add for NH Bill-20120704-05
    bPassSQntyBack2PQnty:boolean;//2012.09.25 add for CMT Bill-20120906-01
    bSQntyChange:boolean;//2012.09.25 add for CMT Bill-20120906-01

    bMergDivAutoPass:boolean;//2012.12.12 add for NH Bill-20121206-01
    bInnerAllPass:String;  //2022.12.20 boolean;//2013.01.18 add for 提高效能

    //2017.07.24 add for MUT
    bPass2DCodeReadOnly:boolean;
    //2021.11.19
    bWPnlFSProc:boolean;
{$R *.dfm}

procedure TfrmFMEdPassPCB.btnCloseClick(Sender: TObject);
begin
  inherited;
//  TForm(commParent.tmpParent.Owner).Close;
end;

procedure TfrmFMEdPassPCB.btFindClick(Sender: TObject);
begin
  inherited;

  if funProcParamExists then exit;//2012.09.12 add for NH Bill-20120704-05

  if tblDetail.fieldbyname('AftProc').asstring='' then
  begin
     MsgDlgJS('批號無下站製程!!', mtInformation, [mbok], 0);
     exit;
  end;
  if tblDetail.fieldbyname('AftLayer').asstring='' then
  begin
     MsgDlgJS('批號無壓合後階段!!', mtInformation, [mbok], 0);
     exit;
  end;
  if not PassMrgSelect(Sender) then
   Exit;

end;

function TfrmFMEdPassPCB.PassMrgSelect(Sender: TObject):Boolean;
var i: integer;
    sSQL: string;
    iChkMrgXOut:integer;
begin
  inherited;
  Result := false;

  //2011.12.28 add,解決第二批(含)之後的批不會出現在選單，而選單上的批被選入前，原第二批(含)之後的批都被刪除的異常
  with tblDetail do
      begin
           First;

           Next;

           while not eof do
           begin
             if FieldByName('Item').AsInteger <> 1 then

             Delete;

             if tblDetail.RecordCount = 1 then
               break;
           end;
      end;


  Application.createform(TdlgPassMrgSelect, dlgPassMrgSelect);
  dlgPassMrgSelect.qryPassMrgSelect.ConnectionString:=qryExec.ConnectionString;

  with dlgPassMrgSelect do
  begin
     with qryPassMrgSelect do
     begin
       close;
       Parameters.ParamByName('PaperNum').Value:=tblDetail.fieldbyname('PaperNum').asstring;
       open;
     end;

     msProcSelect.Setup(slAll);

     showmodal;

     if modalResult=mrok then
     begin
        with tblDetail, msProcSelect do
        begin

           First;

          //2011.12.28 disable,解決第二批(含)之後的批不會出現在選單，而選單上的批被選入前，原第二批(含)之後的批都被刪除的異常
           {Next;

           while not eof do
           begin
             if FieldByName('Item').AsInteger <> 1 then

             Delete;

             if tblDetail.RecordCount = 1 then
               break;
           end;
           }
           for i:= 0 to TargetItems.Count-1 do
           begin
             Append;
             fieldbyname('LotNum').asstring:=TargetItems[i].Caption;
             fieldbyname('Partnum').asstring:=TargetItems[i].SubItems[0];
             fieldbyname('Revision').asstring:=TargetItems[i].SubItems[1];
             fieldbyname('Qnty').Asinteger:=strtoint(TargetItems[i].SubItems[2]);
             fieldbyname('StockId').asstring:=TargetItems[i].SubItems[7];
             fieldbyname('ProcCode').asstring:=TargetItems[i].SubItems[8];
             fieldbyname('LayerId').asstring:=TargetItems[i].SubItems[9];
             fieldbyname('POP').Asinteger:=strtoint(TargetItems[i].SubItems[10]);
             //預設
             fieldbyname('AftProc').asstring:=TargetItems[i].SubItems[11];
             fieldbyname('AftLayer').asstring:=TargetItems[i].SubItems[12];
             fieldbyname('AftPOP').Asinteger:=strtoint(TargetItems[i].SubItems[13]);
             fieldbyname('PQnty').Asfloat:=strtofloat(TargetItems[i].SubItems[14]);
             fieldbyname('SQnty').Asfloat:=strtofloat(TargetItems[i].SubItems[15]);
             fieldbyname('UQnty').Asfloat:=0;
             fieldbyname('XOutQnty').Asfloat:=0;

             Post;
           end;

           tblDetail.Close;
           tblDetail.Parameters.ParamByName('PaperNum').Value:=tblMaster.fieldbyname('PaperNum').asstring;
           tblDetail.Open;

          {//2021.11.11  先補上WPnl單報功能，有需要時再打開
          sSQL:= '';
          sSQL:= 'exec FMEdPassMrgXOutIns '''
                + tblMaster.fieldbyname('PaperNum').asstring+'''';

          unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);

          //2021.11.12 前站有輸入單報在顯示
          sSQL:= '';
          sSQL:= 'exec FMEdPassChkMrgXout '''
                + tblMaster.fieldbyname('PaperNum').asstring+'''';

          unit_DLL.OpenSQLDLL(qryExec,'OPEN',sSQL);

          iChkMrgXOut:=0;
          iChkMrgXOut:=qryExec.FieldByName('iChk').AsInteger;

          if ((iChkMrgXOut=1) or (pnlXOut.Enabled)) and(qryPassSelectLot.Fieldbyname('ProcType').asinteger = 1) then
          begin
             pnlXOut.Enabled:=true;
             label16.Visible:=true;
             tblLotXOut.Close;
             tblLotXOut.Open;

             tblDetail.Close;
             tblDetail.Parameters.ParamByName('PaperNum').Value:=tblMaster.fieldbyname('PaperNum').asstring;
             tblDetail.Open;
          end;}
        end;
        Result := true;
     end;
  end;
end;
{
function TfrmFMEdPassPCB.funChkWIPInStk2AssInStk:boolean;//2010.6.1 add
var sSQL,sMsg:string;
begin
  sSQL:='exec FMEdPassChkWIPInStk '+
    ''''+tblDetail.fieldbyname('AftProc').asstring+'''';

  unit_DLL.OpenSQLDLL(qryExec,'OPEN',sSQL);

  sMsg:=qryExec.FieldByName('sMsg').AsString;

  if sMsg<>'OK' then
    begin
      MsgDlgJS(sMsg, mtError, [mbOk], 0);
      result:=false;
      exit;
    end;

  result:=true;
end;
}

//2017.06.09 add
procedure TfrmFMEdPassPCB.btnEquipStringClick(Sender: TObject);
var sSelectedEquip:string; i:integer;
begin
  inherited;

  if tblDetail.Active=false then exit;

  if tblDetail.RecordCount=0 then exit;

  Application.CreateForm(TfrmEquipSelectMUT,frmEquipSelectMUT);

  frmEquipSelectMUT.sProcCode:=tblDetail.FieldByName('ProcCode').AsString;

  frmEquipSelectMUT.qryEquipMUT.Close;
  frmEquipSelectMUT.qryEquipMUT.ConnectionString:=sConnectStr;

  sSelectedEquip:='';

  frmEquipSelectMUT.ShowModal;

  if frmEquipSelectMUT.ModalResult=mrok then
    begin
      if frmEquipSelectMUT.msDataSelected.TargetItems.Count>0 then
         begin
           for i := 0 to frmEquipSelectMUT.msDataSelected.TargetItems.Count - 1 do
              begin
                if sSelectedEquip<>'' then sSelectedEquip:=sSelectedEquip+',';

                sSelectedEquip:=sSelectedEquip +
                  frmEquipSelectMUT.msDataSelected.TargetItems.Item[i].Caption;
              end;
         end;

     if tblDetail.Active=true then
       begin
         if tblDetail.RecordCount>0 then
           begin
             if not(tblDetail.State in[dsEdit]) then  tblDetail.Edit;

             tblDetail.FieldByName('EquipString').Value :=sSelectedEquip;
           end;
       end;
    end;
end;

procedure TfrmFMEdPassPCB.btnExecuteClick(Sender: TObject);
var //sMrgLotNumSet,
  sSQL:string;
  b2AssStk:boolean;
  sPaperNum4Param:string;
begin
  inherited;
  unit_DLL.prcSaveALL(self);

  //2021.02.01 累積重工報廢數 確認後要關掉
  //lab_RwkSQntySum.Visible:=false;
  //edtRwkSQntySum.Visible:=false;

  //2021.04.26
  {pnl_XOutDefect.Visible:=false;}

  //2020.07.29  重工數
  sSQL:='update FMEdPassSub set RwkQnty = ' + edtRwkQnty.Text
        +' where PaperNum = '
        +''''+tblMaster.FieldByName('PaperNum').AsString+'''';

  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Add(sSQL);
  qryExec.ExecSQL;

  sSQL := '';


  //2012.12.21 搬上去
  sPaperNum4Param:='';
  sPaperNum4Param:=tblMaster.FieldByName('PaperNum').AsString;

  b2AssStk:=false; //2010.6.1 add

  if bWIPInStk2AssInStk then  //2010.6.1 add
     begin
      if unit_ERP.funChkWIPInStk2AssInStk(
        qryExec,
        tblDetail.fieldbyname('AftProc').asstring,
        b2AssStk
        )=false then Exit;
     end;

  if funSysParamsGet('ProcNeedDateCode')='1' then
     begin
      if pnlDateCode.Visible then
        if trim(edtDateCode.Text)='' then
 	        begin
     		    MsgDlgJS('請輸入DateCode', mtError, [mbOk], 0);
     		    Exit;
  	      end;
     end;

  //2008.11.28 add
  if tblLotXOut.Active then
    if tblLotXOut.State in[dsInsert,dsEdit] then  tblLotXOut.Post;

  //檢查預過帳
  qryIsCheckProc.Close;
  qryIsCheckProc.Parameters.ParamByName('AftProc').Value
    := tblDetail.fieldbyname('AftProc').asstring;
  qryIsCheckProc.Open;

  sPrePass:='';

  if qryIsCheckProc.FieldByName('IsCheckPass').AsInteger = 1 then sPrePass:='^3'
  else sPrePass:='^1';

  //----------2012.12.19 add for NH Bill-20121206-03
  if tblLotXOut.Active then
    if tblLotXOut.State in[dsEdit] then tblLotXOut.Post;

  if qryFMEdLotXOutDiv.Active then
    if qryFMEdLotXOutDiv.State in[dsEdit] then qryFMEdLotXOutDiv.Post;

  if bPassUseTranPQnty then
    if tbsht_PassPCB_XOutNH.TabVisible=true then
       begin
         unit_DLL.OpenSQLDLL(qryExec,'EXEC','exec FMEdLotXOutDivPreChk '+''''+sPaperNum4Param+'''');

         if qryFMEdLotXOutDiv.SQL.Text<>'' then
           begin
             qryFMEdLotXOutDiv.Close;
             qryFMEdLotXOutDiv.Open; //一定要 ReOpen,讓操作者看Refresh後的結果,因ExamDo可能會rais error
           end;
       end;
   //----------

   //----------2017.06.14 add for MUT
  if btnWorkUserString.Visible then
     begin
        if trim(edtWorkUserString.Text)='' then
            begin
     		      MsgDlgJS('必須輸入「作業人員」', mtError, [mbOk], 0);
     		      Exit;
            end;

        if trim(edtEquipString.Text)='' then
            begin
              qryExec.Close;
              qryExec.SQL.Clear;
              qryExec.SQL.Text:='exec FMEd2DCodeCheckMUT '+
                ''''+tblDetail.FieldByName('PaperNum').AsString+''''+','+
                tblDetail.FieldByName('Item').AsString+','+
                ''''+tblDetail.FieldByName('ProcCode').AsString+'''';
              qryExec.Open;

              if qryExec.RecordCount>0 then
                 begin
                   if qryExec.FieldByName('iCount').AsInteger>0 then
                      begin
     		                MsgDlgJS('必須輸入「作業機台」', mtError, [mbOk], 0);
     		                Exit;
                      end;
                 end;

              qryExec.Close;
            end;
     end;
   //----------

  if unit_DLL.funPaperCompleted(
    //self,//frm:TForm;
    qryExec,//qry:TADOQuery;
    tblMaster,//tTable:TJSdTable;
    CanbRunFLow,//CanbRunFLow,
    CanbAudit,//CanbAudit:integer;
    sRealTableNameMas1,//sRealTableNameMas1,
    sUserId,//sUserId:string
    CanbLockUserEdit,
    sItemId,
    false,//bUseFlow,
    sUseId,
    sSystemId+sPrePass,
    tblMaster.FieldByName('FlowStatus').AsInteger
    )=false then exit;

  {if MsgDlgJS('已處理成功，是否繼續過帳？',mtConfirmation,[mbYes,mbNo],0)=mrYes then
    begin
      prcInitial;
    end
    else
    begin
      btnClose.Click;
    end;}

//  if sPrePass='^1' then
//    MsgDlgJS('已完成過帳',mtInformation,[mbOk],0);

  //=====
  {sMrgLotNumSet:='';

  with qryGetResult do
  begin
     if active then close;
     Parameters.ParamByName('PaperNum').Value
      :=tblMaster.FieldByName('PaperNum').AsString;
     open;

     if RecordCount>0 then sMrgLotNumSet:=Fields[0].AsString;

     close;
  end;

  if sMrgLotNumSet <> '' then
    //MsgDlgJS('產生壓合後批號：'+sMrgLotNumSet,mtInformation, [mbOk], 0);
    begin
      Application.CreateForm(TfrmShowMsg4Copy,frmShowMsg4Copy);
      frmShowMsg4Copy.meoMsg.Lines.Text:=sMrgLotNumSet;
      frmShowMsg4Copy.ShowModal;
    end;}
  //=====

  sSQL:='exec FMEdPassResult '+''''+
        //tblMaster.FieldByName('PaperNum').AsString
        sPaperNum4Param
        +'''';

  unit_DLL.OpenSQLDLL(qryExec,'OPEN',sSQL);

  if qryExec.RecordCount>0 then
        if qryExec.Fields[0].AsString<>'' then
          begin
            //2022.04.25
            if iDlgMode=1 then
              MsgDlgJS(qryExec.Fields[0].AsString, mtInformation, [mbOk], 0)
            else
            begin
              Application.CreateForm(TfrmShowMsg4Copy,frmShowMsg4Copy);
              frmShowMsg4Copy.meoMsg.Lines.Text:=qryExec.Fields[0].AsString;
              frmShowMsg4Copy.ShowModal;
            end;
          end;

  //2012.09.12 add for NH Bill-20120704-05
  if qryProcParamDtl.Active then qryProcParamDtl.Close;

  //2012.12.21 搬上去
  //sPaperNum4Param:='';
  //sPaperNum4Param:=tblMaster.FieldByName('PaperNum').AsString;

  //2021.05.04 改在匯入時一併恢復原狀
  //2021.03.08 完成再恢復
  //累積重工報廢數
  {lab_RwkSQntySum.Visible:=false;
  edtRwkSQntySum.Visible:=false;
  //報廢數
  lab_SQnty.Visible:=true;
  edtSQnty.Visible:=true;
  //重工報廢數
  lab_RwkSQnty.Visible:=false;
  edtRwkSQnty.Visible:=false;
  //過帳數
  lab_PQnty.Visible:=true;
  edtPQnty.Visible:=true;
  //重工過帳數
  lab_RwkPQnty.Visible:=false;
  edtRwkPQnty.Visible:=false;}

  prcInitial;

  //2012.12.12 add for NH Bill-20121206-01
  //要在產值計算之前執行
  if bMergDivAutoPass then
    begin
       sSQL:='exec FMEdMergDivAutoPass '+
                ''''+sPaperNum4Param+''''+
                ','+''''+sUserId+''''
                ;

       unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);
    end; //if bMergDivAutoPass then

  //2012.09.12 add for NH Bill-20120704-05
  if bPassUseNHParam then
     begin
        if bCmpValueAftPass then
          begin
            sSQL:='exec FMEdCmpValueAftPass '+
                ''''+sPaperNum4Param+''''+
                ','+''''+sUserId+''''
                ;

            unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);
          end;//if bCmpValueAftPass then
     end;//if bPassUseNHParam then
end;

procedure TfrmFMEdPassPCB.prcInitial;
begin
  //2013.11.08 add for NIS James-20131108-01
  if btnShowAttachment.Visible then
      btnShowAttachment.Enabled:=false;

  bSQntyChange:=false;//2012.09.25 add for CMT Bill-20120906-01

  pnlXOutSum.Visible := false;
  btnTmp2Xout.Visible:=false;

  pnlXOutSum.BevelOuter := bvNone;
  pnlDateCode.Visible := false;
  pnlDateCode.BevelOuter := bvNone;
  //pnlPassMrg.Visible := false;
  pnlPassMrg.Enabled := false;
  //pnlXOut.Visible := false;
  pnlXOut.Enabled := false;
  //2019.09.05
  pnl_XOutDefect.Visible := False;

  pnlPassMrg.Visible:=bUseMergeYX;

  unit_DLL.funNewPaper(
    false,//bReGetNum:boolean;
    sUserId,//sUserId:string;
    sUseId,//sUseId:string;
    '',//sPaperNum:string;
    now,//dPaperDate:TDateTime;
    qryExec,//qryExec:TADOQuery;
    tblMaster,//tTable:TJSdTable;
    sRealTableNameMas1,//sRealTableNameMas1:string;
    sSelectSQLMas1,//sSelectSQLMas1:string;
    PaperType,//PaperType:integer;
    CanbSelectType,//CanbSelectType:integer;
    bNewCancel,//var bNewCancel:boolean;
    CurrTypeHead,//var CurrTypeHead:string;
    CurrPaperType,//var CurrPaperType:integer;
    CurrPaperNum,//var CurrPaperNum:string
    sRunSQLAfterAdd,
    PowerType //2010.9.15 add for QU Foster-20100913-1
    );

  //2011.6.15 add for NH Bill-20110609-2
  prcShowOverPQnty(false);

  TfrmShowDLLForm(commParent.tmpParent.Owner).sExecSQL
    :='exec FMEdPassAutoVoidPaperDLL '+''''+CurrPaperNum+''''+','+''''+sUserId+'''';

  //----------2017.06.14 add for MUT
  btnEquipString.Visible:=false;
  edtEquipString.Visible:=false;
  btnWorkUserString.Visible:=false;
  edtWorkUserString.Visible:=false;
  //----------

  //2020.05.19
  edtLotNum.SetFocus;

  //2021.11.10
  //wwDBGrid1.ReadOnly:=false; 先補上WPnl單報功能，有需要時再打開

  //2021.11.11
  //pnlXOut.Enabled:=false; 先補上WPnl單報功能，有需要時再打開

  //2021.11.12
  //label16.Visible:=false; 先補上WPnl單報功能，有需要時再打開

  //2021.12.08
  btnProcParamImport.Font.Color:=clWindowText;
  //2022.11.07
  btnShowAttachment.Font.Color:=clWindowText;

  //2021.10.14 TCI客製為後段報廢拋轉其他入庫部分，故開放顯示
  {if CusId<>'TCI' then
  begin
      Label1.Visible:=false;
      DBEdit4.Visible:=false;
  end;}
end;

//2012.12.19 add for NH Bill-20121206-03
procedure TfrmFMEdPassPCB.qryFMEdLotXOutDivBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  abort;
end;

//2012.12.19 add for NH Bill-20121206-03
procedure TfrmFMEdPassPCB.qryFMEdLotXOutDivBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  abort;
end;

procedure TfrmFMEdPassPCB.qryProcParamDtlBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  abort;
end;

procedure TfrmFMEdPassPCB.qryProcParamDtlBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  abort;
end;


procedure TfrmFMEdPassPCB.qryRwkSDefectBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  if (RwkStatus=1)
    and (qryRwkSDefect.FieldByName('PaperNum').Value <> tblDetail.FieldByName('PaperNum').Value) then
     abort;
end;

procedure TfrmFMEdPassPCB.qryRwkSDefectBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  //if RwkStatus=1 then  abort;

end;

procedure TfrmFMEdPassPCB.qryRwkSDefectNewRecord(DataSet: TDataSet);
var iSerialNum:integer;
    CusId:string;
begin
  inherited;
  iSerialNum:=0;

  unit_DLL.OpenSQLDLL(qryExec,'OPEN',
    'select maxSerialNum=max(SerialNum) from FMEdPassRwkSDefect(nolock) '+
    'where PaperNum='+''''+CurrPaperNum+''''+' and Item='+
    tblDetail.FieldByName('Item').AsString+
    ' and LotNum='+''''+tblDetail.FieldByName('LotNum').AsString+''''
    );

  if qryExec.RecordCount>0 then iSerialNum:=qryExec.Fields[0].AsInteger;

  qryRwksDefect.FieldByName('SerialNum').AsInteger:=iSerialNum+1;
  qryRwksDefect.FieldByName('PaperNum').AsString:=tblDetail.FieldByName('PaperNum').AsString;
  qryRwksDefect.FieldByName('Item').AsInteger:=tblDetail.FieldByName('Item').AsInteger;
  qryRwksDefect.FieldByName('LotNum').AsString:=tblDetail.FieldByName('LotNum').AsString;

  qryRwksDefect.FieldByName('RwkProc').ReadOnly:=false;
  qryRwksDefect.FieldByName('RwkProc').AsString:=tblDetail.FieldByName('ProcCode').AsString;
  qryRwksDefect.FieldByName('RwkProc').ReadOnly:=True;

  with qryExec do
  begin

      close;
      SQL.Clear;
      SQL.Add('select RouteSerial from FMEdProc where LotNum='+''''+tblDetail.FieldByName('LotNum').AsString+'''');
      open;

  end;

  if qryExec.RecordCount>0 then
    qryRwksDefect.FieldByName('RouteSerial').AsInteger:=qryExec.FieldByName('RouteSerial').AsInteger;


  //2021.04.26 TCI重工缺點明細新增「原始數量」，要自動計算
  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Text:='exec FMEdPassRwkSNext '+
    ''''+tblDetail.FieldByName('PaperNum').AsString+''''+','+
    tblDetail.FieldByName('Item').AsString+','+IntToStr(iSerialNum+1);
  qryExec.Open;

  if qryExec.RecordCount>0 then
  begin
    qryRwksDefect.FieldByName('QntyOri').ReadOnly:=false;
    qryRwksDefect.FieldByName('QntyOri').AsInteger:=qryExec.FieldByName('QntyOri').AsInteger;
    qryRwksDefect.FieldByName('QntyOri').ReadOnly:=true;
  end;

  qryExec.Close;



end;

procedure TfrmFMEdPassPCB.qryXOutDefectBeforeInsert(DataSet: TDataSet);
begin
  inherited;

  if tblDetail.Active=false then abort;

  if tblDetail.State in[dsInsert,dsEdit] then tblDetail.Post;

  if tblDetail.RecordCount=0 then abort;

  if tblLotXOut.Active then
     if tblLotXOut.State in[dsEdit] then tblLotXOut.Post;
end;

procedure TfrmFMEdPassPCB.qryXOutDefectClassIdValidate(Sender: TField);
begin
  inherited;
  //2021.07.05 TCI 取消
  {if qryClassId.Active then
    if qryClassId.RecordCount>0 then
      if qryClassId.Locate('ClassId',Sender.Value,[loCaseInsensitive]) then
        begin
          qryXOutDefect.FieldByName('DutyProc').Value
            :=qryClassId.FieldByName('Notes').Value;
        end;}

  //2021.10.25
  if iDutyProc<>1 then exit;

  if qryClassId.Active then
    if qryClassId.RecordCount>0 then
      if qryClassId.Locate('ClassId',Sender.Value,[loCaseInsensitive]) then
      begin
        qryXOutDefect.FieldByName('DutyProc').Value
        :=qryClassId.FieldByName('ResProc').Value;
      end;


end;

procedure TfrmFMEdPassPCB.qryXOutDefectDefectIdValidate(Sender: TField);
begin
  inherited;
  //2021.10.25
  if iDutyProc<>2 then exit;

  //2021.07.05 TCI 不看責任單位 看責任製程
  if qryDefectId.Active then
    if qryDefectId.RecordCount>0 then
      if qryDefectId.Locate('DefectId',Sender.Value,[loCaseInsensitive]) then
      begin
        qryXOutDefect.FieldByName('DutyProc').Value
        :=qryDefectId.FieldByName('ResProc').Value;
      end;


end;

procedure TfrmFMEdPassPCB.qryXOutDefectNewRecord(DataSet: TDataSet);
var iSerialNum:integer;
    CusId:string;
begin
  inherited;
  iSerialNum:=0;

  unit_DLL.OpenSQLDLL(qryExec,'OPEN',
    'select maxSerialNum=max(SerialNum) from FMEdPassXOutDefect(nolock) '+
    'where PaperNum='+''''+CurrPaperNum+''''+' and Item='+
    tblDetail.FieldByName('Item').AsString
    );

  if qryExec.RecordCount>0 then iSerialNum:=qryExec.Fields[0].AsInteger;

  qryXOutDefect.FieldByName('SerialNum').AsInteger:=iSerialNum+1;

  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Text:='exec FMEdPassXOutNext '+
    ''''+tblDetail.FieldByName('PaperNum').AsString+''''+','+
    tblDetail.FieldByName('Item').AsString+','+IntToStr(iSerialNum+1);
  qryExec.Open;

  if qryExec.RecordCount>0 then
  begin
    qryXOutDefect.FieldByName('QntyOri').ReadOnly:=false;
    qryXOutDefect.FieldByName('QntyOri').AsInteger:=qryExec.FieldByName('QntyOri').AsInteger;
    qryXOutDefect.FieldByName('QntyOri').ReadOnly:=true;
  end;

  qryExec.Close;

end;

//2011.6.15 add for NH Bill-20110609-2
procedure TfrmFMEdPassPCB.prcShowOverPQnty(bShow:boolean);
begin
  if bShow then
    begin
      lab_PQnty.Caption:='過帳數量';

      if tblDetail.Active then
        if tblDetail.RecordCount>0 then
           lab_PQnty.Caption:='過帳數量'+trim(tblDetail.FieldByName('StockId').AsString);

      lab_TranPQnty.Visible:=true;
      edtTranPQnty.Visible:=true;

      lab_TranPQnty.Caption:='過帳數量'+trim(sDefStkIdOverProd);

      lab_PQnty.Font.Color:=clRed;
      lab_TranPQnty.Font.Color:=clRed;

      //----------2011.11.14 add for NH Bill-20111104-99
      lab_StkVoidQnty.Visible:=true;
      edtStkVoidPQnty.Visible:=true;
      //----------

      tbsht_PassPCB_XOutNH.TabVisible:=true;//2012.12.19 add for NH Bill-20121206-03
      prcFMEdLotXOutDivOpen(tblDetail.FieldByName('PaperNum').AsString);//2012.12.19 add for NH Bill-20121206-03
    end;

  if (bShow=false) or (bPassUseTranPQnty=false) then
    begin
      lab_PQnty.Caption:='過帳良品數'; //2021.02.09 過帳數量 改 過帳良品數

      lab_TranPQnty.Visible:=false;
      edtTranPQnty.Visible:=false;

      lab_PQnty.Font.Color:=clWindowText;
      lab_TranPQnty.Font.Color:=clWindowText;
      lab_PQnty.Font.Color:=clBlue;  //2022.08.04 顏色改藍色

      //----------2011.11.14 add for NH Bill-20111104-99
      lab_StkVoidQnty.Visible:=false;
      edtStkVoidPQnty.Visible:=false;
      //----------

      tbsht_PassPCB_XOutNH.TabVisible:=false;//2012.12.19 add for NH Bill-20121206-03
      if qryFMEdLotXOutDiv.Active then qryFMEdLotXOutDiv.Close;//2012.12.19 add for NH Bill-20121206-03
    end;
end;

procedure TfrmFMEdPassPCB.btnGetParamsClick(Sender: TObject);
var sMinPOP:string;
    //2020.12.24
    i: integer;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
    //2022.04.25
    sDlgMode, sFocusMode:string;
begin
  inherited;

  //pnl_Center.Top:=0;
  //pnl_Center.Left:=floor(self.Width / 3);

  //2021.10.25
  with qryExec do
  begin
    qryExec.Close;
    SQL.Clear;
    SQL.Add('select Value from CURdSysParams where SystemId='+''''+'FQC'+''''+'and ParamId='+''''+'DutyProcType_tU'+'''');
    open;
  end;

  iDutyProc:=2;//2021.10.25 責任製程預帶預設小站
  iDutyProc:=qryExec.FieldByName('Value').AsInteger;

  //2021.11.10  先補上WPnl單報功能，有需要時再打開
  {tblLotXout.FieldByName('OrgQnty').DisplayLabel:='原始累計小片數';
  tblLotXout.FieldByName('Qnty').DisplayLabel:='至本站累計小片數'; }

  sRealTableNameMas1:='FMEdPassMain';
  sSelectSQLMas1:='select t0.* from FMEdPassMain t0 where 1=1 ';

  qryProcBasic.Open;
  qryEquipId.Open;
  qryProdLayer.Open;
  qryPOP.Open;
  qryLotStatus.Open;
  qryPOType.Open;
  qryClassId.Open;//2010.8.2 add for YX Bill-20100716-Xout

  qryDefectId.Open;//2010.8.2 add for YX Bill-20100716-Xout
  qryProcCode.Open;//2021.05.04 重工提出站

  qryUsers.Open;//2017.06.23 add for CMT

  unit_DLL.OpenSQLDLL(qryExec,'EXEC','exec FQCdProcDepartMergeChk');

  qryProcDepartMerge.Open;

  //2021.11.09
  if iDutyProc=1 then
  begin
      qryProcDepartMerge.Close;
      qryProcDepartMerge.SQL.Clear;
      qryProcDepartMerge.SQL.Add('select DepartId=BProcCode, DepartName=BProcName from FQCdProcInfo ');
      qryProcDepartMerge.Open;
  end;


  //if funSysParamsGet('NoDefPrintPass')='1' then
  //    chkPaperPrint.Checked:=false
  //    else chkPaperPrint.Checked:=true;

  //2008.8.21 add for SK 08081501
  pnl_EquipId.Visible:=
    funSysParamsGet('PassUseEquip')='1' ;

  sRunSQLAfterAdd:=unit_DLL.funPaperGetRunSQLAfterAdd(qryExec,sItemId);

  TfrmShowDLLForm(commParent.tmpParent.Owner).sConnStr:=qryExec.ConnectionString;

  bWIPInStk2AssInStk:=funSysParamsGet('WIPInStk2AssInStk')='1';

  sMinPOP:=funSysParamsGet('MinPOP');//2010.7.8 add

  bInnerAllPass:=funSysParamsGet('InnerAllPass'); //= '1';;//2013.01.18 add for 提高效能

  //2010.7.8 add
  if trim(sMinPOP)='' then iMinPOP:=5
    else iMinPOP:=strtoint(trim(sMinPOP));

  chkMOPrint.Visible:=funSysParamsGet('PassShowMOPrint')='1';

  bUseXoutYX:=funSysParamsGet('UseXoutYX')='1';
  bUseMergeYX:=funSysParamsGet('UseMergeYX')='1';

  pnl_XOutDefect.Visible:=bUseXoutYX;
  splitter_XOutDefect.Visible:=bUseXoutYX;//2012.09.19 add

  lab_ScrapStrXOutQnty.Visible:=bUseXoutYX;
  edt_ScrapStrXOutQnty.Visible:=bUseXoutYX;

  sPassSelectDlgMode:=funSysParamsGet('PassSelectDlgMode');//2011.2.11 add

  //----------2011.6.15 add for NH Bill-20110609-2
    bPassUseTranPQnty:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','PassUseTranPQnty')='1';

    if bPassUseTranPQnty then
      begin
        sDefStkIdOverProd:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','DefStkIdOverProd');
      end;
  //----------

  //----------2011.12.13 add for MUT Bill-20111207-06
  edtTranMatQnty.Visible:=unit_DLL.funDLLSysParamsGet(qryExec,'SBP','MatLayerUseTranMatQnty')='1';
  lab_TranMatQnty.Visible:=edtTranMatQnty.Visible;
  //----------

  //----------2012.1.2 add
  bUseLineId:=unit_DLL.funDLLSysParamsGet(qryExec,'SBP','UseLineId')='1';
  lab_LineId.Visible:=bUseLineId;
  edt_LineId.Visible:=bUseLineId;
  //----------

  //----------2012.3.21 add for NH Bill-20120314-02
  bPassUseMasterNotes:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','PassUseMasterNotes')='1';
  lab_MasterNotes.Visible:=bPassUseMasterNotes;
  edt_MasterNotes.Visible:=bPassUseMasterNotes;
  //----------

  //----------2012.08.27 add for NH Bill-20120704-05
  bPassUseNHParam:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','PassUseNHParam')='1';

  bCmpValueAftPass:=false;//必須先設false
  tbsht_PassPCB_Param.TabVisible:=false;
  if bPassUseNHParam=false then
     begin
       tbsht_PassPCB_Param.TabVisible:=false;
       btnProcParamImport.Visible:=false;
     end
  else
    begin
      page_PassPCB_120827A.ActivePageIndex:=0;
      bCmpValueAftPass:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','CmpValueAftPass')='1';
    end;
  //----------

  //----------2012.09.25 add for CMT Bill-20120906-01
  bPassSQntyBack2PQnty:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','PassSQntyBack2PQnty')='1';

  bSQntyChange:=false;//2012.09.25 add for CMT Bill-20120906-01
  //----------

  //2012.12.13 add for NH Bill-20121206-01
  bMergDivAutoPass:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','MergDivAutoPass')='1';

  //2013.08.22 add for NIS Bill-20130815-03
  btnShowAttachment.Visible
    :=(unit_DLL.funDLLSysParamsGet(qryExec,'FME','PCBPassShowAttachment')='1');

  //----------2017.06.23 add  for CMT
  sSinglePassShowQCUser:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','SinglePassShowQCUser');

  if sSinglePassShowQCUser='1' then
    begin
      lab_QC_UserId.Visible:=true;
      cbo_QC_UserId.Visible:=true;
    end;
  //----------

  //----------2017.07.24 add for MUT
  bPass2DCodeReadOnly:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','Pass2DCodeReadOnly')='1';

  if bPass2DCodeReadOnly then
     begin
       edtEquipString.ReadOnly:=true;
       edtWorkUserString.ReadOnly:=true;
     end;
  //----------

  unit_DLL.OpenSQLDLL(qryExec,'OPEN',
    'select Value from CURdSysParams where SystemId=''EMO'' and ParamId=''CusId'''
    );

  CusId:=qryExec.Fields[0].AsString;

  prcInitial;

  //2020.12.24
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sList:=TstringList.Create;
      sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sList.Strings[0];
      sList.Free;
      FontSize := StrToInt(sFontSize);
      //2021.04.01
      if FontSize<>100 then
        self.ScaleBy(120, FontSize);
  end;

  //2022.04.25
  iDlgMode:=0;
  iFocusMode:=0;
  sDlgMode:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','PassDlgMode');
  if sDlgMode='1' then
    iDlgMode:=1;
  sFocusMode:=unit_DLL.funDLLSysParamsGet(qryExec,'FME','PassFocusMode');
  if sFocusMode='1' then
    iFocusMode:=1;
  //2022.11.07
  //if iFocusMode=1 then
  //begin
    lblMemoNotes.Visible:=True;
    memoNotes.Visible:=True;
  //end;
end;

//2012.09.12 add for NH Bill-20120704-05
function TfrmFMEdPassPCB.funProcParamDtlCheck():boolean;
begin
  result:=false;

  if tblMaster.Active=false then
    begin
      MsgDlgJS('過帳單主檔尚未開啟', mtInformation, [mbok], 0);
      exit;
    end;

  if tblMaster.RecordCount=0 then
    begin
      MsgDlgJS('過帳單主檔沒有資料', mtInformation, [mbok], 0);
      exit;
    end;

  if tblDetail.Active=false then
    begin
      MsgDlgJS('過帳單明細檔尚未開啟', mtInformation, [mbok], 0);
      exit;
    end;

  if tblDetail.RecordCount=0 then
    begin
      MsgDlgJS('過帳單明細檔沒有資料', mtInformation, [mbok], 0);
      exit;
    end;

  result:=true;
end;

//2012.12.19 add for NH Bill-20121206-03
procedure TfrmFMEdPassPCB.prcFMEdLotXOutDivOpen(sPaperNum:string);
begin
  qryFMEdLotXOutDiv.Close;

  if qryFMEdLotXOutDiv.SQL.Text='' then
    begin
      unit_DLL.OpenSQLDLL(qryExec,'OPEN',
        'exec CURdOCXSQLPreView '+''''+'FMEdLotXOutDiv'+'''');

      qryFMEdLotXOutDiv.SQL.Add(qryExec.Fields[0].AsString+' and t0.PassNum = :PassNum');
    end;

  qryFMEdLotXOutDiv.Parameters.ParamByName('PassNum').Value:=sPaperNum;

  qryFMEdLotXOutDiv.Open;
end;

//2012.09.12 add for NH Bill-20120704-05
procedure TfrmFMEdPassPCB.prcProcParamDtlOpen(sPaperNum:string);
begin
  qryProcParamDtl.Close;

  if qryProcParamDtl.SQL.Text='' then
    begin
      unit_DLL.OpenSQLDLL(qryExec,'OPEN',
        'exec CURdOCXSQLPreView '+''''+'FMEdPassProcParamDtl'+'''');

      qryProcParamDtl.SQL.Add(qryExec.Fields[0].AsString+' and t0.PaperNum = :PaperNum');
    end;

  {if edt_ParamIssueNum.DataField='' then edt_ParamIssueNum.DataField:='IssueNum';
  if edt_ParamPONum.DataField='' then edt_ParamPONum.DataField:='PONum';
  if edt_ParamPOItem.DataField='' then edt_ParamPOItem.DataField:='POItem';
  if edt_ParamQuotaNum.DataField='' then edt_ParamQuotaNum.DataField:='QuotaNum';}

  qryProcParamDtl.Parameters.ParamByName('PaperNum').Value:=sPaperNum;

  qryProcParamDtl.Open;
end;

//2012.09.12 add for NH Bill-20120704-05
procedure TfrmFMEdPassPCB.prcProcParamDtlDo(sKind:string);
var sPaperNum:string;sSQL:string;
begin
  if funProcParamDtlCheck=false then exit;

  if tblMaster.State in[dsInsert,dsEdit] then tblMaster.Post;

  if tblDetail.State in[dsInsert,dsEdit] then tblDetail.Post;

  sPaperNum:=tblDetail.FieldByName('PaperNum').AsString;

  if sPaperNum='' then
    begin
      MsgDlgJS('過帳單主檔沒有單號，無法作業', mtInformation, [mbok], 0);
      exit;
    end;

  if sKind='IMPORT' then
    sSQL:='exec FMEdPassImportParam '+''''+sPaperNum+''''+','+''''+sUserId+''''
  else if sKind='CLEAR' then
    sSQL:='exec FMEdPassClearParam '+''''+sPaperNum+''''
  else exit;

  if qryProcParamDtl.Active then
    if qryProcParamDtl.State in[dsInsert,dsEdit] then qryProcParamDtl.Cancel;

  unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);

  prcProcParamDtlOpen(sPaperNum);

  {if sKind='IMPORT' then
    if page_PassPCB_120827A.ActivePageIndex<>1 then
      page_PassPCB_120827A.ActivePageIndex:=1;}
end;

//2012.09.12 add for NH Bill-20120704-05
procedure TfrmFMEdPassPCB.btnParamProcClearClick(Sender: TObject);
begin
  prcProcParamDtlDo('CLEAR');
end;

//2012.09.12 add for NH Bill-20120704-05
procedure TfrmFMEdPassPCB.btnProcParamImportClick(Sender: TObject);
begin
  prcProcParamDtlDo('IMPORT');
end;

//2013.11.08 add for NIS James-20131108-01
function TfrmFMEdPassPCB.funCheckAttachment:boolean;
var
  sSQL
  ,sPartNum
  ,sRevision
  ,sLayerId
  ,sProcCode
      :string;
  sNotesPath
      :widestring;
begin
  result:=false;

  sPartNum  :=edtPartNum.Text;
  sRevision :=edtRevision.Text;
  sLayerId  :=edtLayerId.Text;
  sProcCode :=edtProcCode.Text;

  if sPartNum='' then
    begin
      exit;
    end;

  if sRevision='' then
    begin
      exit;
    end;

  if sLayerId='' then
    begin
      exit;
    end;

  if sProcCode='' then
    begin
      exit;
    end;

  sSQL:='exec FMEdPassGetAttachment '+
    ''''+sPartNum+''''+','+
    ''''+sRevision+''''+','+
    ''''+sLayerId+''''+','+
    ''''+sProcCode+''''
    ;

  unit_DLL.OpenSQLDLL(qryExec,'OPEN',sSQL);

  if qryExec.RecordCount=0 then
    begin
      qryExec.Close;
      exit;
    end;

  sNotesPath:=qryExec.Fields[0].AsWideString;

  qryExec.Close;

  if sNotesPath='' then
    begin
      exit;
    end;

  if not FileExists(sNotesPath) then
     begin
       exit;
     end;

  result:=true;
end;

//2013.09.04 add for NIS Bill-20130815-03
procedure TfrmFMEdPassPCB.btnShowAttachmentClick(Sender: TObject);
var
  sSQL
  ,sPartNum
  ,sRevision
  ,sLayerId
  ,sProcCode
      :string;
  sNotesPath
      :widestring;
begin
  sPartNum  :=edtPartNum.Text;
  sRevision :=edtRevision.Text;
  sLayerId  :=edtLayerId.Text;
  sProcCode :=edtProcCode.Text;

  if sPartNum='' then
    begin
      MsgDlgJS('沒有「料號」，無法顯示附件',mtError,[mbOk],0);
      exit;
    end;

  if sRevision='' then
    begin
      MsgDlgJS('沒有「版序」，無法顯示附件',mtError,[mbOk],0);
      exit;
    end;

  if sLayerId='' then
    begin
      MsgDlgJS('沒有「階段」，無法顯示附件',mtError,[mbOk],0);
      exit;
    end;

  if sProcCode='' then
    begin
      MsgDlgJS('沒有「目前製程」，無法顯示附件',mtError,[mbOk],0);
      exit;
    end;

  sSQL:='exec FMEdPassGetAttachment '+
    ''''+sPartNum+''''+','+
    ''''+sRevision+''''+','+
    ''''+sLayerId+''''+','+
    ''''+sProcCode+''''
    ;

  unit_DLL.OpenSQLDLL(qryExec,'OPEN',sSQL);

  if qryExec.RecordCount=0 then
    begin
      qryExec.Close;
      MsgDlgJS('沒有途程資料，無法顯示附件',mtError,[mbOk],0);
      exit;
    end;

  sNotesPath:=qryExec.Fields[0].AsWideString;

  qryExec.Close;

  if sNotesPath='' then
    begin
      MsgDlgJS('工程系統途程檔裡沒有附件，無法顯示',mtError,[mbOk],0);
      exit;
    end;

  if not FileExists(sNotesPath) then
     begin
       MsgDlgJS('附件不存在於工程系統途程檔所指定的目錄中['+sNotesPath+']', mtError, [mbOk], 0);
       exit;
     end;

  ShellExecute(Handle,'OPEN',pchar(sNotesPath),nil,nil,SW_SHOW);
end;

procedure TfrmFMEdPassPCB.btnTmp2XoutClick(Sender: TObject);
begin
  inherited;

  if tblDetail.Active=false then exit;

  //if tblDetail.State in[dsEdit] then tblDetail.Post;

  if tblDetail.RecordCount=0 then exit;

  //if tblLotXOut.State in[dsEdit] then tblLotXOut.Post;

  if tblLotXOut.RecordCount=0 then exit;

  with qryTmp2XOutNewAdd do
    begin
      if active then close;
      Parameters.ParamByName('PaperNum').Value:=CurrPaperNum;
      Parameters.ParamByName('LotNum').Value
        :=tblDetail.FieldByName('LotNum').Value;
      ExecSQL;
      close;
    end;

  tblDetail.Refresh;
end;

//2017.06.09 add for MUT
procedure TfrmFMEdPassPCB.btnWorkUserStringClick(Sender: TObject);
var sSelectedWorker:string; i:integer;
begin
  inherited;

  if tblDetail.Active=false then exit;

  if tblDetail.RecordCount=0 then exit;

  Application.CreateForm(TfrmWorkerSelectMUT,frmWorkerSelectMUT);

  frmWorkerSelectMUT.sProcCode:=tblDetail.FieldByName('ProcCode').AsString;

  frmWorkerSelectMUT.qryDepartId.Close;
  frmWorkerSelectMUT.qryDepartId.ConnectionString:=sConnectStr;
  frmWorkerSelectMUT.qryDepartId.open;

  frmWorkerSelectMUT.qryWorkerMUT.Close;
  frmWorkerSelectMUT.qryWorkerMUT.ConnectionString:=sConnectStr;

  sSelectedWorker:='';

  frmWorkerSelectMUT.ShowModal;

  if frmWorkerSelectMUT.ModalResult=mrok then
    begin
      if frmWorkerSelectMUT.msDataSelected.TargetItems.Count>0 then
         begin
           for i := 0 to frmWorkerSelectMUT.msDataSelected.TargetItems.Count - 1 do
              begin
                if sSelectedWorker<>'' then sSelectedWorker:=sSelectedWorker+',';

                sSelectedWorker:=sSelectedWorker +
                  frmWorkerSelectMUT.msDataSelected.TargetItems.Item[i].Caption;
              end;
         end;

     if tblDetail.Active=true then
       begin
         if tblDetail.RecordCount>0 then
           begin
             if not(tblDetail.State in[dsEdit]) then  tblDetail.Edit;

             tblDetail.FieldByName('WorkUserString').Value :=sSelectedWorker;
           end;
       end;
    end;
end;

//2012.09.12 add for NH Bill-20120704-05
procedure TfrmFMEdPassPCB.edtLotNumClick(Sender: TObject);
begin
  inherited;
  TCustomEdit(Sender).SelectAll;
end;

//procedure TfrmFMEdPassPCB.edtLotNumExit(Sender: TObject);
procedure TfrmFMEdPassPCB.prcImport(Sender: TObject);
var sSQL{, sErr}: String;
  iNowPOP:integer; bOnlyOne:boolean;//2010.7.7 modify for YX RA10070507-1
  //iLPCS,iLLPCS:integer;//2010.10.23 add for YX Bill-20101023-1
  iLPCS,iLLPCS:extended;//2012.12.13 modify for NH Bill-20121206-05
  bErr:boolean;//2012.10.25 add
//var RwkStatus:integer;//2021.02.01
  iRwkSType :integer;
  iChkMrgXout :integer;//2021.11.12
  iRwkQnty: integer;  //2022.09.15
  sProcCode, sqlProcCode: string;  //2022.12.20
begin

  inherited;
  bErr:=false;//2012.10.25 add

  eOrgPQnty:=0; //2010.10.20 add for YX Bill-20101012-2

  //2021.09.03 參數控管重工是否允許輸入報廢
  with qryExec do
  begin
      qryExec.close;
      SQL.clear;
      SQL.Add('select value from CURdSysParams '+
              'where SystemId='+''''+'FME'+''''+
              ' and ParamId='+''''+'ReworkScrapType'+'''');
      Open;
      iRwkSType:=0;
      if qryExec.RecordCount>0 then
      begin
          iRwkSType := qryExec.Fieldbyname('Value').AsInteger;
      end;
  end;

  if (CurrLotNum <> edtLotNum.Text) Or (tblDetail.Fieldbyname('PartNum').Asstring='') then
  //with qryPassSelectLot do
  //begin
    try
      if edtLotnum.text = '' then exit;

      CurrLotNum := edtLotNum.Text;

      qryPassSelectLot.close;
      qryPassSelectLot.Parameters.ParamByName('LotNum').Value:= edtLotnum.text;
      qryPassSelectLot.Parameters.ParamByName('InStock').Value:= PowerType;
      qryPassSelectLot.Parameters.ParamByName('PaperNum').Value:= tblMaster.Fieldbyname('PaperNum').Asstring;
      qryPassSelectLot.Parameters.ParamByName('UserId').Value:= sUserId;
      qryPassSelectLot.open;

    except
      //raise;
      //exit;
      on e:exception do //2012.10.25 modify
        begin
          bErr:=true;
          //MsgDlgJS(e.Message,mtError,[mbOk],0);
          unit_DLL.JSdMessageDlgDLL(e.Message);//2012.11.21 modify for MUT Bill-20121107-02
        end;
    end;//try

    //2012.10.25 modify
    if bErr=false then
      if (qryPassSelectLot.RecordCount = 0) then
        begin
          bErr:=true;
          MsgDlgJS('無此批號!!', mtError, [mbok], 0);
        end;

    //if (qryPassSelectLot.RecordCount = 0) then
    //begin
      //MsgDlgJS('無此批號!!', mtInformation, [mbok], 0);

    if bErr then
    begin
      //with tblDetail do
      //begin
        if tblDetail.state in [dsEdit, dsInsert] then tblDetail.Cancel;

        tblDetail.First;

        while not tblDetail.eof do
        begin
          tblDetail.Delete;
          //Next; //2008.8.27 disable for RD 因ADO會自動Next,若不disable會刪不乾淨
        end;//while not eof do

        //Append; //2010.8.3 disable,避免觸動 Trigger
        //fieldbyname('LotNum').asstring :=CurrLotNum; //2010.8.3 disable,避免觸動 Trigger
      //end;//with tblDetail do

      edtLotNum.SetFocus;
      Exit;
    end;//if (RecordCount = 0) then

    //2012.10.25 add for 防範異常
    if bErr then
      begin
        exit;
      end;

    //2022.12.21 峻新改為藍底粗紅字
    if CusId='CIR' then
     Begin
        DBEdit5.Color:=clSkyBlue;
        DBEdit3.Color:=clSkyBlue;
        DBEdit5.Font.Color:=clRed;
        DBEdit3.Font.Color:=clRed;
        DBEdit5.Font.Style := [fsBold];
        DBEdit3.Font.Style := [fsBold];
     End;

    lab_UQnty.Visible:=(qryPassSelectLot.Fieldbyname('IsStorage').asinteger = 1);
    edtUQnty.Visible:=(qryPassSelectLot.Fieldbyname('IsStorage').asinteger = 1);

    //內層一起過
    pnlPassMrg.Enabled:=false;
    sProcCode :=qryPassSelectLot.fieldbyname('ProcCode').asstring;  //2022.12.20

    if UpperCase(trim(qryPassSelectLot.Fieldbyname('LayerId').asstring))<>'L0~0' then
    begin
      //2022.12.20  拆分製程
      qryExec.Close;
      qryExec.SQL.Clear;
      qryExec.SQL.Add('exec FMEdInnerAllPass '+
        ''''+bInnerAllPass+''+''','''+''+sProcCode+'''');
      qryExec.Open;
      //sqlProcCode:=qryExec.FieldByName('ProcCode').Value;

      if qryExec.RecordCount>0 then
       Begin
         pnlPassMrg.Enabled:=true;
       End;

      qryExec.Close;

      //pnlPassMrg.Visible
      //pnlPassMrg.Enabled  2022.12.20
        //:=(funSysParamsGet('InnerAllPass') = '1');
        //:=bInnerAllPass;//2013.01.18 modify for 提高效能  2022.12.20
    end;

    //若是壓合製程
    if qryPassSelectLot.Fieldbyname('ProcType').asinteger = 1 then
    begin
       //pnlPassMrg.Visible
       pnlPassMrg.Enabled
        :=((qryPassSelectLot.Fieldbyname('ProcType').asinteger = 1)
                              and (qryPassSelectLot.Fieldbyname('MatchCount').asinteger > 1));
      //2020.04.16
      edtRevNum.Text := qryPassSelectLot.Fieldbyname('RevNum').asstring;
    end//2020.04.16;
    else
    begin
      edtRevNum.Text := '';
      //2021.11.12 先補上WPnl單報功能，有需要時再打開
      //label16.Visible:=false;
    end;

    btFind.Enabled:=pnlPassMrg.Enabled;

    pnlDateCode.Visible
      :=(qryPassSelectLot.Fieldbyname('IsDateCode').asinteger = 1);

    bIsCNC:=qryPassSelectLot.Fieldbyname('ProcType').asinteger = 2;//2009.2.4 add for EW RA09020402

    //2010.7.7 modify for YX RA10070507-1
    iNowPOP:=qryPassSelectLot.Fieldbyname('POP').Asinteger;

    //iLPCS:=qryPassSelectLot.fieldbyname('LPCS').AsInteger;
    //iLLPCS:=qryPassSelectLot.fieldbyname('LLPCS').AsInteger;

    //2012.12.13 modify for NH Bill-20121206-05
    iLPCS:=qryPassSelectLot.fieldbyname('LPCS').Asfloat;
    iLLPCS:=qryPassSelectLot.fieldbyname('LLPCS').Asfloat;

    //2010.7.7 modify for YX RA10070507-1
    bOnlyOne:=(iLLPCS=1)
              and
              ((iNowPOP=4) or
               (iNowPOP=3) and (iLPCS=1)
               ) //2010.10.23 add for YX Bill-20101023-1
              ;

    //2010.10.23 add for YX Bill-20101023-1
    if bOnlyOne=false then
       if (iNowPOP=3) and  (bIsCNC) and (iLLPCS=1) then bOnlyOne:=true;

    //pnlXOut.Visible :=(Fieldbyname('IsXOut').asinteger = 1);

    //pnlXOut.Visible
    pnlXOut.Enabled
      :=(iNowPOP<>iMinPOP) and
        {(bOnlyOne=false) and }
        ((qryPassSelectLot.Fieldbyname('IsXOut').asinteger=1)
          or (qryPassSelectLot.Fieldbyname('AftProcType').asinteger=990));//2010.7.7 modify for YX RA10070507-1
          //or (qryPassSelectLot.Fieldbyname('ProcType').asinteger = 1 ));//2021.11.11 壓合也顯示 //2021.11.12 移到下面判斷

    //2021.11.19  先補上WPnl單報功能，有需要時再打開
   {with qryExec do
    begin
        qryExec.Close;
        SQL.Clear;
        SQL.Add('select Value=ltrim(rtrim(Value)) from CURdSysParams '+
                'where SystemId='+''''+'FME'+''''+' '+
                ' and  ParamId='+''''+'WPnlXoutFSProc'+'''');
        Open;
    end;

    bWPnlFSProc:=false;
    bWPnlFSProc:=qryPassSelectLot.Fieldbyname('ProcCode').AsString=qryExec.Fieldbyname('Value').AsString;}

    //2021.11.10  先補上WPnl單報功能，有需要時再打開
    {//if bIsCNC then
    if bWPnlFSProc then//2021.11.19
    begin
        pnlXOut.Enabled:=true;
    end; }

    //pnlXOutSum.Visible := (Fieldbyname('AftProcType').asinteger = 990);
//2013.01.07 一律停用
{    pnlXOutSum.Visible
      :=(iNowPOP<>iMinPOP) and (bOnlyOne=false) and
      ((qryPassSelectLot.Fieldbyname('IsXOut').asinteger=1)
        or (qryPassSelectLot.Fieldbyname('AftProcType').asinteger=990));//2010.7.7 modify for YX RA10070507-1
 }
   //2012.12.19 add for NH Bill-20121206-03
   //NH必須一律使用多報檔,必須讓資料控制變得單純才能寫NH的需求
   //NH必須開啟入庫前一站的「允許多報數」
   //if bPassUseTranPQnty then
      //pnlXOutSum.Visible:=false; //2013.01.07一律停用

    edtSQnty.Enabled := Not (qryPassSelectLot.Fieldbyname('ProcType').asinteger = 11);

    //chkMOPrint.Enabled:=(Fieldbyname('ProcType').asinteger = 1);
    //chkMOPrint.Checked:=(Fieldbyname('ProcType').asinteger = 1);

    //2010.10.20 add for YX Bill-20101012-2
    eOrgPQnty:=qryPassSelectLot.fieldbyname('PQnty').Asfloat;

    bIsIssue:=qryPassSelectLot.Fieldbyname('ProcType').asinteger = 11;//2009.2.4 add for EW RA09020402



    //btnTmp2Xout.Visible:=pnlXOut.Enabled; //2010.8.31 disable 因發生遞迴

    //-----
  //end;

  //if tblDetail.State in[dsBrowse] then tblDetail.Append;

  //with tblDetail do
  if tblDetail.state in [dsEdit, dsInsert] then
  begin
     tblDetail.fieldbyname('PaperNum').asstring:=tblMaster.fieldbyname('PaperNum').asstring;

     //2012.10.24 add for 解決前端不正常操作所造成的異常
     tblDetail.fieldbyname('LotNum').asstring:=qryPassSelectLot.fieldbyname('LotNum').asstring;
     CurrLotNum:=qryPassSelectLot.fieldbyname('LotNum').asstring;

     tblDetail.fieldbyname('PartNum').asstring:=qryPassSelectLot.fieldbyname('PartNum').asstring;
     tblDetail.fieldbyname('Revision').asstring:=qryPassSelectLot.fieldbyname('Revision').asstring;
     tblDetail.fieldbyname('StockId').asstring:=qryPassSelectLot.fieldbyname('StockId').asstring;
     tblDetail.fieldbyname('ProcCode').asstring:=qryPassSelectLot.fieldbyname('ProcCode').asstring;
     tblDetail.fieldbyname('LayerId').asstring:=qryPassSelectLot.fieldbyname('LayerId').asstring;
     tblDetail.fieldbyname('POP').Asinteger:=qryPassSelectLot.fieldbyname('POP').Asinteger;
     tblDetail.fieldbyname('Qnty').Asfloat:=qryPassSelectLot.fieldbyname('Qnty').Asfloat;

     if qryPassSelectLot.fieldbyname('IsShowPQnty').Asinteger=1 then
        tblDetail.fieldbyname('PQnty').Asfloat:= qryPassSelectLot.fieldbyname('PQnty').Asfloat
     else
        tblDetail.fieldbyname('PQnty').Asfloat:=0;

     //2009.2.4 add for EW RA09020402
     bIsShowPQnty:=qryPassSelectLot.fieldbyname('IsShowPQnty').Asinteger=1;

     tblDetail.FieldByName('TranPQnty').AsFloat:=0;//2011.6.15 add for NH Bill-20110609-2
     tblDetail.FieldByName('StkVoidPQnty').AsFloat:=0;//2011.11.14 add for NH Bill-20111104-99

     tblDetail.fieldbyname('SQnty').Asfloat:=qryPassSelectLot.fieldbyname('SQnty').Asfloat;//2020.09.11 帶入批號的報廢數
     tblDetail.fieldbyname('UQnty').Asfloat:=0;
     tblDetail.fieldbyname('XOutQnty').Asfloat:=0;
     tblDetail.fieldbyname('AftProc').asstring:=qryPassSelectLot.fieldbyname('AftProc').asstring;
     tblDetail.fieldbyname('AftLayer').asstring:=qryPassSelectLot.fieldbyname('AftLayer').asstring;
     tblDetail.fieldbyname('AftPOP').Asinteger:=qryPassSelectLot.fieldbyname('AftPOP').Asinteger;

     tblDetail.fieldbyname('DateCode').asstring //2008.04.14 add for EW
      :=qryPassSelectLot.fieldbyname('DateCode').asstring;

     //fieldbyname('Global_Id').asstring //2008.10.24 add for EW RA08100010 (08102302)
     // :=qryPassSelectLot.fieldbyname('Global_Id').asstring;

     //2011.12.13 add for MUT Bill-20111207-06
     tblDetail.fieldbyname('TranMatQnty').asfloat
      :=qryPassSelectLot.fieldbyname('TranMatQnty').asfloat;

     //2012.08.16 add for MUT Bill-20120801-02
     tblDetail.fieldbyname('Notes').Value
      :=qryPassSelectLot.fieldbyname('DtlNotes').Value;

     tblDetail.Post;

     //labLLPCS.Caption := inttostr(iLPCS)+'*'+
     //                    inttostr(iLLPCS);
      //2012.12.13 modify for NH Bill-20121206-05
     labLLPCS.Caption := floattostr(iLPCS)+'*'+
                         floattostr(iLLPCS);
  end;

  //-----2010.8.2 add for YX Bill-20100716-Xout
    //pnl_XOutDefect.Visible
    pnl_XOutDefect.Enabled
      :=((pnlXOut.Enabled) and //((pnlXOut.Visible) and
       (qryPassSelectLot.Fieldbyname('XOutNeedDefect').asinteger=1));
    //2019.09.05
    pnl_XOutDefect.Visible
      :=((pnlXOut.Enabled) and //((pnlXOut.Visible) and
       (qryPassSelectLot.Fieldbyname('XOutNeedDefect').asinteger=1));
    //2021.04.01 字體放大 Fix
    if pnl_XOutDefect.Visible then
      pnl_XOutDefect.Height := Round(Panel4.Height / 2);

    qryExec.Close;
    qryExec.SQL.Clear;
    qryExec.SQL.Add('exec FMEdBackFromRwk '''
          + tblMaster.fieldbyname('PaperNum').asstring+'''');
    qryExec.Open;

    RwkStatus:=0;
    RwkStatus:=qryExec.FieldByName('RwkStatus').Value;

    //if pnl_XOutDefect.Visible then
    //2021.04.26 重工缺點明細和一般的要分開
    if (pnl_XOutDefect.Enabled)
    and (qryPassSelectLot.Fieldbyname('LotStatus').asinteger<>1)
    and (RwkStatus=0) then
    begin

      if gridXOutDefect.Visible=false then
       begin
           gridXOutDefect.Visible:=true;
           gridRwkSDefect.Visible:=false;
           gridXOutDefect.Align:=alNone;
           gridXOutDefect.Align:=alClient;
           DBNavigator1.DataSource:=dsXOutDefect;
           Panel1.Caption:='單報缺點明細';
       end;

      if qryXOutDefect.Active=false then
      begin

          qryXOutDefect.Open;

      end;

    end;

    //2022.12.21 清除Item>1
    with tblDetail do
      begin
           First;

           Next;

           while not eof do
           begin
             if FieldByName('Item').AsInteger <> 1 then

             Delete;

             if tblDetail.RecordCount = 1 then
               break;
           end;
      end;

  //2016.07.22 add for 公司內部會議
  lab_GoodPCS.Caption:='良品PCS數： '+qryPassSelectLot.fieldbyname('GoodPCS').AsString;

  //if pnlPassMrg.Visible then
  if pnlPassMrg.Enabled then
  begin
    sSQL:= '';
    sSQL:= 'exec FMEdPassMrgInsert '''
          + tblMaster.fieldbyname('PaperNum').asstring+'''';

    {sErr:= '';
    sErr:= SQLExecute(sSQL);
    if sErr<>'' then
    begin
      frmMain.meuSystem.JSdMessageDlg(sErr);
      Exit;
    end;}
    unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);

    //2021.11.11   先補上WPnl單報功能，有需要時再打開
    {sSQL:= '';
    sSQL:= 'exec FMEdPassMrgXOutIns '''
          + tblMaster.fieldbyname('PaperNum').asstring+'''';

    unit_DLL.OpenSQLDLL(qryExec,'EXEC',sSQL);

    //2021.11.12 前站有輸入單報在顯示
    sSQL:= '';
    sSQL:= 'exec FMEdPassChkMrgXout '''
          + tblMaster.fieldbyname('PaperNum').asstring+'''';

    unit_DLL.OpenSQLDLL(qryExec,'OPEN',sSQL);

    iChkMrgXOut:=0;
    iChkMrgXOut:=qryExec.FieldByName('iChk').AsInteger;

    if ((iChkMrgXOut=1) or (pnlXOut.Enabled)) and (qryPassSelectLot.Fieldbyname('ProcType').asinteger = 1) then
    begin
       pnlXOut.Enabled:=true;
       label16.Visible:=true;
       tblLotXOut.Close;
       tblLotXOut.Open;
    end;  }

    tblDetail.Close;
    tblDetail.Parameters.ParamByName('PaperNum').Value:=tblMaster.fieldbyname('PaperNum').asstring;
    tblDetail.Open;
  end
  else
  begin
    if edtPQnty.Visible=true then//2021.02.26  元件關了Focus會報錯
      edtPQnty.SetFocus;

    if edtRwkPQnty.Visible=true then//2021.02.26 重工過帳數 Focus
      edtRwkPQnty.SetFocus;
  end;
  //if (pnlXOut.Visible = True) then

  if pnlXOut.Enabled then
  begin
    //ShowXOut; //2010.8.3 改到 Trigger
    tblLotXOut.Close;

    tblLotXOut.Open;

    //2021.11.10  先補上WPnl單報功能，有需要時再打開
    {if bIsCNC then
    if bWPnlFSProc then//2021.11.19
    begin
      if qryPassSelectLot.Fieldbyname('IsXOut').asinteger<>1 then
        wwDBGrid1.ReadOnly:=true;
    end;}

  end;

  if bUseMergeYX=false then
    begin
      pnlPassMrg.Visible:=pnlPassMrg.Enabled;
    end;

  prcShowFQC_XOutTOL;//2011.5.29 add for YX Bill-20110517-2

  //2011.6.15 add for NH Bill-20110609-2
  if (bPassUseTranPQnty) and (qryPassSelectLot.Fieldbyname('AftProcType').asinteger=990) then
    begin
      prcShowOverPQnty(true);
    end
    else
    begin
      prcShowOverPQnty(false);
    end;

  //2013.11.08 add for NIS James-20131108-01
  if btnShowAttachment.Visible then
     btnShowAttachment.Enabled:=funCheckAttachment;
  //2022.11.07
  btnShowAttachment.Font.Color:=clWindowText;
  if btnShowAttachment.Enabled then
    btnShowAttachment.Font.Color:=clRed;

  //2017.06.09 add for MUT
  if qryPassSelectLot.Fieldbyname('iNeedMUT_Equip').asinteger=1 then
    begin
      btnEquipString.Visible:=true;
      btnWorkUserString.Visible:=true;
      edtEquipString.Visible:=true;
      edtWorkUserString.Visible:=true;
    end
    else
    begin
      btnEquipString.Visible:=false;
      btnWorkUserString.Visible:=false;
      edtEquipString.Visible:=false;
      edtWorkUserString.Visible:=false;
    end;

  //2022.09.15 當站重工數都先關閉
  Label8.Visible:=false;
  edtRwkQnty.Visible:=false;

  //2020.11.30 add  2022.09.15 cancel 改由設定控制
  //Label8.Visible:=(qryPassSelectLot.Fieldbyname('LotStatus').asinteger <> 1);
  //edtRwkQnty.Visible:=(qryPassSelectLot.Fieldbyname('LotStatus').asinteger <> 1);

  //2021.04.21 防範沒按確認直接匯入沒關閉
  //累積重工報廢數
  lab_RwkSQntySum.Visible:=false;
  edtRwkSQntySum.Visible:=false;
  //報廢數
  lab_SQnty.Visible:=true;
  edtSQnty.Visible:=true;
  //重工報廢數
  lab_RwkSQnty.Visible:=false;
  edtRwkSQnty.Visible:=false;
  //過帳數
  lab_PQnty.Visible:=true;
  edtPQnty.Visible:=true;
  //重工過帳數
  lab_RwkPQnty.Visible:=false;
  edtRwkPQnty.Visible:=false;

  //2021.02.24 計算重工過帳數
  //2021.09.03 要搬到參數外，否則過帳數量防呆會有問題
  if (qryPassSelectLot.Fieldbyname('LotStatus').asinteger=1) then
  begin
      qryExec.Close;
      qryExec.SQL.Clear;
      qryExec.SQL.Add('exec FMEdCalRwkPQnty '''
            + tblMaster.fieldbyname('PaperNum').asstring+'''');
      qryExec.execSQL;

      tblDetail.Close;
      tblDetail.Parameters.ParamByName('PaperNum').Value:=tblMaster.fieldbyname('PaperNum').asstring;
      tblDetail.Open;
  end;

  if iRwkSType=1 then
  begin
      //2021.02.22 復原
      //2021.02.09 先取消
      //2021.01.29 重工批 才出現重工報廢數，隱藏報廢數
      if qryPassSelectLot.Fieldbyname('LotStatus').asinteger=1 then
      begin
            //報廢數
            lab_SQnty.Visible:=false;
            edtSQnty.Visible:=false;
            //重工報廢數
            lab_RwkSQnty.Visible:=true;
            edtRwkSQnty.Visible:=true;
            //過帳數
            lab_PQnty.Visible:=false;
            edtPQnty.Visible:=false;
            //重工過帳數
            lab_RwkPQnty.Visible:=true;
            edtRwkPQnty.Visible:=true;

            if edtRwkPQnty.Visible=true then//2021.02.26 重工過帳數 Focus
              edtRwkPQnty.SetFocus;
      end;

      //2021.02.01 允收X報數
      qryAllowXOutQnty.Close;
      qryAllowXOutQnty.Open;

      //2021.02.22 復原
      //2021.02.09 先取消
      //2021.02.01 累積重工報廢數
      //2021.02.02 回到發起站和重工站都顯示
      if (RwkStatus=1) or (qryPassSelectLot.Fieldbyname('LotStatus').asinteger=1) then
      begin
        lab_RwkSQntySum.Visible:=true;
        edtRwkSQntySum.Visible:=true;

        qryExec.Close;
        qryExec.SQL.Clear;
        qryExec.SQL.Add('exec FMEdCalRwkSQnty '''
              + tblMaster.fieldbyname('PaperNum').asstring+'''');
        qryExec.execSQL;

        tblDetail.Close;
        tblDetail.Parameters.ParamByName('PaperNum').Value:=tblMaster.fieldbyname('PaperNum').asstring;
        tblDetail.Open;

      end;
      //2021.02.24 計算重工過帳數
      if (qryPassSelectLot.Fieldbyname('LotStatus').asinteger=1) then
      begin
        //2021.09.03 要搬到參數外，否則過帳數量防呆會有問題
        {qryExec.Close;
        qryExec.SQL.Clear;
        qryExec.SQL.Add('exec FMEdCalRwkPQnty '''
              + tblMaster.fieldbyname('PaperNum').asstring+'''');
        qryExec.execSQL;

        tblDetail.Close;
        tblDetail.Parameters.ParamByName('PaperNum').Value:=tblMaster.fieldbyname('PaperNum').asstring;
        tblDetail.Open;}

        //2021.04.21 重工報廢缺點明細
        pnl_XOutDefect.Enabled:=true;
        pnl_XOutDefect.Visible:=true;

        if pnl_XOutDefect.Visible then
          pnl_XOutDefect.Height := Round(Panel4.Height / 2);

        if pnl_XOutDefect.Enabled then
        begin

           if gridRwkSDefect.Visible=false then
           begin
               gridXOutDefect.Visible:=false;
               gridRwkSDefect.Visible:=true;
               gridRwkSDefect.Align:=alNone;
               gridRwkSDefect.Align:=alClient;
               DBNavigator1.DataSource:=dsRwkSDefect;
               Panel1.Caption:='重工報廢缺點明細';
           end;

           //2021.05.03 開放發起站輸入缺點明細
           {qryRwkSDefect.FieldByName('SerialNum').ReadOnly:=false;
           qryRwkSDefect.FieldByName('ClassId').ReadOnly:=false;
           qryRwkSDefect.FieldByName('DefectId').ReadOnly:=false;
           qryRwkSDefect.FieldByName('DutyProc').ReadOnly:=false;
           qryRwkSDefect.FieldByName('Qnty').ReadOnly:=false;}

           //if qryRwkSDefect.Active=false then
           //begin

                with qryExec do
                begin
                    close;
                    SQL.Clear;
                    SQL.Add('select RouteSerial from FMEdProc where LotNum='+''''+tblDetail.FieldByName('LotNum').AsString+'''');
                    open;
                end;
                //2021.04.26
                qryRwkSDefect.LookupType:=lkNone;
                qryRwkSDefect.close;
                qryRwkSDefect.SQL.Clear;
                qryRwkSDefect.SQL.Add('select * from FMEdPassRwkSDefect '+
                                      'where PaperNum='+''''+tblDetail.FieldByName('PaperNum').AsString+''''+
                                      ' and Item='+tblDetail.FieldByName('Item').AsString+
                                      ' and LotNum='+''''+tblDetail.FieldByName('LotNum').AsString+''''+
                                      ' and RouteSerial='+qryExec.FieldByName('RouteSerial').AsString);
                qryRwkSDefect.Open;

           //end;

        end;
      end;

      if RwkStatus=1 then
      begin

          //2021.04.21 重工報廢缺點明細
          pnl_XOutDefect.Enabled:=true;
          pnl_XOutDefect.Visible:=true;

          if pnl_XOutDefect.Visible then
            pnl_XOutDefect.Height := Round(Panel4.Height / 2);

          if pnl_XOutDefect.Enabled then
          begin

             if gridRwkSDefect.Visible=false then
             begin
                 gridXOutDefect.Visible:=false;
                 gridRwkSDefect.Visible:=true;
                 gridRwkSDefect.Align:=alNone;
                 gridRwkSDefect.Align:=alClient;
                 DBNavigator1.DataSource:=dsRwkSDefect;
                 Panel1.Caption:='重工報廢缺點明細';
             end;

             //2021.05.03 開放發起站輸入缺點明細
             {qryRwkSDefect.FieldByName('SerialNum').ReadOnly:=true;
             qryRwkSDefect.FieldByName('ClassId').ReadOnly:=true;
             qryRwkSDefect.FieldByName('DefectId').ReadOnly:=true;
             qryRwkSDefect.FieldByName('DutyProc').ReadOnly:=true;
             qryRwkSDefect.FieldByName('Qnty').ReadOnly:=true;}


             //if qryRwkSDefect.Active=false then
             //begin

                  with qryExec do
                  begin
                      close;
                      SQL.Clear;
                      SQL.Add('select SerialNum=max(t1.SerialNum) '+
                      'from FMEdLotRoute t1, FMEdLotRoute t2 '+
                      'where t1.LotNum='+''''+tblDetail.FieldByName('LotNum').AsString+''''+
                      ' and t1.LotNum=t2.LotNum and t1.IsRework=0 and  t2.IsRework=1'+
                      ' and abs(t1.SerialNum-t2.SerialNum)=1 and t1.SerialNum<t2.SerialNum');
                      open;
                  end;
                  //2021.04.26
                  qryRwkSDefect.LookupType:=lkNone;
                  qryRwkSDefect.close;
                  qryRwkSDefect.SQL.Clear;
                  qryRwkSDefect.SQL.Add('select * from FMEdPassRwkSDefect where LotNum='+
                                        ''''+tblDetail.FieldByName('LotNum').AsString+''''+
                                        ' and RouteSerial>'+qryExec.FieldByName('SerialNum').AsString+
                                        ' order by RouteSerial, SerialNum');
                  qryRwkSDefect.Open;

             //end;
          end;
      end;
  end;

  //2022.09.15  當站重工數控制
  with qryExec do
  begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('select IsRwkQnty from EMOdProcInfo(nolock) '+
	            'where ProcCode=ltrim(rtrim('+''''+qryPassSelectLot.Fieldbyname('ProcCode').AsString+''''+'))');
      Open
  end;

  iRwkQnty:=qryExec.FieldByName('IsRwkQnty').AsInteger;

  if iRwkQnty=1 then
   Begin
     Label8.Visible:=true;
     edtRwkQnty.Visible:=true;
   End;

  //2021.12.08
  with qryExec do
  begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('select SQU4ValueNum=ltrim(rtrim(isnull(SQU4ValueNum,'+''''+''''+')))'+' '+
              'from FQCdProcInfo(nolock) t1, '+
              '     FQCdProcInfoDtl(nolock) t2 '+
	            'where t1.BProcCode=t2.BProcCode '+
              ' and  t2.ProcCode=ltrim(rtrim('+''''+qryPassSelectLot.Fieldbyname('ProcCode').AsString+''''+'))');
      Open
  end;

  btnProcParamImport.Font.Color:=clWindowText;

  if qryExec.FieldByName('SQU4ValueNum').AsString<>'' then
  begin
      btnProcParamImport.Font.Color:=clRed;
  end;

  //2022.04.25
  //if iFocusMode=1 then
  //begin
    btnExecute.SetFocus;
    //2022.11.07
    memoNotes.ReadOnly:=False;
    memoNotes.Lines.Clear;
    with qryExec do
    begin
      qryExec.Close;
      SQL.Clear;
      SQL.Add('select Notes from EMOdLayerRoute(nolock) '+
          'where PartNum='''+tblDetail.FieldByName('PartNum').AsString+''' '+
          'and Revision='''+tblDetail.FieldByName('Revision').AsString+''' '+
          'and LayerId='''+tblDetail.FieldByName('LayerId').AsString+''' '+
          'and ProcCode='''+tblDetail.FieldByName('ProcCode').AsString+'''');
      Open;
      memoNotes.Lines.Add(FieldByName('Notes').AsString);
      memoNotes.ReadOnly:=True;
    end;

  //end;
end;

procedure TfrmFMEdPassPCB.edtLotNumKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  inherited;
  if key=VK_Return then
  begin
     prcImport(Sender);
  end;

end;

procedure TfrmFMEdPassPCB.edtPQntyClick(Sender: TObject);
begin
  inherited;
  TCustomEdit(Sender).SelectAll;
end;

procedure TfrmFMEdPassPCB.edtPQntyExit(Sender: TObject);
begin
  inherited;

  //=====2009.2.4 add for EW RA09020402

  if bIsShowPQnty=false then exit;

  if bIsIssue then exit;

  //2010.10.20 add for YX Bill-20101012-2
  if (eOrgPQnty<=0) or (VarIsNull(eOrgPQnty)) then  exit;

  with tblDetail do
    begin
      //-----2010.10.20 disable for YX Bill-20101012-2
      //if bIsCNC then
      //  if FieldByName('POP').AsInteger=3 then exit;
      //-----

      if FieldByName('Qnty').AsFloat<=0 then  exit;

      if FieldByName('PQnty').AsFloat<=0 then  exit;

      //if FieldByName('Qnty').AsFloat - FieldByName('PQnty').AsFloat < 0 then exit;
      if eOrgPQnty - FieldByName('PQnty').AsFloat < 0 then exit;//2010.10.20 modify for YX Bill-20101012-2

      if LockType=ltReadOnly then exit;

      if edtSQnty.Enabled=false then exit;

      if FieldByName('SQnty').ReadOnly then exit;

      try
       FieldByName('SQnty').AsFloat:=
        //FieldByName('Qnty').AsFloat - FieldByName('PQnty').AsFloat;
        eOrgPQnty - FieldByName('PQnty').AsFloat //2010.10.20 modify for YX Bill-20101012-2
                  - FieldByName('TranPQnty').AsFloat //2011.6.15 add for NH Bill-20110609-2
                  - FieldByName('StkVoidPQnty').AsFloat //2011.11.14 add for NH Bill-20111104-99
                  ;
      except on E: Exception do
        abort;
      end;

      if edtUQnty.Enabled=false then exit;

      if FieldByName('UQnty').ReadOnly then exit;

      if edtUQnty.Visible then
        try
          FieldByName('UQnty').AsFloat:= 0;
        except on E: Exception do
          abort;
        end;//try

      //2020.12.30 自動存檔
      if tblDetail.State in[dsInsert,dsEdit] then tblDetail.Post;
    end;//with tblDetail do
  //=====

end;
//2021.02.24 自動帶重工報廢數
procedure TfrmFMEdPassPCB.edtRwkPQntyExit(Sender: TObject);
begin
  inherited;

  //if bIsShowPQnty=false then exit;

  if bIsIssue then exit;

  //2010.10.20 add for YX Bill-20101012-2
  if (eOrgPQnty<=0) or (VarIsNull(eOrgPQnty)) then  exit;

  with tblDetail do
    begin
      //-----2010.10.20 disable for YX Bill-20101012-2
      //if bIsCNC then
      //  if FieldByName('POP').AsInteger=3 then exit;
      //-----

      if FieldByName('RwkPQnty').AsFloat<0 then  exit;

      if FieldByName('Qnty').AsFloat<=0 then  exit;

      if FieldByName('RwkSQntySum').AsFloat<0 then  exit;

      if (FieldByName('Qnty').AsFloat-FieldByName('RwkSQntySum').AsFloat)
          < FieldByName('RwkPQnty').AsFloat then  exit;


      //if FieldByName('Qnty').AsFloat - FieldByName('PQnty').AsFloat < 0 then exit;
      if eOrgPQnty - FieldByName('PQnty').AsFloat < 0 then exit;//2010.10.20 modify for YX Bill-20101012-2

      if qryPassSelectLot.Fieldbyname('LotStatus').asinteger<>1 then  exit;

      try

          FieldByName('RwkScrapQnty').AsFloat:=
           FieldByName('Qnty').AsFloat
         - FieldByName('RwkSQntySum').AsFloat
         - FieldByName('RwkPQnty').AsFloat;

      except on E: Exception do
        abort;
      end;



      //2020.12.30 自動存檔
      if tblDetail.State in[dsInsert,dsEdit] then tblDetail.Post;
    end;//with tblDetail do
  //=====
end;

procedure TfrmFMEdPassPCB.edtSQntyChange(Sender: TObject);
begin
  inherited;
  bSQntyChange:=true;//2012.09.25 add for CMT Bill-20120906-01
end;

procedure TfrmFMEdPassPCB.edtSQntyClick(Sender: TObject);
begin
  inherited;
  TCustomEdit(Sender).SelectAll;
end;

procedure TfrmFMEdPassPCB.edtSQntyExit(Sender: TObject);
begin
  inherited;

  bSQntyChange:=false;//2012.09.25 add for CMT Bill-20120906-01

  //=====2009.2.4 add for EW RA09020402
  if bIsShowPQnty=false then exit;

  if bIsIssue then exit;

  if (bPassSQntyBack2PQnty=false) then //2012.09.25 add 'if...' for CMT Bill-20120906-01
    begin
      if edtUQnty.Visible=false then exit;
    end;

  //2010.10.20 add for YX Bill-20101012-2
  if (eOrgPQnty<=0) or (VarIsNull(eOrgPQnty)) then  exit;

  with tblDetail do
  begin
      //-----2010.10.20 disable for YX Bill-20101012-2
      //if bIsCNC then
      //  if FieldByName('POP').AsInteger=3 then exit;
      //-----

      if FieldByName('Qnty').AsFloat<=0 then  exit;

      if FieldByName('PQnty').AsFloat<=0 then  exit;

      if FieldByName('SQnty').AsFloat< 0 then  exit;

      if (bPassSQntyBack2PQnty=false) then //2012.09.25 add 'if...' for CMT Bill-20120906-01
      begin
        //if (FieldByName('Qnty').AsFloat
        if (eOrgPQnty  //2010.10.20 modify for YX Bill-20101012-2
          - FieldByName('PQnty').AsFloat
          - FieldByName('TranPQnty').AsFloat //2011.6.15 add for NH Bill-20110609-2
          - FieldByName('StkVoidPQnty').AsFloat //2011.11.14 add for NH Bill-20111104-99
          - FieldByName('SQnty').AsFloat
          )< 0
          then exit;
      end;

      if tblDetail.LockType=ltReadOnly then exit;

      if (bPassSQntyBack2PQnty)
          and (edtUQnty.Visible=false) //2012.10.08 add for CMT Bill-20121007-04
      then //2012.09.25 add 'if' for CMT Bill-20120906-01
      begin
        //2012.09.25 add for CMT Bill-20120906-01
        try
          FieldByName('PQnty').AsFloat:=
          (eOrgPQnty
          - FieldByName('TranPQnty').AsFloat
          - FieldByName('StkVoidPQnty').AsFloat
          - FieldByName('SQnty').AsFloat);
        except on E: Exception do
        abort;
        end;//try
      end
      else
      begin
        //-----------------------------原來的
        if FieldByName('UQnty').ReadOnly then exit;

        if edtUQnty.Enabled=false then exit;

        try
          FieldByName('UQnty').AsFloat:=
          //(FieldByName('Qnty').AsFloat
          (eOrgPQnty  //2010.10.20 modify for YX Bill-20101012-2
          - FieldByName('PQnty').AsFloat
          - FieldByName('TranPQnty').AsFloat //2011.6.15 add for NH Bill-20110609-2
          - FieldByName('StkVoidPQnty').AsFloat //2011.11.14 add for NH Bill-20111104-99
          - FieldByName('SQnty').AsFloat);
        except on E: Exception do
          abort;
        end;//try
        //-----------------------------
      end;//if bPassSQntyBack2PQnty then

  end;//with tblDetail do
  //=====


end;

procedure TfrmFMEdPassPCB.edtUQntyClick(Sender: TObject);
begin
  inherited;
  TCustomEdit(Sender).SelectAll;

end;

procedure TfrmFMEdPassPCB.edtXOutXXXClick(Sender: TObject);
begin
  inherited;
  TCustomEdit(Sender).SelectAll;

end;

procedure TfrmFMEdPassPCB.edt_ScrapStrXOutQntyExit(Sender: TObject);
var sSQL:string;
var ScrapQnty:integer;
var ScrapStrXOutQnty:integer;
begin
  inherited;

  //2021.11.17 防止沒輸入數字跳開時，文字轉數字錯誤
  if edt_ScrapStrXOutQnty.Text='' then
  begin
     ScrapStrXOutQnty:=0;
  end
  else
  begin
     ScrapStrXOutQnty:=StrToInt(edt_ScrapStrXOutQnty.Text);
  end;

  if edtSQnty.Text='' then
  begin
     ScrapQnty:=0;
  end
  else
  begin
     ScrapQnty:=StrToInt(edtSQnty.Text);
  end;

  //if (StrToInt(edtSQnty.Text)<=0) and (StrToInt(edt_ScrapStrXOutQnty.Text)>0) then
  if (ScrapQnty<=0) and (ScrapStrXOutQnty>0) then
  begin
     edt_ScrapStrXOutQnty.Text := '0';
     sSQL:='update FMEdPassSub set ScrapStrXOutQnty = 0'
          +' where PaperNum = '
          +''''+tblMaster.FieldByName('PaperNum').AsString+'''';

     qryExec.Close;
     qryExec.SQL.Clear;
     qryExec.SQL.Add(sSQL);
     qryExec.ExecSQL;

     tblDetail.Close;
     tblDetail.Open;

     MsgDlgJS('沒有「報廢數」，不能輸入「整報沖銷單報數」',mtError,[mbOk],0);
     exit;
  end;

  //if (StrToInt(edtSQnty.Text)>0) and (StrToInt(edt_ScrapStrXOutQnty.Text)>0) then
  if (ScrapQnty>0) and (ScrapStrXOutQnty>0) then
  begin
    if tblDetail.State in[dsInsert,dsEdit] then tblDetail.Post;

    //先重新計算單報數
    btnTmp2Xout.Click;

    //2020.12.24 「整報沖銷單報數」自動更新「本站新增單報數」
    sSQL:='update FMEdPassSub set XOutQnty = XOutQnty + ' + edt_ScrapStrXOutQnty.Text
          +' where PaperNum = '
          +''''+tblMaster.FieldByName('PaperNum').AsString+'''';

    qryExec.Close;
    qryExec.SQL.Clear;
    qryExec.SQL.Add(sSQL);
    qryExec.ExecSQL;

    tblDetail.Close;
    tblDetail.Open;
  end;
end;

procedure TfrmFMEdPassPCB.FormCreate(Sender: TObject);
begin
  inherited;

  qryProcParamDtl.LookupType:=lkLookupTable;//2012.08.28 add for NH Bill-20120704-05

  qryFMEdLotXOutDiv.LookupType:=lkLookupTable;//2012.12.19 add for NH Bill-20121206-03


end;

procedure TfrmFMEdPassPCB.JSdLookupCombo1Dropdown(Sender: TObject);
begin
  inherited;
  if TJSdLookupCombo(Sender).LkDataSource.DataSet.Active=false then abort;
end;

procedure TfrmFMEdPassPCB.page_PassPCB_120827AChange(Sender: TObject);
begin
  inherited;

  //2012.12.19 add for NH Bill-20121206-03
  if page_PassPCB_120827A.ActivePageIndex<>0 then
     if tbsht_PassPCB_XOutNH.TabVisible=true then
         if tblLotXOut.Active then
           if tblLotXOut.State in[dsEdit] then tblLotXOut.Post;
end;

procedure TfrmFMEdPassPCB.ShowXOut;
begin
  qryPassSetXOut.Close;
  qryPassSetXOut.Parameters.ParamByName('LotNum').Value := CurrLotNum;

  qryPassSetXOut.Parameters.ParamByName('PaperNum').Value
    //:= tblDetail.FieldByName('PaperNum').AsString; //2008.9.30 add for RD
    :=CurrPaperNum;

  qryPassSetXOut.ExecSql;

  tblLotXOut.Close;

  tblLotXOut.Open;
end;


procedure TfrmFMEdPassPCB.StringField2Validate(Sender: TField);
begin
  inherited;
  //2021.07.05 TCI 取消
  {if qryClassId.Active then
    if qryClassId.RecordCount>0 then
      if qryClassId.Locate('ClassId',Sender.Value,[loCaseInsensitive]) then
        begin
          qryRwkSDefect.FieldByName('DutyProc').Value
            :=qryClassId.FieldByName('Notes').Value;
        end; }

  //2021.10.25
  if iDutyProc<>1 then exit;

  //2021.07.05 TCI 不看責任單位 看責任製程
  if qryClassId.Active then
    if qryClassId.RecordCount>0 then
      if qryClassId.Locate('ClassId',Sender.Value,[loCaseInsensitive]) then
      begin
        qryRwkSDefect.FieldByName('DutyProc').Value
        :=qryClassId.FieldByName('ResProc').Value;
      end;

end;



procedure TfrmFMEdPassPCB.StringField3Validate(Sender: TField);
begin
  inherited;
  //2021.10.25
  if iDutyProc<>2 then exit;

  //2021.07.05 TCI 不看責任單位 看責任製程
  if qryDefectId.Active then
    if qryDefectId.RecordCount>0 then
      if qryDefectId.Locate('DefectId',Sender.Value,[loCaseInsensitive]) then
      begin
        qryRwkSDefect.FieldByName('DutyProc').Value
        :=qryDefectId.FieldByName('ResProc').Value;
      end;

end;

//2012.09.12 add for NH Bill-20120704-05
function TfrmFMEdPassPCB.funProcParamExists:boolean;
begin
  result:=true;

  if bPassUseNHParam then
     if qryProcParamDtl.Active then
        if qryProcParamDtl.RecordCount>0 then
           begin
     		     MsgDlgJS('請先清除「製程參數明細」', mtError, [mbOk], 0);
     		     exit;
           end;

  result:=false;
end;

procedure TfrmFMEdPassPCB.btnGetLotClick(Sender: TObject);
begin
  inherited;

  if funProcParamExists then  exit;//2012.09.12 add for NH Bill-20120704-05

  Application.createform(TdlgPassSelect, dlgPassSelect);
  dlgPassSelect.qryPassSelect.ConnectionString:=qryExec.ConnectionString;

  //----------2012.1.2 add
  dlgPassSelect.lab_LineId.Visible:=bUseLineId;
  dlgPassSelect.edt_LineId.Visible:=bUseLineId;
  //----------
  {
  //----------2011.10.4 add
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('UseId='+sUseId);
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('UserId='+sUserId);
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('sGUID='+unit_DLL.funGlobalIdGet(sUserId));
  TJSdTable(dlgPassSelect.qryPassSelect).LogItemId:=sItemId;
  TJSdTable(dlgPassSelect.qryPassSelect).LogUserId:=sUserId;
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('CompanyUseId='+sCompanyUseId);
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('LanguageId='+sLanguageId);
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('ServerName='+sServerName);
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('LoginSvr='+sLoginSvr);
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('DBName='+sDBName);
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('LoginDB='+sLoginDB);
  TJSdTable(dlgPassSelect.qryPassSelect).ReserveList.Add('MainGUID='+sGlobalId);
  //----------
  }
  //2012.09.26 modify
  dlgPassSelect.qryPassSelect.ReserveList.text:=tblMaster.ReserveList.Text;

  //2011.2.11 add
  if sPassSelectDlgMode='1' then
    begin
      dlgPassSelect.grid_JSd.DataSource:=nil;
      dlgPassSelect.grid_JSd.Visible:=false;
      dlgPassSelect.grid_JSd.Align:=alNone;

      dlgPassSelect.grid_WW.DataSource:=dlgPassSelect.dsPassSelect;
      dlgPassSelect.grid_WW.Visible:=true;
      dlgPassSelect.grid_WW.Align:=alClient;
    end
  else
    begin
      dlgPassSelect.grid_JSd.DataSource:=dlgPassSelect.dsPassSelect;
      dlgPassSelect.grid_JSd.Visible:=true;
      dlgPassSelect.grid_JSd.Align:=alClient;

      dlgPassSelect.grid_WW.DataSource:=nil;
      dlgPassSelect.grid_WW.Visible:=false;
      dlgPassSelect.grid_WW.Align:=alNone;
    end;

  with dlgPassSelect, tblDetail do
  begin
     iInStock := PowerType;

     showmodal;

     if modalResult=mrok then
     begin
       //2008.11.28 add for RD
       if qryPassSelect.fieldbyname('LotNum').asstring='' then
          begin
     		    MsgDlgJS('請先選入批號', mtError, [mbOk], 0);
     		    abort;
          end;

       CurrLotNum := qryPassSelect.fieldbyname('LotNum').asstring;

       if State in [dsEdit,dsInsert] then Cancel;

       First;

       while not eof do
       begin
         Delete;
         //Next; //2008.05.27 disable 因ADO元件在delete後會自動NEXT 若再下NEXT會造成第二筆不會刪除
       end;

       Append;

       fieldbyname('LotNum').asstring:=qryPassSelect.fieldbyname('LotNum').asstring;

       prcImport(sender);
      end;
  end;

  edtPQnty.SetFocus;
end;

procedure TfrmFMEdPassPCB.btnGetLotDataClick(Sender: TObject);
begin
  inherited;

  if funProcParamExists then exit;//2012.09.12 add for NH Bill-20120704-05

  prcImport(Sender);
end;

procedure TfrmFMEdPassPCB.tblDetailAfterCancel(DataSet: TDataSet);
begin
  inherited;
  bSQntyChange:=false;//2012.09.25 add for CMT Bill-20120906-01
end;

procedure TfrmFMEdPassPCB.tblDetailAfterEdit(DataSet: TDataSet);
begin
  inherited;
navMain.DataSource:= dsDetail;
end;

procedure TfrmFMEdPassPCB.tblDetailAfterInsert(DataSet: TDataSet);
begin
  inherited;
  navMain.DataSource:= dsDetail;

  //DataSet.fieldbyname('Item').Asinteger:= DataSet.Tag; //2017.05.04 disable
end;

//2011.5.29 add for YX Bill-20110517-2
procedure TfrmFMEdPassPCB.prcShowFQC_XOutTOL;
begin
   pnl_FQC_XOut_TOL.Visible:=edt_ScrapStrXOutQnty.Visible;

  if pnl_FQC_XOut_TOL.Visible then
    begin
      if qryFQC_XOutTOL.Active then qryFQC_XOutTOL.Close;

      if qryFQC_XOutTOL.DataSource=nil then qryFQC_XOutTOL.DataSource:=dsDetail;

      qryFQC_XOutTOL.Close;
      qryFQC_XOutTOL.Open;
    end;
end;

procedure TfrmFMEdPassPCB.tblDetailAfterPost(DataSet: TDataSet);
begin
  inherited;
  tblDetail.Refresh;
  bSQntyChange:=false;//2012.09.25 add for CMT Bill-20120906-01
end;

procedure TfrmFMEdPassPCB.tblDetailAfterScroll(DataSet: TDataSet);
begin
  inherited;
  bSQntyChange:=false;//2012.09.25 add for CMT Bill-20120906-01
end;

procedure TfrmFMEdPassPCB.tblDetailBeforeCancel(DataSet: TDataSet);
begin
  inherited;
  bSQntyChange:=false;//2012.09.25 add for CMT Bill-20120906-01
end;

procedure TfrmFMEdPassPCB.tblDetailBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  bSQntyChange:=false;//2012.09.25 add for CMT Bill-20120906-01
end;

procedure TfrmFMEdPassPCB.tblDetailBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  with dsMaster.DataSet do
     if state in [dsEdit, dsInsert] then Post;

  //2017.06.09 disable
  //DataSet.Tag:=GetMaxSerialNumDLL(DataSet, 'Item')+1;
end;

procedure TfrmFMEdPassPCB.tblDetailBeforePost(DataSet: TDataSet);
begin
  inherited;

  //2012.09.25 add for CMT Bill-20120906-01
  if (bPassSQntyBack2PQnty) and bSQntyChange then
    begin
      if edtSQnty.Visible then
         begin
           edtSQntyExit(edtSQnty);
         end;

      bSQntyChange:=false;
    end;

  if edtUQnty.Visible=false then   //2007.08.27 add
    if DataSet.FieldByName('UQnty').Asfloat<>0 then
      begin
        MsgDlgJS('不允許輸入留存數', mtError, [mbOK], 0);
        //DataSet.FieldByName('UQnty').AsInteger:=0;
        abort;
      end;

  if ((pnlXOut.Enabled=false) and//((pnlXOut.Visible=false) and  //2008.05.05 add
      (pnlXOutSum.Visible=false)) then   //2007.08.27 add
    if DataSet.FieldByName('XOutQnty').Asfloat<>0 then
      begin
        MsgDlgJS('不允許輸入單報數', mtError, [mbOK], 0);
        //DataSet.FieldByName('XOutQnty').AsInteger:=0;
        abort;
      end;

//2008.04.27 disable 因生管批量資訊維護開放手動改DateCode 故此處已不須防範了
//  if pnlDateCode.Visible=false then   //2007.08.27 add
//    if trim(DataSet.FieldByName('DateCode').Asstring)<>'' then
//      begin
//        MsgDlgJS('不允許輸入DateCode', mtError, [mbOK], 0);
        //DataSet.FieldByName('DateCode').value:=null;
//        abort;
//      end;

  if DataSet.FieldByName('UQnty').IsNull then
    DataSet.FieldByName('UQnty').value:=0;

  if DataSet.FieldByName('XOutQnty').IsNull then
    DataSet.FieldByName('XOutQnty').value:=0;

  if DataSet.FieldByName('DateCode').IsNull=false then
    if trim(DataSet.FieldByName('DateCode').Asstring)='' then
        DataSet.FieldByName('DateCode').value:=null;

end;

procedure TfrmFMEdPassPCB.tblDetailNewRecord(DataSet: TDataSet);
var sSQL:string; iItem:integer; //2017.05.04 add
begin
  inherited;

  //----------2017.05.04 add
  sSQL:='select iItem=max(Item) from FMEdPassSub(nolock) where PaperNum='
       + ''''+ tblDetail.FieldByName('PaperNum').asString+'''';

  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Add(sSQL);
  qryExec.Open;

  if qryExec.RecordCount=0 then
     iItem:=0
  else if qryExec.FieldByName('iItem').IsNull then
     iItem:=0
  else
     iItem:=qryExec.FieldByName('iItem').asInteger;

  tblDetail.FieldByName('Item').AsInteger:= iItem+1;
  //----------

  //2011.6.15 add for NH Bill-20110609-2
  tblDetail.FieldByName('TranPQnty').AsFloat:=0;

  //2011.11.14 add for NH Bill-20111104-99
  tblDetail.FieldByName('StkVoidPQnty').AsFloat:=0;

  //2011.12.13 add for MUT Bill-20111207-06
  tblDetail.FieldByName('iIsSinglePass').AsFloat:=1;
end;

procedure TfrmFMEdPassPCB.tblDetailPQntyValidate(Sender: TField);
begin
  inherited;
//2007.07.30 disable 因在CNC製程造成換算錯誤
  {if ((edtUQnty.Visible) or (pnlPassMrg.Visible)) then
    with tblDetail do
    begin
      fieldbyname('UQnty').Asinteger:=
        fieldbyname('Qnty').Asinteger
        -fieldbyname('PQnty').Asinteger
        -fieldbyname('SQnty').Asinteger;
    end;}
end;

procedure TfrmFMEdPassPCB.tblDetailProcCodeChange(Sender: TField);
begin
  inherited;
  //2008.8.21 add for SK 08081501
  if pnl_EquipId.Visible then
    with qryEquipProc do
      begin
        close;
        Parameters.ParamByName('ProcCode').Value
          :=tblDetail.FieldByName('ProcCode').AsString;
        open;
      end;
end;

procedure TfrmFMEdPassPCB.tblDetailSQntyValidate(Sender: TField);
begin
  inherited;
//2007.07.30 disable 因在CNC製程造成換算錯誤
{  if ((edtUQnty.Visible) or (pnlPassMrg.Visible)) then
    with tblDetail do
    begin
      fieldbyname('UQnty').Asinteger:=
        fieldbyname('Qnty').Asinteger
        -fieldbyname('PQnty').Asinteger
        -fieldbyname('SQnty').Asinteger;
    end;}
end;

procedure TfrmFMEdPassPCB.tblLotXOutAfterPost(DataSet: TDataSet);
begin
  inherited;
  tblLotXOut.Refresh;
  btnTmp2Xout.Click;

  //2012.12.19 add for NH Bill-20121206-03
  if tbsht_PassPCB_XOutNH.TabVisible=true then
    if qryFMEdLotXOutDiv.SQL.Text<>'' then
      begin
        qryFMEdLotXOutDiv.Close;
        qryFMEdLotXOutDiv.Open;
      end;

end;

procedure TfrmFMEdPassPCB.tblLotXOutBeforeDelete(DataSet: TDataSet);
begin
  inherited;
  abort;//2010.7.8 add
end;

procedure TfrmFMEdPassPCB.tblLotXOutBeforeInsert(DataSet: TDataSet);
begin
  inherited;
  abort;
end;

procedure TfrmFMEdPassPCB.tblMasterAfterEdit(DataSet: TDataSet);
begin
  inherited;
  navMain.DataSource:= dsMaster;
end;

procedure TfrmFMEdPassPCB.tblMasterAfterInsert(DataSet: TDataSet);
begin
  inherited;
  navMain.DataSource:= dsMaster;
end;

procedure TfrmFMEdPassPCB.tblMasterAfterOpen(DataSet: TDataSet);
begin
  inherited;
  if tblDetail.Active=false then tblDetail.Open;

end;

procedure TfrmFMEdPassPCB.tblMasterAfterPost(DataSet: TDataSet);
begin
  inherited;
  tblMaster.Refresh;
end;

procedure TfrmFMEdPassPCB.wwDBGrid1Enter(Sender: TObject);
begin
  inherited;
  if tblDetail.Active then
    if tblDetail.State in[dsEdit] then tblDetail.Post;

end;

end.

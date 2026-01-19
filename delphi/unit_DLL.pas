unit unit_DLL;

interface

uses Forms, SysUtils, IniFiles, controls, StdCtrls, Classes,
  windows, ComCtrls, dialogs,  ExtCtrls,JSdDBGrid,Graphics,
   Variants, StrUtils, shellapi,DB,ADODB,ActiveX, AxCtrls,

   JSdTable,Buttons,JSdLabel,wwDBDateTimePicker,DBCtrls,JSdLookupCombo,

   ComObj,Excel2000,Mask,DBTables,JSdReport,JSdGrid2Excel,JSdPopupMenu,Menus,
   Messages,DelphiZXingQRCode;


type

    TPopuMenuClick = class
      oldOnClick: TNotifyEvent;
      procedure DLLPopuMenuEvent(Sender: TObject);
    end;


type

    TJSdLookUpComboEnter = class
      oldOnEnter: TNotifyEvent;
      procedure prcJSdLookUpComboEnter(Sender: TObject);
    end;


type

    TevDBEdtDblClickEvent = class
      oldDBEdtDblClickEvent: TNotifyEvent;
      procedure prcDBEdtDblClick(sender: TObject);
    end;


type

    TevTableEvent = class
      oldTableEvent: TDatasetNotifyEvent;
      procedure prcTblEventAfterClose(dataset: TDataSet);
      procedure prcTblEventAfterDelete(dataset: TDataSet);
      procedure prcTblEventAfterDelete2(dataset: TDataSet);//2011.10.13 add
      procedure prcTblEventAfterEdit(dataset: TDataSet);
      procedure prcTblEventAfterInsert(dataset: TDataSet);
      procedure prcTblEventAfterOpen(dataset: TDataSet);
      procedure prcTblEventAfterPost(dataset: TDataSet);
      procedure prcTblEventAfterPost2(dataset: TDataSet);
      procedure prcTblEventAfterRefresh(dataset: TDataSet);
      procedure prcTblEventAfterScroll(dataset: TDataSet);
      procedure prcTblEventBeforeCancel(dataset: TDataSet);
      procedure prcTblEventBeforeClose(dataset: TDataSet);
      procedure prcTblEventBeforeDelete(dataset: TDataSet);
      procedure prcTblEventBeforeDelete2(dataset: TDataSet);//2011.10.13 add
      procedure prcTblEventBeforeEdit(dataset: TDataSet);
      procedure prcTblEventBeforeInsert(dataset: TDataSet);
      procedure prcTblEventBeforeOpen(dataset: TDataSet);
      procedure prcTblEventBeforePost(dataset: TDataSet);
      procedure prcTblEventBeforeRefresh(dataset: TDataSet);
      procedure prcTblEventBeforeScroll(dataset: TDataSet);
    end;


type

    TevFieldValidate = class
      oldFieldValidate: TFieldNotifyEvent;
      procedure prcDoFieldValidate(sender:TField);
    end;
{
type
    TevCustBtnClick = class
      oldCustBtnClick: TNotifyEvent;
      procedure prcDoCustBtnClick(sender:TSpeedButton);
    end;
}
{
type
  POCXFiledOnValidate = ^TOCXFiledOnValidate;
  TOCXFiledOnValidate = record
    bOCXonValidateType: integer;
    OCXonValidateMsg: string;
    OCXonValidateFd1: string;
    OCXonValidateFd2: string;
    OCXonValidateFd3: string;
    OCXonValidateFd4: string;
    OCXonValidateFd5: string;
    OCXonValidateFd6: string;
    OCXonValidateUd: string;
  end;
}

type POCXsysButton = ^TOCXsysButton;
     TOCXsysButton = record
       ButtonName:string;
       CustCaption:widestring;
       CustHint:widestring;
       bVisiable:integer;//2012.04.30 add for Bill-20120426-01
       SerialNum:integer;//2017.02.09 add
       InPanelSeq:integer;//2017.02.09 add
  end;

type POCXcustButton = ^TOCXcustButton;
     TOCXcustButton = record
       ButtonName:string;
       CustCaption:widestring;
       CustHint:widestring;
       OCXName:string;
       CoClassName:string;
       ChkCanbUpdate:integer;
       ChkStatus:integer;
       bNeedNum:integer;
  end;

type

  TShowChildForm=function(
  rParent: TWinControl;
  sTitle:string;
  sCaption:string;
  bShowModal:boolean;
  //====
  sServerName:string;
  sDBName:string;
  sItemId:string;
  sDLLName:string;
  sClassName:string;
  sUserId:string;
  sBUID:string;
  sGlobalId:string;
  sUseId:string;
  //====for Flow
  sPaperId:string;
  sPaperNum:string;
  hDllHandle:THandle;
  sSystemId:string;
  iCallType:integer;
  iDtlItem:integer //new 2009.8.26
  ):Boolean;stdcall;

{

function funGetOCXInfo(

  var sServerName:string;

  var sDBName:string;

  sCoClassName:string;
  var sItemId:string;
  var sClassName:string;
  var sUserId:string;
  var sBUID:string;
  var sGlobalId:string;
  var sUseId:string;
  var sConnectStr:string
  ):boolean;
}
function funConnectedADO(conn:TADOConnection;sConnStr:string):boolean;

procedure prcFormSetConnDLL(fm: TForm;sconnstr: String);

function funGetFormInfo(
  qry:TADOQuery;
  //sCoClassName:string;
  sItemId:string;
  sTableKind:string;
  var sTableName:string;
  var iFixColCount:integer;
  var sMDKey:string;
  var sRealTableName:string;
  iMust:integer;
  var sDDCaption:string;
  sLanguageId:string
  ):boolean;

function funGetSelectSQL(
  qry:TADOQuery;
  sCoClassName:string;
  sTableName:string;
  var sSelectSQL:string
  ):boolean;

//procedure ActiveFormSet(fm: TForm);

//procedure prcSetFormatDBExOCX(dbset: TCustomADODataSet; sTableName, Conn: String);

//procedure prcDoDD(DataSet: TCustomADODataSet);

//procedure DoGrid(gridJS: TJsdDBGrid);

//停用
function funDoAfterOpen(fm:TForm;tTable:TJSdTable):boolean;

function funDoAfterPost(tTable:TJSdTable):boolean;
function funDoBeforeDelete(tTable:TJSdTable):boolean;//2011.10.13 add,//保留
function funDoAfterDelete(tTable:TJSdTable):boolean;//2011.10.13 add,//保留

//停用
function funDoJSdTableEvent(fm:TForm;qry:TADOQuery;tTable:TJSdTable;sEvent:string):string;

function funDoTableEvent(qry:TADOQuery;tTable:TDataSet;iCompelAftPost:integer):boolean;

function funTableEventImpl(tTable:TDataSet;sEvenName:string):string;

function funDoFieldValidate(qry:TADOQuery;tTable:TJSdTable):boolean;
//function funGetTableKeysOCX(qry:TADOQuery;sTableName: String): string;//停用
function funGetLocateKeys(qry:TADOQuery;sItemId,sTableKind: String): string;

function funGetRealTableName(qry:TADOQuery;sTableName: String): string;
function funIFStringStr(bYes:Boolean; str1, str2: String): String;
function funIFFieldFind(tTable:TJSdtable;sFieldName:String; str1: String): String;

//2012.04.30 add
function GetTableKeysDLL(qryPK:TADOQuery;sTableName: WideString): Variant;

function funTableEventByParams(  //停用,要保留
  CanbLockUserEdit:integer;
  sUserId:string;
  sPaperUserId:string;
  dataset:TDataSet;
  sEventKind:string
  ):boolean;

function funCheckSameUser(
  CanbLockUserEdit:integer;
  //sUserId:string; //2012.09.19 disable for debug
  sPaperUserId:string;
  sUserId:string //2012.09.19 add for debug
  ):boolean;

function funDLLOpSetUp(
  afm:TForm;
  qry:TADOQuery;
  sItemId:string;
  sDLLName:string;
  sClassName:string;
  sUserId:string;
  sBUID:string;
  sGlobalId:string;
  sConnectStr:string;
  sUseId:string;
  var sTableNameMas1:string;
  var sTableNameDtl1:string;
  var sSelectSQLMas1:string;
  var sSelectSQLDtl1:string;
  var iFixColCntMas1:integer;
  var iFixColCntDtl1:integer;
  var sMDKeyMas1:string;
  var sMDKeyDtl1:string;
  gridMas1:TJSdDBGrid;
  gridDtl1:TJSdDBGrid;
  tblMas1:TJSdTable;
  tblDtl1:TJSdTable;
  var sRealTableNameMas1:string;
  var sRealTableNameDtl1:string;
  //2
   //--Item
  iOpKind:integer; //0 一般 1單據
  var PaperType:integer;
  var CurrTypeHead: WideString;
  var PowerType: integer;
  var FunctionType: integer;
    //--User Power
  var CanbUpdate	:Integer;
  var CanbUpdateMoney	:Integer;
  var CanbAudit	:Integer;
  var CanbAuditBack	:Integer;
  var CanbScrap	:Integer;
  var CanbViewMoney	:Integer;
  var CanbPrint :Integer;
  var CanbUpdateNotes:Integer;
  var CanbRunFLow	:Integer;
  var CanbSelectType :Integer;
  var CanbLockPaperDate :Integer;
  var CanbLockUserEdit :Integer;
  var CanbMustNotes :Integer;
  var sDDCaptionMas1:string;
  var sDDCaptionDtl1:string;

  var CanbExport:Integer;//2015.11.17 add for Bill-20151112-03
  var CanbF9	  :Integer;//2015.11.17 add for Bill-20151112-03
  var CanbF12	  :Integer//2015.11.17 add for Bill-20151112-03
  ):boolean;

function funGetTableSetUp(
  afm:TForm;
  qry:TADOQuery;
  sItemId:string;
  sTableKind:string;
  var sTableName:string;
  var iFixColCount:integer;
  var sMDKey:string;
  var sSelectSQL:string;
  grid:TJSdDBGrid;
  tbl:TJSdTable;
  var sRealTableName:string;
  iOpKind:integer;
  iMust:integer;
  var sDDCaption:string;
  iCompelAftPost:integer
  ):boolean;

function funReplaceCom(sStr:string;tbl:TJSdTable):string;

function CreateJSdLabel(
  sCaption: WideString;
  iTop: integer;
  iLeft: integer;
  iHeight: integer;
  iWidth: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString
  ): TJSdLabel;

  function CreateJSdLabel2(
  sCaption: WideString;
  iTop: integer;
  iLeft: integer;
  iHeight: integer;
  iWidth: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString
  ): TJSdLabel;

 function CreateLabelDLL(
  sCaption: WideString;
  iTop: integer;
  iLeft: integer;
  iHeight: integer;
  iWidth: integer;
  AOwner: TComponent;
  AParent: TWinControl
  ): TLabel;

  function CreateComboBoxDLL(ParamName: WideString; iPrompt, sTop, sLeft, sHeight,
    sWidth, iDType: integer; Default: WideString; sList: TStrings;
    AOwner: TComponent; AParent: TWinControl): TComboBox;


function CreateRadioGroupDLL(ParamName: WideString; iPrompt, sTop, sLeft, sHeight,
  sWidth, iDType: integer; Default: WideString; AOwner: TComponent;
  AParent: TWinControl): TRadioGroup;

function CreateDBDateTimePicker(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer
  ): TwwDBDateTimePicker;

function CreateDBDateTimePicker2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  sEditColor:string
  ): TwwDBDateTimePicker;

function CreateDateTimePickerDLL(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  Default,
  sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  iReadOnly:integer
  ): TwwDBDateTimePicker;

function CreateDBEdit(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer
  ): TDBEdit;

  function CreateDBEdit2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  iIsNotesField:integer;
  sEditColor:string
  ): TDBEdit;

//2011.10.11 add
function prcComoBoxGetItems(cbo:TDBComboBox):boolean;

function CreateDBComoBox2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  iIsNotesField:integer;
  sEditColor:string;
  sItems:WideString
  ): TDBComboBox;

//2012.05.08 for EMO
function CreateDBComoBox2_Mas(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  iIsNotesField:integer;
  sEditColor:string;
  sItems:WideString;
  iNameItem:Integer
  ): TDBComboBox;

function CreateJSdLookupComboSubDLL_Dtl(ParamName: WideString; iPrompt,
      sTop, sLeft, sHeight,
      sWidth, iDType: integer; SQL: WideString; Default, sSId: WideString;
      AOwner: TComponent;
      AParent: TWinControl; iNameItem:Integer;
      //2012.05.09 add
      tDataSource:TDataSource;
      sDataField:WideString;
      iReadOnly:integer;
      sEditColor:string): TJSdLookupCombo;
//2012.05.08 for EMO end

  function CreateDBMemo2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  iIsNotesField:integer;
  sEditColor:string
  ): TDBMemo;

function CreateEditDLL(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  Default,
  sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  iReadOnly:integer
  ):  TMaskEdit;//2011.6.4 modify for QU Johnson-20110603-1
  //TEdit;

function CreateDBCheckBox(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer
  ): TDBCheckBox;

function CreateDBCheckBox2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer
  ): TDBCheckBox;

function CreateJSdLookupComboOCX(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  SQL: WideString;
  Default: WideString;
  lookds: TDataSource;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  dsLK: TDataSource;
  iReadOnly:integer;
  iComboTextSize:integer
  ): TJSdLookupCombo;

function CreateJSdLookupComboOCX2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  SQL: WideString;
  Default: WideString;
  lookds: TDataSource;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  dsLK: TDataSource;
  iReadOnly:integer;
  iComboTextSize:integer;
  sEditColor:string;
  LookupTable,
  LookupResultField,
  LookupCond1Field,
  LookupCond2Field,
  LookupCond1ResultField,
  LookupCond2ResultField:string;
  iShowSeq:integer //2022.04.20
  ): TJSdLookupCombo;

function CreateJSdLookupComboDLL(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  SQL: WideString;
  Default: WideString;
  lookds: TDataSource;
  AOwner: TComponent;
  AParent: TWinControl;
  sConnectStr:string;
  iReadOnly:Integer
  ): TJSdLookupCombo;
//2018.11.30
function CreateJSdLookupComboDLL2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  sTextWidth,
  iDType: integer;
  SQL: WideString;
  Default: WideString;
  lookds: TDataSource;
  AOwner: TComponent;
  AParent: TWinControl;
  sConnectStr:string;
  iReadOnly:Integer
  ): TJSdLookupCombo;

function CreateJSdLookupComboSubDLL(ParamName: WideString; iPrompt, sTop, sLeft, sHeight,
      sWidth, iDType: integer; SQL: WideString; Default, sSId: WideString; AOwner: TComponent;
      AParent: TWinControl; EnterEvent: TNotifyEvent): TJSdLookupCombo;
//2018.11.30
function CreateJSdLookupComboSubDLL2(ParamName: WideString; iPrompt, sTop, sLeft, sHeight,
      sWidth, sTextWidth, iDType: integer; SQL: WideString; Default, sSId: WideString; AOwner: TComponent;
      AParent: TWinControl; EnterEvent: TNotifyEvent): TJSdLookupCombo;

function funDrawPaperPNL(
  afm:TForm;
  tTable:TJSdTable;
  qry:TADOQuery;
  iOpKind:integer;
  CanbLockPaperDate:integer
  ):boolean;

function funDrawPaperPNL2(
  afm:TForm;
  tTable:TJSdTable;
  qry:TADOQuery;
  iOpKind:integer;
  CanbLockPaperDate:integer;
  CanbViewMoney:integer
  ):boolean;

//2013.10.23 for EMO
function funDrawPaperPNL_EMO(
  afm:TForm;
  tTable:TJSdTable;
  qry:TADOQuery;
  iOpKind:integer;
  CanbLockPaperDate:integer;
  CanbViewMoney:integer
  ):boolean;

//2012.05.28 add for WF Bill-20120518-02A
function funDrawPNL3(
  afm:TForm;
  tTable:TJSdTable;
  qry:TADOQuery;
  iOpKind:integer;
  CanbLockPaperDate:integer;
  CanbViewMoney:integer;
  ds:TDataSource ;
  CanbUpdateMoney:integer //2017.03.06 add for SS
  ):boolean;

function funGetOrderByField(
  qry:TADOQuery;
  sItemId:string;
  sTableKind:string
  ):string;

function funGetFilterSQL(
  qry:TADOQuery;
  sItemId:string;
  sTableKind:string
  ):string;

function funGetMDKeySQL(sMDKey:string):string;
function funGetInqInSubSQL(sMDKey,AliaName:string):string;
procedure prcSaveALL(fm:TForm);
procedure prcCancelALL(fm:TForm);//2011.9.22 add

function DLLSetPowerType(
  iOpKind:integer;
  afm:TForm;
  qry:TADOQuery;
  sItemId:string;
  sUserId:string;
  sUseId:string;
  sRealTableNameMas1:string;
   //--Item
  var PaperType :integer;
  var CurrTypeHead: WideString;
  var PowerType: integer;
  var FunctionType: integer;
    //--User Power
  var CanbUpdate	:Integer;
  var CanbUpdateMoney	:Integer;
  var CanbAudit	:Integer;
  var CanbAuditBack	:Integer;
  var CanbScrap	:Integer;
  var CanbViewMoney	:Integer;
  var CanbPrint	:Integer;
  var CanbUpdateNotes :Integer;
    //--Paper Power
  var CanbRunFLow	:Integer;
  var CanbSelectType :Integer;
  var CanbLockPaperDate :Integer;
  var CanbLockUserEdit :Integer;
  var CanbMustNotes :Integer;

  var CanbExport	:Integer;//2015.11.17 add for Bill-20151112-03
  var CanbF9	    :Integer;//2015.11.17 add for Bill-20151112-03
  var CanbF12	    :Integer  //2015.11.17 add for Bill-20151112-03
  ):boolean;

function OcxCallOcxDlg(
  afm:TForm;
  sOcxName,
  sCoClassName,
  sUserId,
  sBUId,
  sUseId,
  sItemId,
  sSERVERName,
  sDBName,
  sGlobalId,
  sPaperNum
  :string
  ):boolean;
{
function funGetOCXInfoDlg(
  dlgAfm:TForm;
  var sItemId:string;
  var sOcxName:string;
  var sUserId:string;
  var sBUID:string;
  var sServerName:string;
  var sDBName:string;
  var sGlobalId:string;
  var sUseId:string;
  var sPaperNum:string;
  var sConnectStr:string
  ):boolean;
}
function funCustBtnDo( //未完成
  afm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CutBtn:TSpeedButton
  ):boolean;

//2012.04.12 add,以拆字的方式判斷是 TCP/IP 還是 Name
function funCheckTCP_IP(sTarget:string;bLast:boolean):boolean;

//2012.04.12 add,以拆字的方式判斷是 TCP/IP 還是 Name
function funConnectMode(sServerName:string):integer;

//2012.04.25 add for Johnson-20120425-01
function funGetNetworkLibrary():string;

function funConnectStrGet(
  sServerName,
  sDBName
  ,sTempBasJSISpw,sGlobalId //2012.06.01 add for SS Bill-20120531-01
  :string
  ):string;

function funConnectStrGetAdmin(
  sServerName,
  sDBName,
  sPw:string //2012.09.21 add
  ):string;

function funCopyTempeleteToItem(qry:TADOQuery):boolean;

function funStartDLL(
  tFrm:TForm;
  rParent:TWinControl;
  bShowModal:boolean;
  //=====
  sServerName,
  sDBName,
  sItemId,
  sDLLName,
  sClassName,
  sUserId,
  sBUID,
  sGlobalId,
  sUseId,
  sPaperId,
  sPaperNum,
  sSystemId:string;
  iCallType:integer;
  iDtlItem:integer//new 2009.8.26

  ):boolean;

function funStartDllNoParent(
  tFrm:TForm;
  bShowModal:boolean;
  //=====
  sServerName,
  sDBName,
  sItemId,
  sDLLName,
  sClassName,
  sUserId,
  sBUID,
  sGlobalId,
  sUseId,
  sPaperId,
  sPaperNum,
  sSystemId:string;
  iDtlItem:integer //new 2009.8.26
  ):boolean;

function DLLGetTempPathStr : WideString;

function funHandleLogUpdate(sLoginSvr,sLoginDB,sMainGlobalId,sItemId:string):boolean;
function funHandleLogUpdate2(sLoginSvr,sLoginDB,sMainGlobalId,sItemId:string):boolean;
function funShowDLLFormExecSQL(sConnStr,sExecSQL:string):boolean;

function funCallDLL(
  qryExec:TADOQuery;
  fStartForm:TForm;
  iCallType:integer;//0 MainForm, 1 DLL, 2 Flow , 3 PaperTrace
  bShowModal:boolean;
  sItemId,
  sItemName,
  sClassName,
  sSystemId,
  sServerName,
  sDBName,
  sUserId,
  sBUId,
  sUseId,
  sPaperId,
  sPaperNum,
  sGlobalId  :string;
  tOtherParent:TWinControl;
  sServerPath:string;
  sLocalPath:string;
  sLoginSvr:string;
  sLoginDB:string;
  //bLocalTemCopy:boolean;
  //iItemType:integer
  sOCXTemplate:string;
  iDtlItem:integer; //new 2009.8.26
  sTranGlobalId:string; //2009.9.1 add
  sTempBasJSISpw:string//2012.06.01 modify for SS Bill-20120531-01
  ):THandle;

function funCallDLL2(
  qryExec:TADOQuery;
  fStartForm:TForm;
  iCallType:integer;//0 MainForm, 1 DLL, 2 Flow , 3 PaperTrace
  bShowModal:boolean;
  sItemId,
  sItemName,
  sClassName,
  sSystemId,
  sServerName,
  sDBName,
  sUserId,
  sBUId,
  sUseId,
  sPaperId,
  sPaperNum,
  sGlobalId  :string;
  tOtherParent:TWinControl;
  sServerPath:string;
  sLocalPath:string;
  sLoginSvr:string;
  sLoginDB:string;
  //bLocalTemCopy:boolean;
  //iItemType:integer
  sOCXTemplate:string;
  iDtlItem:integer; //new 2009.8.26
  sTranGlobalId:string; //2009.9.1 add
  sTempBasJSISpw:string//2012.06.01 modify for SS Bill-20120531-01
  ):THandle;


function funFreeDLL_Main(sLoginSvr,sLoginDB,sMainGlobalId:string;bALL:boolean):boolean;

function funGlobalIdGet(sUserId:string):string;

function funCustButtonDo(
  fFrm:TForm;
  sBtnName:string;
  tTable:TJSdTable;
  qryExec:TADOQuery;
  sItemId,
  sGlobalId,
  sSystemId,
  sServerName,
  sDBName,
  sUserId,
  sBUID,
  sUseId,
  sRealTableNameMas1:string;
  iCanbUpdate,
  iOPKind:integer;
  sPaperUserId:string;
  CanbLockUserEdit:integer;
  sLoginSvr:string; //new 2009.7.22
  sLoginDB:string;  //new 2009.7.22
  sPaperMode:string;
  tTblDtl:TJSdTable; //new 2009.8.26
  sTranGlobalId:string; //2009.9.1 add
  hMain_btnPrint_Handle:THandle//2010.11.8 add
  ):boolean;

function funTranDataInsert(
  fFrm:TForm;
  sGlobalId,
  sGlobalId_tmp,
  sItemId,
  sBtnName,
  sBeCallDLLName,
  sUserId,
  sUseId,
  sSystemId:string;
  qryExec:TADOQuery;
  tTable:TJSdTable
  ):boolean;

function funCheckPaper4EngDesign(
  tblTable:TJSdTable;
  CanbUpdate:integer;
  CanbLockUserEdit:integer;
  sUserId:string;
  sNowMode:string
  ):boolean;

function funCheckPaper4EngDesign2(
  tblTable:TJSdTable;
  CanbUpdate:integer;
  CanbLockUserEdit:integer;
  sUserId:string;
  sNowMode:string;
  bChkFinished03:boolean;
  bChkFinished2:boolean;
  bChkFinished1:boolean
  ):boolean;

function funPaperMsgGet(
  sItemId:string;
  sButtonName:string;
  qry:TADOQuery;
  iType:integer
  ):string;

function funPaperExam(
  //frm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CanbRunFLow,
  CanbAudit:integer;
  sRealTableNameMas1,
  sUserId:string;
  sItemId:string
  ):boolean;

function funPaperCompleted(
  //frm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CanbRunFLow,
  CanbAudit:integer;
  sRealTableNameMas1,
  sUserId:string;
  CanbLockUserEdit:integer;
  sItemId:string;
  bUseFlow:boolean;
  sUseId:string;
  sSystemId:string;
  iNowFlowStatus:integer
  ):boolean;

function funPaperUpdateNotes(
  iIsMas:integer;
  qry:TADOQuery;
  tTable:TJSdTable;
  sRealTableNameMas:string;
  tDtl:TJSdTable;
  sRealTableNameDtl:string;
  CanbUpdate:integer;
  CanbLockUserEdit:integer;
  sUserId:string
  ):boolean;

function funPaperReGetNum(
  CanbLockPaperDate:integer;
  CanbUpdate:integer;
  //====================
  sUserId:string;
  sUseId:string;
  qryExec:TADOQuery;
  tTable:TJSdTable;
  sRealTableNameMas1:string;
  sSelectSQLMas1:string;
  PaperType:integer;
  CanbSelectType:integer;
  var bNewCancel:boolean;
  var CurrTypeHead:widestring;
  var CurrPaperType:integer;
  var CurrPaperNum:string;
  CanbLockUserEdit:integer;
  PowerType:integer //2010.9.15 add for QU Foster-20100913-1
  ):boolean;

function funPaperRejExam(
  //frm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CanbRunFLow,
  CanbAuditBack:integer;
  sRealTableNameMas1,
  sUserId:string;
  CanbMustNotes:integer;
  sItemId:string
  ):boolean;

function funIsMaxPaperNum(
  qry:TADOQuery;
  CurrPaperId,
  CurrUseHead,
  CurrTypeHead,
  CurrPaperNum:string;
  sUseId:string; //2010.7.23 add for YX RA10070501
  sPaperDate:string //2011.9.29 add for MUT Bill-20110928-02
  ):Boolean;

function funPaperVoid(
  //frm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CanbRunFLow,
  CanbScrap:integer;
  sRealTableNameMas1,
  sUserId:string;
  CanbMustNotes:integer;
  sUseId:string;
  CurrTypeHead:string;
  CanbLockUserEdit:integer;
  sItemId:string
  ):boolean;

function funDLLInfoByPaperId( //未使用2009.6.29
  qry:TADOQuery;
  sPaperId:string;
  var sItemId:string;
  var sItemName:string
  ):boolean;

function funNewPaper(
  bReGetNum:boolean;
  sUserId:string;
  sUseId:string;
  sPaperNum:string;
  dPaperDate:TDateTime;
  qryExec:TADOQuery;
  tTable:TJSdTable;
  sRealTableNameMas1:string;
  sSelectSQLMas1:string;
  PaperType:integer;
  CanbSelectType:integer;
  var bNewCancel:boolean;
  var CurrTypeHead:widestring;
  var CurrPaperType:integer;
  var CurrPaperNum:string;
  sRunSQLAfterAdd:string;
  PowerType:integer //2010.9.15 add for QU Foster-20100913-1
  ):boolean;

function funDLLSysParamsGet(qryExec:TADOQuery;sSystemId,sParamName:string):string;
function funFlowPrcIdGet(qryExec:TADOQuery;sItemId:string):string;

function GetReportFieldDLL(ProcName, ReportName: WideString;
       AParams: Array of WideString;qryExec:TADOQuery;sConnectStr:string): Boolean;

function GetReportFieldDLL2(ProcName, ReportName: WideString;
       AParams: Array of WideString;qryExec:TADOQuery;sConnectStr,sNowSQL:string): Boolean;

function GetParamStringDLL(ProceName: WideString;
      ParamList: array of WideString;sConnectStr:string): WideString;
procedure BackupTableDLL(sTable, bkName, sWhere: WideString;qryExec:TADOQuery;sConnectStr:string);
function SQLExecIfExistExDLL(
  sObjName, strSQL, sType: WideString; bExist: Boolean;sConnectStr:string): WideString;
function DataSet2ExcelDLL(data:TCustomADODataSet; sdbFile, sTableName, ReportName: string;
   iOpen: integer;sConnectStr:string): Boolean;
//2012.05.09 add for MUT Bill-20120509-01
function DataSet2ExcelDLL2(data:TCustomADODataSet; sdbFile, sTableName, ReportName: string;
   iOpen: integer;sConnectStr:string;qryExec:TADOQuery): Boolean;
//2020.07.06  add
function DataSet2ExcelDLL_TCI(data:TCustomADODataSet; sdbFile, sTableName, ReportName: string;
   iOpen: integer;sConnectStr:string;qryExec:TADOQuery): Boolean;
//2013.01.07 add for MU
function DataSet2ExcelDLL_EMO(data:TCustomADODataSet; sdbFile, sTableName, ReportName: string;
   iOpen: integer;sConnectStr:string;qryExec:TADOQuery): Boolean;

function Ds2Excel4ReImport(data:TCustomADODataSet; sTableName: string;
   iOpen: integer): Boolean;

function Ds2ExcelSimple(data:TCustomADODataSet;sSheetName,sOutFileName:string;iOpen:integer):Boolean;
function Ds2ExcelSimple2(data:TCustomADODataSet;sSheetName,sOutFileName:string):Boolean;
function Ds2TxtSimple(frm:TForm;data:TCustomADODataSet;sFileName:WideString):boolean;

function DsExport(frm:TForm;data:TCustomADODataSet):boolean;

function Query2DataSetDLL(SQLStmts: WideString; dset:TADOQuery;sConnectStr:string):Boolean;
procedure SetReportFieldDLL(dbset: TCustomADODataSet; sReportName, Conn: WideString);

//2012.05.09 add for MUT Bill-20120509-01
procedure SetReportFieldDLL2(dbset: TCustomADODataSet; sReportName, Conn: WideString;qryExec:TADOQuery);

function  IIFIntegerDLL(bYes:Boolean; str1, str2: String): Integer;
function  IIFStringDLL(bYes:Boolean; str1, str2: String): String;
procedure ParseFormatWDLL(sInput, sDim: WideString; var sParam : array of WideString; iPara:integer);
function xlsColToNumDLL(ColName: WideString): Integer;
function NumToxlsColDLL(ColNum: Integer): WideString;
function Proc2QueryDLL(ProcName:WideString; AParams: Array of WideString;sConnectStr:string): WideString;
function GetParamString(ProceName: WideString;
  ParamList: array of WideString;sConnectStr:string): WideString;
function FormatParamDLL(ctrl: TControl; CurrLinkType, CurrDisplayType:integer;sConnectStr:string): WideString;
function DataSet2ParadoxDLL(data:TCustomADODataSet; sdbFile, IndexFiled: string): Boolean;
function DataSet2AccessDLL(data:TCustomADODataSet; sdbFile, sTableName: string): Boolean; overload;
//2017.08.28 EMO
function DataSet2AccessDLL(data:TCustomADODataSet; sdbFile, sTableName, sConnectStr: string): Boolean; overload;
function SyncFileStrDLL(sSrc, sDest: WideString): Boolean;
function SyncFileStrDLL2(sSrc, sDest: WideString): Boolean;
function IsEqualDLL(sr1, sr2: TSearchRec): Boolean;
function IsEqualDLL2(sr1, sr2: TSearchRec): Boolean;
function CopyFileStrDLL(sSrc, sDest: String): Boolean;
//function funReportServerGet(qryExec:TADOQuery;sBUID:string):string;
function funReportServerGet(qryExec:TADOQuery;sBUID,sGlobalId:string):string;//2012.05.22 modify for WF Bill-20120518-05
function RightDLL(Patten:String; count:integer) : String;
function GetNoLockFileNameDLL(sFileName, adds:WideString): WideString;
procedure ItemHelpGetDLL(sItemId, sCap: String;qry:TADOQuery);
function RunJSdReportDLL(
      JsRpt: TJSdReport;
      ProcName: WideString;
      ParamList: array of WideString;
      sIndex,
      sRptName: WideString;
      //=====
      qryExec:TADOQuery;
      sSystemId,
      sBUId,
      sUserId:string;
      AftPrn: TNotifyEVent;
      bSetUpByRpt:boolean;
      //=====
      hMain_btnPrint_Handle:THandle;//2010.11.8 add
      sMainGlobalId:string; //2010.11.8 add
      iShowModal:integer //2010.11.8 add
      ): Boolean;
function RunReportAftPrnDLL(JsRpt:TJSdReport;sProcName, sBUId, sUserId, sIndex, sRptName, sSQLLoop: WideString;
  OutputType, ShowTitle: integer; ParamList: array of WideString; AftPrn: TNotifyEVent;sConnectStr:string;
  hMain_btnPrint_Handle:THandle;//2010.11.8 add
  sMainGlobalId:string; //2010.11.8 add
  iShowModal:integer
  ): Boolean;

function ShowWinReportDLL(JsRpt:TJSdReport;sProcName, sBUId, sUserId, sIndex, sRptName, sSQLLoop: WideString;
  OutputType, ShowTitle: integer; ParamList: array of WideString; AftPrn: TNotifyEVent;sConnectStr:string;
  hMain_btnPrint_Handle:THandle;//2010.11.8 add
  sMainGlobalId:string; //2010.11.8 add
  iShowModal:integer
  ): Boolean;
function Paper2DataSetDLL(sProcName, sSQLLoop: WideString; dset:TADOQuery;sBUID,sConnectStr:string):Boolean;
procedure ActiveXViewerRunDLL(LocaldbName,
  LocalRptName: WideString; AftPrn: TNotifyEVent;JSRpt:TJSdReport);
function GetMaxSerialNumDLL(tblDset: TDataset; sFieldName: string):integer;
procedure BrowseDataDLL(tblDset: TDataset; bRefresh: Boolean);
//function funSaveToExcel(tbl:TJSdTable;pwgSaveToExcel:TJSdGrid2Excel;sItemName:string):boolean;
function funSaveToExcel(frm:TForm;tbl:TJSdTable):boolean;
procedure Copy2DD(qry:TADOQuery;tblDset: TDataset;sTableName:string);
function funPaperGetRunSQLAfterAdd(qry:TADOQuery;sItemId:string):string;

procedure OpenSQLDLL(qry:TADOQuery;sDo,sSQL:string);
procedure prcSetReadOnly(tbl:TJSdTable;bReadOnly:boolean);//2009.12.22 add

function funDoSinglePrint( //2009.12.25 add
  iSerialNum,
  iPrintType:integer;
  sCurrCond,
  sRealTableNameMas1,
  sItemName,
  sSystemId,
  sBUId,
  sUserId:string;
  qry:TADOQuery;
  sConnectStr:string;
  hMain_btnPrint_Handle:THandle;//2010.11.8 add
  sMainGlobalId:string;
  sReportTitle:widestring //2012.03.30 add for Bill-20120329-04
  ):boolean;

function funPrintPaper( //2009.12.28 add
  pmuPaperPaper:TJSdPopupMenu;
  qryBrowse:TJSdTable;
  qryExec:TADOQuery;
  sRealTableNameMas1,
  sConnectStr,
  sBUId,
  sUserId,
  sItemName,
  sSystemId:string;
  btn:TSpeedButton;
  iIsList:integer;
  sNoOrderByMasSQL:string;
  hMain_btnPrint_Handle:THandle;//2010.11.8 add
  sGlobalId,
  sItemId //2012.03.30 add for Bill-20120329-04
  :string
  ):boolean;


procedure InitJSdTableLableDLL(ds: TDataSet);//2010.2.1 add,copy from unit_MIS

procedure StringToFontStyleDLL(AFont:TFont; sFStyle: WideString); //2010.2.1 add,copy from unit_MIS

procedure prcGrdHeightSet(grid:TJSdDBGrid;sItemId:string;sKind:string;qry:TADOQuery);

//2012.10.17 add for SS Bill-20110912-01
function funObjSizeSet(sName:string;i:integer;sItemId:string;sKind:string;qry:TADOQuery):integer;

//2012.10.17 add for SS Bill-20110912-01
function funObjValueSet(sName:string;sValue:widestring;sItemId:string;
  sKind:string;qry:TADOQuery) :widestring;

procedure prcStoreFieldNeed_Def(frm:TForm;qryExec:TADOQuery);

function funFieldCheckNeed(tbl:TJSdTable):boolean;

procedure prcFormLanguageDO(frm:TForm;qryExec:TADOQuery;sTableName,sLanguageId:string);

procedure prcLangMsg(qry:TADOQuery;sMsgId:widestring;sLanguageId:string);
procedure JSdMessageDlgDLL(Msg: WideString);
//function funCheckX86:string; //保留

procedure prcCallLog(sConnectStr,sPaperId,sPaperNum:string);
//2019.04.01
procedure prcCallLogNew(sConnectStr,sPaperId,sPaperNum:string;qryBrowse:TJSdTable);
procedure prcImportXLS(sConnectStr,sPaperId:string);
function UConvert(sSource: String; TransType:integer): String;

procedure prcChangeShowMode(pnl_NowMode:TPanel;sNowMode:string); //2010.09.10 add for YX Bill-20100907-2
procedure prcChangeShowMode2(pnl_NowMode:TPanel;sNowMode:string);//2012.05.17 add

function funGetFileName4ToExcelQuick: string;//2013.08.15 add for DYN Bill-20130806-02
procedure prcDsToExcelQuick(qryData:TJSdTable);//2013.08.15 add for DYN Bill-20130806-02

//2019.02.01
function funGetItemFullHeightDel(sCalcItemId:string;qryExec:TADOQuery): Integer;

//2020.03.10
function MsgDlgJS(const Msg: string; DlgType: TMsgDlgType;
                    Buttons: TMsgDlgButtons; HelpCtx: Integer): Integer;

var
  evTableEvent: TevTableEvent;
  evFieldValidate:TevFieldValidate;

const VHeight=24;
      HBlock=4;
      conBtnName_Exam='btnExam';
      conBtnName_RejExam='btnRejExam';
      conBtnName_Void='btnVoid';
      conBtnName_Completed='btnCompleted';

implementation

uses
  ShowDLLForm,
  PaperSelectType2DLL, LoadProgressDLL, unit_Access, AskDestDLL,CRActiveXViewer,
  ShowDBEdit, FunctionNotes2,ErrorDialogDLL, PaperSearchDLL, unit_DLL2,
  CondRunSpDLL,EditGridDLL,UOMGetLotDLL,UpdateLog,MsgUserSelect,ImportXLS,
  PaperPrint;
  //unit_KEY; //2012.09.22 add

//2013.08.15 add for DYN Bill-20130806-02
function funGetFileName4ToExcelQuick: string;
var
  sfilename :string;
  savedialog :TSaveDialog;
  sReFileName:string;
  sErr:boolean;
begin
      result:='';

      sErr:=false;

      sReFileName:='';

      sfilename:='C:\Test.xlsx';

      savedialog:=TSaveDialog.Create(nil);
      savedialog.FileName:=sfilename;
      savedialog.Filter:='Excel(*.xlsx)|*.xlsx';

      if savedialog.Execute then
      begin

        if FileExists(savedialog.FileName) then
          try
            if MsgDlgJS('檔案已存在，是否覆蓋？',mtConfirmation,[mbYes,mbNo],0)=mrYes then
               DeleteFile(PChar(savedialog.FileName))
            else
            begin
              savedialog.free;

              sReFileName:='';
              sErr:=true;
            end;
          except
            savedialog.free;

            sReFileName:='';
            sErr:=true;
          end;

        if sErr then exit;

        sfilename:=savedialog.FileName;

      end;//if savedialog.Execute then

      savedialog.free;

      if sfilename='' then
        begin
          sReFileName:='';
          exit;
        end;

      sReFileName:=sfilename;

      result:=sReFileName;
end;

//2013.08.15 add for DYN Bill-20130806-02
procedure prcDsToExcelQuick(qryData:TJSdTable);
var adoRecordset: Variant;
    xlApp,xlBook,xlSheet,xlQuery: Variant;
    sReFileName:string;
begin
  if qryData.Active=false then
    begin
      MsgDlgJS('資料表未開啟',mtInformation,[mbOk],0);
      exit;
    end;

  if qryData.RecordCount=0 then
    begin
      MsgDlgJS('沒有資料，無法轉Excel',mtInformation,[mbOk],0);
      exit;
    end;

  sReFileName:=funGetFileName4ToExcelQuick;

  if sReFileName='' then exit;

  adoRecordset := CreateOleObject('ADODB.Recordset');
  adoRecordset:=qryData.Recordset;
  try
    xlApp := CreateOleObject('Excel.Application');
    xlBook := xlApp.Workbooks.Add;
    //xlSheet := xlBook.Worksheets['sheet1'];
    xlSheet := xlBook.sheets[1];//2013.09.07 modify

    xlApp.Visible := False;
    xlQuery := xlSheet.QueryTables.Add(adoRecordset, xlSheet.Range['A1']);
    xlQuery.FieldNames := True;
    xlQuery.RowNumbers := False;
    xlQuery.PreserveFormatting := True;
    xlQuery.RefreshOnFileOpen := False;
    xlQuery.BackgroundQuery := True;
    xlQuery.SaveData := True;
    xlQuery.AdjustColumnWidth := True;
    xlQuery.RefreshPeriod := 0;
    xlQuery.PreserveColumnInfo := True;
    xlQuery.FieldNames := True;
    xlQuery.Refresh;

    if copy(sReFileName,length(sReFileName)-3,4)<>'.xlsx' then
        sReFileName:=sReFileName+'.xlsx';

    xlBook.SaveAs(sReFileName);

  finally
    if not VarIsEmpty(XLApp) then begin
      XLApp.displayAlerts:=false;
      XLApp.ScreenUpdating:=true;
      XLApp.quit;
    end;
  end;

  MsgDlgJS('已完成',mtInformation,[mbOk],0);
end;

//2010.9.10 add for YX Bill-20100907-2
procedure prcChangeShowMode(pnl_NowMode:TPanel;sNowMode:string);
begin
  if sNowMode='BROWSE' then
    begin
      pnl_NowMode.Caption:='瀏覽模式';
      pnl_NowMode.Color:=clNavy;
      pnl_NowMode.Font.Color:=clWhite;
    end
  else if sNowMode='UPDATE' then
    begin
      pnl_NowMode.Caption:='編輯模式';
      pnl_NowMode.Color:=clRed;
      pnl_NowMode.Font.Color:=clWhite;
    end;
end;

//2012.05.17 add
procedure prcChangeShowMode2(pnl_NowMode:TPanel;sNowMode:string);
begin
  if sNowMode='BROWSE' then
    begin

      pnl_NowMode.Color:=clNavy;

    end
  else if sNowMode='UPDATE' then
    begin

      pnl_NowMode.Color:=clRed;

    end;
end;

//TransType:0 to TW   1 to CN
function UConvert(sSource: String; TransType:integer):String;
var
  wordapp, Doc: Variant;
  SaveNoChanges: Boolean;
begin
  try
    Result:='無法轉出';
    WordApp := CreateOLEObject('Word.Application');
    WordApp.Application.Visible := false;
    Doc := WordApp.Documents;
    Doc.Add;
    WordApp.Selection.Text:= sSource;
    WordApp.Selection.Range.TCSCConverter(TransType,1,1);
    Result:= wordapp.Selection.Text;
  finally
    SaveNoChanges:= false;
    doc.close(SaveNoChanges);
    wordapp.quit;
    Doc := Unassigned;
    WordApp := Unassigned;
  end;
end;

procedure prcImportXLS(sConnectStr,sPaperId:string);
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Application.CreateForm(TfrmCURdImportXLS,frmCURdImportXLS);
  //2020.03.10
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sList:=TstringList.Create;
      sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sList.Strings[0];
      sList.Free;
      FontSize := StrToInt(sFontSize);
      frmCURdImportXLS.Scaled:=true;
      frmCURdImportXLS.ScaleBy(FontSize,100);
  end;
  frmCURdImportXLS.qryExec.ConnectionString:=sConnectStr;
  frmCURdImportXLS.tblTarger.ConnectionString:=sConnectStr;
  frmCURdImportXLS.sPaperId:=sPaperId;
  frmCURdImportXLS.ShowModal;
end;

procedure prcCallLog(sConnectStr,sPaperId,sPaperNum:string);
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Application.CreateForm(TfrmCURdUpdateLog,frmCURdUpdateLog);
  //2020.03.10
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sList:=TstringList.Create;
      sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sList.Strings[0];
      sList.Free;
      FontSize := StrToInt(sFontSize);
      frmCURdUpdateLog.Scaled:=true;
      frmCURdUpdateLog.ScaleBy(FontSize,100);
  end;
  with frmCURdUpdateLog.qryMaster1 do
    begin
      if active then close;
      ConnectionString:=sConnectStr;
      Parameters.ParamByName('PaperId').Value:=sPaperId;
      Parameters.ParamByName('PaperNum').Value:=sPaperNum;
      Parameters.ParamByName('UserId').Value:='';
      open;
    end;

  frmCURdUpdateLog.ShowModal;
end;
//2019.04.01
procedure prcCallLogNew(sConnectStr,sPaperId,sPaperNum:string;qryBrowse:TJSdTable);
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Application.CreateForm(TfrmCURdUpdateLog,frmCURdUpdateLog);
  //2020.03.10
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sList:=TstringList.Create;
      sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sList.Strings[0];
      sList.Free;
      FontSize := StrToInt(sFontSize);
      frmCURdUpdateLog.Scaled:=true;
      frmCURdUpdateLog.ScaleBy(FontSize,100);
  end;
  with frmCURdUpdateLog.qryMaster1 do
    begin
      if active then close;
      ConnectionString:=sConnectStr;
      Parameters.ParamByName('PaperId').Value:=sPaperId;
      Parameters.ParamByName('PaperNum').Value:=sPaperNum;
      Parameters.ParamByName('UserId').Value:='';
      open;
      //2019.04.01
      ReserveList.Add('UseId='+qryBrowse.ReserveList.Values['UseId']);
      ReserveList.Add('UserId='+qryBrowse.ReserveList.Values['UserId']);
      ReserveList.Add('sGUID='+qryBrowse.ReserveList.Values['sGUID']);
      LogUserId:=qryBrowse.LogUserId;
      ReserveList.Add('LanguageId='+qryBrowse.ReserveList.Values['LanguageId']);
      ReserveList.Add('ServerName='+qryBrowse.ReserveList.Values['ServerName']);
      ReserveList.Add('CompanyUseId='+qryBrowse.ReserveList.Values['CompanyUseId']);
      ReserveList.Add('LoginSvr='+qryBrowse.ReserveList.Values['LoginSvr']);
      ReserveList.Add('DBName='+qryBrowse.ReserveList.Values['DBName']);
      ReserveList.Add('LoginDB='+qryBrowse.ReserveList.Values['LoginDB']);
      ReserveList.Add('MainGUID='+qryBrowse.ReserveList.Values['MainGUID']);
      ReserveList.Add('BUId='+qryBrowse.ReserveList.Values['BUId']);
      ReserveList.Add('TempBasJSISpw='+qryBrowse.ReserveList.Values['TempBasJSISpw']);
    end;

    with frmCURdUpdateLog.qryHis do
    begin
      if active then close;
      ConnectionString:=sConnectStr;
      SQL.Clear;
      SQL.Add('exec CURdTableUpdateLogInqHis '''+sPaperId+''','
        +''''+sPaperNum+''','
        +''''+''+'''');
      open;
      //2019.04.01
      ReserveList.Add('UseId='+qryBrowse.ReserveList.Values['UseId']);
      ReserveList.Add('UserId='+qryBrowse.ReserveList.Values['UserId']);
      ReserveList.Add('sGUID='+qryBrowse.ReserveList.Values['sGUID']);
      LogUserId:=qryBrowse.LogUserId;
      ReserveList.Add('LanguageId='+qryBrowse.ReserveList.Values['LanguageId']);
      ReserveList.Add('ServerName='+qryBrowse.ReserveList.Values['ServerName']);
      ReserveList.Add('CompanyUseId='+qryBrowse.ReserveList.Values['CompanyUseId']);
      ReserveList.Add('LoginSvr='+qryBrowse.ReserveList.Values['LoginSvr']);
      ReserveList.Add('DBName='+qryBrowse.ReserveList.Values['DBName']);
      ReserveList.Add('LoginDB='+qryBrowse.ReserveList.Values['LoginDB']);
      ReserveList.Add('MainGUID='+qryBrowse.ReserveList.Values['MainGUID']);
      ReserveList.Add('BUId='+qryBrowse.ReserveList.Values['BUId']);
      ReserveList.Add('TempBasJSISpw='+qryBrowse.ReserveList.Values['TempBasJSISpw']);
    end;

  frmCURdUpdateLog.ShowModal;
end;

procedure prcLangMsg(qry:TADOQuery;sMsgId:widestring;sLanguageId:string);
var sLangMsg:widestring; //qryTmp:TADOQuery;
begin
  sLangMsg:='';

  //unit_DLL.OpenSQLDLL(qry,'OPEN',
  //    'select dbo.CURdF_LagMsgGet('+''''+sMsgId+''''+','+''''+sLanguageId+''''+')');

  {qryTmp:=TADOQuery.Create(nil);
  qryTmp.ConnectionString:=qry.ConnectionString;
  qryTmp.SQL.Add('exec CURdLagMsgGet :MsgId, :LanguageId');
  with qryTmp.Parameters.AddParameter do
    begin
      DataType:=ftWidestring;
      Name:='MsgId';
      value:=sMsgId;
    end;
  with qryTmp.Parameters.AddParameter do
    begin
      DataType:=ftWidestring;
      Name:='LanguageId';
      value:=sLanguageId;
    end;
  //qryTmp.Parameters.ParamByName('MsgId').Value:=sMsgId;
  //qryTmp.Parameters.ParamByName('LanguageId').Value:=sLanguageId;
  qryTmp.Open;

  if qryTmp.RecordCount>0 then
    sLangMsg:=qryTmp.Fields[0].AsWideString;

  qryTmp.Close;
  qryTmp.Free;}

  qry.Close;
  qry.SQL.Clear;
  qry.SQL.Add('exec CURdLagMsgGet :MsgId, :LanguageId');
  qry.Parameters.ParamByName('MsgId').Value:=sMsgId;
  qry.Parameters.ParamByName('LanguageId').Value:=sLanguageId;
  qry.Open;

  if qry.RecordCount>0 then
    sLangMsg:=qry.Fields[0].AsWideString;

  if sLangMsg='' then sLangMsg:=sMsgId;

  JSdMessageDlgDLL(sLangMsg);
end;

procedure JSdMessageDlgDLL(Msg: WideString);
var sfmTitle: WideString;
    xSave: TSaveDialog;
    MsgJpgName: WideString;
    bSave: boolean;
    lst: TStringList;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
    DlgFontSize: integer;  //2023.08.31 add
begin
  Application.CreateForm(TfrmErrorDialogDLL, frmErrorDialogDLL);
  //2020.03.10
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sList:=TstringList.Create;
      sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sList.Strings[0];
      sList.Free;
      FontSize := StrToInt(sFontSize);
      frmErrorDialogDLL.Scaled:=true;
      frmErrorDialogDLL.ScaleBy(FontSize,100);
  end;
  //sfmTitle := Screen.ActiveForm.Caption;
  //frmErrorDialogDLL.Position:= poMainFormCenter;
  //frmErrorDialogDLL.ServiceEmail:= FSysParams.Values['ServiceEmail'];
  //frmErrorDialogDLL.MaintainInfo:= FSysParams.Values['MaintainInfo'];
  //frmErrorDialogDLL.ErrorSystem:= CurrSystemName;
  frmErrorDialogDLL.ErrorForm:= sfmTitle;
  //frmErrorDialogDLL.ErrorUser:= FUser.UserId;
  DlgFontSize:=9;  //2023.08.31 add
  DlgFontSize:=Round(9 * FontSize / 100);  //2023.08.31 add
  //2023.09.01 add
  if FontSize=100 then
    DlgFontSize:=11;
  frmErrorDialogDLL.Font.Name:= '微軟正黑體';  //2023.08.31 add
  frmErrorDialogDLL.Font.size:= DlgFontSize;  //2023.08.31 add

  frmErrorDialogDLL.SetupContentEx(Msg);
  frmErrorDialogDLL.ShowModal;
  if frmErrorDialogDLL.modalresult=mrYes then
  begin
    try
      lst:= TStringList.Create;
      lst.Add(frmErrorDialogDLL.meoAdvance.Lines.Text);
      xSave:= TSaveDialog.Create(nil);
      xSave.FileName:=
           frmErrorDialogDLL.ErrorSystem
          +frmErrorDialogDLL.ErrorForm
          +FormatDateTime('MMDD-HH', Now)+'.txt';
      if xSave.Execute then
      begin
        lst.SaveToFile(xSave.fileName);
      end;
    finally
      xSave.Free;
      lst.Free;
    end;
  end;
  {else if frmErrorDialogDLL.modalresult=mrYesToAll then
  begin
    try
      lst:= TStringList.Create;
      lst.Add(frmErrorDialogDLL.meoAdvance.Lines.Text);
      MsgJpgName := GetERPAppLocalPath + 'ErrMsg.jpg';

      if FileExists(MsgJpgName) then
        SendMailPrompt(FSysParams.Values['MaintainInfo'],
                     FSysParams.Values['ServiceEmail'],
                     '', '資訊系統郵件訊息',
                     lst.Text, MsgJpgName)
      else
        SendMailPrompt(FSysParams.Values['MaintainInfo'],
                     FSysParams.Values['ServiceEmail'],
                     '', '資訊系統郵件訊息',
                     lst.Text, '');
    finally
      lst.Free;
    end;
  end;}
end;


procedure prcFormLanguageDO(frm:TForm;qryExec:TADOQuery;sTableName,sLanguageId:string);
var sSQL:string;tComp:TComponent;
begin
  if sTableName='' then exit;

  if sLanguageId='' then exit;

  sSQL:='select * from CURdTableFieldLang(nolock) where TableName='+''''+sTableName+''''+
    ' and LanguageId='+''''+sLanguageId+'''';

  OpenSQLDLL(qryExec,'OPEN',sSQL);

  if qryExec.RecordCount=0 then
    begin
      qryExec.Close;
      exit;
    end;

  with qryExec do
  begin
  First;
  while not Eof do
    begin
      tComp:=nil;
      tComp:=frm.FindComponent(FieldByName('FieldName').AsString);
      if tComp<>nil then
        if tComp is TJSdLabel then
        begin
          if TJSdLabel(tComp).DataSource=nil then
            begin
            if FieldByName('DisplayLabel').AsWideString<>'' then
              TJSdLabel(tComp).Caption:=FieldByName('DisplayLabel').AsWideString;

            if FieldByName('FontColor').AsWideString<>'' then
              TJSdLabel(tComp).Font.Color:=StringToColor(FieldByName('FontColor').AsWideString);

            if FieldByName('FontName').AsString<>'' then
              TJSdLabel(tComp).Font.Name:=FieldByName('FontName').AsWideString;

            if FieldByName('FontStyle').AsString<>'' then
              StringToFontStyleDLL(TJSdLabel(tComp).Font,FieldByName('FontStyle').AsWideString);

            {2020.02.24 disable if FieldByName('FontSize').AsInteger>=9 then
              TJSdLabel(tComp).Font.Size:=FieldByName('FontSize').AsInteger;}
            end;
        end
        else
        begin
          if FieldByName('DisplayLabel').AsWideString<>'' then
              begin
                if tComp is TSpeedButton then
                  TSpeedButton(tComp).Caption:=FieldByName('DisplayLabel').AsWideString
                else if tComp is TBitBtn then
                  TBitBtn(tComp).Caption:=FieldByName('DisplayLabel').AsWideString
                else if tComp is TTabSheet then
                  TTabSheet(tComp).Caption:=FieldByName('DisplayLabel').AsWideString
                else if tComp is TPanel then
                  TPanel(tComp).Caption:=FieldByName('DisplayLabel').AsWideString;
              end;

          if FieldByName('FontColor').AsWideString<>'' then
            begin
              if tComp is TSpeedButton then
                TSpeedButton(tComp).Font.Color:=StringToColor(FieldByName('FontColor').AsWideString)
              else if tComp is TBitBtn then
                TBitBtn(tComp).Font.Color:=StringToColor(FieldByName('FontColor').AsWideString)
              else if tComp is TPanel then
                TPanel(tComp).Font.Color:=StringToColor(FieldByName('FontColor').AsWideString)
            end;

          if FieldByName('FontName').AsString<>'' then
            begin
              if tComp is TSpeedButton then
                TSpeedButton(tComp).Font.Name:=FieldByName('FontName').AsWideString
              else if tComp is TBitBtn then
                TBitBtn(tComp).Font.Name:=FieldByName('FontName').AsWideString
              else if tComp is TPanel then
                TPanel(tComp).Font.Name:=FieldByName('FontName').AsWideString
            end;

          if FieldByName('FontStyle').AsString<>'' then
            begin
              if tComp is TSpeedButton then
                StringToFontStyleDLL(TSpeedButton(tComp).Font,FieldByName('FontStyle').AsWideString)
              else if tComp is TBitBtn then
                StringToFontStyleDLL(TBitBtn(tComp).Font,FieldByName('FontStyle').AsWideString)
              else if tComp is TPanel then
                StringToFontStyleDLL(TPanel(tComp).Font,FieldByName('FontStyle').AsWideString)
            end;

          if FieldByName('FontSize').AsInteger>=9 then
            begin
              if tComp is TSpeedButton then
                TJSdLabel(tComp).Font.Size:=FieldByName('FontSize').AsInteger
              else  if tComp is TBitBtn then
                TBitBtn(tComp).Font.Size:=FieldByName('FontSize').AsInteger
              else  if tComp is TPanel then
                TPanel(tComp).Font.Size:=FieldByName('FontSize').AsInteger
            end;
      end;

      next;
    end;
  end;
end;

function funFieldCheckNeed(tbl:TJSdTable):boolean;
var i:integer; tmpFieldName,tmpDisplayLabel:string;
begin
  Result:=True;

  for i := 0 to tbl.ReserveList.Count - 1 do
      if tbl.ReserveList.Names[i]='IsNeed' then
        begin
          tmpFieldName:=tbl.ReserveList.ValueFromIndex[i];
          if tmpFieldName<>'' then
            if tbl.FindField(tmpFieldName)<>nil then
              if tbl.FieldByName(tmpFieldName).Value=null then
                begin
                  tmpDisplayLabel:=tbl.FieldByName(tmpFieldName).DisplayLabel;
                  if tmpDisplayLabel='' then tmpDisplayLabel:=tmpFieldName;

                  MsgDlgJS('必須輸入「'+tmpDisplayLabel+'」',mtError,[mbOk],0);
                  Result:=false;
                end;
        end;
end;

procedure prcStoreFieldNeed_Def(frm:TForm;qryExec:TADOQuery);
var i:integer;
sLanguageId:string;//2010.7.18 add for YX Bill-20100715-1
sItemId,sUserId,sUseId:string;//2010.8.27 add for RD
begin
  for i := 0 to frm.ComponentCount - 1 do
    if frm.Components[i] is TJSdTable then
          if TJSdTable(frm.Components[i]).TableName<>'' then
            begin
              sLanguageId:=TJSdTable(frm.Components[i]).ReserveList.Values['LanguageId'];//2010.7.18 add for YX Bill-20100715-1
              if sLanguageId='' then sLanguageId:='TW';

              sItemId:=TJSdTable(frm.Components[i]).LogItemId;//2010.8.27 add for RD
              sUserId:=TJSdTable(frm.Components[i]).LogUserId;//2010.8.27 add for RD
              sUseId:=TJSdTable(frm.Components[i]).ReserveList.Values['UseId'];//2010.8.27 add for RD

              unit_DLL.OpenSQLDLL(qryExec,'OPEN','exec CURdFieldDef_NeedGet '+
                ''''+TJSdTable(frm.Components[i]).TableName+''''
                +','+''''+sLanguageId+''''//2010.7.18 add for YX Bill-20100715-1
                +','+''''+sItemId+''''//2010.8.27 add for RD
                +','+''''+sUserId+''''//2010.8.27 add for RD
                +','+''''+sUseId+''''//2010.8.27 add for RD
                );

              if qryExec.RecordCount>0 then
                begin
                  qryExec.First;
                  while not qryExec.Eof do
                    begin
                      if TJSdTable(frm.Components[i]).ReserveList.IndexOf(qryExec.Fields[0].AsString)=-1 then
                        TJSdTable(frm.Components[i]).ReserveList.Add(
                          qryExec.Fields[0].AsString);

                      qryExec.next;
                    end;
                end;
            end;
end;

//2012.10.17 add for SS Bill-20110912-01
function funObjSizeSet(sName:string;i:integer;sItemId:string;sKind:string;qry:TADOQuery):integer;
var sReValue:string;iReValue:integer; sSQL:string; sRuleId:string;
begin
  result:=0;

  sRuleId:=sName;

  sSQL:='exec CURdLayerHeightSave '+''''+sItemId+''''+','+
      inttostr(i)+',0,'+''''+sRuleId+''''+','+''''+sKind+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      if sKind='LOAD' then open else ExecSQL;
    end;

  if sKind='LOAD' then
    begin
      iReValue:=0;

      if qry.RecordCount>0 then
        begin
          sReValue:=qry.Fields[0].AsString;
          try
            iReValue:=strtoint(sReValue);
          except
          end;

          if iReValue>0 then  result:=iReValue;
        end;//if qry.RecordCount>0 then

    end;//if sKind='LOAD' then

end;

//2012.10.17 add for SS Bill-20110912-01
function funObjValueSet(sName:string;sValue:widestring;sItemId:string;
  sKind:string;qry:TADOQuery) :widestring;
var sReValue:widestring; sSQL:string; sRuleId:string;
begin
  result:='';

  sRuleId:=sName;

  sSQL:='exec CURdObjValueSet '+''''+sItemId+''''+','+
      ''''+sRuleId+''''+',N'+''''+sValue+''''+','+''''+sKind+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      if sKind='LOAD' then open else ExecSQL;
    end;

  if sKind='LOAD' then
    begin
      if qry.RecordCount>0 then
        begin
          sReValue:=qry.Fields[0].AsString;

          if sReValue<>'' then  result:=sReValue;
        end;//if qry.RecordCount>0 then

    end;//if sKind='LOAD' then

end;

procedure prcGrdHeightSet(grid:TJSdDBGrid;sItemId:string;sKind:string;qry:TADOQuery);
var sGridHeight:string;iGridHeight:integer; sSQL:string; sRuleId:string;
begin
  sRuleId:=sItemId+grid.Name;

  sSQL:='exec CURdLayerHeightSave '+''''+sItemId+''''+','+
      inttostr(grid.Height)+',0,'+''''+sRuleId+''''+','+''''+sKind+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      if sKind='LOAD' then open else ExecSQL;
    end;

  if sKind='LOAD' then
    begin
      iGridHeight:=0;
      if qry.RecordCount>0 then
        begin
          sGridHeight:=qry.Fields[0].AsString;
          try
            iGridHeight:=strtoint(sGridHeight);
          except
          end;

          if iGridHeight>=0 then  grid.Height:=iGridHeight;
        end;
    end;
end;


procedure StringToFontStyleDLL(AFont:TFont; sFStyle: WideString);
const
    FontStyleNames: array [TFontStyle] of String = ('Bold', 'Italic', 'Underline', 'StrikeOut');
var
    fs: TFontStyle;
    FStyle: TFontStyles;
    procedure ReadAFontStyle(StyleName:String; StyleValue: TFontStyle);
    var ips: integer;
    begin
        ips:= POS(lowercase(StyleName), lowercase(sFStyle));
        if ips >0  then
            Include(FStyle, StyleValue)
        else
            Exclude(FStyle, StyleValue);
    end;
begin
    if sFStyle='' then Exit;
    FStyle   :=   AFont.Style;
    for   fs:=Low(TFontStyle)   to   High(TFontStyle)   do
        ReadAFontStyle(   FontStyleNames[fs],   fs   );
    AFont.Style   :=   FStyle;
end;

procedure InitJSdTableLableDLL(ds: TDataSet);
var sItems, sFontAll, sFontName, sFontColor, sFontStyle, sExp1: WideString;
    sFontArray: Array[0..3] of WideString;
    i, iFontSize, iFontColor: integer;
    fstyle: TFontStyle;
    fldname: WideString;
    sEditColor:string;//2010.7.18 add for YX Bill-20100715-1
begin
    with TComponent(ds.Owner) do
    begin
      for i := 0 to ComponentCount - 1 do
      begin
        fldname:='';
        sItems:='';

        //TJSdLabel
        if (Components[i] is TJSdLabel) then
        begin
          fldname:= TJSdLabel(Components[i]).DataField;
          if fldname='' then continue;
          if Assigned(TJSdLabel(Components[i]).DataSource) then
          begin
            if Assigned(TJSdLabel(Components[i]).DataSource.DataSet) then
            begin
              if TJSdLabel(Components[i]).DataSource.DataSet=ds then
              begin
                if Assigned(ds.FindField(fldname)) then
                begin
                    sItems:= TDataset(ds).FieldByName(TJSdLabel(Components[i]).DataField).DisplayLabel;
                    sFontAll:= TDataset(ds).FieldByName(TJSdLabel(Components[i]).DataField).Origin;
                    ParseFormatWDLL(sFontAll, ';', sFontArray, 4);

                    if sFontArray[0]='' then
                       sFontName:= '細明體'
                    else
                       sFontName:= sFontArray[0];
                    if sFontArray[1]<>'' then
                       iFontSize:= strtoint(sFontArray[1])
                    else
                       iFontSize:= 10;
                    if not ((iFontSize>=10) and (iFontSize<=72)) then
                       iFontSize:= 10;

                    if not IdentToColor(sFontArray[2], iFontColor) then
                       sFontColor:= 'clBlack'
                    else
                       sFontColor:= sFontArray[2];

                    sFontStyle:= sFontArray[3];

                    if sItems<>'' then
                    begin
                       TJSdLabel(Components[i]).Caption:= sItems;
                       TJSdLabel(Components[i]).Font.Name:= sFontName;
                       //2020.02.24 disable TJSdLabel(Components[i]).Font.Size:= iFontSize;
                       TJSdLabel(Components[i]).Font.Color:= StringToColor(sFontColor);
                       StringToFontStyleDLL(TJSdLabel(Components[i]).Font, sFontStyle);
                    end;
                    sExp1:= TDataset(ds).FieldByName(TJSdLabel(Components[i]).DataField).DefaultExpression;
                    sExp1:= Copy(sExp1, 1, pos(';', sExp1)-1);
                    {if sExp1<>'' then
                    begin
                       TJSdLabel(Components[i]).Font.Style:= [fsBold];
                       TJSdLabel(Components[i]).Font.Color:= clNavy;
                       //JSdDblClick:= TJSdDblClick.create;
                       //JSdDblClick.oldDblClick:= TJSdLabel(Components[i]).OnDblClick;
                       //TJSdLabel(Components[i]).OnDblClick:= JSdDblClick.ShowFieldLookupEvent;
                    end;}
                end;
              end;
            end;
          end;
        end//if (Components[i] is TJSdLabel) then

        //2010.7.18 add for YX Bill-20100715-1
        else if ds is TJSdTable then
        begin
        if (Components[i] is TJSdLookupCombo) then
        begin
          fldname:= TJSdLookupCombo(Components[i]).DataField;
          if fldname<>'' then
          begin
            if Assigned(TJSdLookupCombo(Components[i]).DataSource) then
              begin
                if Assigned(TJSdLookupCombo(Components[i]).DataSource.DataSet) then
                  begin
                    if TJSdLookupCombo(Components[i]).DataSource.DataSet=ds then
                      begin
                        sEditColor:='';
                        sEditColor:=TJSdTable(TJSdLookupCombo(Components[i]).DataSource.DataSet).ReserveList.Values['EDTCOLOR_'+fldname];
                        if sEditColor<>'' then
                           TJSdLookupCombo(Components[i]).cboColor:=StringToColor(sEditColor);
                      end;
                  end;
              end;
          end;
        end//if (Components[i] is TJSdLookUpCombo) then
        else if (Components[i] is TDBEdit) then
        begin
          fldname:= TDBEdit(Components[i]).DataField;
          if fldname<>'' then
          begin
            if Assigned(TDBEdit(Components[i]).DataSource) then
              begin
                if Assigned(TDBEdit(Components[i]).DataSource.DataSet) then
                  begin
                    if TDBEdit(Components[i]).DataSource.DataSet=ds then
                      begin
                        sEditColor:='';
                        sEditColor:=TJSdTable(TDBEdit(Components[i]).DataSource.DataSet).ReserveList.Values['EDTCOLOR_'+fldname];
                        if sEditColor<>'' then
                           TDBEdit(Components[i]).Color:=StringToColor(sEditColor);
                      end;
                  end;
              end;
          end;
        end //if (Components[i] is TDBEdit) then

        {else if (Components[i] is TDBCheckBox) then
        begin
          fldname:= TDBCheckBox(Components[i]).DataField;
          if fldname<>'' then
          begin
            if Assigned(TDBCheckBox(Components[i]).DataSource) then
              begin
                if Assigned(TDBCheckBox(Components[i]).DataSource.DataSet) then
                  begin
                    if TDBCheckBox(Components[i]).DataSource.DataSet=ds then
                      begin
                        sEditColor:='';
                        sEditColor:=TJSdTable(TDBCheckBox(Components[i]).DataSource.DataSet).ReserveList.Values['EDTCOLOR_'+fldname];
                        if sEditColor<>'' then
                           TDBCheckBox(Components[i]).Color:=StringToColor(sEditColor);
                      end;
                  end;
              end;
          end;
        end;//if (Components[i] is TDBCheckBox) then
        }

        //2011.10.11 add by Garfield
        else if (Components[i] is TDBComboBox) then
        begin
          fldname:= TDBComboBox(Components[i]).DataField;

          if fldname<>'' then
          begin
            if Assigned(TDBComboBox(Components[i]).DataSource) then
              begin
                if Assigned(TDBComboBox(Components[i]).DataSource.DataSet) then
                  begin

                    if TDBComboBox(Components[i]).DataSource.DataSet=ds then
                      begin

                        sEditColor:='';
                        sEditColor:=TJSdTable(TDBComboBox(Components[i]).DataSource.DataSet).ReserveList.Values['EDTCOLOR_'+fldname];
                        if sEditColor<>'' then
                           TDBComboBox(Components[i]).Color:=StringToColor(sEditColor);

                        prcComoBoxGetItems(TDBComboBox(Components[i]));

                      end;//if TDBComboBox(Components[i]).DataSource.DataSet=ds then

                  end;//if Assigned(TDBComboBox(Components[i]).DataSource.DataSet) then
              end;//if Assigned(TDBComboBox(Components[i]).DataSource) then
          end;//if fldname<>'' then

        end;//if (Components[i] is TDBComboBox) then



        end;//if ds is TJSdTable then
      end;
    end;
end;


function funPaperGetRunSQLAfterAdd(qry:TADOQuery;sItemId:string):string;
var sSQL,sRe:string;
begin
  sSQL:='select RunSQLAfterAdd from CURdOCXTableSetUp(nolock) where ItemId='+
    ''''+sItemId+''''+' and TableKind='+''''+'Master1'+'''';

  with qry do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount>0 then
      sRe:= qry.Fields[0].AsString;

   qry.Close;

   result:=sRe;
end;

procedure Copy2DD(qry:TADOQuery;tblDset: TDataset;sTableName:string);
var sSQL:string;  i:integer;
begin

  qry.SQL.Add('delete CURdTableName where TableName='+''''+sTableName+'''');
  qry.ExecSQL;

  sSQL:='insert into CURdTableName(TableName,TableNote,DisplayLabel,SystemId)'+
        ' select '+''''+sTableName+''''+','+
        ''''+sTableName+''''+','+
        ''''+sTableName+''''+','+
        ''''+copy(sTableName,1,3)+'''';
      qry.Close;
      qry.SQL.Clear;
      qry.SQL.Add(sSQL);
      qry.ExecSQL;

  for i := 0 to tblDset.FieldCount - 1 do
    begin
      sSQL:='insert into CURdTableField(TableName,SerialNum,FieldName,DisplayLabel,DisplaySize)'+
        ' select '+''''+sTableName+''''+','+
        inttostr(i+1)+','+
        ''''+tblDset.FieldList[i].FieldName+''''+','+
        ''''+tblDset.FieldList[i].DisplayLabel+''''+','+
        inttostr(tblDset.FieldList[i].DisplayWidth);
      qry.Close;
      qry.SQL.Clear;
      qry.SQL.Add(sSQL);
      qry.ExecSQL;
    end;

  qry.Close;

  MsgDlgJS('已完成',mtInformation,[mbOK],0);
end;


function funSaveToExcel(frm:TForm;tbl:TJSdTable):boolean;
{var
OutFileName,  sExtName:String;
dlgSave:TSaveDialog;}
begin
  result:=false;

  DsExport(frm,tbl);

{
  if tbl.Active=false then
     begin
       MsgDlgJS('無資料可供處理',mtWarning,[mbOK],0);
       exit;
     end;

  if tbl.RecordCount=0 then
     begin
       MsgDlgJS('無資料可供處理',mtWarning,[mbOK],0);
       exit;
     end;

  dlgSave:=TSaveDialog.Create(nil);
  try
    with dlgSave do
    begin
      Filter :=
            'Excel(*.XLS)|*.XLS|Access(*.mdb)|*.mdb|Paradox(*.db)|*.db|Any(*.*)|*.*';

      FilterIndex := 1;
      if Execute then
      begin
        if pos('.', FileName)<=0 then
        begin
          case filterindex of
            1:OutFileName := FileName+'.xls';
            2:OutFileName := FileName+'.mdb';
            3:OutFileName := FileName+'.db';
            else OutFileName := FileName+'.txt';
          end;
        end;
        sExtName:= lowercase(Copy(OutFileName, pos('.', OutFileName), 10));
        if sExtName='.xls' then
        begin
          if Assigned(pwgSaveToExcel.DataSet) then
          begin
            pwgSaveToExcel.PrintFileName := sItemName;
            pwgSaveToExcel.WriteAllToText(OutFileName);
          end;
        end
        else if sExtName='.db' then
        begin
          DataSet2ParadoxDLL(tbl, OutFileName, tbl.IndexFieldNames);
        end
        else if sExtName='.mdb' then
        begin
          DataSet2AccessDLL(tbl, OutFileName, sItemName);
        end
        else if sExtName='.txt' then
        begin
          if Assigned(pwgSaveToExcel.DataSet) then
          begin
            pwgSaveToExcel.PrintFileName := sItemName;
            pwgSaveToExcel.WriteAllToText(OutFileName);
          end;
        end;
      end;
    end;
   finally
     dlgSave.Free;
   end;
}
end;


procedure BrowseDataDLL(tblDset: TDataset; bRefresh: Boolean);
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
           if ((tblDset is TADOQuery) or (tblDset is TJSdTable)) then
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
           if (oldActive and assigned(bk)) then if tblDset.RecordCount>0 then tblDset.GotoBookMark(bk);
        except
           tblDset.next;
        end;
      except
        raise;
      end;
    finally
      if (oldActive and assigned(bk)) then tblDset.FreeBookMark(bk);
      tblDset.EnableControls;
    end;
end;


function GetMaxSerialNumDLL(tblDset: TDataset; sFieldName: string):integer;
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
      if Assigned(bk) then if Recordcount>0 then GotoBookmark(bk);
      if Assigned(bk) then FreeBookmark(bk);
      EnableControls;
   end;
   Result:= Max;
end;

procedure ActiveXViewerRunDLL(LocaldbName,
  LocalRptName: WideString; AftPrn: TNotifyEVent;JSRpt:TJSdReport);
var i: integer;
    sdbPath: WideString;
    obj, rpt: Variant;
    fm2: TfrmCRActiveXViewer;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
  //crHandle:THandle;
begin
   //crHandle:=LoadLibrary('CRViewer.dll');

   sdbPath:= ExtractFilePath(LocaldbName);
   try
      obj := CreateOleObject('CrystalRuntime.Application');
      rpt := obj.OpenReport(LocalRptName);
      rpt.DiscardSavedData;

      for i := 1 to rpt.DataBase.Tables.count do
      begin
         rpt.DataBase.Tables[i].Location:= LocaldbName;
      end;
      if not assigned(fm2) then
      begin
        Application.createform(TfrmCRActiveXViewer, fm2);
        //2020.03.10
        if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
        begin
            sList:=TstringList.Create;
            sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
            sFontSize:=sList.Strings[0];
            sList.Free;
            FontSize := StrToInt(sFontSize);
            fm2.Scaled:=true;
            fm2.ScaleBy(FontSize,100);
        end;
      end;


      fm2.FormStyle:=fsNormal; //fsMDIChild;
      fm2.cavViewer.ReportSource := rpt;
      fm2.Caption:= JSRpt.ReportTitle; //CurrReportTitle;
      fm2.cavViewer.EnableStopButton := false;
      fm2.cavViewer.DisplayTabs := false;
      fm2.cavViewer.DisplayBackgroundEdge := false;
      fm2.cavViewer.EnableGroupTree := true;
      fm2.cavViewer.EnableNavigationControls:= true;
      fm2.cavViewer.EnableStopButton:= false;
      fm2.cavViewer.EnableCloseButton:= false;
      fm2.cavViewer.EnableRefreshButton:= false;
      fm2.cavViewer.EnableAnimationCtrl:= false;
      fm2.CloseEvent:= AftPrn;
      if JSRpt.ReportDest= rdWindow then
      begin
         fm2.cavViewer.ViewReport;
         fm2.Hide;
         fm2.ShowModal;
      end
      else
      begin
         fm2.cavViewer.PrintReport;
      end;

      rpt:=null;
      obj:=null;
      //obj:= Unassigned;
      //FreeAndNil(obj);

      //FreeLibrary(crHandle);
      //ShowMessage('FreeLibrary(crHandle);');
   except
     Raise;
   end;
end;

function Paper2DataSetDLL(sProcName, sSQLLoop: WideString; dset:TADOQuery;sBUID,sConnectStr:string):Boolean;
var qryLoop, qrytemp: TADOQuery;
  i, j, ifld: Integer;
begin
  try
    try
      Result := false;
      qryLoop := TADOQuery.Create(nil);
      with qryLoop do
      begin
        ConnectionString := sConnectStr;
        CommandTimeout := 9600;
        LockType := ltReadOnly;
        SQL.Clear;
        SQL.Text:= sSQLLoop;
        Open;
        first;
      end;
      qrytemp := TADOQuery.Create(nil);
      with qrytemp do
      begin
        ConnectionString := sConnectStr;
        CommandTimeout := 9600;
        LockType := ltReadOnly;
        qrytemp.Close;
        qrytemp.SQL.Clear;
        qrytemp.SQL.Text:= 'exec '+sProcName+' '''+qryLoop.FieldByName('PaperNum').AsWideString+'''';
        qrytemp.Open;
        qrytemp.SaveToFile(DLLGetTempPathStr+sBUID+'\'+sProcName+'.xml', pfxml);
      end;
      with dset do
      begin
        LockType:= ltOptimistic;
        LoadFromFile(DLLGetTempPathStr+sBUID+'\'+sProcName+'.xml');
      end;
      //Result已經有Loop的第1筆之qrytemp的資料，所以從1開始
      qryLoop.Next;
      for i := 1 to qryLoop.RecordCount - 1 do
      begin
        qrytemp.Close;
        qrytemp.SQL.Clear;
        qrytemp.SQL.Text:= 'exec '+sProcName+' '''+qryLoop.FieldByName('PaperNum').AsWideString+'''';
        qrytemp.Open;
        qrytemp.first;
        for j := 0 to qrytemp.RecordCount - 1 do
        begin
          dset.Append;
          for ifld := 0 to qrytemp.FieldCount - 1 do
          begin
            dset.Fields[ifld].Assign(qrytemp.Fields[ifld]);
          end;
          dset.Post;
          qrytemp.Next;
        end;
        qryLoop.Next;
      end;
      Result:= true;
    except
      Raise;
      Result:= false;
    end;
  finally
    if Assigned(qryLoop) then qryLoop.Free;
    if Assigned(qrytemp) then qrytemp.Free;
  end;
end;


function ShowWinReportDLL(JsRpt:TJSdReport;sProcName, sBUId, sUserId, sIndex, sRptName, sSQLLoop: WideString;
  OutputType, ShowTitle: integer; ParamList: array of WideString; AftPrn: TNotifyEVent;sConnectStr:string;
  hMain_btnPrint_Handle:THandle;//2010.11.8 add
  sMainGlobalId:string; //2010.11.8 add
  iShowModal:integer
  ): Boolean;
var
    ServerRptName, LocalRptName: WideString;
    sdbPath: WideString;
    LocaldbName, OutFileName: WideString;
    strParam, SQLStmts: WideString;
    qryProc, qryTitle: TADOQuery;
    qry: TADOQuery;
    SaveDlg:TSaveDialog;
    sExtName:WideString;
    pwgSaveToExcel:TJSdGrid2Excel;
    sToMainTmpSQL:string;//2010.11.8 add
begin
  try
     Result := False;
     if OutputType=3 then
     begin
       strParam:= '';
       strParam:= GetParamString(sProcName, ParamList,sConnectStr);
       SQLStmts:='EXEC '+sProcName+ ' '+ strParam;

       qry:=TADOQuery.Create(nil);
       qry.CommandTimeout := 9600;

       try
        with qry do
          begin
            ConnectionString:=sConnectStr;
            sql.Add(SQLStmts);
            ExecSQL;
            close;
          end;
       finally
         qry.Free;
       end;

       MsgDlgJS('程式執行完畢！', mtInformation, [mbOk], 0);
       exit;
     end
     else
     begin
        //產生檔案
        try
            qryProc:= TADOQuery.Create(nil);
            qryProc.CommandTimeout := 9600;
            qryProc.EnableBCD:=false;//2010.6.23 add for 解決報表轉出mdb時，decimal DataType 變成 Memo的問題

            qryTitle:= TADOQuery.Create(nil);
            qryTitle.CommandTimeout := 9600;
            qryTitle.EnableBCD:=false;//2010.6.23 add for 解決報表轉出mdb時，decimal DataType 變成 Memo的問題

            sdbPath:= DLLGetTempPathStr+sBUID+'\';

            if sSQLLoop='' then
            begin
              SQLStmts:= Proc2QueryDLL(sProcName, ParamList,sConnectStr);
              Query2DataSetDLL(SQLStmts, qryProc,sConnectStr);
            end
            else
            begin
              SQLStmts:= sProcName;
              Paper2DataSetDLL(sProcName, sSQLLoop, qryProc,sBUID,sConnectStr);
            end;

            //使用crystal，產生標準資訊檔
            if OutputType in [0, 1, 2] then
            begin
              SQLStmts:= Proc2QueryDLL('CURdReportTitle', [sBUId, inttostr(showTitle)],sConnectStr);

              Query2DataSetDLL(SQLStmts, qryTitle,sConnectStr);
            end;

          if OutputType in [0, 1, 2] then
            begin
               LocaldbName:= trim(sdbPath+sProcName)+'.mdb';
               LocaldbName:= GetNoLockFileNameDLL(LocaldbName, '_');
               DataSet2AccessDLL(qryTitle, LocaldbName, 'CURdReportTitle');
               DataSet2AccessDLL(qryProc, LocaldbName, sProcName, sConnectStr); //2017.08.28 EMO
            end;

        if OutputType=4 then
        begin
           SaveDlg:=TSaveDialog.Create(nil);
           try
              with SaveDlg do
              begin
                 Filter:='Excel(*.XLSX)|*.XLSX|Access(*.mdb)|*.mdb|Any(*.*)|*.*';

                 if sdbPath <> '' then InitialDir := sdbPath;

                 FileName := sProcName;

                 FilterIndex := 1;

                 if Execute then
                 begin
                    case FilterIndex of
                      1:OutFileName := FileName+'.xlsx';
                      2:OutFileName := FileName+'.mdb';
                      else OutFileName := FileName+'.txt';
                    end;

                    case FilterIndex of
                      1:DataSet2ExcelDLL(qryProc, OutFileName, sProcName, sRptName, 1,sConnectStr);
                      2:DataSet2AccessDLL(qryProc, OutFileName, sProcName);
                      else
                      begin
                        try
                        pwgSaveToExcel:=TJSdGrid2Excel.Create(nil);
                        pwgSaveToExcel.DataSet:=qryProc;
                        pwgSaveToExcel.PrintFileName :=sProcName;
                        pwgSaveToExcel.WriteAllToText(OutFileName);
                        finally
                        pwgSaveToExcel.Free;
                        end;
                      end;
                    end;

                 end;//if Execute then
              end;//with SaveDlg do
           finally
              SaveDlg.Free;
           end;//SaveDlg:=TSaveDialog.Create(nil);
        end;//if OutputType=4 then

        if OutputType=5 then
        begin
            DataSet2ExcelDLL(qryProc, LocaldbName, sProcName, sRptName, 0,sConnectStr);
        end;

        finally
          if assigned(qryProc) then qryProc.Free;
          if assigned(qryTitle) then qryTitle.Free;
        end;////產生檔案
     end;

     //OutputType:0=windows, 1=rinter, 2=User select
     if OutputType in [0, 1, 2] then
     begin
       case OutputType of
       0: begin
            JsRpt.ReportDest:= rdWindow;
            JsRpt.PrintDirect:= true;
          end;
       1: begin
            JsRpt.ReportDest:= rdPrinter;
            JsRpt.PrintDirect:= true;
          end;
       2: begin
            JsRpt.ReportDest:= rdWindow;
            JsRpt.PrintDirect:= false;
          end;
       end;

       ServerRptName:=JsRpt.ReportServer+ trim(JsRpt.ReportFileName);
       LocalRptName:= unit_DLL.DLLGetTempPathStr+sBUID+'\'+ trim(JsRpt.ReportFileName);
       SyncFileStrDLL(ServerRptName, LocalRptName);

      if VarIsNull(hMain_btnPrint_Handle) then hMain_btnPrint_Handle:=0;

      if (hMain_btnPrint_Handle<>0) and (iShowModal=0) then
        begin
          //寫入後端橋接檔
          sToMainTmpSQL:='exec CURdReportToMainTmpIns '+
            ''''+sMainGlobalId+''''+','+
            'N'+''''+LocaldbName+''''+','+
            'N'+''''+LocalRptName+''''+','+
            'N'+''''+JsRpt.ReportTitle+''''
            ;

          qry:=TADOQuery.Create(nil);
          qry.CommandTimeout := 9600;

          try
            with qry do
            begin
              ConnectionString:=sConnectStr;
              sql.Add(sToMainTmpSQL);
              ExecSQL;
              close;
            end;
          finally
            qry.Free;
          end;

          //Call Main Form 執行
          SendMessage(hMain_btnPrint_Handle,WM_LBUTTONDOWN, 0, 0);
          SendMessage(hMain_btnPrint_Handle,WM_LBUTTONUP, 0, 0);
        end
        else
        begin
          ActiveXViewerRunDLL(LocaldbName, LocalRptName, AftPrn,JsRpt);
        end;
    end;

    Result := true;
  except
    Result := False;
    Raise;
  end;
end;

function RunReportAftPrnDLL(JsRpt:TJSdReport;sProcName, sBUId, sUserId, sIndex, sRptName, sSQLLoop: WideString;
  OutputType, ShowTitle: integer; ParamList: array of WideString; AftPrn: TNotifyEVent;sConnectStr:string;
  hMain_btnPrint_Handle:THandle;//2010.11.8 add
  sMainGlobalId:string; //2010.11.8 add
  iShowModal:integer
  ): Boolean;
begin
  try
    Result := False;

    if JsRpt.DisplayType=dtWEB then
    begin
      //ShowWEBReportDLL(sProcName, sBUId, sUserId, sIndex, sRptName, sSQLLoop, OutputType, ShowTitle, ParamList, AftPrn);
    end
    else
    begin
      ShowWinReportDLL(JsRpt,sProcName, sBUId, sUserId, sIndex, sRptName, sSQLLoop, OutputType, ShowTitle, ParamList, AftPrn,sConnectStr,
        hMain_btnPrint_Handle, //2010.11.8 add
        sMainGlobalId, //2010.11.8 add
        iShowModal
        );
    end;

    Result := true;
  except
    Result := False;
    Raise;
  end;
end;

function RunJSdReportDLL(
      JsRpt: TJSdReport;
      ProcName: WideString;
      ParamList: array of WideString;
      sIndex,
      sRptName: WideString;
      //=====
      qryExec:TADOQuery;
      sSystemId,
      sBUId,
      sUserId:string;
      AftPrn: TNotifyEVent;
      bSetUpByRpt:boolean;
      //=====
      hMain_btnPrint_Handle:THandle;
      sMainGlobalId:string; //2010.11.8 add
      iShowModal:integer //2010.11.8 add
      ): Boolean;
var sLink, sDisplay: WideString;
    tLink: TLinkType;
    tDisplay: TDisplayType;
    sWRServer, sWRShare, sWRLocal: WideString;
    sOutputType: WideString; iOutputType: integer;
    sShowTitle: WideString; iShowTitle: integer;
    sSQL:string;
begin
  sSQL:='exec CURdOCXSysRptInfoGet '+
    ''''+sSystemId+''''+','+
    ''''+sBUID+'''';

  with qryExec do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qryExec.RecordCount=0 then
    begin
      qryExec.Close;
      MsgDlgJS('系統['+sSystemId+']報表設定有誤',mtError,[mbOk],0);
      exit;
    end;

  with qryExec do
    begin
      sWRServer:=fieldbyname('WEBReportServer').AsString;
      sWRShare:=fieldbyname('WEBReportShare').AsString;
      sWRLocal:=fieldbyname('WEBReportLocal').AsString;
      sLink:= fieldbyname('LinkType').AsString;
      sDisplay:= fieldbyname('DisplayType').AsString;
      sOutputType:= fieldbyname('OutPutType').AsString;
      sShowTitle:= fieldbyname('ShowTitle').AsString;
    end;

   if sOutputType='' then
      iOutputType:= 0
   else
      iOutputType:= strtoint(sOutputType);

   if sShowTitle='' then
      iShowTitle:= 0
   else
      iShowTitle:= strtoint(sShowTitle);

   if UpperCase(trim(sDisplay)) = 'DTDLL' then
   begin
     tDisplay := dtDll;
   end
   else if UpperCase(trim(sDisplay)) = 'DTWEB' then
   begin
     tDisplay := dtWeb;
   end
   else
   begin
     tDisplay := dtActiveX;
   end;

   if UpperCase(trim(sLink)) = 'LTODBC' then
      tLink:= ltODBC
   else if UpperCase(trim(sLink)) = 'LTBDE' then
      tLink:= ltBDE
   else if UpperCase(trim(sLink)) = 'LTACCESS' then
      tLink:= ltAccess
   else if UpperCase(trim(sLink)) = 'LTEXCEL' then
      tLink:= ltExcel;

   if trim(funDLLSysParamsGet(qryExec,sSystemId,'WEBReportAlways'))='true' then
   begin
      tDisplay:= dtWEB;
   end;

   with JsRpt do
   begin
      //ReportServer := unit_DLL.funReportServerGet(qryExec,sBUID);
      ReportServer := unit_DLL.funReportServerGet(qryExec,sBUID,sMainGlobalId);//2012.05.22 modify for WF Bill-20120518-05
      //LinkType := tLink;
      DisplayType := tDisplay;
      WEBReportServer:= sWRServer;
      WEBReportShare:= sWRShare;
      WEBReportLocal:= sWRLocal;
   end;

   if bSetUpByRpt=false then
     JsRpt.LinkType:=tLink;

   RunReportAftPrnDLL(
        JsRpt,
        ProcName,
        sBUId,
        sUserId,
        sIndex,
        JsRpt.ReportFileName,
        '',
        iOutputType,
        iShowTitle,
        ParamList,
        AftPrn,
        qryExec.ConnectionString,
        hMain_btnPrint_Handle,
        sMainGlobalId, //2010.11.8 add
        iShowModal
        );
end;

function RightDLL(Patten:String; count:integer) : String;

begin
  Result:= Copy(Patten, Length(Patten)-Count+1, Count);
end;

function GetNoLockFileNameDLL(sFileName, adds:WideString): WideString;
var stmp, fileFirst: WideString;
    i : Integer;
begin
    i := 0;
    stmp := sFileName;
    while FileExists(stmp) do
    begin
      if SysUtils.DeleteFile(stmp) then
      begin
        Break;
      end
      else
      begin
        i := i + 1;
        fileFirst := RightDLL(adds+IntToStr(i),5);
        stmp:= ExtractFilePath(sFileName)+fileFirst+ExtractFileName(sFileName);
      end;
    end;
    Result:= stmp;
end;

//function funReportServerGet(qryExec:TADOQuery;sBUID:string):string;
function funReportServerGet(qryExec:TADOQuery;sBUID,sGlobalId:string):string;//2012.05.22 modify for WF Bill-20120518-05
var sReportServer:string;
begin
  sReportServer:='';

  with qryExec do
    begin
      close;
      sql.Clear;
      //sql.Add('select ReportServer from CURdBU(nolock) where BUID='+''''+sBUID+'''');
      sql.Add('exec CURdServerInfoGet '+''''+sBUID+''''+','+''''+sGlobalId+'''');//2012.05.22 modify for WF Bill-20120518-05
      open;
    end;

  if qryExec.RecordCount>0 then
    sReportServer:=qryExec.FieldByName('ReportServer').AsString;

  qryExec.Close;

  result:=sReportServer;
end;

function IsEqualDLL2(sr1, sr2: TSearchRec): Boolean;
begin
   if ( //(sr1.Name = sr2.Name) and
       (sr1.Time = sr2.Time) and
       (sr1.Size = sr2.Size)) then
   begin
      Result := True;
   end
   else
   begin
      Result := False;
   end;
end;

function IsEqualDLL(sr1, sr2: TSearchRec): Boolean;
begin
   if ( (sr1.Name = sr2.Name) and
       (sr1.Time = sr2.Time) and
       (sr1.Size = sr2.Size)) then
   begin
      Result := True;
   end
   else
   begin
      Result := False;
   end;
end;

function CopyFileStrDLL(sSrc, sDest: String): Boolean;
var sDirToCreate: String;
    bCopyRslt: Boolean;
begin
  Result := True;
  try
    sDirToCreate:= ExtractFilePath(sDest);

    if DirectoryExists(sDirToCreate)=false then
      ForceDirectories(sDirToCreate);

    bCopyRslt:= CopyFile(Pchar(sSrc), Pchar(sDest), false);

    if not bCopyRslt then
      Result := false;

  except
    Result := false;
  end;
end;

function SyncFileStrDLL2(sSrc, sDest: WideString): Boolean;
var sr1, sr2: TSearchRec;
    UpdateSuccess, ServerExist, needupdate: Boolean;
    j,k:longint;
begin
  UpdateSuccess:= true;

  needupdate:= false;

  if Fileexists(sSrc) then
  begin
     if Fileexists(sDest) then
     begin
        j:=FindFirst(sSrc, faAnyFile, sr1);
        k:=FindFirst(sDest, faAnyFile, sr2);

        if not IsEqualDLL2(sr1, sr2) then
           needupdate:= true;

        //FindClose(j);
        //FindClose(k);
        SysUtils.FindClose(sr1);
        SysUtils.FindClose(sr2);

        if needupdate then
           UpdateSuccess:= CopyFileStrDLL(sSrc, sDest);
     end
     else
        UpdateSuccess:= CopyFileStrDLL(sSrc, sDest);

    if not UpdateSuccess then
    begin
       MsgDlgJS('ERP系統需要做自動更新，請關閉ERP系統後重新開啟', mtWarning, [mbOK], 0);
       Result:=false;
       exit;
    end;
  end
  else
  begin
    MsgDlgJS('來源檔案['+sSrc+']不存在！', mtError, [mbOK], 0);
    Result:=false;
    exit;
  end;

  if not Fileexists(sDest) then
    begin
      MsgDlgJS('檔案同步至['+sDest+']失敗！', mtError, [mbOK], 0);
      Result:=false;
      exit;
    end;

  Result:= true;
end;


function SyncFileStrDLL(sSrc, sDest: WideString): Boolean;
var sr1, sr2: TSearchRec;
    UpdateSuccess, ServerExist, needupdate: Boolean;
    j,k:longint;
begin
  UpdateSuccess:= true;

  needupdate:= false;

  if Fileexists(sSrc) then
  begin
     if Fileexists(sDest) then
     begin
        j:=FindFirst(sSrc, faAnyFile, sr1);
        k:=FindFirst(sDest, faAnyFile, sr2);

        if not IsEqualDLL2(sr1, sr2) then
           needupdate:= true;

        //FindClose(j);
        //FindClose(k);
        SysUtils.FindClose(sr1);
        SysUtils.FindClose(sr2);

        if needupdate then
           UpdateSuccess:= CopyFileStrDLL(sSrc, sDest);
     end
     else
        UpdateSuccess:= CopyFileStrDLL(sSrc, sDest);

    if not UpdateSuccess then
    begin
       MsgDlgJS('['+sSrc+']檔案同步失敗！', mtError, [mbOK], 0);
       Result:=false;
       exit;
    end;
  end
  else
  begin
    MsgDlgJS('來源檔案['+sSrc+']不存在！', mtError, [mbOK], 0);
    Result:=false;
    exit;
  end;

  if not Fileexists(sDest) then
    begin
      MsgDlgJS('檔案同步至['+sDest+']失敗！', mtError, [mbOK], 0);
      Result:=false;
      exit;
    end;

  Result:= true;

  {Result:= false;
  UpdateSuccess:= false;
  ServerExist:= false;
  needupdate:= false;

  if Fileexists(sSrc) then
  begin
     ServerExist:= true;

     if Fileexists(sDest) then
     begin
        j:=FindFirst(sSrc, faAnyFile, sr1);
        k:=FindFirst(sDest, faAnyFile, sr2);

        if not IsEqualDLL(sr1, sr2) then
           needupdate:= true;

        FindClose(j);
        FindClose(k);

        if needupdate then
           UpdateSuccess:= CopyFileStrDLL(sSrc, sDest);
     end
     else
        UpdateSuccess:= CopyFileStrDLL(sSrc, sDest);
  end;
  Result:= UpdateSuccess;
  if needupdate then
    if not(UpdateSuccess) then
       MsgDlgJS('['+sSrc+']檔案同步失敗！', mtError, [mbOK], 0);

  if not Fileexists(sDest) then
  begin
    if not ServerExist then
       MsgDlgJS('來源檔案['+sSrc+']不存在！', mtError, [mbOK], 0)
    else
       MsgDlgJS('檔案同步至['+sDest+']失敗！', mtError, [mbOK], 0);

    Exit;
  end;}
end;

function DataSet2AccessDLL(data:TCustomADODataSet; sdbFile, sTableName: string): Boolean;
var I, J, icnt: Integer;
    tblDest: TADOQuery;
    ADOCon: TADOConnection;
    ADOCommand: TADOCommand;
    dbTableExist:Boolean;
    lstTable: TStringList;
begin
  Result := False;
  try
    try
      data.DisableControls;
      CreateAccessDBFile(sdbFile, '');
      ADOCon := TADOConnection.Create(nil);
      ADOCon.ConnectionString := format(AccessConnStr, [sdbFile, '']);
      ADOCon.LoginPrompt:= false;
      ADOCon.Open;
      try
        dbTableExist:= false;
        lstTable:= TStringList.Create;
        ADOCon.GetTableNames(lstTable, false);
        dbTableExist:= lstTable.IndexOf(sTableName)>=0;
      finally
        lstTable.Free;
      end;
      ADOCommand:= TADOCommand.Create(nil);
      ADOCommand.Connection:= ADOCon;
      ADOCommand.ExecuteOptions:= [eoExecuteNoRecords];
      if dbTableExist then
      begin
        ADOCommand.CommandText:=' drop table '+ sTableName;
        ADOCommand.Execute;
      end;
      ADOCommand.CommandText:=CmdCreateAccessTable(sTableName, data);
      ADOCommand.Execute;

      tblDest := TADOQuery.Create(nil);
      with tblDest do
      begin
        Connection := ADOCon;
        CommandTimeout := 9600;
        LockType := ltOptimistic;
        SQL.Clear;
        SQL.Text:= 'Select * from '+sTableName;
        Open;
      end;
      //create Access table end
      J := 0;
      frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(nil);
      frmLoadProgressDLL.Caption:= '資料計算彙整中......';
      frmLoadProgressDLL.show;
      with data do
      begin
         First;
         if data.RecordCount>100 then
            frmLoadProgressDLL.Initialize(0, data.RecordCount);
         while not Eof do
         begin
            Inc(J);
            tblDest.open;
            tblDest.Append;
            for i := 0 to (FieldCount - 1) do
            begin
               //tblDest.Fields[i].Assign(Fields[i]);
               if data.Fields[i].DataType = ftWideString then
                  tblDest.FieldbyName(data.Fields[i].FieldName).AsWideString:= data.Fields[i].AsWideString
               else if data.Fields[i].DataType = ftWord then
                  tblDest.FieldbyName(data.Fields[i].FieldName).AsInteger:= data.Fields[i].AsInteger
               else
                  tblDest.FieldbyName(data.Fields[i].FieldName).Assign(Fields[i])
            end;
            tblDest.Post;
            Next;
            frmLoadProgressDLL.Add;
            frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(J) + ' 筆資料';
            frmLoadProgressDLL.pnlStatus.Refresh;
         end;
      end;
      Result := True;
      tblDest.Close;
      tblDest.Open;
    except
      Result := False;
      Raise;
    end;
  finally
    if assigned(frmLoadProgressDLL) then
    begin
      frmLoadProgressDLL.free;
    end;
    if assigned(ADOCommand) then
    begin
      ADOCommand.Free;
    end;
    if assigned(tblDest) then
    begin
      tblDest.Close;
      tblDest.Free;
    end;
    if assigned(ADOCon) then
    begin
      ADOCon.Close;
      ADOCon.Free;
    end;
    data.First;
    data.EnableControls;
  end;
end;

function DataSet2AccessDLL(data:TCustomADODataSet; sdbFile, sTableName,sConnectStr:string): Boolean;
var I, J, icnt: Integer;
    tblDest: TADOQuery;
    ADOCon: TADOConnection;
    ADOCommand: TADOCommand;
    dbTableExist:Boolean;
    lstTable: TStringList;
    //2017.08.28
    QRCode: TDelphiZXingQRCode;
    QRCodeBitmap: TBitmap;
    Row, Column, iZone: Integer;
    QRStream: TStream;
    qrytemp: TADOQuery;
    sQRSource1, sQRSource2, sQRField1, sQRField2, sTmpField: String;
begin
  Result := False;
  try
    try
      data.DisableControls;
      CreateAccessDBFile(sdbFile, '');
      ADOCon := TADOConnection.Create(nil);
      ADOCon.ConnectionString := format(AccessConnStr, [sdbFile, '']);
      ADOCon.LoginPrompt:= false;
      ADOCon.Open;
      try
        dbTableExist:= false;
        lstTable:= TStringList.Create;
        ADOCon.GetTableNames(lstTable, false);
        dbTableExist:= lstTable.IndexOf(sTableName)>=0;
      finally
        lstTable.Free;
      end;
      ADOCommand:= TADOCommand.Create(nil);
      ADOCommand.Connection:= ADOCon;
      ADOCommand.ExecuteOptions:= [eoExecuteNoRecords];
      if dbTableExist then
      begin
        ADOCommand.CommandText:=' drop table '+ sTableName;
        ADOCommand.Execute;
      end;
      ADOCommand.CommandText:=CmdCreateAccessTable(sTableName, data);
      ADOCommand.Execute;

      tblDest := TADOQuery.Create(nil);
      with tblDest do
      begin
        Connection := ADOCon;
        CommandTimeout := 9600;
        LockType := ltOptimistic;
        SQL.Clear;
        SQL.Text:= 'Select * from '+sTableName;
        Open;
      end;
      //create Access table end
      J := 0;
      frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(nil);
      frmLoadProgressDLL.Caption:= '資料計算彙整中......';
      frmLoadProgressDLL.show;

      //2017.08.28
      //=================================================
      sQRField1:='';
      sQRField2:='';
      qrytemp := TADOQuery.Create(nil);
      with qrytemp do
      begin
        ConnectionString := sConnectStr;
        CommandTimeout := 9600;
        LockType := ltReadOnly;
        qrytemp.Close;
        qrytemp.SQL.Clear;
        qrytemp.SQL.Text:= 'select Value from CURdSysParams(nolock)'
          +' where SystemId=''SPO'' and ParamId=''RPTQRCode'''
          +' and IsNull(Value,'''')=''1''';
        qrytemp.Open;
        if RecordCount>0 then
        begin
          qrytemp.Close;
          qrytemp.SQL.Clear;
          qrytemp.SQL.Text:= 'exec SPOdQRCodeField '''+sTableName+'''';
          qrytemp.Open;
          if RecordCount>0 then
          begin
            sQRSource1:=fieldByName('QRSource1').AsString;
            sQRSource2:=fieldByName('QRSource2').AsString;
            sQRField1:=fieldByName('QRField1').AsString;
            sQRField2:=fieldByName('QRField2').AsString;
            iZone:=fieldByName('Zone').AsInteger;
          end;
        end;
      end;
      //=================================================

      with data do
      begin
         First;
         if data.RecordCount>100 then
            frmLoadProgressDLL.Initialize(0, data.RecordCount);
         while not Eof do
         begin
            Inc(J);
            tblDest.open;
            tblDest.Append;
            for i := 0 to (FieldCount - 1) do
            begin
               //tblDest.Fields[i].Assign(Fields[i]);
               //2017.08.28
               //=================================================
               if (
                    ( (data.Fields[i].FieldName = sQRField1)
                      and
                      (sQRField1<>'') )
                    or ( (data.Fields[i].FieldName = sQRField2)
                       and
                       (sQRField2<>'') )
               ) then
               begin
                    if data.Fields[i].FieldName = sQRField1 then
                      sTmpField:= sQRSource1
                    else
                      sTmpField:= sQRSource2;

                    QRCode := TDelphiZXingQRCode.Create;
                    try
                      QRCode.Data := tblDest.FieldbyName(sTmpField).AsWideString;
                      QRCode.Encoding := TQRCodeEncoding(qrUTF8BOM);
                      QRCode.QuietZone := StrToIntDef(IntToStr(iZone), 4);
                      QRCodeBitmap := TBitmap.Create;
                      QRCodeBitmap.SetSize(QRCode.Rows, QRCode.Columns);
                      for Row := 0 to QRCode.Rows - 1 do
                      begin
                        for Column := 0 to QRCode.Columns - 1 do
                        begin
                          if (QRCode.IsBlack[Row, Column]) then
                          begin
                            QRCodeBitmap.Canvas.Pixels[Column, Row] := clBlack;
                          end else
                          begin
                            QRCodeBitmap.Canvas.Pixels[Column, Row] := clWhite;
                          end;
                        end;
                      end;
                      QRStream:= TMemoryStream.Create;
                      QRCodeBitmap.SaveToStream(QRStream);
                      TBlobField(tblDest.FieldbyName(data.Fields[i].FieldName)).
                        LoadFromStream(QRStream);
                    finally
                      QRCode.Free;
                      QRCodeBitmap.Free;
                    end;
               end
               //=================================================
               else
               begin
                 if data.Fields[i].DataType = ftWideString then
                    tblDest.FieldbyName(data.Fields[i].FieldName).AsWideString:= data.Fields[i].AsWideString
                 else if data.Fields[i].DataType = ftWord then
                    tblDest.FieldbyName(data.Fields[i].FieldName).AsInteger:= data.Fields[i].AsInteger
                 else
                    tblDest.FieldbyName(data.Fields[i].FieldName).Assign(Fields[i])
               end;
            end;
            tblDest.Post;
            Next;
            frmLoadProgressDLL.Add;
            frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(J) + ' 筆資料';
            frmLoadProgressDLL.pnlStatus.Refresh;
         end;
      end;
      Result := True;
      tblDest.Close;
      tblDest.Open;
    except
      Result := False;
      Raise;
    end;
  finally
    if assigned(frmLoadProgressDLL) then
    begin
      frmLoadProgressDLL.free;
    end;
    if assigned(ADOCommand) then
    begin
      ADOCommand.Free;
    end;
    if assigned(tblDest) then
    begin
      tblDest.Close;
      tblDest.Free;
    end;
    if assigned(ADOCon) then
    begin
      ADOCon.Close;
      ADOCon.Free;
    end;
    data.First;
    data.EnableControls;
  end;
end;

function DataSet2ParadoxDLL(data:TCustomADODataSet; sdbFile, IndexFiled: string): Boolean;
var I, J, icnt: Integer;
    SecParadox: TSession;
    dbLocal: TDatabase;
    tblDest: TTable;
    dbPh: WideString;
begin
  Result := False;
  try
    try
      data.DisableControls;
      dbPh:= ExtractFilePath(sdbFile);
      frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(nil);
      frmLoadProgressDLL.Caption:= '資料計算彙整中......';
      SecParadox:= TSession.Create(nil);
      with SecParadox do
      begin
        NetFileDir := dbPh;
        SessionName := 'SecParadox';
      end;
      dbLocal:= TDatabase.Create(nil);
      with dbLocal do
      begin
        DatabaseName := 'dbLocal';
        DriverName := 'STANDARD';
        HandleShared := True;
        KeepConnection := False;
        LoginPrompt := False;
        Params.Add('DEFAULT DRIVER=PARADOX');
        Params.Add('ENABLE BCD=FALSE');
        SessionName := 'SecParadox';
      end;
      tblDest := TTable.Create(nil);
      with tblDest do
      begin
        DatabaseName := 'dbLocal';
        FilterOptions := [foCaseInsensitive];
        SessionName := 'SecParadox';
        UpdateMode := upWhereKeyOnly;
        TableType := ttParadox;
        if not DirectoryExists(dbPh) then
        begin
          ForceDirectories(dbPh);
          if not DirectoryExists(dbPh) then
          begin
             Raise Exception.Create('Create failure '+dbPh);
          end;
        end;
        TableName := sdbFile;
        if exists then
          deletetable;
      end;

      with tblDest do
      begin
        tblDest.FieldDefs.Clear;
        FieldDefs.Assign(data.FieldDefs);
        if IndexFiled='' then
        begin
           IndexDefs.Clear;
        end
        else
        begin
           IndexFieldNames:=IndexFiled;
           with IndexDefs do
           begin
             Clear;
             with AddIndexDef do
             begin
               Name := IndexFiled;
               Fields := IndexFiled ;
               Options := [ixPrimary];
             end;
           end;
        end;
        CreateTable;
      end;
      J := 0;
      frmLoadProgressDLL.show;

       with data do
       begin
         First;
         if data.RecordCount>100 then
            frmLoadProgressDLL.Initialize(0, data.RecordCount);
         while not Eof do
         begin
            Inc(J);
            tblDest.open;
            tblDest.Append;
            for i := 0 to (FieldCount - 1) do
            begin
               tblDest.FieldbyName(data.Fields[i].FieldName).Assign(data.Fields[i]);
            end;
            tblDest.Post;
            Next;
            frmLoadProgressDLL.Add;
            frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(J) + ' 筆資料';
            frmLoadProgressDLL.pnlStatus.Refresh;
         end;
      end;
      Result := True;
    except
      Result := False;
      Raise;
    end;
  finally
    frmLoadProgressDLL.free;
    tblDest.Close;
    tblDest.Free;
    dbLocal.Connected:= false;
    dbLocal.Free;
    SecParadox.Active:= false;
    SecParadox.Free;
    data.First;
    data.EnableControls;
  end;
end;


procedure ItemHelpGetDLL(sItemId, sCap:string;qry:TADOQuery);

var ffn: TfrmFunctionNotes2;
    sSQL:String;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
    sSQL:= 'Select convert(varchar(4000),isnull(Notes,'+''''+''''+')) from CURdSysItems(nolock)'
        + ' where ItemId='''+sItemId+''' ';

      with qry do
        begin
          if active then close;
          sql.Clear;
          sql.Add(sSQL);
          Open;
        end;

    Application.CreateForm(TfrmFunctionNotes2, ffn);
    //2020.03.10
    if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
    begin
        sList:=TstringList.Create;
        sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
        sFontSize:=sList.Strings[0];
        sList.Free;
        FontSize := StrToInt(sFontSize);
        ffn.Scaled:=true;
        ffn.ScaleBy(FontSize,100);
    end;
    ffn.CurrItemId:= sItemId;
    ffn.meoFunctionNotes.Lines.Text:= qry.Fields[0].Value;
    ffn.Caption:= sCap+'-功能說明';

    if qry.ConnectionString='' then
      ffn.qryExec.Connection:=qry.Connection
    else
      ffn.qryExec.ConnectionString:=qry.ConnectionString;

    ffn.ShowModal;
end;

function FormatParamDLL(ctrl: TControl; CurrLinkType, CurrDisplayType:integer;sConnectStr:string): WideString;
begin
    if (ctrl is TEdit) then //2009.12.11 add by Garfield
    begin
       Result:= trim(TEdit(ctrl).text);
    end else
    if (ctrl is TMaskEdit) then
    begin
       Result:= trim(TMaskEdit(ctrl).text);
    end else
    if (ctrl is TJSdLookupCombo) then
    begin
       Result:= trim(TJSdLookupCombo(ctrl).text);
    end else
    if (ctrl is TComboBox) then
    begin
       Result:= trim(TComboBox(ctrl).text);
    end else
    if (ctrl is TwwDBDateTimePicker) then
    begin
       if (CurrLinkType=0) and (CurrDisplayType=0) then
       begin
          if TwwDBDateTimePicker(ctrl).text='' then
             Result:=''
          else
             Result:= trim(FormatDateTime('yyyy,mm,dd hh:nn:ss',
                                  TwwDBDateTimePicker(ctrl).datetime));
       end
       else if (CurrLinkType=0) and (CurrDisplayType=1) then
       begin
          if TwwDBDateTimePicker(ctrl).text='' then
             Result:=''
          else
             Result:= trim('DateTime('+FormatDateTime('yyyy,mm,dd,hh,nn,ss',
                                  TwwDBDateTimePicker(ctrl).datetime)+')');
       end
       else
       begin
          if TwwDBDateTimePicker(ctrl).text='' then
             Result:=''
          else
             Result:= trim(FormatDateTime('mm/dd/yyyy HH:nn:ss',
                                  TwwDBDateTimePicker(ctrl).datetime));
       end;
    end
    else
    if (ctrl is TRadioGroup) then
    begin
       if TRadioGroup(ctrl).ItemIndex=2 then
          Result:='255'
       else
          Result:=inttostr(TRadioGroup(ctrl).ItemIndex);
    end;
end;


function GetParamString(ProceName: WideString;
  ParamList: array of WideString;sConnectStr:string): WideString;
var dbQuery: TADOQuery;
    iCnt: Integer;
    sTmp, sRpls: WideString;
begin
  Result:= '';
  dbQuery := TADOQuery.Create(nil);
  try
    try
      with dbQuery do
      begin
        ConnectionString := sConnectStr;
        LockType:= ltReadOnly;
        SQL.Add('exec CURdGetParamType '''+ProceName+'''');
        Open;

        first;

        iCnt:=0;

        while not eof do
        begin
          if (Result<>'') then
             Result:=Result+',';

          if Fieldbyname('Quote').AsInteger=1 then
          begin
            if Length(ParamList[iCnt])<128 then
            begin
              if pos('''', ParamList[iCnt])>0 then
              begin
                 sTmp:= '['+trim(ParamList[iCnt])+']';
              end
              else
                 sTmp:= 'N'+''''+trim(ParamList[iCnt])+'''';

              Result:=Result + Fieldbyname('ParamName').asString+'='
                    +sTmp;
            end
            else //長度有128的限制
            begin
              sTmp:= AnsiReplaceText(ParamList[iCnt], '''', '''''');
              sTmp:= 'N'+''''+sTmp+'''';
              Result:=Result + Fieldbyname('ParamName').asString+'='
                    +sTmp;
            end;
          end
          else
          begin
            if ParamList[iCnt]='' then
               sTmp:= '255'
            else
               sTmp:= ParamList[iCnt];
            Result:=Result + Fieldbyname('ParamName').asString+'='
                  +sTmp;
          end;
          Next;
          iCnt:=iCnt+1;
        end;
      end;
    except
       on E: Exception do Raise;
    end;
  finally
    dbQuery.Close;
    dbQuery.Free;
  end;
end;

function Proc2QueryDLL(ProcName:WideString; AParams: Array of WideString;sConnectStr:string): WideString;
var strParam: WideString;
begin
  try
    Result := '';
    strParam:= '';
    strParam:= GetParamStringDLL(ProcName, AParams,sConnectStr);
    Result:= 'EXEC '+ProcName+ ' '+ strParam;
  except
    Result := '';
  end;
end;

function xlsColToNumDLL(ColName: WideString): Integer;
begin
  if Length(ColName) = 1 then
    Result := ord(ColName[1]) - ord('A') + 1
  else
    Result := (ord(ColName[1]) - ord('A') + 1) * 26 + ord(ColName[2]) - ord('A') + 1;
end;

function NumToxlsColDLL(ColNum: Integer): WideString;
begin
  if ColNum <= 26 then
    Result := Chr(Ord('A') + ColNum - 1)
  else
    Result := Chr(Ord('A') + (ColNum - 1) div 26 - 1) + Chr(Ord('A') + (ColNum - 1) mod 26);
end;

procedure ParseFormatWDLL(sInput, sDim: WideString; var sParam : array of WideString; iPara:integer);
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

function  IIFIntegerDLL(bYes:Boolean; str1, str2: String): Integer;
begin
  if bYes then
    Result := strtoint(str1)
  else
    Result := strtoint(str2);
end;

function  IIFStringDLL(bYes:Boolean; str1, str2: String): String;
begin
  if bYes then
    Result := str1
  else
    Result := str2;
end;

procedure SetReportFieldDLL(dbset: TCustomADODataSet; sReportName, Conn: WideString);
var sItems, sfmt, slab, sNodboName, sFieldName: WideString;
    sFontName, sFontColor, sFontSize, sFontStyle: WideString;
    i, j, k, ivis, iSN, iDisplaySize, iFontColor: integer;
    iReportGroup, iSubTotal, iAutofit, iOrderBy: integer;
    dbQuery: TADOQuery;
    LkResults: Variant;
begin
  sNodboName:= StringReplace(sReportName, 'dbo.', '', [rfReplaceAll, rfIgnoreCase]);
  dbQuery := TADOQuery.Create(nil);
  try
    with dbQuery do
    begin
      ConnectionString := Conn;
      CommandTimeout := 9600;
      LockType := ltReadOnly;
      SQL.Add('Select * from CURdReportField(nolock) where ReportName ='''+sNodboName+'''');
      Open;
      if dbQuery.RecordCount=0 then Exit;
    end;

    with TCustomADODataSet(dbset) do
    begin
      DisableControls;
      for i := 0 to FieldCount - 1 do
      begin
        sFieldName:= Fields[i].FieldName;
        slab:= Fields[i].DisplayLabel;
        if (Fields[i] is TFloatField) then
           sfmt:= TFloatField(Fields[i]).DisplayFormat
        else
           sfmt:= Fields[i].EditMask;

        ivis:= IIFIntegerDLL(Fields[i].Visible, '1', '0');

        iSN:= Fields[i].Index;
        sFontName:= '';
        sFontSize:= '';
        sFontColor:= '';
        sFontStyle:= '';

        iSubTotal:= 0;
        iReportGroup:= 0;
        iAutofit:= 0;
        iOrderBy:= 1;
        if dbQuery.Locate('ReportName;FieldName',
                        VarArrayOf([sNodboName, sFieldName]),
                        [loPartialKey]) then
        begin
          slab:= trim(dbQuery.FieldByName('DisplayLabel').AsWideString);
          sfmt:= trim(dbQuery.FieldByName('FormatStr').AsWideString);
          ivis:= dbQuery.FieldByName('Visible').AsInteger;
          iSN:= dbQuery.FieldByName('SerialNum').AsInteger-1;
          iDisplaySize:= dbQuery.FieldByName('DisplaySize').AsInteger;
          iSubTotal:= dbQuery.FieldByName('SubTotal').AsInteger;
          iReportGroup:= dbQuery.FieldByName('ReportGroup').AsInteger;
          iOrderBy:= dbQuery.FieldByName('OrderBy').AsInteger;
          iAutofit:= dbQuery.FieldByName('Autofit').AsInteger;
          sFontName:= trim(dbQuery.FieldByName('FontName').AsWideString);
          sFontSize:= trim(dbQuery.FieldByName('FontSize').AsWideString);
          sFontColor:= trim(dbQuery.FieldByName('FontColor').AsWideString);
          sFontStyle:= trim(dbQuery.FieldByName('FontStyle').AsWideString);
        end;

        Fields[i].Tag:= iSN;
        Fields[i].Visible := ivis=1;
        Fields[i].DisplayLabel := slab;
        if iDisplaySize>0 then
           Fields[i].DisplayWidth := iDisplaySize;
        //Font
        Fields[i].Origin:= trim(sFontName)+';'+trim(sFontSize)
                            +';'+trim(sFontColor)+';'+trim(sFontStyle)
                            +';'+inttostr(iSubTotal)
                            +';'+inttostr(iReportGroup)
                            +';'+inttostr(iAutofit)
                            +';'+inttostr(iOrderBy);
        if sfmt<>'' then
        begin
          if ((Fields[i] is TStringField) or (Fields[i] is TWideStringField)) then
          begin
             Fields[i].EditMask := sfmt;
          end
          else
          if (Fields[i] is TDatetimeField) then
          begin
            if Uppercase(sfmt)='YYYY/MM/DD' then
            begin
               TDatetimeField(Fields[i]).EditMask := '9999/99/99';
               TDatetimeField(Fields[i]).DisplayFormat := 'YYYY/MM/DD';
            end
            else if Uppercase(sfmt)='HH:MM' then
            begin
               TDatetimeField(Fields[i]).EditMask := '99:99';
               TDatetimeField(Fields[i]).DisplayFormat := 'HH:mm';
            end
            else
            begin
               TDatetimeField(Fields[i]).EditMask := '9999/99/99 99:99';
               TDatetimeField(Fields[i]).DisplayFormat := 'YYYY/MM/DD HH:mm';
            end;
          end
          else
          if (Fields[i] is TFloatField) then
          begin
            TFloatField(Fields[i]).EditFormat := sfmt;
            TFloatField(Fields[i]).DisplayFormat := sfmt;
            //TNTGrid造成只有1個輸入位數+小數點前被限制住
            //TFloatField(Fields[i]).EditMask := sfmt;
          end;
        end;
      end;
      //for j := 0 to FieldCount-1 do
      for j := 0 to FieldCount-1 do
      begin
        for k := j to FieldCount-1 do
        begin
          if Fields[k].Tag = j then
          begin
             Fields[k].Index:= j;
             break;
          end;
        end;
      end;
      EnableControls;
    end;
  finally
    dbQuery.Free;
  end;
end;


//2012.05.09 add for MUT Bill-20120509-01

procedure SetReportFieldDLL2(dbset: TCustomADODataSet; sReportName, Conn: WideString;qryExec:TADOQuery);
var sItems, sfmt, slab, sNodboName, sFieldName: WideString;
    sFontName, sFontColor, sFontSize, sFontStyle: WideString;
    i, j, k, ivis, iSN, iDisplaySize, iFontColor: integer;
    iReportGroup, iSubTotal, iAutofit, iOrderBy: integer;

    LkResults: Variant;
    sLanguageId:string;
begin
  sLanguageId:=TJSdTable(dbset).ReserveList.Values['LanguageId'];

  if sLanguageId='' then sLanguageId:='TW';

  sNodboName:= StringReplace(sReportName, 'dbo.', '', [rfReplaceAll, rfIgnoreCase]);

    with qryExec do
    begin
      close;

      LockType := ltReadOnly;

      SQL.Add('Select * from dbo.CURdF_TableField('+''''+sNodboName+''''+','+
        ''''+sLanguageId+''''+')');

      Open;

      if qryExec.RecordCount=0 then Exit;
    end;

    with TCustomADODataSet(dbset) do
    begin
      DisableControls;
      for i := 0 to FieldCount - 1 do
      begin
        sFieldName:= Fields[i].FieldName;
        slab:= Fields[i].DisplayLabel;
        if (Fields[i] is TFloatField) then
           sfmt:= TFloatField(Fields[i]).DisplayFormat
        else
           sfmt:= Fields[i].EditMask;

        ivis:= IIFIntegerDLL(Fields[i].Visible, '1', '0');

        iSN:= Fields[i].Index;
        sFontName:= '';
        sFontSize:= '';
        sFontColor:= '';
        sFontStyle:= '';

        iSubTotal:= 0;
        iReportGroup:= 0;
        iAutofit:= 0;
        iOrderBy:= 1;
        if qryExec.Locate('TableName;FieldName',
                        VarArrayOf([sNodboName, sFieldName]),
                        [loPartialKey]) then
        begin
          slab:= trim(qryExec.FieldByName('DisplayLabel').AsWideString);
          sfmt:= trim(qryExec.FieldByName('FormatStr').AsWideString);
          ivis:= qryExec.FieldByName('Visible').AsInteger;
          iSN:= qryExec.FieldByName('SerialNum').AsInteger-1;
          iDisplaySize:= qryExec.FieldByName('DisplaySize').AsInteger;
          iSubTotal:= qryExec.FieldByName('SubTotal').AsInteger;//###
          iReportGroup:= qryExec.FieldByName('ReportGroup').AsInteger;//###
          iOrderBy:= qryExec.FieldByName('OrderBy').AsInteger;//###
          iAutofit:= qryExec.FieldByName('Autofit').AsInteger;//###
          sFontName:= trim(qryExec.FieldByName('FontName').AsWideString);
          sFontSize:= trim(qryExec.FieldByName('FontSize').AsWideString);
          sFontColor:= trim(qryExec.FieldByName('FontColor').AsWideString);
          sFontStyle:= trim(qryExec.FieldByName('FontStyle').AsWideString);
        end;

        Fields[i].Tag:= iSN;
        Fields[i].Visible := ivis=1;
        Fields[i].DisplayLabel := slab;
        if iDisplaySize>0 then
           Fields[i].DisplayWidth := iDisplaySize;
        //Font
        if Fields[i].Origin='' then
          Fields[i].Origin:=trim(sFontName)+';'+trim(sFontSize)
                            +';'+trim(sFontColor)+';'+trim(sFontStyle);

        Fields[i].Origin:= //trim(sFontName)+';'+trim(sFontSize)
                           // +';'+trim(sFontColor)+';'+trim(sFontStyle)
                           Fields[i].Origin
                            +';'+inttostr(iSubTotal)
                            +';'+inttostr(iReportGroup)
                            +';'+inttostr(iAutofit)
                            +';'+inttostr(iOrderBy);
        if sfmt<>'' then
        begin
          if ((Fields[i] is TStringField) or (Fields[i] is TWideStringField)) then
          begin
             Fields[i].EditMask := sfmt;
          end
          else
          if (Fields[i] is TDatetimeField) then
          begin
            if Uppercase(sfmt)='YYYY/MM/DD' then
            begin
               TDatetimeField(Fields[i]).EditMask := '9999/99/99';
               TDatetimeField(Fields[i]).DisplayFormat := 'YYYY/MM/DD';
            end
            else if Uppercase(sfmt)='HH:MM' then
            begin
               TDatetimeField(Fields[i]).EditMask := '99:99';
               TDatetimeField(Fields[i]).DisplayFormat := 'HH:mm';
            end
            else
            begin
               TDatetimeField(Fields[i]).EditMask := '9999/99/99 99:99';
               TDatetimeField(Fields[i]).DisplayFormat := 'YYYY/MM/DD HH:mm';
            end;
          end
          else
          if (Fields[i] is TFloatField) then
          begin
            TFloatField(Fields[i]).EditFormat := sfmt;
            TFloatField(Fields[i]).DisplayFormat := sfmt;
            //TNTGrid造成只有1個輸入位數+小數點前被限制住
            //TFloatField(Fields[i]).EditMask := sfmt;
          end;
        end;
      end;
      //for j := 0 to FieldCount-1 do
      for j := 0 to FieldCount-1 do
      begin
        for k := j to FieldCount-1 do
        begin
          if Fields[k].Tag = j then
          begin
             Fields[k].Index:= j;
             break;
          end;
        end;
      end;
      EnableControls;
    end;

end;


function Query2DataSetDLL(SQLStmts: WideString; dset:TADOQuery;sConnectStr:string):Boolean;
begin
    try
      Result := false;
      with dset do
      begin
        Close;
        ConnectionString := sConnectStr;
        CommandTimeout := 9600;
        LockType := ltReadOnly;
        SQL.Clear;
        SQL.Text:= SQLStmts;
        Open;
      end;
      Result := true;
    except
      Raise;
      Result := false;
    end;
end;


function GetReportFieldDLL(ProcName, ReportName: WideString;
       AParams: Array of WideString;qryExec:TADOQuery;sConnectStr:string): Boolean;
var i: Integer;
    dbQuery: TADOQuery;
    sSQL, SQLStmts, strParam: WideString;
begin
  Result := False;
  try
    try
      strParam:= '';
      strParam:= GetParamStringDLL(ProcName, AParams,sConnectStr);
      SQLStmts:='EXEC '+ProcName+ ' '+ strParam;
      dbQuery := TADOQuery.Create(nil);
      with dbQuery do
      begin
        ConnectionString := sConnectStr;
        CommandTimeout := 9600;
        LockType := ltReadOnly;
        SQL.Clear;
        SQL.Text:=SQLStmts;
        Open;
      end;
      BackupTableDLL('CURdReportField', 'CURdReportFieldBK', 'ReportName='''+ReportName+'''',qryExec,sConnectStr);

      sSQL:='';
      sSQL:= 'Delete CURdReportField where ReportName='''+ReportName+'''';
      //SQLExecute(sSQL);
    with qryExec do
      begin
        close;
        sql.Clear;
        sql.Add(sSQL);
        ExecSQL;
        close;
      end;

      dbQuery.FieldDefs.Update;
      with dbQuery.FieldDefs do
      begin
        for i:=0 to Count-1 do
        begin
          sSQL:='';
          sSQL:= 'Insert into CURdReportField(ReportName, FieldName, DisplayLabel, SerialNum, Visible)'
                +' Values('
                          +''''+ReportName+''','
                          +''''+Items[i].Name+''','
                          +''''+Items[i].Name+''','
                          +inttostr(i+1)+','
                          +'1'
                +')';
          //SQLExecute(sSQL);
          with qryExec do
            begin
              close;
              sql.Clear;
              sql.Add(sSQL);
              ExecSQL;
              close;
           end;
        end;
      end;

      sSQL:='';
      sSQL:=  'exec CURdReportFieldRecover '''+ReportName+'''';

      //SQLExecute(sSQL);
          with qryExec do
            begin
              close;
              sql.Clear;
              sql.Add(sSQL);
              ExecSQL;
              close;
           end;

      Result := True;
    except
      Result := False;
      Raise;
    end;
  finally
    dbQuery.Close;
    dbQuery.Free;
  end;
end;


//2010.9.8 add

function GetReportFieldDLL2(ProcName, ReportName: WideString;

       AParams: Array of WideString;qryExec:TADOQuery;sConnectStr,sNowSQL:string): Boolean;
var i: Integer;
    dbQuery: TADOQuery;
    sSQL, SQLStmts, strParam: WideString;
begin
  Result := False;
  try
    try
      strParam:= '';
      strParam:= GetParamStringDLL(ProcName, AParams,sConnectStr);
      //SQLStmts:='EXEC '+ProcName+ ' '+ strParam;

      SQLStmts:=sNowSQL; //2010.9.8 modify

      dbQuery := TADOQuery.Create(nil);
      with dbQuery do
      begin
        ConnectionString := sConnectStr;
        CommandTimeout := 9600;
        LockType := ltReadOnly;
        SQL.Clear;
        SQL.Text:=SQLStmts;
        Open;
      end;
      BackupTableDLL('CURdReportField', 'CURdReportFieldBK', 'ReportName='''+ReportName+'''',qryExec,sConnectStr);

      sSQL:='';
      sSQL:= 'Delete CURdReportField where ReportName='''+ReportName+'''';
      //SQLExecute(sSQL);
    with qryExec do
      begin
        close;
        sql.Clear;
        sql.Add(sSQL);
        ExecSQL;
        close;
      end;

      dbQuery.FieldDefs.Update;
      with dbQuery.FieldDefs do
      begin
        for i:=0 to Count-1 do
        begin
          sSQL:='';
          sSQL:= 'Insert into CURdReportField(ReportName, FieldName, DisplayLabel, SerialNum, Visible)'
                +' Values('
                          +''''+ReportName+''','
                          +''''+Items[i].Name+''','
                          +''''+Items[i].Name+''','
                          +inttostr(i+1)+','
                          +'1'
                +')';
          //SQLExecute(sSQL);
          with qryExec do
            begin
              close;
              sql.Clear;
              sql.Add(sSQL);
              ExecSQL;
              close;
           end;
        end;
      end;

      sSQL:='';
      sSQL:=  'exec CURdReportFieldRecover '''+ReportName+'''';

      //SQLExecute(sSQL);
          with qryExec do
            begin
              close;
              sql.Clear;
              sql.Add(sSQL);
              ExecSQL;
              close;
           end;

      Result := True;
    except
      Result := False;
      Raise;
    end;
  finally
    dbQuery.Close;
    dbQuery.Free;
  end;
end;



function GetParamStringDLL(ProceName: WideString;
  ParamList: array of WideString;sConnectStr:string): WideString;
var dbQuery: TADOQuery;
    iCnt: Integer;
    sTmp, sRpls: WideString;
begin
  Result:= '';
  dbQuery := TADOQuery.Create(nil);
  try
    try
      with dbQuery do
      begin
        ConnectionString := sConnectStr;
        LockType:= ltReadOnly;
        SQL.Add('exec CURdGetParamType '''+ProceName+'''');
        Open;
        first;
        iCnt:=0;
        while not eof do
        begin
          if (Result<>'') then
             Result:=Result+',';

          if Fieldbyname('Quote').AsInteger=1 then
          begin
            if Length(ParamList[iCnt])<128 then
            begin
              if pos('''', ParamList[iCnt])>0 then
              begin
                 sTmp:= '['+trim(ParamList[iCnt])+']';
              end
              else
                 sTmp:= ''''+trim(ParamList[iCnt])+'''';

              Result:=Result + Fieldbyname('ParamName').asString+'='
                    +'N' //2011.1.19 add
                    +sTmp;

            end
            else //長度有128的限制
            begin
              sTmp:= AnsiReplaceText(ParamList[iCnt], '''', '''''');
              sTmp:= ''''+sTmp+'''';
              Result:=Result + Fieldbyname('ParamName').asString+'='
                    +'N' //2011.1.19 add
                    +sTmp;
            end;
          end

          else
          begin
            if ParamList[iCnt]='' then
               sTmp:= '255'
            else
               sTmp:= ParamList[iCnt];
            Result:=Result + Fieldbyname('ParamName').asString+'='
                  +sTmp;
          end;
          Next;
          iCnt:=iCnt+1;
        end;
      end;
    except
       on E: Exception do Raise;
    end;
  finally
    dbQuery.Close;
    dbQuery.Free;
  end;
end;

function SQLExecIfExistExDLL(
  sObjName, strSQL, sType: WideString; bExist: Boolean;sConnectStr:string): WideString;
var dbQuery: TADOQuery;
    sBef: WideString;
begin
  dbQuery := TADOQuery.Create(nil);
  Result := '';
  sBef := '';
  try
    try
      with dbQuery do
      begin
        ConnectionString := sConnectStr;
        CommandTimeout := 9600;
        LockType := ltReadOnly;
        ExecuteOptions:=[eoExecuteNoRecords];
        if bExist then
           sBef:= 'if Exists '
        else
           sBef:= 'if NOT Exists ';

        sBef:= sBef
                +' (Select Id from sysobjects(nolock) '
                +' where id = object_id('''+sObjName+''')'
                +' and Type ='''+sType+''') ';
        SQL.Add(sBef+strSQL);
        ExecSQL;
        Close;
      end;
    except
       on E: Exception do Result:=E.Message;
    end;
  finally
    dbQuery.Free;
  end;
end;

procedure BackupTableDLL(sTable, bkName, sWhere: WideString;qryExec:TADOQuery;sConnectStr:string);
var {sErr,} sSQL, bkNamedbo: WideString;
begin
  bkNamedbo:= StringReplace(bkName, 'dbo.', '', [rfReplaceAll, rfIgnoreCase]);
  bkNamedbo:= 'dbo.'+trim(bkNamedbo);
    //sErr:= '';
    sSQL:= 'drop table '+ bkNamedbo;
    {sErr:=} SQLExecIfExistExDLL(bkNamedbo, sSQL, 'u', true,sConnectStr);
    {if sErr<>'' then
    begin
      MsgDlgJS(sErr, mtError, [mbOk], 0);
      Exit;
    end;}

    //sErr:= '';
    sSQL:= ' Select * into '+ bkNamedbo+' from '+ sTable+'(nolock)'
           + ' where '+ sWhere;
    {sErr:= SQLExecute(sSQL);
    if sErr<>'' then
    begin
      MsgDlgJS(sErr, mtError, [mbOk], 0);
      Exit;
    end;}
    with qryExec do
      begin
        close;
        sql.Clear;
        sql.Add(sSQL);
        ExecSQL;
        close;
      end;
end;


function funDLLSysParamsGet(qryExec:TADOQuery;sSystemId,sParamName:string):string;
var sSQL,sValue:string;
begin
  sValue:='';
  sSQL:='exec CURdOCXSysParamGet '+''''+sSystemId+''''+','+
    ''''+sParamName+'''';
  with qryExec do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qryExec.RecordCount>0 then
    sValue:=qryExec.Fields[0].AsString;

  qryExec.Close;

  result:=sValue;
end;

function funFlowPrcIdGet(qryExec:TADOQuery;sItemId:string):string;
var sSQL,sValue:string;
begin
  sValue:='';
  sSQL:='select FlowPrcId from CURdSysItems(nolock) where ItemId='+
    ''''+sItemId+'''';

  with qryExec do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qryExec.RecordCount>0 then
    sValue:=qryExec.Fields[0].AsString;

  qryExec.Close;

  result:=sValue;
end;

function funNewPaper(
  bReGetNum:boolean;
  sUserId:string;
  sUseId:string;
  sPaperNum:string;
  dPaperDate:TDateTime;
  qryExec:TADOQuery;
  tTable:TJSdTable;
  sRealTableNameMas1:string;
  sSelectSQLMas1:string;
  PaperType:integer;
  CanbSelectType:integer;
  var bNewCancel:boolean;
  var CurrTypeHead:widestring;
  var CurrPaperType:integer;
  var CurrPaperNum:string;
  sRunSQLAfterAdd:string;
  PowerType:integer //2010.9.15 add for QU Foster-20100913-1
  ):boolean;
var sSQL: String;
    //tDate, tNow:TDateTime;
    sDate, sNow:string;//2011.9.29 modify for MUT Bill-20110928-02

    sPaperTypeName:WideString;//2010.7.22 add for YX Bill-20100715-3 Mail

    sUpdateFieldName:string;//2011.11.11 add for MUT Bill-20111110-01
    sUpdateValue:WideString;//2011.11.11 add for MUT Bill-20111110-01

    sTradeId:string;//2016.09.29 add
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  //2016.09.29 add
  if CanbSelectType=1 then
  begin
    sSQL:='';
    sSQL:='select iCount=count(PaperId) from CURdPaperType(nolock)'
          +' where PaperId='+''''+ sRealTableNameMas1+''''
          +' and ((PowerType='+inttostr(PowerType)+') or (PowerType=-1))' ;

    OpenSQLDLL(qryExec,'OPEN',sSQL);

    if qryExec.RecordCount=0 then
      begin
        CanbSelectType:=0;
      end
    else
      begin
        if qryExec.Fields[0].AsInteger=0 then
            CanbSelectType:=0;
      end;
  end;

  if CanbSelectType=1 then
  begin
      Application.CreateForm(TdlgPaperSelectType2DLL, dlgPaperSelectType2DLL);

      //2020.03.10
      if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
      begin
          sList:=TstringList.Create;
          sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
          sFontSize:=sList.Strings[0];
          sList.Free;
          FontSize := StrToInt(sFontSize);
          dlgPaperSelectType2DLL.Scaled:=true;
          dlgPaperSelectType2DLL.ScaleBy(FontSize,100);
      end;
      dlgPaperSelectType2DLL.tblPaperType.Close;
      dlgPaperSelectType2DLL.tblPaperType.ConnectionString:=qryExec.ConnectionString;
      dlgPaperSelectType2DLL.tblPaperType.SQL.Clear;
      //dlgPaperSelectType2DLL.tblPaperType.SQL.Add('Select * from CURdPaperType '
      //      +' where PaperId='''+ sRealTableNameMas1+''''
      //      +' and ((PowerType='+inttostr(PowerType)+') or (PowerType=-1))' //2010.9.15 add for QU Foster-20100913-1
      //      );

      //----------2016.09.29 modify
      sSQL:='';

      if funGetSelectSQL(qryExec,'','CURdPaperType',sSQL)=false then exit;

      sSQL:=sSQL
        +' and t0.PaperId='+''''+ sRealTableNameMas1+''''
        +' and ((t0.PowerType='+inttostr(PowerType)+') or (t0.PowerType=-1))';

      dlgPaperSelectType2DLL.tblPaperType.SQL.Add(sSQL);
      //----------
      dlgPaperSelectType2DLL.tblPaperType.Open;
      dlgPaperSelectType2DLL.CurrPaperId:= sRealTableNameMas1;

      sTradeId:='';//2016.09.29 add

      dlgPaperSelectType2DLL.showmodal;
      if dlgPaperSelectType2DLL.modalresult=mrok then
      begin
         bNewCancel:= false;
         CurrTypeHead:= dlgPaperSelectType2DLL.tblPaperType.FieldByName('HeadFirst').AsString;
         CurrPaperType:= dlgPaperSelectType2DLL.tblPaperType.FieldByName('PaperType').asInteger;
         sPaperTypeName:=dlgPaperSelectType2DLL.tblPaperType.FieldByName('PaperTypeName').asWideString;//2010.7.22 add for YX Bill-20100715-3 Mail
         sUpdateFieldName:=dlgPaperSelectType2DLL.tblPaperType.FieldByName('UpdateFieldName').AsString;//2011.11.11 add for MUT Bill-20111110-01
         sUpdateValue:=dlgPaperSelectType2DLL.tblPaperType.FieldByName('UpdateValue').asWideString;//2011.11.11 add for MUT Bill-20111110-01

         //2016.09.29 add
         if dlgPaperSelectType2DLL.tblPaperType.FindField('TradeId')<> nil then
            begin
              sTradeId:=dlgPaperSelectType2DLL.tblPaperType.FieldByName('TradeId').AsString;
            end;
      end
      else
      begin
         bNewCancel:= true;
      end;
  end
  else
    CurrPaperType:= PaperType;

  if bNewCancel then
  begin
     result:=false;
     Exit;
  end;

  with qryExec do
    begin
      close;
      sql.Clear;
      //SQL.Add('Select tDate=convert(datetime, convert(varchar(10), getdate(), 111))');
      //SQL.Add(' ,tNow=getdate()');
      SQL.Add('exec CURdGetServerDateTimeStr');//2011.9.29 modify for MUT Bill-20110928-02
      Open;
      //tDate:= fieldbyname('tDate').asdatetime;
      //tNow:= fieldbyname('tNow').asdatetime;
    end;

  //----------2011.9.29 modify for MUT Bill-20110928-02
  if qryExec.RecordCount=0 then
    begin
      MsgDlgJS('無法取得 Server 的日期時間',mtError,[mbOk],0);
      result:=false;
      Exit;
    end;

  sDate:= qryExec.fieldbyname('sDate').AsString;
  sNow:= qryExec.fieldbyname('sNow').AsString;
  //----------

  if bReGetNum then
    begin
      //tDate:=dPaperDate;

      //2011.9.29 modify for MUT Bill-20110928-02
      sDate:=FormatDateTime('yyyy/mm/dd',dPaperDate);
    end;

  sSQL:= '';
  sSQL:= 'exec CURdGetPaperNum '
        +' '''+sRealTableNameMas1+''','
        +' '''+''','
        +' '''+copy(sUseId,1,1)+''','
        //+' '''+Datetimetostr(tDate)+''','
        +' '''+sDate+''',' //2011.9.29 modify for MUT Bill-20110928-02
        +' '''+CurrTypeHead+''''
        +','+''''+sUseId+'''' //2010.7.23 add for YX RA10070501
        ;

  with qryExec do
    begin
      close;
      sql.Clear;
      SQL.Add(sSQL);
      Open;
    end;

  if qryExec.RecordCount=0 then
    begin
      qryExec.Close;
      MsgDlgJS('取單號失敗', mtError, [mbOk], 0);
      result:=false;
      exit;
    end;

  CurrPaperNum:= trim(qryExec.Fields[0].AsString);

  if CurrPaperNum='' then
    begin
      qryExec.Close;
      MsgDlgJS('取單號失敗', mtError, [mbOk], 0);
      result:=false;
      exit;
    end;

  sSQL:='';

  if bReGetNum=false then
    begin
      //----------2016.09.29 add,檢查單據主檔有沒有 TradeId 這個欄位
      if sTradeId<>'' then
        begin
          sSQL:='select t1.* from syscolumns t1(nolock),sysobjects t2(nolock)'
            +' where t1.id=t2.Id'
            +' and t2.type='+''''+'u'+''''
            +' and t2.Name='+''''+sRealTableNameMas1+''''
            +' and t1.Name='+''''+'TradeId'+'''';

         OpenSQLDLL(qryExec,'OPEN',sSQL);

         if qryExec.RecordCount=0 then
            begin
              sTradeId:='';
            end;
        end;
      //----------

      sSQL:='';

      sSQL:='insert into '+sRealTableNameMas1
      //+'(PaperNum,PaperDate,Status,Finished,BuildDate,UserId,UseId,dllPaperType,dllPaperTypeName,dllHeadFirst) select '
      +'(PaperNum,PaperDate,Status,Finished,BuildDate,UserId,UseId,dllPaperType,dllPaperTypeName,dllHeadFirst';//2011.11.11 modify for MUT Bill-20111110-01

      if ((sUpdateFieldName<>'') and (sUpdateValue<>'')) then sSQL:=sSQL+','+sUpdateFieldName;//2011.11.11 add for MUT Bill-20111110-01

      //2016.09.29 add
      if sTradeId<>'' then  sSQL:=sSQL+',TradeId';

      sSQL:=sSQL //2011.11.11 add for MUT Bill-20111110-01
      +') select ' //2011.11.11 add for MUT Bill-20111110-01
      +''''+CurrPaperNum+''''+','
      //+''''+datetimetostr(tDate)+''''
      +''''+sDate+'''' //2011.9.29 modify for MUT Bill-20110928-02
      +',0,0,'
      //+''''+FormatDateTime('yyyy/mm/dd hh:nn:ss',tNow)+''''+','
      +''''+sNow+''''+',' //2011.9.29 modify for MUT Bill-20110928-02
      +''''+sUserId+''''+','
      +''''+sUseId+''''+','
      +inttostr(CurrPaperType)+',' //2010.7.22 add for YX Bill-20100715-3 Mail
      +'N'+''''+sPaperTypeName+''''+',' //2010.7.22 add for YX Bill-20100715-3 Mail
      +''''+CurrTypeHead+'''' //2010.7.22 add for YX Bill-20100715-3 Mail
      ;

      if ((sUpdateFieldName<>'') and (sUpdateValue<>'')) then sSQL:=sSQL+',N'+''''+sUpdateValue+'''';//2011.11.11 add for MUT Bill-20111110-01

      //2016.09.29 add
      if sTradeId<>'' then  sSQL:=sSQL+',N'+''''+sTradeId+'''';
    end
    else
    begin

      sSQL:='update '+sRealTableNameMas1
      +' set PaperNum='+''''+CurrPaperNum+''''
      +',dllPaperType='+inttostr(CurrPaperType) //2010.7.22 add for YX Bill-20100715-3 Mail
      +',dllPaperTypeName='+'N'+''''+sPaperTypeName+'''' //2010.7.22 add for YX Bill-20100715-3 Mail
      +',dllHeadFirst='+''''+CurrTypeHead+'''' //2010.7.22 add for YX Bill-20100715-3 Mail
      +' where PaperNum='+''''+sPaperNum+''''
      ;
    end;

  with qryExec do
    begin
      close;
      sql.Clear;
      SQL.Add(sSQL);
      ExecSQL;
      close;
    end;

 if bReGetNum=false then
    if sRunSQLAfterAdd<>'' then
      begin
        funReplaceCom(sRunSQLAfterAdd,tTable);

        if (pos('@PaperNum',sRunSQLAfterAdd)>0)then
          sRunSQLAfterAdd:=AnsiReplaceText(sRunSQLAfterAdd,'@PaperNum',''''+CurrPaperNum+'''');

        sSQL:='';
        sSQL:=sRunSQLAfterAdd+' and t0.PaperNum='+''''+CurrPaperNum+'''';

        with qryExec do
          begin
            close;
            sql.Clear;
            SQL.Add(sSQL);
            ExecSQL;
            close;
          end;
      end;

  with tTable do
    begin
      close;
      sql.Clear;
      sql.Add(sSelectSQLMas1+' and t0.PaperNum='+''''+CurrPaperNum+'''');
      open;
    end;

    result:=true;
end;

function funDLLInfoByPaperId( //未使用2009.6.29
  qry:TADOQuery;
  sPaperId:string;
  var sItemId:string;
  var sItemName:string
  ):boolean;
var sSQL:string;
begin
  sItemId:='';
  sItemName:='';

  sSQL:='select ItemId,ItemName from CURdSysItems(nolock) where ItemType=6 and '
    +' ClassName='+''''+sPaperId+'.dll'+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount>0 then
    begin
      sItemId:=qry.FieldByName('ItemId').AsString;
      sItemName:=qry.FieldByName('ItemName').AsString;
    end;

  qry.Close;
  result:=true;
end;

function funPaperMsgGet(
  sItemId:string;
  sButtonName:string;
  qry:TADOQuery;
  iType:integer
  ):string;
var sSQL,sRe:string;
begin
  sRe:='';
  if iType=0 then
    sSQL:='select sCustMsg from CURdOCXItemSysButton(nolock) where ItemId='+
      ''''+sItemId+''''+' and ButtonName='+''''+sButtonName+''''+
      ' and bShowMsg=1'
  else
    sSQL:='select sCustMsgAft from CURdOCXItemSysButton(nolock) where ItemId='+
      ''''+sItemId+''''+' and ButtonName='+''''+sButtonName+''''+
      ' and bShowMsgAft=1';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount>0 then
    sRe:=qry.Fields[0].AsString;

  qry.Close;

  result:= sRe;
end;

function funPaperExam(
  //frm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CanbRunFLow,
  CanbAudit:integer;
  sRealTableNameMas1,
  sUserId:string;
  sItemId:string
  ):boolean;
var iFinished:integer;sPaperNum,sSQL,sMsg:string;
begin
  if tTable.Active=false then
    begin
      MsgDlgJS('資料表未開啟',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.RecordCount=0 then
    begin
      MsgDlgJS('無資料可供處理',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sPaperNum:=tTable.FieldByName('PaperNum').Asstring;

  if sPaperNum='' then
    begin
      MsgDlgJS('沒有單據號碼',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sMsg:='';
  sMsg:=funPaperMsgGet(sItemId,conBtnName_Exam,qry,0);

  if sMsg<>'' then
    if MsgDlgJS(sMsg,
      mtConfirmation,[mbYes,mbNo],0)<>mrYes then //2023.08.18 =mrNo then
    begin
      result:=false;
      exit;
    end;

  iFinished:=tTable.FieldByName('Finished').AsInteger;

  if iFinished=1 then
    begin
      MsgDlgJS('此單據「已完成」,不可審核',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=2 then
    begin
      MsgDlgJS('此單據「已作廢」,不可審核',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=4 then
    begin
      MsgDlgJS('此單據「已結案」,不可審核',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if ((CanbRunFLow=1) and (CanbAudit=0)) then
    begin
      MsgDlgJS('您沒有「審核」的權限',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sSQL:= 'exec CURdPaperAction '
        +''''+sRealTableNameMas1+''''+','
        +''''+sPaperNum+''''+','
        +''''+sUserId+''''
        +',1,1';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      ExecSQL;
      close;
    end;

  tTable.Close;
  tTable.Open;
  tTable.Locate('PaperNum',sPaperNum,[loCaseInsensitive]);

  //MsgDlgJS('已完成「審核」',mtInformation,[mbOk],0);
  sMsg:='';
  sMsg:=funPaperMsgGet(sItemId,conBtnName_Exam,qry,1);
  if sMsg<>'' then
    MsgDlgJS(sMsg,mtInformation,[mbOk],0);

  result:=true;
end;

function funPaperReGetNum(
  CanbLockPaperDate:integer;
  CanbUpdate:integer;
  //====================
  sUserId:string;
  sUseId:string;
  qryExec:TADOQuery;
  tTable:TJSdTable;
  sRealTableNameMas1:string;
  sSelectSQLMas1:string;
  PaperType:integer;
  CanbSelectType:integer;
  var bNewCancel:boolean;
  var CurrTypeHead:widestring;
  var CurrPaperType:integer;
  var CurrPaperNum:string;
  CanbLockUserEdit:integer;
  PowerType:integer //2010.9.15 add for QU Foster-20100913-1
  ):boolean;
var iFinished:integer;sPaperNum:string;dPaperDate:TDateTime;
begin
  if CanbUpdate=0 then
    begin
      MsgDlgJS('您沒有「編輯」的權限',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if CanbLockPaperDate=1 then
    begin
      MsgDlgJS('此單據已被設定「不允許修改單據日期」，故不可「重取單號」',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.Active=false then
    begin
      MsgDlgJS('資料表未開啟',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.RecordCount=0 then
    begin
      MsgDlgJS('無資料可供處理',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sPaperNum:=tTable.FieldByName('PaperNum').Asstring;

  if sPaperNum='' then
    begin
      MsgDlgJS('沒有單據號碼',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if MsgDlgJS('確定要為單號'+sPaperNum+'重取號碼 ?',
      mtConfirmation,[mbYes,mbNo],0)<>mrYes then //2023.08.18 =mrNo then
    begin
      result:=false;
      exit;
    end;

  iFinished:=tTable.FieldByName('Finished').AsInteger;

  //2024.12.13 原本只有卡<作業中>才能重取號。希望”審核中”單據也能支援<重取號>
  if ((iFinished<>0) and (iFinished<>3)) then
    begin
      MsgDlgJS('只有「作業中、審核中」的單據才可「重取單號」',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  dPaperDate:=tTable.FieldByName('PaperDate').AsdateTime;

  if varisnull(dPaperDate) then
    begin
      MsgDlgJS('沒有單據日期',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if CanbLockUserEdit=1 then
      begin
        if (trim(sUserId)<>trim(tTable.FieldByName('UserId').Asstring))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
        begin
         MsgDlgJS('此單據已被設定「只有建檔者可編輯」',mtWarning,[mbOk],0);
         result:=false;
         exit;
        end;
      end;

  //----------2014.09.12 add for DYN

  //2014.09.15 add for 解決多單頭且不是新增完單據就立即重取號的狀況下，
  //會傳入最近一次新增時取號當時的單頭
  //若開啟單據畫面後沒有新增就點選某筆重取號，則會傳入空白(後端SP會自動取預設)
  //的問題
  if tTable.FindField('dllHeadFirst')<>nil then
    begin
      CurrTypeHead:=tTable.FieldByName('dllHeadFirst').AsString;
    end;

  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Add('exec CURdReGetPaperNumChk '
      +''''+sRealTableNameMas1+''''+','
      +''''+sPaperNum+''''+','
      +''''+copy(sUseId,1,1)+''''+','
      +''''+formatdatetime('yyyy/mm/dd',tTable.FieldByName('BuildDate').AsdateTime)+''''+','
      +''''+CurrTypeHead+''''+','
      +''''+sUseId+''''
      );
  qryExec.Open;

  if qryExec.Fields[0].AsString<>'OK' then
     begin
       MsgDlgJS(qryExec.Fields[0].AsString,mtError,[mbOk],0);
       result:=false;
       exit;
     end;
  //----------

  result:=funNewPaper(
    true,//bReGetNum:boolean;
    sUserId,//sUserId:string;
    sUseId,//sUseId:string;
    sPaperNum,//sPaperNum:string;
    dPaperDate,//dPaperDate:TDateTime;
    qryExec,//qryExec:TADOQuery;
    tTable,//tTable:TJSdTable;
    sRealTableNameMas1,//sRealTableNameMas1:string;
    sSelectSQLMas1,//sSelectSQLMas1:string;
    PaperType,//PaperType:integer;
    CanbSelectType,//CanbSelectType:integer;
    bNewCancel,//var bNewCancel:boolean;
    CurrTypeHead,//var CurrTypeHead:string;
    CurrPaperType,//var CurrPaperType:integer;
    CurrPaperNum, //var CurrPaperNum:string
    '',
    PowerType //2010.9.15 add for QU Foster-20100913-1
    );

end;

function funPaperUpdateNotes(
  iIsMas:integer;
  qry:TADOQuery;
  tTable:TJSdTable;
  sRealTableNameMas:string;
  tDtl:TJSdTable;
  sRealTableNameDtl:string;
  CanbUpdate:integer;
  CanbLockUserEdit:integer;
  sUserId:string
  ):boolean;
var iFinished:integer;sPaperNum,sSQL,sReason:string;iItem:integer;
begin
  if tTable.Active=false then
    begin
      MsgDlgJS('資料表未開啟',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.RecordCount=0 then
    begin
      MsgDlgJS('無資料可供處理',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sPaperNum:=tTable.FieldByName('PaperNum').Asstring;

  if sPaperNum='' then
    begin
      MsgDlgJS('沒有單據號碼',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  iFinished:=tTable.FieldByName('Finished').AsInteger;

  if iFinished in[0,3] then
    begin
      MsgDlgJS('「作業中」或「審核中」的單據可直接修改備註,不須操作此功能',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=2 then
    begin
      MsgDlgJS('此單據「已作廢」,不可修改備註',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=4 then
    begin
      MsgDlgJS('此單據「已結案」,不可修改備註',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if CanbLockUserEdit=1 then
      begin
        if (trim(sUserId)<>trim(tTable.FieldByName('UserId').Asstring))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
        begin
         MsgDlgJS('此單據已被設定「只有建檔者可編輯」',mtWarning,[mbOk],0);
         result:=false;
         exit;
        end;
      end;

   if iIsMas=0 then
     begin
        if tDtl.Active=false then
          begin
            MsgDlgJS('明細資料表未開啟',mtError,[mbOk],0);
            result:=false;
            exit;
          end;

        if tDtl.RecordCount=0 then
          begin
            MsgDlgJS('無明細資料可供處理',mtError,[mbOk],0);
            result:=false;
            exit;
          end;

        iItem:=tDtl.FieldByName('Item').AsInteger;
     end;

    sReason:='';
    if InputQuery('輸入','請輸入備註',sReason)=false then
      begin
        result:=false;
        exit;
      end;

      sReason:=trim(sReason);

      if sReason<>'' then
        if pos('''',sReason)>0 then
          sReason:=AnsiReplaceText(sReason,'''','');

    if iIsMas=1 then
      begin
        sSQL:='Update '+trim(sRealTableNameMas)
              +' Set Notes=rtrim(isnull(Notes,'+''''+''''+'))+N'+''''+sReason+''''
              +' Where PaperNum='+''''+ sPaperNum+'''';
      end
      else
      begin
        sSQL:='Update '+trim(sRealTableNameDtl)
              +' Set Notes=rtrim(isnull(Notes,'+''''+''''+'))+N'+''''+sReason+''''
              +' Where PaperNum='+''''+ sPaperNum+''''
              +' and Item='+inttostr(iItem);
      end;

    with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      ExecSQL;
      close;
    end;

    //if iIsMas=1 then
    //  begin
        tTable.Close;
        tTable.open;
        tTable.locate('PaperNum',sPaperNum,[loCaseInsensitive]);
    //  end
    //  else
    //  begin
    //    tDtl.Close;
    //    tDtl.open;
    //    tDtl.locate('Item',iItem,[loCaseInsensitive]);
    //  end;

  result:=true;
end;

function funPaperCompleted(
  //frm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CanbRunFLow,
  CanbAudit:integer;
  sRealTableNameMas1,
  sUserId:string;
  CanbLockUserEdit:integer;
  sItemId:string;
  bUseFlow:boolean;
  sUseId:string;
  sSystemId:string;
  iNowFlowStatus:integer
  ):boolean;
var iFinished:integer;sPaperNum,sSQL,sFlowResult,sMsg,sFlag,tmpString:string;i,j:integer;
    bPaperExamMergeToConfirm:boolean;//2016.05.04 add
begin
  if tTable.Active=false then
    begin
      MsgDlgJS('資料表未開啟',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.RecordCount=0 then
    begin
      MsgDlgJS('無資料可供處理',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sPaperNum:=tTable.FieldByName('PaperNum').Asstring;

  if sPaperNum='' then
    begin
      MsgDlgJS('沒有單據號碼',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sMsg:='';
  sMsg:=funPaperMsgGet(sItemId,conBtnName_Completed,qry,0);

  if sMsg<>'' then
    if MsgDlgJS(sMsg,
      mtConfirmation,[mbYes,mbNo],0)<>mrYes then //2023.08.18 =mrNo then
    begin
      result:=false;
      exit;
    end;

  iFinished:=tTable.FieldByName('Finished').AsInteger;

  if iFinished=1 then
    begin
      MsgDlgJS('此單據「已完成」,不須重複操作',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=2 then
    begin
      MsgDlgJS('此單據「已作廢」,不可完成',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=4 then
    begin
      MsgDlgJS('此單據「已結案」,不可完成',mtError,[mbOk],0);
      result:=false;
      exit;
    end;


  bPaperExamMergeToConfirm:=funDLLSysParamsGet(qry,'CUR','PaperExamMergeToConfirm')='1'; //2016.05.04 add

  if bPaperExamMergeToConfirm=False then  //2016.05.04 add 'if...'
    if CanbLockUserEdit=1 then
      begin
        if (trim(sUserId)<>trim(tTable.FieldByName('UserId').Asstring))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
        begin
         MsgDlgJS('此單據已被設定「只有建檔者可編輯及完成」',mtWarning,[mbOk],0);
         result:=false;
         exit;
        end;
      end;

  //----------2010.9.14 add for 要檢查一次Master，因使用者若不修改Master就不會觸動JSdTable的BeforPost檢查機制
  for j:=0 to tTable.ReserveList.Count-1 do
    if tTable.ReserveList.Names[j]='IsNeed' then
      begin
        if tTable.FindField(tTable.ReserveList.ValueFromIndex[j])<>nil then
          begin
            if tTable.FieldByName(tTable.ReserveList.ValueFromIndex[j]).IsNull then
              begin
                MsgDlgJS('主檔必須輸入「'+tTable.FieldByName(tTable.ReserveList.ValueFromIndex[j]).DisplayLabel+'」',mtWarning,[mbOk],0);
                result:=false;
                abort;
              end;

            if tTable.FieldByName(tTable.ReserveList.ValueFromIndex[j]).DataType
              in[ftString,ftFixedChar,ftWideString,ftFixedWideChar] then
                if trim(tTable.FieldByName(tTable.ReserveList.ValueFromIndex[j]).AsString)='' then
              begin
                MsgDlgJS('主檔必須輸入「'+tTable.FieldByName(tTable.ReserveList.ValueFromIndex[j]).DisplayLabel+'」',mtWarning,[mbOk],0);
                result:=false;
                abort;
              end;
          end;
      end;

  //----------

  sFlag:='';

  i:=0;

  i:=pos('^',sSystemId);

  if i>0 then
    begin
      tmpString:=sSystemId;
      sSystemId:=copy(tmpString,1,i-1);
      sFlag:=copy(tmpString,i+1,1);
    end;

  if ((CanbRunFLow=0) and (sFlag='')) then //自動審核
    begin
      funPaperExam(
        //frm,
        qry,
        tTable,
        CanbRunFLow,
        CanbAudit,
        sRealTableNameMas1,
        sUserId,
        sItemId
        );

      result:=true;
      exit;
    end;

  sFlowResult:='NOFLOW';

if bUseFlow then
begin
  //若有設定電子簽核則進Flow
  sSQL:='';
  sSQL:='exec CURdOCXPaperToFlow '
        +''''+sItemId+''''+','
        +''''+sRealTableNameMas1+''''+','
        +''''+sPaperNum+''''+','
        +''''+sUserId+''''+','
        +''''+sUseId+''''+','
        +''''+sSystemId+''''+','
        +inttostr(iNowFlowStatus)
        ;

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      Open;
    end;

  if qry.RecordCount>0 then
    sFlowResult:=qry.Fields[0].AsString;

  qry.Close;
end;

if sFlowResult='NOFLOW' then
begin //5

 if sFlag='' then
 begin //4
  if ((CanbRunFLow=1) and (CanbAudit=1)) then
    begin//3
      sMsg:='';
      sMsg:=funPaperMsgGet(sItemId,conBtnName_Exam,qry,0);
      if sMsg<>'' then
      begin//2

        //2016.05.04 add
        if bPaperExamMergeToConfirm then
          begin
            funPaperExam(
              qry,
              tTable,
              CanbRunFLow,
              CanbAudit,
              sRealTableNameMas1,
              sUserId,
              sItemId
              );

            result:=true;
            exit;
          end;

        if MsgDlgJS('您有審核權限，是否要直接核准此單據？',mtConfirmation,[mbYes,mbNo],0)=mrYes then
          begin//1
            funPaperExam(
              qry,
              tTable,
              CanbRunFLow,
              CanbAudit,
              sRealTableNameMas1,
              sUserId,
              sItemId
              );

            result:=true;
            exit;
          end;//1
      end //2
      else
      begin //2A
        funPaperExam(
          qry,
          tTable,
          CanbRunFLow,
          CanbAudit,
          sRealTableNameMas1,
          sUserId,
          sItemId
          );

          result:=true;
          exit;
      end; //2A
    end;//3

 end //4 //if sFlag='' then

 else if ((sFlag='1') and (CanbAudit=1)) then
 begin //4A
        funPaperExam(
          qry,
          tTable,
          CanbRunFLow,
          CanbAudit,
          sRealTableNameMas1,
          sUserId,
          sItemId
          );

          result:=true;
          exit;
 end;//4A  //else if ((sFlag='1') and (CanbAudit=1)) then

 //2010.11.13 disable by Garfield for QU Johnson-201008
 {
  if iFinished=3 then
    begin
      MsgDlgJS('此單據「審核中」,不須重複操作',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

//送審
  sSQL:= 'exec CURdPaperAction '
        +''''+sRealTableNameMas1+''''+','
        +''''+sPaperNum+''''+','
        +''''+sUserId+''''
        +',1,3';
  }

  //2010.11.13 modify by Garfield for QU Johnson-201008
  sSQL:= 'exec CURdPaperDoNewStatus '
        +''''+sRealTableNameMas1+''''+','
        +''''+sPaperNum+''''+','
        +''''+sUserId+''''
        +',1,3,'+''''+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      //ExecSQL;
      //close;
      Open; //2010.11.13 modify by Garfield for QU Johnson-201008
    end;

  //-----2010.11.13 add by Garfield for QU Johnson-201008
  sMsg:='';
  if qry.RecordCount>0 then
    sMsg:=qry.Fields[0].AsString;

  qry.Close;

  if copy(sMsg,1,2)<>'OK' then
    begin
      MsgDlgJS(sMsg,mtError,[mbOk],0);
      result:=false;
      exit;
    end;
  //-----

  tTable.Close;
  tTable.Open;
  tTable.Locate('PaperNum',sPaperNum,[loCaseInsensitive]);
  //MsgDlgJS('已完成「送審」',mtInformation,[mbOk],0);

  if lowercase(sRealTableNameMas1)<>'fmedpassmain' then
    MsgDlgJS(sMsg,mtInformation,[mbOk],0);//2010.11.13 modify by Garfield for QU Johnson-201008

end //5  //if sFlowResult='NOFLOW' then

else if sFlowResult='INTOFLOW' then
begin //5A
  tTable.Close;
  tTable.Open;
  tTable.Locate('PaperNum',sPaperNum,[loCaseInsensitive]);
  MsgDlgJS('已進入電子簽核流程',mtInformation,[mbOk],0);

end //5A //else if sFlowResult='INTOFLOW' then

else if sFlowResult='' then
begin //5B
  MsgDlgJS('發生錯誤',mtError,[mbOk],0);
  result:=false;
  exit;//2012.09.05 add
end //5B //else if sFlowResult='' then

else if sFlowResult<>'' then
begin//5C

  //----------2012.08.31 add for CMT Bill-20120829-02
  if copy(sFlowResult,1,2)='OK' then
  begin
    tTable.Close;
    tTable.Open;
    tTable.Locate('PaperNum',sPaperNum,[loCaseInsensitive]);
    MsgDlgJS(sFlowResult,mtInformation,[mbOk],0);

  end
  else
  begin
  //----------

    MsgDlgJS(sFlowResult,mtError,[mbOk],0);
    result:=false;
    exit;//2012.09.05 add
  end;

end; //5C //else if sFlowResult<>'' then

  result:=true;
end;

function funPaperRejExam(
  //frm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CanbRunFLow,
  CanbAuditBack:integer;
  sRealTableNameMas1,
  sUserId:string;
  CanbMustNotes:integer;
  sItemId:string
  ):boolean;
var iFinished:integer;sPaperNum,sSQL,sReason,sMsg,sStatus:string;
begin
  if tTable.Active=false then
    begin
      MsgDlgJS('資料表未開啟',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.RecordCount=0 then
    begin
      MsgDlgJS('無資料可供處理',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sPaperNum:=tTable.FieldByName('PaperNum').Asstring;

  if sPaperNum='' then
    begin
      MsgDlgJS('沒有單據號碼',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sMsg:='';
  sMsg:=funPaperMsgGet(sItemId,conBtnName_RejExam,qry,0);
  if sMsg<>'' then
    //if MsgDlgJS('確定要退審 單號:'+sPaperNum+' ?',
    if MsgDlgJS(sMsg,
      mtConfirmation,[mbYes,mbNo],0)<>mrYes then //2023.08.18 =mrNo then
    begin
      result:=false;
      exit;
    end;

  iFinished:=tTable.FieldByName('Finished').AsInteger;

  if iFinished=0 then
    begin
      MsgDlgJS('此單據「作業中」,不須退審',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=2 then
    begin
      MsgDlgJS('此單據「已作廢」,不可退審',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=4 then
    begin
      MsgDlgJS('此單據「已結案」,不可退審',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if ((CanbRunFLow=1) and (CanbAuditBack=0)) then
    begin
      MsgDlgJS('您沒有「退審」的權限',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.FieldByName('PaperId').AsString<>'' then
    begin
      //2021.01.22 薪資拋轉的要可以修改
      //if tTable.FieldByName('PaperId').AsString<>'HSAdSalaryHisMain' then
      //2021.07.29 廠商費用單、員工費用單只要有PaperId就放行
      if (sRealTableNameMas1<>'MPHdExtendMain') and (sRealTableNameMas1<>'MPHdPettyMain') then
      begin
        MsgDlgJS('此單據是其它單據拋轉而來，請由原單據修改',mtError,[mbOk],0);
        result:=false;
        exit;
      end;
    end;

  //2010.11.13 disable by Garfield for QU Johnson-201008
  {if (iFinished=3) then
      MsgDlgJS('此單據「審核中」,不可退審',mtError,[mbOk],0);
      result:=false;
      exit;
  }

  //-----2010.11.13 add by Garfield for QU Johnson-201008
  if (iFinished=3) and (lowercase(sRealTableNameMas1)='fmedpassmain') then
    begin
      MsgDlgJS('此單據「審核中」,不可退審',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if (iFinished=3) and (lowercase(sRealTableNameMas1)<>'fmedpassmain') then
    begin
      sReason:='';

      if CanbMustNotes=1 then
        begin
          if InputQuery('輸入','請輸入退審原因',sReason)=false then
            begin
            result:=false;
            exit;
          end;
        end;

      sReason:=trim(sReason);

      if sReason<>'' then
        if pos('''',sReason)>0 then
          sReason:=AnsiReplaceText(sReason,'''','');

      sSQL:= 'exec CURdPaperDoNewStatus '
        +''''+sRealTableNameMas1+''''+','
        +''''+sPaperNum+''''+','
        +''''+sUserId+''''
        +',0,3,N'+''''+sReason+'''';

      with qry do
        begin
          if active then close;
          sql.Clear;
          sql.Add(sSQL);
          Open;
        end;

      sMsg:='';
      if qry.RecordCount>0 then
        sMsg:=qry.Fields[0].AsString;

      qry.Close;

      if copy(sMsg,1,2)<>'OK' then
        begin
          MsgDlgJS(sMsg,mtError,[mbOk],0);
          result:=false;
        end
        else
        begin
          tTable.Close;
          tTable.Open;
          tTable.Locate('PaperNum',sPaperNum,[loCaseInsensitive]);

          MsgDlgJS(sMsg,mtInformation,[mbOk],0);
          result:=true;
        end;

        exit;
      //-----
    end;

  if CanbMustNotes=1 then
  begin
    sReason:='';
    if InputQuery('輸入','請輸入退審原因',sReason)=false then
      begin
        result:=false;
        exit;
      end;

         sReason:=trim(sReason);

      if sReason<>'' then
        if pos('''',sReason)>0 then
          sReason:=AnsiReplaceText(sReason,'''','');

    sSQL:= 'Update '+trim(sRealTableNameMas1)
              +' Set Notes=rtrim(isnull(Notes,'+''''+''''+'))+N'+''''+sReason+''''
              +' Where PaperNum='+''''+ sPaperNum+'''';

    with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      ExecSQL;
      close;
    end;

    //2021.07.07 退審原因要回寫到紀錄裡面
    sSQL:= '';
    sSQL:= 'exec CURdRejNotes '''+trim(sRealTableNameMas1)+''','''+sReason+''','''
            +sPaperNum+''','''+sItemId+''','''+sUserId+'''';

    with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      ExecSQL;
      close;
    end;
  end;

  if lowercase(sRealTableNameMas1)='fmedpassmain' then
     sStatus:='2' else sStatus:='3';

  sSQL:='';
  sSQL:= 'exec CURdPaperAction '
        +''''+sRealTableNameMas1+''''+','
        +''''+sPaperNum+''''+','
        +''''+sUserId+''''
        +',0,'+sStatus;

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      ExecSQL;
      close;
    end;

  tTable.Close;
  tTable.Open;
  tTable.Locate('PaperNum',sPaperNum,[loCaseInsensitive]);

  //MsgDlgJS('已完成「退審」',mtInformation,[mbOk],0);
  sMsg:='';
  sMsg:=funPaperMsgGet(sItemId,conBtnName_RejExam,qry,1);
  if sMsg<>'' then
    MsgDlgJS(sMsg,mtInformation,[mbOk],0);

  result:=true;
end;

function funIsMaxPaperNum(
  qry:TADOQuery;
  CurrPaperId,
  CurrUseHead,
  CurrTypeHead,
  CurrPaperNum:string;
  sUseId:string; //2010.7.23 add for YX RA10070501
  sPaperDate:string //2011.9.29 add for MUT Bill-20110928-02
  ):Boolean;
var sSQL, sMaxNum: String;
begin

  sSQL:= '';
  sSQL:= 'exec CURdGetMaxPaperNum '
        +' '''+CurrPaperId+''','
        +' '''+''','
        +' '''+CurrUseHead+''','
        //+' '''+Datetimetostr(Date)+''','
        +' '''+sPaperDate+''','  //2011.9.29 modify for MUT Bill-20110928-02
        +' '''+CurrTypeHead+''''
        +','+''''+sUseId+'''' //2010.7.23 add for YX RA10070501
        ;

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  sMaxNum:= qry.Fields[0].AsString;
  qry.Close;

  if CurrPaperNum=sMaxNum then
     Result:= true
  else
     Result:= false;
end;

function funPaperVoid(
  //frm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CanbRunFLow,
  CanbScrap:integer;
  sRealTableNameMas1,
  sUserId:string;
  CanbMustNotes:integer;
  sUseId:string;
  CurrTypeHead:string;
  CanbLockUserEdit:integer;
  sItemId:string
  ):boolean;
var iFinished:integer;sPaperNum,sSQL,sReason,sMsg:string; bDeleteMaxNum:boolean;
sPaperDate:string;//2011.9.29 add for MUT Bill-20110928-02
iCanVoid:Integer; //2021.01.19
begin
  if tTable.Active=false then
    begin
      MsgDlgJS('資料表未開啟',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.RecordCount=0 then
    begin
      MsgDlgJS('無資料可供處理',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sPaperNum:=tTable.FieldByName('PaperNum').Asstring;

  //2011.9.29 add for MUT Bill-20110928-02
  sPaperDate:=FormatDateTime('yyyy/mm/dd',tTable.FieldByName('PaperDate').AsDateTime);

  if sPaperNum='' then
    begin
      MsgDlgJS('沒有單據號碼',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  sMsg:='';
  sMsg:=funPaperMsgGet(sItemId,conBtnName_Void,qry,0);

  if sMsg<>'' then
    //if MsgDlgJS('確定要作廢 單號:'+sPaperNum+' ?',
    if MsgDlgJS(sMsg,
      mtConfirmation,[mbYes,mbNo],0)<>mrYes then//2023.08.18 =mrNo then
    begin
      result:=false;
      exit;
    end;

  iFinished:=tTable.FieldByName('Finished').AsInteger;

  if CanbRunFLow=1 then
    if iFinished=1 then
    begin
      iCanVoid:=0;
      //2021.01.19 Fix 電子發票要能直接作廢，但是可能有使用簽核流程
      with qry do
      begin
        if active then qry.close;
        sql.Clear;
        sql.Add('exec CURdCanVoidPaper '''+trim(sRealTableNameMas1)+'''');
        Open;
        if RecordCount>0 then
          iCanVoid:=1;
        qry.close;
      end;

      if //lowercase(sRealTableNameMas1)<>'fmedpassmain' then
        iCanVoid=0 then
        begin
          MsgDlgJS('此單據「已完成」,不可作廢',mtError,[mbOk],0);
          result:=false;
          exit;
        end;
    end;

  if iFinished=2 then
    begin
      MsgDlgJS('此單據「已作廢」,不須作廢',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if iFinished=4 then
    begin
      MsgDlgJS('此單據「已結案」,不可作廢',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if ((CanbRunFLow=1) and (CanbScrap=0)) then
    begin
      MsgDlgJS('您沒有「作廢」的權限',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if tTable.FieldByName('PaperId').AsString<>'' then
    begin
      MsgDlgJS('拋轉單據，請由原單據作廢',mtError,[mbOk],0);
      result:=false;
      exit;
    end;

  if CanbLockUserEdit=1 then
      begin
        if (trim(sUserId)<>trim(tTable.FieldByName('UserId').Asstring))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
        begin
         MsgDlgJS('此單據已被設定「只有建檔者可編輯及作廢」',mtWarning,[mbOk],0);
         result:=false;
         exit;
        end;
      end;

  bDeleteMaxNum:=false;

  if iFinished=0 then
    begin

      //2014.09.15 add for 解決多單頭且不是新增完單據就作廢的狀況下，
      //會傳入最近一次新增時取號當時的單頭
      //若開啟單據畫面後沒有新增就點選某筆作廢，則會傳入空白(後端SP會自動取預設)
      //的問題
      if tTable.FindField('dllHeadFirst')<>nil then
        begin
          CurrTypeHead:=tTable.FieldByName('dllHeadFirst').AsString;
        end;

      bDeleteMaxNum:=
        funIsMaxPaperNum(
          qry,//qry:TADOQuery;
          sRealTableNameMas1,//CurrPaperId,
          copy(sUseId,1,1),//CurrUseHead,
          CurrTypeHead,//CurrTypeHead
          sPaperNum, //CurrPaperNum
          sUseId, //2010.7.23 add for YX RA10070501
          sPaperDate //2011.9.29 add for MUT Bill-20110928-02
          );
    end;

if bDeleteMaxNum then
begin
    with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add('exec CURdPaperDeleteMaxNum '''+trim(sRealTableNameMas1)+''','
        +''''+sPaperNum+'''');
      ExecSQL;
      close;
    end;

    sSQL:= 'delete '+trim(sRealTableNameMas1)
              +' Where PaperNum='+''''+ sPaperNum+'''';

    with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      ExecSQL;
      close;
    end;

  tTable.Close;
  tTable.Open;

  //MsgDlgJS('已完成「刪除」',mtInformation,[mbOk],0);
  sMsg:='';
  sMsg:=funPaperMsgGet(sItemId,conBtnName_Void,qry,1);
  if sMsg<>'' then
    MsgDlgJS(sMsg,mtInformation,[mbOk],0);
end
else
begin
  if CanbMustNotes=1 then
  begin
    sReason:='';
    if InputQuery('輸入','請輸入作廢原因',sReason)=false then
      begin
        result:=false;
        exit;
      end;

      sReason:=trim(sReason);

      if sReason<>'' then
        if pos('''',sReason)>0 then
          sReason:=AnsiReplaceText(sReason,'''','');

    sSQL:= 'Update '+trim(sRealTableNameMas1)
              +' Set Notes=rtrim(isnull(Notes,'+''''+''''+'))+N'+''''+sReason+''''
              +' Where PaperNum='+''''+ sPaperNum+'''';

    with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      ExecSQL;
      close;
    end;
  end;

  sSQL:='';
  sSQL:= 'exec CURdPaperAction '
        +''''+sRealTableNameMas1+''''+','
        +''''+sPaperNum+''''+','
        +''''+sUserId+''''
        +',2,2';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      ExecSQL;
      close;
    end;

  tTable.Close;
  tTable.Open;
  tTable.Locate('PaperNum',sPaperNum,[loCaseInsensitive]);

  //MsgDlgJS('已完成「作廢」',mtInformation,[mbOk],0);
  sMsg:='';
  sMsg:=funPaperMsgGet(sItemId,conBtnName_Void,qry,1);
  if sMsg<>'' then
    MsgDlgJS(sMsg,mtInformation,[mbOk],0);
end;

  result:=true;
end;

function funCheckPaper4EngDesign2(
  tblTable:TJSdTable;
  CanbUpdate:integer;
  CanbLockUserEdit:integer;
  sUserId:string;
  sNowMode:string;
  bChkFinished03:boolean;
  bChkFinished2:boolean;
  bChkFinished1:boolean
  ):boolean;
begin
  result:=false;

  if CanbUpdate=0 then
    begin
      MsgDlgJS('您沒有編輯的權限', mtError, [mbOk], 0);
      exit;
    end;

  if tblTable.active=false then
    begin
      MsgDlgJS('資料表未開啟', mtError, [mbOk], 0);
      exit;
    end;

  if tblTable.RecordCount=0 then
    begin
      MsgDlgJS('沒有資料可供操作', mtError, [mbOk], 0);
      exit;
    end;

  if tblTable.FieldByName('PaperNum').Asstring='' then
        begin
          MsgDlgJS('沒有「單據號碼」可供操作', mtError, [mbOk], 0);
          exit;
        end;

  if bChkFinished2 then
    if (tblTable.FieldByName('Finished').AsInteger=2) then
        begin
          MsgDlgJS('「已作廢」的單據不可使用此功能', mtError, [mbOk], 0);
          exit;
        end;

  if bChkFinished03 then
    if not(tblTable.FieldByName('Finished').AsInteger in[0,3]) then
        begin
          MsgDlgJS('只有「作業中」及「審核中」的單據才可使用此功能', mtError, [mbOk], 0);
          exit;
        end;

  if bChkFinished1 then
    if (tblTable.FieldByName('Finished').AsInteger<>1) then
        begin
          MsgDlgJS('只有「已確認」的單據才可使用此功能', mtError, [mbOk], 0);
          exit;
        end;

  if ((CanbLockUserEdit=1) and (trim(tblTable.FieldByName('UserId').Asstring)<>'')) then
    if (trim(sUserId)<>trim(tblTable.FieldByName('UserId').Asstring))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
        begin
         MsgDlgJS('此單據已被設定「只有建檔者可編輯」',mtWarning,[mbOk],0);
         exit;
        end;

  if sNowMode<>'UPDATE' then
      begin
         MsgDlgJS('必須在「編輯中」才可使用此功能',mtWarning,[mbOk],0);
         exit;
      end;
  result:=true;
end;



function funCheckPaper4EngDesign(
  tblTable:TJSdTable;
  CanbUpdate:integer;
  CanbLockUserEdit:integer;
  sUserId:string;
  sNowMode:string
  ):boolean;
begin
  result:=false;

  if CanbUpdate=0 then
    begin
      MsgDlgJS('您沒有編輯的權限', mtError, [mbOk], 0);
      exit;
    end;

  if tblTable.active=false then
    begin
      MsgDlgJS('資料表未開啟', mtError, [mbOk], 0);
      exit;
    end;

  if tblTable.RecordCount=0 then
    begin
      MsgDlgJS('沒有資料可供操作', mtError, [mbOk], 0);
      exit;
    end;

  if not(tblTable.FieldByName('Finished').AsInteger in[0,3]) then
        begin
          MsgDlgJS('只有「作業中」及「審核中」的單據才可使用此功能', mtError, [mbOk], 0);
          exit;
        end;

  if tblTable.FieldByName('PaperNum').Asstring='' then
        begin
          MsgDlgJS('沒有「單據號碼」可供操作', mtError, [mbOk], 0);
          exit;
        end;

  if ((CanbLockUserEdit=1) and (trim(tblTable.FieldByName('UserId').Asstring)<>'')) then
    if (trim(sUserId)<>trim(tblTable.FieldByName('UserId').Asstring))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
        begin
         MsgDlgJS('此單據已被設定「只有建檔者可編輯」',mtWarning,[mbOk],0);
         exit;
        end;

  if sNowMode<>'UPDATE' then
      begin
         MsgDlgJS('必須在「編輯中」才可使用此功能',mtWarning,[mbOk],0);
         exit;
      end;
  result:=true;
end;

function funCustButtonDo(
  fFrm:TForm;
  sBtnName:string;
  tTable:TJSdTable;
  qryExec:TADOQuery;
  sItemId,
  sGlobalId,
  sSystemId,
  sServerName,
  sDBName,
  sUserId,
  sBUID,
  sUseId,
  sRealTableNameMas1:string;
  iCanbUpdate,
  iOPKind:integer;
  sPaperUserId:string;
  CanbLockUserEdit:integer;
  sLoginSvr:string; //new 2009.7.22
  sLoginDB:string;  //new 2009.7.22
  sPaperMode:string;
  tTblDtl:TJSdTable; //new 2009.8.26
  sTranGlobalId:string; //2009.9.1 add
  hMain_btnPrint_Handle:THandle//2010.11.8 add
  ):boolean;
var
 sSQL,
 sBeCallDLLName,
 sBeCallClassName,
 sNowPaperNum,
 sSpName,
 sTblName,
 sSearchTemplate:string;
 iChkCanbUpdate,
 iChkStatus,
 ibNeedNum,
 ibNeedInEdit,
 iDesignType,
 ibSpHasResult,
 i,
 j:integer;
 hHandle:THandle;
 sReMsg,
 sHead,
 sBody: String;
 iDtlItem:integer;//new 2009.8.26
 sEditGridTblName:string;//new 2009.8.29
 sRealEGTblName:string;//new 2009.8.26
 sDialogCaption:string;
 ibTranByGlobalId:integer;//2009.8.31 add
 sInsertTranSQLHead:string;//2009.9.1 add
 sInsertTranSQLAll:string;//2009.9.1 add
 qryTemp:TADOQuery;
 sSysGlobalId:string;//2009.9.4 add
 bk:TBookmark;
 bkDtl:TBookmark;
 sDLLForceFree:string;
 iInsPaperMsg:integer;//2009.12.28 add
 sToPaperId:string;//2009.12.29 add
 sMsgToUserId:string;//2009.12.28 add

 sPrintSQL:string;
 sPrintSp:string;
 sPrintRptName:widestring;
 rptPaper: TJSdReport;
 k,iFieldCount:integer;
 sParamArray:array of WideString;
 qryTmp:TADOQuery;

 sBeforeRunSQL:string;
 IsUpdateMoney:integer;//2010.7.21 add

 sNewPohtoName,sServerPohtoName:string;
 sGetPohtoSQL,sGetPohtoSQLComplete:string;
 sLocalPath:string;
 iA:integer;
 iB:integer;//2011.3.9 add
 sBUID_org:string;//2011.3.9 add
 //iMasterRecNo,iDtlRecNo:integer;
 iNeedBar:integer;//2011.4.14 add
 iPreChk_UseAddon1:integer;//2011.4.15 add
 iNeedConfirmBefExec:integer;//2012.2.7 add
 sConfirmBefExec:string;//2012.2.7 add
 iReShowDlg:integer;//2012.03.14 add for MUT Bill-20120307-04

 sCustCaption:widestring;//2012.04.03 add for MUT Bill-20120329-04

 sTempBasJSISpw:string;//2012.06.01 modify for SS Bill-20120531-01

 iCallPaperAftTran:integer;//2012.11.16 add for SS Bill-20121102-07
 //iCallPaperPowerType:integer;//2012.11.16 add for SS Bill-20121102-07

 iDlgSearchWidth:integer;//2015.11.12 add for SS Bill-20151111-01
 iDlgSearchHeight:integer;//2015.11.12 add for SS Bill-20151111-01
 bF9:boolean; //2019.11.11
 sList:TstringList;
 sFontSize:string;
 FontSize: integer;
begin
  result:=false;

  sTempBasJSISpw:=tTable.ReserveList.Values['TempBasJSISpw'];//2012.06.01 add for SS Bill-20120531-01
  //2019.11.11
  bF9:=false;
  if tTable.ReserveList.Values['bF9']='1' then
    bF9:=true;

  if sTempBasJSISpw='' then sTempBasJSISpw:='JSIS';//2012.06.01 add for SS Bill-20120531-01

  sBUID_org:=sBUID;//2011.3.9 add

  //sSQL:='select * from CURdOCXItemCustButton(nolock) where ItemId='+
  //      ''''+sItemId+''''+' and ButtonName='+''''+sBtnName+''''+' and bVisible=1';
  sSQL:='exec CURdCustButtonInfoGet '+
        ''''+sItemId+''''+','+
        ''''+sBtnName+''''+','+
        ''''+sGlobalId+''''
        ;

  with qryExec do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qryExec.RecordCount=0 then
    begin
      MsgDlgJS('程式項目['+sItemId+']的自訂按鈕['+sBtnName+']未設定',mtWarning,[mbOK],0);
      qryExec.Close;
      exit;
    end;

  with qryExec do
    begin
      sBeCallDLLName:=FieldByName('OCXName').AsString;
      sBeCallClassName:=FieldByName('CoClassName').AsString;
      iChkCanbUpdate:=FieldByName('ChkCanbUpdate').AsInteger;
      iChkStatus:=FieldByName('ChkStatus').AsInteger;
      ibNeedNum  :=FieldByName('bNeedNum').AsInteger;
      ibNeedInEdit  :=FieldByName('bNeedInEdit').AsInteger;
      iDesignType:=FieldByName('DesignType').AsInteger;
      ibSpHasResult:=FieldByName('bSpHasResult').AsInteger;
      sSpName:=FieldByName('SpName').AsString;
      sSearchTemplate:=FieldByName('SearchTemplate').AsString;
      ibTranByGlobalId:=FieldByName('bTranByGlobalId').Asinteger;
      iInsPaperMsg:=FieldByName('bInsPaperMsg').Asinteger;//2009.12.28 add
      sToPaperId:=FieldByName('ToPaperId').AsString;//2009.12.29 add
      sPrintSQL:=FieldByName('PrintSQL').AsString;
      sPrintSp:=FieldByName('PrintSp').AsString;
      sPrintRptName:=FieldByName('PrintRptName').AswideString;
      sBeforeRunSQL:=FieldByName('BeforeRunSQL').AsString;//2010.5.26 add
      IsUpdateMoney:=FieldByName('IsUpdateMoney').Asinteger;//2010.7.21 add
      sNewPohtoName:=FieldByName('NewPohtoName').AsString;
      sGetPohtoSQL:=FieldByName('GetPohtoSQL').AsString;
      iNeedBar:=FieldByName('iNeedBar').Asinteger;//2011.4.14 add
      iPreChk_UseAddon1:=FieldByName('iPreChk_UseAddon1').Asinteger;//2011.4.15 add
      iNeedConfirmBefExec:=FieldByName('iNeedConfirmBefExec').Asinteger;//2012.2.7 add
      iReShowDlg:=FieldByName('iReShowDlg').Asinteger;//2012.03.14 add for MUT Bill-20120307-04
      sCustCaption:=FieldByName('CustCaption').Aswidestring;//2012.04.03 add for MUT Bill-20120329-04

      //2012.2.7 add
      if (FieldByName('iNeedConfirmBefExec').Asinteger=1) and
         (FieldByName('sConfirmBefExec').AsString='')
      then
         sConfirmBefExec:='確認要「'+FieldByName('CustCaption').AsString+'」嗎？'
      else
         sConfirmBefExec:=FieldByName('sConfirmBefExec').AsString;

      //2012.11.16 add for SS Bill-20121102-07
      iCallPaperAftTran:=FieldByName('iCallPaperAftTran').Asinteger;
      //iCallPaperPowerType:=FieldByName('iCallPaperPowerType').Asinteger;

    end;

//2016.08.16 add
if ibNeedInEdit=1 then
  begin
    if sPaperMode<>'UPDATE' then
      begin
         MsgDlgJS('必須在「編輯中」才可使用此功能',mtWarning,[mbOk],0);
         exit;
      end;
  end;

  //----------2015.11.12 add for SS Bill-20151111-01
  iDlgSearchWidth:=0;
  iDlgSearchHeight:=0;

  if qryExec.FindField('iDlgSearchWidth')<>nil then
    begin
       iDlgSearchWidth:=qryExec.FieldByName('iDlgSearchWidth').Asinteger;
    end;

  if qryExec.FindField('iDlgSearchHeight')<>nil then
    begin
       iDlgSearchHeight:=qryExec.FieldByName('iDlgSearchHeight').Asinteger;
    end;
  //----------

   //2010.5.26 add
  if sBeforeRunSQL<>'' then
   begin
    funReplaceCom(sBeforeRunSQL,tTable);

    if pos('@PaperNum',sBeforeRunSQL)>0 then
        sBeforeRunSQL:=
          AnsiReplaceText(sBeforeRunSQL,'@PaperNum',
            ''''+tTable.FieldByName('PaperNum').AsString+'''');

    prcSaveALL(fFrm);

    OpenSQLDLL(qryExec,'EXEC',sBeforeRunSQL); //若raiserror 會自動中斷
   end;

  if iDesignType=4 then //列印
  begin
    if sPrintSQL='' then
    begin
      MsgDlgJS('未設定「DLL設定」裡的PrintSQL', mtError, [mbOk], 0);
      exit;
    end;

    if sPrintSp='' then
    begin
      MsgDlgJS('未設定「DLL設定」裡的PrintSp', mtError, [mbOk], 0);
      exit;
    end;

    if sPrintRptName='' then
    begin
      MsgDlgJS('未設定「DLL設定」裡的PrintRptName', mtError, [mbOk], 0);
      exit;
    end;

    //funReplaceCom(sPrintSQL,tTable);
    sPrintSQL:=funReplaceCom(sPrintSQL,tTable);//2017.03.30 modify

    if pos('@PaperNum',sPrintSQL)>0 then
        sPrintSQL:=
          AnsiReplaceText(sPrintSQL,'@PaperNum',
            ''''+tTable.FieldByName('PaperNum').AsString+'''');

    OpenSQLDLL(qryExec,'OPEN',sPrintSQL);

    if qryExec.RecordCount=0 then
        begin
          MsgDlgJS('使用「DLL設定」的PrintSQL，沒有擷取到資料',mtError,[mbOk],0);
          exit;
        end;

    iFieldCount:=qryExec.FieldCount;

    SetLength(sParamArray,iFieldCount);

    try
        rptPaper:= TJSdReport.Create(nil);
        //rptPaper.ReportTitle:= tTable.TableName;
        rptPaper.ReportTitle:= sItemId+' '+sCustCaption+'(Preview)';//2012.04.03 add for MUT Bill-20120329-04
        rptPaper.ReportFileName:=sPrintRptName;

        qryTmp:=TADOQuery.Create(nil);
        qryTmp.ConnectionString:=qryExec.ConnectionString;
        qryTmp.CommandTimeout:=9600;

        sLocalPath:= trim(DLLGetTempPathStr)+sBUId+'\';

        qryExec.First;

        while not qryExec.Eof do
        begin
          if sGetPohtoSQL<>'' then
            begin
              if sNewPohtoName='' then
              begin
    		            MsgDlgJS('沒有設定「圖檔下載到Profile的新檔名」', mtError, [mbOk], 0);
                    Break;
                    Exit;
              end;

              sServerPohtoName:='';

              sGetPohtoSQLComplete:=sGetPohtoSQL;

              for iA:=0 to qryExec.FieldCount-1 do
                begin
                  sGetPohtoSQLComplete:=sGetPohtoSQLComplete+' '+''''+qryExec.Fields[iA].AsWideString+'''';
                  if iA< qryExec.FieldCount-1 then sGetPohtoSQLComplete:=sGetPohtoSQLComplete+',';
                end;

              OpenSQLDLL(qryTmp,'OPEN',sGetPohtoSQLComplete);

              if qryTmp.RecordCount>0 then sServerPohtoName:=qryTmp.Fields[0].AsString;

              qryTmp.Close;

              if sServerPohtoName='' then
              begin
    		            MsgDlgJS('沒有圖檔', mtError, [mbOk], 0);
                    Break;
                    Exit;
              end
              else
              begin
                if not FileExists(sServerPohtoName) then
                  begin
     		            MsgDlgJS('圖檔'+sServerPohtoName+'不存在', mtError, [mbOk], 0);
                    Break;
                    Exit;
                  end;

                if CopyFileStrDLL(sServerPohtoName,sLocalPath+sNewPohtoName)=false then
                  begin
     		            MsgDlgJS('下載圖檔...失敗', mtError, [mbOk], 0);
                    Break;
                    Exit;
                  end;
              end;

              qryTmp.Close;
              qryTmp.SQL.Clear;
            end; //if sGetPohtoSQL<>'' then

          for i := low(sParamArray) to high(sParamArray) do
            sParamArray[i]:=qryExec.Fields[i].AsWideString;

          RunJSdReportDLL(
              rptPaper,//JsRpt: TJSdReport;
              sPrintSp,//trim(ThisOperate)+'Paper',//ProcName: WideString;
              sParamArray,//ParamList: array of WideString;
              '',//sIndex,
              sPrintRptName,//sRptName: WideString;
              //=====
              qryTmp,
              sSystemId,
              sBUId,
              sUserId,
              nil, //AftPrn: TNotifyEVent
              false,
              //=====
              hMain_btnPrint_Handle,
              sGlobalId,
              1 //iShowModal
              );

           qryExec.Next;
        end;
      finally
        rptPaper.Free;
        qryTmp.Close;
        qryTmp.Free;
      end;
   result:=true;//2010.11.25 add
   EXIT;

  end; //if iDesignType=4 then

  if iDesignType in[0,2] then //0:自寫 1:Template 2:繼承Template
  begin
    if sBeCallDLLName='' then
    begin
      MsgDlgJS('未設定DLL名稱', mtError, [mbOk], 0);
      exit;
    end;
  end;

  if iDesignType<>1 then sSearchTemplate:='';//0:自寫 1:Template 2:繼承Template

  if iDesignType=1 then
    if sSearchTemplate='' then
    begin
      MsgDlgJS('未設定自訂按鈕所使用的模版', mtError, [mbOk], 0);
      exit;
    end;

  if iChkCanbUpdate=1 then
  begin
    if iCanbUpdate=0 then
    begin
      MsgDlgJS('您沒有編輯的權限', mtError, [mbOk], 0);
       exit;
    end;
  end;

prcSaveALL(fFrm);

if iOPKind=1 then //單據
begin
  if ((iChkStatus=1) or (ibNeedNum=1)) then
  begin
    if tTable.active=false then
    begin
      MsgDlgJS('資料表未開啟', mtError, [mbOk], 0);
      exit;
    end;

    if tTable.RecordCount=0 then
    begin
      MsgDlgJS('沒有資料可供操作', mtError, [mbOk], 0);
      exit;
    end;
  end;

  if iChkStatus=1 then
    if not(tTable.FieldByName('Finished').AsInteger in[0,3]) then
        begin
          if IsUpdateMoney=0 then //2010.7.21 add 'if'
            begin
              MsgDlgJS('只有「作業中」及「審核中」的單據才可使用此功能', mtError, [mbOk], 0);
              exit;
            end
          else
            begin
              if (tTable.FieldByName('Finished').AsInteger=2) then
                begin
                  MsgDlgJS('單據「已作廢」，不可使用此功能', mtError, [mbOk], 0);
                  exit;
                end;
            end;
        end;

  sNowPaperNum:=tTable.FieldByName('PaperNum').Asstring;//2011.1.28 搬上來 for debug

  if ibNeedNum=1 then
  begin
    //sNowPaperNum:=tTable.FieldByName('PaperNum').Asstring;

    if sNowPaperNum='' then
        begin
          MsgDlgJS('沒有「單據號碼」可供操作', mtError, [mbOk], 0);
          exit;
        end;
  end;

  if iChkCanbUpdate=1 then
  begin
    if ((CanbLockUserEdit=1) and (trim(sPaperUserId)<>''))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
      begin
        if trim(sUserId)<>trim(sPaperUserId) then
        begin
         MsgDlgJS('此單據已被設定「只有建檔者可編輯」',mtWarning,[mbOk],0);
         exit;
        end;
      end;
  end;

  if ibNeedInEdit=1 then
  begin
    if sPaperMode<>'UPDATE' then
      begin
         MsgDlgJS('必須在「編輯中」才可使用此功能',mtWarning,[mbOk],0);
         exit;
      end;
  end;
end;//if iOPKind=1 then

//==========2009.8.31 add
if ibTranByGlobalId=1 then
  begin
    sSQL:='';

    sSQL:='exec CURdOCXItmCusBtnTranPmGet '+
        ''''+sItemId+''''+','+''''+sBtnName+''''+','+inttostr(iOpKind);

    with qryExec do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

    if qryExec.RecordCount=0 then
    begin
      MsgDlgJS('此自訂按鈕沒有輸入「傳遞資料給被呼叫的DLL之設定」',mtWarning,[mbOk],0);
      exit;
    end
    else
    begin
      if funTranDataInsert(
          fFrm,
          sGlobalId,
          sTranGlobalId,
          sItemId,
          sBtnName,
          sBeCallDLLName,
          sUserId,
          sUseId,
          sSystemId,
          qryExec,
          tTable
          )=false then
        begin
          exit;
        end;
     end;//if qryExec.RecordCount>0 then
  end
  else
  begin
    sTranGlobalId:='';
  end;//if ibTranByGlobalId=1 then
//==========

//=====2009.9.4 add
if (iDesignType in[1,2]) then
  if ( (lowercase(sSearchTemplate)='jsdpapersearchdll.dll') or
       (lowercase(sSearchTemplate)='jsdcondrunspdll.dll')
     )
  then
  begin
    sSysGlobalId:=funGlobalIdGet(sUserId);

    sSQL:='';
    sSQL:='exec CURdOCXSearchParamsTableGet '+
        ''''+sItemId+''''+','+''''+sBtnName+''''+','+inttostr(iOpKind);

    with qryExec do
      begin
        close;
        sql.Clear;
        sql.Add(sSQL);
        open;
      end;

    if qryExec.RecordCount>0 then
      begin
        if funTranDataInsert(
          fFrm,
          sGlobalId,
          sSysGlobalId,//sTranGlobalId,
          sItemId,
          sBtnName,
          sBeCallDLLName,
          sUserId,
          sUseId,
          sSystemId,
          qryExec,
          tTable
          )=false then
        begin
          exit;
        end;
      end
      else
      begin
        sSysGlobalId:='';
      end;
  end;
//=====

//=====2009.8.29 add
if (iDesignType in[1,2]) then
  if (lowercase(sSearchTemplate)='jsdeditgriddll.dll') then
  begin
    with qryExec do
      begin
        close;
        sql.Clear;
        sql.add('exec CURdOCXItmCusBtnRGGet '+
          ''''+sItemId+''''+','+''''+sBtnName+''''+','+inttostr(iOpKind));
        Open;
      end;

    sDialogCaption:=qryExec.FieldByName('DialogCaption').AsString;

    case qryExec.FieldByName('iEditGridType').AsInteger of
      0:begin //以固定TableName開啟
          sRealTableNameMas1:='select * from '+
              qryExec.FieldByName('SpName').AsString+'^'+
             qryExec.FieldByName('MultiSelectDD').AsString;
        end;
      1:begin //以固定SQL開啟
          sRealTableNameMas1:=
              qryExec.FieldByName('SpName').AsString+'^'+
             qryExec.FieldByName('MultiSelectDD').AsString;
        end;
      2:begin //以動態取某 Table 之某欄位值做為 TableName 開啟
          sTblName:='';
          sTblName:=qryExec.fieldbyname('ClientTblName').AsString;
          sEditGridTblName:=qryExec.fieldbyname('EditGridTableField').AsString;

          if fFrm.FindComponent(sTblName)=nil then
             begin
                MsgDlgJS('物件「'+sTblName+'」不存在',mtError,[mbOk],0);
                exit;
             end;

          if qryExec.fieldbyname('EditGridTableField').AsString='' then
             begin
                MsgDlgJS('「動態取Table Name之欄位名稱」設定錯誤',mtError,[mbOk],0);
                result:=false;
                exit;
             end;

          if TJSdTable(fFrm.FindComponent(sTblName)).FindField(sEditGridTblName)=nil then
             begin
                MsgDlgJS('無此欄位「'+sEditGridTblName+'」',mtError,[mbOk],0);
                exit;
             end;

          sRealEGTblName:=TJSdTable(fFrm.FindComponent(sTblName)).FieldByName(sEditGridTblName).AsString;

          sRealTableNameMas1:='select * from '+sRealEGTblName+'^';

          if qryExec.FieldByName('MultiSelectDD').AsString='' then
            sRealTableNameMas1:=sRealTableNameMas1+sRealEGTblName
          else
            sRealTableNameMas1:=sRealTableNameMas1+
                   qryExec.FieldByName('MultiSelectDD').AsString;
        end;
    end;
  end;
//=====

if iDesignType=3 then //呼叫sp
begin
    sSpName:='exec '+sSpName+' ';

    sSQL:='exec CURdOCXItmCusBtnParamGet '+
        ''''+sItemId+''''+','+''''+sBtnName+''''+','+inttostr(iOpKind);

    with qryExec do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

    if qryExec.RecordCount>0 then
    begin
      with qryExec do
        begin
          first;
          i:=1;

          while not eof do
            begin
              if i>1 then sSpName:=sSpName+',';

              case fieldbyname('ParamType').asinteger of
                //欄位值
                0:begin
                  sTblName:='';
                  sTblName:=fieldbyname('ClientTblName').AsString;

                  if fFrm.FindComponent(sTblName)=nil then
                    begin
                      MsgDlgJS('物件「'+sTblName+'」不存在',mtError,[mbOk],0);
                      exit;
                    end
                    else
                    begin
                      sSpName:=sSpName+''''+
                        TJSdTable(fFrm.FindComponent(sTblName)).FieldByName(qryExec.fieldbyname('ParamFieldName').AsString).AsString
                        +'''';
                    end;
                  end;
                //常數
                1:sSpName:=sSpName+''''+qryExec.fieldbyname('ParamFieldName').AsString+'''';
                //操作者
                2:sSpName:=sSpName+''''+sUserId+'''';
                //公司別
                3:sSpName:=sSpName+''''+sUseId+'''';
                //系統別
                4:sSpName:=sSpName+''''+sSystemId+'''';
                //目前單號
                5:sSpName:=sSpName+''''+tTable.FieldByName('PaperNum').Asstring+'''';
              end;

              inc(i);
              next;
            end;//with qryExec do

        end;//if qryExec.RecordCount>0 then

    end;

    //2012.2.7 add
    if ((iNeedConfirmBefExec=1) and (sConfirmBefExec<>'')) then
      begin
        if MsgDlgJS(sConfirmBefExec,mtConfirmation,[mbYes,mbNo],0)<>mrYes then //2023.08.18 =mrNo then
          exit;
      end;

    with qryExec do
      begin
        if active then close;
        sql.Clear;
        sql.Add(sSpName);
        if ibSpHasResult=1 then
          open
          else
          begin
            ExecSQL;
            close;
          end;
      end;

    if ibSpHasResult=1 then
      begin
      //---------------------------------
            sReMsg:='';
            if qryExec.RecordCount>0 then
              begin
                sReMsg:=qryExec.Fields[0].AsString;
              end;
            qryExec.Close;

            if sReMsg<>'' then
              begin
                if copy(sReMsg,1,2)<>'OK' then
                  begin
                    sHead:=copy(sReMsg,1,6);
                    sBody:=copy(sReMsg,7,length(sReMsg));

                    if sHead='MESGE:' then MsgDlgJS(sBody, mtInformation, [mbOk], 0)
                    else if sHead='ERROR:' then MsgDlgJS(sBody, mtError, [mbOk], 0)
                    else if sHead='ABORT:' then MsgDlgJS(sBody, mtError, [mbOk], 0)
                    else if sHead='CONFI:' then MsgDlgJS(sBody, mtInformation, [mbOk], 0);

                  end;
              end;
      //---------------------------------
      end;

    if Assigned(tTblDtl) then
      if tTblDtl.Active then
        if tTblDtl.RecordCount>0 then
          try bkDtl:=tTblDtl.GetBookmark; except end;

    if tTable.RecordCount>0 then try bk:=tTable.GetBookmark; except end;

    //iMasterRecNo:=tTable.RecNo; //,iDtlRecNo:integer;

    tTable.Close;
    tTable.Open;

    //tTable.Locate('PaperNum',sNowPaperNum,[loCaseInsensitive]);
    if (tTable.RecordCount>0) then //and (tTable.RecordCount >= iMasterRecNo)) then
      if assigned(bk) then try tTable.GotoBookmark(bk);except end;

    if assigned(bk) then try tTable.FreeBookmark(bk); except end;

    if Assigned(tTblDtl) then
           if tTblDtl.Active then
             if tTblDtl.RecordCount>0 then
               if assigned(bkDtl) then
                 //for EMO 請轉採
                 if tTblDtl.TableName<>'MPHdSendOrderQnty' then
                 try tTblDtl.GotoBookmark(bkDtl); except end;//因若是刪除明細後 又要指回該明細就會出現「書簽無效」

     if assigned(bkDtl) then try tTblDtl.FreeBookmark(bkDtl);except end;
end;//if iDesignType=3 then //呼叫sp

{2009.12.17 disable 因在funCallDLL會處理,且寫法有誤(一個Handle被釋放後,OS會給其它的DLL用)
      只用ItemId=sBeCallDLLName判斷會誤Free其它正使用的物件,若該物件是JSIS.exe,就會造成JSIS.exe被Free
//=====強制 Free
sDLLForceFree:=unit_DLL.funDLLSysParamsGet(qryExec,'CUR','DLLForceFree');
if sDLLForceFree='1' then
begin

  sSQL:=
    'select distinct iHandle from CURdOCXHandleLog(nolock) where MainGlobalId='+
    ''''+sGlobalId+''''+
    ' and ItemId='+''''+sBeCallDLLName+'''';

  with qryExec do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qryExec.RecordCount>0 then
    begin
      with qryExec do
        begin
          first;
          while not eof do
            begin
              FreeLibrary(fieldbyname('iHandle').AsInteger);
              next;
            end;
        end;
    end;

  qryExec.Close;
end;
//=====
}

if iDesignType in[0,1,2] then
begin
  iDtlItem:=0;//new 2009.8.26

  if Assigned(tTblDtl) then //new 2009.8.26
    if tTblDtl.Active then
      if tTblDtl.RecordCount>0 then
        begin
         if tTblDtl.FindField('Item')<>nil then
            iDtlItem:=tTblDtl.FieldByName('Item').AsInteger;

         bkDtl:=tTblDtl.GetBookmark;
        end;

  if sDialogCaption='' then  sDialogCaption:=sBeCallDLLName;

  if sSysGlobalId<>'' then sBUID:=sBUID+'^'+sSysGlobalId;//2009.9.4 add

  if iDesignType=1 then
  begin
    //====================================
    unit_DLL2.prcGetSvrName_CpnyUseId(
      qryExec,
      sServerName,
      sDBName,
      sLoginSvr,
      sLoginDB,
      sUseId
      );
    //====================================
    sSystemId:=sItemId+'~'+sBtnName+'^'+tTable.ReserveList.Values['LanguageId'];

    if (lowercase(sSearchTemplate)='jsdpapersearchdll.dll') then
    begin
      Application.CreateForm(TfrmJSdPaperSearchDLL, frmJSdPaperSearchDLL);

      //2020.03.10
      if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
      begin
          sList:=TstringList.Create;
          sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
          sFontSize:=sList.Strings[0];
          sList.Free;
          FontSize := StrToInt(sFontSize);
          frmJSdPaperSearchDLL.Scaled:=true;
          frmJSdPaperSearchDLL.ScaleBy(FontSize,100);
          frmJSdPaperSearchDLL.Width:= Round(frmJSdPaperSearchDLL.Width * (FontSize/100));
      end;

      //2012.09.21 add ,因有使用的僅是極少數特殊的DLL,沒必要因此造成版本不相容
      if frmJSdPaperSearchDLL.sConnectStrAdmin='' then
        if frmJSdPaperSearchDLL.sTempBasJSAdpw='' then
          if TJSdTable(tTable).ReserveList.Values['TempBasJSAdpw']<>'' then
          begin
            frmJSdPaperSearchDLL.sTempBasJSAdpw:=TJSdTable(tTable).ReserveList.Values['TempBasJSAdpw'];
          end;

      frmJSdPaperSearchDLL.pnlInfo.Caption:=
      'sServerName='+sServerName+';'+
      'sDBName='+sDBName+';'+
      'sItemId='+sItemId+';'+
      'sDLLName='+''+';'+
      //'sClassName='+''+';'+
      'sClassName='+'^'+sTempBasJSISpw+';'+ //2012.09.21 modify
      'sUserId='+sUserId+';'+
      'sBUID='+sBUID+';'+
      'sGlobalId='+sGlobalId+';'+
      'sUseId='+sUseId+';'+
      'sPaperId='+sRealTableNameMas1+';'+
      'sPaperNum='+sNowPaperNum+';'+
      'sSystemId='+sSystemId+';'+
      'iDtlItem='+inttostr(iDtlItem)
      ;

      frmJSdPaperSearchDLL.iReShowDlg_PaperSearch:=iReShowDlg;//2015.04.21 add

      //----------2015.11.12 add for SS Bill-20151111-01
      if frmJSdPaperSearchDLL.Width < iDlgSearchWidth then
         frmJSdPaperSearchDLL.Width := iDlgSearchWidth;

      if frmJSdPaperSearchDLL.Height < iDlgSearchHeight then
         frmJSdPaperSearchDLL.Height := iDlgSearchHeight;

      frmJSdPaperSearchDLL.Position:=poScreenCenter;
      //----------

      frmJSdPaperSearchDLL.btnGetParams.Click;
      frmJSdPaperSearchDLL.Hide;
      frmJSdPaperSearchDLL.ShowModal;

      {//2015.04.21 disable for 改在 frmJSdPaperSearchDLL 裡
      //MUT User想維持的效果是「按下確認後重新顯示出來的畫面會顯示在Excel上面」，
      //此次修改後，重新Compiler出來的DLL雖然按下確認後重新顯示出來的畫面會顯示在螢幕最上層，
      //但反而把Excel推到最後面，為解決此現象，應該在按下確認後，不要讓彈出的多選多畫面消失後重新呼叫，
      //只要清除已選的項目就好了

      //2012.03.14 add for MUT Bill-20120307-04
      if (frmJSdPaperSearchDLL.ModalResult=mrOk) and (iReShowDlg=1) then
        begin
          //呼叫自己
          funCustButtonDo(
            fFrm,
            sBtnName,
            tTable,
            qryExec,
            sItemId,
            sGlobalId,
            sSystemId,
            sServerName,
            sDBName,
            sUserId,
            sBUID,
            sUseId,
            sRealTableNameMas1,
            iCanbUpdate,
            iOPKind,
            sPaperUserId,
            CanbLockUserEdit,
            sLoginSvr,
            sLoginDB,
            sPaperMode,
            tTblDtl,
            sTranGlobalId,
            hMain_btnPrint_Handle
            );
        end;
     }
    end//if (lowercase(sSearchTemplate)='jsdpapersearchdll.dll') then
    else if (lowercase(sSearchTemplate)='jsdcondrunspdll.dll') then
    begin
      Application.CreateForm(TfrmCondRunSpDLL, frmCondRunSpDLL);
      //2020.03.10
      {if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
      begin
          sList:=TstringList.Create;
          sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
          sFontSize:=sList.Strings[0];
          sList.Free;
          FontSize := StrToInt(sFontSize);
          frmCondRunSpDLL.ScaleBy(FontSize,100);
      end; }
      frmCondRunSpDLL.iNeedBarCondRunSp:= iNeedBar; //2011.4.14 add
      frmCondRunSpDLL.iPreChkUseAddon1:= iPreChk_UseAddon1; //2011.4.15 add

      //2012.2.7 add
      frmCondRunSpDLL.iNeedConfirm:= iNeedConfirmBefExec; //2011.4.14 add
      frmCondRunSpDLL.sConfirm:= sConfirmBefExec; //2011.4.15 add

      //2012.09.21 add ,因有使用的僅是極少數特殊的DLL,沒必要因此造成版本不相容
      if frmCondRunSpDLL.sConnectStrAdmin='' then
        if frmCondRunSpDLL.sTempBasJSAdpw='' then
          if TJSdTable(tTable).ReserveList.Values['TempBasJSAdpw']<>'' then
          begin
            frmCondRunSpDLL.sTempBasJSAdpw:=TJSdTable(tTable).ReserveList.Values['TempBasJSAdpw'];
          end;

      frmCondRunSpDLL.pnlInfo.Caption:=
      'sServerName='+sServerName+';'+
      'sDBName='+sDBName+';'+
      'sItemId='+sItemId+';'+
      'sDLLName='+''+';'+
      //'sClassName='+''+';'+
      'sClassName='+'^'+sTempBasJSISpw+';'+ //2012.09.21 modify
      'sUserId='+sUserId+';'+
      'sBUID='+sBUID+';'+
      'sGlobalId='+sGlobalId+';'+
      'sUseId='+sUseId+';'+
      'sPaperId='+sRealTableNameMas1+';'+
      'sPaperNum='+sNowPaperNum+';'+
      'sSystemId='+sSystemId+';'+
      'iDtlItem='+inttostr(iDtlItem)
      ;

      frmCondRunSpDLL.btnGetParams.Click;


      //2011.3.9 add
      if trim(sPrintRptName)<>'' then
         frmCondRunSpDLL.iForReport:=1
         else frmCondRunSpDLL.iForReport:=0;

      frmCondRunSpDLL.Hide;
      frmCondRunSpDLL.ShowModal;


//2011.3.9 add
if trim(sPrintRptName)<>'' then
begin
//###############################################################################
        if frmCondRunSpDLL.ModalResult=mrOk then
          begin
            iB:=frmCondRunSpDLL.iParamCount;

            SetLength(sParamArray,iB);

          for iB := low(frmCondRunSpDLL.sReportParamArray) to  high(frmCondRunSpDLL.sReportParamArray) do
            begin
              sParamArray[iB]:=frmCondRunSpDLL.sReportParamArray[iB];
            end;

         //2022.04.21  TCI 列印標籤RPT
         if sSpName='FMEdPCBLabPrint4' then
          Begin

              with qryExec do
                begin
                  close;
                  sql.Clear;
                  sql.Add('exec FMEdPCBLabPrintRpt '+''''+sUserId+''''+','+''''+sParamArray[0]+'''');
                  open;
                 end;

              if qryExec.RecordCount>0 then
              sPrintRptName:=qryExec.FieldByName('ReportFile').AsString;

              qryExec.Close;

          End;

    try
        rptPaper:= TJSdReport.Create(nil);
        //rptPaper.ReportTitle:= tTable.TableName;
        rptPaper.ReportTitle:=sItemId+' '+sCustCaption+'(Preview)';//2012.04.03 add for MUT Bill-20120329-04
        rptPaper.ReportFileName:=sPrintRptName;

        qryTmp:=TADOQuery.Create(nil);
        qryTmp.ConnectionString:=qryExec.ConnectionString;
        qryTmp.CommandTimeout:=9600;

        RunJSdReportDLL(
              rptPaper,//JsRpt: TJSdReport;
              sSpName,//ProcName: WideString;
              sParamArray,//ParamList: array of WideString;
              '',//sIndex,
              sPrintRptName,//sRptName: WideString;
              //=====
              qryTmp,
              sSystemId,
              sBUID_org,//sBUId,
              sUserId,
              nil, //AftPrn: TNotifyEVent
              false,
              //=====
              hMain_btnPrint_Handle,
              sGlobalId,
              1 //iShowModal
              );
      finally
        rptPaper.Free;
        qryTmp.Close;
        qryTmp.Free;
      end;

      EXIT;
end;
//###############################################################################
    end;
    end
    else if (lowercase(sSearchTemplate)='jsdeditgriddll.dll') then
    begin
      Application.CreateForm(TfrmEditGridDLL,frmEditGridDLL);
      //2020.03.10
      if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
      begin
          sList:=TstringList.Create;
          sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
          sFontSize:=sList.Strings[0];
          sList.Free;
          FontSize := StrToInt(sFontSize);
          frmEditGridDLL.Scaled:=true;
          frmEditGridDLL.ScaleBy(FontSize,100);
      end;

      frmEditGridDLL.pnlInfo.Caption:=
      'sServerName='+sServerName+';'+
      'sDBName='+sDBName+';'+
      'sItemId='+sItemId+';'+
      'sDLLName='+''+';'+
      //'sClassName='+''+';'+
      'sClassName='+'^'+sTempBasJSISpw+';'+ //2012.09.21 modify
      'sUserId='+sUserId+';'+
      'sBUID='+sBUID+';'+
      'sGlobalId='+sGlobalId+';'+
      'sUseId='+sUseId+';'+
      'sPaperId='+sRealTableNameMas1+';'+
      'sPaperNum='+sNowPaperNum+';'+
      'sSystemId='+sSystemId+';'+
      'iDtlItem='+inttostr(iDtlItem)
      ;

      frmEditGridDLL.btnGetParams.Click;
      frmEditGridDLL.Hide;
      frmEditGridDLL.ShowModal;
    end
    else if (lowercase(sSearchTemplate)='minduomgetlot.dll') then
    begin
      Application.CreateForm(TfrmMINdUOMGetLotDLL,frmMINdUOMGetLotDLL);
      //2020.03.10
      if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
      begin
          sList:=TstringList.Create;
          sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
          sFontSize:=sList.Strings[0];
          sList.Free;
          FontSize := StrToInt(sFontSize);
          frmMINdUOMGetLotDLL.Scaled:=true;
          frmMINdUOMGetLotDLL.ScaleBy(FontSize,100);
      end;

      frmMINdUOMGetLotDLL.pnlInfo.Caption:=
      'sServerName='+sServerName+';'+
      'sDBName='+sDBName+';'+
      'sItemId='+sItemId+';'+
      'sDLLName='+''+';'+
      //'sClassName='+''+';'+
      'sClassName='+'^'+sTempBasJSISpw+';'+ //2012.09.21 modify
      'sUserId='+sUserId+';'+
      'sBUID='+sBUID+';'+
      'sGlobalId='+sGlobalId+'^'+sTranGlobalId+';'+
      'sUseId='+sUseId+';'+
      'sPaperId='+sRealTableNameMas1+';'+
      'sPaperNum='+sNowPaperNum+';'+
      'sSystemId='+sSystemId+';'+
      'iDtlItem='+inttostr(iDtlItem)
      ;
      //2019.11.11
      frmMINdUOMGetLotDLL.bColSave:=bF9;

      frmMINdUOMGetLotDLL.btnGetParams.Click;
      frmMINdUOMGetLotDLL.Hide;
      frmMINdUOMGetLotDLL.ShowModal;
    end;

    //做 Refresh
    if tTable.RecordCount>0 then bk:=tTable.GetBookmark;
    tTable.Close;
    tTable.Open;

    if tTable.RecordCount>0 then if assigned(bk) then tTable.GotoBookmark(bk);
    if assigned(bk) then tTable.FreeBookmark(bk);

    if Assigned(tTblDtl) then
      if tTblDtl.Active then
        if tTblDtl.RecordCount>0 then if assigned(bkDtl) then tTblDtl.GotoBookmark(bkDtl);

    if assigned(bkDtl) then tTblDtl.FreeBookmark(bkDtl);
  end //if iDesignType=1 then
  else if iDesignType in[0,2] then
  begin
   //2012.03.20 for EMO
   if sBeCallDLLName<>'AJNdHsaInsJour.dll' then begin
   try
   hHandle:=0;
   hHandle:=funCallDLL(
    qryExec,//qryExec:TADOQuery;
    fFrm,//fStartForm:TForm;
    1,//iCallType:integer;//0 MainForm, 1 DLL ,2 Flow
    true,//bShowModal:boolean;
    sBeCallDLLName,//sItemId,
    sDialogCaption,//sItemName,
    sBeCallDLLName,//sClassName,
    sItemId+'~'+sBtnName+'^'+tTable.ReserveList.Values['LanguageId'],//sSystemId,//
    sServerName,//sServerName,
    sDBName,//sDBName,
    sUserId,//sUserId,
    sBUID,//sBUId,
    sUseId,//sUseId,
    sRealTableNameMas1,//sPaperId,
    sNowPaperNum,//sPaperNum,
    sGlobalId,//sGlobalId  :string
    nil,//tOtherParent:TWinControl;
    '',//sServerPath:string;
    '',//sLocalPath:string;
    sLoginSvr, //new 2009.7.22
    sLoginDB,  //new 2009.7.22
    sSearchTemplate, //sOCXTemplate:string
    iDtlItem, //new 2009.8.26
    sTranGlobalId, //2009.9.1 add
    sTempBasJSISpw //2012.06.01 add for SS Bill-20120531-01
    );
  finally
    if hHandle<>0 then
    begin
      {WaitForSingleObject(hHandle, 90000); //2009.7.30 改2000, 因原來的 10 太短

      if hHandle>32 then
        begin}

          FreeLibrary(hHandle);

          if tTable.RecordCount>0 then bk:=tTable.GetBookmark;
          tTable.Close;
          tTable.Open;
          //tTable.Locate('PaperNum',sNowPaperNum,[loCaseInsensitive]);
          if tTable.RecordCount>0 then if assigned(bk) then tTable.GotoBookmark(bk);
          if assigned(bk) then tTable.FreeBookmark(bk);

          //FreeLibrary(hHandle); //new 2009.7.22 //2009.9.7 disable 一定要保留
          if Assigned(tTblDtl) then
           if tTblDtl.Active then
             if tTblDtl.RecordCount>0 then if assigned(bkDtl) then tTblDtl.GotoBookmark(bkDtl);

          if assigned(bkDtl) then tTblDtl.FreeBookmark(bkDtl);

        //end;//if hHandle>32 then
    end;//if hHandle<>0 then
  end;//try
  //2012.03.20 for EMO
  end;
  end;//else if iDesignType in[0,2] then
end;//if iDesignType in[0,1,2] then


if iInsPaperMsg=1 then //2009.12.28 add
  begin
    if iOpKind<>1 then
      begin
        MsgDlgJS('只有單據類型的作業才可「傳送訊息」',mtWarning,[mbOk],0);
        exit;
      end;

    sToPaperId:=trim(sToPaperId);

    if sToPaperId='' then
      begin
        MsgDlgJS('未設定「拋轉單據代號」，無法「傳送訊息」',mtWarning,[mbOk],0);
        exit;
      end;

    OpenSQLDLL(
      qryExec,
      'OPEN',
      //'select UserId from CURdMsgPaperUser(nolock)'+
      //   ' where PaperId='+''''+sToPaperId+''''+' and Kind=0');
      'exec CURdMsgPaperUserByParams '+''''+sToPaperId+''''); //2012.07.18 modify for CMT Bill-20120716-02

   //2012.07.18 note:此變通的做法是 若後端參數值是多受信者也不要市彈出視窗時，
   //該sp只output一筆，讓 CURdMsgPaperInsert 到後端再依參數值去決定是否該塞多筆訊息

   i:=qryExec.RecordCount;
   sNowPaperNum:=tTable.FieldByName('PaperNum').Asstring;

    if i=0 then
      begin
        MsgDlgJS('未設定單據別'+sToPaperId+'的受信者，無法「傳送訊息」',mtWarning,[mbOk],0);
        exit;
      end
    else if i=1 then
      begin
        sMsgToUserId:=qryExec.Fields[0].AsString;

        OpenSQLDLL(
          qryExec,
          'EXEC',
          'CURdMsgPaperInsert '+
            ''''+sMsgToUserId+''''+','+ //@ToUserId
            ''''+sToPaperId+''''+','+ //@ToPaperId
            ''''+''''+','+ //@ToPaperNum
            ''''+sUserId+''''+','+ //@FromUserId
            ''''+sRealTableNameMas1+''''+','+//@FromPaperId
            ''''+sNowPaperNum+''''+','+ //@FromPaperNum
            '-1'+','+//@Spid
            ''''+sUseId+''''
            );
      end
      else
      begin
        //Call DLL
        OpenSQLDLL(
          qryExec,
          'OPEN',
          'exec CURdMsgPaperUserGet '+
            ''''+sToPaperId+''''+','+ //@ToPaperId
            ''''+sUserId+''''+','+ //@FromUserId
            ''''+sRealTableNameMas1+''''+','+  //@FromPaperId
            ''''+sNowPaperNum+''''+','+//@FromPaperNum
            ''''+sUseId+''''
            );

        i:=qryExec.Fields[0].AsInteger;

        Application.CreateForm(TfrmCURdMsgUserSelect,frmCURdMsgUserSelect);
        //2020.03.10
        if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
        begin
            sList:=TstringList.Create;
            sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
            sFontSize:=sList.Strings[0];
            sList.Free;
            FontSize := StrToInt(sFontSize);
            frmCURdMsgUserSelect.Scaled:=true;
            frmCURdMsgUserSelect.ScaleBy(FontSize,100);
        end;
        frmCURdMsgUserSelect.qryExec.ConnectionString:=qryExec.ConnectionString;
        frmCURdMsgUserSelect.qryToUser.ConnectionString:=qryExec.ConnectionString;
        with frmCURdMsgUserSelect.qryToUser do
          begin
            sql.Clear;
            sql.Add('select * from CURdMsgUserTemp where Spid='+inttostr(i));
            open;
          end;
        frmCURdMsgUserSelect.ShowModal;

        {funCallDLL(
          qryExec,//qryExec:TADOQuery;
          fFrm,//fStartForm:TForm;
          1,//iCallType:integer;//0 MainForm, 1 DLL ,2 Flow
          true,//bShowModal:boolean;
          'CURdMsgUserSelect.dll',//sItemId,
          '選擇受信者',//sItemName,
          'CURdMsgUserSelect.dll',//sClassName,
          sItemId+'^'+sBtnName,//sSystemId,//
          sServerName,//sServerName,
          sDBName,//sDBName,
          sUserId,//sUserId,
          sBUID,//sBUId,
          sUseId,//sUseId,
          sRealTableNameMas1,//sPaperId,
          'select * from CURdMsgUserTemp where Spid='+inttostr(i),//sPaperNum,
          sGlobalId,//sGlobalId  :string
          nil,//tOtherParent:TWinControl;
          '',//sServerPath:string;
          '',//sLocalPath:string;
          sLoginSvr, //new 2009.7.22
          sLoginDB,  //new 2009.7.22
          '', //sOCXTemplate:string
          0, //new 2009.8.26
          '' //2009.9.1 add
          );}
      end;
  end;

  //----------2012.11.16 add for SS Bill-20121102-07
  if VarIsNull(hMain_btnPrint_Handle) then hMain_btnPrint_Handle:=0;

  if (iCallPaperAftTran=1) and (hMain_btnPrint_Handle<>0) then
    begin
      OpenSQLDLL(
        qryExec,
        'OPEN',
        'exec CURdCallPaperAftTran '+
            ''''+sGlobalId+''''+','+
            ''''+sToPaperId+''''+','+
            //inttostr(iCallPaperPowerType)+','+ //disbale,因規則要全系統一致
            ''''+sRealTableNameMas1+''''+','+
            ''''+sNowPaperNum+''''+','+
            ''''+sUserId+''''+','+
            ''''+sUseId+''''
        );

      if qryExec.RecordCount>0 then
        begin
          if qryExec.Fields[0].AsString='OK' then
            begin
              if MsgDlgJS('是否要檢視拋轉的單據？',mtConfirmation,[mbYes,mbNo],0)=mrYes then
                begin
                  SendMessage(hMain_btnPrint_Handle,WM_LBUTTONDOWN, 0, 0);
                  SendMessage(hMain_btnPrint_Handle,WM_LBUTTONUP, 0, 0);
                end;
            end;//if qryExec.Fields[0].AsString='OK' then
        end;//if qryExec.RecordCount>0 then

    end;//if iCallPaperAftTran=1 then
  //----------

  result:=true;
end;

function funTranDataInsert(
  fFrm:TForm;
  sGlobalId,
  sGlobalId_tmp,
  sItemId,
  sBtnName,
  sBeCallDLLName,
  sUserId,
  sUseId,
  sSystemId:string;
  qryExec:TADOQuery;
  tTable:TJSdTable
  ):boolean;
var sInsertTranSQLHead,sInsertTranSQLAll,sTblName:string;  i:integer;
    qryTemp:TADOQuery;
begin
      sInsertTranSQLHead:='exec CURdOCXTranDataTempInsert '+
        ''''+sGlobalId+''''+','+ //@MainGlobalId
        ''''+sGlobalId_tmp+''''+','+ //@OnceGlobalId
        ''''+sItemId+''''+','+ //@fromItemId
        ''''+sBtnName+''''+','+ //@ButtonName
        ''''+sBeCallDLLName+''''+','+//@toItemId
        ''''+sUserId+'''' //@UserId
        ;

    qryTemp:=TADOQuery.Create(nil);
    TRY
      qryTemp.ConnectionString:=qryExec.ConnectionString;

      with qryExec do
        begin
          first;
          i:=1;

          while not eof do
            begin
              sInsertTranSQLAll:='';
              sInsertTranSQLAll:=sInsertTranSQLHead;

              //@iFlag
              if i=1 then sInsertTranSQLAll:=sInsertTranSQLHead+',0'
              else if i=qryExec.RecordCount then sInsertTranSQLAll:=sInsertTranSQLHead+',1'
              else sInsertTranSQLAll:=sInsertTranSQLHead+',-1';

              //@iCursorSeq
              sInsertTranSQLAll:=sInsertTranSQLAll+','+inttostr(i);

              //@SeqNum
              sInsertTranSQLAll:=sInsertTranSQLAll+','+fieldbyname('SeqNum').AsString;

              //@Field1
              case fieldbyname('ParamType').asinteger of
                  //欄位值
                0:begin
                  sTblName:='';
                  sTblName:=fieldbyname('ClientTblName').AsString;

                  if fFrm.FindComponent(sTblName)=nil then
                    begin
                      MsgDlgJS('物件「'+sTblName+'」不存在',mtError,[mbOk],0);
                      result:=false;
                      exit;
                    end
                    else
                    begin
                      sInsertTranSQLAll:=sInsertTranSQLAll+','+''''+
                        TJSdTable(fFrm.FindComponent(sTblName)).FieldByName(fieldbyname('ParamFieldName').AsString).AsString
                        +'''';
                    end;
                  end;
                  //常數
                1:sInsertTranSQLAll:=sInsertTranSQLAll+','+''''+fieldbyname('ParamFieldName').AsString+'''';
                  //操作者
                2:sInsertTranSQLAll:=sInsertTranSQLAll+','+''''+sUserId+'''';
                  //公司別
                3:sInsertTranSQLAll:=sInsertTranSQLAll+','+''''+sUseId+'''';
                  //系統別
                4:sInsertTranSQLAll:=sInsertTranSQLAll+','+''''+sSystemId+'''';
                  //目前單號
                5:sInsertTranSQLAll:=sInsertTranSQLAll+','+''''+tTable.FieldByName('PaperNum').Asstring+'''';
              end;

              qryTemp.Close;
              qryTemp.SQL.Clear;
              qryTemp.SQL.Add(sInsertTranSQLAll);
              qryTemp.ExecSQL;
              qryTemp.Close;

              inc(i);
              next;
            end;//while not eof do
          end;//with qryExec do
    FINALLY
      if qryTemp.active then qryTemp.close;
      qryTemp.free;
    END;

  result:=true;
end;

function funGlobalIdGet(sUserId:string):string;
var sRandom:string;
begin
  Randomize;
  sRandom:=inttostr(Random(65535));
  Result:=sUserId
             +copy(FormatDateTime('yyyy',date),3,2)
             +FormatDateTime('mmddhhnnsszzz',now)
             +'R'+sRandom;
end;

//2012.2.16 modify by Johnson,解決Access violation
function DLLGetTempPathStr : WideString;
var
  tempFolder: array[0..MAX_PATH] of Char;
begin
  GetTempPath(MAX_PATH, @tempFolder);
  result := StrPas(tempFolder);
end;
{
function DLLGetTempPathStr : WideString;
var PCr : pWidechar;
begin
  GetMEM(PCr, 1024);
  if GetTempPath(1024, PCr)>0 then
    Result:= StrPas(PCr)
  else
    Result:='';
  FreeMEM(PCr);
end;}

function funCallDLL(
  qryExec:TADOQuery;
  fStartForm:TForm;
  iCallType:integer;//0 from MainForm, 1 from DLL, 2 from Flow , 3 PaperTrace
  bShowModal:boolean;
  sItemId,
  sItemName,
  sClassName,
  sSystemId,
  sServerName,
  sDBName,
  sUserId,
  sBUId,
  sUseId,
  sPaperId,
  sPaperNum,
  sGlobalId  :string;
  tOtherParent:TWinControl;
  sServerPath:string;
  sLocalPath:string;
  sLoginSvr:string;
  sLoginDB:string;
  sOCXTemplate:string;
  iDtlItem:integer; //new 2009.8.26
  sTranGlobalId:string; //2009.9.1 add
  sTempBasJSISpw:string//2012.06.01 modify for SS Bill-20120531-01
  ):THandle;
var i:integer;
    vHandle:THandle;
    ShowChildForm: TShowChildForm;
    ServerDllName, LocalDllName:string;//,sRandom,sGlobalId:string;
    frmCheck:TfrmShowDLLForm;
    frmTemp:TfrmShowDLLForm;
    bNeedCopy:boolean;
    sDLLForceFree:string;
    sPureName:string;
    sToClientClassName:string;
    sMark:string;
    sCompanyUseId:string;
    bMultiDLL:boolean;//2010.5.28 add
    bCopyOtherDLL:boolean;//2010.10.12 add
    sCopyOtherDLLName:string;//2010.10.12 add
    //sTempBasJSAdpw,sTmpStr:string;//2012.09.21 add
    //j,iLen:integer; //2012.09.21 add
    //2020.12.15
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
    iDlgHeight, iDlgWidth: integer;
begin
  if sTempBasJSISpw='' then sTempBasJSISpw:='JSIS'; //2012.06.01 add for SS Bill-20120531-01
  {
  sTempBasJSAdpw:='';

  if sTempBasJSISpw='' then sTempBasJSISpw:='JSIS'
  else
    begin
      j:=0;
      j:=pos('~',sTempBasJSISpw);

      if j>0 then
        begin
          sTmpStr:=sTempBasJSISpw;

          iLen:=length(sTmpStr);

          sTempBasJSISpw:=copy(sTmpStr,1,j-1);

          sTempBasJSAdpw:=copy(sTmpStr,j+1,iLen);
        end;
    end;
  }
  if iCallType=0 then
  begin
    for i := 0 to fStartForm.MDIChildCount - 1 do
    if fStartForm.MDIChildren[i] is TfrmShowDLLForm then
      begin
        frmCheck := fStartForm.MDIChildren[i] as TfrmShowDLLForm;

        if frmCheck.sCheckName = sItemId then
        begin
          if Screen.ActiveForm <> TForm(frmCheck) then
            begin
              TForm(frmCheck).Show;
            end;
          Result:=0;
          Exit;
        end;
     end;

  end;//if iCallType=0 then

  if sClassName='' then
    begin
      MsgDlgJS('未設定類別', mtError, [mbOk], 0);
      result:=0;
      exit;
    end;

{  sDLLForceFree:=unit_DLL.funDLLSysParamsGet(qryExec,'CUR','DLLForceFree');

  if sDLLForceFree='1' then
    funFreeDLL_Main(sLoginSvr,sLoginDB,sGlobalId,false); //new 2009.7.22
}
//==========2009.11.24 add for
    //解決使用Template的單據一單多用且未開啟「強制釋放」時，
    //同時使用JSdPaperOrg及JSdPaper3LDLL者因DLL Name都相同造成DLL互相覆蓋的問題

  sToClientClassName:=sClassName;
  sPureName:=ChangeFileExt(sClassName,'');

  with qryExec do
        begin
          if active then close;
          sql.Clear;
          sql.Add('select iPowerType=isnull(PowerType,0),iCopyPowerType=isnull(CopyFromPowerType,0) from CURdSysItems(nolock)'+
            ' where ItemId='+''''+sItemId+'''');
          open;
        end;

  bMultiDLL:=false;//2010.5.28 add
  bCopyOtherDLL:=false;//2010.10.12 add

  if qryExec.RecordCount>0 then
        if qryExec.FieldByName('iPowerType').AsInteger>0 then
           begin
             //2010.10.12 add
             if qryExec.FieldByName('iCopyPowerType').AsInteger>0 then
              begin
                bCopyOtherDLL:=true;
                sCopyOtherDLLName:=sPureName+qryExec.FieldByName('iCopyPowerType').AsString+'.dll';
              end;

             sToClientClassName:=sPureName+qryExec.FieldByName('iPowerType').AsString+'.dll';

             bMultiDLL:=true; //2010.5.28 add
           end;
//==========

  if sLocalPath='' then sLocalPath:= trim(DLLGetTempPathStr)+'JSIS\';//2010.4.6 add

  LocalDllName:= sLocalPath + trim(sToClientClassName); //2010.4.6 modify

  if sOCXTemplate<>'' then
    begin
      if SyncFileStrDLL2(sLocalPath+sOCXTemplate,LocalDllName)=false then
       begin
         result:=0;
         exit;
       end;
  end;//if sOCXTemplate<>'' then

if sOCXTemplate='' then
begin
    if sServerPath='' then //2010.4.6 add
      begin
        with qryExec do
          begin
            close;
            sql.Clear;
            //sql.Add('select SocketServer from CURdBU(nolock) where BUID='+''''+sBUID+'''');
            sql.Add('exec CURdServerInfoGet '+''''+sBUID+''''+','+''''+sGlobalId+'''');//2012.05.22 modify for WF Bill-20120518-05
            open;
          end;

      //ServerDllName:=qryExec.Fields[0].asstring+trim(sClassName);
      //sServerPath:=qryExec.Fields[0].asstring;//2010.5.3 modify for debug

      sServerPath:=qryExec.FieldByName('SocketServer').AsString;//2012.05.22 modify for WF Bill-20120518-05

      qryExec.Close;
    end;

    ServerDllName:= sServerPath + trim(sClassName);  //2010.4.6 modify

    if bMultiDLL then //2010.5.28 add
    begin
      if FileExists(sServerPath +sToClientClassName) then
        begin
          ServerDllName:='';
          ServerDllName:= sServerPath +sToClientClassName;
        end;
    end;

    //2010.10.12 add
    if bCopyOtherDLL then
    begin
      if FileExists(sServerPath +sCopyOtherDLLName) then
        begin
          ServerDllName:='';
          ServerDllName:= sServerPath +sCopyOtherDLLName;
        end;
    end;

   if SyncFileStrDLL2(ServerDllName,LocalDllName)=false then

       begin
         result:=0;
         exit;
       end;

end;//if sOCXTemplate='' then

if not fileexists(LocalDllName) then
    begin
      MsgDlgJS('檔案不存在[ '+LocalDllName+' ]',mtWarning,[mbOK],0);
      result:=0;
      exit;
    end;

  vHandle :=0;
  vHandle := LoadLibrary(pchar(LocalDllName));

  if vHandle = 0 then
    begin
      MsgDlgJS('無法開啟[ '+LocalDllName+' ]',mtWarning,[mbOK],0);
      result:=0;
      exit;
    end;

{if sDLLForceFree='1' then
begin
  //=====強制釋放,因DLL若在記憶體中 Windows會將其Handle傳回,不會重新Load,
  //若該DLL已Error, 就無法被Free及重Load ,故以此方式確保DLL是重新被Load

  FreeLibrary(vHandle); //釋放掉

  vHandle :=0;
  vHandle := LoadLibrary(pchar(LocalDllName)); //重新Load一次

  if vHandle = 0 then
    begin
      ShowMessage('無法開啟[ '+LocalDllName+' ]');
      result:=0;
      exit;
    end;
  //=====
end;}

  @ShowChildForm := GetProcAddress(vHandle,pchar('ShowForm'));

  if @ShowChildForm=nil then
    begin
      FreeLibrary(vHandle);
      MsgDlgJS('無法呼叫函式['+LocalDllName+'.ShowForm]',mtWarning,[mbOK],0);
      result:=0;
      exit;
    end;

{if sDLLForceFree='1' then
begin
  if iCallType<>2 then //iCallType=2 的由承載Form釋放 2010.4.7 add
  begin
  //new 2009.7.22, 要在CALL之前寫
    if LowerCase(sOCXTemplate)='jsdreportgriddll.dll' then //JSdReportGridDLL.dll
      sMark:='1' else sMark:='0';

    with qryExec do
        begin
          if active then close;
          sql.Clear;
          sql.Add('insert into CURdOCXHandleLog(MainGlobalId,ItemId,iHandle,iMark)'+
            ' select '+''''+sGlobalId+''''+','+
            ''''+sItemId+''''+','+
            vartostr(vHandle)+','+sMark
            );
          ExecSQL;
          close;
        end;
  end;//if iCallType<>2 then
end;}
    //====================================
    unit_DLL2.prcGetSvrName_CpnyUseId(
      qryExec,
      sServerName,
      sDBName,
      sLoginSvr,
      sLoginDB,
      sUseId
      );
    //====================================

  case iCallType of
    0: begin
      frmTemp:=TfrmShowDLLForm.Create(fStartForm);

      frmTemp.sCheckName:=sItemId;

      //frmTemp.Caption:=sItemName;
      frmTemp.Caption:=sItemId+' '+sItemName;//2010.10.8 modify for YX Bill-20101008-2

      //frmTemp.FormStyle:=fsMDIChild;

        //2009.12.2 disable 因用 遠端桌面 以最小權限執行若不開著 檔案總管 會Access Violation
        //故改設在 TfrmShowDLLForm 的Properties

      frmTemp.sMainGlobalId:=sGlobalId;
      frmTemp.sLoginSvr:=sLoginSvr;
      frmTemp.sLoginDB:=sLoginDB;
      frmTemp.iHandle:=vHandle;
      frmTemp.sOCXTemplate:=sOCXTemplate;
      frmTemp.sClassName:=sClassName;

      frmTemp.Show;

      //2020.12.15
      FontSize:=100;
      if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
      begin
          sList:=TstringList.Create;
          sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
          sFontSize:=sList.Strings[0];
          sList.Free;
          FontSize := StrToInt(sFontSize);
      end;

      if (sOCXTemplate='JSdRptInputDLL.dll')
        or (sClassName='APSdParams.dll') //2010.12.27 add
      then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=508;
          iDlgWidth:=508;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100 * 0.8);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          //frmTemp.Width:=424;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end;

      {if sClassName='FMEdPassPCB.dll' then
        begin
          frmTemp.WindowState:=wsNormal;
          frmTemp.Height:=522;
          frmTemp.Width:=638;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end
      else} if sClassName='FMEdPassMultiPCB.dll' then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=539;
          iDlgWidth:=968;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;//2015.02.09 modify //802;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end
      else if sClassName='FMEdDivLotPCB.dll' then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=339;
          iDlgWidth:=486;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end
      //else if sClassName='FMEdCostLock.dll' then
      else if pos('CostLock.dll',sClassName)>0 then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=260;
          iDlgWidth:=350;
          //2020.12.24
          if pos('MINdCostLock.dll',sClassName)>0 then
            iDlgWidth:=450;

          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end
      else if sClassName='AJNdLockMonthSet.dll' then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=260;
          iDlgWidth:=350;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end;

     if sTranGlobalId<>'' then sGlobalId:=sGlobalId+'^'+sTranGlobalId; //2009.9.1 add then

      ShowChildForm(
        frmTemp.pnlShowDLL,//rParent:TWinControls;
        sItemName,//sTitle:string;
        sClassName,//sCaption:string;
        bShowModal,//bShowModal:boolean;
        //====
        sServerName,//sServerName:string;
        sDBName,//sDBName:string;
        sItemId,//sItemId:string;
        sClassName,//sDLLName:string;
        //sClassName,//sClassName:string;
        sClassName+'^'+sTempBasJSISpw,//2012.06.01 modify for SS Bill-20120531-01
        trim(sUserId),//sUserId:string;
        trim(sBUId),//sBUID:string;
        sGlobalId,//sGlobalId:string;
        trim(sUseId),//sUseId:string
        sPaperId, //sPaperId:string
        sPaperNum, //sPaperNum:string
        vHandle,
        sSystemId,
        iCallType,
        iDtlItem //new 2009.8.26
        );

    end;
    1:
    begin
      //在被呼叫端處理

     if sTranGlobalId<>'' then sGlobalId:=sGlobalId+'^'+sTranGlobalId; //2009.9.1 add then

      ShowChildForm(
        nil,//rParent:TWinControls;
        sItemName,//sTitle:string;
        sClassName,//sCaption:string;
        bShowModal,//bShowModal:boolean;
        //====
        sServerName,//sServerName:string;
        sDBName,//sDBName:string;
        sItemId,//sItemId:string;
        sClassName,//sDLLName:string;
        //sClassName,//sClassName:string;
        sClassName+'^'+sTempBasJSISpw,//2012.06.01 modify for SS Bill-20120531-01
        trim(sUserId),//sUserId:string;
        trim(sBUId),//sBUID:string;
        sGlobalId,//sGlobalId:string;
        trim(sUseId),//sUseId:string
        sPaperId, //sPaperId:string
        sPaperNum, //sPaperNum:string
        vHandle,
        sSystemId,
        iCallType,
        iDtlItem //new 2009.8.26
        );
    end;
    2:
    begin
     if sTranGlobalId<>'' then sGlobalId:=sGlobalId+'^'+sTranGlobalId; //2009.9.1 add then

      TForm(tOtherParent.Owner).Tag:=vHandle;

      ShowChildForm(
        tOtherParent,//rParent:TWinControls;
        sItemName,//sTitle:string;
        sClassName,//sCaption:string;
        bShowModal,//bShowModal:boolean;
        //====
        sServerName,//sServerName:string;
        sDBName,//sDBName:string;
        sItemId,//sItemId:string;
        sClassName,//sDLLName:string;
        //sClassName,//sClassName:string;
        sClassName+'^'+sTempBasJSISpw,//2012.06.01 modify for SS Bill-20120531-01
        trim(sUserId),//sUserId:string;
        trim(sBUId),//sBUID:string;
        sGlobalId,//sGlobalId:string;
        trim(sUseId),//sUseId:string
        sPaperId, //sPaperId:string
        sPaperNum, //sPaperNum:string
        vHandle,
        sSystemId,
        iCallType,
        iDtlItem //new 2009.8.26
        );
    end;//2
    {3:
    begin
      //在被呼叫端處理
      ShowChildForm(
        nil,//rParent:TWinControls;
        sItemName,//sTitle:string;
        sClassName,//sCaption:string;
        bShowModal,//bShowModal:boolean;
        //====
        sServerName,//sServerName:string;
        sDBName,//sDBName:string;
        sItemId,//sItemId:string;
        sClassName,//sDLLName:string;
        sClassName,//sClassName:string;
        trim(sUserId),//sUserId:string;
        trim(sBUId),//sBUID:string;
        sGlobalId,//sGlobalId:string;
        trim(sUseId),//sUseId:string
        sPaperId, //sPaperId:string
        sPaperNum, //sPaperNum:string
        vHandle,
        sSystemId,
        iCallType
        );
    end; }
  end;//case iCallType of

  result:=vHandle;
end;

function funCallDLL2(
  qryExec:TADOQuery;
  fStartForm:TForm;
  iCallType:integer;//0 from MainForm, 1 from DLL, 2 from Flow , 3 PaperTrace
  bShowModal:boolean;
  sItemId,
  sItemName,
  sClassName,
  sSystemId,
  sServerName,
  sDBName,
  sUserId,
  sBUId,
  sUseId,
  sPaperId,
  sPaperNum,
  sGlobalId  :string;
  tOtherParent:TWinControl;
  sServerPath:string;
  sLocalPath:string;
  sLoginSvr:string;
  sLoginDB:string;
  sOCXTemplate:string;
  iDtlItem:integer; //new 2009.8.26
  sTranGlobalId:string; //2009.9.1 add
  sTempBasJSISpw:string//2012.06.01 modify for SS Bill-20120531-01
  ):THandle;
var i:integer;
    vHandle:THandle;
    ShowChildForm: TShowChildForm;
    ServerDllName, LocalDllName:string;//,sRandom,sGlobalId:string;
    frmCheck:TfrmShowDLLForm;
    frmTemp:TfrmShowDLLForm;
    bNeedCopy:boolean;
    sDLLForceFree:string;
    sPureName:string;
    sToClientClassName:string;
    sMark:string;
    sCompanyUseId:string;
    bMultiDLL:boolean;//2010.5.28 add
    bCopyOtherDLL:boolean;//2010.10.12 add
    sCopyOtherDLLName:string;//2010.10.12 add
    //sTempBasJSAdpw,sTmpStr:string;//2012.09.21 add
    //j,iLen:integer; //2012.09.21 add
    //2020.12.15
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
    iDlgHeight, iDlgWidth: integer;
begin
  if sTempBasJSISpw='' then sTempBasJSISpw:='JSIS'; //2012.06.01 add for SS Bill-20120531-01

  if sClassName='' then
    begin
      MsgDlgJS('未設定類別', mtError, [mbOk], 0);
      result:=0;
      exit;
    end;

  sToClientClassName:=sClassName;
  sPureName:=ChangeFileExt(sClassName,'');

  with qryExec do
        begin
          if active then close;
          sql.Clear;
          sql.Add('select iPowerType=isnull(PowerType,0),iCopyPowerType=isnull(CopyFromPowerType,0) from CURdSysItems(nolock)'+
            ' where ItemId='+''''+sItemId+'''');
          open;
        end;

  bMultiDLL:=false;//2010.5.28 add
  bCopyOtherDLL:=false;//2010.10.12 add

  if qryExec.RecordCount>0 then
        if qryExec.FieldByName('iPowerType').AsInteger>0 then
           begin
             //2010.10.12 add
             if qryExec.FieldByName('iCopyPowerType').AsInteger>0 then
              begin
                bCopyOtherDLL:=true;
                sCopyOtherDLLName:=sPureName+qryExec.FieldByName('iCopyPowerType').AsString+'.dll';
              end;

             sToClientClassName:=sPureName+qryExec.FieldByName('iPowerType').AsString+'.dll';

             bMultiDLL:=true; //2010.5.28 add
           end;
//==========

  if sLocalPath='' then sLocalPath:= trim(DLLGetTempPathStr)+'JSIS\';//2010.4.6 add

  LocalDllName:= sLocalPath + trim(sToClientClassName); //2010.4.6 modify

  if sOCXTemplate<>'' then
    begin
      if SyncFileStrDLL2(sLocalPath+sOCXTemplate,LocalDllName)=false then
       begin
         result:=0;
         exit;
       end;
  end;//if sOCXTemplate<>'' then

if sOCXTemplate='' then
begin
    if sServerPath='' then //2010.4.6 add
      begin
        with qryExec do
          begin
            close;
            sql.Clear;
            //sql.Add('select SocketServer from CURdBU(nolock) where BUID='+''''+sBUID+'''');
            sql.Add('exec CURdServerInfoGet '+''''+sBUID+''''+','+''''+sGlobalId+'''');//2012.05.22 modify for WF Bill-20120518-05
            open;
          end;

      sServerPath:=qryExec.FieldByName('SocketServer').AsString;//2012.05.22 modify for WF Bill-20120518-05

      qryExec.Close;
    end;

    ServerDllName:= sServerPath + trim(sClassName);  //2010.4.6 modify

    if bMultiDLL then //2010.5.28 add
    begin
      if FileExists(sServerPath +sToClientClassName) then
        begin
          ServerDllName:='';
          ServerDllName:= sServerPath +sToClientClassName;
        end;
    end;

    //2010.10.12 add
    if bCopyOtherDLL then
    begin
      if FileExists(sServerPath +sCopyOtherDLLName) then
        begin
          ServerDllName:='';
          ServerDllName:= sServerPath +sCopyOtherDLLName;
        end;
    end;

   if SyncFileStrDLL2(ServerDllName,LocalDllName)=false then

       begin
         result:=0;
         exit;
       end;

end;//if sOCXTemplate='' then

if not fileexists(LocalDllName) then
    begin
      MsgDlgJS('檔案不存在[ '+LocalDllName+' ]',mtWarning,[mbOK],0);
      result:=0;
      exit;
    end;

  vHandle :=0;
  vHandle := LoadLibrary(pchar(LocalDllName));

  if vHandle = 0 then
    begin
      MsgDlgJS('無法開啟[ '+LocalDllName+' ]',mtWarning,[mbOK],0);
      result:=0;
      exit;
    end;


  @ShowChildForm := GetProcAddress(vHandle,pchar('ShowForm'));

  if @ShowChildForm=nil then
    begin
      FreeLibrary(vHandle);
      MsgDlgJS('無法呼叫函式['+LocalDllName+'.ShowForm]',mtWarning,[mbOK],0);
      result:=0;
      exit;
    end;

    //====================================
    unit_DLL2.prcGetSvrName_CpnyUseId(
      qryExec,
      sServerName,
      sDBName,
      sLoginSvr,
      sLoginDB,
      sUseId
      );
    //====================================

  case iCallType of
    0: begin
      frmTemp:=TfrmShowDLLForm.Create(fStartForm);

      frmTemp.sCheckName:=sItemId;

      frmTemp.Caption:=sItemId+' '+sItemName;//2010.10.8 modify for YX Bill-20101008-2

      frmTemp.sMainGlobalId:=sGlobalId;
      frmTemp.sLoginSvr:=sLoginSvr;
      frmTemp.sLoginDB:=sLoginDB;
      frmTemp.iHandle:=vHandle;
      frmTemp.sOCXTemplate:=sOCXTemplate;
      frmTemp.sClassName:=sClassName;

      frmTemp.Show;

      //2020.12.15
      FontSize:=100;
      if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
      begin
          sList:=TstringList.Create;
          sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
          sFontSize:=sList.Strings[0];
          sList.Free;
          FontSize := StrToInt(sFontSize);
      end;

      if (sOCXTemplate='JSdRptInputDLL.dll')
        or (sClassName='APSdParams.dll') //2010.12.27 add
      then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=508;
          iDlgWidth:=424;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100 * 0.8);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end;

    if sClassName='FMEdPassMultiPCB.dll' then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=539;
          iDlgWidth:=802;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end
      else if sClassName='FMEdDivLotPCB.dll' then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=339;
          iDlgWidth:=486;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end

      else if pos('CostLock.dll',sClassName)>0 then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=260;
          iDlgWidth:=350;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end
      else if sClassName='AJNdLockMonthSet.dll' then
        begin
          frmTemp.WindowState:=wsNormal;
          //2020.12.15
          iDlgHeight:=260;
          iDlgWidth:=350;
          if FontSize<>100 then
          begin
            iDlgHeight:= Round(iDlgHeight * FontSize / 100);
            iDlgWidth:= Round(iDlgWidth * FontSize / 100);
          end;
          frmTemp.Height:=iDlgHeight;
          frmTemp.Width:=iDlgWidth;
          frmTemp.Position:=poMainFormCenter;
          frmTemp.BorderIcons:=[biSystemMenu];
        end;

     if sTranGlobalId<>'' then sGlobalId:=sGlobalId+'^'+sTranGlobalId; //2009.9.1 add then

      ShowChildForm(
        frmTemp.pnlShowDLL,//rParent:TWinControls;
        sItemName,//sTitle:string;
        sClassName,//sCaption:string;
        bShowModal,//bShowModal:boolean;
        //====
        sServerName,//sServerName:string;
        sDBName,//sDBName:string;
        sItemId,//sItemId:string;
        sClassName,//sDLLName:string;
        //sClassName,//sClassName:string;
        sClassName+'^'+sTempBasJSISpw,//2012.06.01 modify for SS Bill-20120531-01
        trim(sUserId),//sUserId:string;
        trim(sBUId),//sBUID:string;
        sGlobalId,//sGlobalId:string;
        trim(sUseId),//sUseId:string
        sPaperId, //sPaperId:string
        sPaperNum, //sPaperNum:string
        vHandle,
        sSystemId,
        iCallType,
        iDtlItem //new 2009.8.26
        );

    end;
    1:
    begin
      //在被呼叫端處理

     if sTranGlobalId<>'' then sGlobalId:=sGlobalId+'^'+sTranGlobalId; //2009.9.1 add then

      ShowChildForm(
        nil,//rParent:TWinControls;
        sItemName,//sTitle:string;
        sClassName,//sCaption:string;
        bShowModal,//bShowModal:boolean;
        //====
        sServerName,//sServerName:string;
        sDBName,//sDBName:string;
        sItemId,//sItemId:string;
        sClassName,//sDLLName:string;
        //sClassName,//sClassName:string;
        sClassName+'^'+sTempBasJSISpw,//2012.06.01 modify for SS Bill-20120531-01
        trim(sUserId),//sUserId:string;
        trim(sBUId),//sBUID:string;
        sGlobalId,//sGlobalId:string;
        trim(sUseId),//sUseId:string
        sPaperId, //sPaperId:string
        sPaperNum, //sPaperNum:string
        vHandle,
        sSystemId,
        iCallType,
        iDtlItem //new 2009.8.26
        );
    end;
    2:
    begin
     if sTranGlobalId<>'' then sGlobalId:=sGlobalId+'^'+sTranGlobalId; //2009.9.1 add then

      TForm(tOtherParent.Owner).Tag:=vHandle;

      ShowChildForm(
        tOtherParent,//rParent:TWinControls;
        sItemName,//sTitle:string;
        sClassName,//sCaption:string;
        bShowModal,//bShowModal:boolean;
        //====
        sServerName,//sServerName:string;
        sDBName,//sDBName:string;
        sItemId,//sItemId:string;
        sClassName,//sDLLName:string;
        //sClassName,//sClassName:string;
        sClassName+'^'+sTempBasJSISpw,//2012.06.01 modify for SS Bill-20120531-01
        trim(sUserId),//sUserId:string;
        trim(sBUId),//sBUID:string;
        sGlobalId,//sGlobalId:string;
        trim(sUseId),//sUseId:string
        sPaperId, //sPaperId:string
        sPaperNum, //sPaperNum:string
        vHandle,
        sSystemId,
        iCallType,
        iDtlItem //new 2009.8.26
        );
    end;//2

  end;//case iCallType of

  result:=vHandle;
end;



function funShowDLLFormExecSQL(sConnStr,sExecSQL:string):boolean;
var  qry:TADOQuery;
begin
  qry:=TADOQuery.Create(nil);
  try
    qry.ConnectionString:=sConnStr;

    with qry do
      begin
        sql.Add(sExecSQL);

        try
          ExecSQL;
        except on e:exception do
          begin
            MsgDlgJS(e.Message,mtWarning,[mbOK],0);
          end;
        end;

        close;
      end;
  finally
    qry.Free;
  end;

  result:=true;
end;


function funHandleLogUpdate2(sLoginSvr,sLoginDB,sMainGlobalId,sItemId:string):boolean;
//var sSQL:string; qry:TADOQuery;
begin
  {sSQL:='update CURdOCXHandleLog set iStatus=1 where MainGlobalId='+
    ''''+sMainGlobalId+''''+
    ' and ItemId='+''''+sItemId+'''';

  qry:=TADOQuery.Create(nil);
  try
    qry.ConnectionString:=funConnectStrGet(sLoginSvr,sLoginDB);

    if unit_DLL.funDLLSysParamsGet(qry,'CUR','DLLForceFree')='1' then
    with qry do
      begin
        sql.Add(sSQL);

        try
          ExecSQL;
        except on e:exception do
          begin
            ShowMessage(e.Message);
          end;
        end;

        close;
      end;
  finally
    qry.Free;
  end;}


  result:=true;
end;


function funHandleLogUpdate(sLoginSvr,sLoginDB,sMainGlobalId,sItemId:string):boolean;
//var sSQL:string; qry:TADOQuery;
begin
  {sSQL:='update CURdOCXHandleLog set iStatus=1 where MainGlobalId='+
    ''''+sMainGlobalId+''''+
    ' and ItemId='+''''+sItemId+'''';

  qry:=TADOQuery.Create(nil);
  try
    qry.ConnectionString:=funConnectStrGet(sLoginSvr,sLoginDB);

    with qry do
      begin
        sql.Add(sSQL);

        try
          ExecSQL;
        except on e:exception do
          begin
            ShowMessage(e.Message);
          end;
        end;

        close;
      end;
  finally
    qry.Free;
  end;}


  result:=true;
end;

function funFreeDLL_Main(sLoginSvr,sLoginDB,sMainGlobalId:string;bALL:boolean):boolean;
//var sSQL:string;qry:TADOQuery;
begin
//2012.06.01 note:已停用
{
  sSQL:='exec CURdOCXHandleGet4Free '+''''+sMainGlobalId+''''+',';
  if bALL then sSQL:=sSQL+'1'
    else sSQL:=sSQL+'0';

qry:=TADOQuery.Create(nil);
try
  with qry do
    begin
      ConnectionString:=funConnectStrGet(sLoginSvr,sLoginDB);
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount>0 then
  with qry do
    begin
      first;
      while not eof do
        begin
          FreeLibrary(fieldbyname('iHandle').AsInteger);

          next;
        end;
    end;

  qry.close;
 finally
  qry.Free;
end;
}
  result:=true;
end;


function funStartDLL(
  tFrm:TForm;
  rParent:TWinControl;
  bShowModal:boolean;
  //=====
  sServerName,
  sDBName,
  sItemId,
  sDLLName,
  sClassName,
  sUserId,
  sBUID,
  sGlobalId,
  sUseId,
  sPaperId,
  sPaperNum,
  sSystemId:string;
  iCallType:integer;  //0 MainForm, 1 DLL  ,2 Flow, 3 PaperTrace
  iDtlItem:integer //new 2009.8.26
  ):boolean;

begin
{2009.10.12 搬到下面
  if iCallType in[0,2] then
  begin
    tFrm.BorderStyle:=bsNone;
    tFrm.Align:=alClient;
    SetParent(tFrm.Handle, rParent.Handle);
  end;
}

  TPanel(tFrm.FindComponent('pnlInfo')).Caption:=
      'sServerName='+sServerName+';'+
      'sDBName='+sDBName+';'+
      'sItemId='+sItemId+';'+
      'sDLLName='+sDLLName+';'+
      'sClassName='+sClassName+';'+
      'sUserId='+sUserId+';'+
      'sBUID='+sBUID+';'+
      'sGlobalId='+sGlobalId+';'+
      'sUseId='+sUseId+';'+
      'sPaperId='+sPaperId+';'+
      'sPaperNum='+sPaperNum+';'+
      'sSystemId='+sSystemId+';'+
      'iCallType='+inttostr(iCallType)+';'+
      'iDtlItem='+inttostr(iDtlItem) //new 2009.8.26
      ;

 TSpeedButton(tFrm.FindComponent('btnGetParams')).Click;

//2009.10.12 搬下來
  if iCallType in[0,2] then
  begin
    tFrm.BorderStyle:=bsNone;
    tFrm.Align:=alClient;
    SetParent(tFrm.Handle, rParent.Handle);//某些元件在此段會自動連線,故要搬下來才會有ConnectString
  end;

 tFrm.Show;

 result:=true;
end;

function funStartDllNoParent(
  tFrm:TForm;
  bShowModal:boolean;
  //=====
  sServerName,
  sDBName,
  sItemId,
  sDLLName,
  sClassName,
  sUserId,
  sBUID,
  sGlobalId,
  sUseId,
  sPaperId,
  sPaperNum,
  sSystemId:string;
  iDtlItem:integer //new 2009.8.26
  ):boolean;
begin
 TPanel(tFrm.FindComponent('pnlInfo')).Caption:=
      'sServerName='+sServerName+';'+
      'sDBName='+sDBName+';'+
      'sItemId='+sItemId+';'+
      'sDLLName='+sDLLName+';'+
      'sClassName='+sClassName+';'+
      'sUserId='+sUserId+';'+
      'sBUID='+sBUID+';'+
      'sGlobalId='+sGlobalId+';'+
      'sUseId='+sUseId+';'+
      'sPaperId='+sPaperId+';'+
      'sPaperNum='+sPaperNum+';'+
      'sSystemId='+sSystemId+';'+
      'iDtlItem='+inttostr(iDtlItem) //new 2009.8.26
      ;

 TSpeedButton(tFrm.FindComponent('btnGetParams')).Click;

 if bShowModal then
   begin
     tFrm.Visible:=false;
     tFrm.ShowModal;
   end
   else tFrm.Show;

 result:=true;
end;

function funCopyTempeleteToItem(qry:TADOQuery):boolean;
var sFileName,//sOCXFNames,
  sOCXTemplete,sClassOCX,sMISPath:string;
  sList:TStringList;
  i:integer;
begin
  sFileName:='IsOCX.txt';

  if not FileExists(sFileName) then
     begin
       MsgDlgJS(sFileName+' not found',mtWarning,[mbOK],0);
       result:=false;
       exit;
     end;

  sList:=TStringList.Create;
  sList.LoadFromFile(sFileName);
  sMISPath:=sList.Strings[1];

  sFileName:='';
  sList.Clear;

  sFileName:='DLLSyncList.txt';
  sList.LoadFromFile(sFileName);

  if not FileExists(sFileName) then
     begin
       MsgDlgJS(sFileName+' not found',mtWarning,[mbOK],0);
       sList.Free;
       result:=false;
       exit;
     end;

  if sList.Count=0 then
     begin
       MsgDlgJS('no DLL Templete',mtWarning,[mbOK],0);
       sList.Free;
       result:=false;
       exit;
     end;

  for i := 0 to sList.Count - 1 do
    begin
       with qry do
        begin
          close;
          sql.Clear;
          sql.Add('exec CURdOCXGetInfo '+''''+sList.Strings[i]+'''');
          open;
        end;

       if qry.RecordCount>0 then
         begin
            with qry do
              begin
                sOCXTemplete:=sMISPath+'\'+sList.Strings[i];

                if FileExists(sOCXTemplete)=false then
                  begin
                    MsgDlgJS('DLL檔['+sOCXTemplete+']不存在',mtWarning,[mbOK],0);
                    sList.Free;
                    break;
                    result:=false;
                    exit;
                  end;

                first;

                while not EOF do
                  begin
                    sClassOCX:=sMISPath+'\'+fieldbyname('ClassName').AsString;

                    if FileExists(sClassOCX) then
                      begin
                        if FileAge(sOCXTemplete)> FileAge(sClassOCX) then
                            if CopyFile(
                              Pchar(sOCXTemplete),
                              Pchar(sClassOCX),
                              false)=false
                            then
                            begin
                              MsgDlgJS('複製'+sList.Strings[I]+'...失敗',mtWarning,[mbOK],0);
                              sList.Free;
                              break;
                              result:=false;
                              exit;
                            end;
                       end
                       else
                       begin
                            if CopyFile(
                              Pchar(sOCXTemplete),
                              Pchar(sClassOCX),
                              false)=false
                            then
                            begin
                              MsgDlgJS('複製'+sList.Strings[I]+'...失敗',mtWarning,[mbOK],0);
                              sList.Free;
                              break;
                              result:=false;
                              exit;
                            end;
                        end;

                    Next;
                  end;//while not EOF do
              end;//with qryGetClassName do
         end;//if qryGetClassName.RecordCount>0 then
     end; //for I := 0 to memoOCXTemplete.Lines.Count - 1 do

  sList.Free;

  result:=true;
end;
{//保留
function funCheckX86:string;
var sCheckFile:string;sList:TStringList;sX64:string;
begin
  sX64:='';
  sCheckFile:=ExtractFilePath(Application.ExeName)+'IsOCX.txt';
  if not FileExists(sCheckFile) then
    begin
      MsgDlgJS('設定檔[IsOCX.txt]不存在',mtError,[mbOk],0);
      Application.Terminate;
    end;
  sList:=TStringList.Create;
  sList.LoadFromFile(sCheckFile);
  sX64:=sList.Values['x64'];
  sList.Free;
  result:=sX64;
end;}

//2012.04.12 add,以拆字的方式判斷是 TCP/IP 還是 Name
function funCheckTCP_IP(sTarget:string;bLast:boolean):boolean;
var iA,iErr:integer;
    ss1:string;
begin
  result:=false;

  if bLast then
  begin
    try
      strtoint(sTarget);
    except
      iErr:=1;
    end;

    if iErr=1 then EXIT;

    if (iErr=0) then
        if not((strtoint(sTarget)>=0) and (strtoint(sTarget)<=9)) then  iErr:=1;

    if iErr=1 then EXIT;

    result:=true;
    EXIT;
  end;

  iA:=0;
  iErr:=0;

  iA:=pos('.',sTarget);

  if iA = 0 then EXIT;

  ss1:=copy(sTarget,1,iA-1);

  try
    strtoint(ss1);
  except
    iErr:=1;
  end;

  if iErr=1 then EXIT;

  if (iErr=0) then
        if not((strtoint(ss1)>=0) and (strtoint(ss1)<=255)) then  iErr:=1;

  if iErr=1 then EXIT;

  result:=true;
end;

//2012.04.12 add,以拆字的方式判斷是 TCP/IP 還是 Name
function funConnectMode(sServerName:string):integer;
var iB,iC,iD:integer;
    sTempStr:string;
begin
  result:=0;// is Name

  iB:=0;
  iC:=0;

//iA 第一段 IP
  if funCheckTCP_IP(sServerName,false)=false then exit;

//iB 第二段 IP
  iB:=pos('.',sServerName);

  sTempStr:=copy(sServerName,iB+1,length(sServerName));

  if funCheckTCP_IP(sTempStr,false)=false then exit;

//iC 第三段 IP
  iC:=pos('.',sTempStr);

  sTempStr:=copy(sTempStr,iC+1,length(sTempStr));

  if funCheckTCP_IP(sTempStr,false)=false then exit;

//iC 第四段 IP,只能檢查第一碼
  iD:=pos('.',sTempStr);

  sTempStr:=copy(sTempStr,iD+1,1);

  if funCheckTCP_IP(sTempStr,true)=false then exit;

//Finally
  result:=1;// is TCP/IP
end;

//2012.04.25 add for Johnson-20120425-01
function funGetNetworkLibrary():string;
var sFileName:string;
  sList:TStringList;
  i:integer;
  sNetworkLibraryId:string;
  sNetWorkLibrary:string;
begin
  result:='';

  sFileName:='IsOCX.txt';

  if not FileExists(sFileName) then
     begin
       //MsgDlgJS('連線設定檔 '+sFileName+' 不存在',mtError,[mbOk],0); //2012.04.26 disable for 相容無此需求的客戶
       exit;
     end;

  sList:=TStringList.Create;
  sList.LoadFromFile(sFileName);

  if sList.Count<6 then
     begin
       sList.free;
       //MsgDlgJS('連線設定檔 '+sFileName+' 的設定值有誤',mtError,[mbOk],0); //2012.04.26 disable for 相容無此需求的客戶
       exit;
     end;

  sNetworkLibraryId:=sList.Strings[5];

  sList.free;

  if not((sNetworkLibraryId='0') or (sNetworkLibraryId='1') or (sNetworkLibraryId='2')) then
     begin
       //MsgDlgJS('連線設定檔 '+sFileName+' 的設定值有誤',mtError,[mbOk],0); //2012.04.26 disable for 相容無此需求的客戶
       exit;
     end;

  if sNetworkLibraryId='1' then sNetWorkLibrary:=';NetWork Library=dbmssocn' // TCP/IP
  else if sNetworkLibraryId='2' then sNetWorkLibrary:=';NetWork Library=dbnmpntw' //Name
  else sNetWorkLibrary:='';

  Result:=sNetWorkLibrary;
end;

function funConnectStrGet(
  sServerName,
  sDBName
  ,sTempBasJSISpw,sGlobalId //2012.06.01 add for SS Bill-20120531-01
  :string
  ):string;
var sPw:string;
  sNetWorkLibrary:string;//2012.04.12 add
  //iLC:integer;//2012.09.22 add
begin
  //if funCheckX86='1' then sPw:='Jsis_2009' else sPw:='JSIS'; //保留

{ //2012.04.12 disable,但要保留,
  //因有客戶的 Client 不會做 TCP IP與 Name 的互相解析，導致連線更緩慢或連不上
  //若客戶的Server設定用TCP/IP，但部分Client不會解析時，用Name還連的上 若統一使用TCP/IP
  //則可能造成問題，反之，若客戶的Server設定用Name也可能有類似的狀況(Client用Name連不上)

  //以拆字的方式判斷是 TCP/IP 還是 Name

  if funConnectMode(sServerName)=1 then
     sNetWorkLibrary:='dbmssocn'
  else
     sNetWorkLibrary:='dbnmpntw';
}

  //----------2012.04.25 add for Johnson-20120425-01
  sNetWorkLibrary:=funGetNetworkLibrary();
  //----------

  {
  sPw:='JSIS';

  result
    :='Provider=SQLOLEDB.1;Password='+sPw+';Persist Security Info=True;'+
      'User ID=JSIS;Initial Catalog='+sDBName+';Data Source='+sServerName
      //+';NetWork Library='+sNetWorkLibrary  //2012.04.12 disable,但要保留
      +sNetWorkLibrary //2012.04.25 add for Johnson-20120425-01
      ;
  }
  //2012.06.01 add for SS Bill-20120531-01
  if sTempBasJSISpw='' then sTempBasJSISpw:='JSIS';
  {
  //2012.09.22 modify
  if sTempBasJSISpw='' then
    //unit_KEY.GetDBKey(sServerName,sTempBasJSISpw,iLC);
    begin
      MsgDlgJS('JSIS的密碼不允許空白',mtError,[mbOk],0);//2012.09.23 modify
      exit;
    end;
  }
  //2012.06.01 add for SS Bill-20120531-01
  result:='Provider=SQLOLEDB.1;Password='+sTempBasJSISpw+';Persist Security Info=True;' +
      'User ID=JSIS;Initial Catalog='+sDBName+';Data Source='+sServerName+
      ';Use Procedure for Prepare=1;Auto Translate=True;Packet Size=4096;'+
      'Application Name='+sGlobalId+
      //;Workstation ID=XXX;' ////電腦名稱讓ADO自己傳遞(避免 unicode 的傳遞出問題)
      ';Use Encryption for Data=False;Tag with column collation when possible=False'
      +sNetWorkLibrary
      ;
end;

function funConnectStrGetAdmin(
  sServerName,
  sDBName,
  sPw:string //2012.09.21 add
  ):string;
var //sPw:string;
  sNetWorkLibrary:string;//2012.04.25 add for Johnson-20120425-01
begin

  //----------2012.04.25 add for Johnson-20120425-01
  sNetWorkLibrary:=funGetNetworkLibrary();
  //----------

  //if funCheckX86='1' then sPw:='Jsis_JSIS0309' else sPw:='JSIS0309';//保留

  //sPw:='JSIS0309'; //2012.09.21 disable

  result
    :='Provider=SQLOLEDB.1;Password='+sPw+';Persist Security Info=True;'+
      'User ID=JSISAdmin;Initial Catalog='+sDBName+';Data Source='+sServerName
      +sNetWorkLibrary //2012.04.25 add for Johnson-20120425-01
      ;
end;

function funCustBtnDo(
  afm:TForm;
  qry:TADOQuery;
  tTable:TJSdTable;
  CutBtn:TSpeedButton
  ):boolean;
//var sSQL:string;
begin
  result:=true;
end;

function OcxCallOcxDlg(
  afm:TForm;
  sOcxName,
  sCoClassName,
  sUserId,
  sBUId,
  sUseId,
  sItemId,
  sSERVERName,
  sDBName,
  sGlobalId,
  sPaperNum
  :string
  ):boolean;{
var
    XForm: TfrmOleContainerOCX;
    i:integer;
    sTempList:TStringList;
    sInfoFileName:string;
    sProfilePath:string;}
begin  {
  sProfilePath:=unit_MIS.GetTempPathStr;

  if not fileexists(sProfilePath+sUseId+'\'+sOcxName+'.ocx') then
    begin
      MsgDlgJS(sProfilePath+sUseId+'\'+sOcxName+'.ocx'+'不存在', mtError, [mbOk], 0);
      exit;
    end;

  sTempList:=TStringList.Create;
      try
              with sTempList do
                begin
                  Add('ItemId='+sItemId);
                  Add('OcxName='+sOcxName);
                  Add('CoClassName='+sCoClassName);
                  Add('UserId='+sUserId);
                  Add('BUID='+sBUId);
                  Add('SERVERName='+sSERVERName);
                  Add('DBName='+sDBName);
                  Add('UseId='+sUseId);
                  Add('GlobalId='+sGlobalId);
                  Add('PaperNum='+sPaperNum);

                  sInfoFileName:=sProfilePath+sCoClassName+'.txt';

                  SaveToFile(sInfoFileName);
                end;
       finally
              sTempList.Free;
       end;
      //=====

  XForm := TfrmOleContainerOCX.Create(afm);
  XForm.CreateOleWithReg(sOcxName+'.ocx');
  XForm.Caption :=sOcxName;
  TForm(XForm).Show;
  }
  result:=true;
end;



function DLLSetPowerType(
  iOpKind:integer;
  afm:TForm;
  qry:TADOQuery;
  sItemId:string;
  sUserId:string;
  sUseId:string;
  sRealTableNameMas1:string;
   //--Item
  var PaperType :integer;
  var CurrTypeHead: WideString;
  var PowerType: integer;
  var FunctionType: integer;
    //--User Power
  var CanbUpdate	:Integer;
  var CanbUpdateMoney	:Integer;
  var CanbAudit	:Integer;
  var CanbAuditBack	:Integer;
  var CanbScrap	:Integer;
  var CanbViewMoney	:Integer;
  var CanbPrint	:Integer;
  var CanbUpdateNotes :Integer;
    //--Paper Power
  var CanbRunFLow	:Integer;
  var CanbSelectType :Integer;
  var CanbLockPaperDate :Integer;
  var CanbLockUserEdit :Integer;
  var CanbMustNotes :Integer;

  var CanbExport	:Integer;//2015.11.17 add for Bill-20151112-03
  var CanbF9	    :Integer;//2015.11.17 add for Bill-20151112-03
  var CanbF12	    :Integer  //2015.11.17 add for Bill-20151112-03
  ):boolean;
var sSQL:string;
begin
  //--Item
  sSQL:='';
  sSQL:='select PaperType,PowerType,FunctionType from CURdSysItems(nolock)'+
    ' where ItemId='+''''+sItemId+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if iOpKind<> -1 then
  begin
    if qry.RecordCount=0 then
      begin
        qry.Close;
        MsgDlgJS('程式'+sItemId+'未設定', mtError, [mbOk], 0);
        result:=false;
        exit;
      end;
  end;

  if qry.RecordCount>0 then
  with qry do
    begin
      PaperType:=fieldbyname('PaperType').AsInteger;
      PowerType:=fieldbyname('PowerType').AsInteger;
      FunctionType:=fieldbyname('FunctionType').AsInteger;
    end;

  //--User Power
  sSQL:='';
  sSQL:='exec CURdGetUserSysItems '+
        ''''+''+''''+','+
        ''''+sUserId+''''+','+
        ''''+''+''''+','+
        ''''+sUseId+''''+','+
        '0,'+
        ''''+sItemId+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if iOpKind<> -1 then
  begin
  if qry.RecordCount=0 then
    begin
      qry.Close;
      MsgDlgJS('使用者'+sUserId+'對程式'+sItemId+'無權限', mtError, [mbOk], 0);
      result:=false;
      exit;
    end;
  end;

  if qry.RecordCount>0 then
  with qry do
    begin
      CanbUpdate      :=fieldbyname('bUpdate').AsInteger;
      CanbUpdateMoney :=fieldbyname('bUpdateMoney').AsInteger;
      CanbAudit       :=fieldbyname('bAudit').AsInteger;
      CanbAuditBack   :=fieldbyname('bAuditBack').AsInteger;
      CanbScrap       :=fieldbyname('bScrap').AsInteger;
      CanbViewMoney   :=fieldbyname('bViewMoney').AsInteger;
      CanbPrint       :=fieldbyname('bPrint').AsInteger;
      CanbUpdateNotes :=fieldbyname('bUpdateNotes').AsInteger;

      CanbExport	:=fieldbyname('bExport').AsInteger;//2015.11.17 add for Bill-20151112-03
      CanbF9	    :=fieldbyname('bF9').AsInteger;//2015.11.17 add for Bill-20151112-03
      CanbF12	    :=fieldbyname('bF12').AsInteger;  //2015.11.17 add for Bill-20151112-03
    end;

  //--Paper Power
  if iOpKind=1 then
  begin
    sSQL:='';
    sSQL:='select RunFlow,SelectType,LockPaperDate,LockUserEdit,MustNotes'+
      ' from CURdPaperInfo(nolock) where PaperId='+''''+sRealTableNameMas1+'''';

    with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

    if qry.RecordCount=0 then
    begin
      qry.Close;
      MsgDlgJS('單據'+sRealTableNameMas1+'未設定PaperInfo', mtError, [mbOk], 0);
      result:=false;
      exit;
    end;

    with qry do
    begin
      CanbRunFLow:=fieldbyname('RunFlow').AsInteger;
      CanbSelectType:=fieldbyname('SelectType').AsInteger;
      CanbLockPaperDate:=fieldbyname('LockPaperDate').AsInteger;
      CanbLockUserEdit:=fieldbyname('LockUserEdit').AsInteger;
      CanbMustNotes:=fieldbyname('MustNotes').AsInteger;
    end;
  end;

  result:=true;
end;

procedure prcSaveALL(fm:TForm);
var i:integer;
begin
  for i := 0 to fm.ComponentCount - 1 do
    if fm.Components[i] is TDataSet then
      if TDataSet(fm.Components[i]).Active then
       if TDataSet(fm.Components[i]).State in[dsInsert,dsEdit] then
          TDataSet(fm.Components[i]).Post;
end;

//2011.9.22 add
procedure prcCancelALL(fm:TForm);
var i:integer;
begin
  for i := 0 to fm.ComponentCount - 1 do
    if fm.Components[i] is TDataSet then
      if TDataSet(fm.Components[i]).Active then
       if TDataSet(fm.Components[i]).State in[dsInsert,dsEdit] then
          TDataSet(fm.Components[i]).Cancel;
end;

function funGetFilterSQL(
  qry:TADOQuery;
  sItemId:string;
  sTableKind:string
  ):string;
var sSQL:string; sFilterSQL:string;
begin
  sFilterSQL:='';

  sSQL:='exec CURdOCXFilterSQLGet '+
        ''''+sItemId+''''+','+''''+sTableKind+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount>0 then
     sFilterSQL:=qry.Fields[0].AsString;

  qry.Close;

  result:=sFilterSQL;
end;


function funGetOrderByField(
  qry:TADOQuery;
  sItemId:string;
  sTableKind:string
  ):string;
var sSQL:string; sOrderByField:string;
begin
  sOrderByField:='';

  sSQL:='exec CURdOCXOrderByFieldGet '+
        ''''+sItemId+''''+','+''''+sTableKind+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount>0 then
     sOrderByField:=qry.Fields[0].AsString;
  //2012.06.11 add for EMO
  with qry do
  begin
    if active then qry.close;
    SQL.Clear;
    SQL.Add('select ItemId from CURdOCXItemOtherRule(nolock) where ItemId='''
            +sItemId+''' and RuleId=''SpecialOrder'' and DLLValue=''1''');
    Open;
    if Recordcount>0 then
    begin
      qry.close;
      SQL.Clear;
      SQL.Add('exec EMOdOCXOrderByFieldGet '''+ sOrderByField+'''');
      Open;
      sOrderByField:=FieldByName('NewStr').AsString;
      qry.close;
    end;
  end;

  qry.Close;

  result:=sOrderByField;
end;

function funDrawPaperPNL2(
  afm:TForm;
  tTable:TJSdTable;
  qry:TADOQuery;
  iOpKind:integer;
  CanbLockPaperDate:integer;
  CanbViewMoney:integer
  ):boolean;
var i:integer;sSQL:string;
  dsLK:TDataSource;
  sList:TstringList;
  sFontSize:string;
  FontSize: integer;
begin
  if not CanbViewMoney in[0,1] then CanbViewMoney:=0;

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

  sSQL:='exec CURdOCXDrawComponetGet2 '+
    ''''+tTable.TableName+''''+','+
    inttostr(iOpKind)+','+
    inttostr(CanbLockPaperDate)+','+
    ''''+tTable.ReserveList.Values['LanguageId']+''''+','+
    inttostr(CanbViewMoney)+',1,'+inttostr(FontSize)
    ;

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount=0 then
    begin
      qry.Close;
      result:=false;
      exit;
    end;

  with qry do
    begin
      first;
      i:=1;
      while not eof do
        begin
                CreateJSdLabel2(
                FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                FieldByName('iLabTop').AsInteger,//iTop: integer;
                FieldByName('iLabLeft').AsInteger,//iLeft: integer;
                FieldByName('iLabHeight').AsInteger,//iHeight: integer;
                FieldByName('iLabWidth').AsInteger,//iWidth: integer;
                afm,//AOwner: TComponent;
                TPanel(afm.FindComponent('pnlMaster'
                        +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl
                TDataSource(afm.FindComponent('dsBrowse')),//dsBrowse,
                FieldByName('FieldName').Asstring
                );

                case FieldByName('iType').AsInteger of
                  0:begin
                    if FieldByName('IsMEMO').AsInteger=0 then
                    begin
                      if tTable.ReserveList.Values[FieldByName('FieldName').Asstring+'^ITEMS']<>'' then
                        CreateDBComoBox2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring,
                        tTable.ReserveList.Values[FieldByName('FieldName').Asstring+'^ITEMS']
                        )
                      else
                      CreateDBEdit2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                    end //if FieldByName('IsMEMO').AsInteger=0 then
                    else
                    begin
                      CreateDBMemo2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                    end;
                    end;//0
                  1:begin
                      CreateDBDateTimePicker2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        //Default,
                        //sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                    end;
                  2:begin

                      dsLK:=TDataSource.Create(afm);

                      CreateJSdLookupComboOCX2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        FieldByName('sLKSQL').Asstring,//SQL: WideString;
                        '',//Default: WideString;
                        nil,//lookds: TDataSource;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)), //AParent: TWinControl
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').asString,//sDataField:WideString
                        dsLK,
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('ComboTextSize').asinteger,
                        FieldByName('EditColor').Asstring,
                        FieldByName('LookupTable').Asstring,
                        FieldByName('LookupResultField').Asstring,
                        FieldByName('LookupCond1Field').Asstring,
                        FieldByName('LookupCond2Field').Asstring,
                        FieldByName('LookupCond1ResultField').Asstring,
                        FieldByName('LookupCond2ResultField').Asstring,
                        FieldByName('iShowSeq').asinteger //2022.04.20
                        );
                    end;
                  3:begin

                      CreateDBCheckBox2(
                        FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger
                        );
                    end;
                end;//case FieldByName('iType').AsInteger of
          inc(i);

          //for EMO if i=7 then i:=1;

          next;
        end;//while not eof do
      close;
    end;//with qryExec do

  result:=true;
end;

//2013.10.23
function funDrawPaperPNL_EMO(
  afm:TForm;
  tTable:TJSdTable;
  qry:TADOQuery;
  iOpKind:integer;
  CanbLockPaperDate:integer;
  CanbViewMoney:integer
  ):boolean;
var i:integer;sSQL:string;
  dsLK:TDataSource;
  //2012.05.08 for EMO
  sSpStr: String;
  tDBEditTmp: TDBEdit; //2013.10.18 for EMO
  sCusId: String; //Only For NIS
  sPlusId: String; //2016.08.10 for YS
  sList:TstringList;
  sFontSize:string;
  FontSize: integer;
begin
  if not CanbViewMoney in[0,1] then CanbViewMoney:=0;

  //2012.05.08 for EMO
  if tTable.TableName='EMOdProdLayer' then
    sSpStr:='EMOdOCXDrawComponetGet2'
  else
    sSpStr:='CURdOCXDrawComponetGet2';

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

  sSQL:='exec '+sSpStr+' '+
    ''''+tTable.TableName+''''+','+
    inttostr(iOpKind)+','+
    inttostr(CanbLockPaperDate)+','+
    ''''+tTable.ReserveList.Values['LanguageId']+''''+','+
    inttostr(CanbViewMoney)
    ;

  //2020.08.27
  if sSpStr='CURdOCXDrawComponetGet2' then
  begin
     sSQL := sSQL+',1,'+inttostr(FontSize);
  end;

  //2013.10.23 Only For NIS
  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'''
        +' and ParamId=''CusId''');
      open;
      sCusId:=FieldByName('Value').AsString;
      qry.Close;
    end;

  //2016.08.10
  sPlusId:='';
  if (sCusId='YS') and (tTable.TableName='EMOdProdInfo') then
    sPlusId:='1';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount=0 then
    begin
      qry.Close;
      result:=false;
      exit;
    end;

  with qry do
    begin
      first;
      i:=1;
      while not eof do
        begin
            //2012.04.27 for EMO
                CreateJSdLabel2(
                FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                FieldByName('iLabTop').AsInteger,//iTop: integer;
                FieldByName('iLabLeft').AsInteger,//iLeft: integer;
                FieldByName('iLabHeight').AsInteger,//iHeight: integer;
                FieldByName('iLabWidth').AsInteger,//iWidth: integer;
                afm,//AOwner: TComponent;
                TPanel(afm.FindComponent('pnlMaster'
                        +sPlusId+FieldByName('iShowWhere').Asstring)),//AParent: TWinControl
                TDataSource(afm.FindComponent('dsBrowse')),//dsBrowse,
                ''
                );

                case FieldByName('iType').AsInteger of
                  0:begin
                    if FieldByName('IsMEMO').AsInteger=0 then
                    begin
                      if tTable.ReserveList.Values[FieldByName('FieldName').Asstring+'^ITEMS']<>'' then
                        CreateDBComoBox2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +sPlusId+FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring,
                        tTable.ReserveList.Values[FieldByName('FieldName').Asstring+'^ITEMS']
                        )
                      else
                      begin
                      tDBEditTmp:= //2013.10.18 for EMO
                      CreateDBEdit2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +sPlusId+FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                      if sCusId='NIS' then
                           tDBEditTmp.Field.Alignment:=taLeftJustify;
                      end;
                    end //if FieldByName('IsMEMO').AsInteger=0 then
                    else
                    begin
                      CreateDBMemo2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +sPlusId+FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                    end;
                    end;//0
                  1:begin
                      CreateDBDateTimePicker2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        //Default,
                        //sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +sPlusId+FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                    end;
                  2:begin

                      dsLK:=TDataSource.Create(afm);

                      CreateJSdLookupComboOCX2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        FieldByName('sLKSQL').Asstring,//SQL: WideString;
                        '',//Default: WideString;
                        nil,//lookds: TDataSource;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +sPlusId+FieldByName('iShowWhere').Asstring)), //AParent: TWinControl
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').asString,//sDataField:WideString
                        dsLK,
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('ComboTextSize').asinteger,
                        FieldByName('EditColor').Asstring,
                        FieldByName('LookupTable').Asstring,
                        FieldByName('LookupResultField').Asstring,
                        FieldByName('LookupCond1Field').Asstring,
                        FieldByName('LookupCond2Field').Asstring,
                        FieldByName('LookupCond1ResultField').Asstring,
                        FieldByName('LookupCond2ResultField').Asstring,
                        0 //2022.04.20
                        );
                    end;
                  3:begin

                      CreateDBCheckBox2(
                        FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +sPlusId+FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger
                        );
                    end;
                  //2012.05.08 add for EMO
                  11:begin
                      CreateDBComoBox2_Mas(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +sPlusId+FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring,
                        //tTable.ReserveList.Values[FieldByName('FieldName').Asstring+'^ITEMS'],
                        FieldByName('Items').AsWidestring,
                        FieldByName('iTypeGroup').AsInteger
                        )
                  end;
                  12:begin
                      CreateJSdLookupComboSubDLL_Dtl(
                        FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        FieldByName('sLKSQL').Aswidestring,//SQL: WideString;
                        '',//Default: WideString;
                        'cboMas_'+IntToStr(FieldByName('iTypeGroup').AsInteger),//sSId: SuperId;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +sPlusId+FieldByName('iShowWhere').Asstring)), //AParent: TWinControl
                        FieldByName('iTemp').AsInteger,
                        //2012.05.09 add
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').asString,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                  end;

                end;//case FieldByName('iType').AsInteger of
          inc(i);

          //for EMO if i=7 then i:=1;

          next;
        end;//while not eof do
      close;
    end;//with qryExec do

  result:=true;
end;

//2012.05.28 add for WF Bill-20120518-02A
function funDrawPNL3(
  afm:TForm;
  tTable:TJSdTable;
  qry:TADOQuery;
  iOpKind:integer;
  CanbLockPaperDate:integer;
  CanbViewMoney:integer;
  ds:TDataSource ;
  CanbUpdateMoney:integer //2017.03.06 add for SS
  ):boolean;
var i:integer;sSQL:string;
    dsLK:TDataSource;
    sList:TstringList;  //2020.08.13
    sFontSize:string;   //2020.08.13
    FontSize: integer;  //2020.08.13
begin
  if not CanbViewMoney in[0,1] then CanbViewMoney:=0;

  //2020.08.13
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

  sSQL:='exec CURdOCXDrawComponetGet2 '+
    ''''+tTable.TableName+''''+','+
    inttostr(iOpKind)+','+
    inttostr(CanbLockPaperDate)+','+
    ''''+tTable.ReserveList.Values['LanguageId']+''''+','+
    inttostr(CanbViewMoney)
   + ',' +inttostr(CanbUpdateMoney) //2017.03.06 add for SS
   + ',' +inttostr(FontSize)  //2020.08.13
    ;

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount=0 then
    begin
      qry.Close;
      result:=false;
      exit;
    end;

  with qry do
    begin
      first;
      i:=1;
      while not eof do
        begin
                CreateJSdLabel2(
                FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                FieldByName('iLabTop').AsInteger,//iTop: integer;
                FieldByName('iLabLeft').AsInteger,//iLeft: integer;
                FieldByName('iLabHeight').AsInteger,//iHeight: integer;
                FieldByName('iLabWidth').AsInteger,//iWidth: integer;
                afm,//AOwner: TComponent;
                TPanel(afm.FindComponent('pnlMaster'
                        +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl
                ds,//dsBrowse,
                FieldByName('FieldName').Asstring
                );

                case FieldByName('iType').AsInteger of
                  0:begin
                    if FieldByName('IsMEMO').AsInteger=0 then
                    begin
                      if tTable.ReserveList.Values[FieldByName('FieldName').Asstring+'^ITEMS']<>'' then
                        CreateDBComoBox2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        ds,//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring,
                        tTable.ReserveList.Values[FieldByName('FieldName').Asstring+'^ITEMS']
                        )
                      else
                      CreateDBEdit2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        ds,//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring
                        )
                    end //if FieldByName('IsMEMO').AsInteger=0 then
                    else
                    begin
                      CreateDBMemo2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        ds,//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('IsNotesField').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                    end;
                    end;//0
                  1:begin
                      CreateDBDateTimePicker2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        //Default,
                        //sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        ds,//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('EditColor').Asstring
                        );
                    end;
                  2:begin

                      dsLK:=TDataSource.Create(afm);

                      CreateJSdLookupComboOCX2(
                        //FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        FieldByName('HintComment').Aswidestring,//2011.9.22 modify for MUT Bill-20110921-02B
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        FieldByName('sLKSQL').Asstring,//SQL: WideString;
                        '',//Default: WideString;
                        nil,//lookds: TDataSource;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)), //AParent: TWinControl
                        ds,//tDataSource:TDataSource;
                        FieldByName('FieldName').asString,//sDataField:WideString
                        dsLK,
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('ComboTextSize').asinteger,
                        FieldByName('EditColor').Asstring,
                        FieldByName('LookupTable').Asstring,
                        FieldByName('LookupResultField').Asstring,
                        FieldByName('LookupCond1Field').Asstring,
                        FieldByName('LookupCond2Field').Asstring,
                        FieldByName('LookupCond1ResultField').Asstring,
                        FieldByName('LookupCond2ResultField').Asstring,
                        0 //2022.04.20
                        );
                    end;
                  3:begin

                      CreateDBCheckBox2(
                        FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        i,//iPrompt,
                        FieldByName('iFieldTop').AsInteger,//sTop,
                        FieldByName('iFieldLeft').AsInteger,//sLeft,
                        FieldByName('iFieldHeight').AsInteger,//sHeight,
                        FieldByName('iFieldWidth').AsInteger,//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnlMaster'
                          +FieldByName('iShowWhere').Asstring)),//AParent: TWinControl;
                        ds,//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger
                        );
                    end;

                end;//case FieldByName('iType').AsInteger of
          inc(i);

          if i=7 then i:=1;

          next;
        end;//while not eof do
      close;
    end;//with qryExec do

  result:=true;
end;


function funDrawPaperPNL(
  afm:TForm;
  tTable:TJSdTable;
  qry:TADOQuery;
  iOpKind:integer;
  CanbLockPaperDate:integer
  ):boolean;
var i:integer;sSQL:string;
  dsLK:TDataSource;
  sList:TstringList;
  sFontSize:string;
  FontSize: integer;
begin
  sSQL:='exec CURdOCXDrawComponetGet '+
    ''''+tTable.TableName+''''+','+
    inttostr(iOpKind)+','+
    inttostr(CanbLockPaperDate)
    ;
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

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount=0 then
    begin
      qry.Close;
      result:=false;
      exit;
    end;

  with qry do
    begin
      first;
      i:=1;
      while not eof do
        begin
                CreateJSdLabel(
                FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                ((VHeight div 2)+(i*VHeight)-34),//iTop: integer;
                (HBlock div 2),//iLeft: integer;
                (VHeight-3),//iHeight: integer;
                (TPanel(afm.FindComponent('pnl_Label'
                        +FieldByName('iShowWhere').Asstring
                        +'_'
                        +FieldByName('iShowSeq').Asstring)).Width-HBlock),//iWidth: integer;
                afm,//AOwner: TComponent;
                TPanel(afm.FindComponent('pnl_Label'
                        +FieldByName('iShowWhere').Asstring
                        +'_'
                        +FieldByName('iShowSeq').Asstring)),//AParent: TWinControl
                TDataSource(afm.FindComponent('dsBrowse')),//dsBrowse,
                FieldByName('FieldName').Asstring
                );

                case FieldByName('iType').AsInteger of
                  0:begin
                      CreateDBEdit(
                        FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        i,//iPrompt,
                        ((VHeight div 2)+(i*VHeight)-34),//sTop,
                        (HBlock div 2),//sLeft,
                        (VHeight-3),//sHeight,
                        (TPanel(afm.FindComponent('pnl_Field'
                        +FieldByName('iShowWhere').Asstring
                        +'_'
                        +FieldByName('iShowSeq').Asstring)).Width-HBlock),//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnl_Field'
                          +FieldByName('iShowWhere').Asstring
                          +'_'
                          +FieldByName('iShowSeq').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger
                        );
                    end;
                  1:begin
                      CreateDBDateTimePicker(
                        FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        i,//iPrompt,
                        ((VHeight div 2)+(i*VHeight)-34),//sTop,
                        (HBlock div 2),//sLeft,
                        (VHeight-3),//sHeight,
                        (TPanel(afm.FindComponent('pnl_Field'
                        +FieldByName('iShowWhere').Asstring
                        +'_'
                        +FieldByName('iShowSeq').Asstring)).Width-HBlock),//sWidth,
                        0,//iDType: integer;
                        //Default,
                        //sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnl_Field'
                          +FieldByName('iShowWhere').Asstring
                          +'_'
                          +FieldByName('iShowSeq').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger
                        );
                    end;
                  2:begin
                      dsLK:=TDataSource.Create(afm);

                      CreateJSdLookupComboOCX(
                        FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        i,//iPrompt,
                        ((VHeight div 2)+(i*VHeight)-34),//sTop,
                        (HBlock div 2),//sLeft,
                        (VHeight-3),//sHeight,
                        (TPanel(afm.FindComponent('pnl_Field'
                        +FieldByName('iShowWhere').Asstring
                        +'_'
                        +FieldByName('iShowSeq').Asstring)).Width-HBlock),//sWidth,
                        0,//iDType: integer;
                        FieldByName('sLKSQL').Asstring,//SQL: WideString;
                        '',//Default: WideString;
                        nil,//lookds: TDataSource;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnl_Field'
                          +FieldByName('iShowWhere').Asstring
                          +'_'
                          +FieldByName('iShowSeq').Asstring)), //AParent: TWinControl
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').asString,//sDataField:WideString
                        dsLK,
                        FieldByName('iReadOnly').asinteger,
                        FieldByName('ComboTextSize').asinteger
                        );
                    end;
                  3:begin
                      CreateDBCheckBox(
                        FieldByName('DisplayLabel').Asstring,//sCaption: WideString;
                        i,//iPrompt,
                        ((VHeight div 2)+(i*VHeight)-34),//sTop,
                        (HBlock div 2),//sLeft,
                        (VHeight-3),//sHeight,
                        (TPanel(afm.FindComponent('pnl_Field'
                        +FieldByName('iShowWhere').Asstring
                        +'_'
                        +FieldByName('iShowSeq').Asstring)).Width-HBlock),//sWidth,
                        0,//iDType: integer;
                        ////Default,
                        ////sEditMask: WideString;
                        afm,//AOwner: TComponent;
                        TPanel(afm.FindComponent('pnl_Field'
                          +FieldByName('iShowWhere').Asstring
                          +'_'
                          +FieldByName('iShowSeq').Asstring)),//AParent: TWinControl;
                        TDataSource(afm.FindComponent('dsBrowse')),//tDataSource:TDataSource;
                        FieldByName('FieldName').Asstring,//sDataField:WideString
                        FieldByName('iReadOnly').asinteger
                        );
                    end;

                end;//case FieldByName('iType').AsInteger of
          inc(i);

          //for EMO if i=7 then i:=1;

          next;
        end;//while not eof do
      close;
    end;//with qryExec do

  result:=true;
end;

function CreateJSdLookupComboOCX(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  SQL: WideString;
  Default: WideString;
  lookds: TDataSource;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  dsLK: TDataSource;
  iReadOnly:integer;
  iComboTextSize:integer
  ): TJSdLookupCombo;
var qryLK: TADOQuery;
    //dsLK:TDataSource; 宣告在此compiler會出現錯誤訊息,改由呼叫端傳入
    iTextSize:integer;
    sList:TstringList; //2020.08.13
    sFontSize:string;  //2020.08.13
    FontSize: integer; //2020.08.13
begin
  if iComboTextSize<=0 then iTextSize:= sWidth div 3
    else if iComboTextSize >= sWidth then iTextSize:= sWidth - 4
    else  iTextSize:= iComboTextSize;

  Result := TJSdLookupCombo.Create(AOwner);
  //2020.08.13
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
  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    TextSize := iTextSize;//sWidth div 3;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    HelpContext:= iDType;
    LkDataSource:= lookds;
    //if iDType=2 then
    //  Enabled:=false
    //else
    //  Enabled:=true;

    Enabled:=(iReadOnly=0);
  end;

  if SQL<>'' then
  begin
      qryLK:= TADOQuery.Create(AOwner);
      qryLK.ConnectionString:=TJsdTable(tDataSource.DataSet).ConnectionString;
      qryLK.LockType:= ltReadOnly;
      qryLK.SQL.clear;
      qryLK.SQL.add(SQL);

      //dsLK:= TdataSource.Create(AOwner); //改由呼叫端傳入
      dsLK.DataSet:= qryLK;
      Result.LkDataSource:= dsLK;
      qryLK.Open;
  end;

 Result.DataSource:=tDataSource;
 Result.DataField:=sDataField;
end;

procedure TJSdLookUpComboEnter.prcJSdLookUpComboEnter(Sender: TObject);
var
  newSQL,
  OrgSQL,
  LookupCond1Field,
  LookupCond2Field,
  LookupCond1ResultField,
  LookupCond2ResultField:string;

  nowValue1,
  nowValue2:string;
begin
  with TJSdTable(TJSdLookupCombo(Sender).LkDataSource.DataSet) do

    begin

      OrgSQL:=ReserveList.Values['OrgSQL'];

      LookupCond1Field:=ReserveList.Values['LookupCond1Field'];

      LookupCond2Field:=ReserveList.Values['LookupCond2Field'];
      LookupCond1ResultField:=ReserveList.Values['LookupCond1ResultField'];
      LookupCond2ResultField:=ReserveList.Values['LookupCond2ResultField'];
    end;

  with TJSdTable(TJSdLookupCombo(Sender).DataSource.DataSet) do
    begin
      if FindField(LookupCond1ResultField)<>nil then
        nowValue1:=FindField(LookupCond1ResultField).AsString;

      if FindField(LookupCond2ResultField)<>nil then
        nowValue2:=FindField(LookupCond2ResultField).AsString;
    end;

   newSQL:=OrgSQL;

   if ((LookupCond1Field<>'') and (LookupCond1ResultField<>'')) then
      begin
        newSQL:=newSQL+' and '+LookupCond1Field+'='+''''+nowValue1+'''';
      end;

   if ((LookupCond2Field<>'') and (LookupCond2ResultField<>'')) then
      begin
        newSQL:=newSQL+' and '+LookupCond2Field+'='+''''+nowValue2+'''';
      end;

  with TJSdTable(TJSdLookupCombo(Sender).LkDataSource.DataSet) do
    begin
      close;
      sql.Clear;
      sql.Add(newSQL);
      Open;
    end;
end;

function CreateJSdLookupComboOCX2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  SQL: WideString;
  Default: WideString;
  lookds: TDataSource;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  dsLK: TDataSource;
  iReadOnly:integer;
  iComboTextSize:integer;
  sEditColor:string;
  LookupTable,
  LookupResultField,
  LookupCond1Field,
  LookupCond2Field,
  LookupCond1ResultField,
  LookupCond2ResultField:string;
  iShowSeq:integer
  ): TJSdLookupCombo;
var qryLK:TJSdTable; //TADOQuery;
    //dsLK:TDataSource; 宣告在此compiler會出現錯誤訊息,改由呼叫端傳入
    iTextSize:integer;
    EnterEvent: TJSdLookUpComboEnter;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  if iComboTextSize<=0 then iTextSize:= sWidth div 3
    else if iComboTextSize >= sWidth then iTextSize:= sWidth - 4
    else  iTextSize:= iComboTextSize;

  Result := TJSdLookupCombo.Create(AOwner);

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

  with Result do
  begin
    hint := sCaption;
    ShowHint:=(sCaption<>'');//2011.9.22 add for MUT Bill-20110921-02B
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    HelpContext:= iDType;
    //LkDataSource:= lookds;
    //if iDType=2 then
    //  Enabled:=false
    //else
    //  Enabled:=true;

    Enabled:=(iReadOnly=0);
    if sEditColor<>'' then cboColor:=StringToColor(sEditColor);
  end;

  if SQL<>'' then
  begin
      qryLK:=TJSdTable.Create(AOwner); //TADOQuery.Create(AOwner);
      qryLK.ConnectionString:=TJsdTable(tDataSource.DataSet).ConnectionString;
      qryLK.LockType:= ltReadOnly;
      qryLK.TableName:=LookupTable;
      qryLK.SQL.clear;
      qryLK.SQL.add(SQL);

      //dsLK:= TdataSource.Create(AOwner); //改由呼叫端傳入
      dsLK.DataSet:= qryLK;
      Result.LkDataSource:= dsLK;

      qryLK.Open;
  end;

 Result.DataSource:=tDataSource;
 Result.DataField:=sDataField;
 Result.TextSize := iTextSize; //要放在最後才有作用

 if Assigned(qryLK) then  //2010.7.1 add for YX RA10063001
  if ((LookupCond1Field<>'') and (LookupCond1ResultField<>'')) or
     ((LookupCond2Field<>'') and (LookupCond2ResultField<>'')) then
      begin
        qryLK.ReserveList.Add('OrgSQL='+SQL);

        qryLK.ReserveList.Add('LookupCond1Field='+LookupCond1Field);
        qryLK.ReserveList.Add('LookupCond1ResultField='+LookupCond1ResultField);

        qryLK.ReserveList.Add('LookupCond2Field='+LookupCond2Field);
        qryLK.ReserveList.Add('LookupCond2ResultField='+LookupCond2ResultField);

        //2022.04.20
        if iShowSeq=1 then
          qryLK.ReserveList.Add('ReOpen=1')
        else
          qryLK.ReserveList.Add('ReOpen=0');

        EnterEvent:=TJSdLookUpComboEnter.Create;
        Result.OnEnter:=EnterEvent.prcJSdLookUpComboEnter;
      end;
end;


function CreateJSdLookupComboDLL(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  SQL: WideString;
  Default: WideString;
  lookds: TDataSource;
  AOwner: TComponent;
  AParent: TWinControl;
  sConnectStr:string;
  iReadOnly:Integer
  ): TJSdLookupCombo;
var qryLK: TADOQuery;
    //dsLK:TDataSource; 宣告在此compiler會出現錯誤訊息,改由呼叫端傳入
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TJSdLookupCombo.Create(AOwner);
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

  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    TextSize := sWidth div 2;
    Top := sTop; //Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight - 5; //2020.12.15 sHeight; //Round(sHeight*(FontSize/100));
    Width := sWidth;
    HelpContext:= iDType;
    LkDataSource:= lookds;
  end;

  if SQL<>'' then
  begin
      qryLK:= TADOQuery.Create(AOwner);
      qryLK.ConnectionString:=sConnectStr;
      qryLK.LockType:= ltReadOnly;
      qryLK.SQL.clear;
      qryLK.SQL.add(SQL);

      lookds.DataSet:= qryLK;
      qryLK.Open;
  end;

  if Default<>'' then
    Result.text:=Default;

  Result.Enabled:=not(iReadOnly=1);

  if sCaption='' then Result.Visible:=false;//2010.10.28 add

end;

//2018.11.30
function CreateJSdLookupComboDLL2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  sTextWidth,
  iDType: integer;
  SQL: WideString;
  Default: WideString;
  lookds: TDataSource;
  AOwner: TComponent;
  AParent: TWinControl;
  sConnectStr:string;
  iReadOnly:Integer
  ): TJSdLookupCombo;
var qryLK: TADOQuery;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
    //dsLK:TDataSource; 宣告在此compiler會出現錯誤訊息,改由呼叫端傳入
begin
  Result := TJSdLookupCombo.Create(AOwner);
  //2020.08.12
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
  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    if ((sTextWidth>0) and (sTextWidth<sWidth)) then
      TextSize := sTextWidth
    else
      TextSize := sWidth div 2;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    HelpContext:= iDType;
    LkDataSource:= lookds;
  end;

  if SQL<>'' then
  begin
      qryLK:= TADOQuery.Create(AOwner);
      qryLK.ConnectionString:=sConnectStr;
      qryLK.LockType:= ltReadOnly;
      qryLK.SQL.clear;
      qryLK.SQL.add(SQL);

      lookds.DataSet:= qryLK;
      qryLK.Open;
  end;

  if Default<>'' then
    Result.text:=Default;

  Result.Enabled:=not(iReadOnly=1);

  if sCaption='' then Result.Visible:=false;//2010.10.28 add

end;

function CreateJSdLookupComboSubDLL(ParamName: WideString; iPrompt, sTop, sLeft, sHeight,
      sWidth, iDType: integer; SQL: WideString; Default, sSId: WideString; AOwner: TComponent;
      AParent: TWinControl; EnterEvent: TNotifyEvent): TJSdLookupCombo;
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TJSdLookupCombo.Create(AOwner);
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
  with Result do
  begin
    hint := ParamName;
    Tag := iPrompt;
    Parent := AParent;
    SQLCmd := SQL;
    TextSize := sWidth div 3;
    Top := sTop; //2020.12.04 test Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight; //2020.12.04 test Round(sHeight*(FontSize/100));
    Width := sWidth;
    //ParentFont := true;
    Visible := not (ParamName='');
    SuperId := sSId;
    OnEnter := EnterEvent;
    HelpContext:= iDType;
  end;
  Result.Text := Default;
end;

//2018.11.30
function CreateJSdLookupComboSubDLL2(ParamName: WideString; iPrompt, sTop, sLeft, sHeight,
      sWidth, sTextWidth, iDType: integer; SQL: WideString; Default, sSId: WideString; AOwner: TComponent;
      AParent: TWinControl; EnterEvent: TNotifyEvent): TJSdLookupCombo;
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TJSdLookupCombo.Create(AOwner);
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
  with Result do
  begin
    hint := ParamName;
    Tag := iPrompt;
    Parent := AParent;
    SQLCmd := SQL;
    if ((sTextWidth>0) and (sTextWidth<sWidth)) then
      TextSize := sTextWidth
    else
      TextSize := sWidth div 3;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    //ParentFont := true;
    Visible := not (ParamName='');
    SuperId := sSId;
    OnEnter := EnterEvent;
    HelpContext:= iDType;
  end;
  Result.Text := Default;
end;

//2011.10.13 copy from unit_MIS
function CreateComboBoxDLL(ParamName: WideString; iPrompt, sTop, sLeft, sHeight,
    sWidth, iDType: integer; Default: WideString; sList: TStrings;
    AOwner: TComponent; AParent: TWinControl): TComboBox;
var sFList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TComboBox.Create(AOwner);
  //2020.03.11
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sFList:=TstringList.Create;
      sFList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sFList.Strings[0];
      sFList.Free;
      FontSize := StrToInt(sFontSize);
  end
  else
  begin
      FontSize := 100;
  end;

  with Result do
  begin
    hint := ParamName;
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop; //2020.12.04 test Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight; //2020.12.04 test Round(sHeight*(FontSize/100));
    Width := sWidth;
    Text := Default;
    Items.AddStrings(sList);
    Visible := not (ParamName='');
    HelpContext:= iDType;
  end;
end;

function CreateRadioGroupDLL(ParamName: WideString; iPrompt, sTop, sLeft, sHeight,
  sWidth, iDType: integer; Default: WideString; AOwner: TComponent; AParent: TWinControl): TRadioGroup;
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TRadioGroup.Create(AOwner);
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
  with Result do
  begin
    hint := ParamName;
    Tag := iPrompt;
    Parent := AParent;
    SendToBack;
    Columns:= 3;
    Items.Clear;
    Items.Add('0:否');
    Items.Add('1:是');
    Items.Add('不限');
    if Default<>'' then
       ItemIndex:= strtoint(Default)
    else
       ItemIndex:= 2;
    Top := sTop-8; //2020.12.04 test Round((sTop-8)*(FontSize/100));
    Left := sLeft;
    Height := sHeight+4; //2020.12.04 test Round((sHeight+4)*(FontSize/100));
    Width := sWidth;
    Caption := '';
    Visible := not (ParamName='');
  end;

  if ParamName='' then Result.Visible:=false;//2010.10.28 add
end;

procedure TevDBEdtDblClickEvent.prcDBEdtDblClick(sender: TObject);
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
    Application.CreateForm(TfrmShowDBEdit, frmShowDBEdit);
    //2020.03.10
    if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
    begin
        sList:=TstringList.Create;
        sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
        sFontSize:=sList.Strings[0];
        sList.Free;
        FontSize := StrToInt(sFontSize);
        frmShowDBEdit.Scaled:=true;
        frmShowDBEdit.ScaleBy(FontSize,100);
    end;
    with frmShowDBEdit do
    begin
      if sender is TDBEdit then
      begin
      memoField.DataSource:= TDBEdit(Sender).DataSource;
      memoField.DataField:= TDBEdit(Sender).DataField;
      end
      else  if sender is TDBMemo then
      begin
      memoField.DataSource:= TDBMemo(Sender).DataSource;
      memoField.DataField:= TDBMemo(Sender).DataField;
      end;

      Hide;
      showmodal;
    end;
end;

function CreateDBEdit2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  iIsNotesField:integer;
  sEditColor:string
  ): TDBEdit;
var evDBEdtDblClickEvent:TevDBEdtDblClickEvent;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TDBEdit.Create(AOwner);
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

  evDBEdtDblClickEvent:=TevDBEdtDblClickEvent.Create;

  with Result do
  begin
    hint := sCaption;
    ShowHint:= (sCaption<>'');//2011.9.22 add for MUT Bill-20110921-02B
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;
    Width := sWidth;
    //Text := Default;
    //EditMask := sEditMask;
    //Visible := not (sCaption='');
    HelpContext:= iDType;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly=1);
    if sEditColor<>'' then Color:=StringToColor(sEditColor);
    if iIsNotesField=1 then onDblClick:=evDBEdtDblClickEvent.prcDBEdtDblClick;
  end;
end;

//2011.10.11 add
function prcComoBoxGetItems(cbo:TDBComboBox):boolean;
var sItems,tmpStr,sRe: WideString;
    //2012.05.08 for EMO
    sName:String;
begin
  result:=false;

  sItems:='';

  if (cbo.DataSource.DataSet is TJSdTable) then
    sItems:=TJSdTable(cbo.DataSource.DataSet).ReserveList.Values[TDBComboBox(cbo).DataField+'^ITEMS'];
  //2012.05.08 for EMO
  sName:=cbo.Name;
  if pos('cbomas',Lowercase(sName))>0 then
    sItems:='';

  if sItems<>'' then
  begin

    TDBComboBox(cbo).Items.Clear;

    if pos(';',sItems)=0 then
     cbo.Items.Append(sItems)
    else
    begin
     tmpStr:=trim(sItems);

     while pos(';',tmpStr)>0 do
      begin
        sRe:='';
        sRe:=copy(tmpStr,1,pos(';',tmpStr)-1);

        if trim(sRe)<>'' then
           cbo.Items.Append(trim(sRe));

        tmpStr:=trim(copy(tmpStr,pos(';',tmpStr)+1,length(tmpStr)));
      end;

    if pos(';',tmpStr)=0 then
      cbo.Items.Append(trim(tmpStr));
    end;

  end;

  result:=true;
end;

function CreateDBComoBox2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  iIsNotesField:integer;
  sEditColor:string;
  sItems:WideString
  ): TDBComboBox;
var tmpStr,sRe: WideString;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TDBComboBox.Create(AOwner);
  //2020.08.07
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

  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100) );
    Width := sWidth;
    HelpContext:= iDType;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly=1);
    if sEditColor<>'' then Color:=StringToColor(sEditColor);
  end;

  if sItems<>'' then
  begin
    if pos(';',sItems)=0 then
     Result.Items.Append(sItems)
    else
    begin
     tmpStr:=trim(sItems);

    while pos(';',tmpStr)>0 do
      begin
        sRe:='';
        sRe:=copy(tmpStr,1,pos(';',tmpStr)-1);

        if trim(sRe)<>'' then
           Result.Items.Append(trim(sRe));

        tmpStr:=trim(copy(tmpStr,pos(';',tmpStr)+1,length(tmpStr)));
      end;

    if pos(';',tmpStr)=0 then
      Result.Items.Append(trim(tmpStr));
    end;
  end;
end;

//2012.05.08 for EMO
function CreateDBComoBox2_Mas(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  iIsNotesField:integer;
  sEditColor:string;
  sItems:WideString;
  iNameItem:Integer
  ): TDBComboBox;
var tmpStr,sRe: WideString;
    sList:TstringList; //2020.08.13
    sFontSize:string;  //2020.08.13
    FontSize: integer; //2020.08.13
begin
  Result := TDBComboBox.Create(AOwner);
  //2020.08.13
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
  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    HelpContext:= iDType;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly=1);
    Name:='cboMas_'+IntToStr(iNameItem);
    if sEditColor<>'' then Color:=StringToColor(sEditColor);
  end;
  //prcComoBoxGetItems will do again
  if sItems<>'' then
  begin
    if pos(';',sItems)=0 then
     Result.Items.Append(sItems)
    else
    begin
     tmpStr:=trim(sItems);

    while pos(';',tmpStr)>0 do
      begin
        sRe:='';
        sRe:=copy(tmpStr,1,pos(';',tmpStr)-1);

        if trim(sRe)<>'' then
           Result.Items.Append(trim(sRe));

        tmpStr:=trim(copy(tmpStr,pos(';',tmpStr)+1,length(tmpStr)));
      end;

    if pos(';',tmpStr)=0 then
      Result.Items.Append(trim(tmpStr));
    end;
  end;
end;

function CreateJSdLookupComboSubDLL_Dtl(ParamName: WideString; iPrompt,
      sTop, sLeft, sHeight,
      sWidth, iDType: integer; SQL: WideString; Default, sSId: WideString;
      AOwner: TComponent;
      AParent: TWinControl; iNameItem:Integer;
      //2012.05.09 add
      tDataSource:TDataSource;
      sDataField:WideString;
      iReadOnly:integer;
      sEditColor:string): TJSdLookupCombo;
var sList:TstringList; //2020.08.13
    sFontSize:string;  //2020.08.13
    FontSize: integer; //2020.08.13
begin
  Result := TJSdLookupCombo.Create(AOwner);
  //2020.08.13
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
  with Result do
  begin
    hint := ParamName;
    Tag := iPrompt;
    Parent := AParent;
    SQLCmd := SQL;
    TextSize := sWidth;//sWidth div 3;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    //ParentFont := true;
    //2012.08.29 Visible := not (ParamName='');
    SuperId := sSId;
    Name:='cboSub_'+IntToStr(iNameItem);
    HelpContext:= iDType;
    //Plus 2012.05.09
    Enabled:=(iReadOnly=0);
    if sEditColor<>'' then cboColor:=StringToColor(sEditColor);
  end;
  Result.DataSource:=tDataSource;
  Result.DataField:=sDataField;
  Result.TextSize := sWidth; //要放在最後才有作用
  //Result.Text := Default;
end;
//2012.05.08 for EMO end

function CreateDBMemo2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  iIsNotesField:integer;
  sEditColor:string
  ): TDBMemo;
var evDBEdtDblClickEvent:TevDBEdtDblClickEvent;
    sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TDBMemo.Create(AOwner);

  //2020.08.07
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

  evDBEdtDblClickEvent:=TevDBEdtDblClickEvent.Create;

  with Result do
  begin
    hint := sCaption;
    ShowHint:=(sCaption<>'');//2011.9.22 add for MUT Bill-20110921-02B
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    //Text := Default;
    //EditMask := sEditMask;
    //Visible := not (sCaption='');
    HelpContext:= iDType;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly=1);
    if sEditColor<>'' then Color:=StringToColor(sEditColor);
    if iIsNotesField=1 then onDblClick:=evDBEdtDblClickEvent.prcDBEdtDblClick;
  end;
end;

function CreateDBEdit(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer
  ): TDBEdit;
var evDBEdtDblClickEvent:TevDBEdtDblClickEvent;
    sList:TstringList; //2020.08.13
    sFontSize:string;  //2020.08.13
    FontSize: integer; //2020.08.13
begin
  Result := TDBEdit.Create(AOwner);
  //2020.08.13
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
  evDBEdtDblClickEvent:=TevDBEdtDblClickEvent.Create;

  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    //Text := Default;
    //EditMask := sEditMask;
    //Visible := not (sCaption='');
    HelpContext:= iDType;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly=1);
    onDblClick:=evDBEdtDblClickEvent.prcDBEdtDblClick;
  end;
end;

function CreateEditDLL(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  Default,
  sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  iReadOnly:integer
  ):
  //TEdit;
  TMaskEdit;//2011.6.4 modify for QU Johnson-20110603-1
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
 begin
//   Result := TEdit.Create(AOwner);
   Result := TMaskEdit.Create(AOwner); //2011.6.4 modify for QU Johnson-20110603-1
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
  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop; //2020.12.04 test Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight; //2020.12.04 test Round(sHeight*(FontSize/100));
    Width := sWidth;
    Text := Default;
    HelpContext:= iDType;
    ReadOnly:= iReadOnly=1;
    EditMask:=sEditMask;//2011.6.4 add for QU Johnson-20110603-1
  end;

  if iReadOnly=1 then Result.Color:=clBtnFace;

  if sCaption='' then Result.Visible:=false;//2010.10.28 add

 end;

function CreateDBCheckBox(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer
  ): TDBCheckBox;
var sList:TstringList; //2020.08.13
    sFontSize:string;  //2020.08.13
    FontSize: integer; //2020.08.13
begin
  Result := TDBCheckBox.Create(AOwner);
  //2020.08.13
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
  with Result do
  begin
    Caption:='';
    ValueChecked:='1';
    ValueUnChecked:='0';
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    HelpContext:= iDType;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly=1);
  end;
end;

function CreateDBCheckBox2(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer
  ): TDBCheckBox;
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TDBCheckBox.Create(AOwner);
  //2020.08.07
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
  with Result do
  begin
    Caption:='';
    ValueChecked:='1';
    ValueUnChecked:='0';
    hint := sCaption;
    ShowHint:=(sCaption<>'');//2011.9.22 add for MUT Bill-20110921-02B
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;//Round(sHeight*(FontSize/100));
    Width := sWidth;
    HelpContext:= iDType;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly=1);
  end;
end;

function CreateDBDateTimePicker(
  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer
  ): TwwDBDateTimePicker;
var sList:TstringList; //2020.08.13
    sFontSize:string;  //2020.08.13
    FontSize: integer; //2020.08.13
begin
  Result := TwwDBDateTimePicker.Create(AOwner);
  //2020.08.13
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
  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;
    Width := sWidth;
    //Visible := not (sCaption='');
    ImeMode := imSAlpha;
    HelpContext:= iDType;
    //DisplayFormat:= sEditMask;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly =1);
  end; {
  if Default<>'' then
  begin
    Result.Date := strtodatetime(Default);
    Result.Time := strtodatetime(Default);
  end;}
end;


function CreateDBDateTimePicker2(

  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  //Default,
  //sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString;
  iReadOnly:integer;
  sEditColor:string
  ): TwwDBDateTimePicker;
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TwwDBDateTimePicker.Create(AOwner);

  //2020.08.07
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

  with Result do
  begin
    hint := sCaption;
    ShowHint:=(sCaption<>'');//2011.9.22 add for MUT Bill-20110921-02B
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop;//Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight;
    Width := sWidth;
    //Visible := not (sCaption='');
    ImeMode := imSAlpha;
    HelpContext:= iDType;
    //DisplayFormat:= sEditMask;
    DataSource:=tDataSource;
    DataField:=sDataField;
    ReadOnly:=(iReadOnly =1);
    if sEditColor<>'' then Color:=StringToColor(sEditColor);
  end; {
  if Default<>'' then
  begin
    Result.Date := strtodatetime(Default);
    Result.Time := strtodatetime(Default);
  end;}
end;



function CreateDateTimePickerDLL(

  sCaption: WideString;
  iPrompt,
  sTop,
  sLeft,
  sHeight,
  sWidth,
  iDType: integer;
  Default,
  sEditMask: WideString;
  AOwner: TComponent;
  AParent: TWinControl;
  iReadOnly:integer
  ): TwwDBDateTimePicker;

var sList:TstringList;

    sFontSize:string;
    FontSize: integer;

begin

  Result := TwwDBDateTimePicker.Create(AOwner);
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

  with Result do
  begin
    hint := sCaption;
    Tag := iPrompt;
    Parent := AParent;
    Top := sTop; //2020.12.04 test Round(sTop*(FontSize/100));
    Left := sLeft;
    Height := sHeight; //2020.12.04 test Round(sHeight*(FontSize/100));
    Width := sWidth;
    ImeMode := imSAlpha;
    HelpContext:= iDType;
    DisplayFormat:= sEditMask;
  end;
  if Default<>'' then
  begin
    Result.Date := strtodatetime(Default);
    Result.Time := strtodatetime(Default);
  end;

  Result.ReadOnly:= iReadOnly=1;

  if iReadOnly=1 then Result.Color:=clBtnFace;

  if sCaption='' then Result.Visible:=false;//2010.10.28 add
end;


function CreateJSdLabel2(

  sCaption: WideString;
  iTop: integer;
  iLeft: integer;
  iHeight: integer;
  iWidth: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString
  ): TJSdLabel;
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TJSdLabel.Create(AOwner);
  //2020.08.07
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
  with Result do
  begin
    Tag := -1;
    Parent := AParent;
    Caption := sCaption;
    AutoSize:= false;
    Alignment:= taRightJustify;
    LayOut := tlCenter;
    Top := iTop;//Round(iTop*(FontSize/100));
    Left := iLeft;
    Height := iHeight-4;//Round((iHeight-4)*(FontSize/100));
    Width := iWidth;
    //Visible := not (sCaption='');
    DataSource:=tDataSource;
    DataField:=sDataField;
  end;
end;


function CreateJSdLabel(
  sCaption: WideString;
  iTop: integer;
  iLeft: integer;
  iHeight: integer;
  iWidth: integer;
  AOwner: TComponent;
  AParent: TWinControl;
  tDataSource:TDataSource;
  sDataField:WideString
  ): TJSdLabel;
var sList:TstringList; //2020.08.13
    sFontSize:string;  //2020.08.13
    FontSize: integer; //2020.08.13
begin
  Result := TJSdLabel.Create(AOwner);
  //2020.08.13
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
  with Result do
  begin
    Tag := -1;
    Parent := AParent;
    Caption := sCaption;
    AutoSize:= false;
    Alignment:= taRightJustify;
    LayOut := tlCenter;
    Top := iTop;//Round(iTop*(FontSize/100));
    Left := iLeft;
    Height := (iHeight-4);//Round((iHeight-4)*(FontSize/100));
    Width := iWidth;
    //Visible := not (sCaption='');
    DataSource:=tDataSource;
    DataField:=sDataField;
  end;
end;

function CreateLabelDLL(
  sCaption: WideString;
  iTop: integer;
  iLeft: integer;
  iHeight: integer;
  iWidth: integer;
  AOwner: TComponent;
  AParent: TWinControl
  ): TLabel;
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  Result := TJSdLabel.Create(AOwner);
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
  with Result do
  begin
    Tag := -1;
    Parent := AParent;
    Caption := sCaption;
    AutoSize:= false;
    Alignment:= taRightJustify;
    LayOut := tlCenter;
    Top := iTop; //2020.12.04 test Round(iTop*(FontSize/100));
    Left := iLeft;
    Height := iHeight-4; //2020.12.04 test Round((iHeight-4)*(FontSize/100));
    Width := iWidth;
  end;

end;


function funDLLOpSetUp(
  afm:TForm;
  qry:TADOQuery;
  sItemId:string;
  sDLLName:string;
  sClassName:string;
  sUserId:string;
  sBUID:string;
  sGlobalId:string;
  sConnectStr:string;
  sUseId:string;
  var sTableNameMas1:string;
  var sTableNameDtl1:string;
  var sSelectSQLMas1:string;
  var sSelectSQLDtl1:string;
  var iFixColCntMas1:integer;
  var iFixColCntDtl1:integer;
  var sMDKeyMas1:string;
  var sMDKeyDtl1:string;
  gridMas1:TJSdDBGrid;
  gridDtl1:TJSdDBGrid;
  tblMas1:TJSdTable;
  tblDtl1:TJSdTable;
  var sRealTableNameMas1:string;
  var sRealTableNameDtl1:string;
  //2
   //--Item
  iOpKind:integer; //0 一般 1單據
  var PaperType:integer;
  var CurrTypeHead: WideString;
  var PowerType: integer;
  var FunctionType: integer;
    //--User Power
  var CanbUpdate	:Integer;
  var CanbUpdateMoney	:Integer;
  var CanbAudit	:Integer;
  var CanbAuditBack	:Integer;
  var CanbScrap	:Integer;
  var CanbViewMoney	:Integer;
  var CanbPrint :Integer;
  var CanbUpdateNotes:Integer;
  var CanbRunFLow	:Integer;
  var CanbSelectType :Integer;
  var CanbLockPaperDate :Integer;
  var CanbLockUserEdit :Integer;
  var CanbMustNotes :Integer;
  var sDDCaptionMas1:string;
  var sDDCaptionDtl1:string;

  var CanbExport:Integer;//2015.11.17 add for Bill-20151112-03
  var CanbF9	  :Integer;//2015.11.17 add for Bill-20151112-03
  var CanbF12	  :Integer//2015.11.17 add for Bill-20151112-03
  ):boolean;
var sSQL:string; i,j:integer; OCXsysButton:POCXsysButton; sysbtnList:TList;
 CustBtnList:TList;  OCXcustButton:POCXcustButton;
 iCompelAftPost:integer;
 bActive:boolean;
 iDLLPaperMasShowSave:integer;//2012.04.30 add for Bill-20120426-01

 iHideCustBtn:integer; //2016.08.17 add

begin
//iOpKind: 0 SingleGrid,1 PaperOrg, 2 MasDtl, 3 Grid

  //若是單據則先執行 Master1 ,取得 sRealTableNameMas1
  if iOpKind =1 then
    if funGetFormInfo(
      qry,
      //afm.Name,
      sItemId,
      'Master1',
      sTableNameMas1,
      iFixColCntMas1,
      sMDKeyMas1,
      sRealTableNameMas1,
      1, //iMust
      sDDCaptionMas1,
      tblMas1.ReserveList.Values['LanguageId']
      )=false then begin //showmessage('執行funGetFormInfo失敗');
                    result:=false; exit; end;

if iOpKind in[0,1,2] then
 if DLLSetPowerType(
  iOpKind,//:integer;
  afm,//:TForm;
  qry,//:TADOQuery;
  sItemId,//sItemId:string;
  sUserId,//sUserId:string;
  sUseId,//sUseId:string;
  sRealTableNameMas1,//sRealTableNameMas1:string;
   //==Item setup
  PaperType,//var PaperType :integer;
  CurrTypeHead,//var CurrTypeHead: WideString;
  PowerType,//var PowerType: integer;
  FunctionType,//var FunctionType: integer;
    //==User Power
  CanbUpdate,//var CanbUpdate	:Integer;
  CanbUpdateMoney,//var CanbUpdateMoney	:Integer;
  CanbAudit,//var CanbAudit	:Integer;
  CanbAuditBack,//var CanbAuditBack	:Integer;
  CanbScrap,//var CanbScrap	:Integer;
  CanbViewMoney,//var CanbViewMoney	:Integer;
  CanbPrint,//var CanbPrint	:Integer;
  CanbUpdateNotes,//var CanbUpdateNotes :Integer;
    //==Paper Power
  CanbRunFLow,//var CanbRunFLow	:Integer;
  CanbSelectType,//var CanbSelectType :Integer;
  CanbLockPaperDate,//var CanbLockPaperDate :Integer;
  CanbLockUserEdit,//var CanbLockUserEdit :Integer;
  CanbMustNotes, //var CanbMustNotes :Integer

  CanbExport,//2015.11.17 add for Bill-20151112-03
  CanbF9	  ,//2015.11.17 add for Bill-20151112-03
  CanbF12	   //2015.11.17 add for Bill-20151112-03
  )=false
  then
  begin
    //showmessage('執行DLLSetPowerType失敗');
    result:=false;
    exit;
  end;

//iOpKind: 0 SingleGrid,1 PaperOrg, 2 MasDtl, 3 Grid

  if iOpKind in[0,1,3] then iCompelAftPost:=1 else iCompelAftPost:=0;

  if iOpKind in[0,1,2,3] then
    begin
      if funGetTableSetUp(
        afm,
        qry,
        sItemId,
        'Master1',
        sTableNameMas1,
        iFixColCntMas1,
        sMDKeyMas1,
        sSelectSQLMas1,
        gridMas1,
        tblMas1,
        sRealTableNameMas1,
        iOpKind,
        1, //iMust
        sDDCaptionMas1,
        iCompelAftPost
        )=false then begin //showmessage('執行funGetTableSetUp失敗');
                      result:=false; exit; end;
    end;

//iOpKind: 0 SingleGrid,1 PaperOrg, 2 MasDtl, 3 Grid

  if iOpKind in[1,2] then
    begin
      if funGetTableSetUp(
        afm,
        qry,
        sItemId,
        'Detail1',
        sTableNameDtl1,
        iFixColCntDtl1,
        sMDKeyDtl1,
        sSelectSQLDtl1,
        gridDtl1,
        tblDtl1,
        sRealTableNameDtl1,
        iOpKind,
        1, //iMust
        sDDCaptionDtl1,
        iCompelAftPost
        )=false then begin //showmessage('執行funGetTableSetUp 2失敗');
                      result:=false; exit; end;

      //2013.10.23 for EMO
      if iOpKind=1 then
      begin
          if ((TJSdTable(afm.FindComponent('qryBrowse')).TableName='EMOdProdInfo')
            or (TJSdTable(afm.FindComponent('qryBrowse')).TableName='EMOdProdLayer')) then
          begin
            if funDrawPaperPNL_EMO(
              afm,
              TJSdTable(afm.FindComponent('qryBrowse')),
              qry,
              iOpKind,
              CanbLockPaperDate,
              CanbViewMoney
              )=false
            then
            begin
              //showmessage('執行funDrawPaperPNL2失敗');
              result:=false; exit;
            end;
          end
          else
          begin
            //Ori
            if funDrawPaperPNL2(
              afm,
              TJSdTable(afm.FindComponent('qryBrowse')),
              qry,
              iOpKind,
              CanbLockPaperDate,
              CanbViewMoney
              )=false
            then
            begin
              //showmessage('執行funDrawPaperPNL2失敗');
              result:=false; exit;
            end;
          end;
      end;
    end;//if iOpKind in[1,2] then

  //System Button
  sSQL:='exec CURdOCXSysButtonGet '+''''+sItemId+''''+','+
    ''''+tblMas1.ReserveList.Values['LanguageId']+''''
    +','+inttostr(iOpKind) //2012.04.30 add for Bill-20120426-01
    ;
  with qry do
      begin
        if active then close;
        sql.Clear;
        sql.Add(sSQL);
        open;
      end;

iDLLPaperMasShowSave:=0;//2012.04.30 add for Bill-20120426-01

if qry.RecordCount>0 then
begin
  sysbtnList:=TList.Create;
  with qry do
    begin
          first;

          iDLLPaperMasShowSave:=fieldbyname('iDLLPaperMasShowSave').AsInteger;//2012.04.30 add for Bill-20120426-01

          while not eof do
            begin
              new(OCXsysButton);
              OCXsysButton^.ButtonName:=fieldbyname('ButtonName').AsString;
              OCXsysButton^.CustCaption:=fieldbyname('langCustCaption').AswideString;
              OCXsysButton^.CustHint:=fieldbyname('langCustHint').AswideString;
              OCXsysButton^.bVisiable:=fieldbyname('bVisiable').AsInteger;//2012.04.30 add for Bill-20120426-01
              OCXsysButton^.SerialNum:=fieldbyname('SerialNum').AsInteger;//2017.02.09 add
              OCXsysButton^.InPanelSeq:=fieldbyname('InPanelSeq').AsInteger;//2017.02.09 add
              sysbtnList.Add(OCXsysButton);
              Next;
            end;
    end;

  for i := 0 to afm.ComponentCount - 1 do
    if afm.Components[i] is TSpeedButton then
       begin
         if TSpeedButton(afm.Components[i]).Name<>'btn_PaperOrgTopExToolClose' then //2019.08.27 Fix Access Violation
         begin
           if qry.Locate('ButtonName',TSpeedButton(afm.Components[i]).Name,[loCaseInsensitive]) then
              begin
                for j := 0 to sysbtnList.Count - 1 do
                   if TOCXsysButton(sysbtnList[j]^).ButtonName=TSpeedButton(afm.Components[i]).Name then
                    begin

                      if iOpKind<>1 then //2012.04.30 add 'if...' for Bill-20120426-01
                      begin
                        //非單據
                        TSpeedButton(afm.Components[i]).Visible:=true;
                      end
                      else
                      begin
                        //單據
                        //----------2012.04.30 disable for Bill-20120426-01
                        TSpeedButton(afm.Components[i]).Tag:=TOCXsysButton(sysbtnList[j]^).bVisiable;
                        TSpeedButton(afm.Components[i]).Enabled:=(TOCXsysButton(sysbtnList[j]^).bVisiable=1);
                        //----------

                        //----------2017.02.09 add
                        if TOCXsysButton(sysbtnList[j]^).InPanelSeq=0 then
                           begin
                             TSpeedButton(afm.Components[i]).Align:=alRight;
                           end
                        else
                          begin
                             TSpeedButton(afm.Components[i]).Align:=alBottom;
                          end;
                        //----------
                      end;

                      TSpeedButton(afm.Components[i]).Caption:=TOCXsysButton(sysbtnList[j]^).CustCaption;
                      TSpeedButton(afm.Components[i]).Hint:=TOCXsysButton(sysbtnList[j]^).CustHint;
                      TSpeedButton(afm.Components[i]).ShowHint:=true;
                    end;
              end;

            if iOpKind=1 then  //單據
              begin
                { 2012.04.30 disable for Bill-20120426-01

                if TSpeedButton(afm.Components[i]).Name='btnExam' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbAudit=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnRejExam' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbAuditBack=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnAdd' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbUpdate=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnUpdate' then
                begin
                  if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbUpdate=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnVoid' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbScrap=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnUpdateMoney' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbViewMoney=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnUpdateNotes' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbUpdateNotes=1;
                end else if TSpeedButton(afm.Components[i]).Name='btnPrintPaper' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbPrint=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnPrintList' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbPrint=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnReGetNum' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbUpdate=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnKeepStatus' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbUpdate=1;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnCompleted' then
                begin
                   if TSpeedButton(afm.Components[i]).Visible then
                      TSpeedButton(afm.Components[i]).Visible:=CanbUpdate=1;
                end;
                }
                //2012.04.30 modify for Bill-20120426-01
                if TSpeedButton(afm.Components[i]).Name='btnExam' then
                begin
                   if CanbAudit=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnRejExam' then
                begin
                   if CanbAuditBack=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnAdd' then
                begin
                   if CanbUpdate=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                //2015.11.18移到下面
                {else if TSpeedButton(afm.Components[i]).Name='btnUpdate' then
                begin
                  if CanbUpdate=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end]}
                else if TSpeedButton(afm.Components[i]).Name='btnVoid' then
                begin
                   if CanbScrap=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnUpdateMoney' then
                begin
                   if CanbViewMoney=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnUpdateNotes' then
                begin
                   if CanbUpdateNotes=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end else if TSpeedButton(afm.Components[i]).Name='btnPrintPaper' then
                begin
                   if CanbPrint=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnPrintList' then
                begin
                   if CanbPrint=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnReGetNum' then
                begin
                   if CanbUpdate=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnKeepStatus' then
                begin
                   if CanbUpdate=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
                else if TSpeedButton(afm.Components[i]).Name='btnCompleted' then
                begin
                   if CanbUpdate=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end

                //-----------2012.04.30 modify for Bill-20120426-01
                else if TSpeedButton(afm.Components[i]).Name='btnPaperOrgDLLSaveMas' then
                begin
                  TSpeedButton(afm.Components[i]).Visible:=iDLLPaperMasShowSave=1;
                end
                else
                if TSpeedButton(afm.Components[i]).Name='btnPaperOrgDLLCancelMas' then
                begin
                  TSpeedButton(afm.Components[i]).Visible:=iDLLPaperMasShowSave=1;
                end;
                //-----------

              end;//if iOpKind=1 then


                //-----------2015.11.18 add for Bill-20151112-03
              if TSpeedButton(afm.Components[i]).Name='btnToExcel' then
                begin
                   if CanbExport=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end
              else if TSpeedButton(afm.Components[i]).Name='btnUpdate' then
                begin
                   if CanbUpdate=0 then
                   begin
                      TSpeedButton(afm.Components[i]).Enabled:=false;
                      TSpeedButton(afm.Components[i]).Tag:=0;
                   end;
                end;
                //-----------

         end;
      end;//if afm.Components[i] is TSpeedButton then

//2017.02.09 add
if iOpKind=1 then  //單據
  begin
  qry.First;
  while not qry.Eof do
    begin
      for i := 0 to afm.ComponentCount - 1 do
        if afm.Components[i] is TSpeedButton then
          begin
             if qry.FieldByName('ButtonName').AsString=TSpeedButton(afm.Components[i]).Name then
                begin
                    if qry.FieldByName('InPanelSeq').asInteger=0 then
                         begin
                           TSpeedButton(afm.Components[i]).Align:=alLeft;
                         end
                      else
                        begin
                           TSpeedButton(afm.Components[i]).Align:=alTop;
                        end;
                end;
          end;

      qry.Next;
    end;
  end; //if iOpKind=1 then

  //if iOpKind=1 then
    if not(CanbUpdate=1) then
      for i := 0 to afm.ComponentCount - 1 do
        if afm.Components[i] is TJSdTable then
          begin
             bActive:=TJSdTable(afm.Components[i]).active;
             if bActive then TJSdTable(afm.Components[i]).close;
             TJSdTable(afm.Components[i]).LockType:=ltReadOnly;
             if bActive then TJSdTable(afm.Components[i]).Open;
          end;

  qry.Close;
  sysbtnList.Free;
end;

  //Custom Button
  sSQL:='exec CURdOCXItemCustButtonGet '+''''+sItemId+''''+','+
    ''''+tblMas1.ReserveList.Values['LanguageId']+'''';
  with qry do
      begin
        if active then close;
        sql.Clear;
        sql.Add(sSQL);
        open;
      end;

if qry.RecordCount>0 then
begin
  //排順序 2010.2.9 add
  if afm.FindComponent('pnlTempBasDLLBottom')<>nil then
    if afm.FindComponent('pnlTempBasDLLBottom') is TPanel then
      for i := 0 to TPanel(afm.FindComponent('pnlTempBasDLLBottom')).ControlCount - 1 do
        if TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i] is TSpeedButton then
          if ((TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Name<>'btnSaveHeight')
              and (TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Name<>'btnExcelDtl')) then
             TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Align:=alRight;

  CustBtnList:=TList.Create;
  with qry do
    begin
          first;
          while not eof do
            begin

              iHideCustBtn:=0; //2016.08.17 add

              //2016.08.17 add ,寫成相容模式
              if qry.FindField('iOnlyForAdmin')<>nil then
                begin
                  if qry.FieldByName('iOnlyForAdmin').AsInteger=1 then
                    begin
                      if sUserId<>'Admin' then
                         begin
                           iHideCustBtn:=1;
                         end;
                    end;
                end;

              if iHideCustBtn=0 then //2016.08.17 add 'if...'
                begin

              new(OCXcustButton);
              OCXcustButton^.ButtonName:=fieldbyname('ButtonName').AsString;
              OCXcustButton^.CustCaption:=fieldbyname('langCustCaption').AswideString;
              OCXcustButton^.CustHint:=fieldbyname('langCustHint').AswideString;
              OCXcustButton^.OCXName:=fieldbyname('OCXName').AsString;
              OCXcustButton^.CoClassName:=fieldbyname('CoClassName').AsString;
              OCXcustButton^.ChkCanbUpdate:=fieldbyname('ChkCanbUpdate').Asinteger;
              OCXcustButton^.ChkStatus:=fieldbyname('ChkStatus').Asinteger;
              OCXcustButton^.bNeedNum:=fieldbyname('bNeedNum').Asinteger;
              CustBtnList.Add(OCXcustButton);
                end;



              Next;
            end;
    end;

  {for i := 0 to afm.ComponentCount - 1 do
    if afm.Components[i] is TSpeedButton then
       begin
         if qry.Locate('ButtonName',TSpeedButton(afm.Components[i]).Name,[loCaseInsensitive]) then
            begin
              for j := 0 to CustBtnList.Count - 1 do
                 if TOCXcustButton(CustBtnList[j]^).ButtonName=TSpeedButton(afm.Components[i]).Name then
                  begin
                    TSpeedButton(afm.Components[i]).Visible:=true;
                    TSpeedButton(afm.Components[i]).Caption:=TOCXcustButton(CustBtnList[j]^).CustCaption;
                    TSpeedButton(afm.Components[i]).Hint:=TOCXcustButton(CustBtnList[j]^).CustHint;
                    TSpeedButton(afm.Components[i]).ShowHint:=true;
                    //TSpeedButton(afm.Components[i]).Align:=alLeft;//排順序 2010.2.9 add
                  end;
            end;
      end;}

  if afm.FindComponent('pnlTempBasDLLBottom')<>nil then
    if afm.FindComponent('pnlTempBasDLLBottom') is TPanel then
    begin
      qry.First;
      while not qry.Eof do
        begin
          for i := 0 to TPanel(afm.FindComponent('pnlTempBasDLLBottom')).ControlCount - 1 do
            if TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i] is TSpeedButton then
              if (TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Name=
                  qry.FieldByName('ButtonName').AsString) then

                for j := 0 to CustBtnList.Count - 1 do
                 if TOCXcustButton(CustBtnList[j]^).ButtonName=
                    TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Name then
                  begin
                    TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Visible:=true;
                    TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Caption:=TOCXcustButton(CustBtnList[j]^).CustCaption;
                    TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Hint:=TOCXcustButton(CustBtnList[j]^).CustHint;
                    TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).ShowHint:=true;
                    TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Align:=alLeft;

                    //2020.10.22
                    if (qry.fieldbyname('InFlow').Asinteger=1) then
                          TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Visible:=false;

                  end;//if TOCXcustButton(CustBtnList[j]^).ButtonName=

          qry.Next;
        end; //while not qry.Eof do
      end; //if afm.FindComponent('pnlTempBasDLLBottom') is TPanel then


{  //排順序 2010.2.9 add
  if afm.FindComponent('pnlTempBasDLLBottom')<>nil then
    if afm.FindComponent('pnlTempBasDLLBottom') is TPanel then
    begin
      qry.First;
      while not qry.Eof do
        begin
          for i := 0 to TPanel(afm.FindComponent('pnlTempBasDLLBottom')).ControlCount - 1 do
            if TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i] is TSpeedButton then
              if (TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Name=
                  qry.FieldByName('ButtonName').AsString) and
                 (TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Visible)
               then
                 TSpeedButton(TPanel(afm.FindComponent('pnlTempBasDLLBottom')).Controls[i]).Align:=alLeft;

          qry.Next;
        end;
    end;
}

 {
type POCXcustButton = ^TOCXcustButton;
     TOCXcustButton = record
       ButtonName:string;
       CustCaption:string;
       CustHint:string;
       OCXName:string;
       CoClassName:string;
       ChkCanbUpdate:integer;
       ChkStatus:integer;
       bNeedNum:integer;
  end;
}

  CustBtnList.Free;
end;//if qry.RecordCount>0 then
  qry.Close;

  result:=true;
end;

function funGetTableSetUp(
  afm:TForm;
  qry:TADOQuery;
  sItemId:string;
  sTableKind:string;
  var sTableName:string;
  var iFixColCount:integer;
  var sMDKey:string;
  var sSelectSQL:string;
  grid:TJSdDBGrid;
  tbl:TJSdTable;
  var sRealTableName:string;
  iOpKind:integer;
  iMust:integer;
  var sDDCaption:string;
  iCompelAftPost:integer
  ):boolean;
var sUnOpenWhenFormShow:string;
    sOpenNoRecord:string;//2011.7.8 add for Johnson-20110704-1
    sIIFNoRecord:string;//2011.7.8 add for Johnson-20110704-1
    sOpenByDefaultCondSQL:string;//2011.10.25 add for Johnson-20111025-01
    //sWhere_and:string;//2013.02.28 disble //2013.02.28 add for Upgrade to SQL2012
begin
  sTableName:='';
  iFixColCount:=0;
  sMDKey:='';
  sRealTableName:='';
  sUnOpenWhenFormShow:='';

  if funGetFormInfo(
    qry,
    //afm.Name,
    sItemId,
    sTableKind,
    sTableName,
    iFixColCount,
    sMDKey,
    sRealTableName,
    iMust,
    sDDCaption,
    tbl.ReserveList.Values['LanguageId']
    )=false then begin result:=false; exit; end;

  if ((sTableName='') and (iMust=0)) then begin result:=true; exit; end;

  sSelectSQL:='';

  if funGetSelectSQL(
    qry,
    afm.Name,
    sTableName,
    sSelectSQL
    )=false then begin result:=false; exit; end;

  if (grid<>nil) and (iFixColCount>0) then grid.FixedCols:=iFixColCount;

  if sTableName<>'' then
    begin
      with tbl do
        begin
          if active then close;
            sql.Clear;

            //iOpKind: 0 SingleGrid,1 PaperOrg, 2 MasDtl, 3 Grid

            sOpenByDefaultCondSQL:='';//2011.10.25 add
            sIIFNoRecord:='';//2011.10.25 add

            //----------2011.10.25 add for Johnson-20111025-01
            if tbl.ReserveList.Values['OpenByDefaultCondSQL']<>'' then
              sOpenByDefaultCondSQL:=tbl.ReserveList.Values['OpenByDefaultCondSQL'];

            if sOpenByDefaultCondSQL<>'' then
              begin
                funReplaceCom(sOpenByDefaultCondSQL,tbl);

                sIIFNoRecord:=' '+sOpenByDefaultCondSQL;
              end;
            //----------

            //----------2011.7.8 add for Johnson-20110704-1
            if sIIFNoRecord='' then //2011.10.25 add 'if...'
              begin
                 if tbl.ReserveList.Values['OpenNoRecord']='1' then
                  begin
                    sIIFNoRecord:=' and 1=2 ';
                  end;
              end;
            //----------

            //2013.02.28 disble
            //----------2013.02.28 add for Upgrade to SQL2012
            //sWhere_and:=' and ';

            //if tbl.ReserveList.Values['sSQL2012']='1' then
            //      begin
            //        sWhere_and:=' where ';
            //      end;
            //----------

            if iOpKind in[0,3] then // SingleGrid、GridDLL
               sql.Add(sSelectSQL+' '+funReplaceCom(funGetFilterSQL(qry,sItemId,sTableKind),tbl)+
                          sIIFNoRecord+ //add for Johnson-20110704-1
                          ' '+funGetOrderByField(qry,sItemId,sTableKind)
                    )
            else
            if iOpKind=1 then // PaperOrg
              begin
                if sTableKind='Master1' then
                    sql.Add(sSelectSQL+' and 1=2 ') // Filter及OrderBy 都在查詢時做
                else
                    sql.Add(sSelectSQL+
                              ' and '
                              //sWhere_and //2013.02.28 disble //2013.02.28 modify for Upgrade to SQL2012
                              +
                              funGetMDKeySQL(sMDKey)+' '+ // 't0.'+sMDKey+' = :'+sMDKey+' '+
                              funReplaceCom(funGetFilterSQL(qry,sItemId,sTableKind),tbl)+' '+
                              funGetOrderByField(qry,sItemId,sTableKind)
                              );
              end
            else
            if iOpKind=2 then  // MasDtl
              begin
                if sTableKind='Master1' then
                   sql.Add(sSelectSQL+' '+funReplaceCom(funGetFilterSQL(qry,sItemId,sTableKind),tbl)+
                        sIIFNoRecord+ //add for Johnson-20110704-1
                        ' '+ funGetOrderByField(qry,sItemId,sTableKind))
                else
                   sql.Add(sSelectSQL+
                              ' and '
                              //sWhere_and //2013.02.28 disble //2013.02.28 modify for Upgrade to SQL2012
                              +
                              funGetMDKeySQL(sMDKey)+' '+ // 't0.'+sMDKey+' = :'+sMDKey+' '+
                              funReplaceCom(funGetFilterSQL(qry,sItemId,sTableKind),tbl)+
                              ' '+funGetOrderByField(qry,sItemId,sTableKind)
                              );
              end;

            TableName:=sTableName;
            IndexFieldNames:=funGetLocateKeys(qry,sItemId,sTableKind);
            funDoTableEvent(qry,TDataSet(tbl),iCompelAftPost);

            //ShowMessage(TJSdTable(tbl).SQL.Text);

            if sIIFNoRecord='' then //2011.10.25 add 'if...'
              sUnOpenWhenFormShow:=tbl.ReserveList.Values['UnOpenWhenFormShow'];

            if sUnOpenWhenFormShow<>'1' then
              begin
                if not(LockType=ltReadOnly) then funDoFieldValidate(qry,tbl);

                if active=false then Open;
              end;
        end;
      //if not(tbl.LockType=ltReadOnly) then funDoFieldValidate(qry,tbl);
    end;
  result:=true;
end;

function funReplaceCom(sStr:string;tbl:TJSdTable):string;
var sUseId,sUserId,sGUID,sCompanyUseId:string;
begin
  sUseId:=tbl.ReserveList.Values['UseId'];
  sUserId:=tbl.ReserveList.Values['UserId'];
  sGUID:=tbl.ReserveList.Values['sGUID'];
  sCompanyUseId:=tbl.ReserveList.Values['CompanyUseId'];

  if (pos('@UseId',sStr)>0) and (sUseId<>'') then
    sStr:=AnsiReplaceText(sStr,'@UseId',''''+sUseId+'''');

  if (pos('@UserId',sStr)>0) and (sUserId<>'') then
    sStr:=AnsiReplaceText(sStr,'@UserId',''''+sUserId+'''');

  if (pos('@sGUID',sStr)>0) and (sGUID<>'') then
    sStr:=AnsiReplaceText(sStr,'@sGUID',''''+sGUID+'''');

  if (pos('@CompanyUseId',sStr)>0) and (sCompanyUseId<>'') then
    sStr:=AnsiReplaceText(sStr,'@CompanyUseId',''''+sCompanyUseId+'''');

  result:=sStr;
end;

function funGetMDKeySQL(sMDKey:string):string;
var sMDSQL,sTempStr,sKey:string; i:integer;
begin
  sTempStr:=sMDKey;

  i:=pos(';',sTempStr);

  while i>0 do
      begin
        sKey:='';
        i:=pos(';',sTempStr);

        if i>0 then
          begin
            sKey:=copy(sTempStr,1,i-1);
            sTempStr:=copy(sTempStr,i+1,length(sTempStr));

            if sMDSQL<>'' then sMDSQL:=sMDSQL+' and ';
            sMDSQL:=sMDSQL+ 't0.'+sKey+' = :'+sKey;
          end;
      end;

   if sMDSQL<>'' then sMDSQL:=sMDSQL+' and ';
   sMDSQL:=sMDSQL+ 't0.'+sTempStr+' = :'+sTempStr;

  result:=sMDSQL;
end;

function funGetInqInSubSQL(sMDKey,AliaName:string):string;
var InSubSQL,sTempStr,sKey:string; i:integer;
begin
  sTempStr:=sMDKey;

  i:=pos(';',sTempStr);

  while i>0 do
      begin
        sKey:='';
        i:=pos(';',sTempStr);

        if i>0 then
          begin
            sKey:=copy(sTempStr,1,i-1);
            sTempStr:=copy(sTempStr,i+1,length(sTempStr));

            if InSubSQL<>'' then InSubSQL:=InSubSQL+'+';
            InSubSQL:=InSubSQL+ 'convert(varchar(255),'+AliaName+sKey+')';
          end;
      end;

   if InSubSQL<>'' then InSubSQL:=InSubSQL+'+';
   InSubSQL:=InSubSQL+ 'convert(varchar(255),'+AliaName+sTempStr+')';

  result:=InSubSQL;
end;

procedure TevTableEvent.prcTblEventAfterClose(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'AfterClose')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;


procedure TevTableEvent.prcTblEventAfterEdit(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'AfterEdit')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventAfterInsert(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'AfterInsert')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventAfterOpen(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'AfterOpen')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventAfterPost(dataset: TDataSet);
begin
  //系統控制
  funDoAfterPost(TJSdTable(dataset));

  //自訂
  if funTableEventImpl(dataset,'AfterPost')='ABORT' then abort;

  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

//保留
procedure TevTableEvent.prcTblEventBeforeDelete(dataset: TDataSet);
begin
  //系統控制
  funDoBeforeDelete(TJSdTable(dataset)); //2011.10.13 add

  if funTableEventImpl(dataset,'BeforeDelete')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

//保留
procedure TevTableEvent.prcTblEventAfterDelete(dataset: TDataSet);
begin
  //系統控制
  funDoAfterDelete(TJSdTable(dataset));//2011.10.13 add

  if funTableEventImpl(dataset,'AfterDelete')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;


procedure TevTableEvent.prcTblEventAfterPost2(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'AfterPost')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventAfterRefresh(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'AfterRefresh')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventAfterScroll(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'AfterScroll')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventBeforeCancel(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'BeforeCancel')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventBeforeClose(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'Beforelose')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

//2011.10.13 add
procedure TevTableEvent.prcTblEventBeforeDelete2;
begin
  if funTableEventImpl(dataset,'BeforeDelete')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

//2011.10.13 add
procedure TevTableEvent.prcTblEventAfterDelete2(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'AfterDelete')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;


procedure TevTableEvent.prcTblEventBeforeEdit;
begin
  if funTableEventImpl(dataset,'BeforeEdit')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventBeforeInsert;
begin
  if funTableEventImpl(dataset,'BeforeInsert')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventBeforeOpen(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'BeforeOpen')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventBeforePost(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'BeforePost')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventBeforeRefresh(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'BeforeRefresh')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);
end;

procedure TevTableEvent.prcTblEventBeforeScroll(dataset: TDataSet);
begin
  if funTableEventImpl(dataset,'BeforeScroll')='ABORT' then abort;
  if assigned(oldTableEvent) then oldTableEvent(Dataset);

end;

function funIFStringStr(bYes:Boolean; str1, str2: String): String;
begin
  if bYes then
    Result := (str1)
  else
    Result := (str2);
end;

function funIFFieldFind(tTable:TJSdtable;sFieldName:String; str1: String): String;
var sUseId,sUserId,sGUID,sCompanyUseId:string;
begin

if (pos('@',str1)>0) then
begin
  sUseId:=tTable.ReserveList.Values['UseId'];
  sUserId:=tTable.ReserveList.Values['UserId'];
  sGUID:=tTable.ReserveList.Values['sGUID'];
  sCompanyUseId:=tTable.ReserveList.Values['CompanyUseId'];

  if sFieldName='@UseId' then
    result:=sUseId
  else if sFieldName='@UserId' then
    result:=sUserId
  else if sFieldName='@sGUID' then
    result:=sGUID
  else if sFieldName='@CompanyUseId' then
    result:=sCompanyUseId
  else result:=str1;

  exit;
end;

  if tTable.FindField(sFieldName)=nil then
    begin
      result:=str1;
      exit;
    end
    else
    begin
      result:=tTable.FindField(sFieldName).AsString;
    end

end;

function funTableEventByParams( //停用,要保留
  CanbLockUserEdit:integer;
  sUserId:string;
  sPaperUserId:string;
  dataset:TDataSet;
  sEventKind:string
  ):boolean;
begin
  //自訂
  if unit_DLL.funTableEventImpl(dataset,sEventKind)='ABORT' then
    begin
      result:=false;
      exit;
    end;

  if CanbLockUserEdit=1 then
    if (trim(sUserId)<>trim(sPaperUserId))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
       begin
         MsgDlgJS('已被設定「只有建檔者可編輯」',mtWarning,[mbOk],0);
         result:=false;
         exit;
       end;

  result:=true;
end;

function funCheckSameUser(
  CanbLockUserEdit:integer;
  //sUserId:string; //2012.09.19 disable for debug
  sPaperUserId:string;
  sUserId:string //2012.09.19 add for debug
  ):boolean;
begin
  if CanbLockUserEdit=1 then
    if (trim(sUserId)<>trim(sPaperUserId))
            and (LowerCase(trim(sUserId))<>'admin')//2012.09.19 add for MUT Bill-20120912-01
        then
       begin
         MsgDlgJS('已被設定「只有建檔者可編輯」',mtWarning,[mbOk],0);
         result:=false;
         exit;
       end;

  result:=true;
end;


function funTableEventImpl(tTable:TDataSet;sEvenName:string):string;
var sSQL:string; qry:TADOQuery;
    bOCXTableEventType: integer;
    OCXTableEventMsg: string;
    OCXTableEventFd1: string;
    OCXTableEventFd2: string;
    OCXTableEventFd3: string;
    OCXTableEventFd4: string;
    OCXTableEventFd5: string;
    OCXTableEventFd6: string;
    OCXTableEventUd: string;
    //sReValue: string;
    sReMsg,sHead,sBody: String;
begin
  sSQL:='exec CURdOCXTableEventSetUpGet '+
    ''''+TJsdTable(tTable).TableName+''''+','+
    ''''+sEvenName+'''';

 qry:=TADOQuery.Create(nil);
 qry.ConnectionString:=TJsdTable(tTable).ConnectionString;
 qry.SQL.Add(sSQL);
 qry.Open;

 if qry.RecordCount=0 then
    begin
      qry.Close;
      qry.Free;
      result:='OK';
      exit;
    end
    else
    begin
      bOCXTableEventType:=qry.FieldByName('bOCXTableEventType').AsInteger;
      OCXTableEventMsg:=qry.FieldByName('OCXTableEventMsg').AsString;
      OCXTableEventFd1:=qry.FieldByName('OCXTableEventFd1').AsString;
      OCXTableEventFd2:=qry.FieldByName('OCXTableEventFd2').AsString;
      OCXTableEventFd3:=qry.FieldByName('OCXTableEventFd3').AsString;
      OCXTableEventFd4:=qry.FieldByName('OCXTableEventFd4').AsString;
      OCXTableEventFd5:=qry.FieldByName('OCXTableEventFd5').AsString;
      OCXTableEventFd6:=qry.FieldByName('OCXTableEventFd6').AsString;
      OCXTableEventUd:=qry.FieldByName('OCXTableEventUd').AsString;
      qry.Close;

      if bOCXTableEventType in[0,1] then qry.Free;
      //if bOCXTableEventType in[0,1,4] then qry.Free; ////only for Test
      case bOCXTableEventType of
        0:begin
            if OCXTableEventMsg<>'' then MsgDlgJS(OCXTableEventMsg,mtError,[mbOK],0);
            result:='ABORT';
            exit;
          end;
        1:begin
            if MsgDlgJS(OCXTableEventMsg,mtConfirmation,[mbYes,mbNo],0)<>mrYes then //2023.08.18 =mrNo then
              begin
                result:='ABORT';
                exit;
              end;
          end;
        2:begin
            sSQL:='';
            sSQL:='exec CURdOCXTableEventRUN '+
              ''''+TJsdTable(tTable).TableName+''''+','+
              ''''+sEvenName+''''+','+
              '2'+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd1,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd2,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd3,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd4,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd5,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd6,'')+'''';
            qry.SQL.Clear;
            qry.SQL.Add(sSQL);
            qry.ExecSQL;
            qry.Close;
            qry.Free;
            result:='OK';
            exit;
          end;
        3:begin
            sSQL:='';
            sSQL:='exec CURdOCXTableEventRUN '+
              ''''+TJsdTable(tTable).TableName+''''+','+
              ''''+sEvenName+''''+','+
              '3'+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd1,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd2,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd3,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd4,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd5,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(tTable),OCXTableEventFd6,'')+'''';
            qry.SQL.Clear;
            qry.SQL.Add(sSQL);
            qry.Open;
            sReMsg:='';
            if qry.RecordCount>0 then
              begin
                sReMsg:=qry.Fields[0].AsString;
              end;
            qry.Close;
            qry.Free;

            if sReMsg<>'' then
              begin
                if copy(sReMsg,1,2)<>'OK' then
                  begin
                    sHead:=copy(sReMsg,1,6);
                    sBody:=copy(sReMsg,7,length(sReMsg));

                    if sHead='MESGE:' then MsgDlgJS(sBody, mtInformation, [mbOk], 0)
                    else if sHead='ERROR:' then MsgDlgJS(sBody, mtError, [mbOk], 0)
                    else if sHead='ABORT:' then
                      begin
                        MsgDlgJS(sBody, mtError, [mbOk], 0);
                        result:='ABORT';
                        exit;
                      end
                   else if sHead='CONFI:' then
                     begin
                      if MsgDlgJS(sBody, mtConfirmation, [mbYes,mbNo], 0)<>mrYes then //2023.08.18 =mrNo then
                        begin
                         result:='ABORT';
                         exit;
                        end;
                     end;
                  end;//if copy(sReMsg,1,2)<>'OK' then
              end;//if sReMsg<>'' then
          end;//3:begin

        {4:begin  //only for Test
            if TForm(tTable.Owner).FindComponent('qryDetail3')<>nil then
            begin
              if TJSdTable(TForm(tTable.Owner).FindComponent('qryDetail3')).SQL.Text<>'' then
              begin
                TDataSet(TForm(tTable.Owner).FindComponent('qryDetail3')).Close;
                TDataSet(TForm(tTable.Owner).FindComponent('qryDetail3')).Open;
              end;
            end;

            result:='OK';
            exit;
          end;}

        else
          begin
            result:='OK';
            exit;
          end;
      end;//case bOCXTableEventType of
    end;//else: if qry.RecordCount=0 then

  result:='OK';
end;
{
procedure TevCustBtnClick.prcDoCustBtnClick(sender:TSpeedButton);
begin
  //
end;
}
procedure TevFieldValidate.prcDoFieldValidate(sender:TField);
var sSQL:string; qry:TADOQuery;
    //ValidateSetUp:POCXFiledOnValidate;
    bOCXonValidateType: integer;
    OCXonValidateMsg: string;
    OCXonValidateFd1: string;
    OCXonValidateFd2: string;
    OCXonValidateFd3: string;
    OCXonValidateFd4: string;
    OCXonValidateFd5: string;
    OCXonValidateFd6: string;
    OCXonValidateUd: string;
    //sReValue: string;
    sReMsg,sHead,sBody: String;
begin
//ShowMessage('1');
  sSQL:='exec CURdOCXFiledOnValidate '+
    ''''+TJsdTable(sender.DataSet).TableName+''''+','+ //@TableName
    ''''+sender.FieldName+'''';//@FieldName

 qry:=TADOQuery.Create(nil);
 qry.ConnectionString:=TJsdTable(sender.DataSet).ConnectionString;
 qry.SQL.Add(sSQL);
 qry.Open;

 if qry.RecordCount=0 then
    begin
      qry.Close;
      qry.Free;
      exit;
    end
    else
    begin
      {
      new(ValidateSetUp);
      ValidateSetUp^.bOCXonValidateType:=qry.FieldByName('bOCXonValidateType').AsInteger;
      ValidateSetUp^.OCXonValidateMsg:=qry.FieldByName('OCXonValidateMsg').Asstring;
      ValidateSetUp^.OCXonValidateFd1:=qry.FieldByName('OCXonValidateFd1').Asstring;
      ValidateSetUp^.OCXonValidateFd2:=qry.FieldByName('OCXonValidateFd2').Asstring;
      ValidateSetUp^.OCXonValidateFd3:=qry.FieldByName('OCXonValidateFd3').Asstring;
      ValidateSetUp^.OCXonValidateFd4:=qry.FieldByName('OCXonValidateFd4').Asstring;
      ValidateSetUp^.OCXonValidateFd5:=qry.FieldByName('OCXonValidateFd5').Asstring;
      ValidateSetUp^.OCXonValidateFd6:=qry.FieldByName('OCXonValidateFd6').Asstring;
      ValidateSetUp^.OCXonValidateUd:=qry.FieldByName('OCXonValidateUd').Asstring;
      }
      bOCXonValidateType:=qry.FieldByName('bOCXonValidateType').AsInteger;
      OCXonValidateMsg:=qry.FieldByName('OCXonValidateMsg').AsString;
      OCXonValidateFd1:=qry.FieldByName('OCXonValidateFd1').AsString;
      OCXonValidateFd2:=qry.FieldByName('OCXonValidateFd2').AsString;
      OCXonValidateFd3:=qry.FieldByName('OCXonValidateFd3').AsString;
      OCXonValidateFd4:=qry.FieldByName('OCXonValidateFd4').AsString;
      OCXonValidateFd5:=qry.FieldByName('OCXonValidateFd5').AsString;
      OCXonValidateFd6:=qry.FieldByName('OCXonValidateFd6').AsString;
      OCXonValidateUd:=qry.FieldByName('OCXonValidateUd').AsString;
      qry.Close;

      if bOCXonValidateType in[0,1] then qry.Free;

      case bOCXonValidateType of
        0:begin
            if OCXonValidateMsg<>'' then MsgDlgJS(OCXonValidateMsg,mtError,[mbOK],0);

            Abort;
          end;
        1:begin
            if MsgDlgJS(OCXonValidateMsg,mtConfirmation,[mbYes,mbNo],0)<>mrYes then //2023.08.18 =mrNo then
            Abort;
          end;
        2:begin

            sSQL:='';
            sSQL:='exec CURdOCXFiledValidateDo '+
              ''''+TJsdTable(sender.DataSet).TableName+''''+','+ //@TableName
              ''''+sender.FieldName+''''+','+//@FieldName
              '2'+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd1,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd2,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd3,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd4,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd5,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd6,'')+'''';
            qry.SQL.Clear;
            qry.SQL.Add(sSQL);
            qry.ExecSQL;
            //qry.Open;
            //if qry.RecordCount>0 then
            //  begin
            //    sReValue:=qry.Fields[0].Asstring;

                //2009.6.7 停用, 因前端會無反應, 日後再解決
                //TJsdTable(sender.DataSet).FieldByName(OCXonValidateUd).Value:= sReValue;
            //  end;
            qry.Close;
            qry.Free;
          end;
        3:begin
            sSQL:='';
            sSQL:='exec CURdOCXFiledValidateDo '+
              ''''+TJsdTable(sender.DataSet).TableName+''''+','+ //@TableName
              ''''+sender.FieldName+''''+','+//@FieldName
              '3'+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd1,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd2,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd3,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd4,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd5,'')+''''+','+
              ''''+funIFFieldFind(TJsdTable(sender.DataSet),OCXonValidateFd6,'')+'''';
            qry.SQL.Clear;
            qry.SQL.Add(sSQL);
            qry.Open;
            sReMsg:='';
            if qry.RecordCount>0 then
              begin
                sReMsg:=qry.Fields[0].AsString;
              end;
            qry.Close;
            qry.Free;

            if sReMsg<>'' then
              begin
                if copy(sReMsg,1,2)<>'OK' then
                  begin
                    sHead:=copy(sReMsg,1,6);
                    sBody:=copy(sReMsg,7,length(sReMsg));

                    if sHead='MESGE:' then MsgDlgJS(sBody, mtInformation, [mbOk], 0)
                    else if sHead='ERROR:' then MsgDlgJS(sBody, mtError, [mbOk], 0)
                    else if sHead='ABORT:' then
                      begin
                        MsgDlgJS(sBody, mtError, [mbOk], 0);
                        ABORT;
                      end
                   else if sHead='CONFI:' then
                     begin
                      if MsgDlgJS(sBody, mtConfirmation, [mbYes,mbNo], 0)<>mrYes then //2023.08.18 =mrNo then
                        ABORT;
                     end;
                  end;
              end;
          end;
        else begin exit end;
      end;
    end;
end;


function funDoTableEvent(qry:TADOQuery;tTable:TDataSet;iCompelAftPost:integer):boolean;
var sSQL:string;
begin
if iCompelAftPost=1 then
  begin
   evTableEvent:=TevTableEvent.Create;


   evTableEvent.oldTableEvent:=tTable.AfterPost;
   tTable.AfterPost:=evTableEvent.prcTblEventAfterPost;

   //保留
   //evTableEvent.oldTableEvent:=tTable.BeforeDelete;
   //tTable.BeforeDelete:=evTableEvent.prcTblEventBeforeDelete;

   //保留
   //evTableEvent.oldTableEvent:=tTable.AfterDelete;
   //tTable.AfterDelete:=evTableEvent.prcTblEventAfterDelete;
  end;


  sSQL:='select EventName from CURdOCXTableEvent(nolock) where TableName='+

        ''''+TJSdTable(tTable).tableName+'''';

  with qry do

    begin

      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;


  if qry.recordcount>0 then

    begin

      with qry do
        begin
          first;
          while not eof do
            begin
              evTableEvent:=TevTableEvent.Create;

              if Fields[0].AsString='AfterClose' then
                begin
                  evTableEvent.oldTableEvent:=tTable.AfterOpen;
                  tTable.AfterOpen:=evTableEvent.prcTblEventAfterClose;
                end
              else if Fields[0].AsString='AfterDelete' then
                begin
                  evTableEvent.oldTableEvent:=tTable.AfterDelete;
                  //tTable.AfterDelete:=evTableEvent.prcTblEventAfterDelete;
                  tTable.AfterDelete:=evTableEvent.prcTblEventAfterDelete2;//2011.10.13 modify
                end
              else if Fields[0].AsString='AfterEdit' then
                begin
                  evTableEvent.oldTableEvent:=tTable.AfterEdit;
                  tTable.AfterEdit:=evTableEvent.prcTblEventAfterEdit;
                end
              else if Fields[0].AsString='AfterInsert' then
                begin
                  evTableEvent.oldTableEvent:=tTable.AfterInsert;
                  tTable.AfterInsert:=evTableEvent.prcTblEventAfterInsert;
                end
              else if Fields[0].AsString='AfterOpen' then
                begin
                  evTableEvent.oldTableEvent:=tTable.AfterOpen;
                  tTable.AfterOpen:=evTableEvent.prcTblEventAfterOpen;
                end


              else if ((Fields[0].AsString='AfterPost') and (iCompelAftPost=0)) then
                begin
                  evTableEvent.oldTableEvent:=tTable.AfterPost;
                  tTable.AfterPost:=evTableEvent.prcTblEventAfterPost2;
                end

              else if Fields[0].AsString='AfterRefresh' then
                begin
                  evTableEvent.oldTableEvent:=tTable.AfterRefresh;
                  tTable.AfterRefresh:=evTableEvent.prcTblEventAfterRefresh;
                end
              else if Fields[0].AsString='AfterScroll' then
                begin
                  evTableEvent.oldTableEvent:=tTable.AfterScroll;
                  tTable.AfterScroll:=evTableEvent.prcTblEventAfterScroll;
                end
              else if Fields[0].AsString='BeforeCancel' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforeCancel;
                  tTable.BeforeCancel:=evTableEvent.prcTblEventBeforeCancel;
                end
              else if Fields[0].AsString='BeforeClose' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforeClose;
                  tTable.BeforeClose:=evTableEvent.prcTblEventBeforeClose;
                end
              else if Fields[0].AsString='BeforeDelete' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforeDelete;
                  //tTable.BeforeDelete:=evTableEvent.prcTblEventBeforeDelete;
                  tTable.BeforeDelete:=evTableEvent.prcTblEventBeforeDelete2;//2011.10.13 modify
                end
              else if Fields[0].AsString='BeforeEdit' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforeEdit;
                  tTable.BeforeEdit:=evTableEvent.prcTblEventBeforeEdit;
                end
              else if Fields[0].AsString='BeforeInsert' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforeInsert;
                  tTable.BeforeInsert:=evTableEvent.prcTblEventBeforeInsert;
                end
              else if Fields[0].AsString='BeforeOpen' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforeOpen;
                  tTable.BeforeOpen:=evTableEvent.prcTblEventBeforeOpen;
                end
              else if Fields[0].AsString='BeforePost' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforePost;
                  tTable.BeforePost:=evTableEvent.prcTblEventBeforePost;
                end
              else if Fields[0].AsString='BeforeRefresh' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforeRefresh;
                  tTable.BeforeRefresh:=evTableEvent.prcTblEventBeforeRefresh;
                end
              else if Fields[0].AsString='BeforeScroll' then
                begin
                  evTableEvent.oldTableEvent:=tTable.BeforeScroll;
                  tTable.BeforeScroll:=evTableEvent.prcTblEventBeforeScroll;
                end;

             next;
            end;//while not eof do
        end;// with qry do
    end;//if qry.recordcount>0 then

  qry.Close;
  result:=true;
end;

function funDoFieldValidate(qry:TADOQuery;tTable:TJSdTable):boolean;
var i:integer; sSQL:string;
begin
  sSQL:='select FieldName from CURdTableField(nolock) where TableName='+
        ''''+tTable.TableName+''''+
        ' and bOCXonValidate=1'
      ;

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount>0 then
    begin
          if tTable.FieldCount=0 then
            if tTable.Active=false then
              if (tTable.ConnectionString<>'') and (tTable.SQL.Text<>'') then
                 tTable.open;

            for i := 0 to tTable.FieldCount - 1 do
              begin
                if qry.Locate('FieldName',tTable.Fields[i].FieldName,[loCaseInsensitive]) then
                  begin
                    evFieldValidate:=TevFieldValidate.Create;
                    evFieldValidate.oldFieldValidate:=tTable.Fields[i].OnValidate;
                    tTable.Fields[i].OnValidate:=evFieldValidate.prcDoFieldValidate;

                end;//if qry.Locate
              end;//for i := 0 to
    end;//if qry.RecordCount>0 then

  qry.Close;
  result:=true;
end;


function funGetRealTableName(qry:TADOQuery;sTableName: string): string;
var sRealTableName:string;
begin
    qry.Close;
    qry.SQL.Clear;
    qry.SQL.Add('select RealTableName=isnull(RealTableName,TableName) from '+
      'CURdTableName(nolock) where TableName='+''''+sTableName+''''
      );
    qry.Open;

    if qry.RecordCount >0  then
       sRealTableName:=qry.FieldByName('RealTableName').Asstring
       else sRealTableName:=sTableName;

      qry.Close;

    result:=sRealTableName;
end;

//2012.04.30 add
function GetTableKeysDLL(qryPK:TADOQuery;sTableName: WideString): Variant;
var
    fldName: WideString;
    k: integer;
    vArr: Variant;
begin

    Result := false;

    qryPK.Close;
    qryPK.SQL.Clear;
    qryPK.SQL.Add('exec CURdGetTablePK '''+sTableName+'''');
    qryPK.Open;

    vArr:= VarArrayCreate([0, qryPK.RecordCount-1], varOleStr);

    qryPK.first;

    k:= 0;
    while not qryPK.eof do
    begin
      fldName:= trim(lowercase(qryPK.FieldByName('COLUMN_NAME').AsString));
      vArr[k]:= fldName;
      Inc(k);
      qryPK.Next;
    end;

    qryPK.close;

    Result:= vArr;
end;


//停用
{
function funGetTableKeysOCX(qry:TADOQuery;sTableName: string): string;
var
    fldName: string;
    sRealTableName:string;
    sKeyStr:string;
begin
    sRealTableName:=funGetRealTableName(qry,sTableName);

    qry.Close;
    qry.SQL.Clear;
    qry.SQL.Add('exec CURdGetTablePK '''+sRealTableName+'''');
    qry.Open;
    qry.first;

    if qry.RecordCount=0 then
      begin
        Result:='';
        exit;
      end;

    sKeyStr:='';

    while not qry.eof do
    begin
      fldName:= trim(lowercase(qry.FieldByName('COLUMN_NAME').AsString));

      if sKeyStr<>'' then sKeyStr:=sKeyStr+';';

      if sKeyStr='' then sKeyStr:=fldName
      else sKeyStr:=sKeyStr+fldName;

      qry.Next;
    end;

    qry.Close;

    Result:= sKeyStr;
end;

}

function funGetLocateKeys(qry:TADOQuery;sItemId,sTableKind: String): string;

var sSQL,sLocateKeys:string;

begin

  sSQL:='select LocateKeys from CURdOCXTableSetUp(nolock)'

    +' where ItemId='+''''+sItemId+''''

    +' and TableKind='+''''+sTableKind+''''

    ;


  with qry do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount>0 then
    sLocateKeys:= qry.Fields[0].AsString
  else sLocateKeys:='';

  qry.Close;

  result:=sLocateKeys;
end;



//停用

function funDoJSdTableEvent(fm:TForm;qry:TADOQuery;tTable:TJSdTable;sEvent:string):string;
//var sSQL:string; sRe:string; sHead,sBody:string;
begin {
  if sEvent='AfterOpen' then
    begin
      if funDoAfterOpen(fm,tTable)=false then
         begin
           result:='OK';
           exit;
         end;
    end
  else if sEvent='AfterPost' then
    begin
      if funDoAfterPost(tTable)=false then
         begin
           result:='OK';
           exit;
         end;
    end;

  sSQL:='exec CURdOCXTableEventDo '+''''+tTable.TableName+''''+','+''''+sEvent+'''';

  with qry do
    begin
      close;
      sql.Clear;
      sql.Add(sSQL);
      open;
    end;

  if qry.RecordCount=0 then
    begin
      qry.Close;
      Result:='OK';
      exit;
    end;

  sRe:=qry.Fields[0].AsString;
  qry.Close;

  if copy(sRe,1,2)<>'OK' then
     begin
       sHead:=copy(sRe,1,6);
       sBody:=copy(sRe,7,length(sRe));

       if sHead='MESGE:' then
         MsgDlgJS(sBody, mtInformation, [mbOk], 0)
       else if sHead='ERROR:' then
            MsgDlgJS(sBody, mtError, [mbOk], 0)
       else if sHead='ABORT:' then
          begin
            MsgDlgJS(sBody, mtError, [mbOk], 0);
            result:='ABORT';
            exit;
          end
       else if sHead='CONFI:' then
          begin
            if MsgDlgJS(sBody, mtConfirmation, [mbYes,mbNo], 0)=mrNo then
            result:='ABORT'
            else
            result:='OK';

            exit;
          end;
     end;
  }
  Result:='OK';
end;

function funDoAfterPost(tTable:TJSdTable):boolean;
{var
    i,j:integer;
    sIndexFieldNames: widestring;
    sList:TStringList;
    vArr:variant;}

//var bk:TBookmark; //2011.10.13 diable for modify

var i:integer; //2011.10.13 modify

begin
{  if tTable.IndexFieldNames<>'' then
    begin
      sIndexFieldNames:=tTable.IndexFieldNames;
      sList:=TStringList.Create;
      //i:=0;
      try
        i:=pos(';',sIndexFieldNames);
        if i=0 then
          begin
            sList.Add(sIndexFieldNames);
          end
        else
          begin
            while i>0 do
              begin
                sList.Add(copy(sIndexFieldNames,1,i-1));
                sIndexFieldNames:=copy(sIndexFieldNames,i+1,length(sIndexFieldNames));
                i:=pos(';',sIndexFieldNames);
              end;
            sList.Add(sIndexFieldNames);
          end;

        vArr:= VarArrayCreate([0, sList.Count -1], varVariant);
        for j := 0 to sList.Count - 1 do
          vArr[j]:=tTable.FieldByName(sList.Strings[j]).Value;

        tTable.Close;
        tTable.Open;

        if sList.Count=1 then
          tTable.Locate(sIndexFieldNames,vArr[0],[loCaseInsensitive]);

        //2009.6.6停用 因無作用 日後再解決
        //else
        //  tTable.Locate(sIndexFieldNames,vArr,[loCaseInsensitive]);
      finally
        sList.Free;
      end;
    end
    else // if tTable.IndexFieldNames<>'' then
    begin
      tTable.Close;
      tTable.Open;
      //Result:=false;
      //exit;
    end; // if tTable.IndexFieldNames<>'' then
}

{ 2011.10.13 diable for modify

    bk:=tTable.GetBookmark;

    tTable.Close;

    tTable.Open;

    if tTable.RecordCount>0 then
      if Assigned(bk) then tTable.GotoBookmark(bk);

    if Assigned(bk) then tTable.FreeBookmark(bk);
}

//----------2011.10.13 modify
    i:= -255;//2012.03.27 add

    if tTable.RecordCount>0 then //2012.03.27 add 'if...'
      i:= tTable.RecNo;

    tTable.Close;
    tTable.Open;

    if tTable.RecordCount>0 then
    begin
      if i= -1 then tTable.Last
      else if i= 0 then tTable.first
      else tTable.MoveBy(i-1);
    end;
//----------

    //2012.03.27 disable 因出現「多重步驟 OLE DB 操作發生錯誤。請檢查每一個可用的 OLE DB 狀態值。尚未完成任何操作。」
    //2012.03.26 modify for MUT Bill-20120307-08
    //tTable.Refresh;

    Result:= true;
end;


////2011.10.13 add,//保留

function funDoBeforeDelete(tTable:TJSdTable):boolean;

var i:integer;
begin
    i:= tTable.RecNo;

    tTable.Tag:=i;

    Result:= true;
end;


//2011.10.13 add ,//保留

function funDoAfterDelete(tTable:TJSdTable):boolean;

var i:integer;

begin
    i:= tTable.Tag;

    //暫不Coding,因未使用

    Result:= true;
end;


//停用
function funDoAfterOpen(fm:TForm;tTable:TJSdTable):boolean;
//var i,j:integer;
begin
{ 2009.6.4 disable,因 JsdTable、 JsdDBGrid 改版後已能自動執行

  DoDD(tTable);

  //====必須用程式碼 Assign 才會觸動 TJSdDBGrid.ActiveChange
  //gridEasyDB.DataSource:=nil;
  //gridEasyDB.DataSource:=dsEasyDB;

  for i := 0 to fm.ComponentCount - 1 do
    if fm.Components[i] is TJSdDBGrid then
      if TJSdDBGrid(fm.Components[i]).Name
          = stringreplace(tTable.Name, 'tbl', 'grid', [rfReplaceAll, rfIgnoreCase]) then
       begin
         for j := 0 to fm.ComponentCount - 1 do
            if fm.Components[j] is TDataSource then
               if TDataSource(fm.Components[j]).DataSet = tTable then
                  //TDataSource(fm.Components[j]).Name
                  //=stringreplace(tTable.Name, 'tbl', 'ds', [rfReplaceAll, rfIgnoreCase]) then

                    begin
                      TJSdDBGrid(fm.Components[i]).DataSource:=nil;
                      TJSdDBGrid(fm.Components[i]).DataSource
                        :=TDataSource(fm.Components[j]);

                    end;
       end;
  //====
}
  result:=true;
end;
{
procedure prcSetFormatDBExOCX(dbset: TCustomADODataSet; sTableName, Conn: string);
var sItems, sfmt, slab, sNodboName, sFieldName: string;
    sFontName, sFontColor, sFontSize, sFontStyle: string;
    sLookupTable, sLookupKeyField, sLookupResultField: string;
    sLookupCond1Field, sLookupCond2Field, sLookupCond1ResultField, sLookupCond2ResultField: string;
    i, j, k, ivis, iSNum, iSize, //iFontColor,
    iReadOnly, iComboStyle: integer;
    dbQuery: TADOQuery;
    //LkResults: Variant;
    nVfd: Boolean;
begin
  sNodboName:= StringReplace(sTableName, 'dbo.', '', [rfReplaceAll, rfIgnoreCase]);
  dbQuery := TADOQuery.Create(nil);
  try
    with dbQuery do
    begin
      ConnectionString := Conn;
      CommandTimeout := 480;
      LockType := ltReadOnly;
      SQL.Add('Select t1.*, TableLabel=t2.DisplayLabel '
            +' from CURdTableField t1(nolock), CURdTableName t2(nolock) '
            +' where t1.TableName = t2.TableName '
            +' and t1.TableName ='''+sNodboName+'''');
      Open;
      if dbQuery.RecordCount=0 then Exit;
    end;
    if dbset is TJSdTable then
      TJSdTable(dbset).DisplayLabel:= dbQuery.FieldByName('TableLabel').Asstring;

    with TCustomADODataSet(dbset) do
    begin
      DisableControls;
      nVfd:= false;
      for i := 0 to FieldCount - 1 do
      begin
        sFieldName:= Fields[i].FieldName;
        slab:= Fields[i].DisplayLabel;
        if (Fields[i] is TFloatField) then
           sfmt:= TFloatField(Fields[i]).DisplayFormat
        else
           sfmt:= Fields[i].EditMask;

        ivis:= IIFInteger(Fields[i].Visible, '1', '0');
        iReadOnly:= IIFInteger(Fields[i].ReadOnly, '1', '0');
        iSNum:= Fields[i].Index;
        //comboBox
        sItems:= '';
        sFontName:= '';
        sFontSize:= '';
        sFontColor:= '';
        sFontStyle:= '';
        sLookupTable:= '';
        sLookupKeyField:= '';
        sLookupResultField:= '';
        sLookupCond1Field:= '';
        sLookupCond2Field:= '';
        iComboStyle:= 0;
        if dbQuery.Locate('TableName;FieldName',
                        VarArrayOf([sNodboName, sFieldName]),
                        [loPartialKey]) then
        begin
          slab:= trim(dbQuery.FieldByName('DisplayLabel').Asstring);
          sfmt:= trim(dbQuery.FieldByName('FormatStr').Asstring);
          ivis:= dbQuery.FieldByName('Visible').AsInteger;
          iSNum:= dbQuery.FieldByName('SerialNum').AsInteger-1;
          iSize:= dbQuery.FieldByName('DisplaySize').AsInteger;
          iReadOnly:= dbQuery.FieldByName('ReadOnly').AsInteger;
          //comboBox
          sItems:= dbQuery.FieldByName('Items').Asstring;
          sFontName:= trim(dbQuery.FieldByName('FontName').Asstring);
          sFontSize:= trim(dbQuery.FieldByName('FontSize').Asstring);
          sFontColor:= trim(dbQuery.FieldByName('FontColor').Asstring);
          sFontStyle:= trim(dbQuery.FieldByName('FontStyle').Asstring);
          sLookupTable:= trim(dbQuery.FieldByName('LookupTable').Asstring);
          sLookupKeyField:= trim(dbQuery.FieldByName('LookupKeyField').Asstring);
          sLookupResultField:= trim(dbQuery.FieldByName('LookupResultField').Asstring);
          sLookupCond1Field:= trim(dbQuery.FieldByName('LookupCond1Field').Asstring);
          sLookupCond2Field:= trim(dbQuery.FieldByName('LookupCond2Field').Asstring);
          sLookupCond1ResultField:= trim(dbQuery.FieldByName('LookupCond1ResultField').Asstring);
          sLookupCond2ResultField:= trim(dbQuery.FieldByName('LookupCond2ResultField').Asstring);
          iComboStyle:= dbQuery.FieldByName('ComboStyle').AsInteger;
        end;
        //Fields[i].Index := iSNum;  整個錯亂，另外處理
        Fields[i].ReadOnly:= iReadOnly=1;
        Fields[i].Tag:= iSNum;
        Fields[i].Visible := ivis=1;
        if NOT nVfd then nVfd:= Fields[i].Visible;
        slab:= IIFString(slab='', Fields[i].DisplayLabel, slab);
        if IsPartNumField(Fields[i]) then
        begin
          slab:= slab+'(F4)';
        end;
        Fields[i].DisplayLabel:= slab;
        if iSize>0 then
           Fields[i].DisplayWidth := iSize;
        //Font
        Fields[i].Origin:= trim(sFontName)+';'+trim(sFontSize)
                            +';'+trim(sFontColor)+';'+trim(sFontStyle);
        //Checkbox
        if iComboStyle=1 then
            Fields[i].Alignment:= taCenter
        else
            Fields[i].Alignment:= taLeftJustify;
        //Lookup
        if sItems='' then
            Fields[i].DefaultExpression:=
                trim(sLookupTable)
                +';'+trim(sLookupKeyField)
                +';'+trim(sLookupResultField)
                +';'+trim(sLookupCond1Field)
                +';'+trim(sLookupCond2Field)
                +';'+trim(sLookupCond1ResultField)
                +';'+trim(sLookupCond2ResultField)
        else
            Fields[i].DefaultExpression:=sItems;

        if sfmt<>'' then
        begin
          if ((Fields[i] is TStringField) or (Fields[i] is TstringField)) then
          begin
             Fields[i].EditMask := sfmt;
          end
          else
          if (Fields[i] is TDatetimeField) then
          begin
            if Uppercase(sfmt)='YYYY/MM/DD' then
            begin
               TDatetimeField(Fields[i]).EditMask := '9999/99/99';
               TDatetimeField(Fields[i]).DisplayFormat := 'YYYY/MM/DD';
            end
            else if Uppercase(sfmt)='HH:MM' then
            begin
               TDatetimeField(Fields[i]).EditMask := '99:99';
               TDatetimeField(Fields[i]).DisplayFormat := 'HH:mm';
            end
            else
            begin
               TDatetimeField(Fields[i]).EditMask := '9999/99/99 99:99';
               TDatetimeField(Fields[i]).DisplayFormat := 'YYYY/MM/DD HH:mm';
            end;
          end
          else
          if (Fields[i] is TFloatField) then
          begin
            TFloatField(Fields[i]).EditFormat := sfmt;
            TFloatField(Fields[i]).DisplayFormat := sfmt;
            //TNTGrid造成只有1個輸入位數+小數點前被限制住
            //TFloatField(Fields[i]).EditMask := sfmt;
          end;
        end;
      end;

      for j := 0 to FieldCount-1 do
      begin
        for k := j to FieldCount-1 do
        begin
          if Fields[k].Tag = j then
          begin
             Fields[k].Index:= j;
             break;
          end;
        end;
      end;
      if ((not nVfd) and (FieldCount>0)) then Fields[0].Visible:= true;
      EnableControls;
    end;
  finally
    dbQuery.Free;
  end;
end;

}


{

procedure TJSdAfterOpen.DoAfterOpenEvent(dataset: TCustomADODataset);

begin
    DoDD(dataset);
end;

}
{
procedure ActiveFormSet(fm: TForm);
var i: integer;
begin
   for i:= 0 to fm.componentcount-1 do
   begin
      if fm.components[i] is TCustomADODataSet then
      begin
        JSdAfterOpen:=TJSdAfterOpen.Create;
        JSdAfterOpen.oldAfterOpen:=TCustomADODataSet(fm.components[i]).AfterOpen;
        TCustomADODataSet(fm.components[i]).AfterOpen
          :=JSdAfterOpen.DoAfterOpenEvent(TCustomADODataSet(fm.components[i]));
      end;
   end;
end;

}

{

procedure DoGrid(gridJS:TJsdDBGrid);

var i: integer;
var sFontAll, sFontName, sFontColor, sFontStyle, sExp1: string;
    sFontArray: Array[0..3] of string;
    iFontSize, iFontColor: integer;
begin
with gridJS do
begin

  for i := 0 to Columns.Count - 1 do
  begin
    if not assigned(Columns[i].Field) then Exit;

    sFontAll:= ';;';
    sFontAll:= Columns[i].Field.Origin;
    if sFontAll<>';;' then
    begin
      ParseFormatW(sFontAll, ';', sFontArray, 4);

      if sFontArray[0]='' then
         sFontName:= '細明體'
      else
         sFontName:= sFontArray[0];
      if sFontArray[1]<>'' then
         iFontSize:= strtoint(sFontArray[1])
      else
         iFontSize:= 9;
      if not ((iFontSize>=8) and (iFontSize<=72)) then
         iFontSize:= 9;
      if not IdentToColor(sFontArray[2], iFontColor) then
         sFontColor:= 'clBlack'
      else
         sFontColor:= sFontArray[2];

      sFontStyle:= sFontArray[3];
       StringToFontStyle(Columns[i].Title.Font, sFontStyle);
       Columns[i].Title.Font.Name:= sFontName;
       Columns[i].Title.Font.Size:= iFontSize;
       Columns[i].Title.Font.Color:= StringToColor(sFontColor);
     end;
     sExp1:= Columns[i].Field.DefaultExpression;
     sExp1:= Copy(sExp1, 1, pos(';', sExp1)-1);
     //(沒有;)=(有Items) or (sExp1<>'')=(有select)
     if sExp1<>'' then
     begin
       Columns[i].Title.Font.Style:=[fsBold];
       Columns[i].Title.Font.Color:=clNavy;
     end;
  end;
end;
end;
}

{

procedure prcDoDD(DataSet: TCustomADODataSet);

var sTblName, connstr: string;
begin
   connstr:=DataSet.ConnectionString;

  if DataSet is TJSdTable then
  begin
     sTblName:= TJSdTable(DataSet).TableName;
     if sTblName<>'' then
        prcSetFormatDBExOCX(TJSdTable(DataSet), sTblName, connstr);
  end
  else if DataSet is TADOTable then
  begin
     sTblName:= TADOTable(DataSet).TableName;
     if sTblName<>'' then
        prcSetFormatDBExOCX(TADOTable(DataSet), sTblName, connstr);
  end
  else if DataSet is TADOQuery then
  begin
     sTblName:='';
     sTblName:= GetQueryTable(TADOQuery(DataSet));
     if sTblName<>'' then
        prcSetFormatDBExOCX(TADOQuery(DataSet), sTblName, connstr);
  end;

  InitJSdTableLable(DataSet);
end;
}

function funGetFormInfo(

  qry:TADOQuery;
  //sCoClassName:string;
  sItemId:string;
  sTableKind:string;
  var sTableName:string;
  var iFixColCount:integer;
  var sMDKey:string;
  var sRealTableName:string;
  iMust:integer;
  var sDDCaption:string;
  sLanguageId:string
  ):boolean;
var sSQL:string;
begin
   sDDCaption:='';

   sSQL:='select * from CURdOCXTableSetUp(nolock)'+
          ' where ItemId='+''''+sItemId+''''+
          ' and TableKind='+''''+sTableKind+''''
          ;

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      Open;
    end;

  if qry.RecordCount=0 then
    begin
      qry.Close;

      if iMust=1 then
        begin
          MsgDlgJS('程式項目'+sItemId+'未建立資料來源', mtError, [mbOk], 0);
          result:=false;
        end
        else
        begin
         result:=true;
        end;

      exit;
    end;

  sTableName:=qry.FieldByName('TableName').AsString;
  iFixColCount:=qry.FieldByName('FixColCount').Asinteger;
  sMDKey:=qry.FieldByName('MDKey').AsString;
  qry.Close;

  {sSQL:='select RealTableName=isnull(RealTableName,TableName),DisplayLabel '+
        'from CURdTableName(nolock) where TableName='+
        ''''+sTableName+'''';}

  sSQL:='exec CURdTableNameRealGet '+''''+sTableName+''''+','+''''+sLanguageId+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      Open;
    end;

  if qry.RecordCount = 0 then
    begin
       MsgDlgJS('資料辭典'+sTableName +'未設定實體資料表', mtError, [mbOk], 0);
       qry.Close;
       result:=false;
       exit;
    end;

  sRealTableName:=qry.FieldByName('RealTableName').AsString;
  sDDCaption:=qry.FieldByName('langDisplayLabel').AswideString;

  qry.Close;

  result:=true;
end;

function funGetSelectSQL(
  qry:TADOQuery;
  sCoClassName:string;
  sTableName:string;
  var sSelectSQL:string
  ):boolean;
var sSQL:string;
begin
  sSQL:='exec CURdOCXSQLPreView '+
      ''''+sTableName+'''';

  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      Open;
    end;

  if qry.RecordCount = 0 then
    begin
       MsgDlgJS('資料辭典有誤('+sTableName+')', mtError, [mbOk], 0);
       qry.Close;
       result:=false;
       exit;
    end;

  sSelectSQL:=qry.FieldByName('sSQL').AsString;

  qry.Close;

  result:=true;
end;

procedure prcFormSetConnDLL(fm: TForm;sconnstr: string);
var i: integer;
//bConnExists:boolean;//2012.05.22 add for WF Bill-20120518-05
//connTemp:TADOConnection;
begin
{   //2012.05.22 add for WF Bill-20120518-05
   bConnExists:=false;

   for i:= 0 to fm.componentcount-1 do
   begin
     if fm.components[i].Name='connJSIS' then
      begin
        bConnExists:=true;

        TADOConnection(fm.components[i]).Connected:=false;
        TADOConnection(fm.components[i]).ConnectionString:=sconnstr;
        connTemp:=TADOConnection(fm.components[i]);

        break;
      end;
   end;
}
   for i:= 0 to fm.componentcount-1 do
   begin
      if fm.components[i] is TCustomADODataSet then
      begin
         TCustomADODataSet(fm.components[i]).Close;

         {if bConnExists then
         begin
           if TCustomADODataSet(fm.components[i]).Connection=nil then
              TCustomADODataSet(fm.components[i]).Connection:=connTemp;
         end
         else} TCustomADODataSet(fm.components[i]).ConnectionString:= sconnstr;

      end
      else
      if fm.components[i] is TJSdTable then
      begin
         TJSdTable(fm.components[i]).Close;

         {if bConnExists then
         begin
           if TJSdTable(fm.components[i]).Connection=nil then
              TJSdTable(fm.components[i]).Connection:=connTemp;
         end
         else} TJSdTable(fm.components[i]).ConnectionString:= sconnstr;

         TJSdTable(fm.components[i]).CommandTimeout:=9600; //2012.04.11 add
      end
      else
      if fm.components[i] is TADOQuery then
      begin
         TADOQuery(fm.components[i]).Close;

         {if bConnExists then
         begin
           if TADOQuery(fm.components[i]).Connection=nil then
              TADOQuery(fm.components[i]).Connection:=connTemp;
         end
         else} TADOQuery(fm.components[i]).ConnectionString:= sconnstr;

         TADOQuery(fm.components[i]).CommandTimeout:=9600; //2012.04.11 add
      end;
   end;
end;

function funConnectedADO(conn:TADOConnection;sConnStr:string):boolean;
begin
  with conn do
    begin
      if Connected then Connected:=false;
      ConnectionString:=sConnStr;
      //ConnectionTimeOut:=600;
      ConnectionTimeOut:=9600; //2012.04.11 modify
      LoginPrompt:=false;

      Connected:=true;
    end;

  result:=conn.Connected;
end;
{
function funGetOCXInfoDlg(
  dlgAfm:TForm;
  var sItemId:string;
  var sOcxName:string;
  var sUserId:string;
  var sBUID:string;
  var sServerName:string;
  var sDBName:string;
  var sGlobalId:string;
  var sUseId:string;
  var sPaperNum:string;
  var sConnectStr:string
  ):boolean;
var sProfilePath,sFileName:string;
  sList:TStringList;
  I:integer;
begin
  sProfilePath:=unit_MIS.GetTempPathStr;

  if sProfilePath='' then
    begin
       MsgDlgJS('無法取得下載路徑', mtError, [mbOk], 0);
       result:=false;
       exit;
    end;

  sFileName:=sProfilePath+dlgAfm.Name+'.txt';

  if not FileExists(sFileName) then
    begin
     		MsgDlgJS('OCX資訊檔['+sFileName+']不存在', mtError, [mbOk], 0);
     		result:=false;
        exit;
    end;

  sList:=TStringList.Create;
  try
    sList.LoadFromFile(sFileName);

    for I := 0 to sList.Count - 1 do
       if sList.Names[I]='ItemId' then sItemId:= sList.Values['ItemId']
       else if sList.Names[I]='OcxName' then sOcxName:= sList.Values['OcxName']
       else if sList.Names[I]='UserId' then sUserId:= sList.Values['UserId']
       else if sList.Names[I]='BUID' then sBUID:= sList.Values['BUID']
       else if sList.Names[I]='SERVERName' then sSERVERName:= sList.Values['SERVERName']
       else if sList.Names[I]='DBName' then sDBName:= sList.Values['DBName']
       else if sList.Names[I]='UseId' then sUseId:= sList.Values['UseId']
       else if sList.Names[I]='GlobalId' then sGlobalId:= sList.Values['GlobalId']
       else if sList.Names[I]='PaperNum' then sPaperNum:= sList.Values['PaperNum'];

     sConnectStr
        :='Provider=SQLOLEDB.1;Password=JSIS;Persist Security Info=True;'+
            'User ID=JSIS;Initial Catalog='+sDBName+';Data Source='+sSERVERName;
  finally
    sList.Free;
  end;

  prcFormSetConnDLL(dlgAfm,sConnectStr);

  result:=true;
end;
}
{
function funGetOCXInfo(
  var sServerName:string;
  var sDBName:string;
  sCoClassName:string;
  var sItemId:string;
  var sClassName:string;
  var sUserId:string;
  var sBUID:string;
  var sGlobalId:string;
  var sUseId:string;
  var sConnectStr:string
  ):boolean;
var sProfilePath,sFileName:string;
  sList:TStringList;
  I:integer;
begin
  sProfilePath:=unit_MIS.GetTempPathStr;  //unit_MIS.GetERPAppLocalPath;

  if sProfilePath='' then
    begin
       MsgDlgJS('無法取得下載路徑', mtError, [mbOk], 0);
       result:=false;
       exit;
    end;

  sFileName:=sProfilePath+sCoClassName+'.txt';

  if not FileExists(sFileName) then
    begin
     		MsgDlgJS('OCX資訊檔['+sFileName+']不存在', mtError, [mbOk], 0);
     		result:=false;
        exit;
    end;

  sList:=TStringList.Create;
  try
    sList.LoadFromFile(sFileName);

    for I := 0 to sList.Count - 1 do
       if sList.Names[I]='ItemId' then sItemId:= sList.Values['ItemId']
       else if sList.Names[I]='ClassName' then sClassName:= sList.Values['ClassName']
       else if sList.Names[I]='UserId' then sUserId:= sList.Values['UserId']
       else if sList.Names[I]='BUID' then sBUID:= sList.Values['BUID']
       else if sList.Names[I]='GlobalId' then sGlobalId:= sList.Values['GlobalId']
       else if sList.Names[I]='SERVERName' then sSERVERName:= sList.Values['SERVERName']
       else if sList.Names[I]='DBName' then sDBName:= sList.Values['DBName']
       else if sList.Names[I]='UseId' then sUseId:= sList.Values['UseId'];

     sConnectStr
        :='Provider=SQLOLEDB.1;Password=JSIS;Persist Security Info=True;'+
            'User ID=JSIS;Initial Catalog='+sDBName+';Data Source='+sSERVERName;
  finally
    sList.Free;
  end;

  result:=true;
end;
}

procedure OpenSQLDLL(qry:TADOQuery;sDo,sSQL:string);
begin
  with qry do
    begin
      if active then close;
      sql.Clear;
      sql.Add(sSQL);
      if sDo='EXEC' then
        begin
          ExecSQL;
          close;
        end
      else if sDo='OPEN' then open;
    end;
end;

function DsExport(frm:TForm;data:TCustomADODataSet):boolean;
var
OutFileName,  sExtName:String;
dlgSave:TSaveDialog;
begin
  result:=false;

  if data.Active=false then
     begin
       MsgDlgJS('無資料可供處理',mtWarning,[mbOK],0);
       exit;
     end;

  unit_DLL.prcSaveALL(frm);

  if data.RecordCount=0 then
     begin
       MsgDlgJS('無資料可供處理',mtWarning,[mbOK],0);
       exit;
     end;

  dlgSave:=TSaveDialog.Create(frm);
  try
    with dlgSave do
    begin
      Filter :='Excel(*.XLSX)|*.XLSX|Access(*.mdb)|*.mdb|Any(*.*)|*.*';   //xls

      FilterIndex := 1;

      if Execute then
      begin
        sExtName:=ExtractFileExt(FileName);//2012.11.21 add

        //if pos('.', FileName)<=0 then
        if sExtName='' then //2012.11.21 modify
        begin
          case filterindex of
            1:OutFileName := FileName+'.xlsx';
            2:OutFileName := FileName+'.mdb';
            else OutFileName := FileName+'.txt';
          end;
        end
        //2012.11.21 add
        else
        begin
          OutFileName:=FileName;
        end;

        //sExtName:= lowercase(Copy(OutFileName, pos('.', OutFileName), 10));
        sExtName:=ExtractFileExt(OutFileName);//2012.11.21 modify

        if sExtName='.xlsx' then
        begin

            Ds2ExcelSimple(data,
              IIFStringDLL(TJSdTable(data).TableName='','Sheet1',TJSdTable(data).TableName)
              ,OutFileName,1);

        end
        else if sExtName='.mdb' then
        begin
          DataSet2AccessDLL(data, OutFileName,
            IIFStringDLL(TJSdTable(data).TableName='','Sheet1',TJSdTable(data).TableName));
        end
        else if sExtName='.txt' then
        begin
          Ds2TxtSimple(frm,data,OutFileName);
        end;
      end;
    end;
   finally
     dlgSave.Free;
   end;


  result:=true;
end;

function Ds2TxtSimple(frm:TForm;data:TCustomADODataSet;sFileName:WideString):boolean;
var
   //BookMark1: TBookMark;
   FieldNo, jRow: longint;
   TmpStr: String;
   mMemo:TMemo;
begin
  result:=false;

  if fileexists(sFileName) then
  begin
      MsgDlgJS('檔案: [' + sFileName +
                 '] 已存在，請輸入其它檔名', mtError, [mbOk], 0);
      Exit;
  end;

  try
     mMemo:=TMemo.Create(frm);
     mMemo.Parent:=frm;

     frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(frm);
     frmLoadProgressDLL.Caption:= '資料計算彙整中......';
     frmLoadProgressDLL.show;
     frmLoadProgressDLL.Initialize(1, data.RecordCount);
     jRow := 1;
     with data do
     begin
        TmpStr := '';
        first;
        for FieldNo := 0 to FieldCount - 1 do
           if Fields[FieldNo].Visible then
              TmpStr := TmpStr + Fields[FieldNo].DisplayLabel + #9;

        mMemo.Lines.Append(TmpStr);

        Screen.Cursor := crHourGlass;
        //Bookmark1 := GetBookMark;
        DisableControls;

        First;

        while not EOF do
        begin
            TmpStr := '';

            for FieldNo := 0 to FieldCount - 1 do
               if Fields[FieldNo].Visible then
                 TmpStr:=TmpStr + Fields[FieldNo].DisplayText + #9;

            mMemo.Lines.Append(TmpStr);

            Inc(jRow);
            Next;

            frmLoadProgressDLL.Add;
            frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(jRow) + ' 筆資料';
            frmLoadProgressDLL.pnlStatus.Refresh;
         end;
      end;

    mMemo.Lines.SaveToFile(sFileName,TEncoding.Unicode);
  finally
    mMemo.Free;

    Screen.Cursor := crDefault;
    //data.GotoBookMark(BookMark1);
    //data.FreeBookMark(BookMark1);
    data.EnableControls;

    if assigned(frmLoadProgressDLL) then
    begin
      frmLoadProgressDLL.free;
    end;
  end;

  result:=true;
end;


function Ds2ExcelSimple(data:TCustomADODataSet;sSheetName,sOutFileName:string;iOpen:integer):Boolean;
var
  Save_Cursor:TCursor;
  XLApp, Sheet, v, vArr: Variant;
  iCol, iColv, jRow: Integer;
begin
  Result := False;

  try
    XLApp := CreateOLEObject('Excel.Application');
  except
    on e:exception do
    begin
      MsgDlgJS('您的電腦可能沒有安裝 Excel (系統錯誤訊息：'+e.Message+')',mtError,[mbOk],0);
      exit;
    end;
  end;

  TRY
    data.DisableControls;

    frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(nil);
    frmLoadProgressDLL.Caption:= '資料計算彙整中......';
    frmLoadProgressDLL.show;
    frmLoadProgressDLL.Initialize(1, data.RecordCount);

    Save_Cursor := Screen.Cursor;
    Screen.Cursor := crHourglass;

    //XLApp := CreateOLEObject('Excel.Application'); //已搬到上面
    XLApp.Visible := false;

    XLApp.Workbooks.Add;
    //XLApp.Workbooks[1].Worksheets[1].Name := sSheetName;
    Sheet := XLApp.Worksheets[1];

    jRow := 1;
    iColv:= 0;

    for iCol := 1 to data.FieldCount do
    begin
      if data.Fields[iCol-1].Visible then
      begin
        v := data.Fields[iCol-1].DisplayLabel;
        Inc(iColv);
        Sheet.Cells[jRow, iColv] := VarToStr(v);

        //XLApp.Workbooks[1].WorkSheets[1].Columns[iColv].Font.Size := 9; //2016.08.12 add
        //XLApp.Workbooks[1].WorkSheets[1].Columns[iColv].ColumnWidth:= data.Fields[iCol-1].DisplayWidth; //2016.08.12 add
      end;
      data.Next;
    end;


    //XLApp.Workbooks[1].WorkSheets[1].Rows.Item[1].Font.Bold := True; //2016.08.12 add

    data.First;
    Inc(jRow);
    while not data.EOF do
    begin
      iColv:= 0;
      for iCol := 1 to data.FieldCount do
      begin
        if data.Fields[iCol-1].Visible then
        begin
          v := data.Fields[iCol-1].Value;
          Inc(iColv);

        if data.Fields[iCol-1].DataType
            in[ftString,ftWideString,ftMemo,ftFmtMemo,ftWideMemo,ftFixedChar,ftFixedWideChar] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('@');

        //2020.07.08 add for SS
        if data.Fields[iCol-1].DataType
            in[ftSmallint,ftInteger] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('#,###');
        if data.Fields[iCol-1].DataType
            in[ftFloat,ftCurrency,ftBCD] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=tfloatfield (data.Fields[iCol-1]).DisplayFormat;

            Sheet.Cells[jRow, iColv] := VarToStr(v);
        end;
      end;
      Inc(jRow);
      data.Next;

      frmLoadProgressDLL.Add;
      frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(jRow) + ' 筆資料';
      frmLoadProgressDLL.pnlStatus.Refresh;
    end;

    XLApp.Visible := iOpen = 0;
  FINALLY
    if iOpen = 1 then
    begin
      //XLApp.ActiveWorkbook.SaveAs(sOutFileName, xlNormal, '', '', False, False);
      //XLApp.ActiveWorkbook.SaveAs(sOutFileName);//2016.10.13 disable
      XLApp.ActiveWorkbook.SaveAs(sOutFileName,51);//2016.10.14 modify     //2020.07.08
      XLApp.quit;
    end;

    if assigned(frmLoadProgressDLL) then
    begin
      frmLoadProgressDLL.free;
    end;

    Screen.Cursor := Save_Cursor;
    data.First;
    data.EnableControls;
  END;

  Result := True;
end;

function Ds2ExcelSimple2(data:TCustomADODataSet;sSheetName,sOutFileName:string):Boolean;
var
  Save_Cursor:TCursor;
  XLApp, Sheet, v, vArr: Variant;
  iCol, iColv, jRow: Integer;
begin
  Result := False;

  TRY
    data.DisableControls;

    Save_Cursor := Screen.Cursor;
    Screen.Cursor := crHourglass;

    XLApp := CreateOLEObject('Excel.Application');
    XLApp.Visible := false;

    //XLApp.Workbooks.Add[XLWBatWorksheet];
    XLApp.Workbooks.Add;//2016.10.13 modify
    //XLApp.Workbooks[1].Worksheets[1].Name := sSheetName; //2016.10.13 disable

    //Sheet := XLApp.Workbooks[1].Worksheets[sSheetName];
    Sheet := XLApp.Worksheets[1]; //2016.10.13 modify

    jRow := 1;
    iColv:= 0;

    for iCol := 1 to data.FieldCount do
    begin
      if data.Fields[iCol-1].Visible then
      begin
        v := data.Fields[iCol-1].DisplayLabel;
        Inc(iColv);
        Sheet.Cells[jRow, iColv] := VarToStr(v);

        //XLApp.Workbooks[1].WorkSheets[1].Columns[iColv].Font.Size := 9; //2016.08.12 add  //2016.10.13 disable
        //XLApp.Workbooks[1].WorkSheets[1].Columns[iColv].ColumnWidth:= data.Fields[iCol-1].DisplayWidth; //2016.08.12 add  //2016.10.13 disable
      end;
      data.Next;
    end;


    //XLApp.Workbooks[1].WorkSheets[1].Rows.Item[1].Font.Bold := True; //2016.08.12 add //2016.10.13 disable

    data.First;
    Inc(jRow);
    while not data.EOF do
    begin
      iColv:= 0;
      for iCol := 1 to data.FieldCount do
      begin
        if data.Fields[iCol-1].Visible then
        begin
          v := data.Fields[iCol-1].Value;
          Inc(iColv);

        if data.Fields[iCol-1].DataType
            in[ftString,ftWideString,ftMemo,ftFmtMemo,ftWideMemo,ftFixedChar,ftFixedWideChar] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('@');

        //2020.07.08 add for SS
        if data.Fields[iCol-1].DataType
            in[ftSmallint,ftInteger] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('#,###');
        if data.Fields[iCol-1].DataType
            in[ftFloat,ftCurrency,ftBCD] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=tfloatfield (data.Fields[iCol-1]).DisplayFormat;

            Sheet.Cells[jRow, iColv] := VarToStr(v);
        end;
      end;
      Inc(jRow);
      data.Next;
    end;

    XLApp.Visible := False;
  FINALLY

      //XLApp.ActiveWorkbook.SaveAs(sOutFileName, xlNormal, '', '', False, False);
      XLApp.ActiveWorkbook.SaveAs(sOutFileName,51);//2016.10.14 modify    //2020.07.08

      XLApp.quit;

    Screen.Cursor := Save_Cursor;
    data.First;
    data.EnableControls;
  END;

  Result := True;
end;


function Ds2Excel4ReImport(data:TCustomADODataSet; sTableName: string;
   iOpen: integer): Boolean;
var dlgSave:TSaveDialog;sOutFileName:string;
begin
  Result := False;

  dlgSave:=TSaveDialog.Create(nil);
  try
    with dlgSave do
      begin
        FileName:=sTableName;
        Filter :='Excel(*.XLSX)|*.XLSX';
        FilterIndex := 1;

        if Execute then
          begin
            sOutFileName := FileName;
            if pos('.', sOutFileName)<=0 then sOutFileName := sOutFileName+'.xlsx';
          end
        else
          begin
            dlgSave.Free;
            exit;
          end;
      end;//with
   except
    on e:exception do
      begin
         dlgSave.Free;
         MsgDlgJS('輸入檔名失敗，系統錯誤訊息：'+e.Message,mtError,[mbOk],0);
         exit;
      end;//on
    end;//try

  dlgSave.Free;

  Ds2ExcelSimple(data,sTableName,sOutFileName,iOpen);
end;

procedure prcSetReadOnly(tbl:TJSdTable;bReadOnly:boolean);//2009.12.22 add
var  bNeedLocate:boolean; bk:Tbookmark;
begin
       bNeedLocate:=false;

       if tbl.Active then
       begin
        if tbl.State in[dsEdit,dsInsert] then tbl.Post;

        if tbl.RecordCount>0 then
          begin
            bNeedLocate:=true;
            bk:=tbl.GetBookmark;
          end;
       end;

          tbl.close;

          if bReadonly then
            begin
              tbl.LockType
               :=ltReadonly;
              tbl.CursorLocation
               :=clUseClient;
            end
          else
            begin
              tbl.LockType
                :=ltOptimistic;
              tbl.CursorLocation
                :=clUseServer;
            end;

       //2021.01.22 薪資轉傳票要先修改CursorLocation屬性,改為clUseClient
       //2021.01.25 先針對薪資轉傳票,日後有時間再改良成CUR可設定
       if (tbl.TableName='HSAdV_PeriodInJour')
          or
          (tbl.TableName='HSAdJourMain')
          or
          (tbl.TableName='SBPdMatReqPlanMain')  //2022.04.12 add  材料需求計畫
          or
          (tbl.TableName='HSAdTuneChargeBossMain') then //2021.02.25 薪資轉傳票改實體Table
       begin
           tbl.CursorLocation:= clUseClient;
       end;

       tbl.open;

       if bNeedLocate then
        begin
         if Assigned(bk) then if tbl.RecordCount>0 then tbl.GotoBookmark(bk);
         if Assigned(bk) then tbl.FreeBookmark(bk);
        end;

end;

function funDoSinglePrint(
  iSerialNum,
  iPrintType:integer;
  sCurrCond,
  sRealTableNameMas1,
  sItemName,
  sSystemId,
  sBUId,
  sUserId:string;
  qry:TADOQuery;
  sConnectStr:string;
  hMain_btnPrint_Handle:THandle;//2010.11.8 add
  sMainGlobalId:string;
  sReportTitle:widestring //2012.03.30 add for Bill-20120329-04
  ):boolean;
var  sSQL:string;
    rptPaper: TJSdReport;
    CurrReportName,
    sProcName:string;
    qryExec:TADOQuery;
begin
  result:=false;

  if assigned(qry) then qryExec:=qry
  else
  begin
    qryExec:=TADOQuery.Create(nil);
    qryExec.ConnectionString:=sConnectStr;
  end;

    sSQL:='exec CURdOCXPaperInfoRptGet '+
      ''''+sRealTableNameMas1+''''+','+
      inttostr(iSerialNum)+','+
      inttostr(iPrintType);

    with qryExec do
      begin
        close;
        sql.Clear;
        sql.Add(sSQL);
        Open;
      end;

    if qryExec.RecordCount=0 then
      begin
        qryExec.close;
        if not assigned(qry) then qryExec.Free;
        MsgDlgJS('印表設定錯誤',mtWarning,[mbOK],0);
        exit;
      end;

    CurrReportName:= qryExec.FieldByName('ClassName').AsString;
    sProcName:=qryExec.FieldByName('ObjectName').AsString;

    //2010.6.17 add for QR工具
    if ((qryExec.FieldByName('ReportName').AsString<>'') and
        (qryExec.FieldByName('Context').AsString<>'')) then
      begin
        qryExec.Close;
        result:=false;
        exit;
      end;

    try
       rptPaper:= TJSdReport.Create(nil);

      with rptPaper do
        begin
          ReportFileName:= CurrReportName;
          // ReportTitle:= sItemName;
          ReportTitle:= sReportTitle;//2012.03.30 add for Bill-20120329-04
        end;

      case qryExec.FieldByName('LinkType').AsInteger of
        0:rptPaper.LinkType:=ltODBC;//0 ODBC連線
        1:rptPaper.LinkType:=ltBDE;//1 資料檔案.db
        2:rptPaper.LinkType:=ltAccess;//2 資料檔案.mdb
        3:rptPaper.LinkType:=ltExcel;//3 資料檔案.xls
      end;

      qryExec.close;

      RunJSdReportDLL(
              rptPaper,//JsRpt: TJSdReport;
              sProcName,//ProcName: WideString;
              [sCurrCond],//ParamList: array of WideString;
              '',//sIndex,
              CurrReportName,//sRptName: WideString;
              //=====
              qryExec,
              sSystemId,
              sBUId,
              sUserId,
              nil,
              true,
              //=====
              hMain_btnPrint_Handle,//2010.11.8 add
              sMainGlobalId,
              0
              );

    finally
      rptPaper.Free;
      if not assigned(qry) then qryExec.Free;
    end;

  result:=true;
end;

procedure TPopuMenuClick.DLLPopuMenuEvent(Sender: TObject);
var  sSQL:string;
    rptPaper: TJSdReport;
    CurrReportName,
    sProcName:string;
    qryExec:TADOQuery;
    i,j:integer;//2012.04.09 add
    sj:string;//2012.04.09 add
    sCaption:string;//2012.04.09 add
begin
    qryExec:=TADOQuery.Create(nil);
    qryExec.ConnectionString:=TJSdPopupMenu(TMenuItem(Sender).Owner).CURConnString;
{
    sSQL:='exec CURdOCXPaperInfoRptGet '+
      ''''+TJSdPopupMenu((TMenuItem(Sender).Owner)).PaperId+''''+','+
      inttostr(TJSdPopupMenu(TMenuItem(Sender).Owner).Tag)+','+
      inttostr(TJSdPopupMenu(TMenuItem(Sender).Owner).PrintType);

    with qryExec do
      begin
        close;
        sql.Clear;
        sql.Add(sSQL);
        Open;
      end;

    if qryExec.RecordCount=0 then
      begin
        qryExec.close;
        qryExec.free;
        ShowMessage('印表設定錯誤');
        exit;
      end;

    CurrReportName:= qryExec.FieldByName('ClassName').AsString;
    sProcName:=qryExec.FieldByName('ObjectName').AsString;

    try
       rptPaper:= TJSdReport.Create(nil);

      with rptPaper do
        begin
          ReportFileName:= CurrReportName;
           ReportTitle:= '';
        end;

      case qryExec.FieldByName('LinkType').AsInteger of
        0:rptPaper.LinkType:=ltODBC;//0 ODBC連線
        1:rptPaper.LinkType:=ltBDE;//1 資料檔案.db
        2:rptPaper.LinkType:=ltAccess;//2 資料檔案.mdb
        3:rptPaper.LinkType:=ltExcel;//3 資料檔案.xls
      end;

      qryExec.close;

      RunJSdReportDLL(
              rptPaper,//JsRpt: TJSdReport;
              sProcName,//ProcName: WideString;
              [TJSdPopupMenu(TMenuItem(Sender).Owner).CurrCond],//ParamList: array of WideString;
              '',//sIndex,
              CurrReportName,//sRptName: WideString;
              //=====
              qryExec,
              copy(TJSdPopupMenu(TMenuItem(Sender).Owner).PaperId,1,3),
              TJSdPopupMenu(TMenuItem(Sender).Owner).CurrBUId,
              TJSdPopupMenu(TMenuItem(Sender).Owner).CurrUserId,
              nil,
              true
              //=====
              );

    finally
      rptPaper.Free;
      qryExec.free;
    end;
}

    //2012.04.09 add
    sj:=TMenuItem(Sender).Hint;

    for i := 0 to TJSdPopupMenu(TMenuItem(Sender).Owner).Items.Count-1 do
      begin
        if TJSdPopupMenu(TMenuItem(Sender).Owner).Items[i].Hint=sj then
          begin
           sCaption:=TJSdPopupMenu(TMenuItem(Sender).Owner).Items[i].Caption;
           break;
          end;
      end;


    unit_DLL.funDoSinglePrint(
      //TJSdPopupMenu(TMenuItem(Sender).Owner).Tag,//iSerialNum,
      strtoint(unit_DLL.IIFStringDLL(TMenuItem(Sender).Hint='','0',TMenuItem(Sender).Hint)),//iSerialNum,
      TJSdPopupMenu(TMenuItem(Sender).Owner).PrintType,//iPrintType:integer;
      TJSdPopupMenu(TMenuItem(Sender).Owner).CurrCond,//sCurrCond,
      TJSdPopupMenu(TMenuItem(Sender).Owner).PaperId,//sRealTableNameMas1,
      '',//sItemName,
      copy(TJSdPopupMenu(TMenuItem(Sender).Owner).PaperId,1,3),//sSystemId,
      TJSdPopupMenu(TMenuItem(Sender).Owner).CurrBUId,//sBUId,
      TJSdPopupMenu(TMenuItem(Sender).Owner).CurrUserId,//sUserId:string;
      nil,//qryExec:TADOQuery;
      TJSdPopupMenu(TMenuItem(Sender).Owner).CURConnString,
      TJSdPopupMenu(TMenuItem(Sender).Owner).Main_btnPrint_Handle, //2010.11.8 add
      TJSdPopupMenu(TMenuItem(Sender).Owner).MainGlobalId, //2010.11.8 add
      //TJSdPopupMenu(TMenuItem(Sender).Owner).Items[strtoint(unit_DLL.IIFStringDLL(TMenuItem(Sender).Hint='','0',TMenuItem(Sender).Hint)) - 1].Caption //2012.04.03 modify for MUT Bill-20120329-04
      sCaption
      );

end;

function funPrintPaper(
  pmuPaperPaper:TJSdPopupMenu;
  qryBrowse:TJSdTable;
  qryExec:TADOQuery;
  sRealTableNameMas1,
  sConnectStr,
  sBUId,
  sUserId,
  sItemName,
  sSystemId:string;
  btn:TSpeedButton;
  iIsList:integer;
  sNoOrderByMasSQL:string;
  hMain_btnPrint_Handle:THandle;//2010.11.8 add
  sGlobalId,
  sItemId //2012.03.30 add for Bill-20120329-04
  :string
  ):boolean;
var rptPaper: TJSdReport;
    ThisOperate:string;
    CurrReportName:string;
    PopuMenuClick:TPopuMenuClick;
    i:integer;
    sListSQL,
    sProcName,
    sCond:string;
    sReportTitle:widestring;//2012.03.30 add for Bill-20120329-04
    iCount:integer; //2012.04.02 add for Bill-20120329-04
    iKind:integer; //2012.04.02 add for Bill-20120329-04
begin
  result:=false;

    if qryBrowse.Active=false then
       begin
         MsgDlgJS('無資料可供列印',mtError,[mbOk],0);
         exit;
       end;

    if qryBrowse.RecordCount=0 then
       begin
         MsgDlgJS('無資料可供列印',mtError,[mbOk],0);
         exit;
       end;

// 2009.10.15 disable by garfield , for Bill
//  if qryBrowse.FieldbyName('Finished').AsInteger=0 then
//  begin
//    MsgDlgJS('單據作業中，不可列印單據!', mtInformation, [mbOk], 0);
//    Exit;
//  end;

 //2012.1.12 add by garfield for MUT Bill-20111230-01
 //為防範多人同時操作同一單據，須從後端檢查最新的單據狀態
 //若從前端 Refresh DataSet 會觸動事件，影響效能
 if iIsList=0 then
  begin
    OpenSQLDLL(qryExec,'OPEN','exec CURdPaperPrintChk '+
          ''''+sRealTableNameMas1+''''+','+
          ''''+qryBrowse.FieldByName('PaperNum').AsString+''''
          );

    if qryExec.FieldByName('Re').AsString<>'OK' then
      begin
        MsgDlgJS(qryExec.Fields[0].AsString, mtInformation, [mbOk], 0);
        qryExec.Close;
        exit;
      end;
  end;

 with qryExec do
      begin
        close;
        sql.Clear;
        sql.Add('exec CURdPaperPrintList '+''''+sRealTableNameMas1+''''+
          ','+inttostr(iIsList)
          +','+''''+sItemId+''''//2012.03.30 add for Bill-20120329-04
          );
        open;
      end;

 //2012.03.30 add for Bill-20120329-04
 iKind:=0;
 iCount:=0;
 sReportTitle:='';

 //2012.03.30 add for Bill-20120329-04
 if qryExec.RecordCount>0 then
 begin
   sReportTitle:=qryExec.FieldByName('ReportTitle').AsWideString;
   iCount:=qryExec.FieldByName('iCount').AsInteger;
   iKind:=qryExec.FieldByName('iKind').AsInteger;
 end;


 if iIsList=1 then
    sCond:='select * from '+sRealTableNameMas1+'(nolock) where PaperNum in('+
      'select tz.PaperNum from ('+sNoOrderByMasSQL+') tz)'
 else
    sCond:=qryBrowse.FieldByName('PaperNum').AsString;

 if //(qryExec.RecordCount>1)
     (iCount>1) and //2012.04.02 add for MUT Bill-20120329-04
     (iKind in[10,20])//2012.04.02 add for MUT Bill-20120329-04
 then
 begin
  pmuPaperPaper.CURConnString:= sConnectStr;
  pmuPaperPaper.PrintType:=iIsList;
  pmuPaperPaper.PaperId:= sRealTableNameMas1;
  //pmuPaperPaper.CurrReportServer:=unit_DLL.funReportServerGet(qryExec,sBUID);
  pmuPaperPaper.CurrReportServer:=unit_DLL.funReportServerGet(qryExec,sBUID,sGlobalId);//2012.05.22 add for WF Bill-20120518-05

  pmuPaperPaper.CurrPaperNum:= qryBrowse.FieldByName('PaperNum').AsString;
  pmuPaperPaper.CurrCond:= sCond;//qryBrowse.FieldByName('PaperNum').AsString;
  pmuPaperPaper.CurrBUId:= sBUId;
  pmuPaperPaper.CurrUserId:= sUserId;
  pmuPaperPaper.BatchPaper:=false;
  pmuPaperPaper.Main_btnPrint_Handle:=hMain_btnPrint_Handle;//2010.11.8 add
  pmuPaperPaper.MainGlobalId:=sGlobalId;//2010.11.8 add
  pmuPaperPaper.ReportTitle:=sReportTitle;//2012.03.30 add for Bill-20120329-04
  pmuPaperPaper.Setup(btn
    ,sItemId //2012.03.30 add for Bill-20120329-04
    );

    for i := 0 to pmuPaperPaper.Items.Count - 1 do
      begin
        pmuPaperPaper.Items[i].OnClick:=nil;
        PopuMenuClick:=TPopuMenuClick.Create;
        PopuMenuClick.oldOnClick:=pmuPaperPaper.Items.OnClick;
        pmuPaperPaper.Items[i].OnClick:=PopuMenuClick.DLLPopuMenuEvent;
      end;

 end
 else if //(qryExec.RecordCount=1)
       (iCount=1) and //2012.04.02 add for MUT Bill-20120329-04
       (iKind in[10,20])//2012.04.02 add for MUT Bill-20120329-04
 then
 begin
   if unit_DLL.funDoSinglePrint( //2009.12.25 add
        qryExec.FieldByName('SerialNum').asInteger,//0,//iSerialNum,
        iIsList,//iPrintType:integer;
        sCond,//qryBrowse.FieldbyName('PaperNum').AsString,//sCurrCond,
        sRealTableNameMas1,
        sItemName,
        sSystemId,
        sBUId,
        sUserId,//sUserId:string;
        qryExec,//qryExec:TADOQuery;
        sConnectStr,
        hMain_btnPrint_Handle,
        sGlobalId,
        sReportTitle //2012.03.30 add for Bill-20120329-04
        )=false then exit;
 end
 else
 begin
    ThisOperate:= StringReplace(sRealTableNameMas1, 'Main','',[rfReplaceAll, rfIgnoreCase]);

    try
      if iIsList=0 then CurrReportName:= trim(sRealTableNameMas1)+'Paper0'
      else CurrReportName:= trim(sRealTableNameMas1)+'PaperList0';

      if iIsList=0 then sProcName:=trim(ThisOperate)+'Paper'
      else sProcName:=trim(ThisOperate)+'PaperList';

      rptPaper:= TJSdReport.Create(nil);

      with rptPaper do
        begin
          if iIsList=0 then ReportFileName:= trim(ThisOperate)+'Paper.rpt'
          else ReportFileName:= trim(ThisOperate)+'PaperList.rpt';

          //ReportTitle:= sItemName;
          ReportTitle:=sReportTitle; //2012.03.30 add for Bill-20120329-04
        end;

      RunJSdReportDLL(
              rptPaper,//JsRpt: TJSdReport;
              sProcName,//trim(ThisOperate)+'Paper',//ProcName: WideString;
              [sCond],//[qryBrowse.FieldbyName('PaperNum').AsString],//ParamList: array of WideString;
              '',//sIndex,
              CurrReportName,//sRptName: WideString;
              //=====
              qryExec,
              sSystemId,
              sBUId,
              sUserId,
              nil, //AftPrn: TNotifyEVent
              false,
              //=====
              hMain_btnPrint_Handle,
              sGlobalId,
              0
              );
    finally
      rptPaper.Free;
    end;
  end;
end;
//2016.11.21 note:已無用了
function DataSet2ExcelDLL(data:TCustomADODataSet; sdbFile, sTableName, ReportName: string;
   iOpen: integer;sConnectStr:string): Boolean;
var
  Save_Cursor:TCursor;
  XLApp, Sheet, v, vArr: Variant;
  iCol, iColv, jRow: Integer;
  aSubTotal: Array of integer;
  sFieldArray: Array[0..7] of WideString;
  iFontSize, iFontColor, subCount, i: Integer;
  key1, key2, key3, ord1, ord2, ord3: Integer;
  sOrigin, sFontName, sFontColor, sFontStyle, sAutofit: WideString;
begin
  Result := False;
TRY
    data.DisableControls;
    SetReportFieldDLL(data, ReportName, sConnectStr);
    frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(nil);
    frmLoadProgressDLL.Caption:= '資料計算彙整中......';
    frmLoadProgressDLL.show;
    frmLoadProgressDLL.Initialize(1, data.RecordCount);
    Save_Cursor := Screen.Cursor;
    Screen.Cursor := crHourglass;
    XLApp := CreateOLEObject('Excel.Application');
    XLApp.Visible := false;
    //XLApp.Workbooks.Add[XLWBatWorksheet];
    XLApp.Workbooks.Add;//2016.10.13 modify

    //XLApp.Workbooks[1].Worksheets[1].Name := sTableName; //2016.10.13 disable

    //Sheet := XLApp.Workbooks[1].Worksheets[sTableName];
    Sheet := XLApp.Worksheets[1]; //2016.10.13 modify
    jRow := 1;
    iColv:= 0;
    for iCol := 1 to data.FieldCount do
    begin
      if data.Fields[iCol-1].Visible then
      begin
        v := data.Fields[iCol-1].DisplayLabel;
        Inc(iColv);
        Sheet.Cells[jRow, iColv] := VarToStr(v);
      end;
      data.Next;
    end;

    //XLApp.Workbooks[1].WorkSheets[sTableName].Rows.Item[1].Font.Bold := True; //2016.10.13 disable
    data.First;
    Inc(jRow);
    while not data.EOF do
    begin
      iColv:= 0;
      for iCol := 1 to data.FieldCount do
      begin
        if data.Fields[iCol-1].Visible then
        begin
          v := data.Fields[iCol-1].Value;
          Inc(iColv);

          //if ((data.Fields[iCol-1] is TStringField)
          //   or (data.Fields[iCol-1] is TWideStringField)) then
          //  Sheet.Cells[jRow, iColv].NumberFormatLocal := '@';

          if data.Fields[iCol-1].DataType
            in[ftString,ftWideString,ftMemo,ftFmtMemo,ftWideMemo,ftFixedChar,ftFixedWideChar] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('@');

          //2020.07.08 add for SS
          if data.Fields[iCol-1].DataType
            in[ftSmallint,ftInteger] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('#,###');
          if data.Fields[iCol-1].DataType
            in[ftFloat,ftCurrency,ftBCD] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=tfloatfield (data.Fields[iCol-1]).DisplayFormat;

          Sheet.Cells[jRow, iColv] := VarToStr(v);
        end;
      end;
      Inc(jRow);
      data.Next;
      frmLoadProgressDLL.Add;
      frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(jRow) + ' 筆資料';
      frmLoadProgressDLL.pnlStatus.Refresh;
    end;

    key1:=-1; key2:=-1; key3:=-1;
    Ord1:=-1; Ord2:=-1; Ord3:=-1;
    subCount:= 0;
    iColv:= 0;

    for iCol := 1 to data.FieldCount do
    begin
       if data.Fields[iCol-1].Visible then
       begin
          Inc(iColv);
          sOrigin:= data.Fields[iCol-1].Origin;
          ParseFormatWDLL(sOrigin, ';', sFieldArray, 8);

          if sFieldArray[0]='' then
             sFontName:= '細明體'
          else
             sFontName:= sFieldArray[0];
          if sFieldArray[1]<>'' then
             iFontSize:= strtoint(sFieldArray[1])
          else
             iFontSize:= 9;
          if not ((iFontSize>=8) and (iFontSize<=72)) then
             iFontSize:= 9;

          if not IdentToColor(sFieldArray[2], iFontColor) then
             sFontColor:= 'clBlack'
          else
             sFontColor:= sFieldArray[2];

          sFontStyle:= sFieldArray[3];
{2016.10.13 disable
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Name := sFontName;
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Color := StringToColor(sFontColor);
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Size := iFontSize;
          //'Bold', 'Italic', 'Underline', 'StrikeOut'
          if Ansipos('bsBold', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Bold := True;
          if Ansipos('bsItalic', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Italic := True;
          if Ansipos('bsUnderline', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.UnderLine := True;
          if Ansipos('bsStrikeOut', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.StrikeOut := True;
          //****sSubTotal:=[4];sReportGroup:=[5];sAutofit:=[6];sOrderBy:=[7]
}
          if (sFieldArray[4]='1') then
          begin
            SetLength(aSubTotal, SubCount+1);
            aSubTotal[subCount]:= iColv;
            subCount:= subCount+1;
          end;
          //Group
          if (key1=-1) then
          begin
            if (sFieldArray[5]='1') then
            begin
               key1:= iColv;
               //昇冪=1&降冪=2
               ord1:= strtoint(sFieldArray[7])+1;
            end;
          end;
          if (key2=-1) then
          begin
            if (sFieldArray[5]='2') then
            begin
               key2:= iColv;
               ord2:= strtoint(sFieldArray[7])+1;
            end;
          end;
          if (key3=-1) then
          begin
            if (sFieldArray[5]='3') then
            begin
               key3:= iColv;
               ord3:= strtoint(sFieldArray[7])+1;
            end;
          end;
{2016.10.13 disable
          //Autofit
          if sFieldArray[6]='1' then
          begin
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].EntireColumn.Autofit;
          end
          else
          begin
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].ColumnWidth:= data.Fields[iCol-1].DisplayWidth;
          end;}
       end;
    end;
    XLApp.DisplayAlerts := False;
    //排序  //小計
    if (data.RecordCount>0) then
    begin
      if Ord1=-1 then Ord1:=1;
      if Ord2=-1 then Ord2:=1;
      if Ord3=-1 then Ord3:=1;
      if subCount>0 then
      begin
          vArr:= VarArrayCreate([0, subCount-1], varSmallint);
          for i := 0 to subCount - 1 do
              vArr[i]:= aSubTotal[i];

          if (key1>0) and (key2=-1) and (key3=-1) then
          begin   {2016.10.13 disable
              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=True, PageBreaks:=False, SummaryBelowData:=True);
              }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify         //2020.07.08
          end
          else if (key1>0) and (key2>0) and (key3=-1) then
          begin  { 2016.10.13 disable
              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
                Order2:=Ord2,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key2, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);
              }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify        //2020.07.08
          end
          else if (key1>0) and (key2>0) and (key3>0) then
          begin {2016.10.13 disable
              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
                Order2:=Ord2,
                Key3:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key3)+'1'],
                Order3:=Ord3,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key2, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key3, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);
              }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify        //2020.07.08 for SS
          end;
        end
        else
        begin
            if key1=-1 then key1:=1;
            if key2=-1 then key2:=1;
            if key3=-1 then key3:=1; {2016.10.13 disable
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
              Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
              Order1:=Ord1,
              Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
              Order2:=Ord2,
              Key3:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key3)+'1'],
              Order3:=Ord3,
              Header:=1,
              OrderCustom:=1,
              MatchCase:=False,
              Orientation:=1);}
        end;
    end;
    XLApp.Visible := iOpen = 0;
FINALLY
    if iOpen = 1 then
    begin
      //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
      XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify           //2020.07.08 for SS
      XLApp.quit;
    end;
    if assigned(frmLoadProgressDLL) then
    begin
      frmLoadProgressDLL.free;
    end;
    Screen.Cursor := Save_Cursor;
    data.First;
    data.EnableControls;
END;
end;

//2012.05.09 add for MUT Bill-20120509-01
function DataSet2ExcelDLL2(data:TCustomADODataSet; sdbFile, sTableName, ReportName: string;
   iOpen: integer;sConnectStr:string;qryExec:TADOQuery): Boolean;
var
  Save_Cursor:TCursor;
  XLApp, Sheet, v, vArr: Variant;
  iCol, iColv, jRow: Integer;
  aSubTotal: Array of integer;
  sFieldArray: Array[0..7] of WideString;
  iFontSize, iFontColor, subCount, i: Integer;
  key1, key2, key3, ord1, ord2, ord3: Integer;
  sOrigin, sFontName, sFontColor, sFontStyle, sAutofit: WideString;
begin
  Result := False;
TRY
    data.DisableControls;

    SetReportFieldDLL2(data, ReportName, sConnectStr,qryExec);

    frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(nil);
    frmLoadProgressDLL.Caption:= '資料計算彙整中......';
    frmLoadProgressDLL.show;
    frmLoadProgressDLL.Initialize(1, data.RecordCount);
    Save_Cursor := Screen.Cursor;
    Screen.Cursor := crHourglass;
    XLApp := CreateOLEObject('Excel.Application');
    XLApp.Visible := false;
    //XLApp.Workbooks.Add[XLWBatWorksheet];
    XLApp.Workbooks.Add;//2016.10.13 modify
    //XLApp.Workbooks[1].Worksheets[1].Name := sTableName; //2016.10.13 disable
    //Sheet := XLApp.Workbooks[1].Worksheets[sTableName];
    Sheet := XLApp.Worksheets[1]; //2016.10.13 modify
    jRow := 1;
    iColv:= 0;
    for iCol := 1 to data.FieldCount do
    begin
      if data.Fields[iCol-1].Visible then
      begin
        v := data.Fields[iCol-1].DisplayLabel;
        Inc(iColv);
        Sheet.Cells[jRow, iColv] := VarToStr(v);
      end;
      data.Next;
    end;

    //XLApp.Workbooks[1].WorkSheets[sTableName].Rows.Item[1].Font.Bold := True; //2016.10.13 disable
    data.First;
    Inc(jRow);
    while not data.EOF do
    begin
      iColv:= 0;
      for iCol := 1 to data.FieldCount do
      begin
        if data.Fields[iCol-1].Visible then
        begin
          v := data.Fields[iCol-1].Value;
          Inc(iColv);

          //if ((data.Fields[iCol-1] is TStringField)
          //   or (data.Fields[iCol-1] is TWideStringField)) then
          //  Sheet.Cells[jRow, iColv].NumberFormatLocal := '@';

          if data.Fields[iCol-1].DataType
            in[ftString,ftWideString,ftMemo,ftFmtMemo,ftWideMemo,ftFixedChar,ftFixedWideChar] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('@');
                //2020.06.20   新增千分位
        if data.Fields[iCol-1].DataType
            in[ftSmallint,ftInteger] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('#,###');
        if data.Fields[iCol-1].DataType
            in[ftFloat,ftCurrency,ftBCD] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=tfloatfield (data.Fields[iCol-1]).DisplayFormat;

          Sheet.Cells[jRow, iColv] := VarToStr(v);
        end;
      end;
      Inc(jRow);
      data.Next;
      frmLoadProgressDLL.Add;
      frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(jRow) + ' 筆資料';
      frmLoadProgressDLL.pnlStatus.Refresh;
    end;

    key1:=-1; key2:=-1; key3:=-1;
    Ord1:=-1; Ord2:=-1; Ord3:=-1;
    subCount:= 0;
    iColv:= 0;

    for iCol := 1 to data.FieldCount do
    begin
       if data.Fields[iCol-1].Visible then
       begin
          Inc(iColv);
          sOrigin:= data.Fields[iCol-1].Origin;
          ParseFormatWDLL(sOrigin, ';', sFieldArray, 8);

          if sFieldArray[0]='' then
             sFontName:= '細明體'
          else
             sFontName:= sFieldArray[0];
          if sFieldArray[1]<>'' then
             iFontSize:= strtoint(sFieldArray[1])
          else
             iFontSize:= 9;
          if not ((iFontSize>=8) and (iFontSize<=72)) then
             iFontSize:= 9;

          if not IdentToColor(sFieldArray[2], iFontColor) then
             sFontColor:= 'clBlack'
          else
             sFontColor:= sFieldArray[2];

          sFontStyle:= sFieldArray[3];
{//2016.10.13 disable
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Name := sFontName;
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Color := StringToColor(sFontColor);
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Size := iFontSize;
          //'Bold', 'Italic', 'Underline', 'StrikeOut'
          if Ansipos('fsBold', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Bold := True;
          if Ansipos('fsItalic', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Italic := True;
          if Ansipos('fsUnderline', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.UnderLine := True;
          if Ansipos('fsStrikeOut', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.StrikeOut := True;
          //****sSubTotal:=[4];sReportGroup:=[5];sAutofit:=[6];sOrderBy:=[7]
}          if (sFieldArray[4]='1') then
          begin
            SetLength(aSubTotal, SubCount+1);
            aSubTotal[subCount]:= iColv;
            subCount:= subCount+1;
          end;
          //Group
          if (key1=-1) then
          begin
            if (sFieldArray[5]='1') then
            begin
               key1:= iColv;
               //昇冪=1&降冪=2
               ord1:= strtoint(sFieldArray[7])+1;
            end;
          end;
          if (key2=-1) then
          begin
            if (sFieldArray[5]='2') then
            begin
               key2:= iColv;
               ord2:= strtoint(sFieldArray[7])+1;
            end;
          end;
          if (key3=-1) then
          begin
            if (sFieldArray[5]='3') then
            begin
               key3:= iColv;
               ord3:= strtoint(sFieldArray[7])+1;
            end;
          end;
{//2016.10.13 disable
          //Autofit
          if sFieldArray[6]='1' then
          begin
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].EntireColumn.Autofit;
          end
          else
          begin
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].ColumnWidth:= data.Fields[iCol-1].DisplayWidth;
          end;}
       end;
    end;
    XLApp.DisplayAlerts := False;
    //排序  //小計
    if (data.RecordCount>0) then
    begin
      if Ord1=-1 then Ord1:=1;
      if Ord2=-1 then Ord2:=1;
      if Ord3=-1 then Ord3:=1;
      if subCount>0 then
      begin
          vArr:= VarArrayCreate([0, subCount-1], varSmallint);
          for i := 0 to subCount - 1 do
              vArr[i]:= aSubTotal[i];

          if (key1>0) and (key2=-1) and (key3=-1) then
          begin  {//2016.10.13 disable              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=True, PageBreaks:=False, SummaryBelowData:=True);
             }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify        //2020.07.08 for SS
          end
          else if (key1>0) and (key2>0) and (key3=-1) then
          begin  {//2016.10.13 disable
              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
                Order2:=Ord2,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key2, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);
              }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify       //2020.07.08 for SS
          end
          else if (key1>0) and (key2>0) and (key3>0) then
          begin {//2016.10.13 disable
              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
                Order2:=Ord2,
                Key3:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key3)+'1'],
                Order3:=Ord3,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key2, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key3, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);
              }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify       //2020.07.08 for SS
          end;
        end
        else
        begin
            if key1=-1 then key1:=1;
            if key2=-1 then key2:=1;
            if key3=-1 then key3:=1;   {//2016.10.13 disable
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
              Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
              Order1:=Ord1,
              Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
              Order2:=Ord2,
              Key3:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key3)+'1'],
              Order3:=Ord3,
              Header:=1,
              OrderCustom:=1,
              MatchCase:=False,
              Orientation:=1);}
        end;
    end;
    XLApp.Visible := iOpen = 0;
FINALLY
    if iOpen = 1 then
    begin
      //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
      XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify            //2020.07.08 for SS
      XLApp.quit;
    end;
    if assigned(frmLoadProgressDLL) then
    begin
      frmLoadProgressDLL.free;
    end;
    Screen.Cursor := Save_Cursor;
    data.First;
    data.EnableControls;
END;
end;

//2020.07.06 add
function DataSet2ExcelDLL_TCI(data:TCustomADODataSet; sdbFile, sTableName, ReportName: string;
   iOpen: integer;sConnectStr:string;qryExec:TADOQuery): Boolean;
var
  Save_Cursor:TCursor;
  XLApp, Sheet, v, vArr: Variant;
  iCol, iColv, jRow: Integer;
  aSubTotal: Array of integer;
  sFieldArray: Array[0..7] of WideString;
  iFontSize, iFontColor, subCount,iGroupNum,  i: Integer;  //2020.07.06 add iGroupNum
  key1, key2, key3, ord1, ord2, ord3: Integer;
  sOrigin, sFontName, sFontColor, sFontStyle, sAutofit: WideString;

begin
  Result := False;
TRY
    data.DisableControls;

    SetReportFieldDLL2(data, ReportName, sConnectStr,qryExec);

    frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(nil);
    frmLoadProgressDLL.Caption:= '資料計算彙整中......';
    frmLoadProgressDLL.show;
    frmLoadProgressDLL.Initialize(1, data.RecordCount);
    Save_Cursor := Screen.Cursor;
    Screen.Cursor := crHourglass;
    XLApp := CreateOLEObject('Excel.Application');
    XLApp.Visible := false;
    //XLApp.Workbooks.Add[XLWBatWorksheet];

     XLApp.Workbooks.Add;//2016.10.13 modify
    //XLApp.Workbooks[1].Worksheets[1].Name := sTableName; //2016.10.13 disable
    //Sheet := XLApp.Workbooks[1].Worksheets[sTableName];


    for iGroupNum := 1 to 5 do//for start
    Begin
    
    if iGroupNum >1 then
      XLApp.Workbooks[1].sheets.add;
      
    Sheet := XLApp.Worksheets[1];

    if iGroupNum = 1 then
    Sheet.Name:= 'PCB成品商品'
    else if iGroupNum = 2 then
    Sheet.Name:= 'BGA成品'
    else if iGroupNum = 3 then
    Sheet.Name:= '五金消耗品'
    else if iGroupNum = 4 then
    Sheet.Name:= '物料'  
    else if iGroupNum = 5 then
    Sheet.Name := '材料';
       
    data.Close;
    data.Filter:='GroupNum='+(InttoStr(iGroupNum));
    data.Filtered:=true;
    data.Open;
    
    jRow := 1;
    iColv:= 0;
    for iCol := 1 to data.FieldCount do
    begin
      if data.Fields[iCol-1].Visible then
      begin
        v := data.Fields[iCol-1].DisplayLabel;
        Inc(iColv);
        Sheet.Cells[jRow, iColv] := VarToStr(v);
      end;
      data.Next;
    end;

    //XLApp.Workbooks[1].WorkSheets[sTableName].Rows.Item[1].Font.Bold := True; //2016.10.13 disable
    data.First;
    Inc(jRow);
    while not data.EOF do
    begin
      iColv:= 0;
      for iCol := 1 to data.FieldCount do
      begin
        if data.Fields[iCol-1].Visible then
        begin
          v := data.Fields[iCol-1].Value;
          Inc(iColv);

          //if ((data.Fields[iCol-1] is TStringField)
          //   or (data.Fields[iCol-1] is TWideStringField)) then
          //  Sheet.Cells[jRow, iColv].NumberFormatLocal := '@';

          if data.Fields[iCol-1].DataType
            in[ftString,ftWideString,ftMemo,ftFmtMemo,ftWideMemo,ftFixedChar,ftFixedWideChar] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('@');
                //2020.06.20   新增千分位
        if data.Fields[iCol-1].DataType
            in[ftSmallint,ftInteger] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('#,###');
        if data.Fields[iCol-1].DataType
            in[ftFloat,ftCurrency,ftBCD] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=tfloatfield (data.Fields[iCol-1]).DisplayFormat;

          Sheet.Cells[jRow, iColv] := VarToStr(v);
        end;
      end;
      Inc(jRow);
      data.Next;
      frmLoadProgressDLL.Add;
      frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(jRow) + ' 筆資料';
      frmLoadProgressDLL.pnlStatus.Refresh;
    end;

    key1:=-1; key2:=-1; key3:=-1;
    Ord1:=-1; Ord2:=-1; Ord3:=-1;
    subCount:= 0;
    iColv:= 0;

    for iCol := 1 to data.FieldCount do
    begin
       if data.Fields[iCol-1].Visible then
       begin
          Inc(iColv);
          sOrigin:= data.Fields[iCol-1].Origin;
          ParseFormatWDLL(sOrigin, ';', sFieldArray, 8);

          if sFieldArray[0]='' then
             sFontName:= '細明體'
          else
             sFontName:= sFieldArray[0];
          if sFieldArray[1]<>'' then
             iFontSize:= strtoint(sFieldArray[1])
          else
             iFontSize:= 9;
          if not ((iFontSize>=8) and (iFontSize<=72)) then
             iFontSize:= 9;

          if not IdentToColor(sFieldArray[2], iFontColor) then
             sFontColor:= 'clBlack'
          else
             sFontColor:= sFieldArray[2];

          sFontStyle:= sFieldArray[3];
{//2016.10.13 disable
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Name := sFontName;
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Color := StringToColor(sFontColor);
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Size := iFontSize;
          //'Bold', 'Italic', 'Underline', 'StrikeOut'
          if Ansipos('fsBold', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Bold := True;
          if Ansipos('fsItalic', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Italic := True;
          if Ansipos('fsUnderline', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.UnderLine := True;
          if Ansipos('fsStrikeOut', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.StrikeOut := True;
          //****sSubTotal:=[4];sReportGroup:=[5];sAutofit:=[6];sOrderBy:=[7]
}          if (sFieldArray[4]='1') then
          begin
            SetLength(aSubTotal, SubCount+1);
            aSubTotal[subCount]:= iColv;
            subCount:= subCount+1;
          end;
          //Group
          if (key1=-1) then
          begin
            if (sFieldArray[5]='1') then
            begin
               key1:= iColv;
               //昇冪=1&降冪=2
               ord1:= strtoint(sFieldArray[7])+1;
            end;
          end;
          if (key2=-1) then
          begin
            if (sFieldArray[5]='2') then
            begin
               key2:= iColv;
               ord2:= strtoint(sFieldArray[7])+1;
            end;
          end;
          if (key3=-1) then
          begin
            if (sFieldArray[5]='3') then
            begin
               key3:= iColv;
               ord3:= strtoint(sFieldArray[7])+1;
            end;
          end;
{//2016.10.13 disable
          //Autofit
          if sFieldArray[6]='1' then
          begin
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].EntireColumn.Autofit;
          end
          else
          begin
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].ColumnWidth:= data.Fields[iCol-1].DisplayWidth;
          end;}
       end;
    end;
    XLApp.DisplayAlerts := False;
    //排序  //小計
    if (data.RecordCount>0) then
    begin
      if Ord1=-1 then Ord1:=1;
      if Ord2=-1 then Ord2:=1;
      if Ord3=-1 then Ord3:=1;
      if subCount>0 then
      begin
          vArr:= VarArrayCreate([0, subCount-1], varSmallint);
          for i := 0 to subCount - 1 do
              vArr[i]:= aSubTotal[i];

          if (key1>0) and (key2=-1) and (key3=-1) then
          begin  {//2016.10.13 disable              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=True, PageBreaks:=False, SummaryBelowData:=True);
             }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify      //2020.07.08 for SS
          end
          else if (key1>0) and (key2>0) and (key3=-1) then
          begin  {//2016.10.13 disable
              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
                Order2:=Ord2,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key2, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);
              }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify       //2020.07.08 for SS
          end
          else if (key1>0) and (key2>0) and (key3>0) then
          begin {//2016.10.13 disable
              XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
                Order2:=Ord2,
                Key3:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key3)+'1'],
                Order3:=Ord3,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key2, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key3, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);
              }
              //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
              XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify       //2020.07.08 for SS
          end;
        end
        else
        begin
            if key1=-1 then key1:=1;
            if key2=-1 then key2:=1;
            if key3=-1 then key3:=1;   {//2016.10.13 disable
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
              Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
              Order1:=Ord1,
              Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
              Order2:=Ord2,
              Key3:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key3)+'1'],
              Order3:=Ord3,
              Header:=1,
              OrderCustom:=1,
              MatchCase:=False,
              Orientation:=1);}
        end;
    end;
    end; //for end
    XLApp.Visible := iOpen = 0;
    data.Close;
    data.Filter:='';
    data.Filtered:=false;
    data.Open;
FINALLY
    if iOpen = 1 then
    begin
      //XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
      XLApp.ActiveWorkbook.SaveAs(sdbFile,51);//2016.10.14 modify     //2020.07.08
      XLApp.quit;
    end;
    if assigned(frmLoadProgressDLL) then
    begin
      frmLoadProgressDLL.free;
    end;
    Screen.Cursor := Save_Cursor;
    data.First;
    data.EnableControls;
END;
end;

//2013.01.07 add MU
function DataSet2ExcelDLL_EMO(data:TCustomADODataSet; sdbFile, sTableName, ReportName: string;
   iOpen: integer;sConnectStr:string;qryExec:TADOQuery): Boolean;
var
  Save_Cursor:TCursor;
  XLApp, Sheet, v, vArr: Variant;
  iCol, iColv, jRow: Integer;
  aSubTotal: Array of integer;
  sFieldArray: Array[0..7] of WideString;
  iFontSize, iFontColor, subCount, i: Integer;
  key1, key2, key3, ord1, ord2, ord3: Integer;
  sOrigin, sFontName, sFontColor, sFontStyle, sAutofit: WideString;
  //2013.01.07
  RowProcI, TmpBegin, TmpEnd, iDoRowArray, RowArrayI: Integer;
  sTmpBegin, sTmpEnd, sTotal_1, sTotal_2: String;
  RowBeginArray: Array[0..100] of integer;
  RowEndArray: Array[0..100] of integer;
begin
  Result := False;
TRY
    data.DisableControls;

    SetReportFieldDLL2(data, ReportName, sConnectStr,qryExec);

    frmLoadProgressDLL:= TfrmLoadProgressDLL.Create(nil);
    frmLoadProgressDLL.Caption:= '資料計算彙整中......';
    frmLoadProgressDLL.show;
    frmLoadProgressDLL.Initialize(1, data.RecordCount);
    Save_Cursor := Screen.Cursor;
    Screen.Cursor := crHourglass;
    XLApp := CreateOLEObject('Excel.Application');
    XLApp.Visible := false;
    XLApp.Workbooks.Add[XLWBatWorksheet];
    XLApp.Workbooks[1].Worksheets[1].Name := sTableName;
    Sheet := XLApp.Workbooks[1].Worksheets[sTableName];
    jRow := 1;
    iColv:= 0;
    //Title
    for iCol := 1 to data.FieldCount do
    begin
      if data.Fields[iCol-1].Visible then
      begin
        v := data.Fields[iCol-1].DisplayLabel;
        Inc(iColv);
        //Sheet.Cells[jRow, iColv] := VarToStr(v); 2013.01.07 No Title
      end;
      data.Next;
    end;

    //2013.01.07 No Title
    //XLApp.Workbooks[1].WorkSheets[sTableName].Rows.Item[1].Font.Bold := True;
    RowProcI:=1;
    data.First;
    //Inc(jRow); 2013.01.07 No Title
    while not data.EOF do
    begin
      iColv:= 0;
      //2013.01.07
      iDoRowArray:=0;
      for iCol := 1 to data.FieldCount do
      begin
        //2013.01.07
        //======================================================================
        if data.Fields[iCol-1].FieldName='ProcGroup' then
        begin
          if data.Fields[iCol-1].AsInteger=RowProcI then
            iDoRowArray:=1;
        end else if data.Fields[iCol-1].FieldName='RowBegin' then
        begin
          TmpBegin:=data.Fields[iCol-1].AsInteger;
        end else if data.Fields[iCol-1].FieldName='RowEnd' then
        begin
          TmpEnd:=data.Fields[iCol-1].AsInteger;
        end;
        //======================================================================
        if data.Fields[iCol-1].Visible then
        begin
          v := data.Fields[iCol-1].Value;
          Inc(iColv);

          //if ((data.Fields[iCol-1] is TStringField)
          //   or (data.Fields[iCol-1] is TWideStringField)) then
          //  Sheet.Cells[jRow, iColv].NumberFormatLocal := '@';

          if data.Fields[iCol-1].DataType
            in[ftString,ftWideString,ftMemo,ftFmtMemo,ftWideMemo,ftFixedChar,ftFixedWideChar] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('@');

          //2020.07.08 add for SS
          if data.Fields[iCol-1].DataType
            in[ftSmallint,ftInteger] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=vartostr('#,###');
          if data.Fields[iCol-1].DataType
            in[ftFloat,ftCurrency,ftBCD] then
             Sheet.Cells[jRow, iColv].NumberFormatLocal :=tfloatfield (data.Fields[iCol-1]).DisplayFormat;

          Sheet.Cells[jRow, iColv] := VarToStr(v);
        end;
      end;
      //2013.01.07
      //========================================================================
      if iDoRowArray=1 then
      begin
        RowBeginArray[RowProcI-1]:=TmpBegin;
        RowEndArray[RowProcI-1]:= TmpEnd;
        RowProcI:=RowProcI +1;
      end;
      //========================================================================
      Inc(jRow);
      data.Next;
      frmLoadProgressDLL.Add;
      frmLoadProgressDLL.pnlStatus.Caption := '目前正在轉換第 ' + IntToStr(jRow) + ' 筆資料';
      frmLoadProgressDLL.pnlStatus.Refresh;
    end;

    key1:=-1; key2:=-1; key3:=-1;
    Ord1:=-1; Ord2:=-1; Ord3:=-1;
    subCount:= 0;
    iColv:= 0;

    for iCol := 1 to data.FieldCount do
    begin
       if data.Fields[iCol-1].Visible then
       begin
          Inc(iColv);
          sOrigin:= data.Fields[iCol-1].Origin;
          ParseFormatWDLL(sOrigin, ';', sFieldArray, 8);

          if sFieldArray[0]='' then
             sFontName:= '細明體'
          else
             sFontName:= sFieldArray[0];
          if sFieldArray[1]<>'' then
             iFontSize:= strtoint(sFieldArray[1])
          else
             iFontSize:= 9;
          if not ((iFontSize>=8) and (iFontSize<=72)) then
             iFontSize:= 9;

          if not IdentToColor(sFieldArray[2], iFontColor) then
             sFontColor:= 'clBlack'
          else
             sFontColor:= sFieldArray[2];

          sFontStyle:= sFieldArray[3];

          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Name := sFontName;
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Color := StringToColor(sFontColor);
          XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Size := iFontSize;
          //'Bold', 'Italic', 'Underline', 'StrikeOut'
          if Ansipos('fsBold', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Bold := True;
          if Ansipos('fsItalic', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.Italic := True;
          if Ansipos('fsUnderline', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.UnderLine := True;
          if Ansipos('fsStrikeOut', sFontStyle)>0 then
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].Font.StrikeOut := True;
          //****sSubTotal:=[4];sReportGroup:=[5];sAutofit:=[6];sOrderBy:=[7]
          if (sFieldArray[4]='1') then
          begin
            SetLength(aSubTotal, SubCount+1);
            aSubTotal[subCount]:= iColv;
            subCount:= subCount+1;
          end;
          //Group
          if (key1=-1) then
          begin
            if (sFieldArray[5]='1') then
            begin
               key1:= iColv;
               //昇冪=1&降冪=2
               ord1:= strtoint(sFieldArray[7])+1;
            end;
          end;
          if (key2=-1) then
          begin
            if (sFieldArray[5]='2') then
            begin
               key2:= iColv;
               ord2:= strtoint(sFieldArray[7])+1;
            end;
          end;
          if (key3=-1) then
          begin
            if (sFieldArray[5]='3') then
            begin
               key3:= iColv;
               ord3:= strtoint(sFieldArray[7])+1;
            end;
          end;
          //Autofit
          if sFieldArray[6]='1' then
          begin
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].EntireColumn.Autofit;
          end
          else
          begin
            XLApp.Workbooks[1].WorkSheets[sTableName].Columns[iColv].ColumnWidth:= data.Fields[iCol-1].DisplayWidth;
          end;
       end;
    end;
    XLApp.DisplayAlerts := False;
    //排序  //小計
    if (data.RecordCount>0) then
    begin
      //2013.01.07
      XLApp.ActiveSheet.Columns[2].WrapText:=True;
      XLApp.ActiveSheet.Range['A1:G1'].Merge;
      XLApp.ActiveSheet.Range['A2:G2'].Merge;
      XLApp.ActiveSheet.Range['C3:D3'].Merge;
      XLApp.ActiveSheet.Range['F3:G3'].Merge;
      XLApp.ActiveSheet.Rows.Item[1].Font.Bold := True;
      XLApp.ActiveSheet.Rows.Item[2].Font.Bold := True;
      XLApp.ActiveSheet.Rows.Item[1].Font.Size := 16;
      XLApp.ActiveSheet.Rows.Item[2].Font.Size := 16;
      XLApp.ActiveSheet.Cells[3,2].Font.Size := 16;
      XLApp.ActiveSheet.Cells[3,5].Font.Size := 16;
      XLApp.ActiveSheet.Rows.Item[1].Font.UnderLine := True;
      XLApp.ActiveSheet.Rows.Item[2].Font.UnderLine := True;
      XLApp.ActiveSheet.Rows.Item[1].HorizontalAlignment := xlCenter;
      XLApp.ActiveSheet.Rows.Item[2].HorizontalAlignment := xlCenter;
      XLApp.ActiveSheet.Rows.Item[3].HorizontalAlignment := xlCenter;
      XLApp.ActiveSheet.Columns[5].HorizontalAlignment := xlLeft;
      XLApp.ActiveSheet.Columns[1].HorizontalAlignment := xlCenter;
      XLApp.ActiveSheet.Rows.Item[4].HorizontalAlignment := xlCenter;
      XLApp.ActiveSheet.Rows.Item[5].HorizontalAlignment := xlCenter;
      XLApp.ActiveSheet.Range['A4:A5'].Merge;
      XLApp.ActiveSheet.Range['B4:B5'].Merge;
      XLApp.ActiveSheet.Range['C4:D4'].Merge;
      XLApp.ActiveSheet.Range['E4:E5'].Merge;
      XLApp.ActiveSheet.Range['F4:F5'].Merge;
      XLApp.ActiveSheet.Range['G4:G5'].Merge;
      for RowArrayI := 0 to RowProcI - 2 do
      begin
        sTmpBegin:=IntToStr(RowBeginArray[RowArrayI]);
        sTmpEnd:=IntToStr(RowEndArray[RowArrayI]);
        //ShowMessage(IntToStr(RowArrayI+1)+','+sTmpBegin+','+sTmpEnd);
        XLApp.ActiveSheet.Range['A'+sTmpBegin+':A'+sTmpEnd].Merge;
        XLApp.ActiveSheet.Range['C'+sTmpBegin+':C'+sTmpEnd].Merge;
        XLApp.ActiveSheet.Range['D'+sTmpBegin+':D'+sTmpEnd].Merge;
        XLApp.ActiveSheet.Range['E'+sTmpBegin+':E'+sTmpEnd].Merge;
        XLApp.ActiveSheet.Range['F'+sTmpBegin+':F'+sTmpEnd].Merge;
        XLApp.ActiveSheet.Range['G'+sTmpBegin+':G'+sTmpEnd].Merge;
      end;
      sTotal_1:= IntToStr(data.RecordCount-3);
      XLApp.ActiveSheet.Rows.Item[sTotal_1].HorizontalAlignment := xlLeft;
      sTotal_1:= IntToStr(data.RecordCount-4);
      sTotal_2:= IntToStr(data.RecordCount);
      XLApp.ActiveSheet.Range['A'+sTotal_1+':D'+sTotal_2].Merge;
      XLApp.ActiveSheet.Range['E'+sTotal_1+':E'+sTotal_2].Merge;
      XLApp.ActiveSheet.Range['F'+sTotal_1+':G'+sTotal_2].Merge;

      //2013.01.10 Draw Line
      sTotal_1:= IntToStr(data.RecordCount-5);
      XLApp.ActiveSheet.Range['A4:G'+sTotal_1].Borders[1].LineStyle := xlContinuous;
      XLApp.ActiveSheet.Range['A4:G'+sTotal_1].Borders[2].LineStyle := xlContinuous;
      XLApp.ActiveSheet.Range['A4:G'+sTotal_1].Borders[3].LineStyle := xlContinuous;
      XLApp.ActiveSheet.Range['A4:G'+sTotal_1].Borders[4].LineStyle := xlContinuous;
      sTotal_1:= IntToStr(data.RecordCount-4);
      XLApp.ActiveSheet.Range['A'+sTotal_1+':A'+sTotal_2].Borders[1].LineStyle := xlContinuous;
      XLApp.ActiveSheet.Range['G'+sTotal_1+':G'+sTotal_2].Borders[2].LineStyle := xlContinuous;
      XLApp.ActiveSheet.Range['A'+sTotal_2+':G'+sTotal_2].Borders[4].LineStyle := xlContinuous;
      for RowArrayI := 0 to RowProcI - 2 do
      begin
        sTmpBegin:=IntToStr(RowBeginArray[RowArrayI]);
        XLApp.ActiveSheet.Range['A'+sTmpBegin+':G'+sTmpBegin].Borders[3].LineStyle
          := xlDouble;
      end;
      {邊框類型 Borders[N]
      xlEdgeLeft 左=1
      xlEdgeRight 右=2
      xlEdgeTop 上=3
      xlEdgeBottom 下=4
      xlDiagonalUp 左上右下=5
      xlDiagonalDown 左下右上=6
      xlEdgeLeft 外部左邊框=7
      xlEdgeTop 外部上邊框=8
      xlEdgeBottom 外部下邊框=9
      xlEdgeRight 外部右邊框=10
      xlInsideVertical 內部直線=11
      xlInsideHorizontal 內部橫線=12
      (其中1：左 2：右 3：上  4：下  5：斜\ 6：斜/)

      線條類型LineStyle,寬度Weight
      單線條的LineStyle := xlContinuous
      雙線條的LineStyle := xlDouble
      虛線 xlHairline 1
      實線 xlThin
      中實線 xlMedium
      粗實線 xlThick }

      if Ord1=-1 then Ord1:=1;
      if Ord2=-1 then Ord2:=1;
      if Ord3=-1 then Ord3:=1;
      if subCount>0 then
      begin
          vArr:= VarArrayCreate([0, subCount-1], varSmallint);
          for i := 0 to subCount - 1 do
              vArr[i]:= aSubTotal[i];

          if (key1>0) and (key2=-1) and (key3=-1) then
          begin
              {XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);}

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=True, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
          end
          else if (key1>0) and (key2>0) and (key3=-1) then
          begin
              {XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
                Order2:=Ord2,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);}

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key2, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
          end
          else if (key1>0) and (key2>0) and (key3>0) then
          begin
              {XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
                Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
                Order1:=Ord1,
                Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
                Order2:=Ord2,
                Key3:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key3)+'1'],
                Order3:=Ord3,
                Header:=1,
                OrderCustom:=1,
                MatchCase:=False,
                Orientation:=1);}

              XLApp.Selection.Subtotal(GroupBy:=key1, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key2, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.Selection.Subtotal(GroupBy:=key3, Function:=xlSum,
                TotalList:=vArr,
                Replace:=False, PageBreaks:=False, SummaryBelowData:=True);

              XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
          end;
        end
        else
        begin
            if key1=-1 then key1:=1;
            if key2=-1 then key2:=1;
            if key3=-1 then key3:=1;
            {XLApp.Workbooks[1].WorkSheets[sTableName].Columns.Sort(
              Key1:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key1)+'1'],
              Order1:=Ord1,
              Key2:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key2)+'1'],
              Order2:=Ord2,
              Key3:=XLApp.Workbooks[1].WorkSheets[sTableName].Range[NumToxlsColDLL(key3)+'1'],
              Order3:=Ord3,
              Header:=1,
              OrderCustom:=1,
              MatchCase:=False,
              Orientation:=1);}
        end;
    end;
    XLApp.Visible := iOpen = 0;
FINALLY
    if iOpen = 1 then
    begin
      XLApp.ActiveWorkbook.SaveAs(sdbFile, xlNormal, '', '', False, False);
      XLApp.quit;
    end;
    if assigned(frmLoadProgressDLL) then
    begin
      frmLoadProgressDLL.free;
    end;
    Screen.Cursor := Save_Cursor;
    data.First;
    data.EnableControls;
END;
end;

function funGetItemFullHeightDel(sCalcItemId:string;qryExec:TADOQuery): Integer;
//2020.12.08
var sList:TstringList;//2020.03.12
    sFontSize:string;//2020.03.12
    iFontSize: integer;//2020.03.12
begin
  //2020.12.08
  iFontSize:=0;
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
    sList:=TstringList.Create;
    sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
    sFontSize:=sList.Strings[0];
    sList.Free;
    iFontSize:=StrToInt(sFontSize);
  end;
  qryExec.Close;
  qryExec.SQL.Clear;
  qryExec.SQL.Add('exec CURdItemFullHeightDel '''+sCalcItemId+''','+IntToStr(iFontSize));
  qryExec.Open;
  result:=qryExec.FieldByName('DelValue').AsInteger;
end;

//2020.03.10
function MsgDlgJS(const Msg: string; DlgType: TMsgDlgType;
                    Buttons: TMsgDlgButtons; HelpCtx: Integer): Integer;
var sList:TstringList;
    sFontSize:string;
    FontSize, DlgFontSize: integer;
begin
    //2020.11.13
    FontSize := 100;
    DlgFontSize:=9;
    //2020.03.10
    if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
    begin
          sList:=TstringList.Create;
          sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
          sFontSize:=sList.Strings[0];
          sList.Free;
          FontSize := StrToInt(sFontSize);
    end;
    DlgFontSize:=Round(11 * FontSize / 100);  //2023.08.31 預設字體調大 9->11
    //2023.09.01 add
    if FontSize=100 then
      DlgFontSize:=11;

    {case FontSize of
        100: FontSize := 9;
        120: FontSize := 12;
        135: FontSize := 14;
        150: FontSize := 16;
    end;}
    Screen.MessageFont.Size := DlgFontSize;//FontSize;
    with CreateMessageDialog(Msg, DlgType, Buttons) do
    try
      Result := ShowModal;
    finally
      Free;
    end
end;

end.

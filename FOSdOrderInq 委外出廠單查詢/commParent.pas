unit commParent;

interface

uses ShareMem,SysUtils,Classes,Forms,Dialogs,ADODB,ComCtrls,ExtCtrls,Windows,StdCtrls,
  Variants, Graphics, Controls,Buttons;

function ShowForm(
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
  //====
  hDllHandle:THandle;
  sSystemId:string;
  iCallType:integer;
  iDtlItem:integer //new 2009.8.26
  ):Boolean;stdcall;

exports
  ShowForm;

var hcommDllHandle:THandle;
  tmpParent:TWinControl;//2010.11.8 add

implementation

uses OrderInq, unit_DLL;

function ShowForm(
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
  //====
  hDllHandle:THandle;
  sSystemId:string;
  iCallType:integer;
  iDtlItem:integer //new 2009.8.26
  ):Boolean;stdcall;
var rRatio:double; //2019.01.18
    iDelH:Integer;
begin
 tmpParent:=rParent;//2010.11.8 add

 hcommDllHandle:=hDllHandle;

 RegisterClasses([TfrmFOSdOrderInq]);

 Application.CreateForm(TfrmFOSdOrderInq, frmFOSdOrderInq);

 rParent.Tag:=frmFOSdOrderInq.btnTempBasDLLDo.Handle;

 unit_DLL.funStartDLL(
    frmFOSdOrderInq,
    rParent,
    bShowModal,
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
    sSystemId,
    iCallType,
    iDtlItem //new 2009.8.26
    );
 //2019.01.18
 rRatio:=Screen.PixelsPerInch / 96;
 iDelH:=frmFOSdOrderInq.Height -
 round(unit_DLL.funGetItemFullHeightDel(sItemId,frmFOSdOrderInq.qryExec) * rRatio);
 //2019.02.01
 //round(80 * rRatio);

 frmFOSdOrderInq.pnlMain.Height:=iDelH; //frmFOSdOrderInq.Height -80;

 result:=true;
end;

end.


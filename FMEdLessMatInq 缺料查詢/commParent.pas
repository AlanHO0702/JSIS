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

var hcommDllHandle:THandle; tmpParent:TWinControl;

implementation

uses LessMatInq, unit_DLL;

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
 hcommDllHandle:=hDllHandle;

 tmpParent:=rParent;

 RegisterClasses([TfrmFMEdLessMatInq]);

 Application.CreateForm(TfrmFMEdLessMatInq, frmFMEdLessMatInq);

 rParent.Tag:=frmFMEdLessMatInq.btnTempBasDLLDo.Handle;

 unit_DLL.funStartDLL(
    frmFMEdLessMatInq,
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
    iDtlItem
    );
 //2019.01.18
 rRatio:=Screen.PixelsPerInch / 96;
 iDelH:=frmFMEdLessMatInq.Height -
 round(unit_DLL.funGetItemFullHeightDel(sItemId,frmFMEdLessMatInq.qryExec) * rRatio);
 //2019.02.01
 //round(30 * rRatio);

 frmFMEdLessMatInq.pnlMain.Height:=iDelH; //frmFMEdLessMatInq.Height -30;

 result:=true;
end;
{
initialization
  hcommDllHandle:=0;

finalization
  if hcommDllHandle<>0 then FreeLibrary(hcommDllHandle);
}
end.


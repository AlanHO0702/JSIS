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

var hcommDllHandle:THandle; tmpParent: TWinControl;

implementation

uses PartNumViewer, unit_DLL;

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

 RegisterClasses([TfrmFMEdPartNumViewer]);

 Application.CreateForm(TfrmFMEdPartNumViewer, frmFMEdPartNumViewer);

 rParent.Tag:=frmFMEdPartNumViewer.btnTempBasDLLDo.Handle;

 unit_DLL.funStartDLL(
    frmFMEdPartNumViewer,
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
 iDelH:=frmFMEdPartNumViewer.Height -
 round(unit_DLL.funGetItemFullHeightDel(sItemId,frmFMEdPartNumViewer.qryExec) * rRatio);
 //2019.02.01
 //round(120 * rRatio);

 frmFMEdPartNumViewer.pagData.Height:=iDelH; //frmFMEdPartNumViewer.Height -120;

 result:=true;
end;
{
initialization
  hcommDllHandle:=0;

finalization
  if hcommDllHandle<>0 then FreeLibrary(hcommDllHandle);
}
end.


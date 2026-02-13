unit commParent;

interface

uses ShareMem,SysUtils,Classes,Forms,Dialogs,ADODB,ComCtrls,ExtCtrls,Windows,StdCtrls,
  Variants, Graphics, Controls,Buttons;

function DoComm(iDo:integer):Boolean;stdcall;

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
  ShowForm,DoComm;

var hcommDllHandle:THandle;
  tmpParent:TWinControl;//2010.11.8 add

implementation

uses TmpPressDLL, unit_DLL;

function DoComm(iDo:integer):Boolean;
begin
   {case iDo of
     0:frmTmpPressDLL.prcDoReOpen;
   end;}

   result:=true;
end;

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

 RegisterClasses([TfrmTmpPressDLL]);

 Application.CreateForm(TfrmTmpPressDLL, frmTmpPressDLL);

 rParent.Tag:=frmTmpPressDLL.btnTempBasDLLDo.Handle;

 unit_DLL.funStartDLL(
    frmTmpPressDLL,
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
 iDelH:=frmTmpPressDLL.Height -
 round(unit_DLL.funGetItemFullHeightDel(sItemId,frmTmpPressDLL.qryExec) * rRatio);
 //2019.02.01
 //round(150 * rRatio);

 frmTmpPressDLL.pnlAll.Height:=iDelH; //frmTmpPressDLL.Height -150;

 result:=true;
end;

{
initialization
  hcommDllHandle:=0;

finalization
  if hcommDllHandle<>0 then FreeLibrary(hcommDllHandle);
}

end.


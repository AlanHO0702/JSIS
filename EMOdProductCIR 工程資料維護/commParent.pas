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
    tmpParent:TWincontrol;//2010.11.8 add

implementation

uses ProductDLL, unit_DLL;

function DoComm(iDo:integer):Boolean;
begin
   case iDo of
     0:frmProductDLL.prcDoReOpen;
   end;

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

 RegisterClasses([TfrmProductDLL]);

 Application.CreateForm(TfrmProductDLL, frmProductDLL);

 rParent.Tag:=frmProductDLL.btnTempBasDLLDo.Handle;

 unit_DLL.funStartDLL(
    frmProductDLL,
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
 iDelH:=frmProductDLL.Height -
 round(unit_DLL.funGetItemFullHeightDel(sItemId,frmProductDLL.qryExec) * rRatio);
 //2019.02.01
 //round(168 * rRatio);

 frmProductDLL.pgeBwsDtl.Height:=iDelH; //frmProductDLL.Height -168;

 result:=true;
end;

{
initialization
  hcommDllHandle:=0;

finalization
  if hcommDllHandle<>0 then FreeLibrary(hcommDllHandle);
}

end.


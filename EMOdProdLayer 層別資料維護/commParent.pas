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

uses ProdLayerDLL, unit_DLL;

function DoComm(iDo:integer):Boolean;
begin
   case iDo of
     0:frmProdLayerDLL.prcDoReOpen;
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

 RegisterClasses([TfrmProdLayerDLL]);

 Application.CreateForm(TfrmProdLayerDLL, frmProdLayerDLL);

 rParent.Tag:=frmProdLayerDLL.btnTempBasDLLDo.Handle;

 unit_DLL.funStartDLL(
    frmProdLayerDLL,
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
 iDelH:=frmProdLayerDLL.Height -
 round(unit_DLL.funGetItemFullHeightDel(sItemId,frmProdLayerDLL.qryExec) * rRatio);
 //2019.02.01
 //round(168 * rRatio);

 frmProdLayerDLL.pgeBwsDtl.Height:=iDelH; //frmProdLayerDLL.Height -168;

 if (frmProdLayerDLL.pnlTempBasDLLbm.Visible=false) then
    begin
      frmProdLayerDLL.pgeBwsDtl.Height:=frmProdLayerDLL.pgeBwsDtl.Height+22;
    end;

 if (frmProdLayerDLL.pnlTempBasDLLBottom.Visible=false) then
    begin
      frmProdLayerDLL.pgeBwsDtl.Height:=frmProdLayerDLL.pgeBwsDtl.Height+28;
    end;
 result:=true;
end;

{
initialization
  hcommDllHandle:=0;

finalization
  if hcommDllHandle<>0 then FreeLibrary(hcommDllHandle);
}

end.


unit DLLParent;

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
  iDtlItem:Integer
  ):Boolean;stdcall;

exports
  ShowForm;

var hcommDllHandle:THandle;

implementation

uses ChargeCalcExcelDLL, unit_DLL;

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
  iDtlItem:Integer
  ):Boolean;stdcall;
begin
 hcommDllHandle:=hDllHandle;

 RegisterClasses([TfrmHPSdChargeCalcExcelDLL]);

 Application.CreateForm(TfrmHPSdChargeCalcExcelDLL, frmHPSdChargeCalcExcelDLL);
 unit_DLL.funStartDllNoParent(
    frmHPSdChargeCalcExcelDLL,
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
    iDtlItem //new 2009.8.26
    );

 result:=true;
end;

{
initialization
  hcommDllHandle:=0;

finalization
  if hcommDllHandle<>0 then FreeLibrary(hcommDllHandle);
}

end.


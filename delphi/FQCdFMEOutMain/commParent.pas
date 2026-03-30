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

uses FQCdFMEOut, unit_DLL;

function DoComm(iDo:integer):Boolean;
begin
   case iDo of
     0:frmFQCdFMEOutMain.prcDoReOpen;
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

 RegisterClasses([TfrmFQCdFMEOutMain]);

 Application.CreateForm(TfrmFQCdFMEOutMain, frmFQCdFMEOutMain);

 rParent.Tag:=frmFQCdFMEOutMain.btnTempBasDLLDo.Handle;

 unit_DLL.funStartDLL(
    frmFQCdFMEOutMain,
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
 iDelH:=frmFQCdFMEOutMain.Height -
 round(unit_DLL.funGetItemFullHeightDel(sItemId,frmFQCdFMEOutMain.qryExec) * rRatio);
 //2019.02.01
 //round(168 * rRatio);

 //frmFQCdFMEOutMain.pgeBwsDtl.Height:=frmFQCdFMEOutMain.Height-160;// -140;
 frmFQCdFMEOutMain.pgeBwsDtl.Height:=iDelH; //frmFQCdFMEOutMain.Height-168;//2015.06.17 modify for SS Bill-20150617-01

 if (frmFQCdFMEOutMain.pnlTempBasDLLbm.Visible=false) then
    begin
      frmFQCdFMEOutMain.pgeBwsDtl.Height:=frmFQCdFMEOutMain.pgeBwsDtl.Height+frmFQCdFMEOutMain.pnlTempBasDLLbm.Height;//+22;
    end;

 if (frmFQCdFMEOutMain.pnlTempBasDLLBottom.Visible=false) then
    begin
      frmFQCdFMEOutMain.pgeBwsDtl.Height:=frmFQCdFMEOutMain.pgeBwsDtl.Height+frmFQCdFMEOutMain.pnlTempBasDLLBottom.Height;//+28;
    end;

 result:=true;
end;

end.


library EMOdProduct;

{ Important note about DLL memory management: ShareMem must be the
  first unit in your library's USES clause AND your project's (select
  Project-View Source) USES clause if your DLL exports any procedures or
  functions that pass strings as parameters or function results. This
  applies to all strings passed to and from your DLL--even those that
  are nested in records and classes. ShareMem is the interface unit to
  the BORLNDMM.DLL shared memory manager, which must be deployed along
  with your DLL. To avoid using BORLNDMM.DLL, pass string information
  using PChar or ShortString parameters. }

uses
  ShareMem,
  SysUtils,
  Classes,
  unit_DLL in '..\..\..\..\JSdUnitOCX\unit_DLL.pas',
  ProductDLL in 'ProductDLL.pas' {frmProductDLL},
  commParent in 'commParent.pas',
  TempBasDLL in '..\..\..\..\JSdFormOCX\TempBasDLL.pas' {frmTempBasDLL},
  TempDlgDLL in '..\..\..\..\JSdFormOCX\TempDlgDLL.pas' {frmTempDlgDLL},
  TempPublic in '..\..\..\..\JSdFormOCX\TempPublic.pas' {frmTempPublic},
  PaperWhereDLL in '..\..\..\..\JSdFormOCX\PaperWhereDLL.pas' {dlgPaperWhereDLL},
  PaperSelectType2DLL in '..\..\..\..\JSdFormOCX\PaperSelectType2DLL.pas' {dlgPaperSelectType2DLL},
  PaperWhereItem2DLL in '..\..\..\..\JSdFormOCX\PaperWhereItem2DLL.pas' {dlgPaperWhereItem2DLL},
  ShowDLLForm in '..\..\..\..\JSdFormOCX\ShowDLLForm.pas' {frmShowDLLForm},
  FunctionNotes2 in '..\..\..\..\JSdFormOCX\FunctionNotes2.pas' {frmFunctionNotes2},
  ErrorDialogDLL in '..\..\..\..\JSdUnitOCX\ErrorDialogDLL.pas' {frmErrorDialog},
  LoadProgressDLL in '..\..\..\..\JSdFormOCX\LoadProgressDLL.pas' {frmLoadProgressDLL},
  AskDestDLL in '..\..\..\..\JSdFormOCX\AskDestDLL.pas' {dlgAskDestDLL},
  CurrPeriodSetDLL in '..\..\..\..\JSdFormOCX\CurrPeriodSetDLL.pas' {frmCurrPeriodSetDLL},
  ShowDBEdit in '..\..\..\..\JSdFormOCX\ShowDBEdit.pas' {frmShowDBEdit},
  LinkPaperDLL in '..\..\..\..\JSdFormOCX\LinkPaperDLL.pas' {frmLinkPaperDLL},
  LinkShowDLL in '..\..\..\..\JSdFormOCX\LinkShowDLL.pas' {frmLinkShowDLL},
  EditPaper in '..\..\..\..\JSdFormOCX\JSdPaperEMODLL\EditPaper.pas' {frmEditPaper},
  PaperOrgDLL in '..\..\..\..\JSdFormOCX\JSdPaperEMODLL\PaperOrgDLL.pas' {frmPaperOrgDLL},
  NotValueShow in 'NotValueShow.pas' {dlgNotValueShow},
  MapEdit in 'MapEdit.pas' {frmMapEdit},
  PressChange in 'PressChange.pas' {dlgPressChange},
  Map in 'Map.pas' {frmMap},
  NewNameEdit in 'NewNameEdit.pas' {dlgNewNameEdit},
  TmpPressSelect in 'TmpPressSelect.pas' {dlgTmpPressSelect},
  TmpRouteSelect in 'TmpRouteSelect.pas' {dlgTmpRouteSelect},
  RouteInsNew in 'RouteInsNew.pas' {dlgRouteInsNew},
  LayerRouteSet in 'LayerRouteSet.pas' {dlgLayerRouteSet},
  LayerPressSet in 'LayerPressSet.pas' {dlgLayerPressSet},
  TmpRouteSet in 'TmpRouteSet.pas' {dlgTmpRouteSet},
  PaperSearchDLL in '..\..\..\..\JSdUnitOCX\PaperSearchDLL.pas' {dlgPaperSearchDLL},
  unit_DLL2 in '..\..\..\..\JSdUnitOCX\unit_DLL2.pas',
  CondRunSpDLL in '..\..\..\..\JSdUnitOCX\CondRunSpDLL.pas',
  EditGridDLL in '..\..\..\..\JSdUnitOCX\EditGridDLL.pas',
  UOMGetLotDLL in '..\..\..\..\JSdUnitOCX\UOMGetLotDLL.pas',
  OrderChange in '..\..\..\..\JSdUnitOCX\OrderChange.pas',
  MsgUserSelect in '..\..\..\..\JSdUnitOCX\MsgUserSelect.pas' {frmMsgUserSelect},
  UpdateLog in '..\..\..\..\JSdUnitOCX\UpdateLog.pas' {frmUpdateLog},
  ProdUseMatSet in 'ProdUseMatSet.pas' {dlgProdUseMatSet},
  CopySelect in 'CopySelect.pas' {dlgCopySelect},
  PaperPrint in '..\..\..\..\JSdUnitOCX\PaperPrint.pas',
  PaperPrint2 in '..\..\..\..\JSdFormOCX\JSdPaperEMODLL\PaperPrint2.pas',
  PaperPrintTools2 in '..\..\..\..\JSdFormOCX\JSdPaperEMODLL\PaperPrintTools2.pas',
  SelectQR in '..\..\..\..\JSdFormOCX\SelectQR.pas',
  unit_MIS in '..\..\..\..\JSdUnit\unit_MIS.pas';

{$R *.res}

begin
end.

library HPSdChargeCalcExcel;

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
  Forms,
  Controls,
  unit_DLL in '..\..\..\..\JSdUnitOCX\unit_DLL.pas',
  DLLParent in 'DLLParent.pas',
  ChargeCalcExcelDLL in 'ChargeCalcExcelDLL.pas' {frmHPSdChargeCalcExcelDLL},
  ShowDLLForm in '..\..\..\..\JSdFormOCX\ShowDLLForm.pas' {frmShowDLLForm},
  TempBasDLL in '..\..\..\..\JSdFormOCX\TempBasDLL.pas' {frmTempBasDLL},
  PaperSelectType2DLL in '..\..\..\..\JSdFormOCX\PaperSelectType2DLL.pas' {dlgPaperSelectType2DLL},
  TempDlgDLL in '..\..\..\..\JSdFormOCX\TempDlgDLL.pas' {frmTempDlgDLL},
  TempPublic in '..\..\..\..\JSdFormOCX\TempPublic.pas' {frmTempPublic},
  ShowDBEdit in '..\..\..\..\JSdFormOCX\ShowDBEdit.pas',
  AskDestDLL in '..\..\..\..\JSdFormOCX\AskDestDLL.pas' {frmAskDestDLL},
  CurrPeriodSetDLL in '..\..\..\..\JSdFormOCX\CurrPeriodSetDLL.pas',
  FunctionNotes2 in '..\..\..\..\JSdFormOCX\FunctionNotes2.pas' {frmFunctionNotes2},
  ErrorDialogDLL in '..\..\..\..\JSdUnitOCX\ErrorDialogDLL.pas' {frmErrorDialog},
  PaperSearchDLL in '..\..\..\..\JSdUnitOCX\PaperSearchDLL.pas' {dlgPaperSearchDLL},
  unit_DLL2 in '..\..\..\..\JSdUnitOCX\unit_DLL2.pas',
  CondRunSpDLL in '..\..\..\..\JSdUnitOCX\CondRunSpDLL.pas',
  EditGridDLL in '..\..\..\..\JSdUnitOCX\EditGridDLL.pas',
  UOMGetLotDLL in '..\..\..\..\JSdUnitOCX\UOMGetLotDLL.pas',
  OrderChange in '..\..\..\..\JSdUnitOCX\OrderChange.pas',
  MsgUserSelect in '..\..\..\..\JSdUnitOCX\MsgUserSelect.pas' {frmMsgUserSelect},
  UpdateLog in '..\..\..\..\JSdUnitOCX\UpdateLog.pas' {frmUpdateLog},
  PaperPrint in '..\..\..\..\JSdUnitOCX\PaperPrint.pas',
  LoadProgressDLL in '..\..\..\..\JSdFormOCX\LoadProgressDLL.pas' {frmLoadProgressDLL};

{$R *.res}

begin
end.

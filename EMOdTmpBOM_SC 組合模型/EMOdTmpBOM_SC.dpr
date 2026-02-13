library EMOdTmpBOM_SC;

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
  TempBasDLL in '..\..\..\..\JSdFormOCX\TempBasDLL.pas' {frmTempBasDLL},
  TempDlgDLL in '..\..\..\..\JSdFormOCX\TempDlgDLL.pas' {frmTempDlgDLL},
  TempPublic in '..\..\..\..\JSdFormOCX\TempPublic.pas' {frmTempPublic},
  LoadProgressDLL in '..\..\..\..\JSdFormOCX\LoadProgressDLL.pas' {frmLoadProgressDLL},
  AskDestDLL in '..\..\..\..\JSdFormOCX\AskDestDLL.pas' {frmAskDestDLL},
  PaperSelectType2DLL in '..\..\..\..\JSdFormOCX\PaperSelectType2DLL.pas' {dlgPaperSelectType2DLL},
  ShowDLLForm in '..\..\..\..\JSdFormOCX\ShowDLLForm.pas' {frmShowDLLForm},
  ShowDBEdit in '..\..\..\..\JSdFormOCX\ShowDBEdit.pas',
  CurrPeriodSetDLL in '..\..\..\..\JSdFormOCX\CurrPeriodSetDLL.pas',
  unit_SQL in '..\..\..\..\JSdUnit\unit_SQL.pas',
  unit_FileIO in '..\..\..\..\JSdUnit\unit_FileIO.pas',
  unit_MIS in '..\..\..\..\JSdUnit\unit_MIS.pas',
  unit_DLL in '..\..\..\..\JSdUnitOCX\unit_DLL.pas',
  FunctionNotes2 in '..\..\..\..\JSdFormOCX\FunctionNotes2.pas' {frmFunctionNotes2},
  MasDtlDLL in '..\..\..\..\JSdFormOCX\JSdMasDtlDLL\MasDtlDLL.pas' {frmMasDtlDLL},
  LinkShowDLL in '..\..\..\..\JSdFormOCX\LinkShowDLL.pas' {frmLinkShowDLL},
  PaperWhereDLL in '..\..\..\..\JSdFormOCX\PaperWhereDLL.pas' {dlgPaperWhereDLL},
  PaperWhereItem2DLL in '..\..\..\..\JSdFormOCX\PaperWhereItem2DLL.pas' {dlgPaperWhereItem2DLL},
  ErrorDialogDLL in '..\..\..\..\JSdUnitOCX\ErrorDialogDLL.pas' {frmErrorDialog},
  commParent in 'commParent.pas',
  PaperSearchDLL in '..\..\..\..\JSdUnitOCX\PaperSearchDLL.pas' {dlgPaperSearchDLL},
  unit_DLL2 in '..\..\..\..\JSdUnitOCX\unit_DLL2.pas',
  CondRunSpDLL in '..\..\..\..\JSdUnitOCX\CondRunSpDLL.pas',
  EditGridDLL in '..\..\..\..\JSdUnitOCX\EditGridDLL.pas',
  UOMGetLotDLL in '..\..\..\..\JSdUnitOCX\UOMGetLotDLL.pas',
  OrderChange in '..\..\..\..\JSdUnitOCX\OrderChange.pas',
  MsgUserSelect in '..\..\..\..\JSdUnitOCX\MsgUserSelect.pas' {frmMsgUserSelect},
  UpdateLog in '..\..\..\..\JSdUnitOCX\UpdateLog.pas' {frmUpdateLog},
  PaperPrint in '..\..\..\..\JSdUnitOCX\PaperPrint.pas',
  TmpBOM_SCDLL in 'TmpBOM_SCDLL.pas' {frmEMOdTmpBOM_SCDLL},
  TmpBOMSet in 'TmpBOMSet.pas' {dlgTmpBOMSet};

{$R *.res}

begin
end.

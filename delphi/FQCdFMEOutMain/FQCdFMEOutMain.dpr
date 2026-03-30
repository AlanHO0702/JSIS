library FQCdFMEOutMain;

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
  commParent in 'commParent.pas',
  TempBasDLL in '..\..\..\JSdFormOCX\TempBasDLL.pas' {frmTempBasDLL},
  unit_DLL in '..\..\..\JSdUnitOCX\unit_DLL.pas',
  PaperOrgDLL in '..\..\..\JSdFormOCX\JSdPaperOrgDLL\PaperOrgDLL.pas' {frmPaperOrgDLL},
  FQCdFMEOut in 'FQCdFMEOut.pas' {frmFQCdFMEOutMain},
  EditPaper in '..\..\..\JSdFormOCX\JSdPaperOrgDLL\EditPaper.pas' {frmEditPaper},
  PaperPrintTools2 in '..\..\..\JSdFormOCX\JSdPaperOrgDLL\PaperPrintTools2.pas' {frmPaperPrintTools2},
  PaperPrint2 in '..\..\..\JSdFormOCX\JSdPaperOrgDLL\PaperPrint2.pas' {frmPaperPrint2},
  InputData in 'InputData.pas' {dlgInputData};

{$R *.res}

begin
end.

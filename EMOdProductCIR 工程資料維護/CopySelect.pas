unit CopySelect;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, JSdMultSelect, DB, ADODB,
  JSdLookupCombo;

type
  TdlgCopySelect = class(TfrmTempDlgDLL)
    Panel1: TPanel;
    msCopy: TJSdMultSelect;
    edtPartNum: TEdit;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    edtRevision: TEdit;
    qryTargetLayer: TADOQuery;
    dsTargetLayer: TDataSource;
    qryLayerId: TADOQuery;
    qryTargetLayerPartNum: TStringField;
    qryTargetLayerRevision: TStringField;
    qryTargetLayerLayerId: TStringField;
    qryTargetLayerSourceLayerId: TStringField;
    qryTargetLayerTmpRouteId: TStringField;
    cboLayerId: TJSdLookupCombo;
    dsLayerId: TDataSource;
    btFind: TSpeedButton;
    qryTargetLayerLayerName: TWideStringField;
    qryTargetLayerSourceName: TWideStringField;
    procedure btFindClick(Sender: TObject);
    procedure cboLayerIdEnter(Sender: TObject);
  private
    { Private declarations }
  public
    var NowPN, NowRev: String;
    { Public declarations }
  end;

var
  dlgCopySelect: TdlgCopySelect;

implementation

{$R *.dfm}

procedure TdlgCopySelect.btFindClick(Sender: TObject);
begin
  inherited;
  with qryTargetLayer do
  begin
    Close;
    Parameters.ParamByName('PartNum').Value:= NowPN;
    Parameters.ParamByName('Revision').Value:= NowRev;
    Parameters.ParamByName('LayerId').Value:= cboLayerId.Text;
    Parameters.ParamByName('SourPartNum').Value:= edtPartNum.Text;
    Parameters.ParamByName('SourRevision').Value:= edtRevision.Text;
    Open;
  end;
  msCopy.Setup(slAll);
end;

procedure TdlgCopySelect.cboLayerIdEnter(Sender: TObject);
begin
  inherited;
  //2012.05.17 add
  with qryLayerId do
  begin
    qryLayerId.Close;
    Parameters.ParamByName('PartNum').Value:= edtPartNum.Text;
    Parameters.ParamByName('Revision').Value:= edtRevision.Text;
    Open;
  end;
end;

end.

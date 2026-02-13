unit TmpRouteSet;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, Spin, JSdLabel, Menus, DB,
  ADODB, JSdMultSelect, ComCtrls;

type
  TdlgTmpRouteSet = class(TfrmTempDlgDLL)
    qryTmpRouteDtl: TADOQuery;
    dsTmpPressDtl: TDataSource;
    qryMas: TADOQuery;
    dsMas: TDataSource;
    pnlUpDown: TPanel;
    btnSerialUp: TSpeedButton;
    btnSerialDown: TSpeedButton;
    msSelects: TJSdMultSelect;
    qryProcBasic: TADOQuery;
    dsProcBasic: TDataSource;
    Label1: TLabel;
    edtBProc: TEdit;
    Label2: TLabel;
    edtEProc: TEdit;
    Label3: TLabel;
    Label4: TLabel;
    edtProcLike: TEdit;
    edtProcNameLike: TEdit;
    btnSearch: TSpeedButton;
    procedure btnSerialUpClick(Sender: TObject);
    procedure btnSerialDownClick(Sender: TObject);
    procedure btnSearchClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  dlgTmpRouteSet: TdlgTmpRouteSet;

implementation

{$R *.dfm}


procedure TdlgTmpRouteSet.btnSearchClick(Sender: TObject);
var i: Integer;
begin
  inherited;
  //2011.08.15 add
  with qryProcBasic do
  begin
    qryProcBasic.Close;
    Parameters.ParamByName('BProc').Value:=edtBProc.Text;
    Parameters.ParamByName('EProc').Value:=edtEProc.Text;
    Parameters.ParamByName('ProcLike').Value:=edtProcLike.Text;
    Parameters.ParamByName('ProcNameLike').Value:=edtProcNameLike.Text;
    Open;
  end;
  msSelects.Setup(slSource);
end;

procedure TdlgTmpRouteSet.btnSerialDownClick(Sender: TObject);
begin
  inherited;
  msSelects.PosMoveDown(sender);
end;

procedure TdlgTmpRouteSet.btnSerialUpClick(Sender: TObject);
begin
  inherited;
  msSelects.PosMoveUp(sender);
end;

end.

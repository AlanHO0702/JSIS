unit ProdUseMatSet;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, DB, ADODB, JSdMultSelect,
  JSdRadioGroup, JSdLookupCombo, JSdLabel;

type
  TdlgProdUseMatSet = class(TfrmTempDlgDLL)
    qryProdPressMat: TADOQuery;
    qryProdPressMatMatCode: TStringField;
    qryProdPressMatMatName: TWideStringField;
    qryProdPressMatNotes: TWideStringField;
    dsPressMat: TDataSource;
    qryLayerPress: TADOQuery;
    qryLayerPressMatCode: TStringField;
    qryLayerPressMatName: TWideStringField;
    qryLayerPressSerialNum: TSmallintField;
    dsLayerPress: TDataSource;
    qrySupplier: TADOQuery;
    dsSupplier: TDataSource;
    Panel1: TPanel;
    Label3D12: TJSdLabel;
    Label1: TJSdLabel;
    btFind: TSpeedButton;
    cboLayerId: TJSdLookupCombo;
    cboSupId: TJSdLookupCombo;
    rdoMatClass: TJSdRadioGroup;
    msSelects: TJSdMultSelect;
    btnSerialUp: TSpeedButton;
    btnSerialDown: TSpeedButton;
    qry2MatClass: TADOQuery;
    ds2MatClass: TDataSource;
    qryProdLayer: TADOQuery;
    dsProdLayer: TDataSource;
    qryClassMat: TADOQuery;
    dsClassMat: TDataSource;
    pnlTOP: TPanel;
    Panel3: TPanel;
    cboClassMat: TJSdLookupCombo;
    procedure btnSerialUpClick(Sender: TObject);
    procedure btFindClick(Sender: TObject);
    procedure btnSerialDownClick(Sender: TObject);
    procedure rdoMatClassClick(Sender: TObject);
    procedure cboSupIdChange(Sender: TObject);
    procedure cboClassMatChange(Sender: TObject);
  private
    { Private declarations }
  public
    BPartNum,BRevision :string;
    CurrMatClass: string;
    sClassMat: String;
    { Public declarations }
  end;

var
  dlgProdUseMatSet: TdlgProdUseMatSet;

implementation

{$R *.dfm}

procedure TdlgProdUseMatSet.btFindClick(Sender: TObject);
begin
  inherited;
  with qryProdPressMat do
  begin
     close;
     Parameters.ParamByName('MatClass').Value:='';
     open;
  end;
  with qryLayerPress do
  begin
     Close;
     Parameters.ParamByName('PartNum').Value:= BPartNum;
     Parameters.ParamByName('Revision').Value:= BRevision;
     Open;
  end;
  msSelects.Setup(slAll);
end;

procedure TdlgProdUseMatSet.btnSerialDownClick(Sender: TObject);
begin
  inherited;
  msSelects.PosMoveDown(sender);
end;

procedure TdlgProdUseMatSet.btnSerialUpClick(Sender: TObject);
begin
  inherited;
  msSelects.PosMoveUp(sender);
end;

procedure TdlgProdUseMatSet.cboClassMatChange(Sender: TObject);
begin
  inherited;
  //2011.09.28
  if cboClassMat.Text<>'' then
  begin
    sClassMat:= cboClassMat.Text;
    qry2MatClass.Close;
    qry2MatClass.Parameters.ParamByName('ClassMat').Value:=sClassMat;
    qry2MatClass.Open;
    rdoMatClass.Refresh;
  end;
end;

procedure TdlgProdUseMatSet.cboSupIdChange(Sender: TObject);
begin
  inherited;
  CurrMatClass:= TRadioData(rdoMatClass.DataList[rdoMatClass.ItemIndex]^).Value;
  with qryProdPressMat do
  begin
     close;
     Parameters.ParamByName('MatClass').Value:=CurrMatClass;
     open;
  end;
  msSelects.setup(slSource);
end;

procedure TdlgProdUseMatSet.rdoMatClassClick(Sender: TObject);
begin
  inherited;
  CurrMatClass:= TRadioData(rdoMatClass.DataList[rdoMatClass.ItemIndex]^).Value;
  with qryProdPressMat do
  begin
     close;
     Parameters.ParamByName('MatClass').Value:=CurrMatClass;
     open;
  end;
  msSelects.setup(slSource);
end;

end.

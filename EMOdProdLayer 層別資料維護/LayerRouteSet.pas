unit LayerRouteSet;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, JSdMultSelect,
  JSdRadioGroup, JSdLookupCombo, JSdLabel, DB, ADODB;

type
  TdlgLayerRouteSet = class(TfrmTempDlgDLL)
    qryProdPressMat: TADOQuery;
    qryProdPressMatMatCode: TStringField;
    qryProdPressMatMatName: TStringField;
    qryProdPressMatNotes: TStringField;
    dsPressMat: TDataSource;
    qryLayerPress: TADOQuery;
    qryLayerPressMatCode: TStringField;
    qryLayerPressMatName: TStringField;
    qryLayerPressSerialNum: TSmallintField;
    dsLayerPress: TDataSource;
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
    qryProdLayer: TADOQuery;
    dsProdLayer: TDataSource;
    qry2MatClass: TADOQuery;
    ds2MatClass: TDataSource;
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
    BPartNum,BRevision,BLayerId,BProcCode :string;
    CurrMatClass: string;
    sClassMat: String;
    { Public declarations }
  end;

var
  dlgLayerRouteSet: TdlgLayerRouteSet;

implementation

{$R *.dfm}

procedure TdlgLayerRouteSet.btFindClick(Sender: TObject);
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
         Parameters.ParamByName('LayerId').Value:=  BLayerId;
         Parameters.ParamByName('ProcCode').Value:=  BProcCode;
         Open;
      end;
  msSelects.Setup(slAll);
end;

procedure TdlgLayerRouteSet.btnSerialDownClick(Sender: TObject);
begin
  inherited;
  msSelects.PosMoveDown(sender);
end;

procedure TdlgLayerRouteSet.btnSerialUpClick(Sender: TObject);
begin
  inherited;
  msSelects.PosMoveUp(sender);
end;

procedure TdlgLayerRouteSet.cboClassMatChange(Sender: TObject);
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

procedure TdlgLayerRouteSet.cboSupIdChange(Sender: TObject);
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

procedure TdlgLayerRouteSet.rdoMatClassClick(Sender: TObject);
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

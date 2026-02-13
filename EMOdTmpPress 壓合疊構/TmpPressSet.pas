unit TmpPressSet;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, Spin, JSdLabel, Menus, DB,
  ADODB, JSdMultSelect, JSdLookupCombo;

type
  TdlgTmpPressSet = class(TfrmTempDlgDLL)
    pnlUpDown: TPanel;
    btnSerialUp: TSpeedButton;
    btnSerialDown: TSpeedButton;
    cboLayerId: TJSdLookupCombo;
    Label3D12: TJSdLabel;
    qryMatClass: TADOQuery;
    dsMatClass: TDataSource;
    qryTmpPressDtl: TADOQuery;
    dsTmpPressDtl: TDataSource;
    msSelects: TJSdMultSelect;
    qryProdLayer: TADOQuery;
    dsProdLayer: TDataSource;
    qryProdLayerLayerId: TStringField;
    qryProdLayerLayerName: TWideStringField;
    btnChgName: TSpeedButton;
    procedure btnSerialUpClick(Sender: TObject);
    procedure btnSerialDownClick(Sender: TObject);
    procedure btnChgNameClick(Sender: TObject);
    //procedure btnOKClick(Sender: TObject);
  private
    { Private declarations }
  public
    CurrMatClass: string;
    { Public declarations }
  end;

var
  dlgTmpPressSet: TdlgTmpPressSet;

implementation

uses unit_DLL;

{$R *.dfm}

{procedure TdlgTmpPressSet.btnOKClick(Sender: TObject);
var i:integer;
begin
  inherited;
  with msSelects do
  begin
    if SourceItems.count>0 then
    begin
      for i := 0 to SourceItems.count -1 do
      begin
           if (Uppercase(Copy(SourceItems.item[i].subitems[0],1,1))='L')
              and (SourceItems.item[i].subitems[1]='12') then
           begin
              MsgDlgJS('不能移除必要的層別!',mtwarning, [mbOk],0);
              ModalResult:=mrNone;
           end;
      end;
    end;
  end;
end;}

procedure TdlgTmpPressSet.btnChgNameClick(Sender: TObject);
//var sSource, sTarget: String;
begin
  inherited;
  //2011.08.11
  if msSelects.SelectedSource=nil then
  begin
    MsgDlgJS('左方無選取資料',mtWarning,[mbOK],0);
    exit;
  end;
  if msSelects.SelectedTarget=nil then
  begin
    MsgDlgJS('右方無選取資料',mtWarning,[mbOK],0);
    exit;
  end;
  //sSource:=msSelects.SelectedSource.caption;
  //sTarget:=msSelects.SelectedTarget.caption;
  //ShowMessage(sSource+','+sTarget);
  msSelects.SelectedTarget.caption:=msSelects.SelectedSource.caption;
  msSelects.SelectedTarget.SubItems[1]:=msSelects.SelectedSource.SubItems[1];
end;

procedure TdlgTmpPressSet.btnSerialUpClick(Sender: TObject);
begin
  inherited;
  msSelects.PosMoveUp(sender);
end;

procedure TdlgTmpPressSet.btnSerialDownClick(Sender: TObject);
begin
  inherited;
  msSelects.PosMoveDown(sender);
end;

end.

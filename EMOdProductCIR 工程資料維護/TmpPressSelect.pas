unit TmpPressSelect;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, DB, ADODB, ImgList,
  ComCtrls, JSdTreeView, Grids, DBGrids, JSdTable, Wwdbigrd, Wwdbgrid, JSdDBGrid;

type
  TdlgTmpPressSelect = class(TfrmTempDlgDLL)
    qryTmpMas: TJSdTable;
    qryTmpMasTmpId: TStringField;
    qryTmpMasTmpBOMId: TStringField;
    qryTmpMasNotes: TWideStringField;
    qryTmpMasStatus: TIntegerField;
    dsTmpPressMas: TDataSource;
    qryTmpBomDtl: TADOQuery;
    dsTmpBomDtl: TDataSource;
    qryTmpDtl: TJSdTable;
    qryTmpDtlTmpId: TStringField;
    qryTmpDtlLayerId: TStringField;
    qryTmpDtlSerialNum: TSmallintField;
    qryTmpDtlClassName: TWideStringField;
    qryTmpDtlMatClass: TStringField;
    qryTmpDtlMatCode: TStringField;
    qryTmpDtlMatName: TWideStringField;
    qryTmpDtlNotes: TWideStringField;
    dsTmpPressDtl: TDataSource;
    Query1: TADOQuery;
    DataSource1: TDataSource;
    pgeMaster: TPageControl;
    TabSheet1: TTabSheet;
    DBGrid2: TJSdDBGrid;
    TabSheet2: TTabSheet;
    btnBrowse: TSpeedButton;
    edtTmpId: TEdit;
    edtNotes: TEdit;
    chkTmpId: TCheckBox;
    chkNotes: TCheckBox;
    edtTmpBomId: TEdit;
    chkTmpBOMId: TCheckBox;
    trvLayerPress: TJSdTreeView;
    ImageList1: TImageList;
    qryMatClass: TADOQuery;
    JSdDBGrid1: TJSdDBGrid;
    procedure edtTmpIdChange(Sender: TObject);
    procedure btnBrowseClick(Sender: TObject);
    procedure DBGrid2DblClick(Sender: TObject);
    procedure qryTmpMasAfterScroll(DataSet: TDataSet);
    function IIFString(bYes:Boolean; str1, str2: WideString): WideString;
    procedure trvLayerPressChange(Sender: TObject; Node: TTreeNode);
  private
    { Private declarations }
  public
    var iTmpActive: Integer;
    procedure OpenBOMDetail(sLayer: String);
    procedure Add2Filter(var sFilter  :string; const sCondition :string);
    { Public declarations }
  end;

var
  dlgTmpPressSelect: TdlgTmpPressSelect;

implementation

{$R *.dfm}

procedure TdlgTmpPressSelect.btnBrowseClick(Sender: TObject);
var sSQLComd: string;
begin
  inherited;
  sSQLComd:='';
  if chkTmpId.checked then
     Add2Filter(sSQLComd,'TmpId like ''%'+ edtTmpId.text+'%''');

  if chkNotes.checked then
  begin
     Add2Filter(sSQLComd,'Notes like N''%'+ edtNotes.text+'%''');
  end;
  {if chkStatus.checked then
     Add2Filter(sSQLComd,'Status = ''' +  inttostr(rdoStatus.itemindex) + '''');
   }
  if chkTmpBOMId.checked then
     begin
     Add2Filter(sSQLComd,'TmpBomId ='''+ edtTmpBomId.text+'''');

     end;
  //¼f®Ö¾÷¨î
  if iTmpActive=1 then
         Add2Filter(sSQLComd,'Status =1');

  if sSQLComd<>'' then sSQLComd:=' Where '+sSQLComd; //2010.02.23
  with qryTmpMas do
  begin
     close;
     SQL.Clear;
     SQL.Add('select * from dbo.EMOdTmpPressMas'+sSQLComd);
     //filter:= sSQLComd;
     //filtered:= true;
     open;
  end;
  pgeMaster.ActivePage := TabSheet1;
  DBGrid2.Visible := True;
end;

procedure TdlgTmpPressSelect.DBGrid2DblClick(Sender: TObject);
begin
  inherited;
  Close;
  ModalResult:= mrok;
end;

procedure TdlgTmpPressSelect.edtTmpIdChange(Sender: TObject);
begin
  inherited;
  if (Sender = edtTmpId) then
     chkTmpId.Checked := True
  else if (Sender = edtNotes) then
          chkNotes.Checked := True;
end;

procedure TdlgTmpPressSelect.qryTmpMasAfterScroll(DataSet: TDataSet);
begin
  inherited;
  trvLayerPress.setup;
  trvLayerPress.FullExpand;
  OpenBOMDetail('L0~0');
end;

procedure TdlgTmpPressSelect.trvLayerPressChange(Sender: TObject;
  Node: TTreeNode);
var CurrLayer:string;
begin
  inherited;
  CurrLayer:= TNodeData(Node.Data^).Id;
      with Query1 do
      begin
         Close;
         Parameters.ParamByName('TmpId').Value:= qryTmpMas.FieldbyName('TmpId').AsString;
         Parameters.ParamByName('LayerId').Value:= CurrLayer;
         Open;
      end;
  OpenBOMDetail(CurrLayer);
end;

procedure TdlgTmpPressSelect.OpenBOMDetail(sLayer: String);
begin
  inherited;
  with qryTmpDtl do
  begin
     Close;
     Parameters.ParamByName('TmpId').Value:= qryTmpMas.FieldByName('TmpId').AsString;
     Parameters.ParamByName('LayerId').Value:= sLayer;
     Open;
  end;
end;

procedure TdlgTmpPressSelect.Add2Filter(var sFilter  :string; const sCondition :string);
begin
   sFilter := sFilter + IIFString(sFilter='', '', ' AND ') + sCondition;
end;

function TdlgTmpPressSelect.IIFString(bYes:Boolean; str1, str2: WideString): WideString;
begin
  if bYes then
    Result := (str1)
  else
    Result := (str2);
end;

end.

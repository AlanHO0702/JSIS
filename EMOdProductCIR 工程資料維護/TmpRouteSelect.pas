unit TmpRouteSelect;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, ComCtrls, JSdTreeView,
  JSdLabel, Grids, Wwdbigrd, Wwdbgrid, DB, ADODB, JSdTable, JSdDBGrid;

type
  TdlgTmpRouteSelect = class(TfrmTempDlgDLL)
    qryMas: TADOQuery;
    qryMasItemId: TStringField;
    qryMasItemName: TWideStringField;
    qryMasLevelNo: TIntegerField;
    qryMasSuperId: TStringField;
    qryMasItemNo: TIntegerField;
    dsMas: TDataSource;
    qryTmpMas: TJSdTable;
    dsTmpRouteMas: TDataSource;
    qryTmpDtl: TJSdTable;
    qryTmpDtlSerialNum: TSmallintField;
    qryTmpDtlProcCode: TStringField;
    qryTmpDtlProcName: TWideStringField;
    qryTmpDtlFinishRate: TFloatField;
    qryTmpDtlNotes: TWideStringField;
    dsTmpRouteDtl: TDataSource;
    qryDtl: TJSdTable;
    dsDtl: TDataSource;
    qryTmp: TJSdTable;
    dsTmp: TDataSource;
    qryTmpDtl2: TJSdTable;
    dsTmpDtl: TDataSource;
    qryTmpIns: TADOQuery;
    qryTmpDel: TADOQuery;
    qryTmpDelAll: TADOQuery;
    pgeMaster: TPageControl;
    TabSheet1: TTabSheet;
    Splitter1: TSplitter;
    pgeDtl: TPageControl;
    tabMaster: TTabSheet;
    grdRouteMas: TJSdDBGrid;
    tabQuery: TTabSheet;
    btnSearch: TSpeedButton;
    Label2: TJSdLabel;
    Label3: TJSdLabel;
    Label4: TJSdLabel;
    edtTmpId: TEdit;
    edtNotes: TEdit;
    rdoStatus: TRadioGroup;
    grdRouteDtl: TJSdDBGrid;
    TabSheet2: TTabSheet;
    Panel5: TPanel;
    Panel1: TPanel;
    Panel6: TPanel;
    wwDBGrid1: TJSdDBGrid;
    wwDBGrid2: TJSdDBGrid;
    Panel3: TPanel;
    BitBtn4: TSpeedButton;
    BitBtn3: TSpeedButton;
    Panel4: TPanel;
    trvMas: TJSdTreeView;
    Panel7: TPanel;
    wwDBGrid3: TJSdDBGrid;
    qryProcBasic: TADOQuery;
    Splitter2: TSplitter;
    Splitter3: TSplitter;
    qryChkTable: TADOQuery;
    procedure qryTmpAfterScroll(DataSet: TDataSet);
    procedure trvMasChanging(Sender: TObject; Node: TTreeNode;
      var AllowChange: Boolean);
    procedure wwDBGrid3DblClick(Sender: TObject);
    procedure btnSearchClick(Sender: TObject);
    procedure BitBtn3Click(Sender: TObject);
    procedure BitBtn4Click(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private
    procedure Add2Filter(var sFilter  :string; const sCondition :string);
    function IIFString(bYes:Boolean; str1, str2: WideString): WideString;
    { Private declarations }
  public
    CurrLSuperId: String;
    CurrLId: string;
    CurrLSystemId: String;
    CurrLLevel: integer;
    CurrRSuperId: String;
    CurrRId: string;
    CurrRSystemId: String;
    CurrRLevel: integer;
    CurrPartNum: String;
    CurrRevision: String;
    var iTmpActive: Integer;
    procedure SetSubList(Sender: Tobject);
    { Public declarations }
  end;

var
  dlgTmpRouteSelect: TdlgTmpRouteSelect;

implementation

uses unit_DLL;

{$R *.dfm}

procedure TdlgTmpRouteSelect.qryTmpAfterScroll(DataSet: TDataSet);
begin
  inherited;
  with qryTmpDtl2 do
  begin
     Close;
     Parameters.ParamByName('ItemId').Value := qryTmp.FieldByName('ItemId').AsString;
     open;
  end;
end;

procedure TdlgTmpRouteSelect.btnSearchClick(Sender: TObject);
var sSQLComd: string;
begin
  inherited;
  with qryChkTable do
  begin
    qryChkTable.Close;
    Open;
    if RecordCount>0 then
      sSQLComd:=' IsStop = 0 '
    else
      sSQLComd:='';
  end;
  //sSQLComd:='select * from dbo.EMOdTmpRouteMas(nolock) where 1=1 ';
   if edtTmpId.text<>'' then
     Add2Filter(sSQLComd,' TmpId like ''%'+ edtTmpId.text+'%''');
   if edtNotes.text<>'' then
     Add2Filter(sSQLComd,' Notes like N''%'+ edtNotes.text+'%''');
   if ((rdoStatus.itemindex>-1) and (rdoStatus.itemindex<2)) then
     Add2Filter(sSQLComd,' Status = ''' +  inttostr(rdoStatus.itemindex) + '''');
   //¼f®Ö¾÷¨î
   if iTmpActive=1 then
   begin
       Add2Filter(sSQLComd,' Status =1');
       Label2.Visible:=False;
       rdoStatus.Visible:=False;
   end;

  if sSQLComd<>'' then sSQLComd:=' Where '+sSQLComd; //2010.02.23
  with qryTmpMas do
  begin
     close;
     Parameters.ParamByName('Cond').Value := sSQLComd;
     open;
  end;
  qryTmpDtl.Close;
  qryTmpDtl.Open;
  pgeDtl.ActivePage := tabMaster;
end;

procedure TdlgTmpRouteSelect.FormCreate(Sender: TObject);
var sList:TstringList;
    sFontSize:string;
    FontSize: integer;
begin
  inherited;
  if FileExists(DLLGetTempPathStr+'JSIS\FontSize.txt') then
  begin
      sList:=TstringList.Create;
      sList.LoadFromFile(DLLGetTempPathStr+'JSIS\FontSize.txt');
      sFontSize:=sList.Strings[0];
      sList.Free;
      FontSize := StrToInt(sFontSize);

      //2020.12.15
      if FontSize<>100 then
      begin
        tabQuery.ScaleBy(70, FontSize);
        Label3.Height:=Round(13 * FontSize / 100);
        Label3.Width:=Round(60 * FontSize / 100);
        Label4.Height:=Round(13 * FontSize / 100);
        Label4.Width:=Round(48 * FontSize / 100);
      end;
  end;
end;

procedure TdlgTmpRouteSelect.trvMasChanging(Sender: TObject; Node: TTreeNode;
  var AllowChange: Boolean);
  function GetRootNode(Node: TTreeNode): TTreeNode;
  begin
     if Node.Level = 0 then
        Result := Node
     else
     begin
        Result := GetRootNode(Node.Parent);
     end;
  end;
var RootNode: TTreeNode;
begin
  inherited;
  CurrLSuperId := TNodeData(Node.Data^).Id;
  CurrLLevel := Node.Level+1;
  RootNode := GetRootNode(Node);
  CurrLSystemId := TNodeData(Node.Data^).Id;
  SetSubList(Sender);
end;

procedure TdlgTmpRouteSelect.wwDBGrid3DblClick(Sender: TObject);
begin
  inherited;
  With qryTmpIns do
  Begin
    Parameters.ParamByName('ItemId').Value := qryDtl.FieldByName('ItemId').AsString;
    Parameters.ParamByName('PartNum').Value:= CurrPartNum;
    Parameters.ParamByName('Revision').Value:= CurrRevision;
    Execsql;
  End;
  qryTmp.Close;
  qryTmp.Open;
end;

procedure TdlgTmpRouteSelect.Add2Filter(var sFilter  :string; const sCondition :string);
begin
   sFilter := sFilter + IIFString(sFilter='', '', ' AND ') + sCondition;
end;

procedure TdlgTmpRouteSelect.BitBtn3Click(Sender: TObject);
begin
  inherited;
  With qryTmpIns do
  Begin
    Parameters.ParamByName('ItemId').Value := qryDtl.FieldByName('ItemId').AsString;
    Parameters.ParamByName('PartNum').Value:= CurrPartNum;
    Parameters.ParamByName('Revision').Value:= CurrRevision;
    Execsql;
  End;
  qryTmp.Close;
  qryTmp.Open;
end;

procedure TdlgTmpRouteSelect.BitBtn4Click(Sender: TObject);
begin
  inherited;
  With qryTmpDel do
  Begin
    Parameters.ParamByName('ItemId').Value := qryTmp.FieldByName('ItemId').AsString;
    Parameters.ParamByName('PartNum').Value:= CurrPartNum;
    Parameters.ParamByName('Revision').Value:= CurrRevision;
    Execsql;
  End;
  qryTmp.Close;
  qryTmp.Open;
end;

function TdlgTmpRouteSelect.IIFString(bYes:Boolean; str1, str2: WideString): WideString;
begin
  if bYes then
    Result := (str1)
  else
    Result := (str2);
end;

procedure TdlgTmpRouteSelect.SetSubList(Sender: Tobject);
begin
  with qryDtl do
  begin
     Close;
     Parameters.ParamByName('ItemId').Value := CurrLSystemId;
     open;
  end;
end;

end.

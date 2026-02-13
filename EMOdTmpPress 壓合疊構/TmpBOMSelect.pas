unit TmpBOMSelect;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, ComCtrls, JSdTreeView, ExtCtrls, StdCtrls, JSdLabel,
  Buttons, Grids, Wwdbigrd, Wwdbgrid, DB, ADODB, JSdTable, JSdDBGrid, ImgList;

type
  TdlgTmpBOMSelect = class(TfrmTempDlgDLL)
    qryTmpMas: TJSdTable;
    dsTmpMas: TDataSource;
    qryTmpDtl: TADOQuery;
    qryTmpDtlTmpId: TStringField;
    qryTmpDtlLayerId: TStringField;
    qryTmpDtlAftLayerId: TStringField;
    qryTmpDtlIssLayer: TIntegerField;
    qryTmpDtlDegree: TIntegerField;
    dsTmpMapDtl: TDataSource;
    pgeMaster: TPageControl;
    TabSheet1: TTabSheet;
    TabSheet2: TTabSheet;
    SpeedButton1: TSpeedButton;
    chkStatus: TJSdLabel;
    chkTmpId: TJSdLabel;
    chkNotes: TJSdLabel;
    rdoStatus: TRadioGroup;
    edtTmpId: TEdit;
    edtNotes: TEdit;
    Splitter1: TSplitter;
    trvBOM: TJSdTreeView;
    ImageList1: TImageList;
    grdData: TJSdDBGrid;
    qryTmpDtlSort: TIntegerField;
    qryTmpDtlLayerName: TWideStringField;
    procedure qryTmpMasAfterScroll(DataSet: TDataSet);
    procedure SpeedButton1Click(Sender: TObject);
    function IIFString(bYes:Boolean; str1, str2: WideString): WideString;
  private
    { Private declarations }
  public
    var iTmpActive: Integer;
    procedure Add2Filter(var sFilter  :string; const sCondition :string);
    { Public declarations }
  end;

var
  dlgTmpBOMSelect: TdlgTmpBOMSelect;

implementation

{$R *.dfm}

procedure TdlgTmpBOMSelect.qryTmpMasAfterScroll(DataSet: TDataSet);
begin
  inherited;
  trvBOM.Setup;
  trvBOM.FullExpand;
end;

procedure TdlgTmpBOMSelect.SpeedButton1Click(Sender: TObject);
var sSQLComd: string;
begin
  inherited;
  sSQLComd:='';
  if edtTmpId.text<>'' then
   begin
      //Add2Filter(sSQLComd,'TmpId >= '''+ edtTmpId.text+'''');
      //Add2Filter(sSQLComd,'TmpId < '''+ edtTmpId.text+char(65535)+'''');
      Add2Filter(sSQLComd,'TmpId like ''%'+ edtTmpId.text+'%'''); //2010.02.23
   end;
   if edtNotes.text<>'' then
   begin
      //Add2Filter(sSQLComd,'Notes >= '''+ edtNotes.text+'''');
      //Add2Filter(sSQLComd,'Notes < '''+ edtNotes.text+char(65535)+'''');
      Add2Filter(sSQLComd,'Notes like ''%'+ edtNotes.text+'%'''); //2010.02.23
   end;
   //¼f®Ö¾÷¨î
   if iTmpActive=1 then
   begin
       Add2Filter(sSQLComd,'Status =1');
       chkStatus.Visible:=False;
       rdoStatus.Visible:=False;
   end;

  if sSQLComd<>'' then sSQLComd:=' Where '+sSQLComd; //2010.02.23

  with qryTmpMas do
  begin
    if State in [dsEdit,dsInsert] then Post;
    Close;
    SQL.Clear;
    SQL.Add('select * from dbo.EMOdTmpBOMMas '+sSQLComd);
     {filter:= sSQLComd;
     filtered:= true;
     if Active then
        refresh
     else
        open;}
    Open;
  end;
  pgeMaster.ActivePage := TabSheet1;
end;

procedure TdlgTmpBOMSelect.Add2Filter(var sFilter  :string; const sCondition :string);
begin
   sFilter := sFilter + IIFString(sFilter='', '', ' AND ') + sCondition;
end;

function TdlgTmpBOMSelect.IIFString(bYes:Boolean; str1, str2: WideString): WideString;
begin
  if bYes then
    Result := (str1)
  else
    Result := (str2);
end;

end.

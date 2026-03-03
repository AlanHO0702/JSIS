unit PartNumViewer;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempPublic, DB, ADODB, Buttons, ExtCtrls, JSdTable, StdCtrls,
  wwdbdatetimepicker, Grids, Wwdbigrd, Wwdbgrid, JSdDBGrid, ComCtrls,
  JSdLookupCombo;

type
  TfrmFMEdPartNumViewer = class(TfrmTempPublic)
    Panel1: TPanel;
    labPaperDate: TLabel;
    imgPaperDate: TImage;
    Label3D2: TLabel;
    btFind: TSpeedButton;
    Label1: TLabel;
    pnlXExit: TPanel;
    edtFPaperDate: TwwDBDateTimePicker;
    edtEPaperDate: TwwDBDateTimePicker;
    edtRevision: TEdit;
    edtDateCode: TEdit;
    qryWIP: TJSdTable;
    qryIssue: TJSdTable;
    qryPass: TJSdTable;
    dsWIP: TDataSource;
    dsIssue: TDataSource;
    dsPass: TDataSource;
    qryInFFG: TJSdTable;
    qryScrap: TJSdTable;
    qryMRBScrap: TJSdTable;
    dsInFFG: TDataSource;
    dsScrap: TDataSource;
    dsMRBScrap: TDataSource;
    pagData: TPageControl;
    TabSheet1: TTabSheet;
    grdWIP: TJSdDBGrid;
    TabSheet2: TTabSheet;
    grdIssue: TJSdDBGrid;
    TabSheet3: TTabSheet;
    grdPass: TJSdDBGrid;
    TabSheet4: TTabSheet;
    JSdDBGrid2: TJSdDBGrid;
    TabSheet5: TTabSheet;
    grdScrap: TJSdDBGrid;
    TabSheet6: TTabSheet;
    grdMRBScrap: TJSdDBGrid;
    edtPartnum: TJSdLookupCombo;
    qryPartNum: TJSdTable;
    dsPartNum: TDataSource;
    procedure pagDataChange(Sender: TObject);
    procedure btnGetParamsClick(Sender: TObject);
    procedure btFindClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmFMEdPartNumViewer: TfrmFMEdPartNumViewer;

implementation

uses commParent;

{$R *.dfm}

procedure TfrmFMEdPartNumViewer.btFindClick(Sender: TObject);
begin
  inherited;
  case pagData.ActivePageIndex of
    0:  //WIP
      begin
        with qryWIP do
        begin
          //Parameters.ParamByName('PartNum').Value := edtPartnum.Text;
          //Parameters.ParamByName('Revision').Value := edtRevision.Text;
          //Parameters.ParamByName('DateCode').Value := edtDateCode.Text;
          Close;
          //2019.05.15
          SQL.Clear;
          SQL.Add('Exec FMEdPartNumView_WIP '''+edtPartnum.Text+''' ,'
            +''''+edtRevision.Text+''' ,'''+edtDateCode.Text+'''');
          Open;
        end;
      end;
    1:  //Issue
      begin
        with qryIssue do
        begin
          //Parameters.ParamByName('PartNum').Value := edtPartnum.Text;
          //Parameters.ParamByName('Revision').Value := edtRevision.Text;
          //Parameters.ParamByName('DateCode').Value := edtDateCode.Text;
          //Parameters.ParamByName('FPaperDate').Value := edtFPaperDate.Date;
          //Parameters.ParamByName('EPaperDate').Value := edtEPaperDate.Date;
          Close;
          //2019.05.15
          SQL.Clear;
          SQL.Add('Exec FMEdPartNumView_Issue '''+edtPartnum.Text+''' ,'
            +''''+edtRevision.Text+''' ,'''+edtDateCode.Text+''','
            +''''+edtFPaperDate.Text+''','''+edtEPaperDate.Text+'''');
          Open;
        end;
      end;
    2:  //Pass
      begin
        with qryPass do
        begin
          //Parameters.ParamByName('PartNum').Value := edtPartnum.Text;
          //Parameters.ParamByName('Revision').Value := edtRevision.Text;
          //Parameters.ParamByName('DateCode').Value := edtDateCode.Text;
          //Parameters.ParamByName('FPaperDate').Value := edtFPaperDate.Date;
          //Parameters.ParamByName('EPaperDate').Value := edtEPaperDate.Date;
          Close;
          //2019.05.15
          SQL.Clear;
          SQL.Add('Exec FMEdPartNumView_Pass '''+edtPartnum.Text+''' ,'
            +''''+edtRevision.Text+''' ,'''+edtDateCode.Text+''','
            +''''+edtFPaperDate.Text+''','''+edtEPaperDate.Text+'''');
          Open;
        end;
      end;
    3:  //InFFG
      begin
        with qryInFFG do
        begin
          //Parameters.ParamByName('PartNum').Value := edtPartnum.Text;
          //Parameters.ParamByName('Revision').Value := edtRevision.Text;
          //Parameters.ParamByName('DateCode').Value := edtDateCode.Text;
          //Parameters.ParamByName('FPaperDate').Value := edtFPaperDate.Date;
          //Parameters.ParamByName('EPaperDate').Value := edtEPaperDate.Date;
          Close;
          //2019.05.15
          SQL.Clear;
          SQL.Add('Exec FMEdPartNumView_InFFG '''+edtPartnum.Text+''' ,'
            +''''+edtRevision.Text+''' ,'''+edtDateCode.Text+''','
            +''''+edtFPaperDate.Text+''','''+edtEPaperDate.Text+'''');
          Open;
        end;
      end;
    4:  //Scrap
      begin
        with qryScrap do
        begin
          //Parameters.ParamByName('PartNum').Value := edtPartnum.Text;
          //Parameters.ParamByName('Revision').Value := edtRevision.Text;
          //Parameters.ParamByName('DateCode').Value := edtDateCode.Text;
          //Parameters.ParamByName('FPaperDate').Value := edtFPaperDate.Date;
          //Parameters.ParamByName('EPaperDate').Value := edtEPaperDate.Date;
          Close;
          //2019.05.15
          SQL.Clear;
          SQL.Add('Exec FMEdPartNumView_Scrap '''+edtPartnum.Text+''' ,'
            +''''+edtRevision.Text+''' ,'''+edtDateCode.Text+''','
            +''''+edtFPaperDate.Text+''','''+edtEPaperDate.Text+'''');
          Open;
        end;
      end;
    5:  //MRBScrap
      begin
        with qryMRBScrap do
        begin
          //Parameters.ParamByName('PartNum').Value := edtPartnum.Text;
          //Parameters.ParamByName('Revision').Value := edtRevision.Text;
          //Parameters.ParamByName('DateCode').Value := edtDateCode.Text;
          //Parameters.ParamByName('FPaperDate').Value := edtFPaperDate.Date;
          //Parameters.ParamByName('EPaperDate').Value := edtEPaperDate.Date;
          Close;
          //2019.05.15
          SQL.Clear;
          SQL.Add('Exec FMEdPartNumView_MRBScrap '''+edtPartnum.Text+''' ,'
            +''''+edtRevision.Text+''' ,'''+edtDateCode.Text+''','
            +''''+edtFPaperDate.Text+''','''+edtEPaperDate.Text+'''');
          Open;
        end;
      end;

  end;

end;

procedure TfrmFMEdPartNumViewer.btnGetParamsClick(Sender: TObject);
begin
  inherited;
  pagData.ActivePageIndex := 0;
  edtFPaperDate.Date := Now -30;
  pagData.OnChange(Sender);

  //2019.05.15
  qryWIP.TableName:='FMEdPartNumView_WIP';
  qryWIP.LookupType:=lkLookupTable;
  qryIssue.TableName:='FMEdPartNumView_Issue';
  qryIssue.LookupType:=lkLookupTable;
  qryPass.TableName:='FMEdPartNumView_Pass';
  qryPass.LookupType:=lkLookupTable;
  qryInFFG.TableName:='FMEdPartNumView_InFFG';
  qryInFFG.LookupType:=lkLookupTable;
  qryScrap.TableName:='FMEdPartNumView_Scrap';
  qryScrap.LookupType:=lkLookupTable;
  qryMRBScrap.TableName:='FMEdPartNumView_MRBScrap';
  qryMRBScrap.LookupType:=lkLookupTable;
  qryPartNum.Close;
  qryPartNum.Open;
end;

procedure TfrmFMEdPartNumViewer.pagDataChange(Sender: TObject);
begin
  inherited;
  if pagData.ActivePageIndex in [0] then
  begin
    labPaperDate.Visible := False;
    imgPaperDate.Visible := False;
    edtFPaperDate.Visible := False;
    edtEPaperDate.Visible := False;
  end else
  begin
    labPaperDate.Visible := True;
    imgPaperDate.Visible := True;
    edtFPaperDate.Visible := True;
    edtEPaperDate.Visible := True;

  end;
end;

end.

unit NotValueShow;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, DB, ADODB, DBCtrls, Mask,
  wwdbedit;

type
  TdlgNotValueShow = class(TfrmTempDlgDLL)
    qryNotValueShow: TADOQuery;
    qryNotValueShowIsError: TIntegerField;
    DataSource1: TDataSource;
    qryCheckLayerRoute: TADOQuery;
    qryCheckLayerRouteS: TIntegerField;
    Panel1: TPanel;
    qryNotValueShowS: TStringField;
    DBMemo1: TDBMemo;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  dlgNotValueShow: TdlgNotValueShow;

implementation

{$R *.dfm}

end.

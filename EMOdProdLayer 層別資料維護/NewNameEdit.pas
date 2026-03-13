unit NewNameEdit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, JSdLabel;

type
  TdlgNewNameEdit = class(TfrmTempDlgDLL)
    Label1: TJSdLabel;
    Edit1: TEdit;
    Label2: TJSdLabel;
    Label4: TJSdLabel;
    edtNotes: TEdit;
    Label3: TJSdLabel;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  dlgNewNameEdit: TdlgNewNameEdit;

implementation

{$R *.dfm}

end.

unit MapEdit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, Menus, XFlowUtils,
  PaintObject, XFlowControl, XFlowLine, XFlowDrawLine;

type
  TfrmMapEdit = class(TfrmTempDlgDLL)
    popTools: TPopupMenu;
    N1: TMenuItem;
    N2: TMenuItem;
    Panel1: TPanel;
    btnCirF: TSpeedButton;
    btnCirE: TSpeedButton;
    btnCirO: TSpeedButton;
    btnSquF: TSpeedButton;
    btnSquE: TSpeedButton;
    btnSquO: TSpeedButton;
    btnVer: TSpeedButton;
    btnVerT: TSpeedButton;
    btnVerA: TSpeedButton;
    btnVerB: TSpeedButton;
    btnHor: TSpeedButton;
    btnHorL: TSpeedButton;
    btnHorA: TSpeedButton;
    btnHorR: TSpeedButton;
    btnText: TSpeedButton;
    btnCom: TSpeedButton;
    btnConvers: TSpeedButton;
    SpeedButton2: TSpeedButton;
    SpeedButton3: TSpeedButton;
    SpeedButton4: TSpeedButton;
    Panel3: TPanel;
    btnSave: TSpeedButton;
    btn: TSpeedButton;
    GroupBox1: TGroupBox;
    cboSize: TComboBox;
    btnSetSize: TSpeedButton;
    btnSetAllSize: TSpeedButton;
    Panel4: TPanel;
    btnCopy: TSpeedButton;
    btnPaste: TSpeedButton;
    btnFront: TSpeedButton;
    btnBack: TSpeedButton;
    btnClear: TSpeedButton;
    btnClearAll: TSpeedButton;
    btnHTwoL: TSpeedButton;
    btnVTwoL: TSpeedButton;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure SpeedButton4Click(Sender: TObject);
    procedure N1Click(Sender: TObject);
    procedure N2Click(Sender: TObject);
    procedure FormClick(Sender: TObject);
    procedure SpeedButton2Click(Sender: TObject);
    procedure SpeedButton3Click(Sender: TObject);
    procedure btnSquFClick(Sender: TObject);
    procedure btnSquEClick(Sender: TObject);
    procedure btnCirFClick(Sender: TObject);
    procedure btnCirOClick(Sender: TObject);
    procedure btnCirEClick(Sender: TObject);
    procedure btnTextClick(Sender: TObject);
    procedure btnHorClick(Sender: TObject);
    procedure btnVerClick(Sender: TObject);
    procedure btnVerTClick(Sender: TObject);
    procedure btnVerBClick(Sender: TObject);
    procedure btnVerAClick(Sender: TObject);
    procedure btnHorLClick(Sender: TObject);
    procedure btnHorRClick(Sender: TObject);
    procedure btnHorAClick(Sender: TObject);
    procedure btnComClick(Sender: TObject);
    procedure btnConversClick(Sender: TObject);
    procedure btnSquOClick(Sender: TObject);
    procedure FormKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure btnSaveClick(Sender: TObject);
    procedure btnClick(Sender: TObject);
    procedure btnSetSizeClick(Sender: TObject);
    procedure btnSetAllSizeClick(Sender: TObject);
    procedure btnPasteClick(Sender: TObject);
    procedure btnCopyClick(Sender: TObject);
    procedure btnFrontClick(Sender: TObject);
    procedure btnBackClick(Sender: TObject);
    procedure btnHTwoLClick(Sender: TObject);
    procedure btnVTwoLClick(Sender: TObject);
  private
    { Private declarations }
  public
    sControlText :string;
    { Public declarations }
  end;

var
  frmMapEdit: TfrmMapEdit;

implementation

{$R *.dfm}

procedure TfrmMapEdit.btnBackClick(Sender: TObject);
var List: TStringList;
    Ctrl: TXFlowControl;
    XLine: TXFlowDrawLine;
begin
  inherited;
  List := GetFocusControls;
  if List.Count > 0 then
  begin
    if List.Objects[0] is TXFlowControl then
    begin
      Ctrl := TXFlowControl(List.Objects[0]);
      Ctrl.SendToBack;
    end
    else if List.Objects[0] is TXFlowDrawLine then
    begin
      XLine := TXFlowDrawLine(List.Objects[0]);
      XLine.SendToBack;
    end;
  end;
end;

procedure TfrmMapEdit.btnCirEClick(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.BrushStyle := bsClear;
  XCtrl.Style := csEllipse;
  //2010.03.08
  XCtrl.Caption:='';
end;

procedure TfrmMapEdit.btnCirFClick(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.Color := clTeal;
  XCtrl.Style := csEllipse;
  //2010.03.08
  XCtrl.Caption:='';
end;

procedure TfrmMapEdit.btnCirOClick(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.Style := csEllipse;
  XCtrl.Color := clTeal;
  XCtrl.BrushStyle := bsBDiagonal;
  //2010.03.08
  XCtrl.Caption:='';
end;

procedure TfrmMapEdit.btnComClick(Sender: TObject);
var
  List: TStringList;
  XCtrl1, XCtrl2: TXFlowControl;
begin
  inherited;
  List := GetFocusControls;
  if (List.Count = 2) and (List.Objects[0] is TXFlowControl) and (List.Objects[1] is TXFlowControl)then
  begin
    XCtrl1 := TXFlowControl(List.Objects[0]);
    XCtrl2 := TXFlowControl(List.Objects[1]);
    NewXFlowLine(Self, XCtrl1, XCtrl2);
  end;
end;

procedure TfrmMapEdit.btnConversClick(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 40, 40);
  XCtrl.Style := csActivity;
  XCtrl.ActivityType := atConvers;
  XCtrl.Caption := '';
end;

procedure TfrmMapEdit.btnHorAClick(Sender: TObject);
var
  XLine: TXFlowDrawLine;
begin
  inherited;
  XLine := NewXFlowDrawLine(Self, 100, 100, ltHorz);
  XLine.ArrowType := atTwice2;
end;

procedure TfrmMapEdit.btnHorClick(Sender: TObject);
begin
  inherited;
  NewXFlowDrawLine(Self, 100, 100, ltHorz);
end;

procedure TfrmMapEdit.btnHorLClick(Sender: TObject);
var
  XLine: TXFlowDrawLine;
begin
  inherited;
  XLine := NewXFlowDrawLine(Self, 100, 100, ltHorz);
  XLine.ArrowType := atSource;
end;

procedure TfrmMapEdit.btnHorRClick(Sender: TObject);
var
  XLine: TXFlowDrawLine;
begin
  inherited;
  XLine := NewXFlowDrawLine(Self, 100, 100, ltHorz);
  XLine.ArrowType := atTarget;
end;

procedure TfrmMapEdit.btnHTwoLClick(Sender: TObject);
var XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.Style := csTwoLineHorz;
  XCtrl.BrushStyle := bsClear;
  XCtrl.Caption:='';
end;

procedure TfrmMapEdit.btnVTwoLClick(Sender: TObject);
var XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.Style := csTwoLineVert;
  XCtrl.BrushStyle := bsClear;
  XCtrl.Caption:='';
end;

procedure TfrmMapEdit.btnSquEClick(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.Style := csRectangle;
  XCtrl.BrushStyle := bsClear;
  //2010.03.08
  XCtrl.Caption:='';
end;

procedure TfrmMapEdit.btnSquFClick(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.Color := clTeal;
  //2010.03.08
  XCtrl.Caption:='';
end;

procedure TfrmMapEdit.btnSquOClick(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.Color := clTeal;
  XCtrl.BrushStyle := bsBDiagonal;
  //2010.03.08
  XCtrl.Caption:='';
end;

procedure TfrmMapEdit.btnTextClick(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 100, 100);
  XCtrl.Style := csText;
  XCtrl.Font.Size :=10;
  XCtrl.Caption := 'New';
  XCtrl.Alignment := taCenter;
end;

procedure TfrmMapEdit.btnVerAClick(Sender: TObject);
var
  XLine: TXFlowDrawLine;
begin
  inherited;
  XLine := NewXFlowDrawLine(Self, 100, 100, ltVert);
  XLine.ArrowType := atTwice2;
end;

procedure TfrmMapEdit.btnVerBClick(Sender: TObject);
var
  XLine: TXFlowDrawLine;
begin
  inherited;
  XLine := NewXFlowDrawLine(Self, 100, 100, ltVert);
  XLine.ArrowType := atTarget;
end;

procedure TfrmMapEdit.btnVerClick(Sender: TObject);
begin
  inherited;
  NewXFlowDrawLine(Self, 100, 100, ltVert);
end;

procedure TfrmMapEdit.btnVerTClick(Sender: TObject);
var
  XLine: TXFlowDrawLine;
begin
  inherited;
  XLine := NewXFlowDrawLine(Self, 100, 100, ltVert);
  XLine.ArrowType := atSource;
end;

procedure TfrmMapEdit.FormClick(Sender: TObject);
begin
  inherited;
  ClearFocus(Self);
end;

procedure TfrmMapEdit.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  inherited;
  ClearFocus(Self);
end;

procedure TfrmMapEdit.FormKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
var List: TStringList;
    Ctrl: TXFlowControl;
    XLine: TXFlowDrawLine;
begin
  inherited;
  //100608 add 鍵盤操控
  List := GetFocusControls;
  if List.Count > 0 then
  begin
    if List.Objects[0] is TXFlowControl then
    begin
      Ctrl := TXFlowControl(List.Objects[0]);
      case Key of
        VK_DELETE: N1Click(Sender);
        //方向鍵要把BitBtn 隱藏才能抓到
        VK_UP   : Ctrl.top :=Ctrl.top -1;
        VK_DOWN : Ctrl.top :=Ctrl.top +1;
        VK_LEFT : Ctrl.Left:=Ctrl.Left-1;
        VK_RIGHT: Ctrl.Left:=Ctrl.Left+1;
      end;
    end
    else if List.Objects[0] is TXFlowDrawLine then
    begin
      XLine := TXFlowDrawLine(List.Objects[0]);
      case Key of
        VK_DELETE: N1Click(Sender);
        //方向鍵要把BitBtn 隱藏才能抓到
        VK_UP   : XLine.top :=XLine.top -1;
        VK_DOWN : XLine.top :=XLine.top +1;
        VK_LEFT : XLine.Left:=XLine.Left-1;
        VK_RIGHT: XLine.Left:=XLine.Left+1;
      end;
    end;
  end;
end;

procedure TfrmMapEdit.N1Click(Sender: TObject);
begin
  inherited;
  DeleteFocusControls(self);
end;

procedure TfrmMapEdit.N2Click(Sender: TObject);
begin
  inherited;
  ClearAll(self);
end;

procedure TfrmMapEdit.SpeedButton2Click(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 40, 40);
  XCtrl.Style := csActivity;
  XCtrl.ActivityType := atPlus;
  XCtrl.Caption := '';
end;

procedure TfrmMapEdit.SpeedButton3Click(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 40, 40);
  XCtrl.Style := csActivity;
  XCtrl.ActivityType := atPlus90;//270
  XCtrl.Caption := '';
end;

procedure TfrmMapEdit.SpeedButton4Click(Sender: TObject);
var
  XCtrl: TXFlowControl;
begin
  inherited;
  XCtrl := NewXFlowControl(Self, 40, 40);
  XCtrl.Style := csActivity;
  XCtrl.ActivityType := atPlus270;
  XCtrl.Caption := '';
end;

procedure TfrmMapEdit.btnSetAllSizeClick(Sender: TObject);
var i:integer;
begin
  ClearFocus(self);
  if cboSize.Text='' then
    Exit;
  i := 0;
  while i < self.ControlCount do
  begin
    if self.Controls[i] is TXFlowControl then
    begin
      if TXFlowControl(self.Controls[i]).Style = csText then
        TXFlowControl(self.Controls[i]).Font.Size :=StrToInt(cboSize.Text);
    end;
    i := i + 1;
  end;
end;

procedure TfrmMapEdit.btnSetSizeClick(Sender: TObject);
var List: TStringList;
    Ctrl: TXFlowControl;
begin
  inherited;
  if cboSize.Text='' then
    Exit;
  List := GetFocusControls;
  if List.Count > 0 then
  begin
    if List.Objects[0] is TXFlowControl then
    begin
      Ctrl := TXFlowControl(List.Objects[0]);
      if Ctrl.Style = csText then
        Ctrl.Font.Size :=StrToInt(cboSize.Text);
    end;
  end;
end;

procedure TfrmMapEdit.btnSaveClick(Sender: TObject);
begin
  inherited;
  ModalResult:=mrOk;
end;

procedure TfrmMapEdit.btnClick(Sender: TObject);
begin
  inherited;
  ModalResult:=mrCancel;
end;

procedure TfrmMapEdit.btnCopyClick(Sender: TObject);
begin
  inherited;
  CopyFocusControls(Self);
end;

procedure TfrmMapEdit.btnPasteClick(Sender: TObject);
begin
  inherited;
  PasteControls(Self);
end;

procedure TfrmMapEdit.btnFrontClick(Sender: TObject);
var List: TStringList;
    Ctrl: TXFlowControl;
    XLine: TXFlowDrawLine;
begin
  inherited;
  List := GetFocusControls;
  if List.Count > 0 then
  begin
    if List.Objects[0] is TXFlowControl then
    begin
      Ctrl := TXFlowControl(List.Objects[0]);
      Ctrl.BringToFront;
    end
    else if List.Objects[0] is TXFlowDrawLine then
    begin
      XLine := TXFlowDrawLine(List.Objects[0]);
      XLine.BringToFront
    end;
  end;
end;

end.

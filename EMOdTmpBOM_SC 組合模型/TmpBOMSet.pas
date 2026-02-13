unit TmpBOMSet;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, JSdLabel, Menus, DB,
  ADODB, Vcl.Samples.Spin;

type
   pLayerInf = ^TLayerInf;
   TLayerInf = Record
     Id: String;
     AftId: String;
     ActNum: integer;
     Degree: integer;
     FL: integer;
     EL: integer;
end;

type
  TdlgTmpBOMSet = class(TfrmTempDlgDLL)
    qryTmpBOMDtl: TADOQuery;
    qryTmpBOMIns: TADOQuery;
    popTools: TPopupMenu;
    N1: TMenuItem;
    N2: TMenuItem;
    qryCheck: TADOQuery;
    mmoData: TMemo;
    pnlSetPress: TPanel;
    btnLayer: TJSdLabel;
    btnPress: TJSdLabel;
    spnLayer: TSpinEdit;
    spnPress: TSpinEdit;
    pnlAll: TPanel;
    pnlPress: TPanel;
    edtOriName: TEdit;
    Label1: TLabel;
    Button1: TButton;
    procedure N1Click(Sender: TObject);
    procedure N2Click(Sender: TObject);
    procedure spnLayerChange(Sender: TObject);
    procedure spnPressChange(Sender: TObject);
    procedure mmoDataClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure Button1Click(Sender: TObject); //2011.07.26
  private
    function  GetAftLayer(bLayer : TSpeedButton):string;
    function  CheckDu(ActNum, FL, EL :integer):Boolean;
    procedure CreateLayer(ActNum, FL, EL :integer; sLayerName:String);
    procedure DoPress(sender : TObject);
    procedure Locked(ActNum, FL, EL :integer);
    procedure DoNamePress(sender : TObject); //2011.07.25
    { Private declarations }
  public
    hStep, vStep: integer;
    TmpId: string;
    DownBtnName: string;
    iReadOnly: Integer;
    procedure DrawLayerGrid(Layer: Integer); virtual;
    procedure DrawPressGrid(Press: Integer); virtual;
    procedure Controls2Memo(Sender: TObject); virtual;
    procedure Memo2DB(Sender: TObject); virtual;
    //還原
    procedure DB2Memo(Sender: TObject); virtual;
    procedure Memo2Controls(Sender: TObject); virtual;
    procedure ParseFormat(sInput, sDim: String; var sParam : array of String; iPara:integer);
    procedure btnCreateCoor;
    { Public declarations }
  end;

var
  dlgTmpBOMSet: TdlgTmpBOMSet;

implementation

{$R *.dfm}

 var CntLayer: integer;
 const baseSize = 16;
       par = '_';

function TdlgTmpBOMSet.GetAftLayer(bLayer : TSpeedButton): string;
var i, iDeg, iTagMax, iNextDegree : integer;
    fL, eL, AftfL, AfteL :integer;
begin
   {2011.07.26 原Caption 回寫時會用到，改用Hint取代}
   //取出起迄層別
   fL := strtoint(copy(Tspeedbutton(bLayer).Hint{Caption},
              1, -1+pos(par,Tspeedbutton(bLayer).Hint{Caption})));
   eL := strtoint(copy(Tspeedbutton(bLayer).Hint{Caption},
                      1+pos(par,Tspeedbutton(bLayer).Hint{Caption} ),
                      Length(Tspeedbutton(bLayer).Hint{Caption})
                      ));
   iDeg := Tspeedbutton(bLayer).Tag;
   iTagMax:= iDeg;
   iNextDegree:= 1;
   Result := '0_0';    //找不到的話，就當成 0_0
   for i := 0 to pnlAll.controlcount - 1 do
   begin
      if pnlAll.Controls[i] is Tspeedbutton then
      begin
         if Tspeedbutton(pnlAll.Controls[i]).Tag>iTagMax then
            iTagMax:= Tspeedbutton(pnlAll.Controls[i]).Tag;
         if Tspeedbutton(pnlAll.Controls[i]).Tag = iDeg+iNextDegree then
         begin
            AftfL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                        1, -1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})));
            AfteL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                                1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption} ),
                                Length(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})
                                ));

            if ((fL >= AftfL)and(eL <= AfteL)) then
            begin
               Result := inttostr(AftfL)+par+inttostr(AfteL);
               break;
            end;
         end;
      end;
   end;

   while ((Result = '0_0') and (iTagMax>(iDeg+iNextDegree))) do
   begin
     iNextDegree:= iNextDegree+1;
     for i := 0 to pnlAll.controlcount - 1 do
     begin
        if pnlAll.Controls[i] is Tspeedbutton then
        begin
           if Tspeedbutton(pnlAll.Controls[i]).Tag = iDeg+iNextDegree then
           begin
              AftfL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                          1, -1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})));
              AfteL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                                  1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption} ),
                                  Length(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})
                                  ));

              if ((fL >= AftfL)and(eL <= AfteL)) then
              begin
                 Result := inttostr(AftfL)+par+inttostr(AfteL);
                 break;
              end;
           end;
        end;
     end;
   end;
end;

procedure TdlgTmpBOMSet.Button1Click(Sender: TObject); //2011.07.26
var i: integer;
begin
  inherited;
  if DownBtnName<>'' then
  begin
    TSpeedButton(FindComponent(DownBtnName)).Caption:= edtOriName.Text;
    //2022.03.10
    for i := 0 to pnlAll.controlcount - 1 do
    begin
      if pnlAll.Controls[i] is Tspeedbutton then
      begin
           if Tspeedbutton(pnlAll.Controls[i]).down then
           begin
              Tspeedbutton(pnlAll.Controls[i]).Down := false;
           end;
      end;
    end;
  end;
end;

function TdlgTmpBOMSet.CheckDu(ActNum, FL, EL: integer):Boolean;
var i: integer;
    BeffL, BefeL :integer;
begin
   {2011.07.26 原Caption 回寫時會用到，改用Hint取代}
   Result := true;
   for i := 0 to pnlAll.controlcount - 1 do
   begin
      if pnlAll.Controls[i] is Tspeedbutton then
      begin
         begin
            BeffL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                        1, -1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})));
            BefeL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                                1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption} ),
                                Length(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})
                                ));

            if (BeffL = BefeL) then continue
            else
            if ((fL < BeffL ) and (eL < BeffL )) then continue // check head
            else
            if ((fL > BefeL ) and (eL > BefeL )) then continue //check tail
            else
            if ((fL <= BeffL ) and (eL >= BefeL ) //check larger then bef
                and not(ActNum = Tspeedbutton(pnlAll.Controls[i]).Tag )) then continue
            else
            begin
               Result := False;
            end;
         end;
      end;
   end;
end;

procedure TdlgTmpBOMSet.CreateLayer(ActNum, FL, EL: integer; sLayerName:String);
var shp : Tspeedbutton;
begin
  //2011.08.16
  if iReadOnly=1 then
    Exit;

  try
      shp := Tspeedbutton.Create(self);
      with shp do
      begin
        name := 'S'+ par +inttostr(ActNum) + par + inttostr(FL) + par + inttostr(EL);
        Parent := pnlAll;
        {2011.07.26}
        if ((sLayerName='') or (sLayerName=inttostr(FL)+'~'+inttostr(EL))) then
          caption := 'L'+inttostr(FL)+'~'+inttostr(EL)//inttostr(FL)+par+inttostr(EL)//2022.03.10
        else
          caption:=sLayerName;
        Hint:= inttostr(FL)+par+inttostr(EL); //2011.07.26 原Caption 回寫時會用到，改用Hint取代

        inc(CntLayer);
        groupindex := CntLayer;
        AllowAllup := true;
        onclick := DoNamePress; //2011.07.25
        tag := ActNum;
        PopupMenu:= popTools;
        top := FL * vStep;
        Left := ActNum * hStep;
        Height := (EL-FL) * vStep + baseSize;
        Width := hStep;
      end;
   except
      shp.free;
   end;
end;

procedure TdlgTmpBOMSet.DoPress(sender: TObject);
var i : integer;
    fL, eL, minfL, maxeL :integer;
begin
   {2011.07.26 原Caption 回寫時會用到，改用Hint取代}
   minfL := 255; maxeL := -1;
   for i := 0 to pnlAll.controlcount - 1 do
   begin
      if pnlAll.Controls[i] is Tspeedbutton then
      begin
         if Tspeedbutton(pnlAll.Controls[i]).Tag < Tbutton(sender).Tag then
         begin
           if Tspeedbutton(pnlAll.Controls[i]).down then
           begin
              fL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                          1, -1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})));
              eL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                                  1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption} ),
                                  Length(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})
                                  ));

              if fL < minfL then minfL := fL ;
              if eL > maxeL then maxeL := eL ;

              Tspeedbutton(pnlAll.Controls[i]).Down := false;
           end;
         end;
      end;
   end;

   if ((minfL = 255) or (maxeL = -1) ) then exit;
   if CheckDu(Tcontrol(sender).tag, minfL, MaxeL) then
   begin
      createLayer(Tcontrol(sender).tag, minfL, MaxeL, ''{2011.07.26});
   end;
end;

procedure TdlgTmpBOMSet.DoNamePress(sender: TObject); //2011.07.26 add
begin
   if TSpeedButton(Sender).Down then
   begin
     DownBtnName:=TSpeedButton(Sender).Name;
     edtOriName.text:= TSpeedButton(Sender).Caption;
   end
   else
   begin
     DownBtnName:='';
     edtOriName.text:='';
   end;
end;

procedure TdlgTmpBOMSet.Locked(ActNum, FL, EL :integer);
var i: integer;
    BeffL, BefeL :integer;
begin
   {2011.07.26 原Caption 回寫時會用到，改用Hint取代}
   for i := 0 to pnlAll.controlcount - 1 do
   begin
      if pnlAll.Controls[i] is Tspeedbutton then
      begin
         if Tspeedbutton(pnlAll.Controls[i]).Tag <= ActNum then
         begin
            BeffL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                        1, -1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})));
            BefeL := strtoint(copy(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption},
                                1+pos(par,Tspeedbutton(pnlAll.Controls[i]).Hint{Caption} ),
                                Length(Tspeedbutton(pnlAll.Controls[i]).Hint{Caption})
                                ));

            if ((BeffL >= fL )and(BefeL <= eL)) then
            begin
               Tspeedbutton(pnlAll.Controls[i]).Enabled:=false;
            end;
         end;
      end;
   end;
end;

procedure TdlgTmpBOMSet.DrawLayerGrid(Layer: Integer);
var i, j : integer;
begin
   for j := pnlAll.controlcount - 1 downto 0 do
   begin
         pnlAll.Controls[j].free;
   end;
  vStep := pnlAll.Height div (spnLayer.value + 1);
  hStep := pnlAll.width div (spnPress.value + 1 + 1);
  for i:= 0 to Layer do
  begin
      if i > 0 then
      begin
        createLayer(0, i, i, ''{2011.07.26});
      end;
  end;
end;

procedure TdlgTmpBOMSet.DrawPressGrid(Press: Integer);
var i, j: integer;
    lab : TSpeedButton;
begin
   for j := pnlPress.controlcount - 1 downto 0 do
   begin
         pnlPress.Controls[j].free;
   end;
  vStep := pnlAll.Height div (spnLayer.value + 1);
  hStep := pnlAll.width div (spnPress.value + 1 + 1);
  for i:= 1 to Press do
  begin
      lab := TSpeedButton.Create(self);
      with lab do
      begin
        name := 'btnPress'+inttostr(i);
        Parent := pnlPress;
        Top := 4;
        Left := i * hStep;
        Height := 26;
        Width := hStep;
        onclick := DoPress;
        tag := i;
        if i = 1 then
           caption := '發料'
        else
           caption := '壓合'+inttostr(i-1);
      end;
  end;
end;

procedure TdlgTmpBOMSet.FormCreate(Sender: TObject);
begin
  inherited;
  CntLayer := 1;
  iReadOnly:=0;
end;

procedure TdlgTmpBOMSet.Controls2Memo(Sender: TObject);
var i : integer;
begin
   mmoData.lines.clear; //2011.07.26 因為傳遞的參數加長了，所以前端Memo的寬度要夠大
   for i := 0 to pnlAll.controlcount - 1 do
   begin
      if pnlAll.Controls[i] is Tspeedbutton then
      begin
        if TspeedButton(pnlAll.Controls[i]).tag > 0 then
        begin
           mmoData.lines.add(TspeedButton(pnlAll.Controls[i]).Name
                              +par+GetAftLayer(TspeedButton(pnlAll.Controls[i]))
           +par+StringReplace(TspeedButton(pnlAll.Controls[i]).Caption,'_','~',[rfIgnoreCase]));//2011.07.26
        end;
      end;
   end;
end;

procedure TdlgTmpBOMSet.Memo2DB(Sender: TObject);
var i, iDegree, iIss : integer;
    sParam : Array[0..6] of string;
begin
   Controls2Memo(Sender);
   for i := 0 to mmoData.lines.count - 1 do
   begin
      parseformat(mmoData.lines[i], par, sParam, 7{6 2011.07.26});
      with qryTmpBOMIns do
      begin
         if sParam[1] = '1' then
            iIss:=1
         else
            iIss := 0;

         iDegree := strtoint(sParam[1]);
         //先處理由大到小，再由Cursor處理
         //TmpId, :IssLayer, :Degree, :FL, :EL, :AftFL, :AftEL
         Parameters.ParamByName('TmpId').Value:=TmpId;
         Parameters.ParamByName('IssLayer').Value:=iIss;
         Parameters.ParamByName('Degree').Value:=iDegree;
         Parameters.ParamByName('FL').Value:=strtoint(sParam[2]);
         Parameters.ParamByName('EL').Value:=strtoint(sParam[3]);
         Parameters.ParamByName('AftFL').Value:=strtoint(sParam[4]);
         Parameters.ParamByName('AftEL').Value:=strtoint(sParam[5]);
         Parameters.ParamByName('LayerName').Value:=sParam[6];
         execsql;
      end;
   end;
   with qryCheck do
   begin
      Parameters.ParamByName('TmpId').Value:=TmpId;
      execsql;
   end;
end;

procedure TdlgTmpBOMSet.mmoDataClick(Sender: TObject);
begin
  inherited;
  Controls2Memo(sender);
end;

procedure TdlgTmpBOMSet.N1Click(Sender: TObject);
var i : integer;
begin
  inherited;
  //2011.08.16
  if iReadOnly=1 then
    Exit;

  for i := pnlAll.controlcount - 1 downto 0 do
  begin
      if pnlAll.Controls[i] is Tspeedbutton then
      begin
         if (Tspeedbutton(pnlAll.Controls[i]).down) and
            (Tspeedbutton(pnlAll.Controls[i]).Tag > 0) then
         begin
            Tspeedbutton(pnlAll.Controls[i]).Destroy;
         end;
      end;
  end;
end;

procedure TdlgTmpBOMSet.N2Click(Sender: TObject);
begin
  inherited;
  //2011.08.16
  if iReadOnly=1 then
    Exit;

  DrawLayerGrid(spnLayer.Value);
end;

procedure TdlgTmpBOMSet.DB2Memo(Sender: TObject);
begin
   with qryTmpBOMDtl do
   begin
     Close;
     Parameters.ParamByName('TmpId0').Value:=TmpId;
     //Parameters.ParamByName('TmpId1').Value:=TmpId;
     Open;
     first;
     while not eof do
     begin
        mmoData.lines.add('S'
                        +par+inttostr(FieldByName('MaxDegree').Asinteger
                                        -FieldByName('Degree').Asinteger+1)
                        +par+FieldByName('FL').AsString
                        +par+FieldByName('EL').AsString
                        +par+FieldByName('LayerName').AsString{2011.07.26 add});
        Next;
     end;
     Close;
   end;
end;

procedure TdlgTmpBOMSet.Memo2Controls(Sender: TObject);
var i : integer;
    sParam : Array[0..4] of string;
begin
   for i := 0 to mmoData.lines.count - 1 do
   begin
      parseformat(mmoData.lines[i], par, sParam, 5{4 2011.07.26});
      createLayer(strtoint(sParam[1]), strtoint(sParam[2]), strtoint(sParam[3]),
                  sParam[4]{2011.07.26 add});
   end;
end;

procedure TdlgTmpBOMSet.ParseFormat(sInput, sDim: String; var sParam : array of String; iPara:integer);
var   i, iPos: integer;
begin
  for i:= 0 to (iPara-1) do sParam[i] := '';

  for i:= 0 to (iPara-1) do
  begin
    iPos := Pos(sDim, sInput);
    if iPos = 0 then iPos := (Length(sInput)+1);
    sParam[i] := Copy(sInput, 1, iPos-1);
    sInput := TrimRight(TrimLeft(Copy(sInput, iPos+1, Length(sInput))));
    if Length(sInput) < 1 then exit;
  end;
end;

procedure TdlgTmpBOMSet.btnCreateCoor;
begin
  DrawPressGrid(spnPress.Value);
  DrawLayerGrid(spnLayer.Value);
end;

procedure TdlgTmpBOMSet.spnLayerChange(Sender: TObject);
begin
  inherited;
  DrawLayerGrid(spnLayer.Value);
end;

procedure TdlgTmpBOMSet.spnPressChange(Sender: TObject);
begin
  inherited;
  DrawPressGrid(spnPress.Value);
  DrawLayerGrid(spnLayer.Value);
end;

end.

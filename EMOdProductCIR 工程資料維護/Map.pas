unit Map;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, TempDlgDLL, StdCtrls, Buttons, ExtCtrls, XFlowDrawBox, DB, ADODB;

type
  TfrmMap = class(TfrmTempDlgDLL)
    qryMapBmp: TADOQuery;
    qryMapBmpSerialNum: TWordField;
    XFlowDrawBox1: TXFlowDrawBox;
    qryExec: TADOQuery;
    qryMapBmp2: TADOQuery;
    qryMapBmp2LayerId: TStringField;
  private
    { Private declarations }
  public
    procedure Save2Map(sPartNum,sRevision: String);
    { Public declarations }
  end;

var
  frmMap: TfrmMap;

implementation

{$R *.dfm}

procedure TfrmMap.Save2Map(sPartNum,sRevision: String);
var bmp: TImage;
    Re: TRect;
    sSQL, sResult, sMap, sName: String;
    sProcStr:String;
    sCurrLayer: string;
Begin
   TfrmMap(Self).Color:= clWindow;
   with qryExec do
   begin
     Close;
     SQL.Clear;
     SQL.Add('select Value from CURdSysParams(nolock) where SystemId=''EMO'' and '
            +'ParamId=''MapDataPath''');
     Open;
   end;
   sMap:= trim(qryExec.FieldByName('Value').AsString);
        if not DirectoryExists(sMap) then
        begin
          ForceDirectories(sMap);
        end;
    XFlowDrawBox1.XRate:= 1.0*XFlowDrawBox1.Width/self.Width;
    XFlowDrawBox1.YRate:= 1.0*XFlowDrawBox1.Width/self.Width;
    with qryMapBmp do
    begin
     Close;
     Parameters.ParamByName('PartNum').Value:= sPartNum;
     Parameters.ParamByName('Revision').Value:= sRevision;
     Open;
     first;
     while (not eof) do
     begin
       sResult:= '';
       sSQL:= ' Select MapData from EMOdProdMap(nolock) where PartNum='''
       +sPartNum
       +''' and Revision='''+sRevision+''' and SerialNum='
       +IntToStr(FieldByName('SerialNum').asInteger);

       with qryExec do
       begin
         Close;
         SQL.Clear;
         SQL.Add(sSQL);
         Open;
         sResult:= FieldByName('MapData').AsString;
       end;
       sName:= sMap+sPartNum+'-'+sRevision+'-'
                +IntToStr(FieldByName('SerialNum').asInteger)+'.jpg';
       XFlowDrawBox1.XRate:=0.6;//0.4;
       XFlowDrawBox1.YRate:=0.6;//0.4;
       XFlowDrawBox1.Content := sResult;
       XFlowDrawBox1.Refresh;
       bmp:= TImage.Create(self);
       try
         bmp.Width := XFlowDrawBox1.Width;
         bmp.Height := XFlowDrawBox1.Height;
         Re.Left:=0;
         Re.Top:=0;
         Re.Right:=XFlowDrawBox1.width;
         Re.Bottom:=XFlowDrawBox1.Height;
         bmp.Canvas.CopyRect(Re, XFlowDrawBox1.Canvas, Re);
         bmp.Picture.SaveToFile(sName);
         {qryMapBmp.edit;
         qryMapBmp.fieldbyName('MapPath').AsString:=sName;
         qryMapBmp.post;}
       finally
         bmp.free;
       end;
       Next;
     end;
   end;


     with qryMapBmp2 do
    begin
     Close;
     Parameters.ParamByName('PartNum').Value:= sPartNum;
     Parameters.ParamByName('Revision').Value:= sRevision;
     sProcStr:='EMOdLayerPressMap';
     Open;
     first;
     while (not eof) do
     begin
       sResult:= '';
       sCurrLayer:= FieldByName('LayerId').asstring;
       sSQL:=('exec '+sProcStr+' '''+sPartNum+''','''+sRevision+''','''+sCurrLayer+'''');
       with qryExec do
       begin
         Close;
         SQL.Clear;
         SQL.Add(sSQL);
         Open;
         sResult:= qryExec.FieldByName('MapData_1').asstring
                  +qryExec.FieldByName('MapData_2').asstring
                  +qryExec.FieldByName('MapData_3').asstring;
       end;
       sName:= sMap+sPartNum+'-'+sRevision+trim(sCurrLayer)+'.jpg';
       XFlowDrawBox1.XRate:= 1.8;//0.4;
       XFlowDrawBox1.YRate:= 1.5;//0.4;
       XFlowDrawBox1.Content := sResult;
       XFlowDrawBox1.Refresh;
       bmp:= TImage.Create(self);
       try
         bmp.Width := XFlowDrawBox1.Width;
         bmp.Height := XFlowDrawBox1.Height;
         Re.Left:=0;
         Re.Top:=0;
         Re.Right:=XFlowDrawBox1.width;
         Re.Bottom:=XFlowDrawBox1.Height;
         bmp.Canvas.CopyRect(Re, XFlowDrawBox1.Canvas, Re);
         bmp.Picture.SaveToFile(sName);
         {qryMapBmp.edit;
         qryMapBmp.fieldbyName('MapPath').AsString:=sName;
         qryMapBmp.post;}
       finally
         bmp.free;
       end;
       Next;
     end;
   end;






end;

end.

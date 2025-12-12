using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    public class CURdTableField
    {
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string? DisplayLabel { get; set; }
        public int? DisplaySize { get; set; }
        public string? FieldNote { get; set; }
        public int? SerialNum { get; set; }
        public int? PK { get; set; }
        public int? FK { get; set; }
        public string? DataType { get; set; }
        public int? ReadOnly { get; set; }
        public string? FormatStr { get; set; }
        public string? Items { get; set; }
        public int? ComboStyle { get; set; }
        public int? Visible { get; set; }
        public string? LookupTable { get; set; }
        public string? LookupKeyField { get; set; }
        public string? LookupResultField { get; set; }
        public string? FontName { get; set; }
        public int? FontSize { get; set; }
        public string? FontColor { get; set; }
        public string? LookupCond1Field { get; set; }
        public string? LookupCond2Field { get; set; }
        public string? LookupCond1ResultField { get; set; }
        public string? LookupCond2ResultField { get; set; }
        public string? FontStyle { get; set; }
        public string? OCXDefaultValue { get; set; }
        public string? OCXLKTableName { get; set; }
        public string? OCXLKResultName { get; set; }
        public string? OCXonValidateFd1 { get; set; }
        public string? OCXonValidateFd2 { get; set; }
        public string? OCXonValidateFd3 { get; set; }
        public string? OCXonValidateFd4 { get; set; }
        public string? OCXonValidateFd5 { get; set; }
        public string? OCXonValidateFd6 { get; set; }
        public int? bOCXonValidateType { get; set; }
        public string? OCXonValidateUd { get; set; }
        public string? OCXonValidateMsg { get; set; }
        public string? OCXColShowWere { get; set; }
        public int? bOCXonValidate { get; set; }
        public int? OCXisLkDraw { get; set; }
        public int? ComboTextSize { get; set; }
        public int? IsMoneyField { get; set; }
        public int? bShow4Money { get; set; }
        public int? IsNotesField { get; set; }
        public int? iShowWhere { get; set; }
        public int? iLabTop { get; set; }
        public int? iLabLeft { get; set; }
        public int? iLabHeight { get; set; }
        public int? iLabWidth { get; set; }
        public int? iFieldTop { get; set; }
        public int? iFieldLeft { get; set; }
        public int? iFieldHeight { get; set; }
        public int? iFieldWidth { get; set; }
        public int? iLayRow { get; set; }
        public int? iLayColumn { get; set; }
        public string? EditColor { get; set; }
        public int? IsNeed { get; set; }
        public string? UseId { get; set; }
        public int? IsUpper { get; set; }
        public string? HightLightValue { get; set; }
        public string? ValidateFrom1 { get; set; }
        public string? ValidateTo1 { get; set; }
        public string? ValidateFrom2 { get; set; }
        public string? ValidateTo2 { get; set; }
        public int? bFooter { get; set; }
        public int? IsFactory { get; set; }
        public string? HightLightEqual { get; set; }
        public string? HightLightVS { get; set; }
        public int? iNoCreateLkTbl { get; set; }
        public string? FieldComments { get; set; }
        public string? HightLightRed { get; set; }
        public string? HightLightNavy { get; set; }
        public string? HightLightAft1 { get; set; }

        [NotMapped]
        public int? IsCommonQuery { get; set; }
    }
}

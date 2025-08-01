using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PcbErpApi.Models;

public partial class CURdOCXTableFieldLK
{
    [Key]
    public string TableName { get; set; } = null!;
    [Key]
    public string FieldName { get; set; } = null!;
    [Key]
    public string KeySelfName { get; set; } = null!;
    [Key]
    public string KeyFieldName { get; set; } = null!;

    public string? UseId { get; set; }

}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

public class ChangePasswordModel : PageModel
{
    private readonly PcbErpContext _context;

    public ChangePasswordModel(PcbErpContext context)
    {
        _context = context;
    }

    [BindProperty]
    [Required(ErrorMessage = "請輸入原密碼")]
    public string OldPassword { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "請輸入新密碼")]
    public string NewPassword { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "請輸入新密碼確認")]
    public string ConfirmPassword { get; set; } = "";

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // 從 Request Header 取得 JWT Token,查詢目前登入的使用者
        var jwtIdHeader = Request.Headers["X-JWTID"].FirstOrDefault();
        if (string.IsNullOrEmpty(jwtIdHeader) || !Guid.TryParse(jwtIdHeader, out Guid jwtId))
        {
            ErrorMessage = "無法取得登入資訊,請重新登入。";
            return Page();
        }

        // 從 UserOnline 表查詢目前登入的使用者
        var userOnline = await _context.CURdUserOnline
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.JwtId == jwtId);

        if (userOnline == null)
        {
            ErrorMessage = "登入已過期,請重新登入。";
            return Page();
        }

        var userId = userOnline.UserId;

        try
        {
            // 檢查新密碼和確認密碼是否一致
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "新密碼確認錯誤,請重新作業。";
                return Page();
            }

            // 取得使用者資料
            var user = await _context.CurdUsers
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                ErrorMessage = "找不到使用者資料。";
                return Page();
            }

            // 檢查是否使用 MD5 加密
            var useMd5Password = await GetSystemParam("UseMD5Password");
            var oldPasswordToCheck = useMd5Password == "1"
                ? GetMD5Hash(OldPassword)
                : OldPassword;

            // 驗證原密碼
            if (user.UserPassword != oldPasswordToCheck)
            {
                ErrorMessage = "原密碼錯誤,請確認並重新作業。";
                return Page();
            }

            // 準備新密碼 (根據是否使用 MD5)
            var newPasswordToSave = useMd5Password == "1"
                ? GetMD5Hash(NewPassword)
                : NewPassword;

            // 更新密碼（與 Delphi 一致，直接更新不檢查 SaveUserPassword）
            user.UserPassword = newPasswordToSave;
            user.LastPwChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();

            SuccessMessage = "密碼變更已完成。";

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"密碼變更失敗: {ex.Message}";
            return Page();
        }
    }

    /// <summary>
    /// 取得系統參數
    /// </summary>
    private async Task<string> GetSystemParam(string paramId)
    {
        try
        {
            var param = await _context.CURdSysParams
                .AsNoTracking()
                .Where(p => p.SystemId == "CUR" && p.ParamId == paramId)
                .Select(p => p.Value)
                .FirstOrDefaultAsync();

            return param ?? "0";
        }
        catch
        {
            return "0";
        }
    }

    /// <summary>
    /// 計算 MD5 雜湊值 (轉小寫,與 Delphi 一致)
    /// </summary>
    private static string GetMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input.Trim().ToLower());
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // 轉換為小寫十六進位字串 (與 Delphi 的 MD5DigestToStr 一致)
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}

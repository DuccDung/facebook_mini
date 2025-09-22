using System.Net;

namespace mail_service.Internal;

public class SimpleTemplateRenderer(IConfiguration config) : ITemplateRenderer
{
    public string RenderActiveSuccess(string displayName, string confirmUrl, DateTimeOffset expiresAt)
    {
        throw new NotImplementedException();
    }

    public string RenderSignUpConfirm(string displayName, string confirmUrl, DateTimeOffset expiresAt)
    {
        var expireText = expiresAt.ToLocalTime().ToString("HH:mm, dd/MM/yyyy");
        return $"""
        <div style="font-family:Arial,Helvetica,sans-serif;max-width:560px">
          <h3>Chào {WebUtility.HtmlEncode(displayName)},</h3>
          <p>Nhấn nút để xác nhận email đăng ký tài khoản Social Network.</p>
          <p><a href="{confirmUrl}" style="background:#2563eb;color:#fff;padding:12px 18px;border-radius:8px;text-decoration:none;display:inline-block">Xác nhận email</a></p>
          <p>Nếu không bấm được, copy link:<br><code>{confirmUrl}</code></p>
          <p><i>Liên kết hết hạn lúc {expireText}.</i></p>
        </div>
        """;
    }
}

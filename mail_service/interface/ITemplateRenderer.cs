namespace mail_service.Internal
{
    public interface ITemplateRenderer
    {
        string RenderSignUpConfirm(string displayName, string confirmUrl, DateTimeOffset expiresAt);
        string RenderActiveSuccess(string displayName, string confirmUrl, DateTimeOffset expiresAt);
    }
}

namespace chat_service.Internal
{
    public interface IAuthorization
    {
        Task<bool> IsAdminGroupAsync(int userId, Guid conversationId);
        Task<bool> IsUserBlockedAsync(int userId, int blockedUserId);
        Task<bool> CanSendMessageAsync(int userId, Guid conversationId);
    }
}

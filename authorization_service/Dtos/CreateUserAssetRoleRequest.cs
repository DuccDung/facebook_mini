namespace authorization_service.Dtos
{
    public class CreateUserAssetRoleRequest
    {
        public int UserId { get; set; }
        public int AssetId { get; set; }
        public int RoleId { get; set; }
        public int GrantedBy { get; set; }
    }
}

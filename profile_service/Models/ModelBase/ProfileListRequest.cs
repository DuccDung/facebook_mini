namespace profile_service.Models.ModelBase
{
    public class ProfileListRequest
    {
        public List<int> UserIds { get; set; } = new();
    }

}

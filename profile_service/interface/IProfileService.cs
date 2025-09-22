using Azure;
using profile_service.Model.ModelBase;
using profile_service.Models;

namespace profile_service.Internal
{
    public interface IProfileService
    {
        Task<ResponseModel<string>> CreateProfile(Profile profile_req);
    }
}

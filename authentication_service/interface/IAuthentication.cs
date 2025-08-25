using authentication_service.Model.ModelBase;
using authentication_service.Models;

namespace authentication_service.Internal
{
    public interface IAuthentication
    {
        Task<ResponseModel<Account>> Login(string acc_info, string password);

    }
}

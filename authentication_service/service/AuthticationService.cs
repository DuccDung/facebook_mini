using authentication_service.Internal;
using authentication_service.Model.ModelBase;
using authentication_service.Models;
using Microsoft.EntityFrameworkCore;

namespace authentication_service.service
{
    public class AuthticationService : IAuthentication
    {
        private readonly AuthenticationContext _context;
        public AuthticationService(AuthenticationContext context)
        {
            _context = context;
        }

        public async Task<ResponseModel<Account>> Login(string acc_info, string password)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => (a.AccountName == acc_info || a.Email == acc_info) && a.Password == password);
            if (account == null)
            {
                return new ResponseModel<Account>
                {
                    IsSussess = false,
                    Message = "Invalid account information or password.",
                    Data = null
                };
            }

            return new ResponseModel<Account>
            {
                IsSussess = true,
                Message = "Login successful.",
                Data = account
            };
        }
    }
}

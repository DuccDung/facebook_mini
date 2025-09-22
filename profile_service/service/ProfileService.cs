using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using profile_service.Internal;
using profile_service.Model.ModelBase;
using profile_service.Models;

namespace profile_service.service
{
    public class ProfileService : IProfileService
    {
        private readonly ProfileContext _context;
        public ProfileService(ProfileContext context)
        {
            _context = context;
        }
        public async Task<ResponseModel<string>> CreateProfile(Profile profile_req)
        {
            if (profile_req == null)
            {
                // Use a concrete implementation of Response<string> instead of trying to instantiate the abstract class.
                var response = new ResponseModel<string>()
                {
                    IsSussess = false,
                    Message = "Profile data is required.",
                    Data = null
                };
                return response;
            }
            // Check if profile with the same AccountId already exists
            var existingProfile = await _context.Profiles
                .FirstOrDefaultAsync(x => x.ProfileId == profile_req.ProfileId);
            if (existingProfile != null) return new ResponseModel<string>()
            {
                IsSussess = false,
                Message = "Profile with the same AccountId already exists.",
                Data = null
            };
            // create success
            _context.Profiles.Add(profile_req);
            await _context.SaveChangesAsync();
            return new ResponseModel<string>()
            {
                IsSussess = true,
                Message = "Profile created successfully.",
                Data = profile_req.ProfileId.ToString()
            };
        }
    }
}

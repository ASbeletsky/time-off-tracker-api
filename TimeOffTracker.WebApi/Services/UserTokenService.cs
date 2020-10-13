using Domain.EF_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TimeOffTracker.WebApi.AuthHelpers;
using TimeOffTracker.WebApi.ViewModels;

namespace TimeOffTracker.WebApi.Services
{
    public class UserTokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppSettings _appSettings;

        public UserTokenService(UserManager<User> userManager, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }

        public LoggedInUserModel Authenticate(string username, string password)
        {
            User user = _userManager.FindByNameAsync(username).Result;
            if (user == null || !_userManager.CheckPasswordAsync(user, password).Result)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            string userRole = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, userRole)
                }),
                Expires = DateTime.UtcNow.AddHours(_appSettings.TokenExpiresTimeHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            LoggedInUserModel userWithToken = new LoggedInUserModel()
            {
                UserId = user.Id,
                Role = userRole,
                Token = tokenHandler.WriteToken(token)
            };

            return userWithToken;
        }
    }
}

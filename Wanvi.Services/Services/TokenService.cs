using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Wanvi.Core.Bases.CoreException;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.ModelViews.UserModelViews;
using Wanvi.ModelViews.AuthModelViews;
using System;
using System.Collections.Generic;
using System.IO;
using Wanvi.Core.Constants;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;

namespace Wanvi.Services.Services
{
    public class TokenService: ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public TokenResponse GenerateTokens(ApplicationUser user, string role)
        {
            DateTime now = DateTime.Now;

            // Common claims for both tokens
            List<Claim> claims = new List<Claim>
        {
            new Claim("id", user.Id.ToString()),
            new Claim("role", role),
            // assign 
            new Claim("exp", now.Ticks.ToString())
        };

            var keyString = _configuration.GetSection("JwtSettings:SecretKey").Value ?? string.Empty;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            var claimsIdentity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(new[] { claimsIdentity });
            _httpContextAccessor.HttpContext.User = principal;

            Console.WriteLine("Check Key:", key);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            // Generate access token
            var accessToken = new JwtSecurityToken(
                claims: claims,
                issuer: _configuration.GetSection("JwtSettings:Issuer").Value,
                audience: _configuration.GetSection("JwtSettings:Audience").Value,
                expires: now.AddMinutes(30),
                signingCredentials: creds
            );
            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

            // Generate refresh token
            var refreshToken = new JwtSecurityToken(
                claims: claims,
            issuer: _configuration.GetSection("JwtSettings:Issuer").Value,
            audience: _configuration.GetSection("JwtSettings:Audience").Value,
            expires: now.AddDays(7),
                signingCredentials: creds
            );
            var refreshTokenString = new JwtSecurityTokenHandler().WriteToken(refreshToken);
            ApplicationUserRole roleUser = _unitOfWork.GetRepository<ApplicationUserRole>().Entities.Where(x => x.UserId == user.Id).FirstOrDefault()
                                    ?? throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Lỗi Authorize");
            string roleName = _unitOfWork.GetRepository<ApplicationRole>().GetById(roleUser.RoleId).Name ?? "Unknow";
            // Return the tokens and user information
            return new TokenResponse
            {
                AccessToken = accessTokenString,
                RefreshToken = refreshTokenString,

                User = new ResponseUserModel
                {
                    Id = user.Id.ToString(),
                    Username = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    CreatedTime = user.CreatedTime,
                    Role = roleName
                }
            };
        }
    }
}

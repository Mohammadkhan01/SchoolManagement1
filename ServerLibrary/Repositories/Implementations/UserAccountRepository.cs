using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using Constants = ServerLibrary.Helpers.Constants;

namespace ServerLibrary.Repositories.Implementations
{
    public class UserAccountRepository : IUserAccount
	{
        private ApplicationDbContext _appDbContext;
        private IOptions<JwtSection> _config;
        public UserAccountRepository(IOptions<JwtSection> config, ApplicationDbContext appdbContext)
        {
            _appDbContext = appdbContext;
            _config = config;
        }
     public async Task<GeneralResponse> CreateAsync(Register user)
        {
            if (user is null) return new GeneralResponse(false, "Model is Empty");
            var checkUser = await FindUserByEmail(user.Email);
            if (checkUser != null) return new GeneralResponse(false, "User Registered already");
            //save user
            var applicationUser = await AddToDatabase(new ApplicationUser()
            {
                FullName = user.FullName,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            }) ;
            //check, create and assign role
            var checkAdminRole =  _appDbContext.SystemRoles.FirstOrDefault(x => x.Name!.Equals(Constants.Admin));
            if(checkAdminRole is null)
            {
                var createAdminRole = await AddToDatabase(new SystemRole() { Name = Constants.Admin });
                await AddToDatabase(new UserRole() { RoleId = createAdminRole.Id, UserId = applicationUser.Id });

                return new GeneralResponse(true, "Account Created");

            }

            var checkUserRole = _appDbContext.SystemRoles.FirstOrDefault(x => x.Name!.Equals(Constants.User));
            if (checkUserRole is null)
            {
                var createUserRole = await AddToDatabase(new SystemRole() { Name = Constants.User });
                await AddToDatabase(new UserRole() { RoleId = createUserRole.Id, UserId = applicationUser.Id });
                
            }
            else
            {
                await AddToDatabase(new UserRole() { RoleId = checkUserRole.Id, UserId = applicationUser.Id });
            }
            return new GeneralResponse(true, "Account Created");

        }

        public async Task<LoginResponse> SignInAsync(Login user)
        {
            if (user is null) return new LoginResponse(false, "Model is Empty");
            var checkUser = await FindUserByEmail(user.Email);
            if (checkUser is null) return new LoginResponse(false, "User Not Found");
            //Verify Password
            if (!BCrypt.Net.BCrypt.Verify(user.Password, checkUser.Password))
                return new LoginResponse(false, "Email/Password are valid.");

            var getUserRole = await FindUserRole(checkUser.Id);
            if (getUserRole is null) return new LoginResponse(false, "User Role not found");

            var getRoleName = await FindRoleName(getUserRole.RoleId);
            if (getUserRole is null) return new LoginResponse(false, "UserRole not found");

            string jwtToken = GenerateToken(checkUser, getRoleName!.Name);
            string refreshToken = GenerateRefreshToken();

            var findUser = await _appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(x => x.UserId == checkUser.Id);
            if(findUser is not null)
            {
                findUser!.Token = refreshToken;
                await _appDbContext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new RefreshTokenInfo() { Token = refreshToken, UserId = checkUser.Id });
            }
            return new LoginResponse(true, "Login Successful", jwtToken, refreshToken);
        }

        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        private string GenerateToken(ApplicationUser user, string role)
        {
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.Value.Key!));
            var credentias = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role!)
            };

            var token = new JwtSecurityToken(
                issuer: _config.Value.Issuer,
                audience: _config.Value.Audience,
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentias

                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<UserRole> FindUserRole(int UserId)
        {
            var userRole = await _appDbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == UserId);
            return userRole;
        }

        private async Task<SystemRole> FindRoleName(int roleId)
        {
            var roleName =await _appDbContext.SystemRoles.FirstOrDefaultAsync(x => x.Id == roleId);
            return roleName;
        }


        private async Task<ApplicationUser> FindUserByEmail(string email)
        {
           var email1 =  await _appDbContext.ApplicationUsers.FirstOrDefaultAsync(x => x.Email!.ToLower()!.Equals(email!.ToLower()));
            return email1;
        }
        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = _appDbContext.Add(model);
            await _appDbContext.SaveChangesAsync();
            return (T)result.Entity;

        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
        {
            if (token is null) return new LoginResponse(false, "Model is empty");
            var findToken = await _appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(x => x.Token!.Equals(token.Token));
            if (findToken is null) return new LoginResponse(false, "Refresh token is required");

            var user = await _appDbContext.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == findToken.UserId);
            if (user is null) return new LoginResponse(false, "Refresh token cound not be generated because user not found");

            var userRole = await FindUserRole(user.Id);
            var roleName = await FindRoleName(userRole.RoleId);
            string jwtToken = GenerateToken(user, roleName.Name);
            string refreshToken = GenerateRefreshToken();

            var updateRefreshToken = await _appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (updateRefreshToken is null) return new LoginResponse(false, "Refresh Token could not be generated because user has not signed in");

            updateRefreshToken.Token = refreshToken;
            await _appDbContext.SaveChangesAsync();
            return new LoginResponse(true, "Token Refreshed successfully!", jwtToken, refreshToken);

        }
    }
}


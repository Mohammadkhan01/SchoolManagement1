using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientLibrary.Helpers
{
	public class CustomAuthenticationStateProvider: AuthenticationStateProvider
	{
        private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());
        private LocalStorageService _localStorage;
		public CustomAuthenticationStateProvider(LocalStorageService localStorageService)
		{
            _localStorage = localStorageService;
		}

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var stringToken = await _localStorage.GetToken();
            if (string.IsNullOrEmpty(stringToken)) return await Task.FromResult(new AuthenticationState(anonymous));

            var deserilizeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
            if (deserilizeToken == null) return await Task.FromResult(new AuthenticationState(anonymous));

            var getUserClaims = DecryptToken(deserilizeToken.Token!);
            if (getUserClaims == null) return await Task.FromResult(new AuthenticationState(anonymous));

            var claimsPrincipal = SetClaimPrincipal(getUserClaims);
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));

        }

        public static CustomUserClaims DecryptToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken)) return new CustomUserClaims();

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            var userId = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var name = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            var email = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            var role = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            return new CustomUserClaims(userId!.Value!, name!.Value, email!.Value, role!.Value);


        }

        public async Task UpdateAuthenticationState(UserSession userSession)
        {
            var claimsPrincipal = new ClaimsPrincipal();
            if (userSession.Token != null || userSession.RefreshToken != null)
            {
                var serializeSession = Serializations.SerializeObj(userSession);
                await _localStorage.SetToken(serializeSession);
                var getUserClaims = DecryptToken(userSession.Token);
                claimsPrincipal = SetClaimPrincipal(getUserClaims);
            }
        }

        public static ClaimsPrincipal SetClaimPrincipal(CustomUserClaims claims)
        {
            if (claims._Email is null) return new ClaimsPrincipal();
            return new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim>
                {
                    new (ClaimTypes.NameIdentifier, claims._Id!),
                    new(ClaimTypes.Name, claims._Name),
                    new (ClaimTypes.Email, claims._Email),
                    new (ClaimTypes.Role, claims._Role),
                }, "JwtAuth"));
        }
    }
}


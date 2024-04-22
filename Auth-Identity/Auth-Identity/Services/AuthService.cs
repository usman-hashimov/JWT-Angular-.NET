﻿using Auth_Identity.DTOs;
using Auth_Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth_Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<AuthDTO> GenerateToken(User user)
        {
            if (user is null)
            {
                return new AuthDTO()
                {
                    Message = "User is null",
                    StatusCode = 404
                };
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_configuration["JWTSettings:secretKey"]!);

            var roles = await _userManager.GetRolesAsync(user);

            List<Claim> claims =
                [
                    new(JwtRegisteredClaimNames.Email, user.Email!),
                    new(JwtRegisteredClaimNames.Name, user.FullName!),
                    new(JwtRegisteredClaimNames.NameId, user.Id),
                    new(JwtRegisteredClaimNames.Aud, _configuration["JWTSettings:ValidAudience"]!),
                    new(JwtRegisteredClaimNames.Iss, _configuration["JWTSettings:ValidIssuer"]!),
                    new(JwtRegisteredClaimNames.Exp, _configuration["JWTSettings:ExpireDate"]!),
                ];

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Double.Parse(_configuration["JWTSettings:ExpireDate"]!)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthDTO()
            {
                Token = tokenHandler.WriteToken(token),
                Message = "Token successfully created",
                StatusCode = 200,
                isSuccess = true
            };
        }
    }
}

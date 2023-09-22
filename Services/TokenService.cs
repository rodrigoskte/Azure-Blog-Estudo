using Blog.Extensions;
using Blog.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Blog.Services
{
    public class TokenService
    {
        public string GenerateToken(User user)
        {
            // Criou a instancia do token handler, item utiliado para de fato gerar o token
            var tokenHandler = new JwtSecurityTokenHandler();
            // Pegamos a chave e a convertemos em bytes
            var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);

            // Criar e usar um extensions de claims
            var claims = user.GetClaims();

            // Especificação do token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                                     new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            // Criou o token baseado nas descrições
            var token = tokenHandler.CreateToken(tokenDescriptor);
            // retornando o token escrito
            return tokenHandler.WriteToken(token);
        }
    }
}

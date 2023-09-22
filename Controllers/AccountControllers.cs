using Blog.Attributes;
using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Runtime.Intrinsics;
using System.Text.RegularExpressions;

namespace Blog.Controllers
{
    [ApiController]
    public class AccountControllers : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly BlogDataContext _blogDataContext;
        private readonly EmailService _emailService;

        public AccountControllers(TokenService tokenService, BlogDataContext blogDataContext, EmailService emailService)
        {
            _tokenService = tokenService;
            _blogDataContext = blogDataContext;
            _emailService = emailService;
        }

        [HttpPost("v1/accounts/")]
        public async Task<IActionResult> Post([FromBody]RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-")
            };

            var password = PasswordGenerator.Generate(25);
            user.PasswordHash = PasswordHasher.Hash(password);

            try
            {
                await _blogDataContext.Users.AddAsync(user);
                await _blogDataContext.SaveChangesAsync();

                _emailService.Send(
                    user.Name, 
                    user.Email,
                    "Bem vindo ao blog do Rodão", 
                    $"Sua senha é <strong>{password}</strong>");

                return Ok(new ResultViewModel<dynamic>(new 
                { 
                    user = user.Email, 
                    password 
                }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("Este e-mail já está cadastrado!"));
            }
            catch (Exception ex) 
            {
                return StatusCode(400, new ResultViewModel<string>("Falha interna do servidor!"));
            }
        }

        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = await _blogDataContext
                            .Users
                            .AsNoTracking()
                            .Include(x => x.Roles)
                            .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            try
            {
                var token = _tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }
        }

        [HttpGet("v1/accounts/usuarios")]
        [ApiKey]
        public async Task<IActionResult> Usuarios()
        {
            try
            {
                var users = await _blogDataContext.Users.AsNoTracking().ToListAsync();
                return Ok(new ResultViewModel<List<User>>(users));
            }
            catch 
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna do servidor"));
            }
        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage([FromBody] UploadImageViewModel model)
        {
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);

            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }

            var user = await _blogDataContext
            .Users
            .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if (user == null)
                return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));

            user.Image = $"https://localhost:0000/images/{fileName}";
            try
            {
                _blogDataContext.Users.Update(user);
                await _blogDataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));
        }
    }
}

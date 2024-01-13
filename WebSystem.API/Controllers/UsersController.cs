using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using WebSystem.API.Data;
using WebSystem.API.Helpers;
using WebSystem.Shared.DTOs;
using WebSystem.Shared.Entities;

namespace WebSystem.API.Controllers
{
    [ApiController]
    [Route("/api/users")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMailHelper _mailHelper;

        public UsersController(DataContext context, IConfiguration configuration, IMailHelper mailHelper)
        {
            _context = context;
            _configuration = configuration;
            _mailHelper = mailHelper;
        }

        //------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        //------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult> Post(UserDTO userDTO)

        {
            User user = new();

            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.Email = userDTO.Email;
            user.IsAdmin = userDTO.IsAdmin;
            user.IsConfirm = false;
            user.Token = CreateToken();
            user.Password = HashPasword(userDTO.Password2, out byte[] salt);



            _context.Add(user);
            try
            {
                var result = await _context.SaveChangesAsync();
                
                var myToken = user.Token;
                var tokenLink = "https://" + _configuration["UrlWEB"] + "/users/ConfirmEmail/?token=" + myToken;

                string subject = "WebSystem - Confirmación de cuenta";
                string body = $"<h1>WebSystem - Confirmación de cuenta</h1>" +
                        $"<p>Para habilitar el usuario, por favor hacer clic 'Confirmar Email':</p>" +
                        $"<b><a href ={tokenLink}>Confirmar Email</a></b>";

                var response = _mailHelper.SendMail(user.FullName, user.Email!,subject,body);


                //" . $_ENV['APP_URL'] . " / confirmar ? token = " . $this->token . "'




                    return Ok(user);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe un Usuario con el mismo Email.");
                }
                else
                {
                    return BadRequest(dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        //------------------------------------------------------------------------------------
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        //------------------------------------------------------------------------------------
        [HttpPut]
        public async Task<ActionResult> Put(User user)
        {
            _context.Update(user);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(user);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe un registro con el mismo nombre.");
                }
                else
                {
                    return BadRequest(dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        //------------------------------------------------------------------------------------
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        
        [HttpGet("ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmailAsync(string token)
        {
            User? user = _context.Users.FirstOrDefault(x => x.Token == token);

            if (user == null)
            {
                return BadRequest("El Usuario no existe");
            }

            user.IsConfirm = true;
            user.Token = "";

            _context.Update(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        //------------------------------------------------------------------------------------
        private string CreateToken()
        {
            Random rdn = new Random();
            string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890$#@";
            int longitud = caracteres.Length;
            char letra;
            int longitudContrasenia = 15;
            string contraseniaAleatoria = string.Empty;
            for (int i = 0; i < longitudContrasenia; i++)
            {
                letra = caracteres[rdn.Next(longitud)];
                contraseniaAleatoria += letra.ToString();
            }
            return contraseniaAleatoria;
        }

        //------------------------------------------------------------------------------------
        string HashPasword(string password, out byte[] salt)
        {
            const int keySize = 64;
            const int iterations = 35;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
            salt = RandomNumberGenerator.GetBytes(keySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
            iterations,
                hashAlgorithm,
                keySize);
            return Convert.ToHexString(hash);
        }

        //------------------------------------------------------------------------------------
        bool VerifyPassword(string password, string hash, byte[] salt)
        {
            const int keySize = 64;
            const int iterations = 35;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);
            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
        }
    }
}

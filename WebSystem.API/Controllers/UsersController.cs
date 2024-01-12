using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using WebSystem.API.Data;
using WebSystem.API.Helpers;
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
        public async Task<ActionResult> Post(User user)
        {
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
        
        //------------------------------------------------------------------------------------
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
    }
}

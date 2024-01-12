using WebSystem.Shared.Entities;

namespace WebSystem.API.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;

        public SeedDb(DataContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckUsersAsync();
        }

        private async Task CheckUsersAsync()
        {
            if (!_context.Users.Any())
            {
                _context.Users.Add(new User { FirstName = "Luis" ,LastName="Núñez", Email="luis@yopmail.com",Password="123456",IsAdmin=true,IsConfirm=true, Token=""});
                _context.Users.Add(new User { FirstName = "Lionel", LastName = "Messi", Email = "messi@yopmail.com", Password = "123456", IsAdmin = false, IsConfirm = true, Token = "" });
            }
            await _context.SaveChangesAsync();
        }
    }
}

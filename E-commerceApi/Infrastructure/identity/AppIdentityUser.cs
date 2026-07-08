using Microsoft.AspNetCore.Identity;

namespace E_commerceApi.Infrastructure.identity
{
    public class ApplicationUsers : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

    }
}
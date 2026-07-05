using Microsoft.AspNetCore.Identity;

namespace E_commerceApi.Domain.Entities
{
    public class ApplicationUsers : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
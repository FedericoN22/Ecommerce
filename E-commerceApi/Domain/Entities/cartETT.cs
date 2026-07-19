using E_commerceApi.Domain.Entities.cartItem;
using E_commerceApi.Domain.Entities.intefaces;

namespace E_commerceApi.Domain.Entities.cart
{
    public class cartETT : IAuditableEntity
    {
        public int Id { get; set; }
        public string? UserId { get; set; }

        public DateTime CreatedDateAud { get; set; }

        public DateTime? ModifiedDateAud { get; set; }

        public List<cartItemETT>? Items { get; set; }

    }
}
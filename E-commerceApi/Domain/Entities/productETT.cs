using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.intefaces;

namespace E_commerceApi.Domain.Entities.product
{
    public class productETT : IAuditableEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public categoryETT? Category { get; set; }
        public int CategoryId { get; set; }

        public DateTime CreatedDateAud { get; set; }
        public DateTime? ModifiedDateAud { get; set; }
        public productETT? Product { get; set; }



    }
}

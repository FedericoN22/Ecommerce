using E_commerceApi.Domain.Entities.product;
using E_commerceApi.Domain.Entities.intefaces;
namespace E_commerceApi.Domain.Entities.category
{
    public class categoryETT : IAuditableEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public List<productETT>? products { get; set; }

        public DateTime CreatedDateAud { get; set; }
        public DateTime? ModifiedDateAud { get; set; }

    }
}
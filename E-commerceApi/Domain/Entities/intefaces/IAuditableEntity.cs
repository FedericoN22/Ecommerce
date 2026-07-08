namespace E_commerceApi.Domain.Entities.intefaces
{

    public interface IAuditableEntity
    {
        DateTime CreatedDateAud { get; set; }
        DateTime? ModifiedDateAud { get; set; }
    }

}
using FluentValidation;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCarttItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1");
    }
}
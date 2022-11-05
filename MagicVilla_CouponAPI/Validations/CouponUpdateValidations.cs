namespace MagicVilla_CouponAPI.Validations;

public class CouponUpdateValidations : AbstractValidator<CouponUpdateRequest>
{
    public CouponUpdateValidations()
    {
        RuleFor(r => r.Id).NotEmpty().GreaterThan(0);
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Percent).InclusiveBetween(0, 100);
    }
}
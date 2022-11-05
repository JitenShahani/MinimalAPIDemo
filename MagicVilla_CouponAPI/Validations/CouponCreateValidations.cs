namespace MagicVilla_CouponAPI.Validations;

public class CouponCreateValidations : AbstractValidator<CouponCreateRequest>
{
    public CouponCreateValidations()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Percent).InclusiveBetween(0, 100);
    }
}
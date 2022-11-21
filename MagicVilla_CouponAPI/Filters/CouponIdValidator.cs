namespace MagicVillaCouponAPI.Filters;

public class CouponIdValidator : IEndpointFilter
{
    private readonly ILogger<CouponIdValidator> _logger;

    public CouponIdValidator (ILogger<CouponIdValidator> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync (EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var id = context.Arguments.SingleOrDefault (f => f.GetType () == typeof (int)) as int?;

        if (id is null || id <= 0)
        {
            APIResponse response = new ()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest
            };

            response.ErrorMessages.Add ("Coupon Id cannot be less than or equal to 0 or null");

            _logger.Log (LogLevel.Information, "Coupon Id cannot be less than or equal to 0 or null");

            return Results.BadRequest (response);
        }

        return await next (context);
    }
}
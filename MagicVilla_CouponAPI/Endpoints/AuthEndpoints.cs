namespace MagicVilla_CouponAPI.Endpoints;

/// <summary>
/// Auth Endpoints Class
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// My endpoint methods for Auth
    /// </summary>
    /// <param name="app">WebApplication</param>
    public static void ConfigureAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/login", Login)
           .WithName("Login")
           .Produces<APIResponse>(200)
           .Produces(400)
           .Accepts<LoginRequest>("application/json");

        app.MapPost("/api/Register", Register)
           .WithName("Register")
           .Produces<APIResponse>(201)
           .Produces(400)
           .Accepts<RegistrationRequest>("application/json");
    }

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="_autoRepo"></param>
    /// <param name="model"></param>
    /// <returns>JWT Token</returns>
    private static async Task<IResult> Login(IAuthRepository _autoRepo, [FromBody] LoginRequest model)
    {
        APIResponse response = new()
        {
            IsSuccess = false,
            StatusCode = HttpStatusCode.BadRequest
        };

        var loginResponse = await _autoRepo.Login(model);

        if (loginResponse is null)
        {
            response.ErrorMessages.Add("UserName or Password is incorrect.");
            return Results.BadRequest(response);
        }

        response.IsSuccess = true;
        response.Result = loginResponse;
        response.StatusCode = HttpStatusCode.OK;

        return Results.Ok(response);
    }

    /// <summary>
    /// Register a New User
    /// </summary>
    /// <param name="_autoRepo"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    private static async Task<IResult> Register(IAuthRepository _autoRepo, [FromBody] RegistrationRequest model)
    {
        APIResponse response = new()
        {
            IsSuccess = false,
            StatusCode = HttpStatusCode.BadRequest
        };

        bool ifUserNameIsUnique = _autoRepo.IsUniqueUser(model.UserName);

        if (!ifUserNameIsUnique)
        {
            response.ErrorMessages.Add("Username already exists.");
            return Results.BadRequest(response);
        }

        var registerResponse = await _autoRepo.Register(model);

        if (registerResponse is null || string.IsNullOrEmpty(registerResponse.UserName))
        {
            return Results.BadRequest(response);
        }

        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;

        return Results.Ok(response);
    }
}
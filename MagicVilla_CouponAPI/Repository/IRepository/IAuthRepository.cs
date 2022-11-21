namespace MagicVilla_CouponAPI.Repository.IRepository
{
    public interface IAuthRepository
    {
        bool IsUniqueUser(string username);

        Task<LoginResponse> Login(LoginRequest loginRequest);

        Task<User> Register(RegistrationRequest registrationRequest);
    }
}
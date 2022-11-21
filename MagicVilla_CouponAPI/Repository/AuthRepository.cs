using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_CouponAPI.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private string secretKey;

        public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            secretKey = _configuration.GetValue<string>("ApiSettings:Secret");
        }

        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var user = _db.LocalUsers.SingleOrDefault(user => user.UserName == loginRequest.UserName && user.Password == loginRequest.Password);

            if (user is null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponse loginResponse = new()
            {
                User = _mapper.Map<User>(user),
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return loginResponse;
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.LocalUsers!.FirstOrDefault(user => user.UserName == username);

            if (user is null)
                return true;

            return false;
        }

        public async Task<User> Register(RegistrationRequest registrationRequest)
        {
            LocalUser userObj = new()
            {
                UserName = registrationRequest.UserName,
                Password = registrationRequest.Password,
                Name = registrationRequest.Name,
                Role = registrationRequest.Role
            };

            _db.LocalUsers!.Add(userObj);
            _db.SaveChanges();

            userObj.Password = "";

            return _mapper!.Map<User>(userObj);
        }
    }
}
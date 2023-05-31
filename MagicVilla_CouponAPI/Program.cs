var builder = WebApplication.CreateBuilder (args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ();
builder.Services.AddSwaggerGen (options =>
{
	options.SwaggerDoc ("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "Magic Villa Coupon API",
		Description = "Keep track of Coupons",
		Contact = new OpenApiContact
		{
			Name = "Jiten Shahani",
			Email = "shahani.jiten@gmail.com"
		}
	});

	options.AddSecurityDefinition ("Bearer", new OpenApiSecurityScheme
	{
		Description =
			"JWT Authorization header using the bearer scheme. \r\n\r\n " +
			"Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
			"Example: \"Bearer 12345abcdef\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	options.AddSecurityRequirement (new OpenApiSecurityRequirement ()
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				},
				Scheme = "oauth2",
				Name = "Bearer",
				In = ParameterLocation.Header
			},
			new List<string>()
		}
	});

	var docFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, docFile));
});
builder.Services.AddScoped<ICouponRepository, CouponRepository> ();
builder.Services.AddScoped<IAuthRepository, AuthRepository> ();
builder.Services.AddDbContext<ApplicationDbContext> (options => options.UseSqlServer (builder.Configuration.GetConnectionString ("DefaultConnection")));
builder.Services.AddAutoMapper (typeof (MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program> ();
builder.Services.AddAuthentication (auth =>
{
	auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer (options =>
{
	options.RequireHttpsMetadata = false;
	options.SaveToken = true;
	options.TokenValidationParameters = new ()
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey (Encoding.ASCII.GetBytes (builder.Configuration.GetValue<string> ("ApiSettings:Secret"))),
		ValidateIssuer = false,
		ValidateAudience = false
	};
});
builder.Services.AddAuthorization (options =>
{
	options.AddPolicy ("AdminOnly", policy => policy.RequireRole ("admin"));
});

var app = builder.Build ();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment ())
{
	app.UseSwagger ();
	app.UseSwaggerUI (options =>
	{
		options.DocumentTitle = "Magic Villa Coupon API";
		options.SwaggerEndpoint ("/swagger/v1/swagger.json", "Coupon API");
	});
}

app.UseAuthentication ();
app.UseAuthorization ();

app.ConfigureAuthEndpoints ();
app.ConfigureCouponEndpoints ();

//app.MapGet("/api/coupon/special", (string couponName, int PageSize, int Page, ILogger<Program> _logger, ApplicationDbContext _db) =>
//{
//    if (couponName is not null)
//        return _db.Coupons!.Where(c => c.Name.Contains(couponName)).Skip((Page - 1) * PageSize).Take(PageSize);

//    return _db.Coupons!.Skip((Page - 1) * PageSize).Take(PageSize);
//});

app.MapGet ("/api/coupon/special", ([AsParameters] CouponRequestSpecial request, ApplicationDbContext _db) =>
{
	if (request.CouponName is not null)
		return _db.Coupons!.Where (c => c.Name.Contains (request.CouponName)).Skip ((request.Page - 1) * request.PageSize).Take (request.PageSize);

	return _db.Coupons!.Skip ((request.Page - 1) * request.PageSize).Take (request.PageSize);
});

app.UseHttpsRedirection ();

app.Run ();

internal class CouponRequestSpecial
{
	public string? CouponName { get; set; }

	[FromHeader (Name = "PageSize")]
	public int PageSize { get; set; }

	[FromHeader (Name = "Page")]
	public int Page { get; set; }

	public ILogger<CouponRequestSpecial> Logger { get; set; }
}
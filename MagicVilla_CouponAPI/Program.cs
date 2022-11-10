var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
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
});
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Magic Villa Coupon API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Coupon API");
    });
}

app.MapGet("/api/coupon", [EndpointDescription("Get all Coupons...")] (ILogger<Coupon> _logger, ApplicationDbContext _db) =>
{
    _logger.Log(LogLevel.Information, "Get all coupons.");

    APIResponse response = new()
    {
        IsSuccess = true,
        Result = _db.Coupons,
        StatusCode = HttpStatusCode.OK
    };

    return Results.Ok(response);
})
.WithName("GetCoupons")
.Produces<APIResponse>(200)
.WithDescription("Get all Coupons...");

app.MapGet("/api/coupon/{id:int}", [EndpointDescription("Get Coupon by Id...")] async (ILogger<Coupon> _logger, ApplicationDbContext _db, int id) =>
{
    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest
    };

    _logger.Log(LogLevel.Information, "Get coupon.");

    if (await _db.Coupons!.FirstOrDefaultAsync(c => c.Id == id) is null)
        response.ErrorMessages.Add("Invalid Id");
    else
    {
        response.IsSuccess = true;
        response.Result = await _db.Coupons!.FirstOrDefaultAsync(c => c.Id == id);
        response.StatusCode = HttpStatusCode.OK;
    }

    return Results.Ok(response);
})
.WithName("GetCoupon")
.Produces<APIResponse>(200)
.Produces(400)
.WithDescription("Get Coupon by Id...");

app.MapPost("/api/coupon", async (ILogger<Coupon> _logger, IValidator<CouponCreateRequest> _validator, IMapper _mapper,
    ApplicationDbContext _db, [FromBody] CouponCreateRequest couponRequest, HttpRequest request) =>
{
    _logger.Log(LogLevel.Information, "Create coupon.");

    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest
    };

    var validationResult = await _validator.ValidateAsync(couponRequest);

    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()!.ErrorMessage.ToString());
        return Results.BadRequest(response);
    }

    if (await _db.Coupons!.FirstOrDefaultAsync(c => c.Name!.ToLower() == couponRequest.Name!.ToLower()) is not null)
    {
        response.ErrorMessages.Add("Coupon Name already Exists");
        return Results.BadRequest(response);
    }

    // Mapping Coupon to CouponCreateRequest
    Coupon coupon = _mapper.Map<Coupon>(couponRequest);

    // Database will detect Id and auto increment it
    //coupon.Id = _db.Coupons!.Max(c => c.Id) + 1;
    coupon.Created = DateTime.Now;

    _db.Coupons!.Add(coupon);
    await _db.SaveChangesAsync();

    // Mapping CouponCreateResponse to Coupon
    CouponCreateResponse couponResponse = _mapper.Map<CouponCreateResponse>(coupon);

    response.IsSuccess = true;
    response.Result = couponResponse;
    response.StatusCode = HttpStatusCode.Created;

    return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, response);
})
.WithName("CreateCoupon")
.Produces<APIResponse>(201)
.Produces(400)
.Accepts<CouponCreateRequest>("application/json");

app.MapPut("/api/coupon", async (ILogger<Coupon> _logger, IValidator<CouponUpdateRequest> _validator, IMapper _mapper,
    ApplicationDbContext _db, [FromBody] CouponUpdateRequest couponRequest) =>
{
    _logger.Log(LogLevel.Information, "Update Coupon.");

    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest
    };

    var validationResult = await _validator.ValidateAsync(couponRequest);

    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()!.ErrorMessage.ToString());
        return Results.BadRequest(response);
    }

    var couponFromStore = await _db.Coupons!.FirstOrDefaultAsync(c => c.Id == couponRequest.Id);
    couponFromStore!.Name = couponRequest.Name;
    couponFromStore.Percent = couponRequest.Percent;
    couponFromStore.IsActive = couponRequest.IsActive;
    couponFromStore.LastUpdated = DateTime.Now;

    //_db.Coupons!.Update(_mapper.Map<Coupon>(couponFromStore));
    await _db.SaveChangesAsync();

    response.IsSuccess = true;
    response.Result = _mapper.Map<Coupon>(couponFromStore);
    response.StatusCode = HttpStatusCode.OK;

    return Results.Ok(response);
})
.WithName("UpdateCoupon")
.Produces<APIResponse>(200)
.Produces(400)
.Accepts<CouponUpdateRequest>("application/json");

app.MapDelete("/api/coupon/{id:int}", async (ILogger<Coupon> _logger, ApplicationDbContext _db, int id) =>
{
    _logger.Log(LogLevel.Information, "Delete Coupon");

    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest
    };

    var couponFromStore = await _db.Coupons!.FirstOrDefaultAsync(c => c.Id == id);

    if (couponFromStore is not null)
    {
        _db.Coupons!.Remove(couponFromStore);
        await _db.SaveChangesAsync();

        response.Result = couponFromStore;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.NoContent;

        return Results.Ok(response);
    }
    else
    {
        response.StatusCode = HttpStatusCode.NotFound;
        response.ErrorMessages.Add("Invalid Coupon Id");
        return Results.NotFound(response);
    }
})
.WithName("DeleteCoupon")
.Produces<APIResponse>(204)
.Produces(404);

app.UseHttpsRedirection();

app.Run();
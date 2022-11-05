var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", (ILogger<Coupon> _logger) =>
{
    _logger.Log(LogLevel.Information, "Get all coupons.");

    APIResponse response = new()
    {
        IsSuccess = true,
        Result = CouponStore.CouponList,
        StatusCode = HttpStatusCode.OK
    };

    return Results.Ok(response);
})
.WithName("GetCoupons")
.Produces<APIResponse>(200);

app.MapGet("/api/coupon/{id:int}", (ILogger<Coupon> _logger, int id) =>
{
    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest
    };

    _logger.Log(LogLevel.Information, "Get coupon.");

    if (CouponStore.CouponList.FirstOrDefault(c => c.Id == id) is null)
        response.ErrorMessages.Add("Invalid Id");
    else
    {
        response.IsSuccess = true;
        response.Result = CouponStore.CouponList.FirstOrDefault(c => c.Id == id);
        response.StatusCode = HttpStatusCode.OK;
    }

    return Results.Ok(response);
})
.WithName("GetCoupon")
.Produces<APIResponse>(200)
.Produces(400);

app.MapPost("/api/coupon", async (ILogger<Coupon> _logger, IValidator<CouponCreateRequest> _validator, IMapper _mapper, [FromBody] CouponCreateRequest couponRequest) =>
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

    if (CouponStore.CouponList.FirstOrDefault(c => c.Name!.ToLower() == couponRequest.Name!.ToLower()) is not null)
    {
        response.ErrorMessages.Add("Coupon Name already Exists");
        return Results.BadRequest(response);
    }

    // Mapping Coupon to CouponCreateRequest
    Coupon coupon = _mapper.Map<Coupon>(couponRequest);

    coupon.Id = CouponStore.CouponList.Max(c => c.Id) + 1;
    coupon.Created = DateTime.Now;

    CouponStore.CouponList.Add(coupon);

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

app.MapPut("/api/coupon", async (ILogger<Coupon> _logger, IValidator<CouponUpdateRequest> _validator, IMapper _mapper, [FromBody] CouponUpdateRequest couponRequest) =>
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

    var couponFromStore = CouponStore.CouponList.FirstOrDefault(c => c.Id == couponRequest.Id);
    couponFromStore!.Name = couponRequest.Name;
    couponFromStore.Percent = couponRequest.Percent;
    couponFromStore.IsActive = couponRequest.IsActive;
    couponFromStore.LastUpdated = DateTime.Now;

    response.IsSuccess = true;
    response.Result = _mapper.Map<CouponUpdateRequest>(couponFromStore);
    response.StatusCode = HttpStatusCode.OK;

    return Results.Ok(response);
})
.WithName("UpdateCoupon")
.Produces<APIResponse>(200)
.Produces(400)
.Accepts<CouponUpdateRequest>("application/json");

app.MapDelete("/api/coupon/{id:int}", (ILogger<Coupon> _logger, IValidator<CouponUpdateRequest> _validator, int id) =>
{
    APIResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest
    };

    var couponFromStore = CouponStore.CouponList.FirstOrDefault(c => c.Id == id);

    if (couponFromStore is not null)
    {
        CouponStore.CouponList.Remove(couponFromStore);

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
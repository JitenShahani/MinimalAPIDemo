using MagicVillaCouponAPI.Filters;
using System.Diagnostics;

namespace MagicVilla_CouponAPI.Endpoints;

public static class CouponEndpoints
{
	public static void ConfigureCouponEndpoints (this WebApplication app)
	{
		app.MapGet ("/api/coupon", GetAllCoupon)
			.WithName ("GetCoupons")
			.Produces<APIResponse> (200)
			.WithDescription ("Get all Coupons...")
			.RequireAuthorization ("AdminOnly");

		app.MapGet ("/api/coupon/{id:int}", GetCoupon)
			.WithName ("GetCoupon")
			.Produces<APIResponse> (200)
			.Produces (400)
			.WithDescription ("Get Coupon by Id...")
			.RequireAuthorization ()
			.AddEndpointFilter<CouponIdValidator> ();

		app.MapPost ("/api/coupon", CreateCoupon)
			.WithName ("CreateCoupon")
			.Produces<APIResponse> (201)
			.Produces (400)
			.Accepts<CouponCreateRequest> ("application/json")
			.RequireAuthorization ();

		app.MapPut ("/api/coupon", UpdateCoupon)
			.WithName ("UpdateCoupon")
			.Produces<APIResponse> (200)
			.Produces (400)
			.Accepts<CouponUpdateRequest> ("application/json")
			.RequireAuthorization ();

		app.MapDelete ("/api/coupon/{id:int}", DeleteCoupon)
			.WithName ("DeleteCoupon")
			.Produces<APIResponse> (204)
			.Produces (404)
			.RequireAuthorization ()
			.AddEndpointFilter<CouponIdValidator> ();
	}

    /// <summary>
    /// Provides a list of all the Coupons...
    /// </summary>
    /// <returns>A collection of all the Coupons...</returns>
    private static async Task<IResult> GetAllCoupon (ILogger<Coupon> _logger, ICouponRepository _couponRepo)
	{
		_logger.Log (LogLevel.Information, "Get all coupons.");

		APIResponse response = new ()
		{
			IsSuccess = true,
			Result = await _couponRepo.GetAllAsync (),
			StatusCode = HttpStatusCode.OK
		};

		return Results.Ok (response);
	}

    /// <summary>
    /// Provides a single Coupon based on Id...
    /// </summary>
    /// <param name="id">The Id of the Coupon : </param>
    /// <returns>The HTTP Status Code</returns>
    private static async Task<IResult> GetCoupon (ILogger<Coupon> _logger, ICouponRepository _couponRepo, int id)
	{
		// Below line of code prints dependencies based on services.
		Debug.WriteLine ("Coupon Repo : " + _couponRepo.GetType ().Name);

		Console.WriteLine ("Endpoint Executed");

		APIResponse response = new ()
		{
			IsSuccess = false,
			StatusCode = HttpStatusCode.BadRequest
		};

		_logger.Log (LogLevel.Information, "Get coupon.");

		if (await _couponRepo.GetAsync (id) is null)
			response.ErrorMessages.Add ("Invalid Id");
		else
		{
			response.IsSuccess = true;
			response.Result = await _couponRepo.GetAsync (id);
			response.StatusCode = HttpStatusCode.OK;
		}

		return Results.Ok (response);
	}

	/// <summary>
	/// Create new Coupon
	/// </summary>
	/// <param name="couponRequest">Fill in the details to create a new Coupon</param>
	/// <returns>Newly created Coupon</returns>
	private static async Task<IResult> CreateCoupon (ILogger<Coupon> _logger, IValidator<CouponCreateRequest> _validator, IMapper _mapper,
			ICouponRepository _couponRepo, [FromBody] CouponCreateRequest couponRequest, HttpRequest request)
	{
		_logger.Log (LogLevel.Information, "Create coupon.");

		APIResponse response = new ()
		{
			IsSuccess = false,
			StatusCode = HttpStatusCode.BadRequest
		};

		var validationResult = await _validator.ValidateAsync (couponRequest);

		if (!validationResult.IsValid)
		{
			response.ErrorMessages.Add (validationResult.Errors.FirstOrDefault ()!.ErrorMessage.ToString ());
			return Results.BadRequest (response);
		}

		if (await _couponRepo.GetAsync (couponRequest.Name!) is not null)
		{
			response.ErrorMessages.Add ("Coupon Name already Exists");
			return Results.BadRequest (response);
		}

		// Mapping Coupon to CouponCreateRequest
		Coupon coupon = _mapper.Map<Coupon> (couponRequest);

		// Database will detect Id and auto increment it
		//coupon.Id = _db.Coupons!.Max(c => c.Id) + 1;
		coupon.Created = DateTime.Now;

		await _couponRepo.UpdateAsync (coupon);
		await _couponRepo.SaveAsync ();

		// Mapping CouponCreateResponse to Coupon
		CouponCreateResponse couponResponse = _mapper.Map<CouponCreateResponse> (coupon);

		response.IsSuccess = true;
		response.Result = couponResponse;
		response.StatusCode = HttpStatusCode.Created;

		return Results.CreatedAtRoute ("GetCoupon", new { id = coupon.Id }, response);
	}

	private static async Task<IResult> UpdateCoupon (ILogger<Coupon> _logger, IValidator<CouponUpdateRequest> _validator, IMapper _mapper,
			ICouponRepository _couponRepo, [FromBody] CouponUpdateRequest couponRequest)
	{
		_logger.Log (LogLevel.Information, "Update Coupon.");

		APIResponse response = new ()
		{
			IsSuccess = false,
			StatusCode = HttpStatusCode.BadRequest
		};

		var validationResult = await _validator.ValidateAsync (couponRequest);

		if (!validationResult.IsValid)
		{
			response.ErrorMessages.Add (validationResult.Errors.FirstOrDefault ()!.ErrorMessage.ToString ());
			return Results.BadRequest (response);
		}

		await _couponRepo.UpdateAsync (_mapper.Map<Coupon> (couponRequest));
		await _couponRepo.SaveAsync ();

		response.IsSuccess = true;
		response.Result = _mapper.Map<Coupon> (await _couponRepo.GetAsync (couponRequest.Id));
		response.StatusCode = HttpStatusCode.OK;

		return Results.Ok (response);
	}

	private static async Task<IResult> DeleteCoupon (ILogger<Coupon> _logger, ICouponRepository _couponRepo, int id)
	{
		_logger.Log (LogLevel.Information, "Delete Coupon");

		APIResponse response = new ()
		{
			IsSuccess = false,
			StatusCode = HttpStatusCode.BadRequest
		};

		var couponFromStore = await _couponRepo.GetAsync (id);

		if (couponFromStore is not null)
		{
			await _couponRepo.RemoveAsync (couponFromStore);
			await _couponRepo.SaveAsync ();

			response.Result = couponFromStore;
			response.IsSuccess = true;
			response.StatusCode = HttpStatusCode.NoContent;

			return Results.Ok (response);
		}
		else
		{
			response.StatusCode = HttpStatusCode.NotFound;
			response.ErrorMessages.Add ("Invalid Coupon Id");
			return Results.NotFound (response);
		}
	}
}
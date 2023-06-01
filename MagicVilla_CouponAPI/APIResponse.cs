namespace MagicVilla_CouponAPI;

/// <summary>
/// My EndPoints response format.
/// </summary>
public class APIResponse
{
    public bool IsSuccess { get; set; }
    public Object Result { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
}
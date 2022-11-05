namespace MagicVilla_CouponAPI.Models.DTO;

public class CouponCreateRequest
{
    public string? Name { get; set; }
    public int Percent { get; set; }
    public bool IsActive { get; set; }
}
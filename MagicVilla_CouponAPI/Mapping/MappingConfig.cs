namespace MagicVilla_CouponAPI.Mapping;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        CreateMap<Coupon, CouponCreateRequest>().ReverseMap();
        CreateMap<Coupon, CouponCreateResponse>().ReverseMap();
        CreateMap<Coupon, CouponUpdateRequest>().ReverseMap();
        CreateMap<Coupon, CouponUpdateResponse>().ReverseMap();
        CreateMap<LocalUser, User>().ReverseMap();
    }
}
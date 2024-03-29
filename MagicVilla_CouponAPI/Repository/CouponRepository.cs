namespace MagicVilla_CouponAPI.Repository;

/// <summary>
/// This is my repository class. This class talks with my DbContext
/// </summary>
public class CouponRepository : ICouponRepository
{
    public readonly ApplicationDbContext _db;

    public CouponRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(Coupon coupon)
    {
        _db.Add(coupon);
    }

    public async Task<ICollection<Coupon>> GetAllAsync()
    {
        return await _db.Coupons!.ToListAsync();
    }

    public async Task<Coupon> GetAsync(int id)
    {
        return await _db.Coupons.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Coupon> GetAsync(string couponName)
    {
        return await _db.Coupons!.FirstOrDefaultAsync(c => c.Name.ToLower() == couponName.ToLower());
    }

    public async Task RemoveAsync(Coupon coupon)
    {
        _db.Remove(coupon);
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Coupon coupon)
    {
        _db.Coupons!.Update(coupon);
    }
}
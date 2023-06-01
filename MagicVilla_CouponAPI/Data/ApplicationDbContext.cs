namespace MagicVilla_CouponAPI.Data;

/// <summary>
/// My DbContext Class for Coupons and Auth
/// </summary>
public class ApplicationDbContext : DbContext
{
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<LocalUser> LocalUsers { get; set; }


    /// <summary>
    /// OnModelCreating. Adding 2 records for Coupon
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating (ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon> ()
            .HasData (
                new Coupon { Id = 1, Name = "10OFF", Percent = 10, IsActive = true },
                new Coupon { Id = 2, Name = "20OFF", Percent = 20, IsActive = true }
            );
    }
}
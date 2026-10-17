using BookingApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Data;

public sealed class BookingDbContext : DbContext
{
    public BookingDbContext(
        DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<BookingEntity> Bookings =>
        Set<BookingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var booking = modelBuilder.Entity<BookingEntity>();

        booking.ToTable("Bookings");

        booking.HasKey(item => item.Id);

        booking.Property(item => item.CustomerName)
            .IsRequired();

        booking.Property(item => item.PackageName)
            .IsRequired();

        booking.Property(item => item.HourlyRate)
            .HasPrecision(12, 4);

        booking.Property(item => item.DiscountPercent)
            .HasPrecision(9, 4);

        booking.Property(item => item.BookingFee)
            .HasPrecision(12, 4);

        booking.Property(item => item.Status)
            .IsRequired();
    }
}
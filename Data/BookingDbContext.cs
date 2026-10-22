using BookingApi.Entities;
using Microsoft.EntityFrameworkCore;

using BookingApi.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BookingApi.Data;

public sealed class BookingDbContext :  IdentityDbContext<ApplicationUser>
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
         base.OnModelCreating(modelBuilder);
        var booking = modelBuilder.Entity<BookingEntity>();

        booking.HasOne<ApplicationUser>()
    .WithMany()
    .HasForeignKey(item => item.OwnerUserId)
    .OnDelete(DeleteBehavior.Restrict);

booking.HasIndex(item => new { item.OwnerUserId, item.Id })
    .HasDatabaseName("IX_Bookings_OwnerUserId_Id");

        booking.ToTable("Bookings", table =>
        {
            table.HasCheckConstraint(
                "CK_Bookings_Hours",
                "\"Hours\" BETWEEN 1 AND 8");

            table.HasCheckConstraint(
                "CK_Bookings_HourlyRate",
                "\"HourlyRate\" > 0 AND \"HourlyRate\" <= 10000");

            table.HasCheckConstraint(
                "CK_Bookings_DiscountPercent",
                "\"DiscountPercent\" BETWEEN 0 AND 100");

            table.HasCheckConstraint(
                "CK_Bookings_BookingFee",
                "\"BookingFee\" BETWEEN 0 AND 10000");

            table.HasCheckConstraint(
                "CK_Bookings_Status",
                "\"Status\" IN ('Pending', 'Confirmed', 'Cancelled')");
        });

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

        booking.HasIndex(item => new
        {
            item.Status,
            item.Id
        })
        .HasDatabaseName("IX_Bookings_Status_Id");
    }
}
using LAS.Models;
using Microsoft.EntityFrameworkCore;

namespace LAS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }

        public DbSet<LateReturn> LateReturns { get; set; }
        public DbSet<Student> Students { get; set; }  // Student table now contains StudentRating
        public DbSet<PendingBorrow> PendingBorrows { get; set; }
        public DbSet<PendingReturn> PendingReturns { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .Property(b => b.SerialNumber)
                .HasColumnType("NVARCHAR(100)")
                .UseCollation("SQL_Latin1_General_CP1_CS_AS");

            modelBuilder.Entity<Student>()
                .Property(r => r.Rating)
                .HasDefaultValue(5.0);

            // ✅ Ensure MatricNumber is the same type in both tables
            modelBuilder.Entity<Student>()
                .Property(s => s.MatricNumber)
                .HasColumnType("NVARCHAR(20)"); // Adjust the size to match

            modelBuilder.Entity<BorrowRecord>()
                .Property(b => b.MatricNumber)
                .HasColumnType("NVARCHAR(20)"); // Ensure this matches the Student table

            // ✅ Define relationship with the same data type
            modelBuilder.Entity<Student>()
                .HasMany(s => s.BorrowRecords)
                .WithOne()
                .HasForeignKey(b => b.MatricNumber)
                .HasPrincipalKey(s => s.MatricNumber);

            // ✅ Enforce unique constraint on Email
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<BorrowRecord>()
                .Property(b => b.Overdue)
                .HasConversion<bool>();

            base.OnModelCreating(modelBuilder);
        }
       



    }
}

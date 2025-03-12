using Library_Management_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_Management_Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }

        public DbSet<BorrowRecord> BorrowRecords { get; set; }  

        public DbSet<LateReturn> LateReturns { get; set; }  

        public DbSet<Student> Students { get; set; }

        public DbSet<Lecturer> Lecturers{ get; set; }

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

            // ✅ Define relationship: One student has many borrow records
            modelBuilder.Entity<Student>()
                .HasMany(s => s.BorrowRecords)
                .WithOne()
                .HasForeignKey(b => b.MatricNumber)
                .HasPrincipalKey(s => s.MatricNumber);

            base.OnModelCreating(modelBuilder);
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LAS.Data;
using LAS.Models;
using LAS.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using static System.Reflection.Metadata.BlobBuilder;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _imageFolderPath;

        public BooksController(AppDbContext context)
        {
            _context = context;
            _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(_imageFolderPath))
            {
                Directory.CreateDirectory(_imageFolderPath);
            }
        }


        // GET: api/Books (Get all books)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            return await _context.Books.ToListAsync();
        }
        [HttpGet("books")]
        public async Task<IActionResult> GetBooks(
            [FromQuery] string? search = null,
            [FromQuery] string? sort = "Id",  // ✅ Default sorting by Id
            [FromQuery] string? order = "asc",
            [FromQuery] string? filter = null)
        {
            var books = _context.Books.AsQueryable();

            // 🔹 Search by Name or Serial Number
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b => b.Name.Contains(search) || b.SerialNumber.Contains(search));
            }

            // 🔹 Filtering Logic
            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter.ToLower())
                {
                    case "available":
                        books = books.Where(b => b.Quantity > 0);
                        break;
                    case "unavailable":
                        books = books.Where(b => b.Quantity == 0);
                        break;
                }
            }

            // 🔹 Sorting Logic (Including Default `Id`)
            var validColumns = new HashSet<string> { "Id", "Name", "Author", "SerialNumber", "Quantity" };
            if (validColumns.Contains(sort))
            {
                books = order.ToLower() == "asc"
                    ? books.OrderBy(b => EF.Property<object>(b, sort))
                    : books.OrderByDescending(b => EF.Property<object>(b, sort));
            }
            else
            {
                // ✅ Default to sorting by Id if sort parameter is invalid
                books = books.OrderBy(b => b.Id);
            }

            return Ok(await books.ToListAsync());
        }


        // GET: api/Books/{serialNumber} (Get a book by Serial Number)
        [HttpGet("{serialNumber}")]
        public async Task<IActionResult> GetBook(string serialNumber)
        {
            var book = await _context.Books
                .Where(b => b.SerialNumber == serialNumber)
                .Select(b => new
                {
                    b.SerialNumber,
                    b.Name,
                    b.Author,
                    b.Year,
                    Image = $"{Request.Scheme}://{Request.Host}/uploads/{b.ImagePath}".Replace("//", "/") // FIXED DOUBLE SLASH ISSUE
                })
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound(new { message = "Book not found" });
            }

            return Ok(book);
        }
        // Borrow a book (reduce quantity)
        //[HttpPost("borrow/{serialNumber}")]
        //public async Task<IActionResult> BorrowBook(string serialNumber)
        //{
        //    var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);

        //    if (book == null)
        //    {
        //        return NotFound(new { message = "Book not found." });
        //    }

        //    if (book.Quantity <= 0)
        //    {
        //        return BadRequest(new { message = "Book is out of stock." });
        //    }

        //    book.Quantity -= 1;
        //    _context.Entry(book).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Book borrowed successfully.", remainingQuantity = book.Quantity });
        //}
        //[HttpPost("borrow/{serialNumber}/{matricNumber}")]
        //public async Task<IActionResult> BorrowBook(string serialNumber, string matricNumber)
        //{
        //    var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == matricNumber);
        //    if (student == null)
        //    {
        //        return NotFound(new { message = "Student not found." });
        //    }
            
        //    var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
        //    if (book == null)
        //    {
        //        return NotFound(new { message = "Book not found." });
        //    }

        //    if (book.Quantity <= 0)
        //    {
        //        return BadRequest(new { message = "Book is out of stock." });
        //    }

        //    // ✅ Check if student has reached their borrow limit
        //    int borrowLimit = student.GetBorrowLimit(); // Limit based on rating
        //    int currentlyBorrowed = await _context.BorrowRecords
        //        .CountAsync(b => b.MatricNumber == matricNumber && !b.IsReturned);

        //    if (currentlyBorrowed >= borrowLimit)
        //    {
        //        return BadRequest(new { message = $"Borrow limit reached. You can only borrow {borrowLimit} books at a time." });
        //    }

        //    // Check if the user already borrowed this book
        //    var existingBorrow = await _context.BorrowRecords
        //        .FirstOrDefaultAsync(b => b.MatricNumber == matricNumber && b.SerialNumber == serialNumber && !b.IsReturned);

        //    if (existingBorrow != null)
        //    {
        //        return BadRequest(new { message = "You have already borrowed this book." });
        //    }

        //    // Allowed borrow time (e.g., 48 hours)
        //    float allowedBorrowHours = 48;
        //    DateTime dueDate = DateTime.UtcNow.AddHours(allowedBorrowHours);

        //    // Create borrow record
        //    var borrowRecord = new BorrowRecord
        //    {
        //        MatricNumber = matricNumber,
        //        SerialNumber = serialNumber,
        //        BorrowTime = DateTime.UtcNow,
        //        AllowedBorrowHours = allowedBorrowHours,
        //        DueDate = dueDate,
        //        IsReturned = false
        //    };

        //    //student.BorrowedBooks += 1; // ✅ Increase BorrowedBooks count
        //    book.Quantity -= 1;         // ✅ Reduce book quantity

        //    _context.BorrowRecords.Add(borrowRecord);
        //    await _context.SaveChangesAsync();
            

        //    return Ok(new
        //    {
        //        message = "Book borrowed successfully.",
        //        borrowTime = borrowRecord.BorrowTime,
        //        allowedBorrowHours,
        //        dueDate,
        //        borrowedBooks = _context.BorrowRecords.Count(b => b.MatricNumber == matricNumber && !b.IsReturned),
        //        borrowlimit =borrowLimit
        //    });
        //}

        [HttpGet("borrowed-books/{matricNumber}")]
        public async Task<IActionResult> GetBorrowedBooks(string matricNumber)
        {
            var student = await _context.Students
                .Include(s => s.BorrowRecords)
                .FirstOrDefaultAsync(s => s.MatricNumber == matricNumber);

            if (student == null)
            {
                return NotFound(new { message = "Student not found." });
            }

            return Ok(new
            {
                matricNumber = student.MatricNumber,
                
                borrowedBooks = _context.BorrowRecords.Count(b => b.MatricNumber == matricNumber && !b.IsReturned)  // ✅ Correct count
            });
        }
        [HttpPost("request-borrow/{serialNumber}/{matricNumber}")]
        public async Task<IActionResult> RequestBorrowCode(string serialNumber, string matricNumber)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == matricNumber);
            if (student == null)
            {
                return NotFound(new { message = "Student not found." });
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            if (book.Quantity <= 0)
            {
                return BadRequest(new { message = "Book is out of stock." });
            }
            //Check if the user already borrowed this book
               var existingBorrow = await _context.BorrowRecords
                   .FirstOrDefaultAsync(b => b.MatricNumber == matricNumber && b.SerialNumber == serialNumber && !b.IsReturned);

            if (existingBorrow != null)
            {
                return BadRequest(new { message = "You have already borrowed this book." });
            }

            // Generate a random 6-character alphanumeric code
            var borrowCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            var pendingBorrow = new PendingBorrow
            {
                MatricNumber = matricNumber,
                SerialNumber = serialNumber,
                BorrowCode = borrowCode,
                RequestTime = DateTime.UtcNow,
                IsApproved = false
            };

            _context.PendingBorrows.Add(pendingBorrow);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Borrow request submitted. Await admin approval.", borrowCode });
        }

        [HttpGet("pending-borrows")]
        public async Task<IActionResult> GetPendingBorrows()
        {
            var pendingBorrows = await _context.PendingBorrows
                .Where(pb => !pb.IsApproved) // Only show unapproved requests
                .Select(pb => new
                {
                    pb.Id,
                    pb.MatricNumber,
                    pb.SerialNumber,
                    pb.BorrowCode,
                    pb.RequestTime,
                    ExpiryTime = pb.RequestTime.AddHours(24) // Show expiration time
                })
                .ToListAsync();

            return Ok(pendingBorrows);
        }

        [HttpPost("approve-borrow/{borrowCode}")]
        public async Task<IActionResult> ApproveBorrowRequest(string borrowCode)
        {
            var pendingBorrow = await _context.PendingBorrows
                .FirstOrDefaultAsync(pb => pb.BorrowCode == borrowCode && !pb.IsApproved);

            if (pendingBorrow == null)
            {
                return NotFound(new { message = "Borrow request not found or already approved." });
            }

            pendingBorrow.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Borrow request approved.", borrowCode });
        }
        [HttpPost("borrow/{serialNumber}/{matricNumber}/{borrowCode}")]
        public async Task<IActionResult> BorrowBook(string serialNumber, string matricNumber, string borrowCode)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == matricNumber);
            var department = student.Department;
            var school = student.School;
            if (student == null)
            {
                return NotFound(new { message = "Student not found." });
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            var pendingBorrow = await _context.PendingBorrows
            .FirstOrDefaultAsync(pb => pb.MatricNumber == matricNumber && pb.SerialNumber == serialNumber && pb.BorrowCode == borrowCode);

            if (pendingBorrow == null || !pendingBorrow.IsApproved)
            {
                return BadRequest(new { message = "Invalid or unapproved borrow code." });
            }

            // ✅ Expiration check (e.g., 24 hours)
            if ((DateTime.UtcNow - pendingBorrow.RequestTime).TotalHours > 24)
            {
                _context.PendingBorrows.Remove(pendingBorrow);
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "Borrow code has expired. Request a new one." });
            }

            if (book.Quantity <= 0)
            {
                return BadRequest(new { message = "Book is out of stock." });
            }

            // ✅ Check if student has reached their borrow limit
            int borrowLimit = student.GetBorrowLimit(); // Limit based on rating
            int currentlyBorrowed = await _context.BorrowRecords
                .CountAsync(b => b.MatricNumber == matricNumber && !b.IsReturned);

            if (currentlyBorrowed >= borrowLimit)
            {
                return BadRequest(new { message = $"Borrow limit reached. You can only borrow {borrowLimit} books at a time." });
            }

            // Check if the user already borrowed this book
            var existingBorrow = await _context.BorrowRecords
                .FirstOrDefaultAsync(b => b.MatricNumber == matricNumber && b.SerialNumber == serialNumber && !b.IsReturned);

            if (existingBorrow != null)
            {
                return BadRequest(new { message = "You have already borrowed this book." });
            }

            // Allowed borrow time (e.g., 48 hours)
            float allowedBorrowHours = 48;
            DateTime dueDate = DateTime.UtcNow.AddHours(allowedBorrowHours);

            // Create borrow record
            var borrowRecord = new BorrowRecord
            {
                Department= department,
                School=school,
                MatricNumber = matricNumber,
                SerialNumber = serialNumber,
                BorrowTime = DateTime.UtcNow,
                AllowedBorrowHours = allowedBorrowHours,
                DueDate = dueDate,
                IsReturned = false
            };

            //student.BorrowedBooks += 1; // ✅ Increase BorrowedBooks count
            book.Quantity -= 1;         // ✅ Reduce book quantity

            _context.BorrowRecords.Add(borrowRecord);

            // ✅ Remove the borrow request from PendingBorrows
            _context.PendingBorrows.Remove(pendingBorrow);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Book borrowed successfully.",
                borrowTime = borrowRecord.BorrowTime,
                allowedBorrowHours,
                dueDate,
                borrowedBooks = _context.BorrowRecords.Count(b => b.MatricNumber == matricNumber && !b.IsReturned),
                borrowLimit = borrowLimit
            });
        }




        //[HttpPost("return/{serialNumber}/{matricNumber}")]
        //public async Task<IActionResult> ReturnBook(string serialNumber, string matricNumber)
        //{
        //    var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
        //    if (book == null)
        //    {
        //        return NotFound(new { message = "Book not found." });
        //    }

        //    // Find the borrow record
        //    var borrowRecord = await _context.BorrowRecords
        //        .FirstOrDefaultAsync(b => b.MatricNumber == matricNumber && b.SerialNumber == serialNumber && !b.IsReturned);

        //    if (borrowRecord == null)
        //    {
        //        return BadRequest(new { message = "No active borrow record found." });
        //    }

        //    // Get student record
        //    var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == matricNumber);
        //    if (student == null)
        //    {
        //        return NotFound(new { message = "Student not found." });
        //    }

        //    // Calculate late return
        //    DateTime returnTime = DateTime.UtcNow;
        //    bool isLate = returnTime > borrowRecord.DueDate;

        //    // Mark as returned
        //    borrowRecord.IsReturned = true;
        //    borrowRecord.ReturnTime = returnTime;
        //    book.Quantity += 1;

        //    // Update rating
        //    if (isLate)
        //    {
        //        student.Rating = Math.Max(1.0, student.Rating - 0.25); // Decrease rating for late return
        //    }
        //    else
        //    {
        //        student.Rating = Math.Min(10.0, student.Rating + 0.15); // Increase rating for early return
        //    }

        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Book returned successfully.", returnTime, isLate, newRating = student.Rating });
        //}


        [HttpPost("request-return/{serialNumber}/{matricNumber}")]
        public async Task<IActionResult> RequestReturnCode(string serialNumber, string matricNumber)
        {
            var borrowRecord = await _context.BorrowRecords
                .FirstOrDefaultAsync(b => b.MatricNumber == matricNumber && b.SerialNumber == serialNumber && !b.IsReturned);

            if (borrowRecord == null)
            {
                return BadRequest(new { message = "No active borrow record found." });
            }

            // Generate a random alphanumeric return code
            string returnCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            // Save in PendingReturns table
            var pendingReturn = new PendingReturn
            {
                MatricNumber = matricNumber,
                SerialNumber = serialNumber,
                ReturnCode = returnCode,
                RequestTime = DateTime.UtcNow,
                IsApproved = false
            };

            _context.PendingReturns.Add(pendingReturn);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Return request submitted. Await admin approval.", returnCode });
        }

        [HttpGet("pending-returns")]
        public async Task<IActionResult> GetPendingReturns()
        {
            var pendingReturns = await _context.PendingReturns
                .Where(pr => !pr.IsApproved)
                .Select(pr => new
                {
                    pr.Id,
                    pr.MatricNumber,
                    pr.SerialNumber,
                    pr.ReturnCode,
                    pr.RequestTime,
                    ExpiryTime = pr.RequestTime.AddHours(24) // Show expiration time
                })
                .ToListAsync();

            return Ok(pendingReturns);
        }

        [HttpPost("approve-return/{returnCode}")]
        public async Task<IActionResult> ApproveReturn(string returnCode)
        {
            //var pendingReturn = await _context.PendingReturns.FindAsync(id);

            //if (pendingReturn == null)
            //{
            //    return NotFound(new { message = "Return request not found." });
            //}

            //pendingReturn.IsApproved = true;
            //await _context.SaveChangesAsync();

            //return Ok(new { message = "Return request approved.", returnCode = pendingReturn.ReturnCode });
            var pendingReturn = await _context.PendingReturns
                .FirstOrDefaultAsync(pb => pb.ReturnCode == returnCode && !pb.IsApproved);

            if (pendingReturn == null)
            {
                return NotFound(new { message = "Return request not found or already approved." });
            }

            pendingReturn.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Return request approved.", returnCode });

        }

        [HttpPost("return/{serialNumber}/{matricNumber}/{returnCode}")]
        public async Task<IActionResult> ReturnBook(string serialNumber, string matricNumber, string returnCode)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            var borrowRecord = await _context.BorrowRecords
                .FirstOrDefaultAsync(b => b.MatricNumber == matricNumber && b.SerialNumber == serialNumber && !b.IsReturned);

            if (borrowRecord == null)
            {
                return BadRequest(new { message = "No active borrow record found." });
            }

            // Check if return code is valid
            var pendingReturn = await _context.PendingReturns
                .FirstOrDefaultAsync(pr => pr.MatricNumber == matricNumber && pr.SerialNumber == serialNumber && pr.ReturnCode == returnCode);

            if (pendingReturn == null || !pendingReturn.IsApproved)
            {
                return BadRequest(new { message = "Invalid or unapproved return code." });
            }

            // Expiration check (e.g., 24 hours)
            if ((DateTime.UtcNow - pendingReturn.RequestTime).TotalHours > 24)
            {
                _context.PendingReturns.Remove(pendingReturn);
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "Return code has expired. Request a new one." });
            }

            DateTime returnTime = DateTime.UtcNow;
            bool isLate = returnTime > borrowRecord.DueDate;

            borrowRecord.IsReturned = true;
            borrowRecord.ReturnTime = returnTime;
            book.Quantity += 1;

            var student = await _context.Students.FirstOrDefaultAsync(s => s.MatricNumber == matricNumber);
            if (student != null)
            {
                student.Rating = isLate ? Math.Max(1.0, student.Rating - 0.25) : Math.Min(10.0, student.Rating + 0.15);
            }

            // Remove the used return request
            _context.PendingReturns.Remove(pendingReturn);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Book returned successfully.", returnTime, isLate, newRating = student?.Rating });
        }



        [HttpGet("image/{serialNumber}")]
        public IActionResult GetBookImage(string serialNumber)
        {
            var book = _context.Books.FirstOrDefault(b => b.SerialNumber == serialNumber);
            if (book == null || string.IsNullOrEmpty(book.ImagePath))
            {
                return NotFound(new { message = "Book or image not found" });
            }

            // Construct the full image path correctly
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            var imagePath = Path.Combine(uploadsFolder, Path.GetFileName(book.ImagePath));

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound(new { message = "Image file does not exist", path = imagePath });
            }

            var imageFileStream = System.IO.File.OpenRead(imagePath);
            return File(imageFileStream, "image/png"); // Ensure the correct MIME type
        }



        // POST: api/Books (Create Book with Image)
        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook([FromForm] CreateBookDto createBookDto, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (file == null)
            {
                return BadRequest(new { message = "An image is required." });
            }

            if (await _context.Books.AnyAsync(b => b.SerialNumber == createBookDto.SerialNumber))
            {
                return BadRequest(new { message = "Serial Number must be unique." });
            }

            var book = new Book
            {
                SerialNumber = createBookDto.SerialNumber,
                Name = createBookDto.Name,
                Author = createBookDto.Author,
                Year = createBookDto.Year,
                Description = createBookDto.Description,
                Quantity = createBookDto.Quantity,
                ImagePath = await SaveAndResizeImage(file)
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { serialNumber = book.SerialNumber }, book);
        }

        // PUT: api/Books/{serialNumber} (Update Book Details)
        [HttpPut("{serialNumber}")]
        public async Task<IActionResult> UpdateBook(string serialNumber, [FromForm] UpdateBookDto updatedBook, IFormFile file)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            book.Name = updatedBook.Name;
            book.Author = updatedBook.Author;
            book.Year = updatedBook.Year;
            book.Description = updatedBook.Description;
            book.Quantity = updatedBook.Quantity;

            if (file != null)
            {
                // Delete old image if it exists (except default image)
                if (!string.IsNullOrEmpty(book.ImagePath) && book.ImagePath != "/images/default-book-cover.jpg")
                {
                    var oldImagePath = Path.Combine(_imageFolderPath, Path.GetFileName(book.ImagePath));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                book.ImagePath = await SaveAndResizeImage(file);
            }

            _context.Entry(book).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("borrow-history")]
        public async Task<IActionResult> GetAllBorrowHistory([FromQuery]string? matricNumber, [FromQuery] bool? overdue, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? Department, [FromQuery] string? School)// [FromQuery] int? Level 
        {
            var query = _context.BorrowRecords.AsQueryable();
            if (startDate.HasValue)
            {
                query = query.Where(b => b.BorrowTime >= startDate.Value);
            }
            //if (!string.IsNullOrEmpty(Department))
            //{
            //    query = query.Where(b => b.Department == Department);
            //}
            //if (!string.IsNullOrEmpty(matricNumber))
            //{
            //    query = query.Where(b => b.MatricNumber == matricNumber);
            //}
            if (!string.IsNullOrEmpty(Department))
            {
                query = query.Where(b => b.Department.Contains(Department));
            }

            if (!string.IsNullOrEmpty(School))
            {
                query = query.Where(b => b.School.Contains(School));
            }
            if (!string.IsNullOrEmpty(matricNumber))
            {
                query = query.Where(b => b.MatricNumber.Contains(matricNumber));
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.BorrowTime <= endDate.Value);
            }
            //if (Level.HasValue)
            //{
            //    query = query.Where(b => b.Level == Level);
            //}



            // Compute the statistics
            var totalBorrowed = await query.CountAsync();
            var totalReturned = await query.CountAsync(b => b.IsReturned);
            var totalLate = await query.CountAsync(b => b.IsReturned && b.ReturnTime > b.DueDate);
            var totalNotReturned = await query.CountAsync(b => !b.IsReturned);

            var borrowHistory = await query
                .Select(b => new
                {
                    b.Department,
                    b.School,
                    b.MatricNumber,
                    IsLate = b.IsLateReturn(),
                    b.IsReturned,
                    b.SerialNumber,
                    b.BorrowTime,
                    b.AllowedBorrowHours,
                    b.DueDate,
                    b.ReturnTime,
                    IsOverdue = !b.IsReturned && b.DueDate < DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new
            {
                TotalBorrowed = totalBorrowed,
                TotalReturned = totalReturned,
                TotalLate = totalLate,
                TotalNotReturned = totalNotReturned,
                BorrowHistory = borrowHistory
            });
        }

        [HttpGet("borrow-history/{matricNumber}")]
        public async Task<IActionResult> GetBorrowHistory(string matricNumber, [FromQuery] bool? overdue, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? Department )
        {
            var query = _context.BorrowRecords
                .Where(b => b.MatricNumber == matricNumber)
                .AsQueryable();
            

            if (startDate.HasValue)
            {
                query = query.Where(b => b.BorrowTime >= startDate.Value);
            }
            if (!string.IsNullOrEmpty(Department))
            {
                query = query.Where(b => b.Department == Department);
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.BorrowTime <= endDate.Value);
            }

            // Compute the statistics
            var totalBorrowed = await query.CountAsync();
            var totalReturned = await query.CountAsync(b => b.IsReturned);
            var totalLate = await query.CountAsync(b => b.IsReturned && b.ReturnTime > b.DueDate);
            var totalNotReturned = await query.CountAsync(b => !b.IsReturned);

            var borrowHistory = await query
                .Select(b => new
                {
                    b.Department,
                    b.School,
                    b.SerialNumber,
                    b.BorrowTime,
                    b.AllowedBorrowHours,
                    b.DueDate,
                    b.ReturnTime,
                    IsOverdue = !b.IsReturned && b.DueDate < DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new
            {
                TotalBorrowed = totalBorrowed,
                TotalReturned = totalReturned,
                TotalLate = totalLate,
                TotalNotReturned = totalNotReturned,
                BorrowHistory = borrowHistory
            });
        }


        [HttpPatch("{serialNumber}")]
        public async Task<IActionResult> PatchBook(string serialNumber, [FromBody] JsonPatchDocument<Book> patchDoc)
        {
            if (patchDoc == null || patchDoc.Operations.Count == 0)
            {
                return BadRequest(new { message = "Invalid or empty patch document." });
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            // Validate operations before applying them
            var validPaths = new HashSet<string> { "/name", "/author", "/year", "/description", "/quantity" };
            foreach (var operation in patchDoc.Operations)
            {
                if (operation.op.ToLower() != "replace")
                {
                    return BadRequest(new { message = $"Unsupported operation '{operation.op}'. Only 'replace' is allowed." });
                }

                if (!validPaths.Contains(operation.path.ToLower()))
                {
                    return BadRequest(new { message = $"Invalid field '{operation.path}'. You can only update: Name, Author, Year, Description, Quantity." });
                }
            }

            patchDoc.ApplyTo(book, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Entry(book).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Book updated successfully.", book });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the book.", error = ex.Message });
            }
        }



        // DELETE: api/Books/{serialNumber} (Delete a book)
        [HttpDelete("{serialNumber}")]
        public async Task<IActionResult> DeleteBook(string serialNumber)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            // Delete book image (if it's not the default image)
            if (!string.IsNullOrEmpty(book.ImagePath) && book.ImagePath != "/images/default-book-cover.jpg")
            {
                var imagePath = Path.Combine(_imageFolderPath, Path.GetFileName(book.ImagePath));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private async Task<string> SaveAndResizeImage(IFormFile file)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_imageFolderPath, uniqueFileName);

            using (var stream = file.OpenReadStream())
            using (var image = await Image.LoadAsync(stream))
            {
                int newWidth = 300;
                image.Mutate(x => x.Resize(newWidth, 0));
                await image.SaveAsync(filePath, new JpegEncoder());
            }

            return "/images/" + uniqueFileName;
        }
    }
}

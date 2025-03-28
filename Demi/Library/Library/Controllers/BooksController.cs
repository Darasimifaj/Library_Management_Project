using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Library.Data;
using Library.Models;
using Library.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using static System.Reflection.Metadata.BlobBuilder;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Library.Controllers
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
       
        //
        [HttpGet("books")]
        public async Task<IActionResult> GetBooks(
    [FromQuery] string? search = null,
    [FromQuery] string? sort = "Id",
    [FromQuery] string? order = "asc",
    [FromQuery] string? filter = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

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

            // 🔹 Sorting Logic
            var validColumns = new HashSet<string> { "Id", "Name", "Author", "SerialNumber", "Quantity" };
            if (validColumns.Contains(sort))
            {
                books = order.ToLower() == "asc"
                    ? books.OrderBy(b => EF.Property<object>(b, sort))
                    : books.OrderByDescending(b => EF.Property<object>(b, sort));
            }
            else
            {
                books = books.OrderBy(b => b.Id);
            }

            // 🔹 Pagination
            var totalRecords = await books.CountAsync();
            var pagedBooks = await books
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = pagedBooks
            };

            return Ok(response);
        }

        //
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
                    b.Description,
                    b.Quantity,
                    Image = $"{Request.Scheme}://{Request.Host}/uploads/{b.ImagePath}".Replace("//", "/") // FIXED DOUBLE SLibraryH ISSUE
                })
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound(new { message = "Book not found" });
            }

            return Ok(book);
        }
        
        [HttpPatch("upload-image")]
        public async Task<IActionResult> UploadBookImage([FromQuery] string serialNumber, IFormFile file)
        {
            if (string.IsNullOrEmpty(serialNumber))
            {
                return BadRequest(new { message = "Serial Number is required." });
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No image uploaded." });
            }

            try
            {
                // Save the image and update the database
                var filePath = await SaveAndResizeImage(file);
                book.ImagePath = filePath ?? string.Empty; // Ensure it's not NULL

                await _context.SaveChangesAsync();
                return Ok(new { message = "Image uploaded successfully.", imagePath = book.ImagePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while saving the image.", error = ex.Message });
            }
        }


        [HttpPost("request-borrow/{serialNumber}/{UserId}")]
        public async Task<IActionResult> RequestBorrowCode(string serialNumber, string UserId)
        {
            UserId = Uri.UnescapeDataString(UserId);
         
            var student = await _context.Users.FirstOrDefaultAsync(s => s.UserId == UserId);
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
            int borrowLimit = student.GetBorrowLimit(); // Limit based on rating
            int currentlyBorrowed = await _context.BorrowRecords
                .CountAsync(b => b.UserId == UserId && !b.IsReturned);

            if (currentlyBorrowed >= borrowLimit)
            {
                return BadRequest(new { message = $"Borrow limit reached. You can only borrow {borrowLimit} books at a time." });
            }
            //Check if the user already borrowed this book
            var existingBorrow = await _context.BorrowRecords
                   .FirstOrDefaultAsync(b => b.UserId == UserId && b.SerialNumber == serialNumber && !b.IsReturned);

            if (existingBorrow != null)
            {
                return BadRequest(new { message = "You have already borrowed this book." });
            }

            // Generate a random 6-character alphanumeric code
            var borrowCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            var pendingBorrow = new PendingBorrow
            {
                UserId = UserId,
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
            var currentTime = DateTime.UtcNow;
            var expiryDuration = TimeSpan.FromHours(0.1);

            // Get pending borrows including expired ones
            var pendingBorrows = await _context.PendingBorrows
                .Where(pb => !pb.IsApproved)
                .Select(pb => new
                {
                    pb.Id,
                    pb.UserId,
                    pb.SerialNumber,
                    pb.BorrowCode,
                    pb.RequestTime,
                    ExpiryTime = pb.RequestTime.Add(expiryDuration) // Show expiration time
                })
                .ToListAsync();

            // Find expired requests
            var expiredRequests = pendingBorrows
                .Where(pb => pb.ExpiryTime <= currentTime)
                .ToList();

            // Remove expired requests from the database
            if (expiredRequests.Any())
            {
                var expiredIds = expiredRequests.Select(pb => pb.Id).ToList();
                var expiredEntities = await _context.PendingBorrows
                    .Where(pb => expiredIds.Contains(pb.Id))
                    .ToListAsync();

                _context.PendingBorrows.RemoveRange(expiredEntities);
                await _context.SaveChangesAsync();
            }

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
        [HttpPost("borrow/{serialNumber}/{UserId}/{borrowCode}")]
        public async Task<IActionResult> BorrowBook(string serialNumber, string UserId, string borrowCode)
        {
            UserId = Uri.UnescapeDataString(UserId);
            var student = await _context.Users.FirstOrDefaultAsync(s => s.UserId == UserId);
            
            if (student == null)
            {
                return NotFound(new { message = "Student not found." });
            } 
            var UserType = student.UserType;
            var department = student.Department;
            var school = student.School;
            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            var pendingBorrow = await _context.PendingBorrows
            .FirstOrDefaultAsync(pb => pb.UserId == UserId && pb.SerialNumber == serialNumber && pb.BorrowCode == borrowCode);

            if (pendingBorrow == null || !pendingBorrow.IsApproved)
            {
                return BadRequest(new { message = "Invalid or unapproved borrow code." });
            }

            // ✅ Expiration check (e.g., 24 hours)
            if ((DateTime.UtcNow - pendingBorrow.RequestTime).TotalHours > 0.1)
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
                .CountAsync(b => b.UserId == UserId && !b.IsReturned);

            if (currentlyBorrowed >= borrowLimit)
            {
                return BadRequest(new { message = $"Borrow limit reached. You can only borrow {borrowLimit} books at a time." });
            }

            // Check if the user already borrowed this book
            var existingBorrow = await _context.BorrowRecords
                .FirstOrDefaultAsync(b => b.UserId == UserId && b.SerialNumber == serialNumber && !b.IsReturned);

            if (existingBorrow != null)
            {
                return BadRequest(new { message = "You have already borrowed this book." });
            }

            // Allowed borrow time (e.g., 48 hours)
            float allowedBorrowHours = 0.1F;
            DateTime dueDate = DateTime.UtcNow.AddHours(allowedBorrowHours);

            // Create borrow record
            var borrowRecord = new BorrowRecord
            {
                Department= department,
                School=school,
                UserId = UserId,
                UserType= UserType,
                SerialNumber = serialNumber,
                BorrowTime = DateTime.UtcNow,
                AllowedBorrowHours = allowedBorrowHours,
                DueDate = dueDate,
                IsReturned = false,
                
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
                borrowedBooks = _context.BorrowRecords.Count(b => b.UserId == UserId && !b.IsReturned),
                borrowLimit = borrowLimit
            });
        }

        [HttpPost("request-return/{serialNumber}/{UserId}")]
        public async Task<IActionResult> RequestReturnCode(string serialNumber, string UserId)
        {
            UserId = Uri.UnescapeDataString(UserId);
            var borrowRecord = await _context.BorrowRecords
                .FirstOrDefaultAsync(b => b.UserId == UserId && b.SerialNumber == serialNumber && !b.IsReturned);

            if (borrowRecord == null)
            {
                return BadRequest(new { message = "No active borrow record found." });
            }

            // Generate a random alphanumeric return code
            string returnCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            // Save in PendingReturns table
            var pendingReturn = new PendingReturn
            {
                UserId = UserId,
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
            var currentTime = DateTime.UtcNow;
            var expiryDuration = TimeSpan.FromHours(0.1);

            // Get pending returns including expired ones
            var pendingReturns = await _context.PendingReturns
                .Where(pr => !pr.IsApproved)
                .Select(pr => new
                {
                    pr.Id,
                    pr.UserId,
                    pr.SerialNumber,
                    pr.ReturnCode,
                    pr.RequestTime,
                    ExpiryTime = pr.RequestTime.Add(expiryDuration) // Show expiration time
                })
                .ToListAsync();

            // Find expired return requests
            var expiredRequests = pendingReturns
                .Where(pr => pr.ExpiryTime <= currentTime)
                .ToList();

            // Remove expired return requests from the database
            if (expiredRequests.Any())
            {
                var expiredIds = expiredRequests.Select(pr => pr.Id).ToList();
                var expiredEntities = await _context.PendingReturns
                    .Where(pr => expiredIds.Contains(pr.Id))
                    .ToListAsync();

                _context.PendingReturns.RemoveRange(expiredEntities);
                await _context.SaveChangesAsync();
            }

            return Ok(pendingReturns);
        }


        [HttpPost("approve-return/{returnCode}")]
        public async Task<IActionResult> ApproveReturn(string returnCode)
        {
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

        [HttpPost("return/{serialNumber}/{UserId}/{returnCode}")]
        public async Task<IActionResult> ReturnBook(string serialNumber, string UserId, string returnCode)
        {
            UserId = Uri.UnescapeDataString(UserId);
            var book = await _context.Books.FirstOrDefaultAsync(b => b.SerialNumber == serialNumber);
            if (book == null)
            {
                return NotFound(new { message = "Book not found." });
            }

            var borrowRecord = await _context.BorrowRecords
                .FirstOrDefaultAsync(b => b.UserId == UserId && b.SerialNumber == serialNumber && !b.IsReturned);

            if (borrowRecord == null)
            {
                return BadRequest(new { message = "No active borrow record found." });
            }

            // Check if return code is valid
            var pendingReturn = await _context.PendingReturns
                .FirstOrDefaultAsync(pr => pr.UserId == UserId && pr.SerialNumber == serialNumber && pr.ReturnCode == returnCode);

            if (pendingReturn == null || !pendingReturn.IsApproved)
            {
                return BadRequest(new { message = "Invalid or unapproved return code." });
            }

            // Expiration check (e.g., 24 hours)
            if ((DateTime.UtcNow - pendingReturn.RequestTime).TotalHours > 0.1)
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

            var student = await _context.Users.FirstOrDefaultAsync(s => s.UserId == UserId);
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
        public async Task<IActionResult> GetAllBorrowHistory([FromQuery]string? UserId, [FromQuery]string? UserType, [FromQuery] string? serialnumber, [FromQuery] bool? overdue, [FromQuery] bool?IsReturned, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? Department, [FromQuery] string? School)// [FromQuery] int? Level 
        {   if(!string.IsNullOrEmpty(UserId))
            {
                UserId = Uri.UnescapeDataString(UserId);
            }
            var query = _context.BorrowRecords.AsQueryable();
            if (startDate.HasValue)
            {
                query = query.Where(b => b.BorrowTime >= startDate.Value);
            }
            if (!string.IsNullOrEmpty(Department))
            {
                query = query.Where(b => b.Department.Contains(Department));
            }
            if (!string.IsNullOrEmpty(UserType))
            {
                query = query.Where(b => b.UserType.Contains(UserType));
            }
            if (!string.IsNullOrEmpty(serialnumber))
            {
                query = query.Where(b => b.SerialNumber.Contains(serialnumber));
            }

            if (!string.IsNullOrEmpty(School))
            {
                query = query.Where(b => b.School.Contains(School));
            }
            if (!string.IsNullOrEmpty(UserId))
            {
                query = query.Where(b => b.UserId.Contains(UserId));
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.BorrowTime <= endDate.Value);
            }

            if (overdue.HasValue)
            {
                query = query.Where(b => b.Overdue == (overdue.Value ? true : false)); // If Overdue is truly bool
                                                                                       // OR, if Overdue is stored as int, use:
                                                                                       // query = query.Where(b => b.OverdueInt == (overdue.Value ? 1 : 0));
            }


            if (IsReturned.HasValue)
            {
                query = query.Where(b => b.IsReturned == IsReturned.Value);
            }
            
            // Compute the statistics
            var totalOverdue =await query.CountAsync(b => b.Overdue);
            var totalBorrowed = await query.CountAsync();
            var totalReturned = await query.CountAsync(b => b.IsReturned);
            var totalLate = await query.CountAsync(b => b.ReturnTime > b.BorrowTime.AddHours(b.AllowedBorrowHours));
            var totalEarly = await query.CountAsync(b => b.ReturnTime <= b.BorrowTime.AddHours(b.AllowedBorrowHours));

            var totalNotReturned = await query.CountAsync(b => !b.IsReturned);
            var borrowHistory = await query
                .Select(b => new
                {
                    b.Department,
                    b.School,
                    b.UserId,
                    b.UserType,
                    IsLateReturn = b.IsLateReturn(),
                    b.IsReturned,
                    b.SerialNumber,
                    b.BorrowTime,
                    b.AllowedBorrowHours,
                    b.DueDate,
                    b.ReturnTime,
                    b.Overdue
                })
                .ToListAsync();
            
            return Ok(new
            {   TotalOverdue= totalOverdue,
                TotalBorrowed = totalBorrowed,
                TotalReturned = totalReturned,
                TotalLate = totalLate,
                TotalNotReturned = totalNotReturned,
                BorrowHistory = borrowHistory
            });
        }

        [HttpGet("borrow-history/{UserId}")]
        public async Task<IActionResult> GetBorrowHistory(string UserId, [FromQuery] bool? overdue, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? Department )
        {
            UserId = Uri.UnescapeDataString(UserId);
            var query = _context.BorrowRecords
                .Where(b => b.UserId == UserId)
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
                    b.SerialNumber,
                    b.BorrowTime,
                    b.AllowedBorrowHours,
                    b.DueDate,
                    b.ReturnTime,
                    b.Overdue,
                    LateReturn=b.IsLateReturn()
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

        [HttpGet("export")]
        public async Task<IActionResult> ExportBooks()
        {
            var books = await _context.Books.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Books");
                worksheet.Cell(1, 1).Value = "Serial Number";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Author";
                worksheet.Cell(1, 4).Value = "Year";
                worksheet.Cell(1, 5).Value = "Quantity";
                worksheet.Cell(1, 6).Value = "Image Path";
                worksheet.Cell(1, 6).Value = "Description";

                int row = 2;
                foreach (var book in books)
                {
                    worksheet.Cell(row, 1).Value = book.SerialNumber;
                    worksheet.Cell(row, 2).Value = book.Name;
                    worksheet.Cell(row, 3).Value = book.Author;
                    worksheet.Cell(row, 4).Value = book.Year;
                    worksheet.Cell(row, 5).Value = book.Quantity;
                    worksheet.Cell(row, 6).Value = book.ImagePath ?? "N/A";
                    worksheet.Cell(row, 7).Value = book.Description;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Books.xlsx");
                }
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportBooks(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row

                    var books = new List<Book>();
                    var skippedBooks = new List<string>();

                    foreach (var row in rows)
                    {
                        string serialNumber = row.Cell(1).GetString().Trim();
                        string name = row.Cell(2).GetString().Trim();
                        string author = row.Cell(3).GetString().Trim();
                        string description = row.Cell(4).GetString().Trim();
                        string image_path = row.Cell(5).GetString().Trim();
                        int year;
                        int quantity;

                        if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(description)|| string.IsNullOrEmpty(image_path) ||
                            !int.TryParse(row.Cell(6).GetString(), out year) || !int.TryParse(row.Cell(7).GetString(), out quantity))
                        {
                            skippedBooks.Add($"Row {row.RowNumber()}: Invalid or missing data.");
                            continue;
                        }

                        if (await _context.Books.AnyAsync(b => b.SerialNumber == serialNumber))
                        {
                            skippedBooks.Add($"Row {row.RowNumber()}: Serial Number '{serialNumber}' already exists.");
                            continue;
                        }

                        var book = new Book
                        {
                            SerialNumber = serialNumber,
                            Name = name,
                            Author = author,
                            Year = year,
                            Quantity = quantity,
                            Description =description,
                            ImagePath=image_path
                        };

                        books.Add(book);
                    }

                    if (books.Count > 0)
                    {
                        _context.Books.AddRange(books);
                        await _context.SaveChangesAsync();
                    }

                    return Ok(new
                    {
                        message = $"{books.Count} books imported successfully.",
                        skippedBooks = skippedBooks.Count > 0 ? skippedBooks : null
                    });
                }
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

            
                var imagePath = Path.Combine(_imageFolderPath, Path.GetFileName(book.ImagePath));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
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

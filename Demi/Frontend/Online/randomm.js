function authorizedFetch(url, options = {}) {
  const token = sessionStorage.getItem("token"); // Your JWT token
  const apiKey = "your-api-key"; // Replace with your actual API key

  const headers = {
    ...options.headers,
    "Content-Type": "application/json",
    "x-api-key": apiKey,
    Authorization: `Bearer ${token}`,
  };

  return fetch(url, {
    ...options,
    headers,
  });
}

let currentPage = 1;
const pageSize = 5;

document.addEventListener("DOMContentLoaded", function () {
  let book_serialNumber = [];
  const booksContainer = document.querySelector(".book_list");
  const searchInput = document.getElementById("search");
  const searchBtn = document.getElementById("search_btn");

  const urlParams = new URLSearchParams(window.location.search);
  const searchQuery = urlParams.get("search") || "";
  if (searchQuery !== null) {
    searchInput.value = searchQuery;
  }

  fetchBooks();

  async function fetchBooks() {
    const userId = sessionStorage.getItem("userId");
    const search = searchInput.value.trim();
    const sort = document.getElementById("sort").value || "Id";
    const order = document.getElementById("order").value || "asc";
    const filter = document.getElementById("filter").value;

    const booksApiUrl = `https://localhost:44354/api/Books/books?search=${encodeURIComponent(
      search
    )}&sort=${sort}&order=${order}&filter=${filter}&pageNumber=${currentPage}&pageSize=${pageSize}`;

    try {
      const response = await authorizedFetch(booksApiUrl);
      const book = await response.json();
      const books = book.data;
      const currentpage = book.pageNumber;
      const totalPages = book.totalPages;
      prevBtn.style.backgroundColor = currentPage > 1 ? "#611991" : "#611991b2";
      nextBtn.style.backgroundColor =
        currentPage < totalPages ? "#611991" : "#611991b2";
      booksContainer.innerHTML = "";

      for (const book of books) {
        book_serialNumber.push(book.serialNumber);

        const borrowHistoryUrl = `https://localhost:44354/api/Books/borrow-history?UserId=${encodeURIComponent(
          userId
        )}&serialnumber=${book.serialNumber}&IsReturned=false&IsOnline=true`;

        const imageUrl = `https://localhost:44354/api/books/image/${book.serialNumber}`;
        const imgresponse = await authorizedFetch(imageUrl);
        const blob = await imgresponse.blob();
        const objectURL = URL.createObjectURL(blob);

        const borrowResponse = await authorizedFetch(borrowHistoryUrl);
        const borrowData = await borrowResponse.json();
        const isBorrowed = borrowData.totalNotReturned > 0;
        const pdf = book.pdfPath;

        const bookElement = document.createElement("div");

        bookElement.classList.add("book");
        bookElement.setAttribute("data-serial", book.serialNumber);
        bookElement.setAttribute("data-serials", book.name);

        bookElement.innerHTML = `
          <img src="${objectURL}" alt="${
          book.name
        }" onerror="this.src='default.jpg'" />
          <h3>${truncateText(book.name, 4)}</h3>
          <p>${truncateText(book.author, 4)}</p>
          <div class="book_details">
            <button class="${
              isBorrowed ? "borrowed" : pdf == null ? "unavailable" : "borrow"
            }">
              ${isBorrowed ? "Read" : pdf == null ? "Unavailable" : "Borrow"}
            </button>
            <a href="BookDetails.html?serialNumber=${
              book.serialNumber
            }" class="details">View <br />Details</a>
          </div>
          
        `;
        booksContainer.appendChild(bookElement);

        console.log(bookElement);
      }
      console.log(book_serialNumber);
      if (!booksContainer.innerHTML) {
        document.getElementById("pagination").style.display = "none";
        document.getElementById("errormsg").style.display = "block";
      }

      document.getElementById("loader").style.display = "none";
      updatePagination(totalPages);
    } catch (error) {
      console.error("Error fetching books:", error);
      document.getElementById("loader").style.display = "none";
    }
  }

  function updatePagination(totalPages) {
    const prevBtn = document.getElementById("prevBtn");
    const nextBtn = document.getElementById("nextBtn");
    const pageNumber = document.getElementById("pageNumber");

    pageNumber.textContent = `Page ${currentPage} of ${totalPages}`;
    prevBtn.disabled = currentPage === 1;
    if (currentPage === 1) {
      prevBtn.style.backgroundColor = "#611991b2";
    } else {
      prevBtn.style.backgroundColor = "#611991";
    }
    if (currentPage === totalPages) {
      nextBtn.style.backgroundColor = "#611991b2";
    } else {
      nextBtn.style.backgroundColor = "#611991";
    }
    nextBtn.disabled = currentPage === totalPages;
  }

  function changePage(direction) {
    const prevBtn = document.getElementById("prevBtn");
    const nextBtn = document.getElementById("nextBtn");
    const pageText = document.getElementById("pageNumber").textContent;
    const totalPages = parseInt(pageText.split(" ")[3]);

    if (direction === "next" && currentPage < totalPages) {
      currentPage++;
    } else if (direction === "prev" && currentPage > 1) {
      currentPage--;
    }

    fetchBooks();
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  // Event delegation for book buttons
  booksContainer.addEventListener("click", function (e) {
    const bookElement = e.target.closest(".book");
    if (!bookElement) return;

    const serialNumber = bookElement.getAttribute("data-serial");
    const bookName = bookElement.getAttribute("data-serials");

    if (e.target.classList.contains("borrow")) {
      localStorage.setItem("savedLocation", window.location.href);
      localStorage.setItem("bookName", bookName);
      localStorage.setItem("selectedSerial", serialNumber);
      window.location.href = "RequestBorrow.html";
    }

    if (e.target.classList.contains("borrowed")) {
      localStorage.setItem("savedLocation", window.location.href);
      localStorage.setItem("bookName", bookName);
      localStorage.setItem("selectedSerial", serialNumber);
      window.location.href = "ReadBook.html";
    }

    if (e.target.classList.contains("details")) {
      localStorage.setItem("selectedSerial", serialNumber);
      window.location.href = "BookDetails.html";
    }
  });

  // Search triggers
  searchInput.addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
      event.preventDefault();
      fetchBooks();
    }
  });

  searchBtn.addEventListener("click", function () {
    fetchBooks();
  });

  // Pagination button events
  document.getElementById("prevBtn").addEventListener("click", function () {
    changePage("prev");
  });

  document.getElementById("nextBtn").addEventListener("click", function () {
    changePage("next");
  });
});

function truncateText(text, wordCount) {
  const words = text.split(" ");
  return words.length > wordCount
    ? words.slice(0, wordCount).join(" ") + "..."
    : text;
}

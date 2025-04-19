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

let currentPage = 1; // Initialize the current page
const pageSize = 5;
document.addEventListener("DOMContentLoaded", async function () {
  const booksContainer = document.querySelector(".book_list");
  const searchInput = document.getElementById("search");
  const searchBtn = document.getElementById("search_btn");

  // Get search query from URL
  const urlParams = new URLSearchParams(window.location.search);
  const searchQuery = urlParams.get("search") || "";

  // Set search input value if query exists
  if (searchQuery != null) {
    searchInput.value = searchQuery;
  }

  // Number of books per page (you can adjust this)

  fetchBooks();

  async function fetchBooks() {
    const userId = sessionStorage.getItem("userId");
    const search = searchInput.value.trim();
    const sortSelect = document.getElementById("sort");
    const sort = sortSelect.value || "Id";
    const order = document.getElementById("order").value || "asc";
    const filter = document.getElementById("filter").value;

    const booksApiUrl = `https://localhost:44354/api/Books/books?search=${encodeURIComponent(
      search
    )}&sort=${sort}&order=${order}&filter=${filter}&pageNumber=${currentPage}&pageSize=${pageSize}`;

    try {
      const response = await authorizedFetch(booksApiUrl);
      const book = await response.json();
      const books = book.data;
      const totalPages = book.totalPages; // Assuming the response includes totalPages

      // Clear previous books
      booksContainer.innerHTML = "";

      // Display books
      for (const book of books) {
        const borrowHistoryUrl = `https://localhost:44354/api/Books/borrow-history?UserId=${encodeURIComponent(
          userId
        )}&serialnumber=${book.serialNumber}&IsReturned=false&IsOnline=false`;
        const imageUrl = `https://localhost:44354/api/books/image/${book.serialNumber}`;
        const borrowResponse = await authorizedFetch(borrowHistoryUrl);
        const borrowData = await borrowResponse.json();

        const isBorrowed = borrowData.totalNotReturned > 0;
        
        const imgresponse = await authorizedFetch(imageUrl);
        const blob = await imgresponse.blob();
        let objectURL = null;
        if (blob.type.startsWith("image/")) {
          objectURL = URL.createObjectURL(blob);
        }

        
        const bookElement = document.createElement("div");
        bookElement.classList.add("book");
        bookElement.setAttribute("data-serial", book.serialNumber);
        bookElement.innerHTML = `
          <img src="${objectURL || "BookImages/BOOK1.png"}" alt="${
          book.name
        }" />
          <h3>${truncateText(book.name, 4)}</h3>
          <p>${truncateText(book.author, 4)}</p>
          <div class="book_details">
            <button class="${
              isBorrowed
                ? "borrowed"
                : book.quantity === 0
                ? "unavailable"
                : "borrow"
            }">
              ${
                isBorrowed
                  ? "Borrowed"
                  : book.quantity === 0
                  ? "Unavailable"
                  : "Borrow"
              }
            </button>
            <a href="BookDetails.html?serialNumber=${
              book.serialNumber
            }" class="details">View <br />Details</a>
          </div>
        `;
        document.getElementById("loader").style.display = "none";
        if (booksContainer.innerHTML == null) {
          document.getElementById("pagination").style.display = "none";
          document.getElementById("errormsg").style.display = "block";
        }
        booksContainer.appendChild(bookElement);
        document.querySelectorAll(".borrow").forEach((button) => {
          button.addEventListener("click", (event) => {
            const bookElement = event.target.closest(".book");
            const serialNumber = bookElement.getAttribute("data-serial");
            localStorage.setItem("savedLocation", window.location.href);
            localStorage.setItem("selectedSerial", serialNumber);
            window.location.href = "RequestBorrow.html";
          });
        });

        document.querySelectorAll(".details").forEach((button) => {
          button.addEventListener("click", (event) => {
            const bookElement = event.target.closest(".book");
            const serialNumber = bookElement.getAttribute("data-serial");

            localStorage.setItem("selectedSerial", serialNumber);
            window.location.href = "BookDetails.html";
          });
        });
      }

      updatePagination(totalPages); // Update pagination controls
    } catch (error) {
      console.error("Error fetching books:", error);
    }
  }

  function updatePagination(totalPages) {
    const prevBtn = document.getElementById("prevBtn");
    const nextBtn = document.getElementById("nextBtn");
    const pageNumber = document.getElementById("pageNumber");

    pageNumber.textContent = `Page ${currentPage} of ${totalPages}`;

    // Disable/Enable Previous button
    prevBtn.disabled = currentPage === 1;

    // Disable/Enable Next button
    nextBtn.disabled = currentPage === totalPages;
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
  }

  // Function to change page
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

  // Trigger search on Enter key
  searchInput.addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
      event.preventDefault();
      fetchBooks();
    }
  });

  // Trigger search on button click
  searchBtn.addEventListener("click", function () {
    fetchBooks();
  });

  // Add event listeners for pagination buttons
  document.getElementById("prevBtn").addEventListener("click", function () {
    changePage("prev");
  });

  document.getElementById("nextBtn").addEventListener("click", function () {
    changePage("next");
  });
});

function truncateText(text, wordCount) {
  let words = text.split(" ");
  return words.length > wordCount
    ? words.slice(0, wordCount).join(" ") + "..."
    : text;
}

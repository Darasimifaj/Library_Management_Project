let currentPage = 1;
const pageSize = 5;

document.addEventListener("DOMContentLoaded", () => {
  const searchInput = document.getElementById("search");
  const searchBtn = document.getElementById("search_btn");
  const historyContainer = document.querySelector(".book_list");

  (async function init() {
    await fetchAndDisplayBorrowedBooks();
  })();

  async function fetchAndDisplayBorrowedBooks() {
    const userId = sessionStorage.getItem("userId");
    const search = searchInput.value.trim();
    const sort = document.getElementById("sort")?.value || "name";
    const order = document.getElementById("order")?.value || "asc";
    const filter = document.getElementById("filter")?.value || "";

    const url = `https://localhost:44354/api/Books/books-by-borrow-history?UserId=${encodeURIComponent(
      userId
    )}&search=${encodeURIComponent(
      search
    )}&sort=${sort}&order=${order}&filter=${filter}&IsReturned=false&IsOnline=true&pageNumber=${currentPage}&pageSize=${pageSize}`;

    try {
      const response = await authorizedFetch(url);
      const result = await response.json();
      const borrowedBooks = result.data;
      const totalPages = result.totalPages;

      historyContainer.innerHTML = "";

      if (!borrowedBooks.length) {
        document.getElementById("pagination").style.display = "none";
        return;
      }

      document.getElementById("pagination").style.display = "flex";

      for (const book of borrowedBooks) {
        const imageUrl = `https://localhost:44354/api/books/image/${book.serialNumber}`;
        const imgRes = await authorizedFetch(imageUrl);
        const blob = await imgRes.blob();
        const objectURL = URL.createObjectURL(blob);

        const bookElement = document.createElement("div");
        bookElement.classList.add("book");
        bookElement.innerHTML = `
          <img src="${objectURL}" alt="${
          book.name
        }" onerror="this.src='default.jpg'" />
          <h3>${truncateText(book.name, 4)}</h3>
          <p>${truncateText(book.author, 4)}</p>
          <div class="book_details">
            <button class="${
              book.pdfPath == null ? "unavailable" : "borrowed"
            }">
              ${book.pdfPath == null ? "Unavailable" : "Read"}
            </button>
            <a href="BookDetails.html?serialNumber=${
              book.serialNumber
            }" class="details">
              View <br />Details
            </a>
          </div>
        `;
        historyContainer.appendChild(bookElement);
      }

      updatePagination(totalPages);
    } catch (err) {
      console.error("Failed to load borrowed books:", err);
    }
  }

  function truncateText(text, wordCount) {
    const words = text.split(" ");
    return words.length > wordCount
      ? words.slice(0, wordCount).join(" ") + "..."
      : text;
  }

  function updatePagination(totalPages) {
    const pageNumber = document.getElementById("pageNumber");
    pageNumber.textContent = `Page ${currentPage} of ${totalPages}`;

    document.getElementById("prevBtn").disabled = currentPage === 1;
    document.getElementById("nextBtn").disabled = currentPage === totalPages;
  }

  function changePage(direction) {
    const totalPages = parseInt(
      document.getElementById("pageNumber").textContent.split(" ")[3]
    );
    if (direction === "next" && currentPage < totalPages) currentPage++;
    else if (direction === "prev" && currentPage > 1) currentPage--;
    fetchAndDisplayBorrowedBooks();
  }

  // Event Listeners
  searchInput.addEventListener("keypress", (e) => {
    if (e.key === "Enter") fetchAndDisplayBorrowedBooks();
  });
  searchBtn.addEventListener("click", fetchAndDisplayBorrowedBooks);
  document
    .getElementById("prevBtn")
    .addEventListener("click", () => changePage("prev"));
  document
    .getElementById("nextBtn")
    .addEventListener("click", () => changePage("next"));
});

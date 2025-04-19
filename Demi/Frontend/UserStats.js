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

document.addEventListener("DOMContentLoaded", () => {
  const searchBtn = document.getElementById("search_btn");
  const prevBtn = document.getElementById("prev_page");
  const nextBtn = document.getElementById("next_page");
  const pageInfo = document.getElementById("page_info");

  let currentPage = 1;
  const pageSize = 10; // Set number of records per page

  fetchBorrowHistory();

  async function fetchBorrowHistory(page = 1) {
    const serialNumber = document.getElementById("serialNumber").value.trim();
    const isReturned = document.getElementById("isReturned").value;
    const overdue = document.getElementById("overdue").value;
    const startDate = document.getElementById("startDate").value;
    const endDate = document.getElementById("endDate").value;
    const department = document.getElementById("department").value;
    const school = document.getElementById("school").value;
    const userId = sessionStorage.getItem("userId");

    const borrowUrl = `https://localhost:44354/api/Books/borrow-history?UserId=${encodeURI(
      userId
    )}&serialnumber=${serialNumber}&overdue=${overdue}&IsReturned=${isReturned}&startDate=${startDate}&endDate=${endDate}&Department=${department}&School=${school}&pageNumber=${page}&pageSize=${pageSize}&IsOnline=false`;

    try {
      const userResponse = await authorizedFetch(
        `https://localhost:44354/api/user/${encodeURIComponent(userId)}`
      );
      const User = await userResponse.json();
      document.querySelector(".rCategory").innerHTML = User.rCategory;

      const borrowResponse = await authorizedFetch(borrowUrl);
      const data = await borrowResponse.json();

      // console.log(borrowUrl);
      // console.log(data);

      const history = data.borrowHistory;
      document.querySelector(".TBB").innerHTML = data.totalBorrowed;
      document.querySelector(".TBR").innerHTML = data.totalReturned;
      // console.log(history);

      // Update pagination info
      currentPage = data.currentPage;
      updatePaginationUI(data.totalPages);

      const table = document.querySelector(".active_table");
      table.innerHTML = `<tr class="active_title">
            <th>Serial Number</th>
            <th>Borrowed Time</th>
            <th>Due Date</th>
            <th>Return Time</th>
            <th>Status</th>
          </tr>`; // Clear previous entries before adding new ones

      history.forEach((book) => {
        const row = document.createElement("tr");
        row.className = "active_data1";
        row.innerHTML = `
          <td>${book.serialNumber}</td>
          <td>${formatDate(book.borrowTime)}</td>
          <td>${formatDate(book.dueDate)}</td>
          <td>${book.returnTime ? formatDate(book.returnTime) : "N/A"}</td>
          <td><button class="${
            book.isLateReturn
              ? "late"
              : book.isReturned
              ? "returned"
              : book.overdue
              ? "due"
              : "pending"
          }">
          ${
            book.isLateReturn
              ? "Late"
              : book.isReturned
              ? "Returned"
              : book.overdue
              ? "Overdue"
              : "Pending"
          }</button></td>
        `;
        document.getElementById("loader").style.display = "none";
        if (!row.innerHTML) {
          document.getElementById("pagination").style.display = "none";
          document.getElementById("errormsg").style.display = "block";
        }
        table.appendChild(row);
      });
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  }

  function formatDate(dateString) {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleString();
  }

  function updatePaginationUI(totalPages) {
    pageInfo.innerText = `Page ${currentPage} of ${totalPages}`;
    prevBtn.disabled = currentPage === 1;
    nextBtn.disabled = currentPage >= totalPages;
  }

  // ✅ Search on Enter key press
  document.querySelectorAll("input").forEach((input) => {
    input.addEventListener("keypress", function (event) {
      if (event.key === "Enter") {
        event.preventDefault();
        fetchBorrowHistory(1); // Reset to page 1 on new search
      }
    });
  });

  // ✅ Search on button click
  searchBtn.addEventListener("click", function () {
    fetchBorrowHistory(1); // Reset to page 1 on new search
  });

  // ✅ Pagination Controls
  prevBtn.addEventListener("click", function () {
    if (currentPage > 1) {
      fetchBorrowHistory(currentPage - 1);
      prevBtn.style.backgroundColor = "#611991";
    }
    if ((currentPage = 1)) {
      prevBtn.style.backgroundColor = "#f0f0f0 ";
    }
  });

  nextBtn.addEventListener("click", function () {
    fetchBorrowHistory(currentPage + 1);
    nextBtn.style.backgroundColor = "#611991";
  });
});

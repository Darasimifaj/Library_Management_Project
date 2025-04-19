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
  // Set number of records per page

  fetchuser();

  async function fetchuser(currentPage = 1) {
    const userId = document.getElementById("userId").value.trim();
    const firstName = document.getElementById("firstName").value.trim();
    const lastName = document.getElementById("lastName").value.trim();
    const email = document.getElementById("email").value.trim();
    const rating = document.getElementById("Rating").value.trim();
    const userType = document.getElementById("userType").value.trim();
    const department = document.getElementById("department").value.trim();
    const school = document.getElementById("school").value.trim();
    const isActive = document.getElementById("isActive").value.trim();
    const isAdmin = document.getElementById("isAdmin").value.trim();
    const isLoggedIn = document.getElementById("IsLoggedIn").value.trim();
    const TA = document.getElementById("TA");
    const TS = document.getElementById("TS");
    const TL = document.getElementById("TL");
    const OU = document.getElementById("OU");
    const TU = document.getElementById("TU");

    const userUrl = `https://localhost:44354/api/user/Users?UserId=${encodeURIComponent(
      userId
    )}&UserType=${userType}&FirstName=${firstName}&LastName=${lastName}&IsAdmin=${isAdmin}&IsActive=${isActive}&IsLogged=${isLoggedIn}&Department=${department}&School=${school}&Rating=${rating}&email=${email}&pageNumber=${currentPage}&pageSize=10`;

    try {
      const UserResponse = await authorizedFetch(userUrl);
      const data = await UserResponse.json();
      TA.innerText=data.totalAdmins;
      TS.innerText=data.totalStudents;
      TL.innerText=data.totalLecturers;
      OU.innerText=data.totalLogged;
      TU.innerText=data.total;
      const users = data.users;
      // document.querySelector(".TBB").innerHTML = data.totalBorrowed;
      // document.querySelector(".TBR").innerHTML = data.totalReturned;
      // console.log(history);

      // Update pagination info
      currentPage = data.currentPage;
      updatePaginationUI(data.totalPages);

      const table = document.querySelector(".active_table");
      table.innerHTML = `<tr class="active_title">
          <th>UserId</th>
          <th>First name</th>
          <th>Last Name</th>
          <th>User Type</th>
          <th>Department</th>
          <th>Rating</th>
          <th>IsAdmin</th>
          <th>IsActive</th>
          </tr>`; // Clear previous entries before adding new ones

      users.forEach((user) => {
        const ratin = user.rating;
        const rating = parseFloat(ratin).toFixed(2); // Format rating to 2 decimal places
        const row = document.createElement("tr");
        row.className = "active_data1";
        row.innerHTML = `
        <td>${user.userId}</td>
          <td>${user.firstName}</td>
          <td>${user.lastName}</td>
          <td>${user.userType}</td>
          <td>${user.department}</td>
          <td>${rating}</td>
          <td>${user.isAdmin}</td>
          <td>${user.isActive}</td>
          
        `;
        // document.getElementById("loader").style.display = "none";
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

  function updatePaginationUI(totalPages) {
    pageInfo.innerText = `Page ${currentPage} of ${totalPages}`;
    prevBtn.disabled = currentPage === 1;
    nextBtn.disabled = currentPage >= totalPages;
    if (currentPage === 1) {
      prevBtn.style.backgroundColor = "white";
    } else {
      prevBtn.style.backgroundColor = "#245a7e17";
    }
    if (currentPage === totalPages) {
      nextBtn.style.backgroundColor = "white";
    } else {
      nextBtn.style.backgroundColor = "#245a7e17";
    }
  }

  // ✅ Search on Enter key press
  document.querySelectorAll("input").forEach((input) => {
    input.addEventListener("keypress", function (event) {
      if (event.key === "Enter") {
        event.preventDefault();
        fetchuser(1); // Reset to page 1 on new search
      }
    });
  });

  // ✅ Search on button click
  searchBtn.addEventListener("click", function () {
    fetchuser(1); // Reset to page 1 on new search
  });

  // ✅ Pagination Controls
  prevBtn.addEventListener("click", function () {
    if (currentPage > 1) {
      fetchuser(currentPage - 1);
    }
  });

  nextBtn.addEventListener("click", function () {
    fetchuser(currentPage + 1);
  });
});

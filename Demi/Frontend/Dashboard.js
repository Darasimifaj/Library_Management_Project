// If not logged in, redirect to login page
const API_BASE_URL = "https://localhost:44354/api/userlogin";
if (!sessionStorage.getItem("isLoggedIn")) {
  window.location.href = "StudentLogin.html";
}

function logoutAPI() {
  sessionStorage.removeItem("userId");
  sessionStorage.removeItem("isLoggedIn");
  window.location.href = "StudentLogin.html";
}

async function logout() {
  const userId = sessionStorage.getItem("userId");
  if (!userId) return alert("No active session found");

  try {
    const response = await fetch(
      `${API_BASE_URL}/logout?userId=${encodeURIComponent(userId)}`,
      {
        method: "POST",
      }
    );

    const data = await response.json();
    if (!response.ok) throw new Error(data.message);

    logoutAPI();
    alert("Logout successful");
  } catch (error) {
    console.error("Logout error:", error.message);
    alert(error.message);
  }
}

// Function to check session status
async function checkSession() {
  const userId = sessionStorage.getItem("userId");
  if (!userId) return alert("No active session");

  try {
    const response = await fetch(
      `${API_BASE_URL}/check-session?userId=${userId}`
    );
    const data = await response.json();

    if (!response.ok) throw new Error(data.message);
    alert(`Session active. Expiry: ${data.sessionExpiry}`);
  } catch (error) {
    console.error("Session check error:", error.message);
    alert(error.message);
  }
}

// Function to handle auto-logout when session expires
async function autoLogout() {
  const userId = sessionStorage.getItem("userId");
  if (!userId) return;

  try {
    const response = await fetch(
      `${API_BASE_URL}/auto-logout?userId=${userId}`,
      {
        method: "POST",
      }
    );

    const data = await response.json();
    console.log(data);
    if (!response.ok) {
      logout();
      alert("Session expired. Logged out automatically.");
    }
  } catch (error) {
    console.error("Auto-logout error:", error.message);
  }
}

// Run autoLogout immediately when the script loads
autoLogout();

// Periodic check for session expiration
setInterval(autoLogout, 60000); // Check every 1 minute
function togglePopup(event) {
  event.stopPropagation(); // Prevents closing when clicking the profile pic
  const popup = document.getElementById("popup");
  popup.style.display = popup.style.display === "block" ? "none" : "block";
}

// Close pop-up when clicking anywhere outside of it
document.addEventListener("click", function () {
  const popup = document.getElementById("popup");
  popup.style.display = "none";
});

document.addEventListener("DOMContentLoaded", async function () {
  const userId = sessionStorage.getItem("userId");
  if (!userId) {
    console.error("UserId number not found in session storage.");
    alert("UserId number not found. Please log in again.");
    return;
  }
  // console.log(userId);

  const emailElement = document.getElementById("Email");
  const nameElement = document.getElementById("Name");

  if (!emailElement || !nameElement) {
    console.error("Email or Name element not found in the DOM.");
    alert("Error: Email or Name field is missing on the page.");
    return;
  }

  try {
    // Fetch User details
    const UserResponse = await fetch(
      `https://localhost:44354/api/user/${encodeURIComponent(userId)}`
    );
    if (!UserResponse.ok) throw new Error("User not found");

    const User = await UserResponse.json();

    // Update the UI with fetched User details
    emailElement.innerText = User.email;
    const welcome = document.querySelector(".welcome_class");
    welcome.innerHTML = "Welcome " + User.firstName;
    nameElement.innerText = "Hi, " + User.firstName;

    // console.log("Email:", User.Email);
    // console.log("Email:", User);
    // console.log(userId);
  } catch (error) {
    console.error("Error:", error.message);
    alert(error.message);
  }
});
document.addEventListener("DOMContentLoaded", function () {
  const searchInput = document.getElementById("search");
  const searchBtn = document.getElementById("search_btn");

  function redirectToSearch() {
    const searchValue = searchInput.value.trim();
    if (searchValue) {
      window.location.href = `Browse.html?search=${encodeURIComponent(
        searchValue
      )}`;
    }
  }

  // ✅ Trigger search on Enter key
  searchInput.addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
      event.preventDefault();
      redirectToSearch();
    }
  });

  // ✅ Trigger search on button click
  searchBtn.addEventListener("click", redirectToSearch);
});

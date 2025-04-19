if (localStorage.getItem("darkMode") === "enabled") {
  document.body.classList.add("dark-mode");
}

document.getElementById('bi-circle-half').addEventListener('click', function () {
  const drop = document.querySelector('.drop');
  if (drop.style.display === 'block') {
    drop.style.display = 'none';
  } else {
    drop.style.display = 'block';
  }
});

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
const moonBtn = document.getElementById("moon");
const sunBtn = document.getElementById("sun");

if (moonBtn) {
  moonBtn.onclick = () => {
    document.body.classList.add("dark-mode");
    localStorage.setItem("darkMode", "enabled");
    if (localStorage.getItem("darkMode") == "enabled") {
      moonBtn.style.backgroundColor = "#61199133";
      sunBtn.style.backgroundColor = "transparent";
    } else if (localStorage.getItem("darkMode") == "disabled") {
      moonBtn.style.backgroundColor = "transparent";
      sunBtn.style.backgroundColor = "#61199133";
    }
  };
}
if (localStorage.getItem("darkMode") == "enabled") {
  moonBtn.style.backgroundColor = "#61199133";
  sunBtn.style.backgroundColor = "transparent";
} else if (localStorage.getItem("darkMode") == "disabled") {
  moonBtn.style.backgroundColor = "transparent";
  sunBtn.style.backgroundColor = "#61199133";
}
if (sunBtn) {
  sunBtn.onclick = () => {
    document.body.classList.remove("dark-mode");
    localStorage.setItem("darkMode", "disabled");
    if (localStorage.getItem("darkMode") == "enabled") {
    moonBtn.style.backgroundColor = "#61199133";
    sunBtn.style.backgroundColor = "transparent";
  } else if (localStorage.getItem("darkMode") == "disabled") {
    moonBtn.style.backgroundColor = "transparent";
    sunBtn.style.backgroundColor = "#61199133";
  }
  };
  
}
// If not logged in, redirect to login page
const API_BASE_URL = "https://localhost:44354/api/userlogin";
if (!sessionStorage.getItem("isLoggedIn")) {
  window.location.href = "/StudentLogin.html";
}

function logoutAPI() {
  sessionStorage.removeItem("userId");
  sessionStorage.removeItem("isLoggedIn");
  window.location.href = "/StudentLogin.html";
}
function logoutAPIs() {
  sessionStorage.removeItem("userId");
  sessionStorage.removeItem("isLoggedIn");
  alert("Logging Out");
  window.location.href = "/StudentLogin.html";
}

async function logout() {
  const userId = sessionStorage.getItem("userId");
  if (!userId) return alert("No active session found");

  try {
    const response = await authorizedFetch(
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
    const response = await authorizedFetch(
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
    const response = await authorizedFetch(
      `${API_BASE_URL}/auto-logout?userId=${userId}`,
      {
        method: "POST",
      }
    );

    const data = await response.json();
    console.log(data);
    if (!response.ok) {
      logoutAPIs();
      alert("Session expired. Logged out automatically.");
    }
  } catch (error) {
    logoutAPIs();
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
    const UserResponse = await authorizedFetch(
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
async function checkType() {
  const userId = sessionStorage.getItem("userId");
  const userType = sessionStorage.getItem("userType");
  if (!userId) return alert("No active session");

  try {
    const response = await authorizedFetch(
      `${API_BASE_URL}/checkType?userId=${userId}&usertype=${userType}&isAdmin=true`
    );
    const data = await response.json();

    if (!response.ok) {
      logoutAPIs();
      alert("Not Admin.");
      throw new Error(data.message);
    }
  } catch (error) {
    console.error("Check-Type error:", error.message);
    alert(error.message);
  }
}
document.addEventListener("DOMContentLoaded", async function () {
  checkType();
  const userId = sessionStorage.getItem("userId");
  const url = `https://localhost:44354/api/userlogin/Isverified?userId=${userId}`;
  const response = await authorizedFetch(url);
  const data = response.json();
  console.log(data);
  if (response.status === 200) {
    console.log("User is verified");
  } else {
    logout();
    window.location.href = "/Verifyemail.html";
  }
});

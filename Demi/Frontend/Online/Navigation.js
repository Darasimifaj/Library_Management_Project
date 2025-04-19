// ✅ Apply dark mode early (before DOMContentLoaded to avoid flicker)
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

// ✅ Load Navigation.html and attach event listeners afterward
fetch("Navigation.html")
  .then((response) => response.text())
  .then(async (data) => {
    document.getElementById("nav-container").innerHTML = data;
    setupNavigationEvents(); // Attach nav event listeners

    // ✅ Then load user data after nav is loaded (because Email/Name are in the nav)
    await loadUserDetails();
  })
  .catch((error) => console.error("Error loading navigation:", error));

// ✅ Constants and Utility
const API_BASE_URL = "https://localhost:44354/api/userlogin";

function authorizedFetch(url, options = {}) {
  const token = sessionStorage.getItem("token");
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

// ✅ Auto redirect if not logged in
if (!sessionStorage.getItem("isLoggedIn")) {
  window.location.href = "/StudentLogin.html";
}

// ✅ Logout handlers
function logoutAPI() {
  sessionStorage.removeItem("userId");
  sessionStorage.removeItem("isLoggedIn");
  window.location.href = "/StudentLogin.html";
}

async function logout() {
  const userId = sessionStorage.getItem("userId");
  if (!userId) return alert("No active session found");

  try {
    const response = await authorizedFetch(
      `${API_BASE_URL}/logout?userId=${encodeURIComponent(userId)}`,
      { method: "POST" }
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

function logoutAPIs() {
  sessionStorage.removeItem("userId");
  sessionStorage.removeItem("isLoggedIn");
  alert("Logging Out");
  window.location.href = "/StudentLogin.html";
}

// ✅ Session Check & Auto Logout
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

async function autoLogout() {
  const userId = sessionStorage.getItem("userId");
  if (!userId) return;

  try {
    const response = await authorizedFetch(
      `${API_BASE_URL}/auto-logout?userId=${encodeURIComponent(userId)}`,
      { method: "POST" }
    );

    const data = await response.json();
    if (!response.ok) {
      logoutAPIs();
      alert("Session expired. Logged out automatically.");
    }
  } catch (error) {
    logoutAPIs();
    console.error("Auto-logout error:", error.message);
  }
}

// ✅ Run once immediately and every 1 minute
autoLogout();
setInterval(autoLogout, 60000);

// ✅ Sidebar functions
function closeSidebar() {
  const sidebar = document.getElementById("SideH");
  if (sidebar) sidebar.style.display = "none";
}

function openSidebar() {
  const sidebar = document.getElementById("SideH");
  if (sidebar) sidebar.style.display = "flex";
}

// ✅ Popup toggle
// ✅ Popup toggle
function togglePopup(event) {
  event.stopPropagation(); // Prevent click event from bubbling up to the window

  const popup1 = document.getElementById("popup1");
  const popup = document.getElementById("popup");

  if (popup) {
    // Toggle the display of popup
    popup.style.display = popup.style.display === "block" ? "none" : "block";
  }

  if (popup1) {
    // Toggle the display of popup1
    popup1.style.display = popup1.style.display === "block" ? "none" : "block";
  }
}

// ✅ Close popups when clicking outside
window.addEventListener("click", (event) => {
  const popup = document.getElementById("popup");
  const popup1 = document.getElementById("popup1");

  if (popup && !popup.contains(event.target)) {
    popup.style.display = "none"; // Close popup when clicked outside
  }

  if (popup1 && !popup1.contains(event.target)) {
    popup1.style.display = "none"; // Close popup1 when clicked outside
  }
});

// ✅ Attach all event listeners after nav is loaded
function setupNavigationEvents() {
  const closeButton2 = document.getElementById("close2");
  const closeButton = document.getElementById("close");
  const openButton = document.getElementById("open");

  if (closeButton) closeButton.addEventListener("click", closeSidebar);
  if (closeButton2) closeButton2.addEventListener("click", closeSidebar);
  if (openButton) openButton.addEventListener("click", openSidebar);

  const profileBtn = document.querySelector("profilePic");
  if (profileBtn) profileBtn.addEventListener("click", togglePopup);

  document
    .querySelectorAll(".logout-btn")
    .forEach((btn) => btn.addEventListener("click", logout));

  // Close popup when clicking outside
  window.addEventListener("click", () => {
    const popup = document.getElementById("popup1");
    if (popup) popup.style.display = "none";
  });

  // Dark mode toggles
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
}

// ✅ Load user details once DOM is ready
async function loadUserDetails() {
  const userId = sessionStorage.getItem("userId");
  if (!userId) {
    console.error("UserId number not found in session storage.");
    alert("UserId number not found. Please log in again.");
    return;
  }

  const emailElement = document.getElementById("Email");
  const nameElement = document.getElementById("Name");

  if (!emailElement || !nameElement) {
    console.error("Email or Name element not found in the DOM.");
    return;
  }

  try {
    const studentResponse = await authorizedFetch(
      `https://localhost:44354/api/user/${encodeURIComponent(userId)}`
    );
    if (!studentResponse.ok) throw new Error("Student not found");

    const student = await studentResponse.json();
    emailElement.innerText = student.email;
    nameElement.innerText = "Hi, " + student.firstName;
  } catch (error) {
    console.error("Error fetching student details:", error.message);
    alert(error.message);
  }
}
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

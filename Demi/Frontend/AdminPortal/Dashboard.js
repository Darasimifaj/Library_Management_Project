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

const API_BASE_URL = "https://localhost:44354/api/userlogin";

// Redirect to login if not logged in
// if (!sessionStorage.getItem("isLoggedIn")) {
//   window.location.href = "StudentLogin.html";
// }

// Function to handle user login
// async function login(userId, password, role) {
//   try {
//     const response = await fetch(
//       `${API_BASE_URL}/student?UserId=${encodeURIComponent(
//         userId
//       )}&Password=${Password}`,
//       {
//         method: "POST",
//         headers: {
//           "Content-Type": "application/json",
//         },
//       }
//     );

//     const data = await response.json();
//     if (!response.ok) throw new Error(data.message || "Login failed");

//     sessionStorage.setItem("sessionExpiry", data.sessionExpiry);
//     sessionStorage.setItem("userId", userId);
//     sessionStorage.setItem("isLoggedIn", "true");
//     alert(`${role} login successful!`);
//   } catch (error) {
//     console.error("Login error:", error.message);
//     alert(error.message);
//   }
// }

// Function to handle user logout
function logoutAPI() {
  sessionStorage.removeItem("userId");
  sessionStorage.removeItem("isLoggedIn");
  // window.location.href = "StudentLogin.html";
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

function logoutAPIs() {
  sessionStorage.removeItem("userId");
  sessionStorage.removeItem("isLoggedIn");
  alert("Logging Out");
  // window.location.href = "StudentLogin.html";
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
// async function autoLogout() {
//   const userId = sessionStorage.getItem("userId");
//   if (!userId) return;

//   try {
//     const response = await authorizedFetch(
//       `${API_BASE_URL}/auto-logout?userId=${encodeURIComponent(userId)}`,
//       {
//         method: "POST",
//       }
//     );

//     const data = await response.json();
//     console.log(data);
//     if (!response.ok) {
//       logoutAPIs();
//       alert("Session expired. Logged out automatically.");
//     }
//   } catch (error) {
//     logoutAPIs();
//     console.error("Auto-logout error:", error.message);
//   }
// }

// // Run autoLogout immediately when the script loads
// autoLogout();

// // Periodic check for session expiration
// setInterval(autoLogout, 60000); // Check every 1 minute

function closeSidebar() {
  const sidebar = document.getElementById("SideH");
  if (sidebar) {
    sidebar.style.display = "none "; // Hide the sidebar
  }
}
function openSidebar() {
  const sidebar = document.getElementById("SideH");
  if (sidebar) {
    sidebar.style.display = "flex "; // Hide the sidebar
  }
}

function togglePopup(event) {
  event.stopPropagation(); // Prevents closing when clicking the profile pic
  const popup = document.getElementById("popup");
  const popup1 = document.getElementById("popup1");
  popup.style.display = popup.style.display === "flex" ? "none" : "flex";
  popup1.style.display = popup1.style.display === "flex" ? "none" : "flex";
}

// Close pop-up when clicking anywhere outside of it
document.addEventListener("click", function () {
  const popup = document.getElementById("popup");
  popup.style.display = "none";
  const popup1 = document.getElementById("popup1");
  popup1.style.display = "none";
});

document.addEventListener("DOMContentLoaded", function () {
  const closeButton2 = document.getElementById("close2");
  const closeButton = document.getElementById("close");
  const openButton = document.getElementById("open");

  if (closeButton) {
    closeButton.addEventListener("click", closeSidebar);
  }

  if (closeButton2) {
    closeButton2.addEventListener("click", closeSidebar);
  }
  if (openButton) {
    openButton.addEventListener("click", openSidebar);
  }
});
document.addEventListener("DOMContentLoaded", async function () {
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
    alert("Error: Email or Name field is missing on the page.");
    return;
  }

  try {
    // Fetch student details
    const studentResponse = await authorizedFetch(
      `https://localhost:44354/api/user/${encodeURIComponent(userId)}`
    );
    if (!studentResponse.ok) throw new Error("Student not found");

    const Student = await studentResponse.json();

    // Update the UI with fetched student details
    emailElement.innerText = Student.email;
    nameElement.innerText = "Hi, " + Student.firstName;

    // console.log("Email:", Student.Email);
    // console.log("Email:", Student);
    // console.log(userId);
  } catch (error) {
    console.error("Error:", error.message);
    alert(error.message);
  }
});

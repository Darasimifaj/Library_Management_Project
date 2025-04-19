if (sessionStorage.getItem("isLoggedIn")) {
  window.location.href = "Dashboard.html";
}

const userId = document.getElementById("matric").value;
const password = document.getElementById("password").value;
const passwordinput = document.getElementById("password");
const errorMessage = document.getElementById("error-message");
const API_BASE_URL = "https://localhost:44354/api/userlogin";

// Redirect to login if not logged in

// Function to handle user login
async function login() {
  const userId = document.getElementById("matric").value;
  const password = document.getElementById("password").value;
  const errorMessage = document.getElementById("error-message");
  const API_BASE_URL = "https://localhost:44354/api/userlogin";
  try {
    const response = await fetch(
      `${API_BASE_URL}/student?UserId=${encodeURIComponent(
        userId
      )}&password=${password}`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    const data = await response.json();

    if (!response.ok) throw new Error(data.message || "Login failed");

    sessionStorage.setItem("sessionExpiry", data.sessionExpiry);
    sessionStorage.setItem("userId", userId);
    sessionStorage.setItem("userType", "Student");
    const token = data.token;
    sessionStorage.setItem("token", token);
    console.log(response.json);
    sessionStorage.setItem("isLoggedIn", "true");
    alert(`Student login successful!`);
    localStorage.setItem("email", data.email);
    window.location.href = "Dashboard.html";
  } catch (error) {
    console.error("Login error:", error.message);
    errorMessage.textContent = error.message;
  }
}
document.addEventListener("DOMContentLoaded", function () {
  passwordinput.addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
      event.preventDefault();
      login();
    }
  });
});
function openSidebar() {
  const main = document.getElementById("Main");
  const sidebar = document.getElementById("SideH");
  if (sidebar) {
    sidebar.style.display = "flex "; // Hide the sidebar
    main.style.display = "none";
  }
}
function closeSidebar() {
  const main = document.getElementById("Main");
  const sidebar = document.getElementById("SideH");
  if (sidebar) {
    sidebar.style.display = "none "; // Hide the sidebar
    main.style.display = "flex";
  }
}
document.addEventListener("DOMContentLoaded", function () {
  const closeButton = document.getElementById("close");
  const openButton = document.getElementById("open");

  if (closeButton) {
    closeButton.addEventListener("click", closeSidebar);
  }
  document.querySelector(".login-btn").addEventListener("click", login);

  if (openButton) {
    openButton.addEventListener("click", openSidebar);
  }
});
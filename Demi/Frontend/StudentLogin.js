if (sessionStorage.getItem("isLoggedIn")) {
  window.location.href = "Dashboard.html";
}
function togglePassword() {
  var passwordField = document.getElementById("password");
  var toggleText = document.querySelector(".toggle-password");

  if (passwordField.type === "password") {
    passwordField.type = "text";
    toggleText.textContent = "Hide Password";
  } else {
    passwordField.type = "password";
    toggleText.textContent = "Show Password";
  }
}

// async function login() {
//   const matricNo = document.getElementById("matric").value;
//   const password = document.getElementById("password").value;
//   const errorMessage = document.getElementById("error-message");

//   if (!matricNo || !password) {
//     errorMessage.textContent = "Matric No and Password are required!";
//     return;
//   }

//   const loginData = {
//     matricNumber: matricNo,
//     password: password,
//   };

//   try {
//     //  https://localhost:44354/api/Students/login?MatricNumber=22%2F2464&Password=Trip
//     const response = await fetch(
//       `https://localhost:44354/api/Students/login?matricNo=${matricNo}&password=${password}`,
//       {
//         method: "POST",
//         headers: { "Content-Type": "application/json" },
//       }
//     );

//     console.log(response);
//     if (!response.ok) {
//       throw new Error("Invalid Matric No or Password");
//     }

//     const data = await response.json();
//     console.log("Login successful", data);

//     // ✅ Store login status
//     sessionStorage.setItem("isLoggedIn", "true");

//     // ✅ Store Matric Number
//     sessionStorage.setItem("matricNo", matricNo);

//     // Redirect to Dashboard
//     window.location.href = "Dashboard.html";
//   } catch (error) {
//     console.error("Error:", error);
//     errorMessage.textContent = error.message;
//   }
// }
const userId = document.getElementById("matric").value;
const password = document.getElementById("password").value;
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
    console.log(response.json);
    sessionStorage.setItem("isLoggedIn", "true");
    alert(`Student login successful!`);
    window.location.href = "Dashboard.html";
  } catch (error) {
    console.error("Login error:", error.message);
    errorMessage.textContent = error.message;
  }
}

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

  password.addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
      event.preventDefault();
      login();
    }
  });

  if (closeButton) {
    closeButton.addEventListener("click", closeSidebar);
  }

  if (openButton) {
    openButton.addEventListener("click", openSidebar);
  }
});

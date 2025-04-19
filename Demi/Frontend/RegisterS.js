//
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

document.addEventListener("DOMContentLoaded", function () {
  const registerBtn = document.getElementById("register-btn");
  if (registerBtn) {
    registerBtn.addEventListener("click", register);
  }
});

const first = document.getElementById("first").value.trim();
const last = document.getElementById("last").value.trim();
const email = document.getElementById("email").value.trim();
const School = document.getElementById("School").value.trim();
const matric = document.getElementById("matric").value.trim();
// const password = document.getElementById("password").value.trim();
const Department = document.getElementById("Department").value.trim();
const url = `https://localhost:44354/api/user/register?UserId=${matric}&FirstName=${first}&LastName=${last}&Email=${encodeURIComponent(
  email
)}&UserType=Student&IsAdmin=false&Department=${Department}&School=${School}&Password=${password}`;
const errorMessage = document.getElementById("error-message");

const registerBtn = document.getElementById("register-btn");
async function register() {
  try {
    const first = document.getElementById("first").value.trim();
    const last = document.getElementById("last").value.trim();
    const email = document.getElementById("email").value.trim();
    const School = document.getElementById("School").value.trim();
    const matric = document.getElementById("matric").value.trim();
    const password = document.getElementById("password").value.trim();
    const Department = document.getElementById("Department").value.trim();
    const errorMessage = document.getElementById("error-message");
    const url = `https://localhost:44354/api/user/register?UserId=${encodeURIComponent(matric)}&FirstName=${first}&LastName=${last}&Email=${encodeURIComponent(
      email
    )}&UserType=Student&IsAdmin=false&Department=${Department}&School=${School}&Password=${password}`;

    const response = await authorizedFetch(url, {
      method: "POST",
    });
    const data = await response.json();
    if (!response.ok) {
      errorMessage.style.display = "block";
      errorMessage.innerText = data.message;
      setTimeout(() => {
        errorMessage.style.display = "none";
      }, 3000);
    } else {
      alert("Registration successful");
      sessionStorage.setItem("email", email);
      window.location.href = "Verifyemail.html";
    }
  } catch (error) {
    console.error(error);
  }
}

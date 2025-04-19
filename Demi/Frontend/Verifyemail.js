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
const email = document.getElementById("email");
email.textContent = localStorage.getItem("email") || "";
email.value = localStorage.getItem("email") || "";
const btn = document.getElementById("send-otp");
email.value = localStorage.getItem("email") || "";
document.addEventListener("DOMContentLoaded", function () {
  if (btn) {
    btn.addEventListener("click", Verifyemail);
  }
});
async function Verifyemail() {
  document.getElementById("otp-message").innerText = "Sending OTP...";
  const url = `https://localhost:44354/api/user/send-otp?email=${email.value}`;
  const response = await authorizedFetch(url, {
    method: "POST",
  });
  const data = await response.json();
  if (response.status === 200) {
    document.getElementById("otp-message").innerText = data.message;
    
    setTimeout(() => {
      alert("Email Sent");
      console.log(data);
      localStorage.setItem("email", email.value);
      window.location.href = "VerifyOtp.html";
    }, 2000); // Waits for 2 seconds before executing
  } else {
    document.getElementById("otp-message").innerText = data.message;
  }
}

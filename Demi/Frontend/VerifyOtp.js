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
const val_btn = document.getElementById("validate-otp");
const val = document.getElementById("OTP").value.trim();
document.addEventListener("DOMContentLoaded", function () {
  const email = localStorage.getItem("email");
  document.getElementById("email").innerHTML = email;
  if (val_btn) {
    val_btn.addEventListener("click", ValidateOtp);
  }
});
async function ValidateOtp() {
  const val = document.getElementById("OTP").value.trim();
  console.log(val);
  document.getElementById("otp-message").innerText = "Validating OTP...";
  const email = localStorage.getItem("email");
  const url = `https://localhost:44354/api/user/verify-otp?Email=${email}&Otp=${val}`;
  const repsonse = await authorizedFetch(url, {
    method: "POST",
  });
  const data = await repsonse.json();
  if (repsonse.status === 200) {
    document.getElementById("otp-message").innerText = data.message;
    setTimeout(() => {
      alert("Email verified successfully");
      console.log(data);
      localStorage.setItem("email", email);
      if (!sessionStorage.getItem("isLoggedIn")) {
        window.location.href = "Dashboard.html";
      } else {
        window.location.href = "StudentLogin.html";
      }
    }, 2000); // Waits for 2 seconds before executing
  } else {
    document.getElementById("otp-message").innerText = data.message;
  }
}

let sendCount = 0;
let countdownInterval;

async function sendOtp() {
  const email = document.getElementById("email");
  const sendBtn = document.getElementById("send-otp-btn");
  const message = document.getElementById("otp-message");
  const timer = document.getElementById("otp-timer");

  message.innerText = "Sending OTP...";

  const url = `https://localhost:44354/api/user/send-otp?email=${email.innerHTML}`;
  const response = await authorizedFetch(url, {
    method: "POST",
  });
  const data = await response.json();

  if (response.status === 200) {
    message.innerText = data.message;

    
    setTimeout(() => {
      message.innerText = "";
    }, 1000); // Waits for 2 seconds before executing

    sendCount++;

    // Set wait time: 2 mins first, 5 mins second
    const waitTime = sendCount === 1 ? 2 * 60 : 5 * 60;

    // Start countdown
    let seconds = waitTime;
    sendBtn.disabled = true;

    clearInterval(countdownInterval);
    countdownInterval = setInterval(() => {
      const mins = Math.floor(seconds / 60);
      const secs = seconds % 60;
      timer.innerText = `Please wait ${mins}:${secs
        .toString()
        .padStart(2, "0")} to resend OTP`;
      seconds--;

      if (seconds < 0) {
        clearInterval(countdownInterval);
        if (sendCount < 2) {
          sendBtn.disabled = false;
          sendBtn.innerText = "Resend OTP";
          timer.innerText = "";
        } else {
          sendBtn.disabled = true;
          sendBtn.innerText = "Send OTP";
          timer.innerText = "Try again later";
        }
      }
    }, 1000);
  } else {
    message.innerText = data.message;
  }
}

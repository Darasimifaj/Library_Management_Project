const serialNumber = localStorage.getItem("selectedSerial");
const userId = sessionStorage.getItem("userId");
const goback = localStorage.getItem("savedLocation");
const returncode = localStorage.getItem("returncode");
const returnUrl = `https://localhost:44354/api/Books/return/${serialNumber}/${encodeURIComponent(
  userId
)}/${returncode}`;
console.log(returncode);
document.getElementById("returncode").innerHTML = returncode;
const continuebutton = document.getElementById("Continue");
// continuebutton.addEventListener("click", async () => {
//   try {
//   if (serialNumber) {
//     console.log(matricNo);
//     console.log("Serial Number:", serialNumber);
//     // document.getElementById("serialDisplay").textContent = serialNumber; // Display it
//   } else {
//     console.log("No serial number found.");
//   }
//   try {
//     const response = await fetch(borrowUrl, {
//       method: "POST",
//       headers: {
//         "Content-Type": "application/json",
//         accept: "*/*",
//       },
//     });

//     if (!response.ok) {
//       throw new Error(`HTTP error! Status: ${response.status}`);
//     }
//     const responseData = await response.json();
//     console.log(responseData);
//   } catch (error) {
//     console.error("Eroor fetching Book");
//   }
// });
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

continuebutton.addEventListener("click", async () => {
  try {
    console.log(returncode);
    const response = await authorizedFetch(returnUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        accept: "*/*",
      },
    });

    if (!response.ok) {
      window.location.href = "BadReturn.html";
      throw new Error(`HTTP error! Status: ${response.status}`);
    }

    const responseData = await response.json();
    console.log(responseData);
    window.location.href = "SuccessfulReturn.html";
  } catch (error) {
    console.error("Error Returning books:", error);
  }
});

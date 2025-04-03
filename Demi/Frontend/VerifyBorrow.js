const serialNumber = localStorage.getItem("selectedSerial");
const userId = sessionStorage.getItem("userId");
const goback = localStorage.getItem("savedLocation");
const borrowcode = localStorage.getItem("borrowcode");
console.log(userId);
const borrowUrl = `https://localhost:44354/api/Books/borrow/${serialNumber}/${encodeURIComponent(
  userId
)}/${borrowcode}`;
document.getElementById("borrowcode").innerHTML = borrowcode;
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

continuebutton.addEventListener("click", async () => {
  try {
    const response = await fetch(borrowUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        accept: "*/*",
      },
    });

    if (!response.ok) {
      window.location.href = "BadBorrow.html";
      throw new Error(`HTTP error! Status: ${response.status}`);
    }

    const responseData = await response.json();
    console.log(responseData);
    window.location.href = "SuccessfulBorrow.html";
  } catch (error) {
    console.error("Error Requesting books:", error);
  }
});

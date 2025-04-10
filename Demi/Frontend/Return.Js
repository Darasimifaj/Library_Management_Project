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
// const search = document.getElementById("search").value.trim();
// console.log(serialNumber);
const scannedSerialNumber = localStorage.getItem("scannedSerialNumber");

if (scannedSerialNumber) {
  // Set the value of the serial number input field
  document.getElementById("serialnumber").value = scannedSerialNumber;
} else {
  // If no serial number is found, alert the user or handle it accordingly
  document.getElementById("serialnumber").value = "No serial number found!";
}
const ErrorMessage = document.getElementById("error_message");
function geterror() {
  ErrorMessage.style.display = "block";
  setTimeout(() => {
    ErrorMessage.innerText = "";
    ErrorMessage.style.display = "none";
  }, 2000);
}

const Enter = document.getElementById("Enter");
Enter.addEventListener("click", async () => {
  try {
    const matricNo = sessionStorage.getItem("matricNo");
    const serialNumber = document.getElementById("serialnumber").value.trim();
    const borrowHistoryUrl = `https://localhost:44354/api/Books/borrow-history?matricNumber=${encodeURIComponent(
      matricNo
    )}&serialnumber=${serialNumber}&IsReturned=false`;

    // Check if the book has been borrowed
    const borrowResponse = await authorizedFetch(borrowHistoryUrl);
    const borrowData = await borrowResponse.json();
    const isBorrowed = borrowData.totalNotReturned > 0;
    console.log(isBorrowed);
    const bookurl = `https://localhost:44354/api/Books/${serialNumber}`;
    const response = await authorizedFetch(bookurl);
    const bookresponse = await response.json();

    if (!serialNumber) {
      console.error("Input a serial number");
      ErrorMessage.innerHTML = "Input Serial Number";
      geterror();
    } else if (isBorrowed) {
      if (response.status === 200) {
        localStorage.setItem("selectedSerial", serialNumber);
        localStorage.setItem("savedLocation", window.location.href);
        window.location.href = "RequestReturn.html";
      } else {
        ErrorMessage.innerText = "BOOK NOT FOUND";
        geterror();
      }
    } else if (!isBorrowed && bookresponse.name) {
      console.log(bookurl);
      ErrorMessage.innerText = "No borrow record";
      geterror();
      console.error("Book is not borrowed");
    } else if (response.status === 400 && !response.name) {
      ErrorMessage.innerText = "BOOK NOT FOUND";
      geterror();
      console.error("BOOK NOT FOUND");
    } else {
      ErrorMessage.innerText = "BOOK NOT FOUND";
      geterror();
      console.error("BOOK NOT FOUND");
    }
  } catch (error) {
    ErrorMessage.innerHTML = "Book not Found";
    ErrorMessage.style.display = "block";
    geterror();
    console.error("Error Getting book", error);
  }
});

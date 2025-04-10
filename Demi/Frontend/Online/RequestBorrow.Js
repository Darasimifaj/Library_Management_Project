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

const duration = document.querySelector(".Duration");

const serialNumber = localStorage.getItem("selectedSerial");
const bookurl = `https://localhost:44354/api/Books/${serialNumber}`;
const userId = sessionStorage.getItem("userId");
const requestborrowurl = `https://localhost:44354/api/Books/borrowOnline/${serialNumber}/${encodeURIComponent(
  userId
)}`;
const borrowedbooksUrl = `https://localhost:44354/api/user/Users?UserId=${encodeURIComponent(
  userId
)}`;
document.addEventListener("DOMContentLoaded", async function () {
  if (serialNumber) {
    console.log("Serial Number:", serialNumber);
    // document.getElementById("serialDisplay").textContent = serialNumber; // Display it
  } else {
    console.log("No serial number found.");
  }
  try {
    const response = await authorizedFetch(bookurl);
    const book = await response.json();
    const imageUrl = `https://localhost:44354/api/Books/image/${serialNumber}`;
    const imgresponse = await authorizedFetch(imageUrl);
    const blob = await imgresponse.blob();
    const objectURL = URL.createObjectURL(blob);
    console.log(book.name);
    document.getElementById("BName").innerText = truncateText(book.name, 4);
    document.getElementById("Image").src = objectURL;
    document.getElementById("Author").innerText = truncateText(book.author, 4);
  } catch (error) {
    console.error("Eroor fetching Book");
  }
});
const continuebutton = document.getElementById("Continue");
continuebutton.addEventListener("click", async () => {
  try {
    const borrowedResponse = await authorizedFetch(borrowedbooksUrl);
    const borrowedbooks = await borrowedResponse.json();
    const borrowlimit = borrowedbooks.borrowlimit;
    const borrowed = borrowedbooks.currentlyBorrowed;
    console.log(borrowlimit);
    console.log(borrowed);
    if (borrowed >= borrowlimit) {
      document.getElementById("Error_message").style.display = "block";
      setTimeout(() => {
        document.getElementById("Error_message").style.display = "none";
      }, 2000);
    }
    const response = await authorizedFetch(requestborrowurl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        accept: "*/*",
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }

    const responseData = await response.json();
    console.log("Borrow request successful:", responseData);

    window.location.href = "SuccessfulBorrow.html";
  } catch (error) {
    console.error("Error Requesting books:", error);
  }
});
function truncateText(text, wordCount) {
  let words = text.split(" ");
  return words.length > wordCount
    ? words.slice(0, wordCount).join(" ") + "..."
    : text;
}

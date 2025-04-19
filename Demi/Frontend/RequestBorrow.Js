//
const serialNumber = localStorage.getItem("selectedSerial");
const bookurl = `https://localhost:44354/api/Books/${serialNumber}`;
const userId = sessionStorage.getItem("userId");
const requestborrowurl = `https://localhost:44354/api/Books/request-borrow/${serialNumber}/${encodeURIComponent(
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
    const response = await fetch(bookurl);
    const book = await response.json();
    const imageUrl = `https://localhost:44354/api/Books/image/${serialNumber}`;
    console.log(book.name);
    document.getElementById("BName").innerText = truncateText(book.name, 4);
    document.getElementById("Image").src = imageUrl;
    document.getElementById("Author").innerText = truncateText(book.author, 4);
  } catch (error) {
    console.error("Eroor fetching Book");
  }
});
const continuebutton = document.getElementById("Continue");
continuebutton.addEventListener("click", async () => {
  try {
    const borrowedResponse = await fetch(borrowedbooksUrl);
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
    const response = await fetch(requestborrowurl, {
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

    const borrowcode = responseData.borrowCode;
    console.log(borrowcode);
    localStorage.setItem("borrowcode", borrowcode);

    window.location.href = "VerifyBorrow.html";
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

//
const serialNumber = localStorage.getItem("selectedSerial");
const bookurl = `https://localhost:44354/api/Books/${serialNumber}`;
const userId = sessionStorage.getItem("userId");
const requestreturnurl = `https://localhost:44354/api/Books/request-return/${serialNumber}/${encodeURIComponent(
  userId
)}`;
document.addEventListener("DOMContentLoaded", async function () {
  if (serialNumber) {
    console.log(userId);
    console.log("Serial Number:", serialNumber);
  } else {
    console.log("No serial number found.");
  }
  try {
    const response = await fetch(bookurl);
    const book = await response.json();
    const imageUrl = `https://localhost:44354/api/Books/image/${serialNumber}`;
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
    const borrowHistoryUrl = `https://localhost:44354/api/Books/borrow-history?UserId=${encodeURIComponent(
      userId
    )}&serialnumber=${serialNumber}&IsReturned=false`;

    // Check if the book has been borrowed
    const borrowResponse = await fetch(borrowHistoryUrl);
    const borrowData = await borrowResponse.json();
    const isBorrowed = borrowData.totalNotReturned > 0;
    if (!isBorrowed) {
      document.getElementById("Error_message").style.display = "block";
      setTimeout(() => {
        document.getElementById("Error_message").style.display = "none";
      }, 2000);
    }
    const response = await fetch(requestreturnurl, {
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
    console.log("Return request successful:", responseData);

    const returncode = responseData.returnCode;
    console.log(returncode);
    localStorage.setItem("returncode", returncode);

    window.location.href = "VerifyReturn.html";
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

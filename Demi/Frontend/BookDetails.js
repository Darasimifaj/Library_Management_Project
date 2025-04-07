function closeSidebar() {
  const sidebar = document.getElementById("SideH");
  if (sidebar) {
    sidebar.style.display = "none "; // Hide the sidebar
  }
}
function openSidebar() {
  const sidebar = document.getElementById("SideH");
  if (sidebar) {
    sidebar.style.display = "flex "; // Hide the sidebar
  }
}

function togglePopup(event) {
  event.stopPropagation(); // Prevents closing when clicking the profile pic
  const popup = document.getElementById("popup");
  const popup1 = document.getElementById("popup1");
  popup.style.display = popup.style.display === "flex" ? "none" : "flex";
  popup1.style.display = popup1.style.display === "flex" ? "none" : "flex";
}

// Close pop-up when clicking anywhere outside of it
document.addEventListener("click", function () {
  const popup = document.getElementById("popup");
  popup.style.display = "none";
  const popup1 = document.getElementById("popup1");
  popup1.style.display = "none";
});

document.addEventListener("DOMContentLoaded", function () {
  const closeButton2 = document.getElementById("close2");
  const closeButton = document.getElementById("close");
  const openButton = document.getElementById("open");

  if (closeButton) {
    closeButton.addEventListener("click", closeSidebar);
  }

  if (closeButton2) {
    closeButton2.addEventListener("click", closeSidebar);
  }
  if (openButton) {
    openButton.addEventListener("click", openSidebar);
  }
});
//
const serialNumber = localStorage.getItem("selectedSerial");
const bookurl = `https://localhost:44354/api/Books/${serialNumber}`;
const userId = sessionStorage.getItem("userId");
const requestborrowurl = `https://localhost:44354/api/Books/request-borrow/${serialNumber}/${encodeURIComponent(
  userId
)}`;
const borrowHistoryUrl = `https://localhost:44354/api/Books/borrow-history?UserId=${encodeURIComponent(
  userId
)}&serialnumber=${serialNumber}&IsReturned=false`;

document.addEventListener("DOMContentLoaded", async function () {
  const borrowResponse = await fetch(borrowHistoryUrl);
  const borrowData = await borrowResponse.json();
  const isBorrowed = borrowData.totalNotReturned > 0;
  if (serialNumber) {
    console.log(userId);
    console.log("Serial Number:", serialNumber);
    // document.getElementById("serialDisplay").textContent = serialNumber; // Display it
  } else {
    console.log("No serial number found.");
  }
  try {
    const response = await fetch(bookurl);
    const book = await response.json();
    const imageUrl = `https://localhost:44354/api/Books/image/${serialNumber}`;
    console.log(book);
    document.getElementById("BName").innerText = truncateText(book.name, 7);
    document.getElementById("Image").src = imageUrl;
    document.getElementById("Author").innerText = truncateText(book.name, 7);
    document.getElementById("Description").innerHTML = book.description;
    document.getElementById("Year").innerHTML = "Published: " + book.year;
    console.log(book.quantity);
    const Bbutton = document.getElementById("borrow");
    console.log(Bbutton);
    if (book.quantity === 0 && !isBorrowed) {
      Bbutton.className = "unavailable";
      Bbutton.innerHTML = "Unavailable";
    } else if (isBorrowed) {
      Bbutton.className = "borrowed";
      Bbutton.innerHTML = "Borrowed";
    } else if (!isBorrowed && book.quantity >= 1) {
      Bbutton.className = "borrow";
      Bbutton.innerHTML = "Borrow";
    }

    document.querySelectorAll(".borrow").forEach((button) => {
      button.addEventListener("click", (event) => {
        console.log("Clicked!"); // Save serial number
        localStorage.setItem("savedLocation", window.location.href);
        window.location.href = "RequestBorrow.html"; // Redirect to next page
      });
    });
  } catch (error) {
    console.error("Eroor fetching Book");
  }
});
function truncateText(text, wordCount) {
  let words = text.split(" ");
  return words.length > wordCount
    ? words.slice(0, wordCount).join(" ") + "..."
    : text;
}
function applyDarkMode() {
  if (localStorage.getItem("darkMode") === "enabled") {
    document.body.classList.add("dark-mode");
  }
}


applyDarkMode();

document.getElementById("moon").onclick = function () {
  document.body.classList.add("dark-mode");
  localStorage.setItem("darkMode", "enabled"); // Store the preference
};

document.getElementById("sun").onclick = function () {
  document.body.classList.remove("dark-mode");
  localStorage.setItem("darkMode", "disabled"); // Store the preference
};
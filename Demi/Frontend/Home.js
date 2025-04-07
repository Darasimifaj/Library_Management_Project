function openSidebar() {
  const main = document.getElementById("Main");
  const sidebar = document.getElementById("SideH");
  if (sidebar) {
    sidebar.style.display = "flex "; // Hide the sidebar
    main.style.display = "none";
  }
}
function closeSidebar() {
  const main = document.getElementById("Main");
  const sidebar = document.getElementById("SideH");
  if (sidebar) {
    sidebar.style.display = "none "; // Hide the sidebar
    main.style.display = "flex";
  }
}
document.addEventListener("DOMContentLoaded", function () {
  const closeButton = document.getElementById("close");
  const openButton = document.getElementById("open");

  if (closeButton) {
    closeButton.addEventListener("click", closeSidebar);
  }

  if (openButton) {
    openButton.addEventListener("click", openSidebar);
  }
});

// Function to apply dark mode based on localStorage
function applyDarkMode() {
  if (localStorage.getItem("darkMode") === "enabled") {
    document.body.classList.add("dark-mode");
  }
}

// Apply dark mode immediately on page load to avoid flickering
applyDarkMode();

// Event listener for toggling to dark mode
document.getElementById("moon").onclick = function () {
  document.body.classList.add("dark-mode");
  localStorage.setItem("darkMode", "enabled"); // Store the preference
};


document.getElementById("sun").onclick = function () {
  document.body.classList.remove("dark-mode");
  localStorage.setItem("darkMode", "disabled"); // Store the preference
};

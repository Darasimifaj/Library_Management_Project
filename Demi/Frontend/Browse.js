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
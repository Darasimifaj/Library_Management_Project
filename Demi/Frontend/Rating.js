document.addEventListener("DOMContentLoaded", () => {
  const userId = sessionStorage.getItem("userId");
  fetch(`https://localhost:44354/api/user/${encodeURIComponent(userId)}`)
    .then((response1) => response1.json())
    .then((User) => {
      document.querySelector(".rCategory").innerHTML = User.rCategory;
      document.querySelector(".rating").innerHTML =
        parseInt(User.rating) + "/10";
      document.querySelector(".borrow").innerHTML =
        parseInt(User.borrowlimit) + "/10";
    });
  fetch(
    `https://localhost:44354/api/Books/borrow-history?UserId=${encodeURIComponent(
      userId
    )}`
  )
    .then((response) => response.json())
    .then((data) => {
      document.querySelector(".TLR").innerHTML = data.totalLateReturn;
      document.querySelector(".TER").innerHTML = data.totalEarlyReturn;
    })
    .catch((error) => console.error("Error fetching data:", error));
});

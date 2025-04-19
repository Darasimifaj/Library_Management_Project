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
    .then((responsed) => responsed.json())
    .then((datar) => {
      console.log(userId);
      document.querySelector(".TLR").innerHTML = datar.totalLateReturn || 0;
      document.querySelector(".TER").innerHTML = datar.totalEarlyReturn || 0;
    })
    .catch((error) => console.error("Error fetching data:", error));
});

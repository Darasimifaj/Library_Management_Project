async function updateBook() {
  const serialNumber = document.getElementById("serialNumber").value.trim();
  const field = document.getElementById("field").value;
  const newValue = document.getElementById("newValue").value.trim();
  const messageElement = document.getElementById("message");

  if (!serialNumber || !newValue) {
    messageElement.textContent = "Please fill all fields.";
    messageElement.style.color = "red";
    return;
  }

  const patchData = [
    {
      op: "replace",
      path: field,
      value: isNaN(newValue) ? newValue : Number(newValue),
    },
  ];

  try {
    const response = await fetch(
      `https://localhost:44354/api/books/${serialNumber}`,
      {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(patchData),
      }
    );

    const data = await response.json();

    if (response.ok) {
      messageElement.textContent = data.message || "Book updated successfully.";
      messageElement.style.color = "green";
    } else {
      messageElement.textContent = data.message || "An error occurred.";
      messageElement.style.color = "red";
    }
  } catch (error) {
    messageElement.textContent = "Failed to connect to the server.";
    messageElement.style.color = "red";
  }
}

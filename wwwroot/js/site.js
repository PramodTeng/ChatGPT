// Set focus on the input field when the page loads
document.addEventListener("DOMContentLoaded", function () {
  const userInput = document.getElementById("userInput");
  if (userInput) {
    userInput.focus();
  }

  // Add event listener for mobile devices
  if (window.innerWidth <= 768) {
    const chatContainer = document.getElementById("chatContainer");
    if (chatContainer) {
      userInput.addEventListener("focus", function () {
        // Scroll chat container to the bottom when mobile keyboard appears
        setTimeout(function () {
          chatContainer.scrollTop = chatContainer.scrollHeight;
        }, 300);
      });
    }
  }
});

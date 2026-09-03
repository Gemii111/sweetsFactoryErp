document.addEventListener("DOMContentLoaded", function () {
  const html = document.documentElement;
  const themeSwitch = document.getElementById("themeSwitch");
  const themeIcon = document.getElementById("themeIcon");
  // Load theme from localStorage
  const savedTheme = localStorage.getItem("theme") || "light";
  html.setAttribute("data-bs-theme", savedTheme);
  if (themeSwitch) themeSwitch.checked = savedTheme === "dark";
  if (themeIcon)
    themeIcon.className =
      savedTheme === "dark"
        ? "bi bi-brightness-high-fill"
        : "bi bi-moon-stars-fill";
  if (themeSwitch) {
    themeSwitch.addEventListener("change", function () {
      const theme = themeSwitch.checked ? "dark" : "light";
      html.setAttribute("data-bs-theme", theme);
      localStorage.setItem("theme", theme);
      if (themeIcon)
        themeIcon.className =
          theme === "dark"
            ? "bi bi-brightness-high-fill"
            : "bi bi-moon-stars-fill";
      // Update footer text if present
      const footerTheme = document.getElementById("footer-theme");
      if (footerTheme)
        footerTheme.textContent = theme === "dark" ? "Dark Mode" : "Light Mode";
    });
  }
});

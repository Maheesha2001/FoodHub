// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function() {
    const cartIcon = document.getElementById("cartIcon");
    if (cartIcon) {
        cartIcon.addEventListener("click", function() {
            const sidebar = document.getElementById("cartSidebar");
            if (sidebar) {
                // Toggle the sidebar
                if (sidebar.classList.contains("show")) {
                    sidebar.classList.remove("show");
                } else {
                    sidebar.classList.add("show");
                }
            }
        });
    }
});
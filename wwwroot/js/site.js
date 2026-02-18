// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {

    // ---------------------------
    // Back to Top Button Logic
    // ---------------------------
    const backToTopButton = document.getElementById('backToTop');
    const heroSection = document.querySelector('.hero-section');
    const navBar = document.querySelector('.navbar');

    if (backToTopButton && heroSection && navBar) {
        window.addEventListener('scroll', () => {
            const heroHeight = heroSection.offsetHeight;
            if (window.scrollY > heroHeight) {
                backToTopButton.classList.add('show');
            } else {
                backToTopButton.classList.remove('show');
            }
        });

        backToTopButton.addEventListener('click', () => {
            const navHeight = navBar.offsetHeight;
            const heroTop = heroSection.getBoundingClientRect().top + window.scrollY;
            window.scrollTo({
                top: heroTop - navHeight,  // offset by navbar height
                behavior: 'smooth'
            });
        });
    }

    // ---------------------------
    // Cart Sidebar Toggle Logic
    // ---------------------------
    const cartIcon = document.getElementById("cartIcon");
    const cartSidebar = document.getElementById("cartSidebar");

    if (cartIcon && cartSidebar) {
        cartIcon.addEventListener("click", () => {
            cartSidebar.classList.toggle("show");
        });
    }

});


// // Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// // for details on configuring this project to bundle and minify static web assets.

// // Write your JavaScript code.
// document.addEventListener("DOMContentLoaded", function() {
//     const cartIcon = document.getElementById("cartIcon");
//     if (cartIcon) {
//         cartIcon.addEventListener("click", function() {
//             const sidebar = document.getElementById("cartSidebar");
//             if (sidebar) {
//                 // Toggle the sidebar
//                 if (sidebar.classList.contains("show")) {
//                     sidebar.classList.remove("show");
//                 } else {
//                     sidebar.classList.add("show");
//                 }
//             }
//         });
//     }
// });
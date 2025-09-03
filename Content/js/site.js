document.addEventListener('DOMContentLoaded', () => {
    const backToTopButton = document.getElementById('backToTop');
    const heroSection = document.querySelector('.hero-section');
    const navBar = document.querySelector('.navbar');

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
});

document.addEventListener("DOMContentLoaded", () => {
    fetch("/api/specials")
        .then(response => response.json())
        .then(data => {
            const specialsContainer = document.getElementById("specials-container");
            specialsContainer.innerHTML = ""; // clear old

            data.forEach(item => {
                specialsContainer.innerHTML += `
                    <div class="col-md-4">
                        <div class="card shadow-sm border-0 h-100">
                            <img src="/Content/images/${item.imageName}" class="card-img-top" alt="${item.title}">
                            <div class="card-body text-center">
                                <h5 class="card-title">${item.title}</h5>
                                <p class="card-text">${item.description}</p>
                            </div>
                        </div>
                    </div>
                `;
            });
        })
        .catch(error => console.error("Error loading specials:", error));
});

document.addEventListener("DOMContentLoaded", () => {
    fetch("/api/specials")
        .then(response => response.json())
        .then(data => {
            const specialsContainer = document.getElementById("specials-container");
            specialsContainer.innerHTML = ""; // clear old

            data.forEach(item => {
                const discountText = item.discountType === "Percentage" 
                    ? `${item.discountValue}% OFF` 
                    : `$${item.discountValue} OFF`;

                specialsContainer.innerHTML += `
                    <div class="col-md-4 mb-4">
                        <div class="card h-100 shadow-sm border-0 hover-scale" style="transition: transform 0.3s;">
                            <div class="position-relative">
                                <img src="/uploads/specials/${item.imageName}" class="card-img-top" alt="${item.title}" style="height: 200px; object-fit: cover;" />
                                <span class="badge bg-danger position-absolute top-0 end-0 m-2">${discountText}</span>
                            </div>
                            <div class="card-body text-center">
                                <h5 class="card-title fw-bold">${item.title}</h5>
                                <p class="card-text text-muted">${item.description}</p>
                                <h6 class="text-success fw-bold">Final Price: $${item.finalPrice}</h6>
                                <h6 class="text-danger"><s>$${item.totalPrice}</s></h6>
                            </div>
                            <div class="card-footer text-center bg-white border-0">
                                <button class="btn btn-warning btn-sm w-75 add-special-btn" 
                                        data-id="${item.id}" 
                                        data-price="${item.finalPrice}">
                                    Order Now
                                </button>
                            </div>
                        </div>
                    </div>
                `;
            });

           document.getElementById("specials-container").addEventListener("click", async (e) => {
    if (!e.target.classList.contains("add-special-btn")) return;

    const btn = e.target;
    const specialId = btn.dataset.id;
    const quantity = 1; // default

    console.log("Clicked specialId:", specialId);

    try {
        const response = await fetch("/Cart/AddSpecialToCart", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value || ""
            },
            credentials: "include",  // include cookies for auth
            body: JSON.stringify({ specialId, quantity })
        });

        if (response.ok) {
            // const data = await response.json();
            // console.log("Special added:", data);
            // loadCart(); // your cart update function

                const data = await response.json();
    const item = data.cartItem;

    // Normalize values (important!)
    const id = item.productId.toString();
    const type = item.type.toString();

    // Check if item exists locally
    let existing = cart.find(i => i.id === id && i.type === type);

    if (existing) {
        // Use server quantity (NEVER overwrite with 1!)
        existing.quantity = Number(item.quantity);
    } else {
        // Add new item exactly as server defines
        cart.push({
            id: id,
            name: item.productName,
            type: type,
            price: Number(item.price),
            quantity: Number(item.quantity)
        });
    }

    saveCart();
    renderCart();
    updateCartBadge();
    openCartSidebar();

        } else if (response.status === 401) {
            // Not logged in
            window.location.href = "/Account/Login";
        } else {
            console.error("Failed to add special:", await response.text());
        }
    } catch (err) {
        console.error("Error adding special:", err);
    }
});


//             // Attach click event to all Order Now buttons
//             document.querySelectorAll(".add-special-btn").forEach(btn => {
//                btn.addEventListener("click", async () => {
//     const specialId = btn.dataset.id;
//     const price = parseFloat(btn.dataset.price);

//     try {
//       const response = await fetch("/Cart/AddSpecialToCart", {
//     method: "POST",
//     headers: {
//         "Content-Type": "application/json",
//         "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value || ""
//     },
//     body: JSON.stringify({ specialId, quantity: 1 }),
//     credentials: "include" // important to include cookies
// });

// if (response.redirected) {
//     console.warn("User is not logged in, redirecting to login page...");
//     window.location.href = response.url;
//     return;
// }

// const contentType = response.headers.get("content-type") || "";
// if (contentType.includes("application/json")) {
//     const data = await response.json();
//     console.log("Special added:", data);
//     loadCart();
// } else {
//     console.error("Unexpected response:", await response.text());
// }


//     } catch (err) {
//         console.error("Error adding special:", err);
//     }
//                 });

//             });
        })
        .catch(error => console.error("Error loading specials:", error));
});

// document.addEventListener("DOMContentLoaded", () => {
//     fetch("/api/specials")
//         .then(response => response.json())
//         .then(data => {
//             const specialsContainer = document.getElementById("specials-container");
//             specialsContainer.innerHTML = ""; // clear old

//             data.forEach(item => {
//                 const discountText = item.discountType === "Percentage"
//                     ? `${item.discountValue}% OFF`
//                     : `$${item.discountValue} OFF`;

//                 specialsContainer.innerHTML += `
//                     <div class="col-md-4 mb-4">
//                         <div class="card h-100 shadow-sm border-0 hover-scale" style="transition: transform 0.3s;">
//                             <div class="position-relative">
//                                 <img src="/uploads/specials/${item.imageName}" class="card-img-top" alt="${item.title}" style="height: 200px; object-fit: cover;" />
//                                 <span class="badge bg-danger position-absolute top-0 end-0 m-2">${discountText}</span>
//                             </div>
//                             <div class="card-body text-center">
//                                 <h5 class="card-title fw-bold">${item.title}</h5>
//                                 <p class="card-text text-muted">${item.description}</p>
//                                 <h6 class="text-success fw-bold">Final Price: $${item.finalPrice}</h6>
//                                 <h6 class="text-danger">
//                                     <s>$${item.totalPrice}</s>
//                                 </h6>
//                             </div>
//                             <div class="card-footer text-center bg-white border-0">
//                                 <a href="#" class="btn btn-primary btn-sm w-75">Order Now</a>
//                             </div>
//                         </div>
//                     </div>
//                 `;
//             });
//         })
//         .catch(error => console.error("Error loading specials:", error));
// });


// document.addEventListener("DOMContentLoaded", () => {
//     fetch("/api/specials")
//         .then(response => response.json())
//         .then(data => {
//             const specialsContainer = document.getElementById("specials-container");
//             specialsContainer.innerHTML = ""; // clear old

//             data.forEach(item => {
//                 // Determine discount display
//                 const discountText = item.discountType === "Percentage"
//                     ? `${item.discountValue}% off`
//                     : `$${item.discountValue} off`;

//                 specialsContainer.innerHTML += `
//                     <div class="col-md-4">
//                         <div class="card shadow-sm border-0 h-100">
//                             <img src="/uploads/specials/${item.imageName}" class="card-img-top" alt="${item.title}" />
//                             <div class="card-body text-center">
//                                 <h5 class="card-title">${item.title}</h5>
//                                 <p class="card-text">${item.description}</p>
//                                 <h6>Final Price: $${item.finalPrice}</h6>
//                                 <h6>
//                                     Original:
//                                     <span style="text-decoration: line-through; text-decoration-color: red;">
//                                         $${item.totalPrice}
//                                     </span>
//                                 </h6>
//                                 <h6>${discountText}</h6>
//                             </div>
//                         </div>
//                     </div>
//                 `;
//             });
//         })
//         .catch(error => console.error("Error loading specials:", error));
// });


// document.addEventListener("DOMContentLoaded", () => {
//     fetch("/api/specials")
//         .then(response => response.json())
//         .then(data => {
//             const specialsContainer = document.getElementById("specials-container");
//             specialsContainer.innerHTML = ""; // clear old

//             data.forEach(item => {
//                 specialsContainer.innerHTML += `
//                     <div class="col-md-4">
//                         <div class="card shadow-sm border-0 h-100">
//                              <img src="/uploads/specials/${item.imageName}" class="card-img-top" alt="${item.title}" />
//                             <div class="card-body text-center">
//                                 <h5 class="card-title">${item.title}</h5>
//                                 <p class="card-text">${item.description}</p>
//                                  <h6>Final Price: $${item.finalPrice}</h6>
//                                  <h6>
//                                     Original:
//                                     <span style="text-decoration: line-through; text-decoration-color: red;">
//                                         $${item.totalPrice}
//                                     </span>
//                                 </h6>
//                                 if(${item.discountType} == "Percentage")
//                                 {
//                                     <h6>${item.discountValue}% off </h6>
//                                 } else
//                                 {
//                                     <h6>$${item.discountValue} off </h6>
//                                 }
//                             </div>
//                         </div>
//                     </div>
//                 `;
//             });
//         })
//         .catch(error => console.error("Error loading specials:", error));
// });

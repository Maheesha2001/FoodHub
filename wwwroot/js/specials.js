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
                                <h6 class="text-danger">
                                    <s>$${item.totalPrice}</s>
                                </h6>
                            </div>
                            <div class="card-footer text-center bg-white border-0">
                                <a href="#" class="btn btn-primary btn-sm w-75">Order Now</a>
                            </div>
                        </div>
                    </div>
                `;
            });
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

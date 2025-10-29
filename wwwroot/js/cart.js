let cart = [];

// ---------------------- CART STORAGE & CLEANUP ----------------------
function cleanCart() {
    cart = cart.filter(item => item.name && item.type);
    saveCart();
}

function saveCart() {
    localStorage.setItem("cart", JSON.stringify(cart));
}

// ---------------------- LOAD CART ----------------------
async function loadCart() {
    try {
        // Fetch server cart first
        const res = await fetch('/Cart/GetCart');
        if (res.ok) {
            const data = await res.json();
            cart = data.items || [];
            saveCart(); // optional: keep localStorage in sync
        } else {
            console.error('Failed to fetch server cart:', res.status);
            const savedCart = localStorage.getItem("cart");
            cart = savedCart ? JSON.parse(savedCart) : [];
        }
    } catch (err) {
        console.error('Error fetching cart:', err);
        const savedCart = localStorage.getItem("cart");
        cart = savedCart ? JSON.parse(savedCart) : [];
    }

    cleanCart();
    renderCart();
    updateCartBadge();
}

// ---------------------- ADD TO CART ----------------------
async function addToCart(id, name, type, price) {
    const existing = cart.find(item => item.id === id && item.type === type);
    if (existing) existing.quantity += 1;
    else cart.push({ id, name, type, price, quantity: 1 });

    saveCart();
    renderCart();
    updateCartBadge();
    openCartSidebar();

    // Sync with server
    try {
        await fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id, name, type, price, quantity: 1 })
        });
    } catch (err) {
        console.error('Failed to sync cart with server:', err);
    }
}

// ---------------------- REMOVE FROM CART ----------------------
async function removeFromCart(id, type) {
    try {
        // 1️⃣ Call server to remove from DB
        const res = await fetch(`/Cart/RemoveItem?productId=${id}&type=${encodeURIComponent(type)}`, { method: 'POST' });
        if (!res.ok) {
            console.error('Failed to remove item from server cart:', res.status);
            return;
        }

        // 2️⃣ Update local cart array
        cart = cart.filter(item => !(item.id === id && item.type === type));

        // 3️⃣ Update localStorage
        localStorage.setItem("cart", JSON.stringify(cart));

        // 4️⃣ Update UI
        renderCart();
        updateCartBadge();
    } catch (err) {
        console.error('Error removing cart item:', err);
    }
}


// ---------------------- CHANGE QUANTITY ----------------------
async function changeQuantity(id, type, delta) {
    const item = cart.find(i => i.id === id && i.type === type);
    if (!item) return;
    item.quantity += delta;
    if (item.quantity < 1) item.quantity = 1;

    saveCart();
    renderCart();
    updateCartBadge();

    try {
        await fetch(`/Cart/UpdateItemQuantity?productId=${id}&quantity=${item.quantity}`, { method: 'POST' });
    } catch (err) {
        console.error('Failed to update quantity on server:', err);
    }
}

function setQuantity(id, type, newQty) {
    const item = cart.find(i => i.id === id && i.type === type);
    if (!item) return;
    item.quantity = Math.max(1, parseInt(newQty) || 1);
    saveCart();
    renderCart();
    updateCartBadge();

    fetch(`/Cart/UpdateItemQuantity?productId=${id}&quantity=${item.quantity}`, { method: 'POST' })
        .catch(err => console.error('Failed to update quantity on server:', err));
}

// ---------------------- RENDER CART ----------------------
function renderCart() {
    const cartContainer = document.getElementById("cart-items");
    const totalContainer = document.getElementById("cart-total");
    const checkoutTable = document.getElementById("checkout-cart-body");
    const checkoutTotal = document.getElementById("checkout-cart-total");

    if (cartContainer) cartContainer.innerHTML = "";
    if (checkoutTable) checkoutTable.innerHTML = "";

    let total = 0;

    if (cart.length === 0) {
        if (cartContainer) {
            const li = document.createElement("li");
            li.className = "list-group-item text-center text-muted";
            li.textContent = "Your cart is empty.";
            cartContainer.appendChild(li);
        }
        if (checkoutTable) checkoutTable.innerHTML = "<tr><td colspan='6' class='text-center text-muted'>Your cart is empty.</td></tr>";
        if (totalContainer) totalContainer.textContent = "$0.00";
        if (checkoutTotal) checkoutTotal.textContent = "0.00";
        return;
    }

    cart.forEach(item => {
        total += item.price * item.quantity;

        // Sidebar
        if (cartContainer) {
            const li = document.createElement("li");
            li.className = "list-group-item d-flex justify-content-between align-items-center";
            li.innerHTML = `
                <div>
                    <strong>${item.name}</strong><br/>
                    <small>${item.type}</small><br/>
                    <small>${item.quantity} × $${item.price.toFixed(2)}</small>
                </div>
                <button class="btn btn-sm btn-outline-danger rounded-circle"
                        style="width:28px; height:28px; line-height:1;"
                        onclick="removeFromCart('${item.id}','${item.type}')">
                    <span aria-hidden="true">&times;</span>
                </button>`;
            cartContainer.appendChild(li);
        }

        // Checkout page
        if (checkoutTable) {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${item.name}</td>
                <td>${item.type}</td>
                <td>
                    <input type="number" class="form-control"
                           value="${item.quantity}" min="1"
                           onchange="setQuantity('${item.id}','${item.type}', this.value)">
                </td>
                <td class="unit-price">${item.price.toFixed(2)}</td>
                <td class="line-total">${(item.price * item.quantity).toFixed(2)}</td>
                <td>
                    <button class="btn btn-danger btn-sm"
                            onclick="removeFromCart('${item.id}','${item.type}')">
                        Remove
                    </button>
                </td>`;
            checkoutTable.appendChild(row);
        }
    });

    if (totalContainer) totalContainer.textContent = `$${total.toFixed(2)}`;
    if (checkoutTotal) checkoutTotal.textContent = total.toFixed(2);
}

// ---------------------- CART BADGE ----------------------
function updateCartBadge() {
    const cartCountEl = document.getElementById("cartCount");
    if (!cartCountEl) return;

    const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
    cartCountEl.textContent = totalItems;
    cartCountEl.style.display = totalItems > 0 ? "inline-block" : "none";
}

// ---------------------- SIDEBAR TOGGLE ----------------------
function openCartSidebar() {
    const sidebar = document.getElementById("cartSidebar");
    if (sidebar) sidebar.classList.add("show");
}

function closeCartSidebar() {
    const sidebar = document.getElementById("cartSidebar");
    if (sidebar) sidebar.classList.remove("show");
}

// // ---------------------- CHECKOUT BUTTON ----------------------
// function attachCheckoutListener() {
//     const checkoutBtn = document.getElementById('checkoutBtn');
//     if (!checkoutBtn) return;

//     checkoutBtn.addEventListener('click', async () => {
//         try {
//             const res = await fetch('/Cart/IsLoggedIn');
//             const text = await res.text();
//            // const data = await res.json();
//             const data = text ? JSON.parse(text) : { isLoggedIn: false };

//             if (data.isLoggedIn) {
//                 const cartItems = JSON.parse(localStorage.getItem("cart") || "[]");

//                 // Transform keys to match JsonPropertyName
//                 const payload = cartItems.map(i => ({
//                     id: i.id.toString(),
//                     name: i.name,
//                     type: i.type,
//                     price: i.price,
//                     quantity: i.quantity
//                 }));


//                 const response = await fetch("/Cart/SaveCheckoutCart", {
//                     method: "POST",
//                     headers: {
//                         "Content-Type": "application/json",
//                         "RequestVerificationToken":
//                             document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
//                     },
//                     body: JSON.stringify(payload)
//                 });

//                 if (response.ok) {
//                      localStorage.removeItem("cart");

//                     // ✅ Optionally reload server cart to sync UI
//                     await loadCart();
//                     window.location.href = "/Customer/Checkout/Index";
//                 } else {
//                     alert("Error saving your cart!");
//                 }
//             } else {
//                 await fetch("/Cart/StoreGuestCart", {
//                     method: "POST",
//                     headers: { "Content-Type": "application/json" },
//                     body: JSON.stringify(localStorage.getItem("cart") || "[]"),
//                     credentials: "same-origin"
//                 });
//                 window.location.href = "/Account/Login?returnUrl=/Customer/Checkout/Index";
//             }
//         } catch (err) {
//             console.error("Error during checkout:", err);
//         }
//     });
// }
// ---------------------- CHECKOUT BUTTON ----------------------
function attachCheckoutListener() {
    const checkoutBtn = document.getElementById('checkoutBtn');
    if (!checkoutBtn) return;

    checkoutBtn.addEventListener('click', async () => {
        try {
            const cartItems = JSON.parse(localStorage.getItem("cart") || "[]");
            const payload = cartItems.map(i => ({
                id: i.id.toString(),
                name: i.name,
                type: i.type,
                price: i.price,
                quantity: i.quantity
            }));

            // Save cart to server
            const response = await fetch("/Cart/SaveCheckoutCart", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken":
                        document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                localStorage.removeItem("cart");
                await loadCart(); // sync server cart
                window.location.href = "/Cart/Checkout"; // ✅ redirect to server-protected page
            } else {
                alert("Error saving your cart!");
            }
        } catch (err) {
            console.error("Error during checkout:", err);
        }
    });
}
// ---------------------- INITIALIZE ----------------------
document.addEventListener("DOMContentLoaded", function () {
    loadCart();
    attachCheckoutListener();
});

let cart = [];

// ---------------------- STORAGE HELPERS ----------------------
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
        const res = await fetch('/Cart/GetCart');
        if (res.ok) {
            const data = await res.json();
            const serverItems = data.items?.map(i => ({
                id: (i.id ?? i.productId ?? "").toString(),
                name: i.name ?? "",
                type: (i.type ?? "").toString(),
                price: Number(i.price ?? 0),
                quantity: Number(i.quantity ?? 1)
            })) ?? [];

            const local = JSON.parse(localStorage.getItem("cart") || "[]");

            // Merge — prefer local quantities when the same item exists locally
            for (const l of local) {
                const existing = serverItems.find(
                    s => s.id === l.id && s.type === l.type
                );
                if (existing) existing.quantity = l.quantity;
                else serverItems.push({
                    id: l.id.toString(),
                    name: l.name,
                    type: l.type.toString(),
                    price: Number(l.price || 0),
                    quantity: Number(l.quantity || 1)
                });
            }

            cart = serverItems;
            saveCart();
        } else {
            // fallback to local only
            cart = JSON.parse(localStorage.getItem("cart") || "[]");
        }
    } catch (err) {
        console.error("Error loading cart:", err);
        cart = JSON.parse(localStorage.getItem("cart") || "[]");
    }

    cleanCart();
    renderCart();
    updateCartBadge();
}

// ---------------------- ADD TO CART ----------------------
async function addToCart(id, name, type, price) {
    id = id.toString();
    type = type.toString();

    let item = cart.find(i => i.id === id && i.type === type);
    if (item) item.quantity += 1;
    else cart.push({ id, name, type, price: Number(price || 0), quantity: 1 });

    saveCart();
    renderCart();
    updateCartBadge();
    openCartSidebar();

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

// ---------------------- REMOVE ITEM ----------------------
async function removeFromCart(id, type) {
    id = id.toString();
    type = type.toString();

    try {
        await fetch(`/Cart/Remove?id=${id}&type=${encodeURIComponent(type)}`, { method: 'POST' });
    } catch (err) {
        console.error('Error removing from server cart:', err);
    }

    cart = cart.filter(i => !(i.id === id && i.type === type));
    saveCart();
    renderCart();
    updateCartBadge();
}

// ---------------------- CHANGE QUANTITY ----------------------
async function changeQuantity(id, type, delta) {
    id = id.toString();
    type = type.toString();

    let item = cart.find(i => i.id === id && i.type === type);
    if (!item) {
        console.warn("Item not found locally, reloading cart...");
        await loadCart();
        item = cart.find(i => i.id === id && i.type === type);
        if (!item) {
            console.error("Item still not found after reload.");
            return;
        }
    }

    item.quantity = Math.max(1, item.quantity + delta);
    console.log("Updating quantity:", id, type, item.quantity);

    saveCart();
    renderCart();
    updateCartBadge();

    try {
        await fetch(`/Cart/UpdateItemQuantity?productId=${id}&quantity=${item.quantity}&type=${encodeURIComponent(type)}`, {
            method: 'POST'
        });
    } catch (err) {
        console.error('Failed to update quantity on server:', err);
    }
}

// ---------------------- SET QUANTITY DIRECTLY ----------------------
function setQuantity(id, type, newQty) {
    id = id.toString();
    type = type.toString();

    const item = cart.find(i => i.id === id && i.type === type);
    if (!item) return;

    item.quantity = Math.max(1, parseInt(newQty) || 1);
    saveCart();
    renderCart();
    updateCartBadge();

    fetch(`/Cart/UpdateItemQuantity?productId=${id}&quantity=${item.quantity}&type=${encodeURIComponent(type)}`, {
        method: 'POST'
    }).catch(err => console.error('Failed to update quantity on server:', err));
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
        if (cartContainer)
            cartContainer.innerHTML = `<li class="list-group-item text-center text-muted">Your cart is empty.</li>`;
        if (checkoutTable)
            checkoutTable.innerHTML = `<tr><td colspan='6' class='text-center text-muted'>Your cart is empty.</td></tr>`;
        if (totalContainer) totalContainer.textContent = "$0.00";
        if (checkoutTotal) checkoutTotal.textContent = "0.00";
        return;
    }

    cart.forEach(item => {
        total += item.price * item.quantity;

        if (cartContainer) {
            const li = document.createElement("li");
            li.className = "list-group-item d-flex justify-content-between align-items-center";
            li.innerHTML = `
                <div>
                    <strong>${item.name}</strong><br/>
                    <small>${item.type}</small><br/>
                    <div class="d-flex align-items-center mt-1">
                        <button class="btn btn-sm btn-outline-secondary rounded-circle me-1 btn-qty-dec"
                                data-id="${item.id}" data-type="${item.type}">
                            <i class="bi bi-dash"></i>
                        </button>
                        <span class="mx-1 fw-bold">${item.quantity}</span>
                        <button class="btn btn-sm btn-outline-secondary rounded-circle ms-1 btn-qty-inc"
                                data-id="${item.id}" data-type="${item.type}">
                            <i class="bi bi-plus"></i>
                        </button>
                    </div>
                    <small class="text-muted">Unit: $${item.price.toFixed(2)}</small>
                </div>
                <button class="btn btn-sm btn-outline-danger rounded-circle"
                        style="width:28px; height:28px;"
                        onclick="removeFromCart('${item.id}','${item.type}')">
                    &times;
                </button>`;
            cartContainer.appendChild(li);
        }

        if (checkoutTable) {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${item.name}</td>
                <td>${item.type}</td>
                <td><input type="number" min="1" value="${item.quantity}" onchange="setQuantity('${item.id}','${item.type}', this.value)" class="form-control"/></td>
                <td>${item.price.toFixed(2)}</td>
                <td>${(item.price * item.quantity).toFixed(2)}</td>
                <td><button class="btn btn-danger btn-sm" onclick="removeFromCart('${item.id}','${item.type}')">Remove</button></td>`;
            checkoutTable.appendChild(row);
        }
    });

    if (totalContainer) totalContainer.textContent = `$${total.toFixed(2)}`;
    if (checkoutTotal) checkoutTotal.textContent = total.toFixed(2);
}

// ---------------------- CART BADGE ----------------------
function updateCartBadge() {
    const el = document.getElementById("cartCount");
    if (!el) return;
    const totalItems = cart.reduce((sum, i) => sum + i.quantity, 0);
    el.textContent = totalItems;
    el.style.display = totalItems > 0 ? "inline-block" : "none";
}

// ---------------------- SIDEBAR ----------------------
function openCartSidebar() {
    document.getElementById("cartSidebar")?.classList.add("show");
}
function closeCartSidebar() {
    document.getElementById("cartSidebar")?.classList.remove("show");
}

// ---------------------- QUANTITY BUTTONS ----------------------
document.addEventListener("click", e => {
    const inc = e.target.closest(".btn-qty-inc");
    const dec = e.target.closest(".btn-qty-dec");
    if (inc) changeQuantity(inc.dataset.id, inc.dataset.type, 1);
    if (dec) changeQuantity(dec.dataset.id, dec.dataset.type, -1);
});

// ---------------------- CHECKOUT BUTTON ----------------------
function attachCheckoutListener() {
    const checkoutBtn = document.getElementById('checkoutBtn');
    if (!checkoutBtn) return;

    checkoutBtn.addEventListener('click', async () => {
        try {
            // Use in-memory cart (already normalized)
            const payload = cart.map(i => ({
                id: i.id.toString(),
                name: i.name,
                type: i.type,
                price: i.price,
                quantity: i.quantity
            }));

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

            const response = await fetch("/Cart/SaveCheckoutCart", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                // Clear local cache and reload from server so DB becomes source of truth
                localStorage.removeItem("cart");
                await loadCart();
                // Redirect to checkout page (protected)
                window.location.href = "/Cart/Checkout";
            } else {
                const text = await response.text();
                console.error("Checkout save failed:", response.status, text);
                alert("Error saving your cart. Try again.");
            }
        } catch (err) {
            console.error("Error during checkout:", err);
            alert("Error during checkout. Check the console.");
        }
    });
}

// ---------------------- INITIALIZE ----------------------
document.addEventListener("DOMContentLoaded", () => {
    loadCart();
    attachCheckoutListener();
});

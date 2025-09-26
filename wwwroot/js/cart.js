let cart = [];

function cleanCart() {
    cart = cart.filter(item => item.name && item.type && !item.name.includes("undefined"));
    saveCart();
}

function loadCart() {
    const savedCart = localStorage.getItem("cart");
    cart = savedCart ? JSON.parse(savedCart) : [];

     // Remove any undefined items
    cleanCart();
    renderCart();
     updateCartBadge(); 
}

function saveCart() {
    localStorage.setItem("cart", JSON.stringify(cart));
}

function addToCart(id, name, type, price) {
    const existing = cart.find(item => item.id === id && item.type === type);
    if (existing) existing.quantity += 1;
    else cart.push({ id, name, type, price, quantity: 1 });

    saveCart();
    renderCart();
    updateCartBadge(); 
    openCartSidebar();
}

function removeFromCart(id, type) {
    cart = cart.filter(item => !(item.id === id && item.type === type));
    saveCart();
    renderCart();
     updateCartBadge(); 
}

function changeQuantity(id, type, delta) {
    const item = cart.find(i => i.id === id && i.type === type);
    if (!item) return;
    item.quantity += delta;
    if (item.quantity < 1) item.quantity = 1;
    saveCart();
    renderCart();
     updateCartBadge(); 
}

function renderCart() {
    const cartContainer = document.getElementById("cart-items");
    const totalContainer = document.getElementById("cart-total");
    if (!cartContainer) return;

    cartContainer.innerHTML = "";
    let total = 0;

      if (cart.length === 0) {
        // Display empty message
        const li = document.createElement("li");
        li.className = "list-group-item text-center text-muted";
        li.textContent = "Your cart is empty.";
        cartContainer.appendChild(li);
        totalContainer.textContent = "$0.00";
        return;
    }

    cart.forEach(item => {
        const li = document.createElement("li");
        li.className = "list-group-item d-flex justify-content-between align-items-center";

        li.innerHTML = `
            <div>
                <strong>${item.name}</strong> <br />
                <small>${item.type}</small>
            </div>
            <div class="d-flex align-items-center gap-2">
                <button class="btn btn-sm btn-outline-secondary" onclick="changeQuantity('${item.id}','${item.type}',-1)">-</button>
                <span>${item.quantity}</span>
                <button class="btn btn-sm btn-outline-secondary" onclick="changeQuantity('${item.id}','${item.type}',1)">+</button>
                <span class="ms-2">$${(item.price * item.quantity).toFixed(2)}</span>
                <button class="btn btn-sm btn-danger ms-2" onclick="removeFromCart('${item.id}','${item.type}')">&times;</button>
            </div>
        `;

        cartContainer.appendChild(li);
        total += item.price * item.quantity;
    });

    totalContainer.textContent = `$${total.toFixed(2)}`;
}

function openCartSidebar() {
    const sidebar = document.getElementById("cartSidebar");
    if (sidebar) sidebar.classList.add("show");
}

function closeCartSidebar() {
    const sidebar = document.getElementById("cartSidebar");
    if (sidebar) sidebar.classList.remove("show");
}

function updateCartBadge() {
    const cartCountEl = document.getElementById("cartCount");
    if (!cartCountEl) return;

    const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
    cartCountEl.textContent = totalItems;

    // Hide badge if cart is empty
    cartCountEl.style.display = totalItems > 0 ? "inline-block" : "none";
}


document.addEventListener("DOMContentLoaded", loadCart);

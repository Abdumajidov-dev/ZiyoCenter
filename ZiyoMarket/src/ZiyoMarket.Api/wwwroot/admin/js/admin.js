// Configuration
const API_BASE_URL = window.location.origin + '/api';
let currentPage = 1;
let currentPageSize = 20;
let currentPaymentId = null;
let currentAction = null;

// Check authentication on page load
document.addEventListener('DOMContentLoaded', function() {
    checkAuth();
    loadStatistics();
    loadPendingPayments();

    // Tab change events
    document.getElementById('all-tab').addEventListener('shown.bs.tab', function() {
        loadAllPayments();
    });
});

// Authentication
function checkAuth() {
    const token = localStorage.getItem('admin_token');
    const user = localStorage.getItem('admin_user');

    if (!token || !user) {
        window.location.href = 'login.html';
        return;
    }

    const userData = JSON.parse(user);
    document.getElementById('userName').textContent = userData.user_type || 'Admin';
}

function logout() {
    if (confirm('Chiqishni xohlaysizmi?')) {
        localStorage.removeItem('admin_token');
        localStorage.removeItem('admin_user');
        window.location.href = 'login.html';
    }
}

function getAuthHeaders() {
    const token = localStorage.getItem('admin_token');
    return {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };
}

// API Calls
async function apiCall(endpoint, options = {}) {
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options,
            headers: {
                ...getAuthHeaders(),
                ...options.headers
            }
        });

        if (response.status === 401) {
            localStorage.removeItem('admin_token');
            localStorage.removeItem('admin_user');
            window.location.href = 'login.html';
            return null;
        }

        const result = await response.json();
        return result;
    } catch (error) {
        console.error('API Call Error:', error);
        showToast('error', 'Serverga ulanishda xatolik: ' + error.message);
        return null;
    }
}

// Load Statistics
async function loadStatistics() {
    const result = await apiCall('/payment-proof/stats');

    if (result && result.status && result.data) {
        const stats = result.data;

        document.getElementById('pendingCount').textContent = stats.pending_count || 0;
        document.getElementById('pendingAmount').textContent = formatCurrency(stats.pending_amount || 0);

        document.getElementById('verifiedCount').textContent = stats.verified_count || 0;
        document.getElementById('verifiedAmount').textContent = formatCurrency(stats.verified_amount || 0);

        document.getElementById('rejectedCount').textContent = stats.rejected_count || 0;
        document.getElementById('rejectedAmount').textContent = formatCurrency(stats.rejected_amount || 0);

        document.getElementById('totalCount').textContent = stats.total_count || 0;
        document.getElementById('totalAmount').textContent = formatCurrency(stats.total_amount || 0);

        document.getElementById('pendingBadge').textContent = stats.pending_count || 0;
    }
}

// Load Pending Payments
async function loadPendingPayments() {
    const container = document.getElementById('pendingPayments');
    container.innerHTML = '<div class="loading"><div class="spinner-border text-primary" role="status"></div><p class="mt-3 text-muted">Yuklanmoqda...</p></div>';

    const result = await apiCall('/payment-proof/pending');

    if (result && result.status && result.data) {
        const proofs = result.data.proofs || [];

        if (proofs.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-check-circle text-success"></i>
                    <h5>Tasdiqlash kutilayotgan to'lovlar yo'q</h5>
                    <p class="text-muted">Barcha to'lovlar ko'rib chiqilgan</p>
                </div>
            `;
            return;
        }

        container.innerHTML = proofs.map(proof => createPaymentCard(proof, true)).join('');
    } else {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-exclamation-triangle text-warning"></i>
                <h5>Ma'lumotlarni yuklab bo'lmadi</h5>
                <p class="text-muted">Iltimos qaytadan urinib ko'ring</p>
                <button class="btn btn-primary" onclick="loadPendingPayments()">
                    <i class="fas fa-redo me-2"></i>Qayta yuklash
                </button>
            </div>
        `;
    }
}

// Load All Payments
async function loadAllPayments(page = 1) {
    currentPage = page;
    const container = document.getElementById('allPayments');
    container.innerHTML = '<div class="loading"><div class="spinner-border text-primary" role="status"></div><p class="mt-3 text-muted">Yuklanmoqda...</p></div>';

    const status = document.getElementById('filterStatus').value;
    const pageSize = document.getElementById('pageSize').value;
    currentPageSize = parseInt(pageSize);

    const params = new URLSearchParams({
        page: page,
        pageSize: pageSize
    });

    if (status) {
        params.append('status', status);
    }

    const result = await apiCall(`/payment-proof/all?${params.toString()}`);

    if (result && result.status && result.data) {
        const proofs = result.data.proofs || [];
        const pagination = result.data.pagination || {};

        if (proofs.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-inbox"></i>
                    <h5>To'lovlar topilmadi</h5>
                    <p class="text-muted">Hech qanday to'lov mavjud emas</p>
                </div>
            `;
            document.getElementById('paginationNav').style.display = 'none';
            return;
        }

        container.innerHTML = proofs.map(proof => createPaymentCard(proof, proof.status === 3)).join('');

        // Update pagination
        if (pagination.total_pages > 1) {
            updatePagination(pagination);
            document.getElementById('paginationNav').style.display = 'block';
        } else {
            document.getElementById('paginationNav').style.display = 'none';
        }
    } else {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-exclamation-triangle text-warning"></i>
                <h5>Ma'lumotlarni yuklab bo'lmadi</h5>
                <p class="text-muted">Iltimos qaytadan urinib ko'ring</p>
                <button class="btn btn-primary" onclick="loadAllPayments()">
                    <i class="fas fa-redo me-2"></i>Qayta yuklash
                </button>
            </div>
        `;
    }
}

// Create Payment Card HTML
function createPaymentCard(proof, showActions = true) {
    const statusBadge = getStatusBadge(proof.status);
    const imageUrl = proof.proof_image_url ? `${window.location.origin}/${proof.proof_image_url}` : null;

    return `
        <div class="payment-card p-3">
            <div class="row align-items-center">
                <div class="col-md-2">
                    ${imageUrl ? `
                        <img src="${imageUrl}" class="payment-image" onclick="showImage('${imageUrl}')" alt="Payment Proof">
                    ` : `
                        <div class="payment-image d-flex align-items-center justify-content-center bg-light">
                            <i class="fas fa-file-invoice fa-2x text-muted"></i>
                        </div>
                    `}
                </div>
                <div class="col-md-3">
                    <h6 class="mb-1">
                        <i class="fas fa-receipt me-2"></i>${proof.order_number}
                    </h6>
                    <p class="mb-1 text-muted small">
                        <i class="fas fa-user me-2"></i>${proof.customer_name}
                    </p>
                    <p class="mb-0 text-muted small">
                        <i class="fas fa-phone me-2"></i>${proof.customer_phone}
                    </p>
                </div>
                <div class="col-md-2">
                    <div class="mb-1">
                        <strong class="text-primary">${formatCurrency(proof.amount)}</strong>
                    </div>
                    ${proof.sender_card_number ? `
                        <small class="text-muted">
                            <i class="fas fa-credit-card me-1"></i>${proof.sender_card_number}
                        </small>
                    ` : ''}
                </div>
                <div class="col-md-2">
                    ${statusBadge}
                    <div class="mt-2">
                        <small class="text-muted">
                            <i class="fas fa-clock me-1"></i>${formatDate(proof.created_at)}
                        </small>
                    </div>
                </div>
                <div class="col-md-3 text-end">
                    ${showActions ? `
                        <button class="btn btn-success btn-sm btn-action me-2" onclick="openActionModal(${proof.id}, 'verify')">
                            <i class="fas fa-check me-1"></i>Tasdiqlash
                        </button>
                        <button class="btn btn-danger btn-sm btn-action" onclick="openActionModal(${proof.id}, 'reject')">
                            <i class="fas fa-times me-1"></i>Rad etish
                        </button>
                    ` : ''}
                    ${proof.admin_notes ? `
                        <div class="mt-2">
                            <small class="text-muted">
                                <i class="fas fa-comment me-1"></i>${proof.admin_notes}
                            </small>
                        </div>
                    ` : ''}
                </div>
            </div>
            ${proof.customer_notes ? `
                <div class="row mt-2">
                    <div class="col-12">
                        <div class="alert alert-light mb-0 py-2">
                            <small>
                                <i class="fas fa-comment-dots me-2"></i>
                                <strong>Mijoz izohi:</strong> ${proof.customer_notes}
                            </small>
                        </div>
                    </div>
                </div>
            ` : ''}
        </div>
    `;
}

// Show Image Modal
function showImage(imageUrl) {
    document.getElementById('modalImage').src = imageUrl;
    const modal = new bootstrap.Modal(document.getElementById('imageModal'));
    modal.show();
}

// Open Action Modal (Verify/Reject)
function openActionModal(paymentId, action) {
    currentPaymentId = paymentId;
    currentAction = action;

    const modal = new bootstrap.Modal(document.getElementById('actionModal'));
    const title = document.getElementById('actionModalTitle');
    const btn = document.getElementById('actionModalBtn');
    const notes = document.getElementById('adminNotes');

    notes.value = '';

    if (action === 'verify') {
        title.textContent = 'To\'lovni tasdiqlash';
        btn.textContent = 'Tasdiqlash';
        btn.className = 'btn btn-success';
        notes.placeholder = 'Tasdiqlash izohi (ixtiyoriy)...';
    } else {
        title.textContent = 'To\'lovni rad etish';
        btn.textContent = 'Rad etish';
        btn.className = 'btn btn-danger';
        notes.placeholder = 'Rad etish sababi (majburiy)...';
    }

    modal.show();
}

// Confirm Action
async function confirmAction() {
    const notes = document.getElementById('adminNotes').value.trim();

    if (currentAction === 'reject' && !notes) {
        alert('Rad etish sababi kiritilishi shart!');
        return;
    }

    const btn = document.getElementById('actionModalBtn');
    const originalText = btn.textContent;
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Kutilmoqda...';

    const endpoint = currentAction === 'verify'
        ? `/payment-proof/verify/${currentPaymentId}`
        : `/payment-proof/reject/${currentPaymentId}`;

    const result = await apiCall(endpoint, {
        method: 'POST',
        body: JSON.stringify({ admin_notes: notes })
    });

    btn.disabled = false;
    btn.textContent = originalText;

    if (result && result.status) {
        showToast('success', result.message);

        // Close modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('actionModal'));
        modal.hide();

        // Reload data
        await loadStatistics();
        await loadPendingPayments();

        // Reload all payments if that tab is active
        if (document.getElementById('all-tab').classList.contains('active')) {
            await loadAllPayments(currentPage);
        }
    } else {
        showToast('error', result?.message || 'Xatolik yuz berdi');
    }
}

// Update Pagination
function updatePagination(pagination) {
    const container = document.getElementById('pagination');
    const currentPage = pagination.page || 1;
    const totalPages = pagination.total_pages || 1;

    let html = '';

    // Previous button
    html += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="loadAllPayments(${currentPage - 1}); return false;">
                <i class="fas fa-chevron-left"></i>
            </a>
        </li>
    `;

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
            html += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="loadAllPayments(${i}); return false;">${i}</a>
                </li>
            `;
        } else if (i === currentPage - 3 || i === currentPage + 3) {
            html += `<li class="page-item disabled"><a class="page-link">...</a></li>`;
        }
    }

    // Next button
    html += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="loadAllPayments(${currentPage + 1}); return false;">
                <i class="fas fa-chevron-right"></i>
            </a>
        </li>
    `;

    container.innerHTML = html;
}

// Reset Filters
function resetFilters() {
    document.getElementById('filterStatus').value = '';
    document.getElementById('pageSize').value = '20';
    loadAllPayments(1);
}

// Get Status Badge HTML
function getStatusBadge(status) {
    const badges = {
        1: '<span class="badge-status bg-secondary">Kutilmoqda</span>',
        2: '<span class="badge-status bg-info">Isboti kutilmoqda</span>',
        3: '<span class="badge-status bg-warning">Ko\'rib chiqilmoqda</span>',
        4: '<span class="badge-status bg-success">Tasdiqlandi</span>',
        5: '<span class="badge-status bg-danger">Rad etildi</span>',
        6: '<span class="badge-status bg-dark">Qaytarildi</span>'
    };

    return badges[status] || '<span class="badge-status bg-secondary">Noma\'lum</span>';
}

// Format Currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('uz-UZ', {
        style: 'decimal',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount) + ' UZS';
}

// Format Date
function formatDate(dateString) {
    if (!dateString) return '-';

    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) {
        const hours = Math.floor(diff / (1000 * 60 * 60));
        if (hours === 0) {
            const minutes = Math.floor(diff / (1000 * 60));
            return `${minutes} daqiqa oldin`;
        }
        return `${hours} soat oldin`;
    } else if (days === 1) {
        return 'Kecha';
    } else if (days < 7) {
        return `${days} kun oldin`;
    }

    return date.toLocaleDateString('uz-UZ', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Show Toast Notification
function showToast(type, message) {
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'success' ? 'success' : 'danger'} position-fixed top-0 end-0 m-3`;
    toast.style.zIndex = '9999';
    toast.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
        ${message}
    `;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 3000);
}

// Auto-refresh every 30 seconds
setInterval(() => {
    if (document.getElementById('pending-tab').classList.contains('active')) {
        loadStatistics();
        loadPendingPayments();
    }
}, 30000);

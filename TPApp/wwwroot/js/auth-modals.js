// ===============================
// AUTH MODALS - AJAX LOGIC
// ===============================

// Switch from Login to Register modal
function switchToRegister(event) {
    if (event) event.preventDefault();
    
    // Hide login modal
    const loginModal = bootstrap.Modal.getInstance(document.getElementById('loginModal'));
    if (loginModal) {
        loginModal.hide();
    }
    
    // Show register modal after a short delay
    setTimeout(() => {
        const registerModal = new bootstrap.Modal(document.getElementById('registerModal'));
        registerModal.show();
    }, 300);
}

// Switch from Register to Login modal
function switchToLogin(event) {
    if (event) event.preventDefault();
    
    // Hide register modal
    const registerModal = bootstrap.Modal.getInstance(document.getElementById('registerModal'));
    if (registerModal) {
        registerModal.hide();
    }
    
    // Show login modal after a short delay
    setTimeout(() => {
        const loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
        loginModal.show();
    }, 300);
}

// Reset form when modal is closed
document.addEventListener('DOMContentLoaded', function() {
    // Reset login form
    const loginModal = document.getElementById('loginModal');
    if (loginModal) {
        loginModal.addEventListener('hidden.bs.modal', function() {
            document.getElementById('loginForm').reset();
            hideErrors('loginErrors');
        });
    }
    
    // Reset register form
    const registerModal = document.getElementById('registerModal');
    if (registerModal) {
        registerModal.addEventListener('hidden.bs.modal', function() {
            document.getElementById('registerForm').reset();
            hideErrors('registerErrors');
        });
    }
    
    // Handle login form submission
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLoginSubmit);
    }
    
    // Handle register form submission
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', handleRegisterSubmit);
    }
});

// Handle Login Form Submission
async function handleLoginSubmit(event) {
    event.preventDefault();
    
    const form = event.target;
    const submitBtn = form.querySelector('.auth-submit-btn');
    const btnText = submitBtn.querySelector('.btn-text');
    const spinner = submitBtn.querySelector('.spinner-border');
    
    // Show loading state
    submitBtn.classList.add('loading');
    submitBtn.disabled = true;
    spinner.classList.remove('d-none');
    
    // Hide previous errors
    hideErrors('loginErrors');
    
    // Get form data
    const formData = new FormData(form);
    
    try {
        const response = await fetch('/Account/LoginAjax', {
            method: 'POST',
            body: formData
        });
        
        const result = await response.json();
        
        if (result.success) {
            // Success - redirect to dashboard
            window.location.href = result.redirectUrl || '/Dashboard';
        } else {
            // Show errors
            displayErrors('loginErrors', result.errors);
        }
    } catch (error) {
        console.error('Login error:', error);
        displayErrors('loginErrors', ['Đã xảy ra lỗi. Vui lòng thử lại.']);
    } finally {
        // Hide loading state
        submitBtn.classList.remove('loading');
        submitBtn.disabled = false;
        spinner.classList.add('d-none');
    }
}

// Handle Register Form Submission
async function handleRegisterSubmit(event) {
    event.preventDefault();
    
    const form = event.target;
    const submitBtn = form.querySelector('.auth-submit-btn');
    const btnText = submitBtn.querySelector('.btn-text');
    const spinner = submitBtn.querySelector('.spinner-border');
    
    // Client-side password validation
    const password = form.querySelector('#registerPassword').value;
    const confirmPassword = form.querySelector('#registerConfirmPassword').value;
    
    if (password !== confirmPassword) {
        displayErrors('registerErrors', ['Mật khẩu và xác nhận mật khẩu không khớp.']);
        return;
    }
    
    // Show loading state
    submitBtn.classList.add('loading');
    submitBtn.disabled = true;
    spinner.classList.remove('d-none');
    
    // Hide previous errors
    hideErrors('registerErrors');
    
    // Get form data
    const formData = new FormData(form);
    
    try {
        const response = await fetch('/Account/RegisterAjax', {
            method: 'POST',
            body: formData
        });
        
        const result = await response.json();
        
        if (result.success) {
            // Success - redirect to dashboard
            window.location.href = result.redirectUrl || '/Dashboard';
        } else {
            // Show errors
            displayErrors('registerErrors', result.errors);
        }
    } catch (error) {
        console.error('Register error:', error);
        displayErrors('registerErrors', ['Đã xảy ra lỗi. Vui lòng thử lại.']);
    } finally {
        // Hide loading state
        submitBtn.classList.remove('loading');
        submitBtn.disabled = false;
        spinner.classList.add('d-none');
    }
}

// Display errors in alert box
function displayErrors(errorElementId, errors) {
    const errorElement = document.getElementById(errorElementId);
    if (!errorElement) return;
    
    if (errors && errors.length > 0) {
        let errorHtml = '';
        if (errors.length === 1) {
            errorHtml = errors[0];
        } else {
            errorHtml = '<ul class="mb-0">';
            errors.forEach(error => {
                errorHtml += `<li>${error}</li>`;
            });
            errorHtml += '</ul>';
        }
        
        errorElement.innerHTML = errorHtml;
        errorElement.classList.remove('d-none');
        
        // Scroll to error
        errorElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
}

// Hide errors
function hideErrors(errorElementId) {
    const errorElement = document.getElementById(errorElementId);
    if (errorElement) {
        errorElement.classList.add('d-none');
        errorElement.innerHTML = '';
    }
}

// Trigger login modal from external links
function openLoginModal() {
    const loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
    loginModal.show();
}

// Trigger register modal from external links
function openRegisterModal() {
    const registerModal = new bootstrap.Modal(document.getElementById('registerModal'));
    registerModal.show();
}

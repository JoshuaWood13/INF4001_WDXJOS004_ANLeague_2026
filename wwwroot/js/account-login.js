// Handles user login with Firebase Authentication + client-side validation (used Claude to help me write this file))

// Initialize Firebase
function initializeFirebase(config) {
    firebase.initializeApp(config);
}

// Get Firebase Auth instance
function getAuth() {
    return firebase.auth();
}

// Handle login form submission
async function handleLogin(email, password, loginCallbackUrl) {
    const auth = getAuth();

    try {
        // Log in with Firebase
        const userCredential = await auth.signInWithEmailAndPassword(email, password);
        const user = userCredential.user;

        // Get ID token
        const idToken = await user.getIdToken();

        // Send token to server to verify and set cookie
        const response = await fetch(loginCallbackUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ idToken: idToken })
        });

        const result = await response.json();

        if (result.success) {
            return { success: true, redirectUrl: result.redirectUrl };
        } else {
            return { success: false, error: result.message || 'Login failed. Please try again.' };
        }
    } catch (error) {
        const errorMsg = parseFirebaseLoginError(error);

        return { success: false, error: errorMsg };
    }
}

// Parse Firebase login errors into user friendly messages
function parseFirebaseLoginError(error) {
    if (error.code) {
        switch (error.code) {
            case 'auth/user-not-found':
                return 'No account found with this email address.';
            
            case 'auth/wrong-password':
            case 'auth/invalid-credential':
                return 'Incorrect email or password. Please check your credentials and try again.';
            
            case 'auth/invalid-email':
                return 'Please enter a valid email address.';
            
            case 'auth/user-disabled':
                return 'This account has been disabled. Please contact support for assistance.';
            
            case 'auth/too-many-requests':
                return 'Too many failed login attempts. Please try again later or reset your password.';
            
            case 'auth/network-request-failed':
                return 'Network error. Please check your internet connection and try again.';
            
            case 'auth/operation-not-allowed':
                return 'Email/password login is not enabled. Please contact support.';
            
            case 'auth/requires-recent-login':
                return 'For security reasons, please log out and log back in.';
            
            case 'auth/popup-closed-by-user':
                return 'Login was cancelled. Please try again.';
            
            default:
                // Check error code string for patterns
                const errorCodeStr = String(error.code).toLowerCase();
                if (errorCodeStr.includes('invalid-credential') || errorCodeStr.includes('wrong-password')) {
                    return 'Incorrect email or password. Please check your credentials and try again.';
                }
                if (errorCodeStr.includes('user-not-found')) {
                    return 'No account found with this email address.';
                }
                if (errorCodeStr.includes('network')) {
                    return 'Network error. Please check your internet connection and try again.';
                }
                break;
        }
    }
    
    // Handle by error message content
    if (error.message) {
        const lowerMessage = String(error.message).toLowerCase();
        
        // Check for credential errors
        if (lowerMessage.includes('invalid credential') || 
            lowerMessage.includes('wrong password') || 
            lowerMessage.includes('password is invalid') ||
            lowerMessage.includes('invalid password')) {
            return 'Incorrect email or password. Please check your credentials and try again.';
        }
        
        // Check for user not found
        if (lowerMessage.includes('user not found') || lowerMessage.includes('no user record')) {
            return 'No account found with this email address.';
        }
        
        // Check for network errors
        if (lowerMessage.includes('network') || 
            lowerMessage.includes('failed to fetch') || 
            lowerMessage.includes('networkerror')) {
            return 'Network error. Please check your internet connection and try again.';
        }
        
        // Check for timeout
        if (lowerMessage.includes('timeout')) {
            return 'Request timed out. Please try again.';
        }
    }
    
    // Default fallback
    return 'Incorrect email or password. Please check your credentials and try again.';
}

// Show loading state
function setButtonLoading(button, isLoading) {
    const loginText = button.querySelector('#login-text');
    const loginSpinner = button.querySelector('#login-spinner');

    if (isLoading) {
        button.disabled = true;
        loginText.style.display = 'none';
        loginSpinner.style.display = 'inline-block';
    } else {
        button.disabled = false;
        loginText.style.display = 'inline';
        loginSpinner.style.display = 'none';
    }
}

// Show error message
function showError(errorElement, message) {
    var errorText = errorElement.querySelector('#error-text');
    if (errorText) {
        errorText.textContent = message;
    } else {
        errorElement.textContent = message;
    }
    errorElement.style.display = 'block';
    
    // Auto-hide after 3 seconds
    setTimeout(function() {
        $(errorElement).fadeOut(500);
    }, 3000);
}

// Hide error message
function hideError(errorElement) {
    errorElement.style.display = 'none';
}

// Initialize login form validation and handlers
function initializeLoginForm(loginCallbackUrl) {
    // Auto-hide success message after 3 seconds
    var successMessage = document.getElementById('successMessage');
    if (successMessage) {
        setTimeout(function () {
            $(successMessage).fadeOut(500, function () {
                $(this).remove();
            });
        }, 3000);
    }

    // Input validation
    var currentError = null;
    var fields = ['email', 'password'];

    function showFieldError(field) {
        // Clear all existing errors first
        $('.is-invalid, .is-valid').removeClass('is-invalid is-valid');
        $('.invalid-feedback').text('');
        
        var $field = $('[name="' + field + '"]');
        $field.addClass('is-invalid');
        $field.siblings('.invalid-feedback').text($field.data('val-required'));
        currentError = field;
    }

    $('input').on('input', function () {
        var $field = $(this);
        var name = $field.attr('name');
        var val = $field.val();

        if (currentError === name && val?.trim()) {
            $field.removeClass('is-invalid').addClass('is-valid');
            $field.siblings('.invalid-feedback').text('');
            currentError = null;
        } else if (val?.trim() && !currentError) {
            $field.addClass('is-valid');
        }
    });

    // Handle form submission
    const loginForm = document.getElementById('login-form');
    if (loginForm) {
        loginForm.addEventListener('submit', async function(e) {
            e.preventDefault();

            // Client-side validation
            var firstEmpty = fields.find(function(f) {
                return !$('[name="' + f + '"]').val()?.trim();
            });

            if (firstEmpty) {
                showFieldError(firstEmpty);
                $('[name="' + firstEmpty + '"]').focus();
                return;
            }

            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            const loginBtn = document.getElementById('login-btn');
            const errorMessage = document.getElementById('error-message');

            setButtonLoading(loginBtn, true);
            hideError(errorMessage);

            // Complete login process after firebase auth 
            const result = await handleLogin(email, password, loginCallbackUrl);

            if (result.success) {
                window.location.href = result.redirectUrl;
            } else {
                // Clear valid states and form inputs when error occurs
                $('.is-valid').removeClass('is-valid');
                $('#email').val('');
                $('#password').val('');
                
                // Show error with parsed message
                showError(errorMessage, result.error || 'Login failed. Please try again.');
                setButtonLoading(loginBtn, false);
            }
        });
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//
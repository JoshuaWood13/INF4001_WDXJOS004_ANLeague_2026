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
        console.error('Login error:', error);

        // Handle Firebase-specific errors
        let errorMsg = 'Login failed. Please try again.';

        if (error.code === 'auth/user-not-found') {
            errorMsg = 'No account found with this email.';
        } else if (error.code === 'auth/wrong-password') {
            errorMsg = 'Incorrect email or password.';
        } else if (error.code === 'auth/invalid-email') {
            errorMsg = 'Invalid email address.';
        } else if (error.code === 'auth/user-disabled') {
            errorMsg = 'This account has been disabled.';
        } else if (error.code === 'auth/too-many-requests') {
            errorMsg = 'Too many failed login attempts. Please try again later.';
        } else if (error.code === 'auth/invalid-credential') {
            errorMsg = 'Incorrect email or password.';
        }

        return { success: false, error: errorMsg };
    }
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
    errorElement.textContent = message;
    errorElement.style.display = 'block';
}

// Hide error message
function hideError(errorElement) {
    errorElement.style.display = 'none';
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//
// Tournament Bracket JavaScript (used Claude to help me in writing this code)

document.addEventListener('DOMContentLoaded', function () {
    initializeJoinButtons();
    initializeAdminButtons();
    initializeRemoveButtons();
});

// Show toast notification
function showToast(message, type = 'error') {
    // Remove any existing toast messages
    const existingToasts = document.querySelectorAll('.toast-alert-message');
    existingToasts.forEach(toast => {
        toast.classList.remove('show');
        setTimeout(() => {
            if (toast && toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    });

    // Determine alert class based on type
    let alertClass = 'alert-danger';
    let alertTitle = 'Error!';
    
    if (type === 'success') {
        alertClass = 'alert-success';
        alertTitle = 'Success!';
    } else if (type === 'info') {
        alertClass = 'alert-info';
        alertTitle = 'Info!';
    }

    // Create alert element
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert ${alertClass} alert-dismissible fade show toast-alert-message position-fixed`;
    alertDiv.style.cssText = 'top: 80px; right: 20px; z-index: 1100; max-width: 400px;';
    alertDiv.setAttribute('role', 'alert');
    
    // Create close button
    const closeButton = document.createElement('button');
    closeButton.type = 'button';
    closeButton.className = 'btn-close';
    closeButton.setAttribute('aria-label', 'Close');
    closeButton.addEventListener('click', function() {
        alertDiv.classList.remove('show');
        setTimeout(() => {
            if (alertDiv && alertDiv.parentNode) {
                alertDiv.parentNode.removeChild(alertDiv);
            }
        }, 300);
    });
    
    // Create message content
    const strong = document.createElement('strong');
    strong.textContent = alertTitle + ' ';
    alertDiv.appendChild(strong);
    alertDiv.appendChild(document.createTextNode(message));
    alertDiv.appendChild(closeButton);

    // Append to body
    document.body.appendChild(alertDiv);

    // Trigger fade in animation
    setTimeout(() => {
        alertDiv.classList.add('show');
    }, 10);

    // Auto-dismiss after 3 seconds
    setTimeout(() => {
        if (alertDiv && alertDiv.parentNode) {
            alertDiv.classList.remove('show');
            // Remove from DOM after fade out
            setTimeout(() => {
                if (alertDiv && alertDiv.parentNode) {
                    alertDiv.parentNode.removeChild(alertDiv);
                }
            }, 300);
        }
    }, 3000);
}

// Initialize all join buttons for representatives
function initializeJoinButtons() {
    const joinButtons = document.querySelectorAll('.join-slot-btn');

    joinButtons.forEach(button => {
        button.addEventListener('click', async function (e) {
            e.preventDefault();
            e.stopPropagation();

            const matchId = this.getAttribute('data-match');
            const slot = this.getAttribute('data-slot');

            await joinTournamentSlot(matchId, slot, this);
        });
    });
}


// Join tournament slot
async function joinTournamentSlot(matchId, slot, button) {
    // Disable button during request
    button.disabled = true;
    const originalText = button.textContent;
    button.textContent = 'Joining...';

    try {
        const response = await fetch('/Tournament/JoinTournament', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                matchId: matchId,
                slot: slot
            })
        });

        // Check if response is ok before parsing
        if (!response.ok) {
            const errorText = await response.text();
            console.error('Server response:', response.status, errorText);
            throw new Error(`Server returned ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();

        if (result.success) {
            // Reload immediately to show updated server-side rendered data
            window.location.reload();
        } else {
            // Re-enable button on failure
            button.disabled = false;
            button.textContent = originalText;

            showToast(result.message || 'Failed to join tournament', 'error');
        }
    } catch (error) {
        console.error('Error joining tournament:', error);
        button.disabled = false;
        button.textContent = originalText;
        showToast(error.message || 'An error occurred while joining the tournament', 'error');
    }
}

// Initialize all remove buttons for admins
function initializeRemoveButtons() {
    const removeButtons = document.querySelectorAll('.remove-country-btn');

    removeButtons.forEach(button => {
        button.addEventListener('click', async function (e) {
            e.preventDefault();
            e.stopPropagation();

            const matchId = this.getAttribute('data-match');
            const slot = this.getAttribute('data-slot');
            const countryId = this.getAttribute('data-country-id');

            await removeCountryFromTournament(matchId, countryId, slot, this);
        });
    });
}

// Remove country from tournament
async function removeCountryFromTournament(matchId, countryId, slot, button) {
    // Disable button during request
    button.disabled = true;
    const originalText = button.textContent;
    button.textContent = '...';

    try {
        const response = await fetch('/Tournament/RemoveCountryFromTournament', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                matchId: matchId,
                countryId: countryId,
                slot: slot
            })
        });

        // Check if response is ok before parsing
        if (!response.ok) {
            const errorText = await response.text();
            console.error('Server response:', response.status, errorText);
            throw new Error(`Server returned ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();

        if (result.success) {
            // Reload immediately to show updated server-side rendered data
            window.location.reload();
        } else {
            // Re-enable button on failure
            button.disabled = false;
            button.textContent = originalText;

            showToast(result.message || 'Failed to remove country', 'error');
        }
    } catch (error) {
        console.error('Error removing country:', error);
        button.disabled = false;
        button.textContent = originalText;
        showToast(error.message || 'An error occurred while removing the country', 'error');
    }
}

// Initialize admin action buttons
function initializeAdminButtons() {
    // Start Tournament button
    const startBtn = document.querySelector('[data-action="start"]');
    if (startBtn) {
        startBtn.addEventListener('click', startTournament);
    }

    // Restart Tournament button
    const restartBtn = document.querySelector('[data-action="restart"]');
    if (restartBtn) {
        restartBtn.addEventListener('click', restartTournament);
    }

    // Play/Simulate buttons
    const actionButtons = document.querySelectorAll('[data-action="play"], [data-action="simulate"]');
    actionButtons.forEach(button => {
        button.addEventListener('click', function () {
            const action = this.getAttribute('data-action');
            const matchId = this.getAttribute('data-match');

            if (action === 'play') {
                playMatch(matchId, this);
            } else if (action === 'simulate') {
                simulateMatch(matchId, this);
            }
        });
    });
}

// Start tournament (admin only)
async function startTournament() {
    const startBtn = document.getElementById('startTournamentBtn');
    
    if (startBtn && startBtn.disabled) {
        return;
    }

    if (startBtn) {
        startBtn.disabled = true;
        startBtn.textContent = 'Starting...';
    }

    try {
        const response = await fetch('/Tournament/StartTournament', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        // Check if response is ok before parsing
        if (!response.ok) {
            const errorText = await response.text();
            console.error('Server response:', response.status, errorText);
            throw new Error(`Server returned ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();

        if (result.success) {
            // Reload immediately
            window.location.reload();
        } else {
            showToast(result.message || 'Failed to start tournament', 'error');
            if (startBtn) {
                startBtn.disabled = false;
                startBtn.textContent = 'Start Tournament';
            }
        }
    } catch (error) {
        console.error('Error starting tournament:', error);
        showToast(error.message || 'An error occurred while starting the tournament', 'error');
        if (startBtn) {
            startBtn.disabled = false;
            startBtn.textContent = 'Start Tournament';
        }
    }
}

// Restart tournament (admin only)
async function restartTournament() {
    const restartBtn = document.querySelector('[data-action="restart"]');
    
    if (restartBtn && restartBtn.disabled) {
        return;
    }
    
    // Disable button and show loading state
    if (restartBtn) {
        restartBtn.disabled = true;
        restartBtn.textContent = 'Restarting...';
    }

    try {
        const response = await fetch('/Tournament/RestartTournament', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        // Check if response is ok before parsing
        if (!response.ok) {
            const errorText = await response.text();
            console.error('Server response:', response.status, errorText);
            throw new Error(`Server returned ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();

        if (result.success) {
            // Reload immediately
            window.location.reload();
        } else {
            showToast(result.message || 'Failed to restart tournament', 'error');
            // Re-enable button on failure
            if (restartBtn) {
                restartBtn.disabled = false;
                restartBtn.textContent = 'Restart Tournament';
            }
        }
    } catch (error) {
        console.error('Error restarting tournament:', error);
        showToast(error.message || 'An error occurred while restarting the tournament', 'error');
        // Re-enable button on error
        if (restartBtn) {
            restartBtn.disabled = false;
            restartBtn.textContent = 'Restart Tournament';
        }
    }
}

// Play match with AI commentary (admin only)
async function playMatch(matchId, button) {
    // Check if button is already disabled
    if (button && button.disabled) {
        return;
    }

    // Disable all play and simulate buttons
    const allActionButtons = document.querySelectorAll('[data-action="play"], [data-action="simulate"]');
    allActionButtons.forEach(btn => {
        btn.disabled = true;
    });

    // Show loading state on the clicked button
    if (button) {
        button.textContent = 'Playing...';
    }

    // Create a form to submit POST request
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = `/Tournament/PlayMatch?matchId=${matchId}`;
    
    // Add anti-forgery token if available
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
    if (antiForgeryToken) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = antiForgeryToken.value;
        form.appendChild(tokenInput);
    }
    
    document.body.appendChild(form);
    form.submit();
}

// Simulate match (admin only)
async function simulateMatch(matchId, button) {
    // Check if button is already disabled
    if (button && button.disabled) {
        return;
    }

    // Disable all play and simulate buttons
    const allActionButtons = document.querySelectorAll('[data-action="play"], [data-action="simulate"]');
    allActionButtons.forEach(btn => {
        btn.disabled = true;
    });

    // Show loading state on the clicked button
    if (button) {
        button.textContent = 'Simulating...';
    }

    // Create a form to submit POST request
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = `/Tournament/SimulateMatch?matchId=${matchId}`;
    
    // Add anti-forgery token if available
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
    if (antiForgeryToken) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = antiForgeryToken.value;
        form.appendChild(tokenInput);
    }
    
    document.body.appendChild(form);
    form.submit();
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);
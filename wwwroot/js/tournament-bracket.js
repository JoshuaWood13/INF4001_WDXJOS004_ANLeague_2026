// Tournament Bracket JavaScript (used Claude to help me in writing this code)

document.addEventListener('DOMContentLoaded', function () {
    initializeJoinButtons();
    initializeAdminButtons();
    initializeRemoveButtons();
});

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

        const result = await response.json();

        if (result.success) {
            showToast(result.message, 'success');
            
            // Reload page to show updated server-side rendered data
            setTimeout(() => {
                window.location.reload();
            }, 1000);
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
        showToast('An error occurred while joining the tournament', 'error');
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
    // Confirm removal
    if (!confirm('Are you sure you want to remove this country from the tournament?')) {
        return;
    }

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
            showToast(result.message, 'success');
            
            // Reload page to show updated server-side rendered data
            setTimeout(() => {
                window.location.reload();
            }, 1000);
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
                playMatch(matchId);
            } else if (action === 'simulate') {
                simulateMatch(matchId);
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
    
    if (!confirm('Are you sure you want to start the tournament? All 8 teams must be registered before starting.')) {
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
            showToast(result.message, 'success');
            // Reload the page to show the new tournament state
            setTimeout(() => {
                window.location.reload();
            }, 1500);
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
    
    if (!confirm('Are you sure you want to restart the tournament? All original quarter-final teams will be re-registered automatically.')) {
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
            showToast(result.message, 'success');
            // Keep loading state while page reloads
            // Reload the page to show the new tournament state
            setTimeout(() => {
                window.location.reload();
            }, 1500);
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
async function playMatch(matchId) {
    if (!confirm(`Are you sure you want to play ${matchId} with AI commentary?`)) {
        return;
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
async function simulateMatch(matchId) {
    if (!confirm(`Are you sure you want to simulate ${matchId}?`)) {
        return;
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

// Toast notification helper
function showToast(message, type = 'info') {
    // Check if toast container exists, if not create it
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            display: flex;
            flex-direction: column;
            gap: 10px;
        `;
        document.body.appendChild(toastContainer);
    }

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;

    const colors = {
        success: '#28a745',
        error: '#dc3545',
        info: '#17a2b8',
        warning: '#ffc107'
    };

    toast.style.cssText = `
        background-color: ${colors[type] || colors.info};
        color: white;
        padding: 12px 20px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        min-width: 250px;
        max-width: 400px;
        font-size: 14px;
        font-weight: 500;
        animation: slideIn 0.3s ease-out;
    `;

    toast.textContent = message;
    toastContainer.appendChild(toast);

    // Auto-remove after 4 seconds
    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease-out';
        setTimeout(() => {
            toast.remove();
        }, 300);
    }, 4000);
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
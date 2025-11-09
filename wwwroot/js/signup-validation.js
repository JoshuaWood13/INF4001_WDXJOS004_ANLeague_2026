// Client-side validation for Sign Up form (used Claude to help me write this file)

$(document).ready(function () {
    
    // Reset submit button on page load (in case of server-side error)
    var $submitBtn = $('#signUpForm button[type="submit"]');
    $submitBtn.prop('disabled', false).html('Create Account');
    
    // Auto-hide Firebase error after 3 seconds
    var $firebaseError = $('#firebaseError');
    if ($firebaseError.length) {
        setTimeout(function () {
            $firebaseError.fadeOut(500, function () {
                $(this).remove();
            });
        }, 3000);
    }

    // Input Validation
    var currentError = null;
    var fields = ['Email', 'CountryName', 'Password', 'ConfirmPassword'];

    // Show error on a specific field
    function showError(field) {
        $('.is-invalid, .is-valid').removeClass('is-invalid is-valid');
        $('.invalid-feedback').text('');
        
        var $field = $('[name="' + field + '"]');
        $field.addClass('is-invalid');
        $field.siblings('.invalid-feedback').text($field.data('val-required'));
        currentError = field;
    }

    // Clear error and show valid state on type
    $('input, select').on('input change', function () {
        var $field = $(this);
        var name = $field.attr('name');
        var val = $field.val();

        if ($field.hasClass('is-invalid')) {
            $field.removeClass('is-invalid');
            $field.siblings('.invalid-feedback').text('');
            
            if (val?.trim()) {
                $field.addClass('is-valid');
            }
            return;
        }

        if (currentError === name && val?.trim()) {
            $field.removeClass('is-invalid').addClass('is-valid');
            $field.siblings('.invalid-feedback').text('');
            currentError = null;
        } 

        else if (val?.trim() && !currentError) {
            $field.addClass('is-valid');
        }
    });

    // Form submission - validate one field at a time
    $('#signUpForm').on('submit', function (e) {
        var password = $('[name="Password"]').val();
        var confirmPassword = $('[name="ConfirmPassword"]').val();

        // Find first empty field
        var firstEmpty = fields.find(function(f) {
            return !$('[name="' + f + '"]').val()?.trim();
        });

        if (firstEmpty) {
            e.preventDefault();
            showError(firstEmpty);
            $('[name="' + firstEmpty + '"]').focus();
            return false;
        }

        // Check if passwords match before submitting to server
        if (password !== confirmPassword) {
            e.preventDefault();
            $('.is-invalid, .is-valid').removeClass('is-invalid is-valid');
            $('.invalid-feedback').text('');
            var $confirmField = $('[name="ConfirmPassword"]');
            $confirmField.addClass('is-invalid');
            $confirmField.siblings('.invalid-feedback').text('Passwords do not match');
            currentError = 'ConfirmPassword';
            $confirmField.focus();
            return false;
        }

        // All validation passed
        $submitBtn.prop('disabled', true).html(
            '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Creating Account...'
        );
    });
});


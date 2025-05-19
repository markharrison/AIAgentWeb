$(document).ready(function () {

    $('#IdGetThreadForm').on('submit', function () {
        $('#IdSpinnerText').text('Getting Thread ...');
        $('#IdSpinner').show();
        $('#IdGetThreadBut').hide();
        $(".alert").hide();
    });

    $('#IdChatForm').on('submit', function (event) {

        var formData = $(this).serialize();

        var htmlUserMessage = '<div class="message user-message">' + $('#IdAsk').val() + '</div>';

        $('#IdSpinnerText').text('');
        $('#IdSpinner').show();
        $('#IdChatBut').hide();
        $(".alert").hide();
        $('#IdChatContainer').show();
        $('#IdChatContainer').append(htmlUserMessage);
        $('#IdAsk').val('');
        scrollToInput();

        event.preventDefault();

        $.ajax({
            type: 'POST',
            url: $(this).attr('action'),
            data: formData,
            success: function (response) {

                var htmlAssistantMessage = '<div class="message assistant-message">' + response + '</div>';

                $('#IdSpinner').hide();
                $('#IdChatContainer').append(htmlAssistantMessage);
                $('#IdChatContainer').show();
                $('#IdChatBut').show();
                scrollToButton();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $('#IdSpinner').hide();
                var errorCode = jqXHR.status;
                var errorText = jqXHR.responseText;

                $('#IdChatContainer').prepend('<div class="alert alert-danger">Error: ' + errorCode + ' ' + errorText + '</div>');
                $('#IdChatContainer').show();
                $('#IdChatBut').show();

            }
        });
    });

    function scrollToInput() {
        var inputField = document.getElementById('IdAsk');
        if (inputField) {
            inputField.scrollIntoView({ behavior: 'smooth', block: 'end' });
        }
    }

    function scrollToButton() {
        var button = document.getElementById('IdChatBut');
        if (button) {
            button.scrollIntoView({ behavior: 'smooth', block: 'end' });
        }
    }


});


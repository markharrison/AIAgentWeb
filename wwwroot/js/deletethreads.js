function deleteThreads() {

    $('#IdSpinnerText').text('Deleting ...');
    $('#IdDeleteThreadsBut').hide();
    $('#IdSpinner').show();


    $.get('/DeleteThreads?handler=DeleteThreads')
        .done(function (data) {
            $('#IdSpinner').hide();
            $('#IdAlert').addClass('alert-success');
            $('#IdAlert').show();
            $('#IdAlert').html("Threads deleted");
 

        })
        .fail(function (jqXHR) {
            var errorText = jqXHR.responseText;
            var errorCode = jqXHR.status;

            $('#IdSpinner').hide();
            $('#IdAlert').addClass('alert-danger');
            $('#IdAlert').show();
            $('#IdAlert').html('Error: ' + errorCode + ' ' + errorText );
        });

}


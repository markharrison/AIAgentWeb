function loadFiles() {

    $('#IdLoadFilesBut').hide();
    $('#IdSpinner').show();
    $('#IdDataContainer').show();

    $.get('/LoadFiles?handler=LoadFiles')
        .done(function (data) {
            $('#IdDataContainer').html(data);
            pollProgress();
        })
        .fail(function (jqXHR) {
            if (jqXHR.status === 429) {
                $('#IdDataContainer').html('<div class="alert alert-warning">Process already running - please try again later.</div>');
            } else {
                $('#IdDataContainer').html('<div class="alert alert-danger">An error occurred: ' + jqXHR.status + ' ' + jqXHR.responseText + '</div>');
            }
            $('#IdSpinner').hide();
            $('#IdLoadFilesBut').show();

        });

}

function pollProgress() {
    $('#IdDataContainer').show();

    $.get('/LoadFiles?handler=Progress')
        .done(function (data, textStatus, jqXHR) {
            $('#IdDataContainer').html(data);
            if (jqXHR.status === 200) {
                $('#IdSpinner').hide();
                $('#IdLoadFilesBut').show();
            } else if (jqXHR.status === 202) {
                setTimeout(pollProgress, 1000);  
            }
        })
        .fail(function (jqXHR) {
            $('#IdDataContainer').html('An error occurred: ' + jqXHR.statusText);
            $('#IdSpinner').hide();
            $('#IdLoadFilesBut').show();
        });
}

function copyText() {
    const text = $('#IdTextToCopy').text();

    navigator.clipboard.writeText(text).then(() => {
        setTimeout(() => {
            $('#IdCopyBut').hide();
        }, 500);
    });
}

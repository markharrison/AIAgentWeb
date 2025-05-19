$(document).ready(function () {
    $.get('/GetAgents?handler=GetAgents', function (data) {
        $('#IdDataContainer').html(data);
        $('#IdSpinner').hide();
        $('#IdDataContainer').show();

        $('#IdStoreForm').on('submit', function () {
            $('#IdSpinnerText').text('Deleting ...');
            $('#IdSpinner').show();
            $('#IdSubmitBut').hide();
        });
    });
});
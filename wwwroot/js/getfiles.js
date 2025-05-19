$(document).ready(function () {

    $.get('/GetFiles?handler=GetFiles', function (data) {
        $('#IdDataContainer').html(data);
        $('#IdSpinner').hide();
        $('#IdDataContainer').show();

        $('#IdFileForm').on('submit', function () {
            $('#IdSpinnerText').text('Deleting ...');
            $('#IdSpinner').show();
            $('#IdSubmitBut').hide();
            $(".alert").hide();
        });
    });
});
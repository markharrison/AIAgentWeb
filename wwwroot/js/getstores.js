$(document).ready(function () {
    $.get('/GetStores?handler=GetStores', function (data) {
        $('#IdDataContainer').html(data);
        $('#IdSpinner').hide();
        $('#IdDataContainer').show();

        $('#IdStoreForm').on('submit', function () {
            $('#IdSpinnerText').text('Deleting ...');
            $('#IdSpinner').show();
            $('#IdSubmitBut').hide();
            $(".alert").hide();
        });
    });
});
$(document).ready(function () {
    $("#IdFiles").on("change", function () {
        const maxSize = 1000 * 1000 * 1000; // 1 GB
        let files = this.files;
        let totalsize = 0;
        for (let file of files) {
            totalsize += file.size;
        }
        if (totalsize > maxSize) {
            $("#IdSubmitBut").prop("disabled", true);
            $("#IdAlertFileWarning").show();
            $('#IdAlertFileWarning').html('Total file size exceeds 1 GB.');
            $("#IdAlertUpload").hide();  
        }
        else {
            $("#IdAlertFileWarning").hide();
            $("#IdSubmitBut").prop("disabled", false);
        }
    });

    $("form").on("submit", function (e) {

        $("#IdAlertUpload").hide();          
        $("#IdAlertFileWarning").hide();   

        // Add spinner HTML after h4 element
        $(`<div id="IdSpinner">
            <p id="IdSpinnerText">Uploading ...</p>
            <div class="spinner-border text-primary" role="status">
            </div>
        </div>`).insertAfter("h4");

    });




});

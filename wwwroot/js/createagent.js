$(document).ready(function () {

    //$.validator.addMethod("noWhitespaceOnly", function (value, element) {
    //    return this.optional(element) || $.trim(value).length > 0;
    //}, "This field cannot contain only whitespace.&nbsp;");

    //// Apply the custom rule to specific fields
    //$("form").validate({
    //    errorClass: "text-danger field-validation-error",
    //    rules: {
    //        AgentName: {
    //            noWhitespaceOnly: true
    //        },
    //        ModelDeployment: {
    //            noWhitespaceOnly: true
    //        },
    //        AgentInstructions: {
    //            noWhitespaceOnly: true
    //        }
    //    }
    //});


    $('#IdCreateForm').on('submit', function (e) {

        $(".alert").hide();

        if (!$(this).valid()) {

            e.preventDefault();
            return;
        }

        $('#IdSpinnerText').text('Creating ...');
        $('#IdSpinner').show();
        $('#IdSubmitBut').hide();


    });

    $('#IdFileSearchTool').change(function () {
        doFileSearchTool();
    });

    doFileSearchTool();


});

function doFileSearchTool() {
    if ($('#IdFileSearchTool').is(':checked')) {
        $('#IdFileSearchGroup').show();
    } else {
        $('#IdFileSearchGroup').hide();
    }
}

function copyText() {
    const text = $('#IdTextToCopy').text();

    navigator.clipboard.writeText(text).then(() => {
        setTimeout(() => {
            $('#IdCopyBut').hide();
        }, 500);
    });
}
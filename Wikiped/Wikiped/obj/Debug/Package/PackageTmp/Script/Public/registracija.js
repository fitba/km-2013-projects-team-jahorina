function PrijavaL() {
    $("#LoginForma").dialog({
        width: 300,
        height: 230,
        modal: true,
        resizable: false,
        draggable: false,
        autoOpen: true,
        open: function () {

            $(this).parent().appendTo("form#frmLogin");
            $('.ui-widget-overlay').bind('click', function () {
                deAtachValidacija();
                $('#LoginForma').dialog('close');

            })
        }

    })
}

function promSifre2() {
    $("#registracijskaForma").dialog({
        width: 300,
        height: 280,
        modal: true,
        resizable: false,
        draggable: false,
        autoOpen: true,
        open: function () {
            $(this).parent().appendTo("form#frmReg");
            $('.ui-widget-overlay').bind('click', function () {
                deAtachValidacija();
                $('#registracijskaForma').dialog('close');

            })
        }

    })
}
function deAtachValidacija(forma) {

    $(forma).validationEngine('detach');

}
function AtachValidaciju(forma) {
    jQuery(forma).validationEngine('attach', {
        promptPosition: "topLeft",
        onValidationComplete: function (form, status) {
            if (status == true) {
                deAtachValidacija(forma);
                jQuery(forma).submit();
            }
            else {

            }


        }
    });
}
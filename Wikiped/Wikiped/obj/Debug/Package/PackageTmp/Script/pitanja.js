$(document).ready(function () {

    $(".vote .vote-up-off").click(function () {
        var obj = $(this);
        var id = $(this).parent().children('input').val();
        var voteNumber = $(this).parent().children('.vote-count-post').text();
        $.ajax({
            url: '/pitanja/VoteUp',
            data: { 'id': id, 'voteNumber': voteNumber },
            type: "post",
            cache: false,
            success: function (data) {
                $(obj).parent().children('.vote-count-post').text(data.voteNumber);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr);
            }
        });

    });

    $(".vote .vote-down-off").click(function () {
        var obj = $(this);
        var id = $(this).parent().children('input').val();
        var voteNumber = $(this).parent().children('.vote-count-post').text();
        $.ajax({
            url: '/pitanja/VoteDown',
            data: { 'id': id, 'voteNumber': voteNumber },
            type: "post",
            cache: false,
            success: function (data) {
                $(obj).parent().children('.vote-count-post').text(data.voteNumber);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr);
            }
        });

        //        $.getJSON("/pitanja/VoteDown", { id: id, number: "2pm" }, function (data) {
        //            alert(data.id);
        //        });
    });

});
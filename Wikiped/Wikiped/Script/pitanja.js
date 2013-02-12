$(document).ready(function () {

    function userVoteUp(tempVal) {
        var obj = $(tempVal);
        var id = $(tempVal).parent().children('input').val();
        var voteNumber = $(tempVal).parent().children('.vote-count-post').text();
        var mainQu = $(tempVal).parent().hasClass('mainQuestion');

        $.ajax({
            url: '/pitanja/VoteUp',
            data: { 'id': id, 'mainQuestion': mainQu },
            type: "post",
            cache: false,
            success: function (data) {
                if (data.voteNumber == -1) {
                    // alert("vec ste glasali");
                }
                $(obj).parent().children('.vote-count-post').text(data.voteNumber);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr);
            }
        });
    }
    function userVoteDown(tempVal) {
        var obj = $(tempVal);
        var id = $(tempVal).parent().children('input').val();
        var voteNumber = $(tempVal).parent().children('.vote-count-post').text();
        var mainQu = $(tempVal).parent().hasClass('mainQuestion');

        $.ajax({
            url: '/pitanja/VoteDown',
            data: { 'id': id, 'mainQuestion': mainQu },
            type: "post",
            cache: false,
            success: function (data) {
                if (data.voteNumber == -1) {
                    //alert("vec ste glasali");
                }
                $(obj).parent().children('.vote-count-post').text(data.voteNumber);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr);
            }
        });
    }

    $(".vote .vote-up-off").click(function () {
        userVoteUp($(this));
    });

    $(".vote .vote-down-off").click(function () {
        userVoteDown($(this));
    });

    $(".textComment a").click(function () {
        $(this).parent().children('.comment').toggle('slow');
    });

    $(".btnComment").click(function () {
        var id = $(this).parent().children('input').val();
        var text = $(this).parent().children('.commentText').val();

        $.ajax({
            url: '/pitanja/AddCommentForAnswer',
            data: { 'id': id, 'textComm': text },
            type: "post",
            cache: false,
            success: function (data) {

                window.location = window.location;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr);
            }
        });

    });
  
    $(".btnComment").button();
    $(".mainCommentBtn").button();
    $(".wmd-input").text('');
});

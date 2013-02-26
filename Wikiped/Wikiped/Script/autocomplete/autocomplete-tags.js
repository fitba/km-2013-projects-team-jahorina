$(function () {
    //    $.ajax({
    //        url: '/pitanja/GetTags',
    //        data: { 'id': 'test' },
    //        type: "post",
    //        cache: false,
    //        async: true,
    //        success: function (data) {
    //            alert(data);
    //        },
    //        error: function (xhr, ajaxOptions, thrownError) {
    //            console.log(xhr);
    //        }
    //    });

    $('#txtTags').tagit({
        tagSource: function (request, response) {
            $.ajax({
                url: '/pitanja/GetTags',
                data: { 'id': 'test' },
                type: "post",
                cache: false,
                async: true,
                success: function (data) {
                   // alert(data[0].Ime);
                    response($.map(data, function (item) {
                        return {
                            label: item.Name, value: item.Name, id: item.Id
                        }
                    }))
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    console.log(xhr);
                }
            })
        }
    });

    $('#txtTagsClanci').tagit({
        class: "clanak-create-textBox",
        tagSource: function (request, response) {
            $.ajax({
                url: '/pitanja/GetTags',
                data: { 'id': 'test' },
                type: "post",
                cache: false,
                async: true,
                success: function (data) {
                    // alert(data[0].Ime);
                    response($.map(data, function (item) {
                        return {
                            label: item.Name, value: item.Name, id: item.Id
                        }
                    }))
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    console.log(xhr);
                }
            })
        }
    });

    //    $('#txtTags').tagit({
    //        tagSource: function (request, response) {
    //            $.ajax({
    //                type: "GET",
    //                contentType: "application/json; charset=utf-8",
    //                url: '/pitanja/GetTags',
    //                data: { 'id': "1" },
    //                dataType: "json",
    //                cache: false,
    //                async: true,
    //                success: function (data) {
    //                    alert(data.d);
    //                    response($.map(data.d, function (item) {
    //                        return {
    //                            label: item.Ime, value: item.TagID, id: item.TagID
    //                        }
    //                    }))
    //                },
    //                error: function (XMLHttpRequest, textStatus, errorThrown) {
    //                    console.log(textStatus);
    //                    //alert("hen"+textStatus);
    //                }
    //            })
    //        }
    //    });

});
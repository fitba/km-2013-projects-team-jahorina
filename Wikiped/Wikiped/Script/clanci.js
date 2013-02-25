function zloupotreba(clanakID) {
    var clanakIDint = clanakID.replace("cl", "");

    $.ajax({
        url: '/Clanci/zloupotreba',
        data: { 'zlID': clanakIDint },
        type: "post",
        cache: false,
        success: function (data) {
            if (data.Success == true) {

            }
            $("#zl" + clanakIDint).text(data.message);
            $("#zl" + clanakIDint).slideDown();
            setTimeout(function () {
                $($("#zl" + clanakIDint)).slideUp();
            }, 3000);



        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert("Problem");
            console.log(xhr);
        }
    });

}

function NoviKomentar(text, clanakID) {

    $.ajax({
        url: '/Clanci/NoviKomentar',
        data: { 'text': text, 'clanakID': clanakID },
        type: "post",
        cache: false,
        success: function (data) {
            if (data.Success == true) {
                clanakApp(data.tekst, data.datum, data.korisnik);
            }

            Postano(data.Success, data.poruka);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert("Problem");
            console.log(xhr);
        }
    });

}


function Postano(uspjesno, poruka) {

    if (uspjesno == true) {

        $(".clanci-poruka-poslano").css("color", "#0b7100");

    }
    else {
        $(".clanci-poruka-poslano").css("color", "#ee0101");
    }
    $(".clanci-poruka-poslano").text(poruka);
}
function fadeError(Ocjene) {
    $(Ocjene).fadeIn();
    setTimeout(function () {
        $(Ocjene).fadeOut();
    }, 3000);
}
function clanakApp(text, datum, korisik) {
    var Clanak = '<div class="clanak-komentar-wraper">';
    Clanak += '<div class="clanak-komentar-evidencija">';
    Clanak += '<div class="clanak-komentar-ko" style="font-weight:bold">' + korisik + '</div>';
    Clanak += '<div class="clanak-komentar-ko">' + datum + '</div>';
    Clanak += '</div><div class="clanak-komentar-tekst">' + text + '</div>';
    Clanak += '<div class="clanak-komentar-dojam"><a class="clanak-greska" title="Prijavi komentar"></a></div></div>';

    $(".mainComment-Clanci").append(Clanak);







}


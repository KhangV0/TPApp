$(document).ready(function () {

    $(".GioHang").hover(
        function () { $(this).find(".button-group").show(); },
        function () { $(this).find(".button-group").hide(); }
    );

    $("#product-detail p, #product-detail span, #product-detail div").removeAttr("style");
    $("#information p, #information span, #information div").removeAttr("style");
    $("#reviews p, #reviews span, #reviews div").removeAttr("style");

    $(".ui-tabViewNav li").click(function () {
        $(".ui-tabViewNav li").removeClass("selected");
        $(this).addClass("selected");

        $(".ChiTietNhaTuVanTab").hide();
        $("#" + $(this).find("a").attr("href")).show();
        return false;
    });

    $("#btnHoanTat").click(function () {
        alert("Đánh giá gửi thành công. Cám ơn đánh giá của bạn.");
    });

    // ====== INIT OWL ======
    $(".owl-carousel").owlCarousel({
        items: 4,
        loop: true,
        nav: true,
        dots: false,
        margin: 30,
        autoplay: true,
        autoplayTimeout: 3000,
        autoplayHoverPause: true,
        responsive: {
            0: { items: 2 },
            600: { items: 4 },
            1000: { items: 4 }
        }
    });

});


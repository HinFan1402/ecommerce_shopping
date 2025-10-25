$(function () {

    if ($("a.confirmDeletion").length) {
        $("a.confirmDeletion").click(() => {
            if (!confirm("Confirm deletion")) return false;
        });
    }

    if ($("div.alert.notification").length) {
        setTimeout(() => {
            $("div.alert.notification").fadeOut();
        }, 2000);
    }

});

function readURL(input) {
    if (input.files && input.files[0]) {
        let reader = new FileReader();

        reader.onload = function (e) {
            $("img#imgpreview").attr("src", e.target.result).width(200).height(200);
        };

        reader.readAsDataURL(input.files[0]);
    }
}

    $(document).ready(function () {

        // Thêm vào wishlist
        $(document).on('click', '.wishlist-btn', function (e) {
            e.preventDefault();
            var productId = $(this).data('product-id');
            $.ajax({
                url:'/Home/AddToWishList',
                type: 'POST',
                data: { id: productId },
                success: function (result) {
                    if (result.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Thành công',
                            text: 'Sản phẩm đã được thêm vào wishlist!'
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi',
                            text: 'Không thể thêm sản phẩm vào wishlist.'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Đã xảy ra lỗi khi kết nối tới server.'
                    });
                }
            });
        });

    // Thêm vào compare
    $(document).on('click', '.compare-btn', function (e) {
        e.preventDefault();
    var productId = $(this).data('product-id');
    $.ajax({
        url: "/Home/AddToCompare",
    type: 'POST',
    data: {id: productId },
    success: function (result) {
                        if (result.success) {
        Swal.fire({
            icon: 'success',
            title: 'Thành công',
            text: 'Sản phẩm đã được thêm vào compare!'
        });
                        } else {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Không thể thêm sản phẩm vào compare.'
        });
                        }
                    },
    error: function () {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Đã xảy ra lỗi khi kết nối tới server.'
        });
                    }
                });
            });

    // Thêm vào giỏ hàng
    $(document).on('click', '.add-to-cart', function (e) {
        e.preventDefault();
    var productId = $(this).data('product-id');
    $.ajax({
        url: '/Cart/Add',
    type: 'POST',
    data: {id: productId },
    success: function (result) {
                        if (result.success) {
        Swal.fire({
            icon: 'success',
            title: 'Thành công',
            text: 'Sản phẩm đã được thêm vào giỏ hàng!'
        });
                        } else {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Không thể thêm sản phẩm vào giỏ hàng.'
        });
                        }
                    },
    error: function () {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Đã xảy ra lỗi khi kết nối tới server.'
        });
                    }
                });
            });
        });
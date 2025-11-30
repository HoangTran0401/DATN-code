$(document).ready(function () {
    ShowCount();
    // FIX LỖI OWL CAROUSEL CLONE GỌI 2 LẦN
    $('body').on('click', '.owl-item.cloned .btnAddToCart', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation(); // Ngăn click vào item clone
    });

    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        e.stopPropagation();  // ✅ CHỈ CHẶN SỰ KIỆN LAN RA NGOÀI, KHÔNG CHẶN SWEETALERT

        var id = $(this).data('id');

        $.ajax({
            url: '/shoppingcart/addtocart',
            type: 'POST',
            data: { id: id, quantity: 1 },
            success: function (rs) {
                if (rs.Success) {

                    $('#checkout_items').html(rs.Count);

                    Swal.fire({
                        title: "Thêm vào giỏ hàng!",
                        text: rs.msg,
                        icon: "success",
                        timer: 2000,
                        showConfirmButton: false
                    });
                }
            }
        });
    });

});

function ShowCount() {
    $.ajax({
        url: '/shoppingcart/ShowCount',
        type: 'GET',
        success: function (rs) {
            $('#checkout_items').html(rs.Count);
        }
    });
}

function LoadCart() {
    $.ajax({
        url: '/shoppingcart/Partial_Item_Cart',
        type: 'GET',
        success: function (rs) {
            $('#load_data').html(rs);
        }
    });
}

function Update(id, quantity) {
    $.ajax({
        url: '/shoppingcart/Update',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        }
    });
}

function DeleteAll() {
    $.ajax({
        url: '/shoppingcart/DeleteAll',
        type: 'POST',
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        }
    });
}

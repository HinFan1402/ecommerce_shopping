<script>

    $('.add-to-cart').click(function () {
				var Id = $(this).data("product_id");

    //alert(product_id);

                $.ajax({
                    type: "POST",
                    url: "@Url.Action("Add","Cart")",
                    data: {Id: Id }, // Send data to the server

                    success: function (result) {
						// Handle successful update
						if (result) {
                            Swal.fire("Thêm giỏ hàng thành công.");
						} 
					}

				});
			});

</script>
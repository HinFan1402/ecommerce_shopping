using ecommerce_shopping.Areas.Admin.Repository;
using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModel;
using ecommerce_shopping.Repository;
using ecommerce_shopping.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Linq;

namespace ecommerce_shopping.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly IStatisticalService _statisticalService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public CheckoutController(DataContext dataContext, IEmailSender emailSender, IStatisticalService statisticalService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _dataContext = dataContext;
            _emailSender = emailSender;
            _statisticalService = statisticalService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CartItemViewModel model)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                TempData["error"] = "Vui lòng đăng nhập để thanh toán";
                return RedirectToAction("Login", "Account");
            }

            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            if (!cartItems.Any())
            {
                TempData["error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            // Validate shipping info
            if (string.IsNullOrEmpty(model.ShippingName) ||
                string.IsNullOrEmpty(model.ShippingPhone) ||
                string.IsNullOrEmpty(model.ShippingProvince) ||
                string.IsNullOrEmpty(model.ShippingDistrict) ||
                string.IsNullOrEmpty(model.ShippingWard) ||
                string.IsNullOrEmpty(model.ShippingAddress))
            {
                TempData["error"] = "Vui lòng điền đầy đủ thông tin giao hàng";
                return RedirectToAction("Index", "Cart");
            }

            if (string.IsNullOrEmpty(model.PaymentMethod))
            {
                TempData["error"] = "Vui lòng chọn phương thức thanh toán";
                return RedirectToAction("Index", "Cart");
            }

            // Tạo đơn hàng
            var orderCode = Guid.NewGuid().ToString();
            var orderItem = new OrderModel
            {
                OrderCode = orderCode,
                UserName = userEmail,
                status = 0,
                CreateDate = DateTime.Now,
                ShippingName = model.ShippingName,
                ShippingPhone = model.ShippingPhone,
                ShippingProvince = model.ShippingProvince,
                ShippingDistrict = model.ShippingDistrict,
                ShippingWard = model.ShippingWard,
                ShippingAddress = model.ShippingAddress,
                ShippingCost = model.ShippingCost,
                CouponCode = model.CouponCode,
                FinalTotal = model.FinalTotal,
                PriceCoupon = model.PriceCoupon,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending
            };

            _dataContext.Orders.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            // Thêm chi tiết đơn hàng + statistical + update product quantities
            foreach (var cart in cartItems)
            {
                var orderdetails = new OrderDetailModel
                {
                    OrderCode = orderCode,
                    UserName = userEmail,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };

                await _statisticalService.CreateStatisticalAsync(
                    orderdetails,
                    orderStatus: 0,
                    couponCode: model.CouponCode,
                    discountAmount: model.PriceCoupon
                );

                var product = _dataContext.Products.First(p => p.Id == cart.ProductId);
                product.Quantity -= cart.Quantity;
                product.Sold += cart.Quantity;

                _dataContext.Add(orderdetails);
                _dataContext.Update(product);
            }

            await _dataContext.SaveChangesAsync();

            // amount calculation
            decimal grandTotal = cartItems.Sum(x => x.Total);
            decimal amountToPay = model.FinalTotal > 0 ? model.FinalTotal : (grandTotal + model.ShippingCost - model.PriceCoupon);
            if (amountToPay < 0) amountToPay = 0;

            // Online flows
            if (model.PaymentMethod == "Momo")
            {
                var momoUrl = await CreateMomoPaymentUrlAsync(orderItem, amountToPay);
                if (!string.IsNullOrEmpty(momoUrl))
                {
                    HttpContext.Session.Remove("Cart");
                    return Redirect(momoUrl);
                }

                TempData["error"] = "Không thể khởi tạo thanh toán Momo. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }
            else if (model.PaymentMethod == "VNPay")
            {
                var vnpUrl = CreateVnPayUrl(orderItem, amountToPay);
                if (!string.IsNullOrEmpty(vnpUrl))
                {
                    HttpContext.Session.Remove("Cart");
                    return Redirect(vnpUrl);
                }

                TempData["error"] = "Không thể khởi tạo thanh toán VNPay. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }

            // Cash: complete order (no online gateway)
            HttpContext.Session.Remove("Cart");
            orderItem.PaymentStatus = PaymentStatus.Pending;
            _dataContext.Orders.Update(orderItem);
            await _dataContext.SaveChangesAsync();

            // send confirmation email (existing behavior)
            string emailBody = $@"
                <h2>Đơn hàng #{orderCode}</h2>
                <p>Cảm ơn bạn đã đặt hàng!</p>
                <h3>Thông tin giao hàng:</h3>
                <p>Người nhận: {model.ShippingName}</p>
                <p>Số điện thoại: {model.ShippingPhone}</p>
                <p>Địa chỉ: {model.ShippingAddress}, {model.ShippingWard}, {model.ShippingDistrict}, {model.ShippingProvince}</p>
                <h3>Chi tiết đơn hàng:</h3>
                <p>Tổng tiền hàng: {grandTotal.ToString("#,##0")} VND</p>
                <p>Phí vận chuyển: {model.ShippingCost.ToString("#,##0")} VND</p>
                <p><strong>Tổng thanh toán: {amountToPay.ToString("#,##0")} VND</strong></p>
                <p><strong>Phương thức thanh toán: {model.PaymentMethod}</strong></p>
            ";

            await _emailSender.SenderEmailAsync(userEmail, "Xác nhận đơn hàng", emailBody);

            TempData["success"] = "Đặt hàng thành công! Đơn hàng đang được xử lý.";
            return RedirectToAction("Index", "Home");
        }

        private async Task<string> CreateMomoPaymentUrlAsync(OrderModel order, decimal amount)
        {
            var cfg = _configuration.GetSection("PaymentProviders:Momo");
            var endpoint = cfg["Endpoint"];
            var partnerCode = cfg["PartnerCode"];
            var accessKey = cfg["AccessKey"];
            var secretKey = cfg["SecretKey"];
            var returnUrl = cfg["ReturnUrl"];
            var notifyUrl = cfg["NotifyUrl"];

            var requestId = Guid.NewGuid().ToString();
            var orderId = order.OrderCode;
            var orderInfo = $"Thanh toan don hang {order.OrderCode}";
            var amountStr = ((long)amount).ToString(); // Momo expects integer string (VND)

            // Momo request signature ordering (official example)
            var rawHash = $"accessKey={accessKey}&amount={amountStr}&extraData=&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType=captureWallet";
            var signature = HmacSHA256(rawHash, secretKey);

            var payload = new
            {
                partnerCode = partnerCode,
                accessKey = accessKey,
                requestId = requestId,
                amount = amountStr,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                extraData = "",
                requestType = "captureWallet",
                signature = signature
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            var json = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("payUrl", out var payUrlElement))
                {
                    order.PaymentTransactionId = requestId;
                    order.PaymentStatus = PaymentStatus.Pending;
                    _dataContext.Orders.Update(order);
                    await _dataContext.SaveChangesAsync();

                    return payUrlElement.GetString();
                }
            }
            catch
            {
            }

            return null;
        }

        private string CreateVnPayUrl(OrderModel order, decimal amount)
        {
            var cfg = _configuration.GetSection("PaymentProviders:VnPay");
            var baseUrl = cfg["Endpoint"]?.TrimEnd('/') ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var tmnCode = cfg["TmnCode"];
            var hashSecret = cfg["HashSecret"];
            var returnUrl = cfg["ReturnUrl"];

            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", ((long)(amount * 100)).ToString() },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", order.OrderCode },
                { "vnp_OrderInfo", $"Thanh toan don hang {order.OrderCode}" },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1" }
            };

            var signData = string.Join("&", vnpParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var vnpSecureHash = HmacSHA512(signData, hashSecret);

            var query = string.Join("&", vnpParams.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
            query += $"&vnp_SecureHash={WebUtility.UrlEncode(vnpSecureHash)}&vnp_SecureHashType=HmacSHA512";

            order.PaymentTransactionId = order.OrderCode;
            order.PaymentStatus = PaymentStatus.Pending;
            _dataContext.Orders.Update(order);
            _dataContext.SaveChanges();

            return $"{baseUrl}?{query}";
        }

        private static string HmacSHA256(string message, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? string.Empty));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message ?? string.Empty));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static string HmacSHA512(string message, string secret)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret ?? string.Empty));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message ?? string.Empty));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}

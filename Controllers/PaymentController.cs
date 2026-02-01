using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using ecommerce_shopping.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ecommerce_shopping.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        private readonly IStatisticalService _statisticalService;

        public PaymentController(DataContext dataContext, IConfiguration configuration, IStatisticalService statisticalService)
        {
            _dataContext = dataContext;
            _configuration = configuration;
            _statisticalService = statisticalService;
        }

        // ✅ VNPay redirect return (customer)
        [HttpGet]
        [Route("payment/vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var query = Request.Query;
                var vnpSecureHash = query["vnp_SecureHash"].ToString();
                var vnpTxnRef = query["vnp_TxnRef"].ToString();
                var vnpResponseCode = query["vnp_ResponseCode"].ToString();

                System.Diagnostics.Debug.WriteLine($"VNPay Return - TxnRef: {vnpTxnRef}, ResponseCode: {vnpResponseCode}");

                var dict = new SortedDictionary<string, string>();
                foreach (var key in query.Keys)
                {
                    if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                    {
                        dict.Add(key, query[key]);
                    }
                }

                var signData = string.Join("&", dict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var secret = _configuration["PaymentProviders:VnPay:HashSecret"];
                var checkSum = HmacSHA512(signData, secret);

                if (string.Equals(checkSum, vnpSecureHash, StringComparison.OrdinalIgnoreCase) && vnpResponseCode == "00")
                {
                    // ✅ Query order và orderDetails riêng
                    var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == vnpTxnRef);

                    if (order != null && order.PaymentStatus != PaymentStatus.Paid)
                    {
                        // ✅ Lấy OrderDetails theo OrderCode
                        var orderDetails = await _dataContext.OrderDetails
                            .Where(od => od.OrderCode == vnpTxnRef)
                            .ToListAsync();

                        // ✅ CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
                        order.PaymentStatus = PaymentStatus.Paid;
                        order.status = 1;
                        order.PaymentDate = DateTime.Now;

                        // ✅ TRỪ SỐ LƯỢNG VÀ TĂNG SOLD
                        foreach (var orderDetail in orderDetails)
                        {
                            var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == orderDetail.ProductId);
                            if (product != null)
                            {
                                product.Quantity -= orderDetail.Quantity;
                                product.Sold += orderDetail.Quantity;
                                _dataContext.Products.Update(product);
                            }

                            // ✅ TẠO STATISTICAL
                            await _statisticalService.CreateStatisticalAsync(
                                orderDetail,
                                orderStatus: 1,
                                couponCode: order.CouponCode,
                                discountAmount: order.PriceCoupon
                            );
                        }

                        _dataContext.Orders.Update(order);
                        await _dataContext.SaveChangesAsync();
                    }

                    return Ok("VNPAY Return URL verified successfully.");
                }
                else
                {
                    // ✅ THANH TOÁN THẤT BẠI - XÓA ĐƠN HÀNG
                    var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == vnpTxnRef);

                    if (order != null)
                    {
                        var orderDetails = await _dataContext.OrderDetails
                            .Where(od => od.OrderCode == vnpTxnRef)
                            .ToListAsync();

                        _dataContext.OrderDetails.RemoveRange(orderDetails);
                        _dataContext.Orders.Remove(order);
                        await _dataContext.SaveChangesAsync();
                    }

                    return Ok("VNPAY Return URL verified successfully.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VNPay Return Error: {ex.Message}");
                return RedirectToAction("Failure");
            }
        }

        // ✅ VNPay IPN (server-to-server)
        [HttpPost]
        [Route("payment/vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var form = Request.Form;
            var vnpSecureHash = form["vnp_SecureHash"].ToString();
            var vnpTxnRef = form["vnp_TxnRef"].ToString();
            var vnpResponseCode = form["vnp_ResponseCode"].ToString();

            var dict = new SortedDictionary<string, string>();
            foreach (var key in form.Keys)
            {
                var keyStr = key.ToString();
                if (keyStr.StartsWith("vnp_") && keyStr != "vnp_SecureHash" && keyStr != "vnp_SecureHashType")
                {
                    dict.Add(keyStr, form[keyStr]);
                }
            }

            var signData = string.Join("&", dict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var secret = _configuration["PaymentProviders:VnPay:HashSecret"];
            var checkSum = HmacSHA512(signData, secret);

            if (string.Equals(checkSum, vnpSecureHash, StringComparison.OrdinalIgnoreCase) && vnpResponseCode == "00")
            {
                var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == vnpTxnRef);

                if (order != null && order.PaymentStatus != PaymentStatus.Paid)
                {
                    var orderDetails = await _dataContext.OrderDetails
                        .Where(od => od.OrderCode == vnpTxnRef)
                        .ToListAsync();

                    order.PaymentStatus = PaymentStatus.Paid;
                    order.status = 1;
                    order.PaymentDate = DateTime.Now;

                    foreach (var orderDetail in orderDetails)
                    {
                        var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == orderDetail.ProductId);
                        if (product != null)
                        {
                            product.Quantity -= orderDetail.Quantity;
                            product.Sold += orderDetail.Quantity;
                            _dataContext.Products.Update(product);
                        }

                        await _statisticalService.CreateStatisticalAsync(
                            orderDetail,
                            orderStatus: 1,
                            couponCode: order.CouponCode,
                            discountAmount: order.PriceCoupon
                        );
                    }

                    _dataContext.Orders.Update(order);
                    await _dataContext.SaveChangesAsync();
                }

                return Content("OK");
            }

            return Content("Invalid signature");
        }

        // ✅ Momo redirect return (customer)
        [HttpGet]
        [Route("payment/momo-return")]
        public async Task<IActionResult> MomoReturn()
        {
            try
            {
                var orderId = Request.Query["orderId"].ToString();
                var resultCode = Request.Query["resultCode"].ToString();

                System.Diagnostics.Debug.WriteLine($"Momo Return - OrderId: {orderId}, ResultCode: {resultCode}");

                if (!string.IsNullOrEmpty(orderId) && resultCode == "0")
                {
                    var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderId);

                    if (order != null && order.PaymentStatus != PaymentStatus.Paid)
                    {
                        var orderDetails = await _dataContext.OrderDetails
                            .Where(od => od.OrderCode == orderId)
                            .ToListAsync();

                        // ✅ CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
                        order.PaymentStatus = PaymentStatus.Paid;
                        order.status = 1;
                        order.PaymentDate = DateTime.Now;

                        // ✅ TRỪ SỐ LƯỢNG VÀ TĂNG SOLD
                        foreach (var orderDetail in orderDetails)
                        {
                            var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == orderDetail.ProductId);
                            if (product != null)
                            {
                                product.Quantity -= orderDetail.Quantity;
                                product.Sold += orderDetail.Quantity;
                                _dataContext.Products.Update(product);
                            }

                            // ✅ TẠO STATISTICAL
                            await _statisticalService.CreateStatisticalAsync(
                                orderDetail,
                                orderStatus: 1,
                                couponCode: order.CouponCode,
                                discountAmount: order.PriceCoupon
                            );
                        }

                        _dataContext.Orders.Update(order);
                        await _dataContext.SaveChangesAsync();
                    }

                    return RedirectToAction("Success");
                }
                else
                {
                    // ✅ THANH TOÁN THẤT BẠI - XÓA ĐƠN HÀNG
                    var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderId);

                    if (order != null)
                    {
                        var orderDetails = await _dataContext.OrderDetails
                            .Where(od => od.OrderCode == orderId)
                            .ToListAsync();

                        _dataContext.OrderDetails.RemoveRange(orderDetails);
                        _dataContext.Orders.Remove(order);
                        await _dataContext.SaveChangesAsync();
                    }

                    return RedirectToAction("Failure");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Momo Return Error: {ex.Message}");
                return RedirectToAction("Failure");
            }
        }

        // ✅ Momo notify (server-to-server)
        [HttpPost]
        [Route("payment/momo-notify")]
        public async Task<IActionResult> MomoNotify()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            System.Diagnostics.Debug.WriteLine($"Momo Notify Body: {body}");

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                string partnerCode = root.TryGetProperty("partnerCode", out var p1) ? p1.GetString() : "";
                string accessKey = root.TryGetProperty("accessKey", out var p2) ? p2.GetString() : "";
                string requestId = root.TryGetProperty("requestId", out var p3) ? p3.GetString() : "";
                string amount = root.TryGetProperty("amount", out var p4) ? p4.GetString() : "";
                string orderId = root.TryGetProperty("orderId", out var p5) ? p5.GetString() : "";
                string orderInfo = root.TryGetProperty("orderInfo", out var p6) ? p6.GetString() : "";
                string orderType = root.TryGetProperty("orderType", out var p7) ? p7.GetString() : "";
                string transId = root.TryGetProperty("transId", out var p8) ? p8.GetString() : "";
                string message = root.TryGetProperty("message", out var p9) ? p9.GetString() : "";
                string localMessage = root.TryGetProperty("localMessage", out var p10) ? p10.GetString() : "";
                string responseTime = root.TryGetProperty("responseTime", out var p11) ? p11.GetString() : "";
                string errorCode = root.TryGetProperty("errorCode", out var p12) ? p12.GetRawText().Trim('"') : "";
                string payType = root.TryGetProperty("payType", out var p13) ? p13.GetString() : "";
                string extraData = root.TryGetProperty("extraData", out var p14) ? p14.GetString() : "";
                string signature = root.TryGetProperty("signature", out var p15) ? p15.GetString() : "";

                var rawList = new List<string>
                {
                    $"partnerCode={partnerCode}",
                    $"accessKey={accessKey}",
                    $"requestId={requestId}",
                    $"amount={amount}",
                    $"orderId={orderId}",
                    $"orderInfo={orderInfo}",
                    $"orderType={orderType}",
                    $"transId={transId}",
                    $"message={message}",
                    $"localMessage={localMessage}",
                    $"responseTime={responseTime}",
                    $"errorCode={errorCode}",
                    $"payType={payType}",
                    $"extraData={extraData}"
                };
                var rawHash = string.Join("&", rawList);

                var secret = _configuration["PaymentProviders:Momo:SecretKey"];
                var computed = HmacSHA256(rawHash, secret);

                if (string.Equals(computed, signature, StringComparison.OrdinalIgnoreCase) && errorCode == "0")
                {
                    var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderId);

                    if (order != null && order.PaymentStatus != PaymentStatus.Paid)
                    {
                        var orderDetails = await _dataContext.OrderDetails
                            .Where(od => od.OrderCode == orderId)
                            .ToListAsync();

                        order.PaymentStatus = PaymentStatus.Paid;
                        order.status = 1;
                        order.PaymentTransactionId = transId;
                        order.PaymentDate = DateTime.Now;

                        foreach (var orderDetail in orderDetails)
                        {
                            var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == orderDetail.ProductId);
                            if (product != null)
                            {
                                product.Quantity -= orderDetail.Quantity;
                                product.Sold += orderDetail.Quantity;
                                _dataContext.Products.Update(product);
                            }

                            await _statisticalService.CreateStatisticalAsync(
                                orderDetail,
                                orderStatus: 1,
                                couponCode: order.CouponCode,
                                discountAmount: order.PriceCoupon
                            );
                        }

                        _dataContext.Orders.Update(order);
                        await _dataContext.SaveChangesAsync();
                    }

                    return Ok(new { resultCode = 0, message = "Confirm Success" });
                }

                return BadRequest(new { resultCode = -1, message = "Invalid signature or payment failed" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Momo Notify Error: {ex.Message}");
                return BadRequest(new { resultCode = -2, message = "Invalid payload" });
            }
        }

        // ✅ Success page
        [HttpGet]
        [Route("payment/success")]
        public IActionResult Success()
        {
            ViewBag.Message = "Thanh toán thành công!";
            ViewBag.Description = "Cảm ơn bạn đã đặt hàng. Đơn hàng của bạn đang được xử lý.";
            return View();
        }

        // ✅ Failure page
        [HttpGet]
        [Route("payment/failure")]
        public IActionResult Failure()
        {
            ViewBag.Message = "Thanh toán thất bại!";
            ViewBag.Description = "Đã có lỗi xảy ra trong quá trình thanh toán. Vui lòng thử lại.";
            return View();
        }

        private static string HmacSHA256(string message, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? string.Empty));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message ?? string.Empty));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static string HmacSHA512(string data, string key)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key ?? string.Empty));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
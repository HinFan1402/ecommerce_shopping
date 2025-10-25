using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Contact")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ContactController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("Index")]
        public IActionResult Index()
        {
            return View(_dataContext.Contacts.ToList());
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contacts.FirstOrDefaultAsync();
            return View(contact);
        }
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            ContactModel contactEdit = await _dataContext.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Name == contact.Name);
            

            if (ModelState.IsValid)
            {
                if (contact.logoImgUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.logoImgUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await contact.logoImgUpload.CopyToAsync(fs);
                    fs.Close();
                    contact.logoImgs = imageName;
                    string oldFilePath = Path.Combine(uploadsDir, contactEdit.logoImgs);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                else
                {
                    contact.logoImgs = contactEdit.logoImgs;
                }

                _dataContext.Update(contact);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update product successfull";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model has something wrong!";
                return View(contact); // 👈 Trả lại View để hiển thị lỗi và giữ data
            }
        }
    }
}

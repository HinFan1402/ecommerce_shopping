using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModels;
using ecommerce_shopping.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Slider")]
    [Authorize(Roles = "Admin,Publisher")]
    public class SliderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public SliderController(DataContext dataContext, IWebHostEnvironment webHostEnviroment)
        {
            _dataContext = dataContext;
            _webHostEnviroment = webHostEnviroment;
        }
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<SliderModel> allSliders = await _dataContext.Sliders.ToListAsync();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }

            int recsCount = allSliders.Count();
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = allSliders.Skip(recSkip).Take(pager.PageSize).ToList();

            var viewModel = new TPaginateViewModel<SliderModel>
            {
                Items = data,
                Pager = pager
            };

            // Lấy active sliders       
            return View(viewModel);
        }
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(SliderModel slider)
        {
            if (ModelState.IsValid)
            {
                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider.Image = imageName;
                }
                else { Console.WriteLine("ImageUpload IS NULL"); }

                _dataContext.Add(slider);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Create Slider successfull";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model has something wrong!";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
           
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id, SliderModel slider)
        {
            SliderModel SliderEdit = await _dataContext.Sliders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
           

            if (ModelState.IsValid)
            {
                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "images");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider.Image = imageName;
                    string oldFilePath = Path.Combine(uploadsDir, SliderEdit.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                else
                {
                    slider.Image = SliderEdit.Image;
                }

                _dataContext.Update(slider);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Slider successfull";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model has something wrong!";
                return View(slider); // 👈 Trả lại View để hiển thị lỗi và giữ data
            }
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(id);
            if (string.Equals(slider.Image, "noimage.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnviroment.WebRootPath, "images");
                string filePath = Path.Combine(uploadsDir, slider.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _dataContext.Remove(slider);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Delete Slider successfull";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var slider = await _dataContext.Sliders.FirstOrDefaultAsync(s=> s.Id== id);

            if (slider == null)
            {
                return NotFound();
            }
            if (slider.Status == 1)
            {
                slider.Status = 0;
            }
            else 
            {
                slider.Status= 1;
            }
                

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Slider status updated successfully" });
            }
            catch (Exception)
            {


                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }
    }
}

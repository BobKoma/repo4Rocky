using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Packaging.Signing;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;

namespace Rocky.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objList = _db.Products;

            //foreach (var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(u => u.CategoryId == obj.CategoryId);
            //}

            return View(objList);
        }

        public IActionResult Upsert(int? id)
        {
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem { 
            //    Text=i.Name,
            //    Value = i.CategoryId.ToString(),
            //});

            ////ViewBag.CategoryDropDown = CategoryDropDown;
            //ViewData["CategoryDropDown"] = CategoryDropDown;

            //Product product = new Product();
            ProductVM productVM = new ProductVM()
            { 
                Product = new Product(),
                CategorySelectList = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.CategoryId.ToString(),
                }),
                ApplicationTypeSelectList = _db.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                })
            };

            if (id == null )
            {
                //this is for create
                //return View(product);
                return View(productVM);
            }
            else
            {
                //product = _db.Products.Find(id);
                productVM.Product = _db.Products.Find(id);
                if (productVM.Product == null ) 
                { 
                    return NotFound(); 
                }
                return View(productVM);
            }
        }

        //POST UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            //IEnumerable<ApplicationType> objList = _db.ApplicationType ;

            //if (obj.Product.ApplicationTypeId == null || obj.Product.ApplicationTypeId == 0)
            //{
            //    obj.Product.ApplicationTypeId = 1; // _db.ApplicationType.FirstOrDefault(u => u.Id == objList. FirstOrDefault(defaultValue:);
            //}
            if (ModelState.IsValid)
            {
                //_db.Products.Add(obj.Product);
                //_db.SaveChanges();

                //The number of state entries written to the database.
                Int32 entries = 0;

                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.ProductId == 0)
                {
                    //Create
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString(); //Path.GetFileName(upload);
                    string extension = Path.GetExtension(files[0].FileName).ToLower();

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName+extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;

                    _db.Products.Add(productVM.Product);
                }

                entries = _db.SaveChanges() ;
                return RedirectToAction("Index");
            }
            return View(productVM);

        }
    }
}

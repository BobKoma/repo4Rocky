using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
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
           // Eager-loading
            IEnumerable<Product> objList = _db.Products.Include(u => u.Category).Include(u => u.ApplicationType);

            // Old Method
            //IEnumerable<Product> objList = _db.Products;

            //foreach (var obj in objList)
            //{
            //    obj.Category        = _db.Category.FirstOrDefault(u => u.CategoryId == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(u => u.Id == obj.ApplicationTypeId);
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

            if (id == null)
            {
                //this is for create
                //return View(product);
                return View(productVM);
            }
            else
            {
                //product = _db.Products.Find(id);
                productVM.Product = _db.Products.Find(id);
                if (productVM.Product == null)
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

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;

                    _db.Products.Add(productVM.Product);
                }
                else
                {
                    //update
                    // Used to get old file-name from objFromdb object.
                    // ATTENTION! Use <AsNoTracking()> cuz two models in code: productVM.Product & objFromdb    
                    var objFromdb = _db.Products.AsNoTracking().FirstOrDefault(u => u.ProductId == productVM.Product.ProductId);
                    //var objFromdb2 = productVM.Product;

                    if (files.Count() > 0) 
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString(); //Path.GetFileName(upload);
                        string extension = Path.GetExtension(files[0].FileName).ToLower();

                        var oldFile = Path.Combine(upload, objFromdb.Image); 
                        if (System.IO.File.Exists(oldFile)) 
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromdb.Image;
                    }
                    _db.Products.Update(productVM.Product); 
                }

                entries = _db.SaveChanges();
                return RedirectToAction("Index");
            }
            // Fill Category & AppType to show in productVM-ViewModel if IsValid = False:
            productVM.
                CategorySelectList = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.CategoryId.ToString(),
                });
            productVM.
                ApplicationTypeSelectList = _db.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                });
            return View(productVM);

        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }


            // 1. Classic method
            //var product = _db.Products.Find(id);
            // 2. Eager Loading Category & AppType
            var product = _db.Products
                .Include(u => u.Category).Include(u => u.ApplicationType).FirstOrDefault(u => u.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }
            //Make Category And AppType Visible
            // 1. Classic method used earlier versa eager-loading
            //product.Category = _db.Category.Find(product.CategoryId);
            //product.ApplicationType = _db.ApplicationType.Find(product.ApplicationTypeId);
            return View(product);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? ProductId)
        {
            var obj = _db.Products.Find(ProductId);
            if (obj == null)
            {
                return NotFound();
            }

            // Remove product-image from images-folder
            if (obj.Image != null)
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                string curImage = Path.Combine(webRootPath + WC.ImagePath, obj.Image);

                if (System.IO.File.Exists(curImage))
                {
                    System.IO.File.Delete(curImage);
                }
            }

            _db.Products.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}

﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperShop.Data;
using SuperShop.Helpers;
using SuperShop.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SuperShop.Controllers
{
   
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserHelper _userHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;

        public ProductsController(IProductRepository productRepository,
            IUserHelper userHelper,
            IImageHelper imageHelper,
            IConverterHelper converterHelper)
        {
            _productRepository = productRepository;
            _userHelper = userHelper;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
        }

        // GET: Products
        public IActionResult Index()
        {
            return View(_productRepository.GetAll().OrderBy(p => p.Name));
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var product = await _productRepository.GetByIdAsync(id.Value);

            if (product is null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            return View(product);
        }

        // GET: Products/Create
        //[Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var path = string.Empty;

                if (model.ImageFile is not null && model.ImageFile.Length > 0)
                {
                    path = await _imageHelper.UploadImageAsync(model.ImageFile, "products");
                }

                var product = _converterHelper.ToProduct(model,path,true);

                product.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                await _productRepository.CreateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        /*  private Product ToProduct(ProductViewModel model, string path)
          {
              return new Product()
              {
                  Id = model.Id,
                  ImageUrl = path,
                  IsAvailable = model.IsAvailable,
                  LastPurchase = model.LastPurchase,
                  LastSale = model.LastSale,
                  Name = model.Name,
                  Price = model.Price,
                  Stock = model.Stock,
                  User = model.User
              };
          }*/

        // GET: Products/Edit/5
        [Authorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product is null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var model = _converterHelper.ToProductViewModel(product);
            return View(model);
        }

       /* private ProductViewModel ToProductViewModel(Product product)
        {
            return new ProductViewModel()
            {
                Id = product.Id,
                IsAvailable = product.IsAvailable,
                LastPurchase = product.LastPurchase,
                LastSale = product.LastSale,
                ImageUrl = product.ImageUrl,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                User = product.User
            };
        }*/

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var path = model.ImageUrl;

                    if (model.ImageFile is not null && model.ImageFile.Length > 0)
                    {                     
                        path = await _imageHelper.UploadImageAsync(model.ImageFile, "products");
                    }

                    var product = _converterHelper.ToProduct(model, path, false);


                    product.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                    await _productRepository.UpdateAsync(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _productRepository.ExistsAsync(model.Id))
                    {
                        return new NotFoundViewResult("ProductNotFound");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Products/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product is null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            try
            {              
                await _productRepository.DeleteAsync(product);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is not null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"{product.Name} provavelmente está a ser usado!";
                    ViewBag.ErrorMessage = $"{product.Name} não pode ser apagado visto haverem encomendas que o usam.</br></br>" +
                            $"Experimente primeiro apagar todas as encomendas que o estão a usar," +
                            $"e torne novamente a apagá-lo";
                }

                return View("Error");
            }
         
        }

        public IActionResult ProductNotFound()
        {
            return View();
        }
    }
}

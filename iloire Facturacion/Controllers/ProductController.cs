﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcPaging;

namespace CoffeeInvoice.Controllers
{
	public class ProductController : Controller
	{
		private InvoiceDB db = new InvoiceDB();
		private int defaultPageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DefaultPaginationSize"]);

		private decimal _rate;
		private decimal AUDCNYRate
		{
			get { return _rate; }			 
		}

		// GET: /Provider/
		public ViewResult Index(int? page)
		{
			CurrencyConvertYahooController rateController = new CurrencyConvertYahooController();
			_rate = rateController.ConvertCurrency("AUD", "CNY", 1);

			var products = db.Products.OrderBy(i => i.ProductName).Include("Provider").ToList();
			foreach (var pro in products)
			{
				pro.CNYPrice = pro.Price * AUDCNYRate;
			}

			int currentPageIndex = page.HasValue ? page.Value - 1 : 0;

			var productsListPaged = products.ToPagedList(currentPageIndex, defaultPageSize);
			return View(productsListPaged);
		}

		public ViewResult Create()
		{
			var providers = db.Providers.OrderBy(x => x.Name).ToList();

			List<SelectListItem> items = new List<SelectListItem>();

			foreach(var pro in providers)
			{
				SelectListItem item = new SelectListItem();
				item.Text = pro.Name;
				item.Value = pro.ProviderID.ToString();

				items.Add(item);
			}

			ViewBag.providers = items;
			return View();
		}

		[HttpPost]
		public ActionResult Create(Product product, int? providers)
		{
			if (ModelState.IsValid)
			{
				if (providers.HasValue && providers.Value > 0)
				{
					product.ProviderID = providers.Value;
				}
				else
					product.ProviderID = -1;

				db.Products.Add(product);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(product);
		}

		// Get Proeuct with id 
		public ActionResult Details(int id)
		{
			Product product = db.Products.Find(id);
			return View(product);
		}

		[HttpPost]
		public ActionResult Edit(Product product)
		{
			if (ModelState.IsValid)
			{
				db.Entry<Product>(product).State = EntityState.Modified;
				
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			ViewBag.ProviderID = new SelectList(db.Providers.OrderBy(x=>x.Name), "ProviderID", "Name", product.ProviderID);
			return View(product);
		}

		public ActionResult Edit(int id)
		{
			Product product = db.Products.Find(id);
			CurrencyConvertYahooController rateController = new CurrencyConvertYahooController();

			_rate = rateController.ConvertCurrency("AUD", "CNY", 1);
			if (!product.CNYPrice.HasValue)
			{
				if (product.Price > 0)
					product.CNYPrice = product.Price * AUDCNYRate;

			}

			ViewBag.ProviderID = new SelectList(db.Providers.OrderBy(x=>x.Name), "ProviderID","Name", product.ProviderID);
			return View(product);
		}		 
	}	 
}
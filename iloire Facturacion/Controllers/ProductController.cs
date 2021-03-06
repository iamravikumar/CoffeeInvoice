﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcPaging;
using Microsoft.International.Converters.PinYinConverter;

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

		public ActionResult Copy(int id)
		{
			Product source = db.Products.Find(id);
			if(source !=null)
			{
				Product p = new Product();
				p.ProductName = source.ProductName;
				p.Price = source.Price;
				p.CNYSellPrice = source.CNYSellPrice;
				p.CNYPrice = source.CNYPrice;
				//p.Provider = source.Provider;
				p.ProviderID = source.ProviderID;
				p.UserID = source.UserID;
				db.Products.Add(p);
				db.SaveChanges();
			}

			return RedirectToAction("Index");
		}

		public ViewResultBase Search(string q, int? page)
		{
			q = q.ToUpper();

			List<CoffeeInvoice.Models.ViewModel.ProductViewModel> productsVM = new List<Models.ViewModel.ProductViewModel>();
			if (Session["LoginUser"] != null)
			{
				User user = (User)Session["LoginUser"];
				var products = db.Products.Where(x => x.UserID == user.UserID).OrderBy(x => x.ProductID).ToList();
				foreach (var pro in products)
				{
					CoffeeInvoice.Models.ViewModel.ProductViewModel proVM = new Models.ViewModel.ProductViewModel();

					proVM.ProductID = pro.ProductID;
					proVM.ProductName = pro.ProductName;
					
					string r = string.Empty;

					foreach (char obj in proVM.ProductName)
					{
						try
						{
							ChineseChar chineseChar = new ChineseChar(obj);
							string t = chineseChar.Pinyins[0].ToString();
							r += t.Substring(0, 1);
						}
						catch
						{
							r += obj.ToString();
						}
					}

					if (q.Length == 1)//alphabetical search, first letter
					{
						ViewBag.LetraAlfabetica = q;
						if (r.StartsWith(q) || proVM.ProductName.ToUpper().StartsWith(q))
						{
							proVM.Provider = pro.Provider;
							proVM.ProviderID = pro.ProviderID;
							proVM.Price = pro.Price;
							proVM.CNYSellPrice = pro.CNYSellPrice;
							proVM.CNYPrice = pro.Price * AUDCNYRate;
							proVM.UserID = pro.UserID;
							productsVM.Add(proVM);
						}
					}
					else if (q.Length > 1)
					{
						//normal search
						if (r.IndexOf(q) > -1 || proVM.ProductName.ToUpper().IndexOf(q) >-1)
						{
							proVM.Provider = pro.Provider;
							proVM.ProviderID = pro.ProviderID;
							proVM.Price = pro.Price;
							proVM.CNYSellPrice = pro.CNYSellPrice;
							proVM.CNYPrice = pro.Price * AUDCNYRate;
							proVM.UserID = pro.UserID;
							productsVM.Add(proVM);
						}
					}					
				}
			}

			int currentPageIndex = page.HasValue ? page.Value - 1 : 0;

			var productsListPaged = productsVM.ToPagedList(currentPageIndex, defaultPageSize);

			if (Request.IsAjaxRequest())
			{
				return PartialView("Index", productsListPaged);
			}
			else
				return View("Index",productsListPaged);
		}
		 
		public ViewResult Index(int? page)
		{
			CurrencyConvertYahooController rateController = new CurrencyConvertYahooController();
			_rate = rateController.ConvertCurrency("AUD", "CNY", 1);

			var productsVM = new List<CoffeeInvoice.Models.ViewModel.ProductViewModel>();
			
			if (Session["LoginUser"] != null)
			{
				User user = (User)Session["LoginUser"];
				var products = db.Products.OrderBy(i => i.ProductName).Include("Provider").ToList();
				products = products.Where(x => x.UserID == user.UserID).ToList();
				foreach (var pro in products)
				{
					CoffeeInvoice.Models.ViewModel.ProductViewModel proVM = new Models.ViewModel.ProductViewModel();

					proVM.ProductID = pro.ProductID;
					proVM.ProductName = pro.ProductName;
					proVM.Provider = pro.Provider;
					proVM.ProviderID = pro.ProviderID;
					proVM.Price = pro.Price;
					proVM.CNYSellPrice = pro.CNYSellPrice;
					proVM.CNYPrice = pro.Price * AUDCNYRate;
					proVM.UserID = pro.UserID;

					if (db.PurchaseProducts.Where(x => x.ProductID == pro.ProductID).Count() > 0)
					{
						proVM.IsProductPurchased = true;
					}
					else
						proVM.IsProductPurchased = false;

					productsVM.Add(proVM);
				}
			}

			int currentPageIndex = page.HasValue ? page.Value - 1 : 0;

			var productsListPaged = productsVM.ToPagedList(currentPageIndex, defaultPageSize);
			return View(productsListPaged);
		}

		public ViewResult Create()
		{
			if (Session["LoginUser"] != null)
			{
				User user = (User)Session["LoginUser"];

				var providers = db.Providers.Where(x=>x.UserID == user.UserID).OrderBy(x => x.Name).ToList();

				List<SelectListItem> items = new List<SelectListItem>();

				foreach (var pro in providers)
				{
					SelectListItem item = new SelectListItem();
					item.Text = pro.Name;
					item.Value = pro.ProviderID.ToString();

					items.Add(item);
				}

				ViewBag.providers = items;
			}
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

				if (Session["LoginUser"] != null)
				{
					product.UserID = ((User)Session["LoginUser"]).UserID;
				}
				
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
				product.UserID = ((User)Session["LoginUser"]).UserID;
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			if (Session["LoginUser"] != null)
			{
				User user = (User)Session["LoginUser"];
			
				ViewBag.ProviderID = new SelectList(db.Providers.OrderBy(x=>x.Name), "ProviderID", "Name", product.ProviderID);
			}
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

			if (Session["LoginUser"] != null)
			{
				User user = (User)Session["LoginUser"];				
				ViewBag.ProviderID = new SelectList(db.Providers.Where(x => x.UserID == user.UserID).OrderBy(x => x.Name), "ProviderID", "Name", product.ProviderID);
			}
			return View(product);
		}

		public PartialViewResult MostPopularProducts(int top)
		{
			List<PopularProduct> products = new List<PopularProduct>();
			if (Session["LoginUser"] != null)
			{
				User user = (User)Session["LoginUser"];
				var singleTranMostPopularProds =
					db.Transactions.Where(x => x.UserID == user.UserID).GroupBy(x => new { ProductID=x.ProductID, Product=x.Product.ProductName}).Select(x => new PopularProduct { ProductID = x.Key.ProductID,Product=x.Key.Product, TotalValue = x.Sum(y => y.Product.CNYSellPrice.Value), Count = x.Count() }).ToList();

				List<PopularProduct> comboTransPopularProds = GetComboTransactionPopularProducts(user.UserID);
				//products = comboTransPopularProds.OrderByDescending(x => x.TotalValue).ToList();	
				
				products.AddRange(singleTranMostPopularProds.OrderByDescending(x => x.TotalValue).ToList());
				foreach (PopularProduct cPP in comboTransPopularProds)
				{
					PopularProduct pp = products.Where(x => x.ProductID == cPP.ProductID).FirstOrDefault();

					if (pp != null)
					{
						pp.Count += cPP.Count;
						pp.TotalValue += cPP.TotalValue;
					}
					else
					{
						products.Add(cPP);
					}
				}
			}

			return PartialView("TopProductsPartial", products.OrderByDescending(x=>x.TotalValue).Take(top));
		}

		private List<PopularProduct> GetComboTransactionPopularProducts(int userID)
		{
			List<int> comboTranIDs = db.ComboTransactions.Where(x=>x.UserID == userID).Select(x=>x.ComboTransactionID).ToList();

			List<IndividualProductTransaction> individualTrans =
				db.IndividualProductTransactions.Where(x => comboTranIDs.Contains(x.ComboTransactionID)).ToList();

			List<PopularProduct> rtn = 
				individualTrans.GroupBy(
				x => new { ProductID = x.ProductID, Product = x.Product.ProductName }).
					Select(
					x => new PopularProduct { ProductID = x.Key.ProductID, Product = x.Key.Product, TotalValue = x.Sum(y=>y.CNYSellPrice.Value*y.Number), Count = x.Sum(y=>y.Number)}).ToList();

			return rtn;
		}

		public ActionResult Delete(int id)
		{
			Product product = db.Products.Find(id);
			return View(product);
		}
	}	 
}
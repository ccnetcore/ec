using CC.ElectronicCommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.PageDetail.Controllers
{
	public class PageDetaiController : Controller
	{
        private IPageDetailService _pageDetailService;
        public PageDetaiController(IPageDetailService pageDetailService)
        {
            _pageDetailService = pageDetailService;
        }
		[Route("/item/{id}.html")]
        public IActionResult Index(long id)
        {
            var htmlmodel = _pageDetailService.loadModel(id);

			return View(htmlmodel);
        }
    }
}

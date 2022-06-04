using System;
using Microsoft.AspNetCore.Mvc;

namespace QuantifulStocksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	public class BaseController : ControllerBase
	{
		public BaseController()
		{
		}
	}
}


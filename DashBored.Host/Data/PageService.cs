using DashBored.Host.Models;

namespace DashBored.Host.Data
{
	public class PageService
	{
		public PageService(List<Page> pages)
		{
			Pages = pages;
		}
		
		public List<Page> Pages { get; private set; }
	}
}

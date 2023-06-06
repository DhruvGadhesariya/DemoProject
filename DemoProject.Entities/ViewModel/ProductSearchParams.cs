using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Entities.ViewModel
{
    public class ProductSearchParams
    {
        public string Name { get; set; }
        public int Pg { get; set; }
        public string Finder { get; set; }
        public string Sort { get; set; }
        public int PageSize { get; set; }
    }
}

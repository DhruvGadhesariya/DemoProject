using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Entities.ViewModel
{
    public class PageList<T> where T : class
    {

        public PageList(List<T> records, int totalCount)
        {
            Records = records;
            TotalCount = totalCount;
        }


        public List<T> Records { get; set; }

        public int TotalCount { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace DemoProject.Entities.ViewModel
{
    public class UserSearchParams
    {
        public string Search { get; set; } = string.Empty;

        public int Pg { get; set; } 

        public string Finder { get; set; } = string.Empty;

        public string Sort { get; set; } = string.Empty;

    }
}

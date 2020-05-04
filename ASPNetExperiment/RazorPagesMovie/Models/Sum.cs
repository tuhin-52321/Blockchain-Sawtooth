using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RazorPagesMovie.Models
{
  
    
    public class Sum
    {

        [Display(Name = "First Number")]
        public int FirstNumber { get; set; }

        [Display(Name = "Second Number")]
        public int SecondNumber { get; set; }
    }
}

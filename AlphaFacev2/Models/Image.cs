using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlphaFacev2.Models
{
    public class Image
    {
        public byte[] ImageByteArray { get; set; }
        public string ImagePath { get; set; }
        public string ImageBase64 { get; set; }
        public string ImageSource { get; set; }
    }
}

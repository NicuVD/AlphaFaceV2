using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlphaFacev2.Models
{
    public class VerifiedFace
    {
        public bool IsIdentical { get; set; }
        public float Confidence { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewPattern
{
    public class Dolphin:Animal,IAnimal
    {
        public int Depth { get; set; }
        public override void Voice()
        {
            base.Voice();
        }
    }
}

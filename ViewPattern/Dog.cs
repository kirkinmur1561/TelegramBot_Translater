using System;
using System.Diagnostics;
namespace ViewPattern
{
    public class Dog:Animal
    {
        public string EyeColor { get; set; }
        public override void Voice()
        {
            Debug.WriteLine("OOO...");
        }
    }
}
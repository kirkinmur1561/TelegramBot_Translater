using System.ComponentModel;
using System.Diagnostics;

namespace ViewPattern
{
    public abstract class Animal : IAnimal
    {
        public string PetName { get; set; }
        public int Age { get; set; }
        public bool IsTail { get; set; }


        public virtual void Voice()
        {
            Debug.WriteLine("Ничего");
        }

        public override string ToString() =>
            $"Petname:{PetName}\n" +
            $"Age:{Age.ToString()}\n" +
            $"IsTail:{IsTail.ToString()}";

    }
}
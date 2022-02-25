using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ViewPattern
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            List<Animal> animals = new List<Animal>();
            Animal animal = new Dog
            {
                PetName = "Rex",
                Age = 1,
                IsTail = false,
                EyeColor = "Red"
            };
            Dog dog = new Dog();
            dog.Age = 1;
            dog.IsTail = true;
            dog.PetName = "Rex";
            dog.EyeColor = "Black";

            Animal Dolphin = new Dolphin() { Age = 1, Depth = 1000, IsTail = true, PetName = "Mo" };
            List<IAnimal> ianimals = new List<IAnimal>();
            ianimals.Add(Dolphin);

            animals.Add(dog);
            animals.Add(animal);
            animals.Add(Dolphin);
            animals.ForEach(f=>Debug.WriteLine(f+"\n"));
            Console.ReadLine();
        }
    }
}
namespace ViewPattern
{
    public interface IAnimal
    {
        int Age { get; set; }
        bool IsTail { get; set; }
        string PetName { get; set; }

        string ToString();
        void Voice();
    }
}
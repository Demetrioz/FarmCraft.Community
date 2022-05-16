namespace FarmCraft.Community.Migrations
{
    public class AutoIncrementer
    {
        private int Index { get; set; }

        public AutoIncrementer()
        {
            Index = 1;
        }

        public AutoIncrementer(int index)
        {
            Index = index;
        }

        public int Get()
        {
            return Index++;
        }
    }
}

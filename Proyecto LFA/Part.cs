namespace Proyecto_LFA
{
    public abstract class Part
    {
        public Part(string line)
        {
            this.Validate(line);
        }

        public abstract void Validate(string line);
    }
}

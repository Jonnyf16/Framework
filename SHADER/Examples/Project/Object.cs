using OpenTK;

namespace Example
{
    public class Object : iObject
    {
        public Object()
        {
            Location = Vector3.Zero;
        }

        public Object(Vector3 location)
        {
            Location = location;
        }

        public Vector3 Location { get; set; }

        public void Update(Vector3 location)
        {
            Location += location;
        }
    }
}

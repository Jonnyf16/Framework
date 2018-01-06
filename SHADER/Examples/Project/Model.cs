using OpenTK;
using System.Collections.Generic;

namespace Example
{
	public class Model
	{
		public Model()
		{
            //for each body - setup body position and mass
            objects.Add(new Object(new Vector3(0, 18, 10)));
            objects.Add(new Object(new Vector3(14, 0, -5)));
            objects.Add(new Object(new Vector3(0, -20, 0)));
            objects.Add(new Object(new Vector3(-20, 0, 5)));
            objects.Add(new Object(new Vector3(11, -20, 0)));
            objects.Add(new Object(new Vector3(-20, 17, 5)));
		}

		public IEnumerable<iObject> Objects { get { return objects; } }

		private List<Object> objects = new List<Object>();
	}
}

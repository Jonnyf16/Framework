using DMS.OpenGL;
using OpenTK.Graphics.OpenGL;
using System.Text;
using OpenTK;
using DMS.Geometry;
using OpenTK.Input;

namespace Example
{
	public class VisualPlane
	{
		public VisualPlane()
		{
			var sVertex = Encoding.UTF8.GetString(Resourcen.plane_vert);
			var sFragment = Encoding.UTF8.GetString(Resourcen.plane_frag);
			shdPlane = ShaderLoader.FromStrings(sVertex, sFragment);
			var mesh = Meshes.CreateQuad(2, 2, 1, 1);
			plane = VAOLoader.FromMesh(mesh, shdPlane);
		}

		public void Draw(Matrix4 cam)
		{
			if (ReferenceEquals(shdPlane, null)) return;
			GL.Disable(EnableCap.CullFace);
			shdPlane.Activate();
			GL.UniformMatrix4(shdPlane.GetUniformLocation("camera"), true, ref cam);

			plane.Draw();
			shdPlane.Deactivate();
			GL.Enable(EnableCap.CullFace);
		}

		private VAO plane = new VAO();
		private Shader shdPlane;
	}
}
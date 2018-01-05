using DMS.Geometry;
using DMS.OpenGL;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Example
{
	public class PhongShading
	{
		public PhongShading()
		{
			camera.FarClip = 50;
			camera.Distance = 5;
			camera.FovY = 30;

			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
		}

		public CameraOrbit OrbitCamera { get { return camera; } }

		public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.PhongShader = shader;
			if (ReferenceEquals(shader, null)) return;
		}

		public void Render()
		{
			if (ReferenceEquals(PhongShader, null)) return;
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			PhongShader.Activate();
			GL.Uniform3(PhongShader.GetUniformLocation("light1Direction"), new Vector3(-1, -1, -1).Normalized());
			GL.Uniform4(PhongShader.GetUniformLocation("light1Color"), new Color4(1f, 1f, 1f, 1f));
			GL.Uniform3(PhongShader.GetUniformLocation("light2Position"), new Vector3(-1, -1, 1));
			GL.Uniform4(PhongShader.GetUniformLocation("light2Color"), new Color4(1f, .1f, .1f, 1f));
			GL.Uniform3(PhongShader.GetUniformLocation("light3Position"), new Vector3(-2, 2, 2));
			GL.Uniform3(PhongShader.GetUniformLocation("light3Direction"), new Vector3(1, -1, -1).Normalized());
			GL.Uniform1(PhongShader.GetUniformLocation("light3Angle"), DMS.Geometry.MathHelper.DegreesToRadians(10f));
			GL.Uniform4(PhongShader.GetUniformLocation("light3Color"), new Color4(0, 0, 1f, 1f));
			GL.Uniform4(PhongShader.GetUniformLocation("ambientLightColor"), new Color4(.1f, .1f, .1f, 1f));
			GL.Uniform4(PhongShader.GetUniformLocation("materialColor"), new Color4(.7f, .9f, .7f, 1f));
			var cam = camera.CalcMatrix().ToOpenTK();
			GL.UniformMatrix4(PhongShader.GetUniformLocation("camera"), true, ref cam);
			GL.Uniform3(PhongShader.GetUniformLocation("cameraPosition"), camera.CalcPosition().ToOpenTK());
			PhongShader.Deactivate();
		}

		public static readonly string ShaderName = nameof(PhongShader);
		private CameraOrbit camera = new CameraOrbit();

		private Shader PhongShader;
	}
}

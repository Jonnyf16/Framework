using DMS.OpenGL;
using DMS.Geometry;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Example
{
	public class MainVisual
	{
		public CameraOrbit OrbitCamera { get { return camera; } }

		public MainVisual()
		{
			plane = new VisualPlane();

			visualSmoke = new VisualSmoke(Vector3.Zero, new Vector3(.2f, 0, 0));
			visualWaterfall = new VisualWaterfall(new Vector3(-.5f, 1, -.5f));

			camera.FarClip = 20;
			camera.Distance = 2;
			camera.FovY = 70;
			camera.Elevation = 15;

			GL.Enable(EnableCap.DepthTest);
		}

		public void ShaderChanged(string name, Shader shader)
		{
			visualSmoke.ShaderChanged(name, shader);
			visualWaterfall.ShaderChanged(name, shader);
		}

		public void Update(float time)
		{
			glTimerUpdate.Activate(QueryTarget.TimeElapsed);
			visualSmoke.Update(time);
			visualWaterfall.Update(time);
			glTimerUpdate.Deactivate();
		}

		public void Render()
		{
			glTimerRender.Activate(QueryTarget.TimeElapsed);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			var cam = camera.CalcMatrix().ToOpenTK();
			plane.Draw(cam);
			visualSmoke.Render(cam);
			visualWaterfall.Render(cam);
			glTimerRender.Deactivate();

			Console.Write("Update:");
			Console.Write(glTimerUpdate.ResultLong / 1e6);
			Console.Write("msec  Render:");
			Console.Write(glTimerRender.ResultLong / 1e6);
			Console.WriteLine("msec");
		}

		private CameraOrbit camera = new CameraOrbit();

		private VisualPlane plane;
		private readonly VisualSmoke visualSmoke;
		private readonly VisualWaterfall visualWaterfall;
		private QueryObject glTimerRender = new QueryObject();
		private QueryObject glTimerUpdate = new QueryObject();
	}
}

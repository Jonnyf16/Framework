using DMS.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Example
{
	class MainVisual
	{
        public static readonly string ShaderName = nameof(shader);
        private const int pointCount = 3;
        private Shader shader;
        private Stopwatch timeSource = new Stopwatch();
        private VAO geometry;

        public MainVisual()
		{
            GL.Enable(EnableCap.ProgramPointSize);
			GL.Enable(EnableCap.PointSprite);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			timeSource.Start();
		}

        public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shader = shader;
			if (ReferenceEquals(shader, null)) return;
			UpdateGeometry(shader);
		}

		private void UpdateGeometry(Shader shader)
		{
			geometry = new VAO();
			//generate position arrray on CPU
			var rnd = new Random(12);
			Func<float> Rnd01 = () => (float)rnd.NextDouble();
			Func<float> RndCoord = () => (Rnd01() - 0.5f) * 2.0f;
			var positions = new Vector2[pointCount];
			for (int i = 0; i < pointCount; ++i)
			{
				positions[i] = new Vector2(0, 0);
			}
			//copy positions to GPU
			geometry.SetAttribute(shader.GetAttributeLocation("in_position"), positions, VertexAttribPointerType.Float, 2);

            //generate velocity arrray on CPU
            Func<float> RndSpeed = () => (Rnd01() - 0.5f) * 0.1f;
			var velocities = new Vector2[pointCount];
			for (int i = 0; i < pointCount; ++i)
			{
				velocities[i] = new Vector2(RndSpeed(), RndSpeed());
			}
			//copy velocities to GPU
			geometry.SetAttribute(shader.GetAttributeLocation("in_velocity"), velocities, VertexAttribPointerType.Float, 2);
		}

        public void Render()
        {
            if (ReferenceEquals(shader, null)) return;
            GL.Clear(ClearBufferMask.ColorBufferBit);
            shader.Activate();
            ////ATTENTION: always give the time as a float if the uniform in the shader is a float
            GL.Uniform1(shader.GetUniformLocation("iGlobalTime"), (float)timeSource.Elapsed.TotalSeconds);
            GL.Uniform2(shader.GetUniformLocation("iResolution"), new Vector2(512, 512));
            geometry.Activate();
            GL.DrawArrays(PrimitiveType.Points, 0, pointCount);
            geometry.Deactivate();
            shader.Deactivate();
        }
    }
}

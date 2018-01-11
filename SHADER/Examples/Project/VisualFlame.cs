using DMS.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Example
{
	public class VisualFlame
	{
        public static readonly string ShaderName = nameof(shaderFlame);
        private Shader shaderFlame;
        private Stopwatch timeSource = new Stopwatch();
        private VAO flame = new VAO();

        public VisualFlame(Vector3 firePosition)
		{  
			timeSource.Start();
        }

        public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderFlame = shader;
            if (ReferenceEquals(shader, null)) return;
     
        }

		public void Update( Vector3 firePosition, Vector3 windDirection)
		{
            if (ReferenceEquals(this.shaderFlame, null)) return;
            this.flame.SetAttribute(this.shaderFlame.GetAttributeLocation("instance_position"), new Vector3[] { firePosition }, VertexAttribPointerType.Float, 3);
            this.flame.SetAttribute(this.shaderFlame.GetAttributeLocation("wind_direction"), new Vector3[] { windDirection }, VertexAttribPointerType.Float, 3);
            //this.flame.SetAttribute(this.shaderFlame.GetUniformLocation("smoke_state"), new uint[] { Convert.ToUInt32(smokeState) }, VertexAttribPointerType.UnsignedInt, 1);
        }

        public void Render(Matrix4 camera, int windowHeight, int windowWidth, float camElevation, float camAzimuth, bool smokeState)
        {
            if (ReferenceEquals(this.shaderFlame, null)) return;

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.VertexProgramPointSize);
            GL.PointParameter(PointSpriteCoordOriginParameter.LowerLeft);

            this.shaderFlame.Activate();
            GL.Uniform1(this.shaderFlame.GetUniformLocation("iGlobalTime"), (float)timeSource.Elapsed.TotalSeconds);
            GL.Uniform2(this.shaderFlame.GetUniformLocation("iResolution"), new Vector2(windowHeight, windowWidth));
            GL.UniformMatrix4(this.shaderFlame.GetUniformLocation("camera"), true, ref camera);
            GL.Uniform1(this.shaderFlame.GetUniformLocation("camElevation"), camElevation);
            GL.Uniform1(this.shaderFlame.GetUniformLocation("camAzimuth"), camAzimuth);
            GL.Uniform1(this.shaderFlame.GetUniformLocation("smokeState"), (float)Convert.ToDouble(smokeState));
            this.flame.Activate();
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            this.flame.Deactivate();
            this.shaderFlame.Deactivate();

            GL.Disable(EnableCap.VertexProgramPointSize);
            GL.Disable(EnableCap.PointSprite);
            GL.Disable(EnableCap.Blend);
        }
    }
}

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
        private const int pointCount = 1;
        private Shader shaderFlame;
        private Stopwatch timeSource = new Stopwatch();
        private VAO flame = new VAO();

        public VisualFlame()
		{
			timeSource.Start();
		}

        public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderFlame = shader;
            if (ReferenceEquals(shader, null)) return;
            Update();
        }

		public void Update()
		{
            //this.flame.SetAttribute(this.shaderFlame.GetAttributeLocation("position"), position, VertexAttribPointerType.Float, 2);
        }

        public void Render()
        {
            if (ReferenceEquals(this.shaderFlame, null)) return;
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.ProgramPointSize);
    
            GL.Clear(ClearBufferMask.ColorBufferBit);
            this.shaderFlame.Activate();
            
            GL.Uniform1(this.shaderFlame.GetUniformLocation("iGlobalTime"), (float)timeSource.Elapsed.TotalSeconds);
            GL.Uniform2(this.shaderFlame.GetUniformLocation("iResolution"), new Vector2(512, 512));
            this.flame.Activate();
            GL.DrawArrays(PrimitiveType.Points, 0, pointCount);
            this.flame.Deactivate();
            this.shaderFlame.Deactivate();

            GL.Disable(EnableCap.VertexProgramPointSize);
            GL.Disable(EnableCap.PointSprite);
            GL.Disable(EnableCap.Blend);
        }
    }
}

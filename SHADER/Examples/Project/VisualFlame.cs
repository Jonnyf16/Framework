﻿using DMS.OpenGL;
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
        private Vector3 firePosition;

        public VisualFlame(Vector3 firePosition)
		{  
			timeSource.Start();
            this.firePosition = firePosition;
        }

        public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderFlame = shader;
            if (ReferenceEquals(shader, null)) return;
     
        }

		public void Update()
		{
            if (ReferenceEquals(this.shaderFlame, null)) return;
        }

        public void Render(Matrix4 camera, int windowHeight, int windowWidth)
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
            this.flame.SetAttribute(this.shaderFlame.GetAttributeLocation("instance_position"), new Vector3[] { this.firePosition }, VertexAttribPointerType.Float, 3);
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

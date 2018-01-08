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
        private const int pointCount = 3;
        private Shader shaderFlame;
        private Stopwatch timeSource = new Stopwatch();
        private VAO flame;

        public VisualFlame()
		{
			timeSource.Start();
		}

        public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderFlame = shader;
			if (ReferenceEquals(shader, null)) return;
			Update(shader);
		}

		private void Update(Shader shader)
		{
			flame = new VAO();
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
			flame.SetAttribute(shader.GetAttributeLocation("in_position"), positions, VertexAttribPointerType.Float, 2);

            //generate velocity arrray on CPU
            Func<float> RndSpeed = () => (Rnd01() - 0.5f) * 0.1f;
			var velocities = new Vector2[pointCount];
			for (int i = 0; i < pointCount; ++i)
			{
				velocities[i] = new Vector2(RndSpeed(), RndSpeed());
			}
			//copy velocities to GPU
			flame.SetAttribute(shader.GetAttributeLocation("in_velocity"), velocities, VertexAttribPointerType.Float, 2);
		}

        public void Render()
        {
            if (ReferenceEquals(shaderFlame, null)) return;
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.ProgramPointSize);
    
            GL.Clear(ClearBufferMask.ColorBufferBit);
            shaderFlame.Activate();
            GL.Uniform1(shaderFlame.GetUniformLocation("iGlobalTime"), (float)timeSource.Elapsed.TotalSeconds);
            GL.Uniform2(shaderFlame.GetUniformLocation("iResolution"), new Vector2(512, 512));
            flame.Activate();
            GL.DrawArrays(PrimitiveType.Points, 0, pointCount);
            flame.Deactivate();
            shaderFlame.Deactivate();

            GL.Disable(EnableCap.VertexProgramPointSize);
            GL.Disable(EnableCap.PointSprite);
            GL.Disable(EnableCap.Blend);
        }
    }
}

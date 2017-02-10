﻿using DMSOpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Example
{
	public class MainVisual
	{
		public MainVisual()
		{
			var sVertex = Encoding.UTF8.GetString(Resourcen.vertex);
			var sFragment = Encoding.UTF8.GetString(Resourcen.fragment);
			shader = ShaderLoader.FromStrings(sVertex, sFragment);

			InitVBOs();
			timeSource.Start();
		}

		public void Render()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.PointSize(1.0f);
			shader.Activate();
			//ATTENTION: always giver the time as a float if the uniform in the shader is a float
			GL.Uniform1(shader.GetUniformLocation("time"), (float)timeSource.Elapsed.TotalSeconds);
			GL.BindVertexArray(VAO);
			GL.DrawArrays(PrimitiveType.Points, 0, particelCount);
			GL.BindVertexArray(0);
			shader.Deactivate();
		}

		private const int particelCount = 100000;
		private Shader shader;
		private Stopwatch timeSource = new Stopwatch();
		private int VAO;

		private void InitVBOs()
		{
			VAO = GL.GenVertexArray();
			GL.BindVertexArray(VAO);
			var rnd = new Random(12);
			Func<float> Rnd01 = () => (float)rnd.NextDouble();
			Func<float> RndCoord = () => (Rnd01() - 0.5f) * 2.0f;
			Func<float> RndSpeed = () => (Rnd01() - 0.5f) * 0.1f;
			var vertices = new List<VertexFormat>();
			for (int i = 0; i < particelCount; ++i)
			{
				vertices.Add(new VertexFormat(
					new Vector2(RndCoord(), RndCoord()),
					new Vector2(RndSpeed(), RndSpeed())
					));
			}
			uint bufferID; //our vbo handler
			GL.GenBuffers(1, out bufferID);
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferID);//bind to context
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(VertexFormat.size * vertices.Count), vertices.ToArray(), BufferUsageHint.StaticDraw);
			VertexFormat.Activate();
			//everything stored in VAO -> unbind everything
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			VertexFormat.Deactive();
		}
	}
}

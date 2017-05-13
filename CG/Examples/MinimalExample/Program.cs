﻿using DMS.Application;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace Example
{
	class MyVisual : IWindow
	{
		[STAThread]
		private static void Main()
		{
			var app = new ExampleApplication();
			var visual = new MyVisual();
			app.Run(visual);
		}

		public void Update(float updatePeriod)
		{
		}

		public void Render()
		{
			//clear screen - what happens without?
			GL.Clear(ClearBufferMask.ColorBufferBit);
			//draw a primitive
			GL.Begin(PrimitiveType.Quads);
			//color is active as long as no new color is set
			GL.Color3(Color.Cyan);
			GL.Vertex2(0.0f, 0.0f);
			GL.Vertex2(0.5f, 0.0f);
			GL.Color3(Color.White);
			GL.Vertex2(0.5f, 0.5f);
			GL.Vertex2(0.0f, 0.5f);
			GL.End();
		}
	}
}
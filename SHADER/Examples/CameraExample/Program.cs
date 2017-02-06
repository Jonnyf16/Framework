﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;

namespace Example
{
	class MyApplication
	{
		private GameWindow gameWindow;
		private MainVisual visual;

		[STAThread]
		public static void Main()
		{
			var app = new MyApplication();
			app.gameWindow.Run();
		}

		private MyApplication()
		{
			gameWindow = new GameWindow();
			//gameWindow.WindowState = WindowState.Fullscreen;
			gameWindow.MouseMove += GameWindow_MouseMove;
			gameWindow.MouseWheel += GameWindow_MouseWheel;
			gameWindow.KeyDown += (s, arg) => gameWindow.Close();
			gameWindow.Resize += (s, arg) => GL.Viewport(0, 0, gameWindow.Width, gameWindow.Height);
			gameWindow.RenderFrame += (s, arg) => visual.Render();			
			gameWindow.RenderFrame += (s, arg) => gameWindow.SwapBuffers();
			visual = new MainVisual();
		}

		private void GameWindow_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			visual.OrbitCamera.Distance -= 10 * e.DeltaPrecise;
		}

		private void GameWindow_MouseMove(object sender, MouseMoveEventArgs e)
		{
			if (ButtonState.Pressed == e.Mouse.LeftButton)
			{
				visual.OrbitCamera.Heading += 300 * e.XDelta / (float)gameWindow.Width;
				visual.OrbitCamera.Tilt += 300 * e.YDelta / (float)gameWindow.Height;
			}
		}
	}
}
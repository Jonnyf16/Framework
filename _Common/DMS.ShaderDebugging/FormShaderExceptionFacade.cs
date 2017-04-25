﻿using DMS.OpenGL;
using System;
using System.Drawing;

namespace DMS.ShaderDebugging
{
	class Owner : System.Windows.Forms.IWin32Window
	{
		public Owner(IntPtr handle)
		{
			Handle = handle;
		}

		public IntPtr Handle { get; private set; }
	};

	public class FormShaderExceptionFacade
	{
		public event EventHandler<EventArgs> Save;

		public FormShaderExceptionFacade()
		{
			form = new FormShaderException();
			form.Save += (s, a) => Save?.Invoke(s, a);
		}

		public void Clear()
		{
			lastException = null;
			form.richTextBox.Clear();
			form.Errors.Clear();
		}

		public void Hide()
		{
			form.Hide();
		}

		public void Show(ShaderException e)
		{
			if (ReferenceEquals(null, lastException) || e.Log != lastException.Log)
			{
				Clear(); //clears last log too -> need to store lastLog afterwards
				lastException = e;
				FillData(e);
			}
			form.Show();
			//todo: bring to front
			form.TopMost = true;
			form.TopMost = false;
		}

		public void Show(ShaderException e, OpenTK.Platform.IGameWindow gameWindow)
		{
			if (ReferenceEquals(null, lastException) || e.Log != lastException.Log)
			{
				Clear(); //clears last log too -> need to store lastLog afterwards
				lastException = e;
				FillData(e);
			}
			form.SetBounds(gameWindow.Bounds.Left, gameWindow.Bounds.Top, gameWindow.Width, gameWindow.Height);
			if (!form.Visible) form.Show(new Owner(gameWindow.WindowInfo.Handle));
			//todo: bring to front
			form.TopMost = true;
		}

		public static void ShowModal(ShaderException e)
		{
			var facade = new FormShaderExceptionFacade();
			//facade.OnSave += (s, a) => facade.
			facade.Clear();
			facade.FillData(e);
			facade.form.ShowDialog();
		}

		private readonly FormShaderException form;
		private ShaderException lastException;

		private void FillData(ShaderException e)
		{
			var rtf = form.richTextBox;
			var font = rtf.Font;
			var errorFont = new Font(font, FontStyle.Strikeout);
			char[] newline = new char[] { '\n' };
			var sourceLines = e.ShaderCode.Split(newline);
			foreach (var sourceLine in sourceLines)
			{
				rtf.AppendText(sourceLine);
			}

			var log = new ShaderLog(e.Log);
			foreach (var logLine in log.Lines)
			{
				form.Errors.Add(logLine);
			}
		}
	}
}

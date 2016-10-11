﻿using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace ShaderForm
{
	public class ShaderFile : IShaderFile
	{
		public event EventHandler<string> OnChange;

		public ShaderFile(VisualContext visualContext, ISynchronizeInvoke syncObject)
		{
			this.visualContext = visualContext;
			//establish watcher
			shaderWatcher.Changed += (object sender, FileSystemEventArgs e) => LoadShader(e.FullPath);
			shaderWatcher.SynchronizingObject = syncObject;
		}

		public void Load(string shaderFileName)
		{
			if (!File.Exists(shaderFileName)) throw new ShaderLoadException("Non existent shader file '" + shaderFileName + "'!");
			this.shaderFileName = shaderFileName;
			shaderWatcher.Filter = Path.GetFileName(shaderFileName);
			shaderWatcher.Path = Path.GetDirectoryName(shaderFileName);
			shaderWatcher.EnableRaisingEvents = true;
			LoadShader(shaderFileName);
		}

		public void Dispose()
		{
			visualContext.RemoveShader(shaderFileName);
			shaderWatcher.Dispose();
			shaderWatcher = null;
		}

		private string shaderFileName;
		private FileSystemWatcher shaderWatcher = new FileSystemWatcher();
		private VisualContext visualContext;

		private void LoadShader(string fileName)
		{
			try
			{
				visualContext.AddUpdateFragmentShader(fileName);
				CallOnChange("Loading '+" + fileName + "' with success!");
			}
			catch (ShaderLoadException e)
			{
				CallOnChange("Error while compiling shader '" + fileName + "'" + Environment.NewLine + e.Message);
			}
			catch (FileNotFoundException e)
			{
				CallOnChange(e.Message);
			}
			catch (Exception e)
			{
				//try reload in 2 seconds, because sometimes file system is still busy
				Timer timer = new Timer(); //todo: is this executed on main thread?
				timer.Interval = 2000;
				timer.Tick += (a, b) =>
				{
					timer.Stop();
					timer.Dispose();
					LoadShader(shaderFileName); //if fileName is used here timer will always access fileName of first call and not a potential new one 
				};
				timer.Start();
				CallOnChange("Error while accessing shaderfile '" + fileName + "'! Will retry shortly..." + Environment.NewLine + e.Message);
			}
		}

		private void CallOnChange(string message)
		{
			if (null != OnChange) OnChange(this, message);
		}
	}
}
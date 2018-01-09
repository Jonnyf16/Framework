using DMS.Application;
using DMS.Base;using DMS.Application;
using System;
using System.Diagnostics;
using System.IO;

namespace Example
{
	class Controller
	{
		[STAThread]
		private static void Main()
		{
            var windowHeight = 512;
            var windowWidth = 512;
			var app = new ExampleApplication(windowHeight, windowWidth);
            var i = app.GameWindow.Height;
			LoadResources(app.ResourceManager);
			var controller = new Controller();
			MainVisual visual = new MainVisual(windowHeight, windowWidth);
			app.ResourceManager.ShaderChanged += visual.ShaderChanged;
			var timeSource = new Stopwatch();
			app.GameWindow.ConnectEvents(visual.OrbitCamera);
			app.Render += visual.Render;
			app.Update += (t) => visual.Update((float)timeSource.Elapsed.TotalSeconds);
			timeSource.Start();
			app.Run();
		}

		private static void LoadResources(ResourceManager resourceManager)
		{
			resourceManager.Add(nameof(Resourcen.smoke), new ResourceTextureBitmap(Resourcen.smoke));
			var dir = Path.GetDirectoryName(PathTools.GetSourceFilePath()) + "/Resources/";
            // TODO: change name of "smoke" shader to "particle"
			resourceManager.AddShader(VisualSmoke.ShaderName, dir + "smoke.vert", dir + "smoke.frag"
				, Resourcen.smoke_vert, Resourcen.smoke_frag);
			resourceManager.AddShader(VisualRain.ShaderName, dir + "rain.vert", dir + "rain.frag"
				, Resourcen.smoke_vert, Resourcen.smoke_frag);
            resourceManager.AddShader(VisualObjects.ShaderName, dir + "objects.vert", dir + "objects.frag"
                , Resourcen.objects_vert, Resourcen.objects_frag);
            resourceManager.AddShader(VisualFlame.ShaderName, dir + "flame.vert", dir + "flame.frag"
                , Resourcen.flame_vert, Resourcen.flame_frag);
        }
	}
}
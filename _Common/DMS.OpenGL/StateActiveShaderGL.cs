﻿using DMS.HLGL;

namespace DMS.OpenGL
{
	public class StateActiveShaderGL : IState
	{
		public IShader Shader
		{
			get => shader;
			set
			{
				if (ReferenceEquals(shader, value)) return;
				shader?.Deactivate();
				shader = value;
				shader?.Activate();
			}
		}

		private IShader shader = null;
	}
}

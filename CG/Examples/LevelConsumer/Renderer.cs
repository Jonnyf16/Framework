﻿using DMS.OpenGL;
using DMS.Geometry;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using DMS.HLGL;

namespace Example
{
	public class Renderer
	{
		public Renderer()
		{
			//for transparency in textures we use blending
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Enable(EnableCap.Blend);
		}

		public void AddSprite(string name, int layer, Box2D renderBounds, string textureName, Bitmap bitmap)
		{
			var texture = GetTexture(textureName, bitmap);
			if (!layers.ContainsKey(layer))
			{
				layers.Add(layer, new Layer());
			}
			layers[layer].Add(texture, renderBounds);
			AddNamedSprite(name, renderBounds);
		}

		public void Resize(int width, int height)
		{
			aspect = width / (float)height;
		}

		public void Render(Box2D bounds)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.MatrixMode(MatrixMode.Projection);
			var size = Math.Max(bounds.SizeX, bounds.SizeY);
			GL.LoadIdentity();
			GL.Ortho(0, size * aspect, 0, size, 0, 1);
			GL.MatrixMode(MatrixMode.Modelview);
			foreach (var layer in layers)
			{
				layer.Value.Draw();
			}
		}

		public void UpdateSprites(string name, float x, float y)
		{
			var sprites = FindNamedSprites(name);
			foreach (var sprite in sprites)
			{
				//todo: do some hierarchical transform, otherwise all sprites have same position
				sprite.CenterX = x;
				sprite.CenterY = y;
			}
		}

		private class Layer
		{
			public void Add(ITexture tex, Box2D bounds)
			{
				if (!textures.ContainsKey(tex))
				{
					textures.Add(tex, new List<Box2D>());
				}
				textures[tex].Add(bounds);
			}

			public void Draw()
			{
				foreach(var tex in textures)
				{
					tex.Key.Activate();
					foreach(var box in tex.Value)
					{
						box.DrawTexturedRect(Box2D.BOX01);
					}
					tex.Key.Deactivate();
				}
			}

			private Dictionary<ITexture, List<Box2D>> textures = new Dictionary<ITexture, List<Box2D>>();
		}

		private SortedDictionary<int, Layer> layers = new SortedDictionary<int, Layer>();
		private Dictionary<string, ITexture> textures = new Dictionary<string, ITexture>();
		private Dictionary<string, List<Box2D>> namedSprites = new Dictionary<string, List<Box2D>>();
		private float aspect = 1f;

		private void AddNamedSprite(string name, Box2D bounds)
		{
			if (string.IsNullOrWhiteSpace(name)) return;
			var n = name.ToLowerInvariant();
			if (!namedSprites.ContainsKey(n))
			{
				namedSprites.Add(n, new List<Box2D>());
			}
			namedSprites[n].Add(bounds);
		}

		private IEnumerable<Box2D> FindNamedSprites(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return null;
			var n = name.ToLowerInvariant();
			if (namedSprites.TryGetValue(n, out List<Box2D> sprites))
			{
				return sprites;
			}
			return null;
		}

		private ITexture GetTexture(string textureName, Bitmap bitmap)
		{
			ITexture texture;
			if (!textures.TryGetValue(textureName, out texture))
			{
				texture = TextureLoader.FromBitmap(bitmap);
				texture.Filter = TextureFilterMode.Mipmap;
				texture.WrapFunction = TextureWrapFunction.ClampToEdge;
				textures.Add(textureName, texture);
			}
			return texture;
		}
	}
}

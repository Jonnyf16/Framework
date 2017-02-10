﻿using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;

namespace DMS.OpenGL
{
	[Serializable]
	public class VAOException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VAOException"/> class.
		/// </summary>
		/// <param name="msg">The error msg.</param>
		public VAOException(string msg) : base(msg) { }
	}

	public class VAO : IDisposable
	{
		public VAO()
		{
			idVAO = GL.GenVertexArray();
		}

		public void Dispose()
		{
			foreach (var buffer in boundBuffers.Values)
			{
				buffer.Dispose();
			}
			boundBuffers.Clear();
			GL.DeleteVertexArray(idVAO);
			idVAO = 0;
		}

		public void SetID<Index>(Index[] data, PrimitiveType primitiveType) where Index : struct
		{
			Activate();
			var buffer = RequestBuffer(idBufferBinding, BufferTarget.ElementArrayBuffer);
			// set buffer data
			buffer.Set(data, BufferUsageHint.StaticDraw);
			//activate for state
			buffer.Activate();
			//cleanup state
			Deactive();
			buffer.Deactive();
			//save data for draw call
			DrawElementsType drawElementsType = GetDrawElementsType(typeof(Index));
			idData = new IDData(primitiveType, data.Length, drawElementsType);
		}

		public void SetAttribute<DataElement>(int bindingID, DataElement[] data, VertexAttribPointerType type, int elementSize, bool perInstance = false) where DataElement : struct
		{
			if (-1 == bindingID) return; //if attribute not used in shader or wrong name
			Activate();
			var buffer = RequestBuffer(bindingID, BufferTarget.ArrayBuffer);
			buffer.Set(data, BufferUsageHint.StaticDraw);
			//activate for state
			buffer.Activate();
			//set data format
			int elementBytes = Marshal.SizeOf(typeof(DataElement));
			GL.VertexAttribPointer(bindingID, elementSize, type, false, elementBytes, 0);
			GL.EnableVertexAttribArray(bindingID);
			if (perInstance)
			{
				GL.VertexAttribDivisor(bindingID, 1);
			}
			//cleanup state
			Deactive();
			buffer.Deactive();
			GL.DisableVertexAttribArray(bindingID);
		}

		/// <summary>
		/// sets or updates a vertex attribute of type Matrix4
		/// </summary>
		/// <param name="bindingID">shader binding location</param>
		/// <param name="data">ATTENTION: here the matrices are assumed to be rowmajor. why i don't know</param>
		/// <param name="perInstance"></param>
		public void SetMatrixAttribute(int bindingID, Matrix4[] data, bool perInstance = false)
		{
			if (-1 == bindingID) return; //if matrix not used in shader or wrong name
			Activate();
			var buffer = RequestBuffer(bindingID, BufferTarget.ArrayBuffer);
			// set buffer data
			buffer.Set(data, BufferUsageHint.StaticDraw);
			//activate for state
			buffer.Activate();
			//set data format
			int columnBytes = Marshal.SizeOf(typeof(Vector4));
			int elementBytes = Marshal.SizeOf(typeof(Matrix4));
			for (int i = 0; i < 4; i++)
			{
				GL.VertexAttribPointer(bindingID + i, 4, VertexAttribPointerType.Float, false, elementBytes, columnBytes * i);
				GL.EnableVertexAttribArray(bindingID + i);
				if (perInstance)
				{
					GL.VertexAttribDivisor(bindingID + i, 1);
				}
			}
			//cleanup state
			Deactive();
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			for (int i = 0; i < 4; i++)
			{
				GL.DisableVertexAttribArray(bindingID + i);
			}
		}

		public void Activate()
		{
			GL.BindVertexArray(idVAO);
		}

		public void Deactive()
		{
			GL.BindVertexArray(0);
		}

		public void DrawArrays(PrimitiveType type, int count, int start = 0)
		{
			Activate();
			GL.DrawArrays(type, start, count);
			Deactive();
		}

		public void Draw(int instanceCount = 1)
		{
			if (0 == idData.length) throw new VAOException("Empty id data set! Draw yourself using active/deactivate!");
			Activate();
			GL.DrawElementsInstanced(idData.primitiveType, idData.length, idData.drawElementsType, (IntPtr)0, instanceCount);
			Deactive();
		}

		private struct IDData
		{
			public DrawElementsType drawElementsType;
			public int length;
			public PrimitiveType primitiveType;

			public IDData(PrimitiveType primitiveType, int length, DrawElementsType drawElementsType)
			{
				this.primitiveType = primitiveType;
				this.length = length;
				this.drawElementsType = drawElementsType;
			}
		}

		private IDData idData;
		private int idVAO;
		private const int idBufferBinding = int.MaxValue;
		private Dictionary<int, BufferObject> boundBuffers = new Dictionary<int, BufferObject>();

		private static DrawElementsType GetDrawElementsType(Type type)
		{
			if (type == typeof(ushort)) return DrawElementsType.UnsignedShort;
			if (type == typeof(uint)) return DrawElementsType.UnsignedInt;
			throw new Exception("Invalid index type");
		}
		
		private BufferObject RequestBuffer(int bindingID, BufferTarget bufferTarget)
		{
			BufferObject buffer;
			if (!boundBuffers.TryGetValue(bindingID, out buffer))
			{
				buffer = new BufferObject(bufferTarget);
				boundBuffers[bindingID] = buffer;
			}
			return buffer;
		}
	}
}
﻿using System;

using Duality.Drawing;
using Duality.Resources;
using Duality.Editor;
using Duality.Properties;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Duality.Components.Renderers
{
	/// <summary>
	/// Renders a sprite to represent the <see cref="GameObject"/>.
	/// </summary>
	[Serializable]
	[EditorHintCategory(typeof(CoreRes), CoreResNames.CategoryGraphics)]
	[EditorHintImage(typeof(CoreRes), CoreResNames.ImageSpriteRenderer)]
	public class SpriteRenderer : Renderer
	{
		/// <summary>
		/// SPecifies, how the sprites uv-Coordinates are calculated.
		/// </summary>
		public enum UVMode
		{
			/// <summary>
			/// The uv-Coordinates are constant, stretching the supplied texture to fit the SpriteRenderers dimensions.
			/// </summary>
			Stretch			= 0x0,
			/// <summary>
			/// The u-Coordinate is calculated based on the available horizontal space, allowing the supplied texture to be
			/// tiled across the SpriteRenderers width.
			/// </summary>
			WrapHorizontal	= 0x1,
			/// <summary>
			/// The v-Coordinate is calculated based on the available vertical space, allowing the supplied texture to be
			/// tiled across the SpriteRenderers height.
			/// </summary>
			WrapVertical	= 0x2,
			/// <summary>
			/// The uv-Coordinates are calculated based on the available space, allowing the supplied texture to be
			/// tiled across the SpriteRenderers size.
			/// </summary>
			WrapBoth		= WrapHorizontal | WrapVertical
		}

		protected	Rect					rect		= Rect.AlignCenter(0, 0, 256, 256);
		protected	ContentRef<Material>	sharedMat	= Material.DualityIcon;
		protected	BatchInfo				customMat	= null;
		protected	ColorRgba				colorTint	= ColorRgba.White;
		protected	UVMode					rectMode	= UVMode.Stretch;
		protected	bool					pixelGrid	= false;
		protected	int						offset		= 0;
		[NonSerialized]
		protected	VertexC1P3T2[]			vertices	= null;

		[EditorHintFlags(MemberFlags.Invisible)]
		public override float BoundRadius
		{
			get { return this.rect.Transform(this.gameobj.Transform.Scale, this.gameobj.Transform.Scale).BoundingRadius; }
		}
		/// <summary>
		/// [GET / SET] The rectangular area the sprite occupies. Relative to the <see cref="GameObject"/>.
		/// </summary>
		[EditorHintDecimalPlaces(1)]
		public Rect Rect
		{
			get { return this.rect; }
			set { this.rect = value; }
		}
		/// <summary>
		/// [GET / SET] The <see cref="Duality.Resources.Material"/> that is used for rendering the sprite.
		/// </summary>
		public ContentRef<Material> SharedMaterial
		{
			get { return this.sharedMat; }
			set { this.sharedMat = value; }
		}
		/// <summary>
		/// [GET / SET] A custom, local <see cref="Duality.Resources.BatchInfo"/> overriding the <see cref="SharedMaterial"/>,
		/// allowing this sprite to look unique without having to create its own <see cref="Duality.Resources.Material"/> Resource.
		/// However, this feature should be used with caution: Performance is better using <see cref="SharedMaterial">shared Materials</see>.
		/// </summary>
		public BatchInfo CustomMaterial
		{
			get { return this.customMat; }
			set { this.customMat = value; }
		}
		/// <summary>
		/// [GET / SET] A color by which the sprite is tinted.
		/// </summary>
		public ColorRgba ColorTint
		{
			get { return this.colorTint; }
			set { this.colorTint = value; }
		}
		/// <summary>
		/// [GET / SET] Specifies how the sprites uv-Coordinates are calculated.
		/// </summary>
		public UVMode RectMode
		{
			get { return this.rectMode; }
			set { this.rectMode = value; }
		}
		/// <summary>
		/// [GET / SET] Specified whether or not the rendered sprite will be aligned to actual screen pixels.
		/// </summary>
		public bool AlignToPixelGrid
		{
			get { return this.pixelGrid; }
			set { this.pixelGrid = value; }
		}
		/// <summary>
		/// [GET / SET] A virtual Z offset that affects the order in which objects are drawn. If you want to assure an object is drawn after another one,
		/// just assign a higher Offset value to the background object.
		/// </summary>
		public int Offset
		{
			get { return this.offset; }
			set { this.offset = value; }
		}
		/// <summary>
		/// [GET] The internal Z-Offset added to the renderers vertices based on its <see cref="Offset"/> value.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public float VertexZOffset
		{
			get { return this.offset * 0.01f; }
		}


		public SpriteRenderer() {}
		public SpriteRenderer(Rect rect, ContentRef<Material> mainMat)
		{
			this.rect = rect;
			this.sharedMat = mainMat;
		}

		protected Texture RetrieveMainTex()
		{
			if (this.customMat != null)
				return this.customMat.MainTexture.Res;
			else if (this.sharedMat.IsAvailable)
				return this.sharedMat.Res.MainTexture.Res;
			else
				return null;
		}
		protected ColorRgba RetrieveMainColor()
		{
			if (this.customMat != null)
				return this.customMat.MainColor * this.colorTint;
			else if (this.sharedMat.IsAvailable)
				return this.sharedMat.Res.MainColor * this.colorTint;
			else
				return this.colorTint;
		}
		protected DrawTechnique RetrieveDrawTechnique()
		{
			if (this.customMat != null)
				return this.customMat.Technique.Res;
			else if (this.sharedMat.IsAvailable)
				return this.sharedMat.Res.Technique.Res;
			else
				return null;
		}
		protected void PrepareVertices(ref VertexC1P3T2[] vertices, IDrawDevice device, ColorRgba mainClr, Rect uvRect)
		{
			Vector3 posTemp = this.gameobj.Transform.Pos;
			float scaleTemp = 1.0f;
			device.PreprocessCoords(ref posTemp, ref scaleTemp);

			Vector2 xDot, yDot;
			MathF.GetTransformDotVec(this.GameObj.Transform.Angle, scaleTemp, out xDot, out yDot);

			Rect rectTemp = this.rect.Transform(this.gameobj.Transform.Scale, this.gameobj.Transform.Scale);
			Vector2 edge1 = rectTemp.TopLeft;
			Vector2 edge2 = rectTemp.BottomLeft;
			Vector2 edge3 = rectTemp.BottomRight;
			Vector2 edge4 = rectTemp.TopRight;

			MathF.TransformDotVec(ref edge1, ref xDot, ref yDot);
			MathF.TransformDotVec(ref edge2, ref xDot, ref yDot);
			MathF.TransformDotVec(ref edge3, ref xDot, ref yDot);
			MathF.TransformDotVec(ref edge4, ref xDot, ref yDot);

			if (vertices == null || vertices.Length != 4) vertices = new VertexC1P3T2[4];

			vertices[0].Pos.X = posTemp.X + edge1.X;
			vertices[0].Pos.Y = posTemp.Y + edge1.Y;
			vertices[0].Pos.Z = posTemp.Z + this.VertexZOffset;
			vertices[0].TexCoord.X = uvRect.X;
			vertices[0].TexCoord.Y = uvRect.Y;
			vertices[0].Color = mainClr;

			vertices[1].Pos.X = posTemp.X + edge2.X;
			vertices[1].Pos.Y = posTemp.Y + edge2.Y;
			vertices[1].Pos.Z = posTemp.Z + this.VertexZOffset;
			vertices[1].TexCoord.X = uvRect.X;
			vertices[1].TexCoord.Y = uvRect.MaximumY;
			vertices[1].Color = mainClr;

			vertices[2].Pos.X = posTemp.X + edge3.X;
			vertices[2].Pos.Y = posTemp.Y + edge3.Y;
			vertices[2].Pos.Z = posTemp.Z + this.VertexZOffset;
			vertices[2].TexCoord.X = uvRect.MaximumX;
			vertices[2].TexCoord.Y = uvRect.MaximumY;
			vertices[2].Color = mainClr;
				
			vertices[3].Pos.X = posTemp.X + edge4.X;
			vertices[3].Pos.Y = posTemp.Y + edge4.Y;
			vertices[3].Pos.Z = posTemp.Z + this.VertexZOffset;
			vertices[3].TexCoord.X = uvRect.MaximumX;
			vertices[3].TexCoord.Y = uvRect.Y;
			vertices[3].Color = mainClr;
			
			if (this.pixelGrid)
			{
				vertices[0].Pos.X = MathF.Round(vertices[0].Pos.X);
				vertices[1].Pos.X = MathF.Round(vertices[1].Pos.X);
				vertices[2].Pos.X = MathF.Round(vertices[2].Pos.X);
				vertices[3].Pos.X = MathF.Round(vertices[3].Pos.X);

				if (MathF.RoundToInt(device.TargetSize.X) != (MathF.RoundToInt(device.TargetSize.X) / 2) * 2)
				{
					vertices[0].Pos.X += 0.5f;
					vertices[1].Pos.X += 0.5f;
					vertices[2].Pos.X += 0.5f;
					vertices[3].Pos.X += 0.5f;
				}

				vertices[0].Pos.Y = MathF.Round(vertices[0].Pos.Y);
				vertices[1].Pos.Y = MathF.Round(vertices[1].Pos.Y);
				vertices[2].Pos.Y = MathF.Round(vertices[2].Pos.Y);
				vertices[3].Pos.Y = MathF.Round(vertices[3].Pos.Y);

				if (MathF.RoundToInt(device.TargetSize.Y) != (MathF.RoundToInt(device.TargetSize.Y) / 2) * 2)
				{
					vertices[0].Pos.Y += 0.5f;
					vertices[1].Pos.Y += 0.5f;
					vertices[2].Pos.Y += 0.5f;
					vertices[3].Pos.Y += 0.5f;
				}
			}
		}

		public override void Draw(IDrawDevice device)
		{
			Texture mainTex = this.RetrieveMainTex();
			ColorRgba mainClr = this.RetrieveMainColor();

			Rect uvRect;
			if (mainTex != null)
			{
				if (this.rectMode == UVMode.WrapBoth)
					uvRect = new Rect(mainTex.UVRatio.X * this.rect.W / mainTex.PixelWidth, mainTex.UVRatio.Y * this.rect.H / mainTex.PixelHeight);
				else if (this.rectMode == UVMode.WrapHorizontal)
					uvRect = new Rect(mainTex.UVRatio.X * this.rect.W / mainTex.PixelWidth, mainTex.UVRatio.Y);
				else if (this.rectMode == UVMode.WrapVertical)
					uvRect = new Rect(mainTex.UVRatio.X, mainTex.UVRatio.Y * this.rect.H / mainTex.PixelHeight);
				else
					uvRect = new Rect(mainTex.UVRatio.X, mainTex.UVRatio.Y);
			}
			else
				uvRect = new Rect(1.0f, 1.0f);

			this.PrepareVertices(ref this.vertices, device, mainClr, uvRect);
			if (this.customMat != null)
				device.AddVertices(this.customMat, VertexMode.Quads, this.vertices);
			else
				device.AddVertices(this.sharedMat, VertexMode.Quads, this.vertices);
		}
		protected override void OnCopyTo(Component target, Duality.Cloning.CloneProvider provider)
		{
			base.OnCopyTo(target, provider);
			SpriteRenderer t = target as SpriteRenderer;
			t.sharedMat	= this.sharedMat;
			t.customMat	= this.customMat != null ? new BatchInfo(this.customMat) : null;
			t.rect		= this.rect;
			t.colorTint	= this.colorTint;
			t.rectMode	= this.rectMode;
			t.offset	= this.offset;
		}
	}
}

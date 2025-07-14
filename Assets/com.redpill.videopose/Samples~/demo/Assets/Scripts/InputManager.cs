using System;
using System.Linq;
using System.Collections;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;

public struct ImageBuffer
{
	public ulong id;
	public IntPtr buffer;
	public int width;
	public int height;
	public ImageBuffer(ulong id, IntPtr buffer, int width, int height)
	{
		this.id = id;
		this.buffer = buffer;
		this.width = width;
		this.height = height;
	}
}

public delegate void OnNewFrame(ImageBuffer buffer);

class TransformTexture
{
	private Material transform_material;
	public CustomRenderTexture rt { get; private set; }
	private Texture2D reader;
	public int width { get; private set; }
	public int height { get; private set; }

	public TransformTexture(Texture source, int degree, Vector2 flip=new Vector2()) : this(source, getMatrix(degree, flip)) {}
	TransformTexture(Texture source, Vector4 transform)
	{
		(width, height) = transform[0]!=0 ? (source.width, source.height) : (source.height, source.width);

		/*
		transform_material = Resources.Load<Material>("Materials/Transform");
		transform_material.mainTexture = source;
		transform_material.SetVector("_Transform", transform);
		rt = new CustomRenderTexture(width, height, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
		{
			name = "TransformTexture",
			depth = 0,
			initializationMaterial = transform_material,
			initializationMode = CustomRenderTextureUpdateMode.Realtime,
			initializationSource = CustomRenderTextureInitializationSource.Material,
		};
		/*/
		transform_material = Resources.Load<Material>("Materials/CRTTransform");
		transform_material.mainTexture = source;
		transform_material.SetVector("_Transform", transform);
		rt = new CustomRenderTexture(width, height, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
		{
			name = "TransformTexture",
			depth = 0,
			material = transform_material,
			updateMode = CustomRenderTextureUpdateMode.Realtime,
			initializationTexture = source,
			// initializationMode = CustomRenderTextureUpdateMode.OnLoad,
			// initializationSource = CustomRenderTextureInitializationSource.TextureAndColor,
		};//*/
		reader = new Texture2D(width, height, TextureFormat.ARGB32, false);	
	}
	public Color32[] GetPixels32()
	{
		RenderTexture.active = rt;
		reader.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		return reader.GetPixels32();
	}
	public IntPtr GetPtr()
	{
		// unsafe{
		// 	var request = AsyncGPUReadback.Request(rt);
		// 	request.WaitForCompletion();
		// 	return new IntPtr(request.GetData<Color32>().GetUnsafePtr());
		// }
		unsafe{fixed(void* ptr = GetPixels32()){
			return (IntPtr)ptr;
		}}
	}

	private static Vector4 degree2Matrix(int degrees)
	{
		double rad = degrees/180.0 * Math.PI;
		float c = (float)Math.Round(Math.Cos(rad)), s = (float)Math.Round(Math.Sin(rad));
		return new Vector4(c, s, -s, c);
	}
	private static Vector4 getMatrix(int degrees, Vector2 flip)
	{
		Vector4 flip_matrix = new Vector4(flip.x, flip.y, flip.x, flip.y);
		Vector4 matrix = degree2Matrix(degrees);
		return Vector4.Scale(matrix, flip_matrix);
	}
}

abstract class IInput
{
	public event OnNewFrame OnNewFrameEvent;
	protected void OnNewFrame(ImageBuffer ibuf) => OnNewFrameEvent?.Invoke(ibuf);
	protected TransformTexture transformed;
	public Texture texture => transformed.rt;
	public Vector2 flip;
	public virtual IEnumerator Start(){yield return null;}
	public virtual void Update(){}
	public virtual void Stop(){}
}

class CameraInput: IInput
{
	// private int id;
	private bool front, wide;
	private WebCamTexture webcam=null;
	public CameraInput(bool front=true, bool wide=true) 
	{
		// this.id = id;
		this.front = front;
		this.wide = wide;
	}
	public override IEnumerator Start()
	{
		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
		// if(id >= WebCamTexture.devices.Length)
		// {
			// Debug.LogError("CameraInput: invalid camera id, fallback to 0");
			// id = 0;
		// }
		var devices = WebCamTexture.devices.Where(device => device.kind==WebCamKind.UltraWideAngle||device.kind==WebCamKind.WideAngle);
		devices = front ? 
			devices.OrderByDescending(device => device.isFrontFacing) :
			devices.OrderBy(device => device.isFrontFacing);
        devices = wide ? 
			devices.OrderByDescending(device => device.kind) :
			devices.OrderBy(device => device.kind);
		var devicesArray = devices.ToArray();
		webcam = new WebCamTexture(devices.First().name, 1080, 960, 60)
        {
            name = "WebCamTexture",
            filterMode = FilterMode.Trilinear
        };
        webcam.Play();
		while (webcam.width == 16 && webcam.height == 16)
			yield return null;
		#if UNITY_ANDROID
			transformed = new TransformTexture(webcam, (webcam.videoRotationAngle+180)%360, new Vector2(-1, webcam.videoVerticallyMirrored ? 1 : -1) * flip);
		#else
			transformed = new TransformTexture(webcam, webcam.videoRotationAngle, new Vector2(-1, webcam.videoVerticallyMirrored ? 1 : -1) * flip);
		#endif
	}
	public override void Update()
	{
		// if (webcam.didUpdateThisFrame)
		OnNewFrame(new ImageBuffer(webcam.updateCount, transformed.GetPtr(), transformed.width, transformed.height));
		webcam.IncrementUpdateCount();
	}
	public override void Stop() => webcam.Stop();
}

class VideoInput: IInput
{
	private VideoPlayer player;
	public VideoInput(MonoBehaviour monoBehaviour, VideoClip clip)
	{
		player = monoBehaviour.gameObject.AddComponent<VideoPlayer>();
		player.clip = clip;
		player.targetCamera = null;
		player.isLooping = true;
		player.sendFrameReadyEvents = true;
		player.renderMode = VideoRenderMode.APIOnly;
		player.frameReady += (VideoPlayer source, long frameIdx) => OnNewFrame(new ImageBuffer((ulong)frameIdx, transformed.GetPtr(), (int)player.width, (int)player.height));
		player.Prepare();
	}
	public override IEnumerator Start()
	{
		player.Play();
		while (!player.isPrepared)
			yield return null;
		transformed = new TransformTexture(player.texture, 0, new Vector2(1, -1));
	}
	public override void Stop() => player.Stop();
}

class ImageInput : IInput
{
	public ImageInput(Texture2D image) => transformed = new TransformTexture(image, 0, new Vector2(1, -1));
	public override void Update() => OnNewFrame(new ImageBuffer(0, transformed.GetPtr(), transformed.width, transformed.height));
}

public class InputManager : MonoBehaviour
{
	public Texture2D image;
	public VideoClip clip;
	// public int camara_id;
	public bool front=true;
	public bool wide=true;
	public Vector2 flip = Vector2.one;
	public RawImage rawImage;
	private IInput input;
	public event OnNewFrame OnNewFrame;
	IEnumerator Start()
	{
		enabled = false;
		if (image != null)
			input = new ImageInput(image);
		else if (clip != null)
			input = new VideoInput(this, clip);
		else
			input = new CameraInput(front, wide);
		input.OnNewFrameEvent += (ImageBuffer ibuf) => OnNewFrame?.Invoke(ibuf);
		input.flip = flip;
		yield return input.Start();

		if(rawImage!=null)
		{
			rawImage.texture = input.texture;
			if(rawImage.gameObject.TryGetComponent<AspectRatioFitter>(out var fitter))
			{
				fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
				fitter.aspectRatio = (float)rawImage.texture.width/rawImage.texture.height;
			}
			rawImage.uvRect = new Rect(0, 1, 1, -1);
		}
		enabled = true;
	}
	void OnRenderObject() => input?.Update();
	void OnDestroy() => input?.Stop();
}

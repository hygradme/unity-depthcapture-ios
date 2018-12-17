using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class CallbackButton : MonoBehaviour {
	DepthCapture capture_;

	//depth
	int width_, height_;
	float[] pixels_;
	Texture2D texture_;
	public GameObject quad_d;

	//rgb
	int widthrgb_, heightrgb_;
	byte[] pixelsrgb_;
	Texture2D texturergb_;
	public GameObject quad_rgb;

	bool IsCapturing;

	// Use this for initialization
	IEnumerator Start () {
		IsCapturing = false;
		Debug.Log("DepthCapture.Start");
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			capture_ = new DepthCapture();
			capture_.DepthCaptured += OnDepthCaptured;

			yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

			capture_.Configure();
			capture_.Start();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (IsCapturing) {
			ApplyRGBToTexture ();
			ApplyDepthToTexture ();
		}
	}

	public void ApplyDepthToTexture(){
		if (pixels_ == null)
		{
			return;
		}

		lock (pixels_)
		{
			if (texture_ == null || texture_.width != width_ || texture_.height != height_)
			{
				texture_ = new Texture2D(width_, height_);
				quad_d.GetComponent<Renderer>().material.mainTexture = texture_;
			}

			for (var y = 0; y < (int)height_; y++)
			{
				for (var x = 0; x < (int)width_; x++)
				{
					var v = pixels_[y * width_ + x];
					Color color;
					if (float.IsNaN(v))
					{
						color = new Color(0f, 1f, 0f);
					}
					else
					{
						color = new Color(v, v, v);
					}
					texture_.SetPixel(x, y, color);
				}
			}
		}

		texture_.Apply();
	}

	public void ApplyRGBToTexture(){
		if (pixelsrgb_ == null)
		{
			return;
		}

		lock (pixelsrgb_)
		{
			if (texturergb_ == null || texturergb_.width != widthrgb_ || texturergb_.height != heightrgb_)
			{
				texturergb_ = new Texture2D(widthrgb_, heightrgb_);
				quad_rgb.GetComponent<Renderer>().material.mainTexture = texturergb_;
			}

			for (var y = 0; y < (int)heightrgb_; y++)
			{
				for (var x = 0; x < (int)widthrgb_; x++)
				{
					var offset = (y * widthrgb_ + x) * 4;
					Color color;
					color = new Color(pixelsrgb_[offset+2]/255f, pixelsrgb_[offset+1]/255f, pixelsrgb_[offset+0]/255f);
					texturergb_.SetPixel(x, y, color);
				}
			}
		}
		texturergb_.Apply();
	}

	public void OnClick()
	{
		Debug.Log("OnClick");
		IsCapturing = !IsCapturing;
	}
		
	private void OnDepthCaptured(IntPtr pVideoData,int videoWidth,int videoHeight,IntPtr pDepthData,int depthWidth,int depthHeight){
		//depth
		if (pixels_ == null || width_ != depthWidth || height_ != depthHeight)
		{
			width_ = depthWidth;
			height_ = depthHeight;
			pixels_ = new float[depthWidth * depthHeight];
		}
		lock (pixels_)
		{
			Marshal.Copy(pDepthData, pixels_, 0, depthWidth * depthHeight);
		}

		//rgb
		if (pixelsrgb_ == null || widthrgb_ != videoWidth || heightrgb_ != videoHeight)
		{
			widthrgb_ = videoWidth;
			heightrgb_ = videoHeight;
			pixelsrgb_ = new byte[videoWidth * videoHeight*4];
		}
		lock (pixelsrgb_)
		{
			Marshal.Copy(pVideoData, pixelsrgb_, 0, videoWidth * videoHeight*4);
		}

	}
}

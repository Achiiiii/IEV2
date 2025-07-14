using System.Runtime.InteropServices;
public class MiniAudio
{
	public delegate void AudioDataCallback(short[] data, ulong size);
	public delegate void AudioCallback(short[] data);
#if UNITY_EDITOR
	private static void SetAudioCallback(AudioDataCallback callback){}
	public static void StartMicrophone(int id, int samplerate, int channels){}
	public static void StopMicrophone(){}
#else
	private const string lib = "MiniAudio";
	[DllImport(lib)]
	private static extern void SetAudioCallback(AudioDataCallback callback);
	[DllImport(lib)]
	public static extern void StartMicrophone(int id, int samplerate, int channels);
	[DllImport(lib)]
	public static extern void StopMicrophone();
#endif
	private static AudioCallback callback=null;
	[AOT.MonoPInvokeCallback(typeof(AudioCallback))]
	private static void OnAudioData([MarshalAs(
			UnmanagedType.LPArray,
			ArraySubType = UnmanagedType.I2, 
			SizeParamIndex = 1
		)]short[] data, ulong size)
	{
		if(callback!=null)
			callback(data);
	}
	public static void SetAudioCallback(AudioCallback callback)
	{
		MiniAudio.callback = callback;
		SetAudioCallback(OnAudioData);
	}
}

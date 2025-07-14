using System.Runtime.InteropServices;

public class PhonemeLibrary
{
#if UNITY_EDITOR
	public static int Lipsync_Init() => 0;
	public static int Lipsync_GetVisemes(short[] data, int data_length, ref int label_id)=>0;
	public static int Lipsync_Deinit()=>0;
#else
	private const string lib = "VideoPose";
	[DllImport(lib)]
	public static extern int Lipsync_Init();
	[DllImport(lib)]
	public static extern int Lipsync_GetVisemes(short[] data, int data_length, ref int label_id);
	[DllImport(lib)]
	public static extern int Lipsync_Deinit();
#endif

}

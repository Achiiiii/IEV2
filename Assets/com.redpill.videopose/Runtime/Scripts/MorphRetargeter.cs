using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public interface IMorphSource
{
	public float[] GetMorphs();
}

public class ListToPopupAttribute : PropertyAttribute
{
	public Type myType;
	public string propertyName;
	public ListToPopupAttribute(Type _myType, string _propertyName)
	{
		myType = _myType;
		propertyName = _propertyName;
	}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ListToPopupAttribute))]
public class ListToPopupDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		ListToPopupAttribute atb = attribute as ListToPopupAttribute;
		List<string> stringList = new List<string>();
		var listproperty = property.serializedObject.FindProperty(atb.propertyName);

		for (int i = 0; i < listproperty.arraySize; i++)
			stringList.Add(listproperty.GetArrayElementAtIndex(i).stringValue);

		int selectedIndex = stringList.IndexOf(property.stringValue);
		if (selectedIndex < 0) selectedIndex = 0;

		// 新增一個空白選項作為預設
		List<string> popupOptions = new List<string> { "<None>" };
		popupOptions.AddRange(stringList);

		// Popup 顯示，+1 是因為 "<None>" 為第0個選項
		selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex + 1, popupOptions.ToArray());

		// 回寫值（-1 代表 "<None>"）
		if (selectedIndex == 0)
		{
			property.stringValue = "";
		}
		else
		{
			property.stringValue = stringList[selectedIndex - 1];
		}
	}

}
#endif

[Serializable]
public struct FaceMapping
{
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> AA;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> EE;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> IY;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> OO;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> UU;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> ER;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> LL;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> WW;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> DT;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> CH;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> MBP;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> FV;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> Blink;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> Happy;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> Sad;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> BrowsUp;
	[ListToPopup(typeof(MorphRetargeter), "allBlends")]
	public List<string> BrowsDown;
}
struct MorphMap
{
	public SkinnedMeshRenderer mesh;
	public int id;
}

public static class IEnumerableExtentions
{
	public static IEnumerable<(T item, int index)> Enumerate<T>(this IEnumerable<T> self) => self.Select((item, index)=>(item, index));
}

public class MorphRetargeter : MonoBehaviour, ISerializationCallbackReceiver
{
	[HideInInspector]
	public List<string> allBlends = new List<string>();
	Dictionary<string, MorphMap> morph_map = new Dictionary<string, MorphMap>();
	[SerializeField]
	public FaceMapping faceMapping;
	private IMorphSource morphSource;
	public void Start()
	{
		morphSource = GetComponent<IMorphSource>();
		GetAllMorphs();
	}

	private List<string> GetAllMorphs()
	{
		var BlendshapeList = new List<string>();
		morph_map.Clear();
		Component[] meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer mesh in meshes)
		{
			var skinnedMesh = mesh.sharedMesh;
			var blendShapeCount = skinnedMesh.blendShapeCount;
			for (int i = 0; i < blendShapeCount; i++)
			{
				string thisName = skinnedMesh.GetBlendShapeName(i);
				//thisName = thisName.Replace (filterText, "");

				BlendshapeList.Add(mesh.name + "." + thisName);
				MorphMap map = new MorphMap
				{
					mesh = mesh,
					id = i
				};
				morph_map.Add(mesh.name + "." + thisName, map);
			}
		}
		return BlendshapeList;
	}
	public void OnBeforeSerialize()
	{
		allBlends = GetAllMorphs();
	}
	public void OnAfterDeserialize(){ }
	private void UpdateMorph()
	{
		var props = faceMapping.GetType().GetFields();
		var sourceMorphs = morphSource.GetMorphs();
		Dictionary<int, float> dict = new Dictionary<int, float>();
		foreach (var (prop, id) in props.Enumerate())
		{
			foreach(var bs in prop.GetValue(faceMapping) as List<string>)
			{
				MorphMap map = morph_map[bs];
				if(!dict.TryGetValue(map.id, out float p))
					dict[map.id] = 0;
				dict[map.id] = Math.Max(dict[map.id], sourceMorphs[id+1]*100);
			}
		}

		foreach (var prop in props)
		{
			foreach(var bs in prop.GetValue(faceMapping) as List<string>)
			{
				MorphMap map = morph_map[bs];
				map.mesh.SetBlendShapeWeight(map.id, dict[map.id]);
			}
		}
	}

	public void Update()
	{
		UpdateMorph();
	}
	
	// public void OnGUI()
	// {
	// 	GUIStyle style = new GUIStyle(GUI.skin.label){fontSize = 80};
	// 	style.normal.textColor = Color.green;
	// 	GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
	// 	GUILayout.Label("", style);
	// 	GUILayout.Label("", style);

	// 	HashSet<string> set = new HashSet<string>();
	// 	foreach (var prop in faceMapping.GetType().GetFields())
	// 	{
	// 		foreach(var bs in prop.GetValue(faceMapping) as List<string>)
	// 		{
	// 			if(!set.Contains(bs))
	// 			{
	// 				MorphMap map = morph_map[bs];
	// 				var n = bs.Split(".").Last();
	// 				var w = map.mesh.GetBlendShapeWeight(map.id);
	// 				GUILayout.Label(n + ": " + w.ToString("0.0000"), style);
	// 				set.Add(bs);
	// 			}
	// 		}
	// 	}		
	// 	GUILayout.EndVertical();
	// }
}
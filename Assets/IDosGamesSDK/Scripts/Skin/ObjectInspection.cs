using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
	public class ObjectInspection : ScriptableObject
	{
		/*
		[Space(5)]
		[SerializeField] private Material _inspectionMaterial;
		public Material InspectionMaterial => _inspectionMaterial;
		*/
		[Space(5)]
		[SerializeField] private List<ObjectInspectionData> _objectInspectionData = new();

		public ObjectInspectionData GetInspectionData(string objectType)
		{
			objectType = objectType.Trim();

			return _objectInspectionData.FirstOrDefault(x => x.ObjectType.Trim() == objectType);
		}
	}
}
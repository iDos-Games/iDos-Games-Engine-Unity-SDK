using System;
using UnityEngine;

namespace IDosGames
{
	[Serializable]
	public class ObjectInspectionData
	{
		[Space(5)]
		[SerializeField] private string _objectType;
		public string ObjectType => _objectType;

		[Space(5)]
		[SerializeField] private GameObject _model;
		public GameObject Model => _model;

        [Space(5)]
        [SerializeField] private Material _modelMaterial;
        public Material ModelMaterial => _modelMaterial;

        [Space(5)]

		[Header("Transform")]
		[Space(5)]
		[SerializeField] private Vector3 _position;
		public Vector3 Position => _position;

		[SerializeField] private Vector3 _rotation;
		public Vector3 Rotation => _rotation;

		[SerializeField] private Vector3 _scale = Vector3.one;
		public Vector3 Scale => _scale;

		[Space(5)]
		[SerializeField] private AdditionalInspectionObject _additionalObject;
		public AdditionalInspectionObject AdditionalObject => _additionalObject;
	}
}

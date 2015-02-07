using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AMRotationKey : AMKey {
	
	//public int type = 0; // 0 = Rotate To, 1 = Look At
	private Quaternion _rotation;
	public Vector3 rotationValues;
	public Quaternion rotation {
		get {
			return _rotation;	
		}
		set {
			_rotation = value;
			rotationValues = new Vector3(_rotation.x, _rotation.y, _rotation.z);	// re-check for methodinfo
		}
	}


	public bool setRotation(Vector3 rotation) {
		if(this.rotation != Quaternion.Euler(rotation)) {
			this.rotation = Quaternion.Euler(rotation);
			this.rotationValues = rotation;
			return true;
		}
		return false;
	}
	public Vector3 getRotation() {
		return 	rotation.eulerAngles;
	}
	/*public bool setType(int type) {
		if(this.type != type) {
			this.type = type;
			return true;
		}
		return false;
	}*/
	// copy properties from key
	public override AMKey CreateClone ()
	{
		
		AMRotationKey a = ScriptableObject.CreateInstance<AMRotationKey>();
		a.frame = frame;
		//a.type = type;
		a.rotation = rotation;
		a.easeType = easeType;
		a.customEase = new List<float>(customEase);
		
		return a;
	}
	
}

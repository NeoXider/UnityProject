using UnityEngine;
using System.Collections;

namespace ArcReactor.Demo
{
	public class ArcReactorDemoGunController : MonoBehaviour {

		public GunDefinition[] guns;
		public int selectedGun;

		[System.Serializable]
		public class GunDefinition
		{
			public ArcReactor_Launcher launcher;
			public float rechargeTime;
			public float currentRechargeTimeLeft;
			public bool continuous;
			public KeyCode keycode;
		}

		void Start ()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		// Update is called once per frame
		void Update () 
		{
			for (int i = 0; i < guns.Length; i++)
			{
				if (Input.GetKeyDown(guns[i].keycode))
				{
					selectedGun = i;
					break;
				}
				guns[i].currentRechargeTimeLeft -= Time.deltaTime;
				if (guns[i].currentRechargeTimeLeft < 0)
					guns[i].currentRechargeTimeLeft = 0;
			}
			
				
			if (guns[selectedGun].continuous)
			{
				if (Input.GetMouseButtonDown(0) && guns[selectedGun].launcher.Rays.Count == 0)
				{
					guns[selectedGun].launcher.LaunchRay();
				}
				if (Input.GetMouseButtonUp(0))
				{
					foreach (ArcReactor_Launcher.RayInfo ray in guns[selectedGun].launcher.Rays)
					{
						ray.arc.playBackward = true;
						ray.arc.freeze = false;
					}
				}
			}
			else
			{
				if (Input.GetMouseButton(0) && guns[selectedGun].currentRechargeTimeLeft == 0)
				{
					guns[selectedGun].launcher.LaunchRay();
					guns[selectedGun].currentRechargeTimeLeft = guns[selectedGun].rechargeTime;
				}
			}
		
		}
	}
}
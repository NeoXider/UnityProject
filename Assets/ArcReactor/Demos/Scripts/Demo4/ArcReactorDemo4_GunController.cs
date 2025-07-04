﻿using UnityEngine;
using System.Collections;

namespace ArcReactor.Demo
{
	public class ArcReactorDemo4_GunController : MonoBehaviour {

		public float speed;
		public ArcReactor_Launcher2D launcher;
		public ArcReactorDemo4_Manager manager;

		protected float detachTimer = 0;


		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () 
		{
			if (detachTimer > 0)
			{
				detachTimer -= Time.deltaTime;
				if (detachTimer < 0)
				{
					detachTimer = 0;
					launcher.DetachAllArcsFromLauncher();
				}
			}
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.RightArrow))
				transform.Rotate(new Vector3(0,0,speed * Time.deltaTime));
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow))
				transform.Rotate(new Vector3(0,0,-speed * Time.deltaTime));
			if (Input.GetKeyDown(KeyCode.Space))
			{
				launcher.LaunchRay();
				detachTimer = 0.1f;
				manager.points -= manager.mirrorGen.mirrorCount/3;
			}
		}
	}
}
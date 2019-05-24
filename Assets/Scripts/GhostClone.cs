﻿using CAVS.ProjectOrganizer.Controls;
using CAVS.ProjectOrganizer.Interation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.GrabAttachMechanics;

namespace VRTK
{
    namespace CAVS.ProjectOrganizer.Interation
    {

        public class GhostClone : MonoBehaviour
        {

            private VRTK_InteractableObject InteractObjScript;
            //CNG private VRTK_ChildOfControllerGrabAttach GrabAttachScript;
            private VRTK_FixedJointGrabAttach GrabAttachScript;

            private GameObject thisClone, clonable, globalCanvasScript;
            private DICanvas DICanvasScript;
            private VRTK_ControllerEvents Hand;
            private VRTK_InteractGrab GrabScript;
            private VRTK_InteractTouch handTouch;
            private Transform headsetTransform;
            
            // Use this for initialization
            void Start()
            {
                if(headsetTransform == null) headsetTransform = VRTK_DeviceFinder.HeadsetTransform();
                if(thisClone == null) thisClone = new GameObject();
                if(clonable == null) clonable = new GameObject();
                if(GrabScript == null) GrabScript = new VRTK_InteractGrab();
                if(Hand == null)Hand = GameObject.FindObjectOfType<VRTK_ControllerEvents>(); //CNG
                if(handTouch == null)handTouch = GameObject.FindObjectOfType<VRTK_InteractTouch>();
                if(globalCanvasScript == null) globalCanvasScript = new GameObject();
                if(GrabAttachScript == null) GrabAttachScript = new VRTK_FixedJointGrabAttach();

                //CNG
                DICanvasScript = FindObjectOfType<DICanvas>();
                if (DICanvasScript == null)
                {
                    DICanvasScript = globalCanvasScript.AddComponent<DICanvas>();
                }
                //Debug.Log(DICanvasScript.gameObject.name);

                VRTK_Button[] buttons = GameObject.FindObjectsOfType<VRTK_Button>();
                foreach (VRTK_Button thisButton in buttons)
                {
                    if (thisButton.gameObject == null) Destroy(thisButton);
                }

            }

            // Update is called once per frame
            void Update()
            {                
            }


            /// <summary>
            /// This function is called by the object which can be cloned.
            /// 
            /// </summary>
            /// <param name="targetClonable"></param>
            /// <param name="spawnLocation"></param>
            public void createGC(GameObject targetClonable, Transform spawnLocation)
            {
                headsetTransform = VRTK_DeviceFinder.HeadsetTransform();

                //"There can be only one."
                cleanupOldClones();

                //Instantiate the clonable at the target position
                clonable = targetClonable;
                thisClone = Instantiate(targetClonable, spawnLocation.forward, spawnLocation.rotation);
                thisClone.transform.position = new Vector3(thisClone.transform.position.x, headsetTransform.position.y, thisClone.transform.position.z);
                thisClone.transform.rotation = targetClonable.transform.rotation;
                thisClone.transform.localScale = targetClonable.transform.localScale;                

                //CNG PARENTING CHANGED TO ObjectFollow
                /*
                //set thisClone as parent so it will affect the clonable
                clonable.transform.SetParent(thisClone.transform, false);
                //clonable.transform.parent = thisClone.transform;
                */

                //Make empty for the ObjectFollow scripts
                GameObject followObject = new GameObject("PositionOffset");
                followObject.transform.position = clonable.transform.position;
                followObject.tag = "temporary";

                //attach positionFollow script to positionOffset object targeting GClone, affecting the clonable                
                VRTK_TransformFollow followPositionScript = followObject.AddComponent<VRTK_TransformFollow>();                
                followPositionScript.gameObjectToChange = followObject;
                followPositionScript.gameObjectToFollow = thisClone;
                followPositionScript.followsPosition = true;
                followPositionScript.followsRotation = false;

                //set the parent of the cloneable = positionOffset object
                //scaling fix: manual
                Transform scaleFix = fixScaling(clonable);                
                clonable.transform.SetParent(followObject.transform, false);
                clonable.transform.localScale = new Vector3(scaleFix.localScale.x, scaleFix.localScale.y, scaleFix.localScale.z);
                thisClone.transform.localScale = new Vector3(scaleFix.localScale.x, scaleFix.localScale.y, scaleFix.localScale.z);
                Debug.Log(scaleFix.ToString());

                //add objectFollow(rotation) affecting the clonable targeting GClone
                VRTK_TransformFollow followRotationScript = followObject.AddComponent<VRTK_TransformFollow>();
                followRotationScript.gameObjectToChange = clonable;
                followRotationScript.gameObjectToFollow = thisClone;
                followRotationScript.followsPosition = false;
                followRotationScript.followsRotation = true;


                //copy this script, and attach it to the clone
                thisClone.AddComponent<GhostClone>();

                //continue setup from the new script (which is ON the clone), rather than this one, which hangs out in the scene
                thisClone.GetComponent<GhostClone>().setupGC(clonable, thisClone);
            }

            private Transform fixScaling(GameObject clonable)
            {
                Transform scaleFix = new GameObject().transform;
                scaleFix.localScale = new Vector3(clonable.transform.localScale.x, clonable.transform.localScale.y, clonable.transform.localScale.z);
                return scaleFix;
            }

            private void cleanupOldClones()
            {
                GameObject[] temporary = GameObject.FindGameObjectsWithTag("temporary");
                foreach (GameObject thisTemp in temporary)
                {
                    if (thisTemp.GetComponent<GhostClone>() != null) Destroy(thisTemp);
                    if (thisTemp.GetComponent<VRTK_TransformFollow>() != null) {
                        Transform[] children = thisTemp.GetComponentsInChildren<Transform>();
                        foreach (Transform thisChild in children) thisChild.SetParent(null);
                        Destroy(thisTemp);
                    } 
                }
            }

            public void setupGC(GameObject targetClonable, GameObject myClone)
            {                
                //CNG
                DICanvasScript = FindObjectOfType<DICanvas>();
                if (DICanvasScript == null)
                {
                    DICanvasScript = globalCanvasScript.AddComponent<DICanvas>();
                }
                //Debug.Log(DICanvasScript.gameObject.name);

                //Assign GameObject references
                clonable = targetClonable;
                thisClone = myClone;

                //Make the clone more transparent to distinguish it
                var trans = 0.3f;
                var col = thisClone.GetComponent<Renderer>().material.color;
                col.a = trans;

                //set tag and name so we can clean it up, find it later, and distinguish it in the editor
                thisClone.name = clonable.name + "(Ghost Clone)";
                thisClone.tag = "temporary";

                //Remove the script from thisClone that allows a clone to be made from it
                try { Destroy(thisClone.GetComponent<Cloneable>()); }
                catch { Debug.Log("No Clonable Script to remove."); }

                //Give grab attach if doesn't have
                InteractObjScript = thisClone.GetComponent<VRTK_InteractableObject>();
                //CNG GrabAttachScript = thisClone.GetComponent<VRTK_ChildOfControllerGrabAttach>();
                GrabAttachScript = thisClone.GetComponent<VRTK_FixedJointGrabAttach>();
                
                
                Hand = GameObject.FindObjectOfType<VRTK_ControllerEvents>(); //CNG
                //Finish script setup
                setupInteractScript(InteractObjScript, GrabAttachScript, Hand);

                

                //Give RigidBody if doesn't have
                if (thisClone.GetComponent<Rigidbody>() == null) thisClone.AddComponent<Rigidbody>();

                //Unfreeze the rigidbody so it can be modified
                thisClone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                thisClone.GetComponent<Rigidbody>().useGravity = false;
                thisClone.GetComponent<Rigidbody>().isKinematic = true;



                //Create the canvas which will let us modify the constraints on this clone
                DICanvasScript.createDICanvas(thisClone, thisClone.transform);
            }

            internal void releaseChildren()
            {                
                clonable.transform.SetParent(null);                
            }

            private void setupInteractScript(VRTK_InteractableObject IOScript, VRTK_FixedJointGrabAttach GScript, VRTK_ControllerEvents CEvents)
            {
                if (IOScript == null)
                {              
                    InteractObjScript = thisClone.AddComponent<VRTK_InteractableObject>();
                }
                else InteractObjScript = IOScript;
                if (GScript == null)
                {            
                    GrabAttachScript = thisClone.AddComponent<VRTK_FixedJointGrabAttach>();
                }
                else GrabAttachScript = GScript;
                if (CEvents == null)
                {
                    Debug.Log("Created new Controller Events");
                    Hand = new VRTK_ControllerEvents();
                }
                else Hand = CEvents;

                if (handTouch == null) handTouch = Hand.gameObject.GetComponent<VRTK_InteractTouch>();
                if (handTouch == null) handTouch = Hand.gameObject.AddComponent<VRTK_InteractTouch>();
                
                GrabScript = Hand.gameObject.GetComponent<VRTK_InteractGrab>();
                if (GrabScript== null) GrabScript = Hand.gameObject.AddComponent<VRTK_InteractGrab>();
                GrabScript.grabButton = VRTK_ControllerEvents.ButtonAlias.GripPress;
                GrabScript.controllerEvents = Hand;

                GrabAttachScript.precisionGrab = true;

                InteractObjScript.grabAttachMechanicScript = GrabAttachScript;
                InteractObjScript.holdButtonToGrab = true;
                InteractObjScript.isGrabbable = true;
                InteractObjScript.stayGrabbedOnTeleport = true;
                InteractObjScript.touchHighlightColor = Color.yellow;
                InteractObjScript.grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.GripPress;                                                                           
            }

            void OnCollisionEnter(Collision collision)
            {
                Debug.Log("Collision Detected");
                if (collision.gameObject.tag == "controller")
                {
                    Debug.Log("Collision == Controller");
                    Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), collision.gameObject.GetComponent<Collider>());
                }
            }

            }//end of class
    }//end of CAVS namespace
}//end of VRTK namespace
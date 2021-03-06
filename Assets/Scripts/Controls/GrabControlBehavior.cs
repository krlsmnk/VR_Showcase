﻿using UnityEngine;
using VRTK;
using CAVS.ProjectOrganizer.Interation;

namespace CAVS.ProjectOrganizer.Controls
{

    public class GrabControlBehavior : MonoBehaviour
    {
        private class ObjectState
        {
            Rigidbody rigidbody;

            RigidbodyConstraints rigidbodyConstraints;

            bool colliderIsEnabled;

            Transform parent;

            public ObjectState(GameObject gameObject)
            {
                var col = gameObject.GetComponent<Collider>();
                if (col != null)
                {
                    colliderIsEnabled = col.enabled;
                }

                rigidbody = gameObject.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbodyConstraints = rigidbody.constraints;
                }

                parent = gameObject.transform.parent;
            }

            public void Restore(GameObject gameObject, Vector3 currentControllerVelocity)
            {
                var col = gameObject.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = colliderIsEnabled;
                }

                if (rigidbody != null)
                {
                    rigidbody.constraints = rigidbodyConstraints;
                    rigidbody.velocity = currentControllerVelocity;
                }

                gameObject.transform.SetParent(parent);
            }
        }

        private LineRenderer pointer;

        private VRTK_ControllerEvents hand;

        private VRTK_InteractableObject interactableObject;

        private float distanceOfObjectFromController;

        private ObjectState objectStateOnGrab;

        private Vector3 objectPositionLastFrame;

        private SnappingZoneBehavior[] snappingZones;

        private VRTK_RadialMenu radialMenu;

        public static GrabControlBehavior Initialize(VRTK_ControllerEvents hand)
        {
            var newScript = hand.gameObject.AddComponent<GrabControlBehavior>();
            newScript.TurnOnPointer();
            newScript.hand = hand;
            newScript.interactableObject = null;
            newScript.radialMenu = hand.gameObject.GetComponentInChildren<VRTK_RadialMenu>();

            newScript.hand.GripPressed += newScript.Hand_GripPressed;
            newScript.hand.TriggerClicked += newScript.Hand_TriggerPressed;
            newScript.hand.TriggerUnclicked += newScript.Hand_TriggerReleased;
            newScript.hand.TouchpadAxisChanged += newScript.TouchpadAxisChanged;
            newScript.hand.TouchpadTouchEnd += newScript.Hand_TouchpadTouchEnd;

            return newScript;
        }


        private Vector3 Discritize(Vector3 pos)
        {
            var cleanedPos = pos;
            foreach (var zone in snappingZones)
            {
                if (zone.InZone(pos))
                {
                    cleanedPos = zone.Discritize(cleanedPos);
                }
            }
            return cleanedPos;
        }

        private Quaternion Discritize(Quaternion rot, Vector3 pos)
        {
            var cleanedRot = rot;
            foreach (var zone in snappingZones)
            {
                if (zone.InZone(pos))
                {
                    cleanedRot = zone.Discritize(cleanedRot);
                }
            }
            return cleanedRot;
        }

        float lastTouchpadY = 0;

        float currentTouchpadY = 0;

        /// <summary>
        /// A value I played with a lot till I got something I liked
        /// </summary>
        float frictionConstant = 4f;

        float lastTimeAxisChanged;

        float axisVelocity = 0;

        private void Hand_TouchpadTouchEnd(object sender, ControllerInteractionEventArgs e)
        {
            currentTouchpadY = -666;
            lastTouchpadY = -666;
        }

        private void TouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
        {
            if (interactableObject != null)
            {
                if (currentTouchpadY != -666)
                {
                    distanceOfObjectFromController = Mathf.Clamp(distanceOfObjectFromController + (e.touchpadAxis.y - currentTouchpadY), 0, 1000f);
                }
                lastTouchpadY = currentTouchpadY;
                currentTouchpadY = e.touchpadAxis.y;
                
                if (currentTouchpadY != -666 && lastTouchpadY != -666)
                {
                    axisVelocity = (currentTouchpadY - lastTouchpadY)/Time.deltaTime;
                }
            }
        }

        private void UpdateInteractableObject(VRTK_InteractableObject newInteractable)
        {
            if(newInteractable == null)
            {
                pointer.startColor = Color.grey;
                pointer.endColor = Color.grey;
            } else
            {
                pointer.startColor = Color.cyan;
                pointer.endColor = Color.cyan;
            }

            if (newInteractable == interactableObject)
            {
                return;
            }

            interactableObject = newInteractable;
        }

        private void Hand_TriggerReleased(object sender, ControllerInteractionEventArgs e)
        {
            if (interactableObject != null)
            {
                objectStateOnGrab.Restore(interactableObject.gameObject, ((interactableObject.transform.position - objectPositionLastFrame) / Time.deltaTime) * .2f);

                interactableObject = null;
                objectStateOnGrab = null;
                radialMenu.gameObject.SetActive(true);
            }
        }

        private void Hand_TriggerPressed(object sender, ControllerInteractionEventArgs e)
        {
            if (interactableObject != null && objectStateOnGrab == null)
            {
                objectStateOnGrab = new ObjectState(interactableObject.gameObject);
                distanceOfObjectFromController = Vector3.Distance(interactableObject.transform.position, transform.position);
                interactableObject.transform.position = transform.position;

                radialMenu.gameObject.SetActive(false);

                var col = interactableObject.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                var rb = interactableObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                }
            }
        }

        private void Hand_GripPressed(object sender, ControllerInteractionEventArgs e)
        {
            if (PointerIsOn())
            {
                TurnOffPointer();
            }
            else
            {
                TurnOnPointer();
            }
        }

        void Start()
        {
            snappingZones = GameObject.FindObjectsOfType<SnappingZoneBehavior>();
        }

        void Update()
        {
            if (pointer == null)
            {
                return;
            }

            pointer.SetPosition(0, transform.position);

            float distanceForObject = distanceOfObjectFromController;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                if (objectStateOnGrab == null)
                {
                    UpdateInteractableObject(hit.transform.gameObject.GetComponent<VRTK_InteractableObject>());
                }
                pointer.SetPosition(1, hit.point);

                distanceForObject = Mathf.Min(distanceForObject, hit.distance);
            }
            else
            {
                if (objectStateOnGrab == null)
                {
                    UpdateInteractableObject(null);
                }
                pointer.SetPosition(1, transform.position + (transform.rotation * Vector3.forward * 100));
            }


            if (objectStateOnGrab != null)
            {
                if(hand.touchpadTouched == false)
                {
                    distanceOfObjectFromController = Mathf.Clamp(distanceOfObjectFromController + (axisVelocity * Time.deltaTime), 0, 1000f);
                }
                axisVelocity *= 1f - (frictionConstant * Time.deltaTime);

                objectPositionLastFrame = interactableObject.transform.position;
                interactableObject.transform.position = Discritize((transform.forward * distanceForObject) + transform.position);
                interactableObject.transform.rotation = Discritize(interactableObject.transform.rotation, interactableObject.transform.position);

                //InteractableScreen
                var restrained = interactableObject as RestrainedInteractableObject;
                if (restrained != null)
                {
                    interactableObject.transform.position = restrained.GetAvailablePositionFromDesired(interactableObject.transform.position, interactableObject.transform.rotation);
                    interactableObject.transform.rotation = restrained.GetAvailableRotationFromDesired(interactableObject.transform.position, interactableObject.transform.rotation);
                }
            }
        }

        private bool PointerIsOn()
        {
            return pointer != null;
        }

        private void TurnOnPointer()
        {
            if (PointerIsOn())
            {
                return;
            }
            var pointerParent = new GameObject("Pointer Parent");
            pointerParent.transform.SetParent(transform);
            pointerParent.transform.localPosition = Vector3.zero;
            pointer = pointerParent.AddComponent<LineRenderer>();
            if (pointer != null)
            {
                pointer.material = new Material(Shader.Find("Sprites/Default"));
                pointer.startColor = Color.grey;
                pointer.endColor = Color.grey;
                pointer.positionCount = 2;
                pointer.startWidth = .025f;
                pointer.endWidth = .025f;
            }

        }

        private void TurnOffPointer()
        {
            if (PointerIsOn() == false)
            {
                return;
            }
            Destroy(pointer.gameObject);
        }

        private void OnDestroy()
        {
            if (PointerIsOn())
            {
                TurnOffPointer();
            }
            if (interactableObject != null && objectStateOnGrab != null)
            {
                objectStateOnGrab.Restore(interactableObject.gameObject, ((interactableObject.transform.position - objectPositionLastFrame) / Time.deltaTime) * .2f);

            }
            UpdateInteractableObject(null);
            hand.GripPressed -= Hand_GripPressed;
            hand.TriggerClicked -= Hand_TriggerPressed;
            hand.TriggerUnclicked -= Hand_TriggerReleased;
            hand.TouchpadAxisChanged -= TouchpadAxisChanged;
            hand.TouchpadTouchEnd -= Hand_TouchpadTouchEnd;
        }
    }

}
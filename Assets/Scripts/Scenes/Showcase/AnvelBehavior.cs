﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CAVS.Anvel;
using CAVS.Anvel.Lidar;
using CAVS.Anvel.Vehicle;


namespace CAVS.ProjectOrganizer.Scenes.Showcase
{
    public class AnvelBehavior : MonoBehaviour
    {

        enum DisplayType
        {
            Networked,
            File
        }

        [SerializeField]
        private GameObject objectForOffset;

        [SerializeField]
        private DisplayType startup;

        [SerializeField]
        private LiveDisplayBehavior.LidarEntry[] lidarSensors;

        [SerializeField]
        private string anvelVehicleName;

        [SerializeField]
        private string anvelIP;

        [SerializeField]
        private int anvelPort;

        // Use this for initialization
        void Start()
        {
            switch (startup)
            {
                case DisplayType.File:
                    gameObject.AddComponent<FileDisplayBehavior>().Initialize(
                        LidarSerialization.Load("360 Lidar-11.pcrp"),
                        VehicleLoader.LoadVehicleData("vehicle1_pos_2.vprp")
                    );
                    break;

                case DisplayType.Networked:
                    var display = gameObject.AddComponent<LiveDisplayBehavior>();
                    display.Initialize(
                        ConnectionFactory.CreateConnection(),
                        lidarSensors,
                        anvelVehicleName,
                        new Vector3(0.1f, 0.1f, 2.27f),
                        new Vector3(0, 90, 0)
                     );
                    StartCoroutine(UpdateOffsetTick(display));
                    break;
            }

        }

        IEnumerator UpdateOffsetTick(LiveDisplayBehavior displayBehavior)
        {
            while(true)
            {
                if(objectForOffset == null)
                {
                    break;
                }
                displayBehavior.UpdateCenterOffset(objectForOffset.transform.position);
                displayBehavior.UpdateRotationOffset(objectForOffset.transform.rotation.eulerAngles);
                yield return null;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
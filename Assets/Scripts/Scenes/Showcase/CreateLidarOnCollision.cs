﻿using UnityEngine;
using CAVS.Anvel;
using AnvelApi;

namespace CAVS.ProjectOrganizer.Scenes.Showcase
{
    public class CreateLidarOnCollision : CreateAnvelObjectOnCollision
    {
        protected override void CreateApprorpiateAnvelObject()
        {
            string name = $"Lidar - {Random.Range(0, 1000000)}"; // Because anvel  won't allow two objects to have the same name, despite having two different keys

            var baseObj = connection.CreateObject(AssetName.Sensors.API_3D_LIDAR, name , parent.ObjectDescriptor().ObjectKey, new Point3(), new Euler(), false);
            objectWeArecontrolling = new AnvelObject(connection, baseObj, false);
            //objectSensorWeArecontrolling = new AnvelObject(connection, connection.GetObjectDescriptorByTypeAndName("APILidar", name), false);
            objectWeArecontrolling.UpdateTransform(transform.localPosition, transform.localRotation);
            FindObjectsOfType<LiveDisplayBehavior>()[0].AddLidar(name, Color.blue, objectWeArecontrolling.ObjectDescriptor());

            //var t = connection.EnumeratePropertyNames(objectWeArecontrolling.ObjectDescriptor().ObjectKey);
            //foreach (var prop in t)
            //{
            //    Debug.Log(prop.ToString());
            //}
        }

        protected override string PropertyKeyForModifying()
        {
            return "Range";
        }

        protected override Vector2 PropertyRangeForModifying()
        {
            return new Vector2(1, 100);
        }

        protected override float PropertyStartingValueForModifying()
        {
            return 75f;
        }
    }

}
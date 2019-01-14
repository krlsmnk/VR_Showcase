﻿using UnityEngine;
using AnvelApi;

namespace CAVS.ProjectOrganizer.Scenes.Showcase
{
    public class AnvelObject
    {
        private ObjectDescriptor objectDescriptor;

        private AnvelControlService.Client client;

        public AnvelObject(AnvelControlService.Client connection, ObjectDescriptor descriptor, bool newlyCreated)
        {
            client = connection;
            objectDescriptor = descriptor;

            if (newlyCreated)
            {
                AnvelObjectManager.Instance.RegisterCreatedObject(this);
            }
        }

        public static AnvelObject GetReferenceByName(AnvelControlService.Client connection, string objectName)
        {
            return new AnvelObject(connection, connection.GetObjectDescriptorByName(objectName), false);
        }

        public static AnvelObject CreateObject(AnvelControlService.Client connection, string objectName, string assetName, ObjectDescriptor parent)
        {
            return new AnvelObject(connection, connection.CreateObject(assetName, objectName, parent == null ? 0 : parent.ObjectKey, new Point3(), new Euler(), false), true);
        }

        public static AnvelObject CreateObject(AnvelControlService.Client connection, string objectName, string assetName)
        {
            return CreateObject(connection, objectName, assetName, null);
        }

        public void UpdateTransform(Vector3 pos, UnityEngine.Quaternion rot)
        {
            client.SetPoseRelQ(objectDescriptor.ObjectKey, new Point3
            {
                X = pos.z,
                Y = -pos.x,
                Z = pos.y
            }, new AnvelApi.Quaternion
            {
                X = rot.z ,
                Y = -rot.x ,
                Z = rot.y ,
                W = rot.w 
            });
        }

        public string ObjectName()
        {
            return objectDescriptor.ObjectName;
        }

        public ObjectDescriptor ObjectDescriptor()
        {
            return objectDescriptor;
        }

        /// <summary>
        /// Queries and retrieves the absolute position in the scene of the object inside of ANVEL
        /// </summary>
        /// <returns></returns>
        public Vector3 Position()
        {
            var pose = client.GetPoseAbs(objectDescriptor.ObjectKey);
            return new Vector3(-(float)pose.Position.Y, (float)pose.Position.Z, (float)pose.Position.X);
        }

        public UnityEngine.Quaternion Rotation()
        {
            var pose = client.GetPoseAbs(objectDescriptor.ObjectKey);
            return new UnityEngine.Quaternion((float)pose.Attitude.Quaternion.Y, (float)pose.Attitude.Quaternion.Z, (float)pose.Attitude.Quaternion.X, (float)pose.Attitude.Quaternion.W);
            //return new Vector3((float)pose.Attitude.Euler.Roll, (float)pose.Attitude.Euler.Yaw, (float)pose.Attitude.Euler.Pitch);
        }

        public void RemoveObject()
        {
            client.RemoveObject(objectDescriptor.ObjectKey);
            Debug.Log($"Destroyed: {objectDescriptor.ObjectName} ({objectDescriptor.ObjectKey})");
        }

    }
}

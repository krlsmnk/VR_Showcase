﻿using System;
using UnityEngine;

namespace CAVS.ProjectOrganizer.Project.Aggregations.Plot
{

    /// <summary>
    /// A 3D plot of Items
    /// </summary>
    public class ItemPlot: Graph
    {

        string x;

        string y;

        string z;

        public ItemPlot(Item[] items, string x, string y, string z) : base (items, null)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public GameObject Build(Vector3 scale)
        {
            return Build(scale, null);
        }

        public GameObject Build(Vector3 scale, Func<Item, GameObject> itemBuilder)
        {
            Vector2 xRange = GetItemsRangeForProperty(items, x);
            Vector2 yRange = GetItemsRangeForProperty(items, y);
            Vector2 zRange = GetItemsRangeForProperty(items, z);

            GameObject plot = new GameObject(string.Format("Plot ({0}, {1}, {2})", x, y, z));

            foreach (Item item in items)
            {
                Vector3 position = GetPositionGivenRanges(item, xRange, yRange, zRange, scale);
                GameObject point = Plot(item, position);
                point.transform.localScale = scale / 40f;
                point.transform.parent = plot.transform;
            }

            return plot;
        }

        private Vector3 GetPositionGivenRanges(Item item, Vector2 xRange, Vector2 yRange, Vector2 zRange, Vector3 scale)
        {
            float xParse = 0;
            if (float.TryParse(item.GetValue(this.x), out xParse))
            {
                xParse = ((xParse - xRange.x) / (xRange.y - xRange.x)) * scale.x;
            }

            float yParse = 0;
            if (float.TryParse(item.GetValue(this.y), out yParse))
            {
                yParse = ((yParse - yRange.x) / (yRange.y - yRange.x)) * scale.y;
            }

            float zParse = 0;
            if (float.TryParse(item.GetValue(this.z), out zParse))
            {
                zParse = ((zParse - zRange.x) / (zRange.y - zRange.x)) * scale.z;
            }

            return new Vector3(xParse, yParse, zParse);
        }


        private Vector2 GetItemsRangeForProperty(Item[] items, string property)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (Item item in items)
            {
                float result;
                if (float.TryParse(item.GetValue(property), out result))
                {
                    if (result < min)
                    {
                        min = result;
                    }
                    if (result > max)
                    {
                        max = result;
                    }
                }
            }
            return new Vector2(min, max);
        }

    }


}
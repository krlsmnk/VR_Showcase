﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using CAVS.ProjectOrganizer.Project.Filtering;

namespace CAVS.ProjectOrganizer.Project.Aggregations.Spiral
{

    class ItemFilterComparer : IComparer
    {
        Dictionary<Item, Dictionary<Filter, bool>> itemFilterMappings;
        public ItemFilterComparer(Dictionary<Item, Dictionary<Filter, bool>> itemFilterMappings)
        {
            this.itemFilterMappings = itemFilterMappings;
        }

        public int Compare(System.Object x, System.Object y)
        {
            return filterPassCount(itemFilterMappings[(Item)y]) - filterPassCount(itemFilterMappings[(Item)x]);
        }

        private int filterPassCount(Dictionary<Filter, bool> item)
        {
            int aCount = 0;
            foreach (var value in item.Values)
            {
                if (value)
                {
                    aCount++;
                }
            }
            return aCount;
        }

    }

    public class ItemSpiral : Graph
    {

        public ItemSpiral(Item[] itemsToDisplay, Filter[] filters): base(itemsToDisplay, filters)
        {
        }

        public ItemSpiral(Item[] itemsToDisplay, Filter filter): base(itemsToDisplay, new Filter[]{filter})
        {
        }

        public ItemSpiral(Item[] itemsToDisplay) : base(itemsToDisplay, new Filter[0])
        {
        }

        private GameObject getSpiralContainerReference()
        {
            return Resources.Load<GameObject>("Spiral Container");
        }

        public GameObject BuildPreview(Vector3 positionForPreview)
        {
            GameObject palace = GameObject.Instantiate(getSpiralContainerReference(), Vector3.zero, Quaternion.identity);
            int i = 0;
            Filter f = (Filters.Count == 1 ? new AggregateFilter(Filters.ToArray()) : Filters[0]);
            Item[] filteredItems = f.FilterItems(items);
            foreach (Item item in filteredItems)
            {
                GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                node.transform.parent = palace.transform;
                node.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                node.transform.position = new Vector3(Mathf.Sin(i) * .2f, -.35f + ((float)i / 400f), Mathf.Cos(i) * .2f);
                node.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                i++;
            }
            palace.GetComponent<SprialPreviewBehavior>().SetFilter(f);
            palace.transform.position = positionForPreview;
            return palace;
        }


        /// <summary>
        /// Creates a large construction that some one can walk through.
        /// </summary>
        /// <returns>The palace that was constructed</returns>
        public GameObject BuildPalace()
        {
            GameObject palace = new GameObject("Palace");
            int itemsCreated = 0;
            foreach (Item item in items)
            {
                Vector3 position = new Vector3(Mathf.Sin(itemsCreated) * 10, itemsCreated / 5, Mathf.Cos(itemsCreated) * 10);
                GameObject itemInstances = Plot(item, position);

                if (itemInstances != null)
                {
                    itemInstances.transform.position = position;
                    itemInstances.transform.parent = palace.transform;
                    itemInstances.transform.LookAt(Vector3.zero);
                    itemsCreated++;
                }
            }
            return palace;
        }

        public Item[] sortItems(Item[] itemsToSort, Dictionary<Item, Dictionary<Filter, bool>> filtersApplied)
        {
            Item[] copy = (Item[])itemsToSort.Clone();
            Array.Sort(copy, new ItemFilterComparer(filtersApplied));
            return copy;
        }

    }

}
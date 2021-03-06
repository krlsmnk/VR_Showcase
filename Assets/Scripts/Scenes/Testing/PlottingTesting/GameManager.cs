﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CAVS.ProjectOrganizer.Project;
using CAVS.ProjectOrganizer.Project.Aggregations.Plot;
using CAVS.ProjectOrganizer.Project.Aggregations.Spiral;
using CAVS.ProjectOrganizer.Project.Filtering;

namespace CAVS.ProjectOrganizer.Scenes.Testing.PlottingTesting
{

    public class GameManager : MonoBehaviour
    {

        void Start()
        {
			Item[] allItems = ProjectFactory.BuildItemsFromCSV("CarData.csv", 7);

            GameObject plot = new ItemPlot(allItems, "Year", "Length (in)", "Width (in)").Build(Vector3.one*3);
			plot.transform.position = Vector3.up*2;

            //GameObject spiralNoFilters = new ItemSpiralBuilder()
            //    .AddItems(allItems)
            //    .Build()
            //    .BuildPalace();

            //spiralNoFilters.transform.localScale = new Vector3(1.1f, 1, 1.1f);

            GameObject spiralWithFilters = new ItemSpiralBuilder()
                .AddItems(allItems)
                .AddFilter(new NumberFilter("Year", NumberFilter.Operator.GreaterThan, 1999), delegate(bool passed, GameObject point)
                {
                    if(point != null && !passed)
                    {
                        point.GetComponent<MeshRenderer>().material.color = Color.blue;
                    }
                })
                .AddFilter(new NumberFilter("Year", NumberFilter.Operator.LessThan, 2007), delegate (bool passed, GameObject point)
                {
                    if (point != null && !passed)
                    {
                        point.GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                })
                .AddFilter(new StringFilter("Model", StringFilter.Operator.Equal, "ES"), delegate (bool passed, GameObject point)
                {
                    if (point != null && !passed)
                    {
                        if (point.GetComponent<MeshRenderer>().material.color == Color.green)
                        {
                            point.GetComponent<MeshRenderer>().material.color = Color.white;
                        }
                        point.transform.localScale = Vector3.one * .5f;
                    }
                })
                .SetItemBuilder(delegate()
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    obj.GetComponent<MeshRenderer>().material.color = Color.green;
                    return obj;
                })
                .Build()
                .BuildPalace();
        }

        void Update()
        {

        }
    }

}
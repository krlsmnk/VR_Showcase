﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CAVS.ProjectOrganizer.Project
{

    public abstract class Item
    {

		private static GameObject nodeGameobjectReference;
		private static GameObject getGameobjectReference()
		{
			if (nodeGameobjectReference == null) {
				nodeGameobjectReference = Resources.Load<GameObject> ("Node");
			}
			return nodeGameobjectReference;
		}

        private readonly string title;

        public Item(string title)
        {
            this.title = title;
        }

		/// <summary>
		/// Gets the title, something to represent the node
		/// </summary>
		/// <returns>The title.</returns>
        public string GetTitle()
        {
            return title;
        }


		/// <summary>
		/// Builds a graphical representation of the object inside of the scene
		/// </summary>
		/// <returns>The item.</returns>
		public ItemBehaviour BuildItem (Vector3 position, Vector3 rotation) {
			GameObject node = GameObject.Instantiate (getGameobjectReference());
			node.transform.name = this.GetTitle ();
			node.transform.position = position;
			node.transform.Find ("Canvas").Find("Text").GetComponent<Text>().text = this.GetTitle ();
			return BuildItem (node);
		}

		protected abstract ItemBehaviour BuildItem (GameObject node);

    }

}
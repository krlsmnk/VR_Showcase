﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CAVS.ProjectOrganizer.Project
{

    public class TextItem : Item
    {

        private string content;

        public TextItem(string title, string content) : base(title)
        {
            this.content = content;
        }

        public string GetContent()
        {
            return this.content;
        }
        
		/// <summary>
		/// Builds a graphical representation of the object inside of the scene for
		/// displaying the text content.
		/// </summary>
		/// <returns>The item just built.</returns>
		protected override ItemBehaviour BuildItem (GameObject node) 
		{
			return node.AddComponent<ItemBehaviour>();
		}

    }

}
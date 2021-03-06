﻿using System.Collections.Generic;

namespace VueCoreFramework.API.ViewModels
{
    /// <summary>
    /// Used to transfer information about a group (role).
    /// </summary>
    public class GroupViewModel
    {
        /// <summary>
        /// The name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The username of the group's manager.
        /// </summary>
        public string Manager { get; set; }

        /// <summary>
        /// The members of the group (including the manager).
        /// </summary>
        public List<string> Members { get; set; }
    }
}

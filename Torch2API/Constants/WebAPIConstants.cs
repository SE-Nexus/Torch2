using System;
using System.Collections.Generic;
using System.Text;

namespace Torch2API.Constants
{
    //Maybe put in config later? But these are the endpoints for the web api, so they should be consistent across the codebase.

    /// <summary>
    /// Provides constant values for common Web API endpoint paths used throughout the application.
    /// </summary>
    /// <remarks>This class defines string constants representing relative URI paths for various Web API
    /// endpoints related to instance management and world or profile retrieval. These constants can be used to
    /// construct requests to the appropriate API routes and help ensure consistency across the codebase. This class is
    /// static and cannot be instantiated.</remarks>
    /// 

    public static class WebAPIConstants
    {
        public const string InstanceBase = "api/instance";
        public const string Update = InstanceBase + "/update";
        public const string AllWorlds = InstanceBase + "/allworlds";
        public const string CustomWorlds = InstanceBase + "/customworlds";
        public const string AllProfiles = InstanceBase + "/allprofiles";
        public const string DedicatedSchema = InstanceBase + "/dedicatedschema";
        public const string Register = InstanceBase + "/regsiter";

        public const string ModListBase = "api/modlist";
        public const string GetModIdsByListName = ModListBase + "/modids/{name}";
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InstanceUtils.Utils.Identification
{
    /// <summary>
    /// Manages and maintains unique identification for torch instances.
    /// </summary>
    public class InstanceIdentification
    {
        private string _BaseDir;

        private Guid? _InstanceID = null; 

        public InstanceIdentification(string BaseDir) { _BaseDir = BaseDir; }

        public Guid InstanceID
        {
            get
            {
                if (_InstanceID == null)
                    Initialize();
                return _InstanceID.Value;
            }
        }


        public void Initialize()
        {
            _InstanceID = GetInstanceID();
        }

        public Guid GetInstanceID()
        {
            Directory.CreateDirectory(_BaseDir);

            var filePath = Path.Combine(_BaseDir, $"Instance.id");

            // Load existing ID
            if (File.Exists(filePath))
            {
                var existing = File.ReadAllText(filePath).Trim();

                if (Guid.TryParse(existing, out var guid))
                    return guid;
            }

            // Create new ID
            var newGuid = Guid.NewGuid();

            File.WriteAllText(filePath, newGuid.ToString());
            return newGuid;
        }

    }
}

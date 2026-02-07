using System;
using System.IO;

namespace uDefend.Core
{
    public class SaveSlot
    {
        public string SlotId { get; }
        public string FilePath { get; }

        public DateTime LastSaveTime
        {
            get
            {
                if (!File.Exists(FilePath))
                    return DateTime.MinValue;
                return File.GetLastWriteTimeUtc(FilePath);
            }
        }

        public bool Exists => File.Exists(FilePath);

        public SaveSlot(string slotId, string basePath)
        {
            if (string.IsNullOrEmpty(slotId))
                throw new ArgumentNullException(nameof(slotId));
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException(nameof(basePath));

            SlotId = slotId;
            FilePath = Path.Combine(basePath, slotId + ".sav");
        }
    }
}

namespace DatingSim.SaveSystem
{
    public static class PendingSaveLoadRequest
    {
        public static bool HasPendingLoad { get; private set; }
        public static int PendingSlotIndex { get; private set; }

        public static void RequestLoad(int slotIndex)
        {
            HasPendingLoad = true;
            PendingSlotIndex = slotIndex;
        }

        public static void Clear()
        {
            HasPendingLoad = false;
            PendingSlotIndex = 0;
        }
    }
}

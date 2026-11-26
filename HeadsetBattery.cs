using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TrayMixer
{
    static class HeadsetBattery
    {
        // Windows stores the reported HFP or Bluetooth LE charge on a PnP node,
        // which may be a different node in the same container as the audio endpoint.
        static readonly PROPERTYKEY BatteryLevel = new PROPERTYKEY(new Guid("104ea319-6ee2-4701-bd47-8ddbf425bbe5"), 2);
        static readonly PROPERTYKEY ContainerId = new PROPERTYKEY(new Guid("8c7ed206-3f8a-4827-b3ab-ae9e1faefc6c"), 2);
        const uint FilterEnumerator = 1, FilterPresent = 0x100, BufferSmall = 0x1A;
        const uint TypeByte = 3, TypeGuid = 13;

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern uint CM_Get_Device_ID_List_SizeW(out uint length, string filter, uint flags);
        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern uint CM_Get_Device_ID_ListW(string filter, [Out] char[] ids, uint length, uint flags);
        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern uint CM_Locate_DevNodeW(out uint node, string id, uint flags);
        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern uint CM_Get_DevNode_PropertyW(uint node, ref PROPERTYKEY key, out uint type,
            [Out] byte[] buffer, ref uint length, uint flags);

        public static Dictionary<string, int> Read(List<DeviceInfo> devices)
        {
            var result = new Dictionary<string, int>();
            if (!devices.Exists(d => d.IsHeadphones)) return result;

            var byContainer = new Dictionary<Guid, int>();
            var buffer = new byte[16];
            foreach (string enumerator in new[] { "BTHLE", "BTHENUM" })
            {
                foreach (string id in PresentIds(enumerator))
                {
                    uint node;
                    Guid container;
                    if (CM_Locate_DevNodeW(out node, id, 0) == 0 &&
                        ReadProperty(node, BatteryLevel, TypeByte, 1, buffer) && buffer[0] <= 100)
                    {
                        int percent = buffer[0];
                        if (TryContainer(node, buffer, out container) && !byContainer.ContainsKey(container))
                            byContainer.Add(container, percent);
                    }
                }
            }
            if (byContainer.Count == 0) return result;

            foreach (var device in devices)
            {
                uint node;
                Guid container;
                int percent;
                if (device.IsHeadphones && CM_Locate_DevNodeW(out node, @"SWD\MMDEVAPI\" + device.Id, 0) == 0 &&
                    TryContainer(node, buffer, out container) && byContainer.TryGetValue(container, out percent))
                    result.Add("dev:" + device.Id, percent);
            }
            return result;
        }

        static IEnumerable<string> PresentIds(string enumerator)
        {
            uint flags = FilterEnumerator | FilterPresent;
            // PnP can grow the list between the size query and the read.
            for (int attempt = 0; attempt < 3; attempt++)
            {
                uint length;
                if (CM_Get_Device_ID_List_SizeW(out length, enumerator, flags) != 0 || length < 2) yield break;
                var ids = new char[length];
                uint status = CM_Get_Device_ID_ListW(enumerator, ids, length, flags);
                if (status == BufferSmall) continue;
                if (status != 0) yield break;
                for (int start = 0; start < ids.Length && ids[start] != '\0';)
                {
                    int end = Array.IndexOf(ids, '\0', start);
                    if (end < 0) yield break;
                    yield return new string(ids, start, end - start);
                    start = end + 1;
                }
                yield break;
            }
        }

        static bool ReadProperty(uint node, PROPERTYKEY key, uint expectedType, uint size, byte[] buffer)
        {
            uint type, length = size;
            return CM_Get_DevNode_PropertyW(node, ref key, out type, buffer, ref length, 0) == 0 &&
                type == expectedType && length == size;
        }

        static bool TryContainer(uint node, byte[] buffer, out Guid container)
        {
            container = ReadProperty(node, ContainerId, TypeGuid, 16, buffer) ? new Guid(buffer) : Guid.Empty;
            return container != Guid.Empty;
        }
    }
}

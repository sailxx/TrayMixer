using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace TrayMixer
{
    static class HeadsetBattery
    {
        // Windows stores the reported HFP or Bluetooth LE charge on a PnP node,
        // which may be a different node in the same container as the audio endpoint.
        static readonly PROPERTYKEY BatteryLevel = new PROPERTYKEY(new Guid("104ea319-6ee2-4701-bd47-8ddbf425bbe5"), 2);
        static readonly PROPERTYKEY ContainerId = new PROPERTYKEY(new Guid("8c7ed206-3f8a-4827-b3ab-ae9e1faefc6c"), 2);
        static readonly PROPERTYKEY ClassGuid = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 10);
        static readonly Guid KeyboardClass = new Guid("4d36e96b-e325-11ce-bfc1-08002be10318");
        static readonly Guid MouseClass = new Guid("4d36e96f-e325-11ce-bfc1-08002be10318");
        const string Address = "System.Devices.Aep.DeviceAddress", AepContainer = "System.Devices.Aep.ContainerId";
        const string Appearance = "System.Devices.Aep.Bluetooth.Le.Appearance";
        const string Major = "System.Devices.Aep.Bluetooth.Cod.Major", Minor = "System.Devices.Aep.Bluetooth.Cod.Minor";
        const string ConnectedBluetooth = "System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True AND " +
            "(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\" OR " +
            "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
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

            var byContainer = ReadLevels(null);
            if (byContainer.Count == 0) return result;
            var buffer = new byte[16];
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

        public static async Task<Dictionary<Guid, WirelessCharge>> ReadConnectedAsync()
        {
            var result = new Dictionary<Guid, WirelessCharge>();
            var byAddress = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var levels = ReadLevels(byAddress);
            if (levels.Count == 0) return result;

            var properties = new[] { Address, AepContainer, Appearance, Major, Minor };
            var endpoints = await WindowsMedia.Completion(DeviceInformation.FindAllAsync(
                ConnectedBluetooth, properties, DeviceInformationKind.AssociationEndpoint)).ConfigureAwait(false);
            foreach (var endpoint in endpoints)
            {
                // A Bluetooth address also links endpoints whose AEP and PnP containers differ.
                var address = Get(endpoint, Address) as string;
                var container = Get(endpoint, AepContainer) as Guid? ?? Guid.Empty;
                Guid mapped;
                int percent;
                if (address != null &&
                    byAddress.TryGetValue(address.Replace(":", "").Replace("-", ""), out mapped) &&
                    levels.ContainsKey(mapped))
                    container = mapped;
                if (!levels.TryGetValue(container, out percent)) continue;

                var kind = Classify(endpoint);
                WirelessCharge existing;
                if (!result.TryGetValue(container, out existing) || kind > existing.Kind)
                    result[container] = new WirelessCharge(endpoint.Name, kind, percent);
            }
            AddHidKinds(result);
            return result;
        }

        static WirelessKind Classify(DeviceInformation endpoint)
        {
            var appearance = Get(endpoint, Appearance) as ushort? ?? 0;
            if (appearance == 0x03C1) return WirelessKind.Keyboard;
            if (appearance == 0x03C2 || appearance == 0x03C9) return WirelessKind.Mouse;
            if ((appearance >> 6) == 0x25) return WirelessKind.Headphones;

            var major = Get(endpoint, Major) as ushort? ?? 0;
            var minor = Get(endpoint, Minor) as ushort? ?? 0;
            if (major == 4 && (minor == 1 || minor == 2 || minor == 6)) return WirelessKind.Headphones;
            if (major == 5 && (minor & 0x10) != 0) return WirelessKind.Keyboard;
            if (major == 5 && (minor & 0x20) != 0) return WirelessKind.Mouse;
            return WirelessKind.Other;
        }

        static object Get(DeviceInformation endpoint, string property)
        {
            object value;
            return endpoint.Properties.TryGetValue(property, out value) ? value : null;
        }

        static void AddHidKinds(Dictionary<Guid, WirelessCharge> charges)
        {
            var unknown = new HashSet<Guid>();
            foreach (var pair in charges)
                if (pair.Value.Kind == WirelessKind.Other) unknown.Add(pair.Key);
            if (unknown.Count == 0) return;

            var buffer = new byte[16];
            foreach (string id in PresentIds("HID"))
            {
                uint node;
                Guid container;
                if (CM_Locate_DevNodeW(out node, id, 0) != 0 || !TryContainer(node, buffer, out container) ||
                    !unknown.Contains(container) || !ReadProperty(node, ClassGuid, TypeGuid, 16, buffer)) continue;
                var deviceClass = new Guid(buffer);
                var kind = deviceClass == KeyboardClass ? WirelessKind.Keyboard :
                    deviceClass == MouseClass ? WirelessKind.Mouse : WirelessKind.Other;
                var charge = charges[container];
                if (kind > charge.Kind) charges[container] = new WirelessCharge(charge.Name, kind, charge.Percent);
            }
        }

        static Dictionary<Guid, int> ReadLevels(Dictionary<string, Guid> byAddress)
        {
            var byContainer = new Dictionary<Guid, int>();
            var buffer = new byte[16];
            foreach (string enumerator in new[] { "BTHLE", "BTHENUM", "BTHLEENUM" })
            {
                foreach (string id in PresentIds(enumerator))
                {
                    uint node;
                    Guid container;
                    const string prefix = "DEV_";
                    int start = enumerator.Length + 1;
                    bool hasAddress = byAddress != null && id.Length > start + prefix.Length + 12 &&
                        id.IndexOf(prefix, start, StringComparison.OrdinalIgnoreCase) == start &&
                        id[start + prefix.Length + 12] == '\\';
                    if (CM_Locate_DevNodeW(out node, id, 0) != 0) continue;
                    bool hasLevel = ReadProperty(node, BatteryLevel, TypeByte, 1, buffer) && buffer[0] <= 100;
                    if (!hasLevel && !hasAddress) continue;
                    int percent = buffer[0];
                    if (!TryContainer(node, buffer, out container)) continue;
                    if (hasLevel && !byContainer.ContainsKey(container)) byContainer.Add(container, percent);
                    if (hasAddress)
                    {
                        string key = id.Substring(start + prefix.Length, 12);
                        if (hasLevel || !byAddress.ContainsKey(key)) byAddress[key] = container;
                    }
                }
            }
            return byContainer;
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

using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioFlyout.Classes
{
    internal class AudioDeviceNotificationClient : IMMNotificationClient
    {
        public event EventHandler<string> DefaultDeviceChanged;

        public void OnDefaultDeviceChanged(DataFlow dataFlow, Role deviceRole, string defaultDeviceId)
        {
            if (dataFlow == DataFlow.Render && deviceRole == Role.Multimedia)
            {
                DefaultDeviceChanged?.Invoke(this, defaultDeviceId);
            }
        }

        public void OnDeviceAdded(string deviceId)
        {   }

        public void OnDeviceRemoved(string deviceId)
        {   }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {   }

        public void OnPropertyValueChanged(string deviceId, PropertyKey propertyKey)
        {   }
    }
}

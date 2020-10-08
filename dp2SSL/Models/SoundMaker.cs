using Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2SSL
{
    static class SoundMaker
    {
        static OutputDevice _outputDevice = null;

        public static void Open()
        {
            _outputDevice = ChooseOutputDevice();
            if (_outputDevice == null)
            {
                throw new Exception($"当前没有发现 MIDI 输出设备");
            }
            _outputDevice.Open();
        }

        public static void Close()
        {
            _outputDevice?.Close();
            _outputDevice = null;
        }

        public static OutputDevice ChooseOutputDevice()
        {
            if (OutputDevice.InstalledDevices.Count >= 1)
            {
                return OutputDevice.InstalledDevices[0];
            }
            return null;
        }

        public static void Start()
        {
            if (_outputDevice == null)
            {
                Open();
                _outputDevice.SendProgramChange(Channel.Channel1, Instrument.Violin);
            }

            _outputDevice.SendNoteOn(Channel.Channel1, Pitch.C4, 50);
        }

        public static void Stop()
        {
            _outputDevice.SendNoteOff(Channel.Channel1, Pitch.C4, 50);
        }

        // 表示出错的声音
        public static void ErrorSound()
        {
            _outputDevice.SendControlChange(Channel.Channel2, Control.SustainPedal, 100);
            _outputDevice.SendNoteOn(Channel.Channel2, Pitch.B4, 127);
            _outputDevice.SendNoteOff(Channel.Channel2, Pitch.B4, 100);
        }

            public static void AddSound()
        {
            _outputDevice.SendControlChange(Channel.Channel2, Control.SustainPedal, 100);
            _outputDevice.SendNoteOn(Channel.Channel2, Pitch.G4, 127);
            _outputDevice.SendNoteOff(Channel.Channel2, Pitch.G4, 100);

            /*
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                _outputDevice.SendNoteOff(Channel.Channel2, Pitch.G4, 100);
            });
            */
        }
#if NO

        static int _tone = 1147;

        static Task _task = null;

        static CancellationTokenSource _cancel = new CancellationTokenSource();

        public static void Start()
        {
            Stop();

            _cancel = new CancellationTokenSource();
            var token = _cancel.Token;
            _task = Task.Factory.StartNew(async () =>
            {
                while (token.IsCancellationRequested == false)
                {
                    System.Console.Beep(_tone, 1000);
                    await Task.Delay(1000);
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Current);
        }

        public static void Stop()
        {
            if (_cancel != null)
            {
                _cancel?.Cancel();
                _cancel?.Dispose();
                _cancel = null;
            }
        }

        public static void SetTone(int tone)
        {
            _tone = tone;
        }

#endif
    }
}

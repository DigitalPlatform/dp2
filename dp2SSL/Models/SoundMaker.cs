using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Midi;

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

        static List<int> _sequence = new List<int>();
        static int _currentPitch = -1; // -1 表示尚未开始

        // 初始化音符序列
        public static void InitialSequence(int count)
        {
            StopCurrent();

            var chord = new Chord("C");

            int start = (int)Pitch.C4;
            _sequence = new List<int>();
            while (_sequence.Count < count)
            {
                // 音符不要越过最大值
                // TODO: 试验一下找一个不是特别高的音作为最高音
                if (start >= (int)Pitch.G9)
                {
                    // 保护
                    if (_sequence.Count == 0)
                        break;

                    _sequence.Add(_sequence[_sequence.Count - 1]);  // 用最后一个重复填充
                }
                else if (chord.Contains((Pitch)start))
                    _sequence.Add(start);

                start++;
            }

            _sequence.Reverse();

            _currentPitch = -1;
        }

        static void InitialChannel1()
        {
            if (_outputDevice == null)
            {
                Open();
                _outputDevice.SendProgramChange(Channel.Channel1, Instrument.Violin);
            }
        }

        static void InitialChannel2()
        {
            if (_outputDevice == null)
            {
                Open();
            }
            _outputDevice.SendProgramChange(Channel.Channel2, Instrument.BirdTweet);
        }

        // 单条成功的声音
        static void InitialChannel3()
        {
            if (_outputDevice == null)
            {
                Open();
            }
            _outputDevice.SendProgramChange(Channel.Channel3, Instrument.ElectricPiano1);
        }

        public static void StopCurrent()
        {
            try
            {
                InitialChannel1();
                if (_currentPitch != -1)
                {
                    _outputDevice.SendNoteOff(Channel.Channel1, (Pitch)_currentPitch, 50);
                    _currentPitch = -1;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new ArgumentOutOfRangeException($"pitch 越过正常值范围。_currentPich={_currentPitch}", ex);
            }
        }

        public static void FirstSound(int offset)
        {
            InitialChannel1();

            StopCurrent();

            // 最大值 G9 = 127
            // 起点值 C4 = 60,
            _currentPitch = (int)Pitch.C4 + Math.Min(offset, Pitch.G9 - Pitch.C4);
            _outputDevice.SendNoteOn(Channel.Channel1, (Pitch)_currentPitch, 50);
        }


        public static void NextSound()
        {
            InitialChannel1();

            StopCurrent();

            if (_sequence.Count == 0)
                _currentPitch = (int)Pitch.C4;
            else
            {
                _currentPitch = _sequence[0];
                _sequence.RemoveAt(0);
            }

            try
            {
                _outputDevice.SendNoteOn(Channel.Channel1, (Pitch)_currentPitch, 50);
            }
            catch (Exception ex)
            {

            }
        }

        // 表示出错的声音
        public static void ErrorSound()
        {
            InitialChannel2();

            // _outputDevice.SendControlChange(Channel.Channel2, Control.SustainPedal, 10);
            _outputDevice.SendNoteOn(Channel.Channel2, Pitch.B4, 100);

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                _outputDevice.SendNoteOff(Channel.Channel2, Pitch.B4, 100);
            });
        }

        // 表示成功的声音
        public static void SucceedSound()
        {
            InitialChannel3();

            _outputDevice.SendNoteOn(Channel.Channel3, Pitch.B4, 100);

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                _outputDevice.SendNoteOff(Channel.Channel3, Pitch.B4, 100);
            });
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

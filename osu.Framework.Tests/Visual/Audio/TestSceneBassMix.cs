// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using ManagedBass;
using ManagedBass.Mix;
using ManagedBass.Fx;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Tests.Audio;
using osuTK;

namespace osu.Framework.Tests.Visual.Audio
{
    public class TestSceneBassMix : FrameworkTestScene
    {
        private int mixerHandle;
        private int subMixerHandle;

        private int bgmHandle;

        private int sfxSampleHandle;
        private int sfxHandle;

        private int sfx2SampleHandle;
        private int sfx2Handle;

        private int reverbHandle;
        private int compressorHandle;

        private const int num_mix_channels = 8;
        private readonly int[] channelHandles = new int[num_mix_channels];
        private readonly ChannelStrip[] channelStrips = new ChannelStrip[num_mix_channels];

        [BackgroundDependencyLoader]
        private void load(ITrackStore tracks)
        {
            DllResourceStore resources = new DllResourceStore(typeof(TrackBassTest).Assembly);

            for (int i = 0; i < num_mix_channels; i++)
            {
                channelStrips[i] = new ChannelStrip
                {
                    IsMixerChannel = { Value = i < num_mix_channels - 1 },
                    Width = 1f / num_mix_channels
                };
            }

            // Create Mixer
            mixerHandle = BassMix.CreateMixerStream(44100, 2, BassFlags.MixerNonStop);
            Logger.Log($"[BASSDLL] CreateMixerStream: {Bass.LastError}");
            // Make Mixer Go
            Bass.ChannelPlay(mixerHandle);
            Logger.Log($"[BASSDLL] ChannelPlay: {Bass.LastError}");
            channelHandles[num_mix_channels - 1] = mixerHandle;

            // Create SubMixer
            subMixerHandle = BassMix.CreateMixerStream(44100, 2, BassFlags.MixerNonStop | BassFlags.Decode);
            Logger.Log($"[BASSDLL] CreateMixerStream: {Bass.LastError}");
            // Route SubMixer into Mixer
            BassMix.MixerAddChannel(mixerHandle, subMixerHandle, BassFlags.MixerChanBuffer);
            Logger.Log($"Mixer Connection: {Bass.LastError}");
            channelHandles[num_mix_channels - 2] = subMixerHandle;

            // Load BGM Track
            var bgmData = resources.Get("Resources.Tracks.sample-track.mp3");
            bgmHandle = Bass.CreateStream(bgmData, 0, bgmData.Length, BassFlags.Decode | BassFlags.Loop);

            // Apply ReplayGain
            // double replayGain = calculateReplayGain(bgmData);
            // Bass.ChannelSetAttribute(trackHandle, ChannelAttribute.Volume, BassUtils.DbToLevel(replayGain));

            // Add BGM Track to Mixer
            BassMix.MixerAddChannel(mixerHandle, bgmHandle, BassFlags.MixerChanPause | BassFlags.MixerChanBuffer);
            Logger.Log($"[BASSDLL] MixerAddChannel: {Bass.LastError}");
            channelHandles[0] = bgmHandle;

            // Load SFX1
            var sfxData = resources.Get("Resources.Samples.long.mp3");
            sfxSampleHandle = Bass.SampleLoad(sfxData, 0, sfxData.Length, 2, BassFlags.Default | BassFlags.SampleOverrideLongestPlaying);

            // Load SFX2
            var sfx2Data = resources.Get("Resources.Samples.tone2.wav");
            sfx2SampleHandle = Bass.SampleLoad(sfx2Data, 0, sfx2Data.Length, 2, BassFlags.Default | BassFlags.SampleOverrideLongestPlaying);

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(1.0f),
                Children = channelStrips
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("idle", () =>
            {
                // do nothing
            });
            AddStep("start track", () =>
            {
                Bass.ChannelSetPosition(bgmHandle, 0);
                BassMix.ChannelFlags(bgmHandle, BassFlags.Default, BassFlags.MixerChanPause);
            });
            AddStep("stop track", () =>
            {
                BassMix.ChannelFlags(bgmHandle, BassFlags.MixerChanPause, BassFlags.MixerChanPause);
            });

            AddStep("Reverb on", () =>
            {
                reverbHandle = Bass.ChannelSetFX(subMixerHandle, EffectType.Freeverb, 100);
                Bass.FXSetParameters(reverbHandle, new ReverbParameters
                {
                    fDryMix = 1f,
                    fWetMix = 0.25f,
                });
                Logger.Log($"[BASSDLL] ChannelSetFX: {Bass.LastError}");
            });
            AddStep("Reverb off", () =>
            {
                Bass.ChannelRemoveFX(subMixerHandle, reverbHandle);
                Logger.Log($"[BASSDLL] ChannelSetFX: {Bass.LastError}");
            });

            AddStep("Compressor on", () =>
            {
                compressorHandle = Bass.ChannelSetFX(mixerHandle, EffectType.Compressor, 1);
                Bass.FXSetParameters(compressorHandle, new CompressorParameters
                {
                    fAttack = 5,
                    fRelease = 100,
                    fThreshold = -6,
                    fGain = 0,
                    // fRatio = 4,
                });
                Logger.Log($"[BASSDLL] ChannelSetFX: {Bass.LastError}");
            });
            AddStep("Compressor off", () =>
            {
                Bass.ChannelRemoveFX(mixerHandle, compressorHandle);
                Logger.Log($"[BASSDLL] ChannelSetFX: {Bass.LastError}");
            });

            AddStep("Play SFX1", () =>
            {
                sfxHandle = Bass.SampleGetChannel(sfxSampleHandle, BassFlags.Decode | BassFlags.SampleChannelStream);
                BassMix.MixerAddChannel(subMixerHandle, sfxHandle, BassFlags.MixerChanBuffer);
                Logger.Log($"[BASSDLL] MixerAddChannel: {Bass.LastError}");
                channelHandles[1] = sfxHandle;
            });

            AddStep("Play SFX2", () =>
            {
                sfx2Handle = Bass.SampleGetChannel(sfx2SampleHandle, BassFlags.Decode | BassFlags.SampleChannelStream);
                BassMix.MixerAddChannel(subMixerHandle, sfx2Handle, BassFlags.MixerChanBuffer);
                Logger.Log($"[BASSDLL] MixerAddChannel: {Bass.LastError}");
                channelHandles[2] = sfx2Handle;
            });

            AddStep("Reset Peaks", () =>
            {
                foreach (var strip in channelStrips)
                {
                    strip.ResetPeaks();
                }
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Bass.StreamFree(bgmHandle);
        }

        protected override void Update()
        {
            base.Update();

            for (int i = 0; i < num_mix_channels; i++)
            {
                channelStrips[i].Handle = channelHandles[i];
            }
        }

        // private double calculateReplayGain(byte[] data)
        // {
        //     int replayGainProcessingStream = Bass.CreateStream(data, 0, data.Length, BassFlags.Decode);
        //     TrackGain trackGain = new TrackGain(44100, 16);
        //
        //     const int buf_len = 1024;
        //     short[] buf = new short[buf_len];
        //
        //     List<int> leftSamples = new List<int>();
        //     List<int> rightSamples = new List<int>();
        //
        //     while (true)
        //     {
        //         int length = Bass.ChannelGetData(replayGainProcessingStream, buf, buf_len * sizeof(short));
        //         if (length == -1) break;
        //
        //         for (int a = 0; a < length / sizeof(short); a += 2)
        //         {
        //             leftSamples.Add(buf[a]);
        //             rightSamples.Add(buf[a + 1]);
        //         }
        //     }
        //
        //     trackGain.AnalyzeSamples(leftSamples.ToArray(), rightSamples.ToArray());
        //
        //     double gain = trackGain.GetGain();
        //     double peak = trackGain.GetPeak();
        //
        //     Logger.Log($"REPLAYGAIN GAIN: {gain}");
        //     Logger.Log($"REPLAYGAIN PEAK: {peak}");
        //
        //     Bass.StreamFree(replayGainProcessingStream);
        //
        //     return gain;
        // }
    }

    public class ChannelStrip : CompositeDrawable
    {
        public int Handle { get; set; }
        public int BuffSize = 30;
        public Bindable<bool> IsMixerChannel = new Bindable<bool>(true);

        private float maxPeak = float.MinValue;
        private float peak = float.MinValue;
        private readonly Box volBarL;
        private readonly Box volBarR;
        private readonly SpriteText peakText;
        private readonly SpriteText maxPeakText;

        public ChannelStrip(int handle = -1)
        {
            Handle = handle;

            RelativeSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                volBarL = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Colour = Colour4.Green,
                    Height = 0f,
                    Width = 0.5f,
                },
                volBarR = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    Colour = Colour4.Green,
                    Height = 0f,
                    Width = 0.5f,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        peakText = new SpriteText { Text = "N/A" },
                        maxPeakText = new SpriteText { Text = "N/A" }
                    }
                }
            };

            IsMixerChannel.ValueChanged += (val) => volBarL.Colour = volBarR.Colour = val.NewValue ? Colour4.Green : Colour4.GreenYellow;
        }

        protected override void Update()
        {
            base.Update();

            if (Handle == 0)
            {
                volBarL.Height = 0;
                volBarR.Height = 0;
                peakText.Text = "N/A";
                maxPeakText.Text = "N/A";

                return;
            }

            float[] levels = new float[2];

            if (IsMixerChannel.Value)
                BassMix.ChannelGetLevel(Handle, levels, 1 / (float)BuffSize, LevelRetrievalFlags.Stereo);
            else
                Bass.ChannelGetLevel(Handle, levels, 1 / (float)BuffSize, LevelRetrievalFlags.Stereo);

            peak = (levels[0] + levels[1]) / 2f;
            maxPeak = Math.Max(peak, maxPeak);

            volBarL.TransformTo(nameof(Drawable.Height), levels[0], BuffSize * 4);
            volBarR.TransformTo(nameof(Drawable.Height), levels[1], BuffSize * 4);
            peakText.Text = $"{BassUtils.LevelToDb(peak):F}dB";
            maxPeakText.Text = $"{BassUtils.LevelToDb(maxPeak):F}dB";
        }

        public void ResetPeaks()
        {
            peak = float.MinValue;
            maxPeak = float.MinValue;
        }
    }
}
